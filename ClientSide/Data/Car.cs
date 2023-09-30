using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Data
{
    public static class Car
    {
        private static bool partUpdateHandleRunning;
        private static bool carMovementHandleRunning;
        
        public static void UpdateCars()
        {
            if(!partUpdateHandleRunning)
                MelonCoroutines.Start(CarPartUpdateHandle(0.25f));
            if (!carMovementHandleRunning)
                MelonCoroutines.Start(CarMovementHandle(1));
            
        }
        
        private static IEnumerator CarPartUpdateHandle(float updateRate)
        {
            partUpdateHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            foreach (ModCar car in ClientData.carOnScene)
            {
                MelonCoroutines.Start(UpdatePartsCoroutine(car));
            }
            yield return new WaitForSeconds(0.25f);
            partUpdateHandleRunning = false;
        }
        
        private static IEnumerator CarMovementHandle(int updateRate)
        {
            carMovementHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            HandleCarPos();
            isCarStillLoaded();
            yield return new WaitForEndOfFrame();
            carMovementHandleRunning = false;
        }

        #region Get Parts References

            public static IEnumerator GetPartsReferencesCoroutine(int carLoaderID)
            {
                ModCar car = ClientData.carOnScene[carLoaderID];
                car.partInfo = new ModPartInfo();

                yield return new WaitForEndOfFrame();
                
                if (car.isCarLoaded)
                {
                    yield return new WaitForEndOfFrame();

                    IEnumerator getOtherPartCoroutine = GetOtherPartsReferencesCoroutine(car);
                    IEnumerator getEnginePartCoroutine = GetEnginePartsReferencesCoroutine(car);
                    IEnumerator getSuspensionPartCoroutine = GetSuspensionsPartsReferencesCoroutine(car);
                    IEnumerator getBodyPartCoroutine = GetBodyPartsReferencesCoroutine(car);

                    yield return getOtherPartCoroutine;
                    yield return getEnginePartCoroutine;
                    yield return getSuspensionPartCoroutine;
                    yield return getBodyPartCoroutine;

                    car.isReferenced = true;
                    MelonLogger.Msg("Car is Referenced!");
                }
                else
                {
                    MelonLogger.Msg("Car is not loaded!");
                }
            }

            private static IEnumerator GetBodyPartsReferencesCoroutine(ModCar _car)
            {
                var BodypartReferencePoint = _car.partInfo.BodyPartsReferences;
                var bodyParts = ClientData.carLoaders[_car.carLoaderID].carParts;
                for (int i = 0; i <bodyParts.Count; i++)
                {
                    if(!BodypartReferencePoint.ContainsKey(i))
                        BodypartReferencePoint.Add(i, bodyParts._items[i]);
                } 
                
                yield return null;
            }

            private static IEnumerator GetSuspensionsPartsReferencesCoroutine (ModCar _car)
            {
                List<GameObject> suspensions = new List<GameObject>();
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_frontCenter_h);
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_frontLeft_h);
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_frontRight_h);
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_rearCenter_h);
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_rearLeft_h);
                suspensions.Add(ClientData.carLoaders[_car.carLoaderID].s_rearRight_h);

                var suspensionReferencePoint = _car.partInfo.SuspensionPartsReferences;
                
                for (int i = 0; i < suspensions.Count; i++)
                {
                    var partsInSuspension = suspensions[i].GetComponentsInChildren<PartScript>().ToList();
                    for (int j = 0; j < partsInSuspension.Count; j++)
                    {
                        if (!suspensionReferencePoint.ContainsKey(i))
                        {
                            suspensionReferencePoint.Add(i, new List<PartScript>() { partsInSuspension[j] });
                        }
                        if (!suspensionReferencePoint[i].Contains(partsInSuspension[j]))
                        {
                            suspensionReferencePoint[i].Add(partsInSuspension[j]);
                        }
                    }
                }
                
                yield return null;
            }

            private static IEnumerator GetEnginePartsReferencesCoroutine (ModCar _car)
            {
                var engine = ClientData.carLoaders[_car.carLoaderID].e_engine_h;
                var partsInEngine = engine.GetComponentsInChildren<PartScript>().ToList();

                var engineReferencePoint = _car.partInfo.EnginePartsReferences;

                for (int i = 0; i < partsInEngine.Count; i++)
                {
                    if (!engineReferencePoint.ContainsKey(i))
                    {
                        engineReferencePoint.Add(i, partsInEngine[i]);
                    }
                }
                yield return null;
            }

            private static IEnumerator GetOtherPartsReferencesCoroutine (ModCar _car)
            {
                var otherPartsObjects = ClientData.carLoaders[_car.carLoaderID].Parts;
                var otherPartsReferencePoint = _car.partInfo.OtherPartsReferences;

                for (int i = 0; i < otherPartsObjects.Count; i++)
                {
                    var partObject = ClientData.carLoaders[_car.carLoaderID].Parts._items[i].p_handle;
                    var _parts = partObject.GetComponentsInChildren<PartScript>().ToList();

                    for (int j = 0; j < _parts.Count; j++)
                    {
                        if (!otherPartsReferencePoint.ContainsKey(i))
                        {
                            otherPartsReferencePoint.Add(i, new List<PartScript>() { _parts[j] });
                        }
                        else if(!otherPartsReferencePoint[i].Contains(_parts[j]))
                        {
                            otherPartsReferencePoint[i].Add(_parts[j]);
                        }
                    }
                }
                yield return null;
            }

        #endregion
        
        #region Handle Part

            private static IEnumerator UpdatePartsCoroutine(ModCar car)
            {
                if (car.isReferenced)
                {
                    var updateOtherPartCoroutine = MelonCoroutines.Start(UpdateOtherPartsCoroutine(car));
                    var updateEnginePartCoroutine = MelonCoroutines.Start(UpdateEnginePartsCoroutine(car));
                    var updateSuspensionCoroutine = MelonCoroutines.Start(UpdateSuspensionPartsCoroutine(car));
                    var updateBodyPartCoroutine = MelonCoroutines.Start(UpdateBodyPartsCoroutine(car));
                    
                    yield return updateSuspensionCoroutine;
                    yield return updateEnginePartCoroutine;
                    yield return updateOtherPartCoroutine;
                    yield return updateBodyPartCoroutine;
                    
                    if (!car.isReady)
                    {
                        car.isReady = true;
                        MelonLogger.Msg("Car Ready !");
                        if (car.isFromServer)
                        {
                            car.isFromServer = false;
                            MelonLogger.Msg("Car is no longer from server !");
                        }
                    }
                }
            }

            private static IEnumerator UpdateBodyPartsCoroutine(ModCar _car)
            {
                var BodyPartsReferences = _car.partInfo.BodyPartsReferences;
                var BodyParts = _car.partInfo.BodyParts;

                for (int i = 0; i < BodyPartsReferences.Count; i++)
                {
                    var part = BodyPartsReferences[i];
                    var partConverted = new ModCarPart(part, i);

                    bool isNewPart = false;
                    if (!BodyParts.Any(s => s.Value.carPartID == partConverted.carPartID))
                    {
                        isNewPart = true;
                        BodyParts.Add(i, partConverted);
                        if (!_car.isFromServer)
                        {
                            MelonLogger.Msg("Send new Body part");
                            ClientSend.SendBodyPart(_car.carLoaderID, partConverted);
                        }
                    }
                            
                    if (!_car.isFromServer)
                    {
                        if (!isNewPart)
                        {
                            if (hasDifferences(BodyParts[i], BodyPartsReferences[i]))
                            {
                                BodyParts[i] = partConverted;
                                MelonLogger.Msg("Body part updated");
                                ClientSend.SendBodyPart(_car.carLoaderID, partConverted);
                            }
                        }
                    }
                }
                
                yield return null;
            }

            private static IEnumerator UpdateSuspensionPartsCoroutine(ModCar _car)
            {
                var suspensionPartsReferences = _car.partInfo.SuspensionPartsReferences;
                var suspensionParts = _car.partInfo.SuspensionParts;

                for (int i = 0; i < suspensionPartsReferences.Count; i++)
                {
                    var parts = suspensionPartsReferences[i];
                    for (int j = 0; j < parts.Count; j++)
                    {
                        var partConverted = new ModPartScript(parts[j], i, j, partType.suspension);

                        if (!suspensionParts.ContainsKey(i))
                        {
                            suspensionParts.Add(i, new List<ModPartScript>());
                        }

                        bool isNewPart = false;
                        if (!suspensionParts[i].Any(s => s.partID == partConverted.partID && s.partIdNumber == partConverted.partIdNumber))
                        {
                            isNewPart = true;
                            suspensionParts[i].Add(partConverted);
                            if (!_car.isFromServer)
                            {
                                MelonLogger.Msg("Send new suspension part");
                                ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                            }
                        }
                            
                        if (!_car.isFromServer)
                        {
                            if (!isNewPart)
                            {
                                if (hasDifferences(suspensionParts[i][j], parts[j]))
                                {
                                    suspensionParts[i][j] = partConverted;
                                    MelonLogger.Msg("Suspension part updated");
                                    ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                                }
                            }
                        }
                    }
                }
                yield return null;
            }

            private static IEnumerator UpdateEnginePartsCoroutine(ModCar _car)
            {
                var EnginePartsReferences = _car.partInfo.EnginePartsReferences;
                var engineParts = _car.partInfo.EngineParts;

                for (int i = 0; i < EnginePartsReferences.Count; i++)
                {
                    var partConverted = new ModPartScript(EnginePartsReferences[i], i, -1, partType.engine);
                    if (!engineParts.ContainsKey(i))
                    {
                        engineParts.Add(i, partConverted);
                        if (!_car.isFromServer)
                        {
                            MelonLogger.Msg("Send new engine part");
                            ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                        }
                    }
                    else
                    {
                        if (!_car.isFromServer)
                        {
                            if (hasDifferences(engineParts[i], EnginePartsReferences[i]))
                            {
                                engineParts[i] = partConverted;
                                MelonLogger.Msg("Engine part updated");
                                ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                                
                            }
                        }   
                    }
                }
                yield return null;
            }

            private static IEnumerator UpdateOtherPartsCoroutine(ModCar _car)
            {
                var otherPartsReferences = _car.partInfo.OtherPartsReferences;
                var otherParts = _car.partInfo.OtherParts;

                for (int i = 0; i < otherPartsReferences.Count; i++)
                {
                    var parts = otherPartsReferences[i];
                    for (int j = 0; j < parts.Count; j++)
                    {
                        var partConverted = new ModPartScript(parts[j], i, j, partType.other);
                        
                        if (!otherParts.ContainsKey(i))
                        {
                            otherParts.Add(i, new List<ModPartScript>());
                        }

                        bool isNewPart = false;
                        if(!otherParts[i].Any(s => s.partID == partConverted.partID && s.partIdNumber == partConverted.partIdNumber))
                        {
                            isNewPart = true;
                            otherParts[i].Add(partConverted);
                            if (!_car.isFromServer)
                            {
                                MelonLogger.Msg("Send new other part");
                                ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                            }
                        }
                            
                        if (!_car.isFromServer)
                        {
                            if (!isNewPart)
                            {
                                if (hasDifferences(otherParts[i][j], parts[j]))
                                {
                                    otherParts[i][j] = partConverted;
                                    MelonLogger.Msg("Other part updated");
                                    ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                                }
                            }
                        }
                    }
                }
                yield return null;
            }

            private static bool hasDifferences(ModPartScript handled, PartScript toHandle) //TODO: Add more differences check
            {
                bool hasDifferences = false;
                if (handled.unmounted != toHandle.IsUnmounted)
                    hasDifferences = true;

                return hasDifferences;
            }
            private static bool hasDifferences(ModCarPart handled, CarPart toHandle) //TODO: Add more differences check
            {
                bool hasDifferences = false;
                if (handled.unmounted != toHandle.Unmounted)
                    hasDifferences = true;
                if(handled.switched != toHandle.Switched)
                    hasDifferences = true;

                return hasDifferences;
            }
            

            #endregion

        #region Other Car Info

            public static void HandleCarPos()
            {
                foreach (var car in ClientData.carOnScene)
                {
                    if (ClientData.carLoaders[car.carLoaderID].placeNo != car.carPosition && ClientData.carLoaders[car.carLoaderID].placeNo != -1)
                    {
                        ClientSend.SendCarPosition(car.carLoaderID, ClientData.carLoaders[car.carLoaderID].placeNo);
                        car.carPosition = ClientData.carLoaders[car.carLoaderID].placeNo;
                    }
                    
                        
                }
            }

            public static void isCarStillLoaded()
            {
                for (var index = 0; index < ClientData.carOnScene.Count; index++)
                {
                    var car = ClientData.carOnScene[index];
                    if (String.IsNullOrEmpty(ClientData.carLoaders[car.carLoaderID].carToLoad))
                    {
                        MelonLogger.Msg("Detected A removed car");
                        ClientSend.SendCarInfo(car);
                        ClientData.carOnScene.Remove(car);
                    }
                }
            }
        
        

        #endregion
        
        #region PartUpdate
            public static IEnumerator HandleNewPart(int _carLoaderID, ModPartScript _carPart=null, ModCarPart _bodyPart=null)
            {
                var car = ClientData.carOnScene[_carLoaderID];

                if (!car.isReady)
                {
                    while (!car.isReady && !ClientData.carLoaders[_carLoaderID].modelLoaded)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                        
                    yield return new WaitForSeconds(3); // Additional wait to be sure that the car is ready
                }
                
                yield return new WaitForSeconds(0.25f); // Additional wait to be sure that the car is ready
                
                try
                {
                    if (_carPart != null)
                    {
                        switch (_carPart.type)
                        {
                            case partType.other:
                                UpdatePart(ClientData.carOnScene[_carLoaderID].partInfo.OtherPartsReferences[_carPart.partID][
                                _carPart.partIdNumber], _carPart, _carLoaderID);
                                MelonLogger.Msg("Other part added");
                                break;
                            case partType.engine:
                                UpdatePart(ClientData.carOnScene[_carLoaderID].partInfo.EnginePartsReferences[_carPart.partID],
                                    _carPart, _carLoaderID);
                                MelonLogger.Msg("Engine part added");
                                break;
                            case partType.suspension:
                                UpdatePart(
                                    ClientData.carOnScene[_carLoaderID].partInfo.SuspensionPartsReferences[_carPart.partID][
                                        _carPart.partIdNumber], _carPart, _carLoaderID);
                                MelonLogger.Msg("Suspension part added");
                                break;
                        }
                    }
                    if(_bodyPart != null)
                        UpdateBodyPart(ClientData.carOnScene[_carLoaderID].partInfo.BodyPartsReferences[_bodyPart.carPartID], _bodyPart, _carLoaderID);
                    
                    
                }
                catch (Exception e)
                {
                    MelonLogger.Msg("Error while handling new part: " + e.Message + " " + e.StackTrace + " " + _carPart.type + " " + _carPart.partID + " " + _carPart.partIdNumber);
                }
            }
            public static void UpdatePart(PartScript originalPart, ModPartScript newpart, int carLoaderID)
            {
                try
                {
                    if (!String.IsNullOrEmpty(newpart.tunedID))
                    {
                        originalPart.tunedID = newpart.tunedID;
                        // if (originalPart.tunedID != newPart.tunedID)
                        // MainMod.carLoaders[Newpart._carLoaderID].TunePart(originalPart.tunedID, newPart.tunedID); 
                    }
                    originalPart.IsExamined = newpart.isExamined;
                    
                    if (newpart.unmounted == false)
                    {
                        originalPart.IsPainted = newpart.isPainted;
                        if (newpart.isPainted)
                        {
                            originalPart.CurrentPaintType = (PaintType)newpart.paintType;
                            originalPart.CurrentPaintData = new ModPaintData().ToGame(newpart.paintData);
                            originalPart.SetColor(ModColor.ToColor(newpart.color));
                            if ((PaintType)newpart.paintType == PaintType.Custom)
                                PaintHelper.SetCustomPaintType(originalPart.gameObject, originalPart.CurrentPaintData, false);
                        }

                        originalPart.Quality = newpart.quality;
                        originalPart.SetCondition(newpart.condition);
                       //MelonLogger.Msg("part condition: " + newpart.condition);
                        originalPart.UpdateDust(newpart.dust, true);
                        // Handle Bolts

                        if (originalPart.IsUnmounted)
                        {
                            originalPart.ShowBySaveGame();
                            originalPart.ShowMountAnimation();
                            originalPart.FastMount();
                        }
                        
                        //Wheel Handle
                        var wheelData =  ClientData.carLoaders[carLoaderID].WheelsData;
                        for (int i = 0; i <  ClientData.carLoaders[carLoaderID].WheelsData.Wheels.Count; i++)
                        {
                            ClientData.carLoaders[carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width, 
                                (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                            ClientData.carLoaders[carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
                        }
                        ClientData.carLoaders[carLoaderID].SetWheelSizes();
                    }
                    else
                    {
                        originalPart.Quality = newpart.quality;
                        //originalPart.SetCondition(newpart.condition);
                        originalPart.Condition = newpart.condition;
                        originalPart.UpdateDust(newpart.dust);
                        if (originalPart.IsUnmounted == false)
                        {
                            originalPart.HideBySavegame(false, ClientData.carLoaders[carLoaderID]);
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg("Error while updating pieces : " + e);
                    throw;
                }
            }

            public static void UpdateBodyPart(CarPart orginalPart, ModCarPart newBodyPart, int carLoaderID)
            {
                if (ClientData.carLoaders != null && ClientData.carOnScene[carLoaderID] != null)
                {
                    if (!String.IsNullOrEmpty(ClientData.carLoaders[carLoaderID].carToLoad))
                    {
                        Color color = ModColor.ToColor(newBodyPart.colors);
                        Color tintColor = ModColor.ToColor(newBodyPart.TintColor);

                        if(orginalPart.TunedID != newBodyPart.tunedID)
                            ClientData.carLoaders[carLoaderID].TunePart(orginalPart.name, newBodyPart.tunedID);
                        
                        ClientData.carLoaders[carLoaderID].SetDent(orginalPart, newBodyPart.dent);
                        ClientData.carLoaders[carLoaderID].EnableDust(orginalPart, newBodyPart.Dust);
                        ClientData.carLoaders[carLoaderID].SetCondition(orginalPart, newBodyPart.condition);
                        
                        orginalPart.IsTinted = newBodyPart.isTinted;
                        orginalPart.TintColor = tintColor;
                        orginalPart.Color = color;
                        orginalPart.PaintType = (PaintType)newBodyPart.paintType;
                        orginalPart.OutsideRustEnabled = newBodyPart.outsaidRustEnabled;
                        orginalPart.AdditionalString = newBodyPart.additionalString;
                        orginalPart.Quality = newBodyPart.quality;
                        orginalPart.WashFactor = newBodyPart.washFactor;
                        
                        
                        if (!orginalPart.Unmounted && !orginalPart.name.StartsWith("license_plate"))
                        {
                            ClientData.carLoaders[carLoaderID].SetCustomCarPaintType(orginalPart, new ModPaintData().ToGame(newBodyPart.paintData));  
                            ClientData.carLoaders[carLoaderID].SetCarColorAndPaintType(orginalPart, color, (PaintType)newBodyPart.paintType);
                        }
                        ClientData.carLoaders[carLoaderID].SetCarLivery(orginalPart, newBodyPart.livery, newBodyPart.liveryStrength);
                        
                        if(!orginalPart.Unmounted && newBodyPart.unmounted)
                            ClientData.carLoaders[carLoaderID].TakeOffCarPartFromSave(newBodyPart.name);
                        if (orginalPart.Unmounted && !newBodyPart.unmounted)
                        {
                            ClientData.carLoaders[carLoaderID].TakeOnCarPartFromSave(newBodyPart.name);
                        }
                                          
                                     
                        
                        if (orginalPart.Switched != newBodyPart.switched)
                            ClientData.carLoaders[carLoaderID].SwitchCarPart(orginalPart, false, newBodyPart.switched);


                        if(newBodyPart.isTinted)
                            PaintHelper.SetWindowProperties(orginalPart.handle, (int)(newBodyPart.TintColor.a * 255), orginalPart.TintColor);
                        
                        //MelonLogger.Msg($"Parts[{_bodyPart.carPartID}] updated!, {_bodyPart.name}, -> {MainMod.carLoaders[_carLoaderID].carParts._items[_bodyPart.carPartID].name}, {_bodyPart.unmounted}");
                    }
                    else
                    {
                        MelonLogger.Msg($"Loss of data from bodyPart ! {newBodyPart.name} on car with CarLoaderID{carLoaderID}");
                    }
                }
            }

        #endregion
    }
}