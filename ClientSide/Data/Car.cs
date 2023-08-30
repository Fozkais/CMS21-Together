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
            MelonCoroutines.Start(UpdatePartsCoroutine());
            yield return new WaitForSeconds(0.25f);
            partUpdateHandleRunning = false;
        }
        
        private static IEnumerator CarMovementHandle(int updateRate)
        {
            carMovementHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            HandleCarPos();
            yield return new WaitForEndOfFrame();
            carMovementHandleRunning = false;
        }

        #region Get Parts References

            public static IEnumerator GetPartsReferencesCoroutine(int carLoaderID)
            {
                ModCar car = ClientData.carOnScene[carLoaderID];
                car.partInfo = new ModPartInfo();
                
                if (car.isCarLoaded)
                {
                    IEnumerator getOtherPartCoroutine = GetOtherPartsReferencesCoroutine(car);
                    IEnumerator getEnginePartCoroutine = GetEnginePartsReferencesCoroutine(car);
                    IEnumerator getSuspensionPartCoroutine = GetSuspensionsPartsReferencesCoroutine(car);
                    //TODO: Handle body parts references
                    
                    yield return getOtherPartCoroutine;
                    yield return getEnginePartCoroutine;
                    yield return getSuspensionPartCoroutine;

                    car.isReferenced = true;
                    MelonLogger.Msg("Car is Referenced!");
                }
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

            private static IEnumerator UpdatePartsCoroutine()
            {
                for (var index = 0; index < ClientData.carOnScene.Count; index++)
                {
                    var car = ClientData.carOnScene[index];
                    if (car.isReferenced)
                    {
                        var updateOtherPartCoroutine = MelonCoroutines.Start(UpdateOtherPartsCoroutine(car));
                        var updateEnginePartCoroutine = MelonCoroutines.Start(UpdateEnginePartsCoroutine(car));
                        var updateSuspensionCoroutine = MelonCoroutines.Start(UpdateSuspensionPartsCoroutine(car));
                        //TODO: Handle body parts
                        yield return updateSuspensionCoroutine;
                        yield return updateEnginePartCoroutine;
                        yield return updateOtherPartCoroutine;
                        
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
            

            #endregion

        #region Car Moving

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

        #endregion
        
        #region PartUpdate
            public static IEnumerator HandleNewPart(int _carLoaderID, ModPartScript _carPart)
            {
                var car = ClientData.carOnScene[_carLoaderID];
                while (!car.isReady && !ClientData.carLoaders[_carLoaderID].modelLoaded)
                {
                    yield return new WaitForEndOfFrame();
                }
                    
                yield return new WaitForSeconds(3); // Additional wait to be sure that the car is ready

                try
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
                        // TODO: Handle bodyParts
                        case partType.body:
                            break;
                    }
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

        #endregion
    }
}