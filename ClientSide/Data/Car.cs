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
using UnityEngine.SceneManagement;

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

            foreach (ModCar car in ClientData.carOnScene.Values)
            {
                MelonCoroutines.Start(UpdatePartsCoroutine(car)); // Classic Update

                #region CarSpawn Buffer

                //MelonLogger.Msg("BufferPart!");
                if (car.partInfo.CarPartsBuffer.Count > 0)
                {
                    MelonLogger.Msg("Found Bodypart in buffer!: " + car.partInfo.CarPartsBuffer.Count);
                    if (car.isReady && car.isReferenced)
                    {
                        MelonLogger.Msg("Car Ready! Updating..");
                        for (var index = 0; index < car.partInfo.CarPartsBuffer.Count; index++)
                        {
                            var part = car.partInfo.CarPartsBuffer[index];
                            HandleNewPart(car.carLoaderID, null, part);
                            car.partInfo.CarPartsBuffer.Remove(part);
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("Car is not ready...");
                    }
                }
                else
                {
                   // MelonLogger.Msg("Body Buffer Empty.");
                }

                if (car.partInfo.PartScriptsBuffer.Count > 0)
                {
                    MelonLogger.Msg("Found PartScript in buffer!: " + car.partInfo.PartScriptsBuffer.Count);
                    if (car.isReady && car.isReferenced)
                    {
                        MelonLogger.Msg("Car Ready! Updating..");
                        for (var index = 0; index < car.partInfo.PartScriptsBuffer.Count; index++)
                        {
                            var part = car.partInfo.PartScriptsBuffer[index];
                            HandleNewPart(car.carLoaderID, part);
                            car.partInfo.PartScriptsBuffer.Remove(part);
                        }
                    }
                    else
                    {
                        MelonLogger.Msg("Car is not ready...");
                    }
                }
                else
                {
                  //  MelonLogger.Msg("Part Buffer Empty.");
                }

                #endregion
            }

            yield return new WaitForSeconds(updateRate/3);
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
                    MelonLogger.Msg("Other part Reference Count :" + car.partInfo.OtherPartsReferences.Count);
                    MelonLogger.Msg("Suspension part Reference Count :" + car.partInfo.SuspensionPartsReferences.Count);
                    MelonLogger.Msg("Engine part Reference Count :" + car.partInfo.EnginePartsReferences.Count);
                    MelonLogger.Msg("Body part Reference Count :" + car.partInfo.BodyPartsReferences.Count);
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

              //  MelonLogger.Msg("Car Parts Loaded!");
                
                var bodyParts = GameData.carLoaders[_car.carLoaderID].carParts;
                
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
                
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_frontCenter_h);
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_frontLeft_h);
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_frontRight_h);
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_rearCenter_h);
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_rearLeft_h);
                suspensions.Add(GameData.carLoaders[_car.carLoaderID].s_rearRight_h);

                var suspensionReferencePoint = _car.partInfo.SuspensionPartsReferences;
                
                for (int i = 0; i < suspensions.Count; i++)
                {

                    //MelonLogger.Msg("Suspension " + i + " is loaded!");
                    
                    
                    var partsInSuspension = suspensions[i].GetComponentsInChildren<PartScript>().ToList(); // Object reference not set to an instance of an object
                    if (partsInSuspension.Count == 0)
                    {
                      //  MelonLogger.Msg("Suspension " + i + " is empty!");
                        suspensionReferencePoint.Add(i, new List<PartScript>());
                    }
                    
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

              //  MelonLogger.Msg("Engine is loaded!");
                
                var engine = GameData.carLoaders[_car.carLoaderID].e_engine_h;
                var partsInEngine = engine.GetComponentsInChildren<PartScript>().ToList(); // Object reference not set to an instance of an object

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
                yield return new WaitForEndOfFrame();
                
                var otherPartsObjects = GameData.carLoaders[_car.carLoaderID].Parts;
                var otherPartsReferencePoint = _car.partInfo.OtherPartsReferences;

                for (int i = 0; i < otherPartsObjects.Count; i++)
                {
                    var partObject = GameData.carLoaders[_car.carLoaderID].Parts._items[i].p_handle;
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
                        
                        ClientSend.SendPartScripts(car.tempCarScripts, car.carLoaderID);
                        ClientSend.SendBodyParts(car.tempCarParts, car.carLoaderID);
                        
                        car.tempCarParts.Clear();
                        car.tempCarParts = null;
                        car.tempCarScripts.Clear();
                        car.tempCarScripts = null;
                        if (car.isFromServer)
                        {
                            yield return new WaitForSeconds(1.5f);
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
                            //MelonLogger.Msg("Send new Body part");
                            //ClientSend.SendBodyPart(_car.carLoaderID, partConverted); Instead Send all in one ?
                            _car.tempCarParts.Add(partConverted);
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
                                //MelonLogger.Msg("Send new suspension part");
                                //ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                                _car.tempCarScripts.Add(partConverted);
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
                           // MelonLogger.Msg("Send new engine part");
                            // ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                            _car.tempCarScripts.Add(partConverted);
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
                                //MelonLogger.Msg("Send new other part");
                                //ClientSend.SendCarPart(_car.carLoaderID, partConverted);
                                _car.tempCarScripts.Add(partConverted);
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
                    if (!car.Value.isFromServer)
                    {
                        if (GameData.carLoaders[car.Value.carLoaderID].placeNo != car.Value.carPosition && GameData.carLoaders[car.Value.carLoaderID].placeNo != -1)
                        {
                            ClientSend.SendCarPosition(car.Value.carLoaderID, GameData.carLoaders[car.Value.carLoaderID].placeNo);
                            car.Value.carPosition = GameData.carLoaders[car.Value.carLoaderID].placeNo;
                        }
                    }
                    
                        
                }
            }

            public static void isCarStillLoaded()
            {
                foreach (KeyValuePair<int,ModCar> car in ClientData.carOnScene)
                {
                    if (String.IsNullOrEmpty(GameData.carLoaders[car.Value.carLoaderID].carToLoad) && GameData.carLoaders[car.Value.carLoaderID].carParts == null)
                    {
                        MelonLogger.Msg("Detected A removed car");
                        ClientSend.SendCarInfo(new ModCar(car.Value));
                        ClientData.carOnScene.Remove(car.Key);
                    }
                }
            }
        
        

        #endregion
        
        #region PartUpdate


        public static IEnumerator SetCarToReadyAndUpdated(int carLoaderID, List<ModPartScript> partScripts = null,
            List<ModCarPart> carParts = null)
        {
            var HandleNewCar = MelonCoroutines.Start(Car.HandleNewCar(carLoaderID, partScripts, carParts));

            yield return HandleNewCar;
            
            yield return new WaitForEndOfFrame();
            if (ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                ClientData.carOnScene[carLoaderID].isUpdated = true;
            }
        }

        public static IEnumerator HandleNewCar(int carLoaderID, List<ModPartScript> partScripts = null,
            List<ModCarPart> carParts = null)
        {
            if (ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                var car = ClientData.carOnScene[carLoaderID];
                while (!car.isReferenced)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                while (!car.isReady )
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                yield return new WaitForEndOfFrame();
                
                MelonLogger.Msg($"Waiting car to be ready finished!.. : {car.isReady} , {car.isReferenced}");

                if (partScripts != null)
                {
                    for (int i = 0; i < partScripts.Count; i++)
                    {
                        HandleNewPart(carLoaderID, partScripts[i]);
                    }
                }

                if (carParts != null)
                {
                    for (int i = 0; i < carParts.Count; i++)
                    {
                        HandleNewPart(carLoaderID, null,carParts[i]);
                    }
                }
                yield return null;
            }
        }
        
        
            public static void HandleNewPart(int _carLoaderID, ModPartScript _partScript=null, ModCarPart _bodyPart=null)
            {
                //MelonLogger.Msg("Update Part Begin");
                    try
                    {
                        if (_partScript != null)
                        {
                            switch (_partScript.type)
                            {
                                case partType.other:
                                    if (ClientData.carOnScene[_carLoaderID].partInfo.OtherPartsReferences
                                        .TryGetValue(_partScript.partID, out List<PartScript> otherParts))
                                    {
                                        if (otherParts.Count >= _partScript.partIdNumber)
                                        {
                                            UpdatePart(otherParts[_partScript.partIdNumber], _partScript, _carLoaderID);
                                            MelonLogger.Msg("Other part added");
                                        }
                                        else
                                        {
                                            MelonLogger.Msg($"Cant update other part 2: {otherParts.Count} , {_partScript.partIdNumber} ");
                                        }
                                    }
                                    else
                                    {
                                        MelonLogger.Msg("Cant update other part 1");
                                    }
                                    break;
                                case partType.engine:
                                    if (ClientData.carOnScene[_carLoaderID].partInfo.EnginePartsReferences
                                        .TryGetValue(_partScript.partID, out PartScript enginePart))
                                    {
                                        UpdatePart(enginePart, _partScript, _carLoaderID);
                                        MelonLogger.Msg("Engine part added");
                                    }
                                    else
                                    {
                                        MelonLogger.Msg("Cant update engine part");
                                    }
                                    break;
                                case partType.suspension:
                                    if (ClientData.carOnScene[_carLoaderID].partInfo.SuspensionPartsReferences
                                        .TryGetValue(_partScript.partID, out List<PartScript> suspensionParts))
                                    {
                                        if (suspensionParts.Count >= _partScript.partIdNumber)
                                        {
                                            UpdatePart(suspensionParts[_partScript.partIdNumber], _partScript, _carLoaderID);
                                            MelonLogger.Msg("Suspension part added");
                                        }
                                        else
                                        {
                                            MelonLogger.Msg("Cant update suspension part 2");
                                        }
                                        
                                    }
                                    else
                                    {
                                        MelonLogger.Msg("Cant update suspension part 1");
                                    }
                                    break;
                            }
                        }
                        else if(_bodyPart == null)
                        {
                            MelonLogger.Msg("Invalid Part Handle");
                        }
                        else 
                        {
                            if (ClientData.carOnScene[_carLoaderID].partInfo.BodyPartsReferences
                                .TryGetValue(_bodyPart.carPartID, out CarPart carPart))
                            {
                                UpdateBodyPart(carPart, _bodyPart, _carLoaderID);
                            }
                            else
                            {
                                MelonLogger.Msg("Cant update bodypart");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg("Error while handling new part: " + e.Message + " " + e.StackTrace + " " + _partScript.type + " " + _partScript.partID + " " + _partScript.partIdNumber);
                        throw;
                    }
            }
            public static void UpdatePart(PartScript originalPart, ModPartScript newpart, int carLoaderID)
            {
               // MelonLogger.Msg("Update Part");
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
                            MelonCoroutines.Start(HarmonyPatches.ResetCursorBlockCoroutine());
                            originalPart.ShowBySaveGame();
                            originalPart.ShowMountAnimation();
                            originalPart.FastMount();
                            //originalPart.StartCoroutine(originalPart.DoMount());
                        }
                        
                        //Wheel Handle
                        var wheelData =  GameData.carLoaders[carLoaderID].WheelsData;
                        for (int i = 0; i <  GameData.carLoaders[carLoaderID].WheelsData.Wheels.Count; i++)
                        {
                            GameData.carLoaders[carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width, 
                                (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                            GameData.carLoaders[carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
                        }
                        GameData.carLoaders[carLoaderID].SetWheelSizes();
                    }
                    else
                    {
                        originalPart.Quality = newpart.quality;
                        originalPart.SetCondition(newpart.condition);
                       // originalPart.Condition = newpart.condition;
                        originalPart.UpdateDust(newpart.dust);
                        if (originalPart.IsUnmounted == false)
                        {
                            MelonCoroutines.Start(HarmonyPatches.ResetCursorBlockCoroutine());
                            originalPart.FastUnmount();
                            originalPart.HideBySavegame(false, GameData.carLoaders[carLoaderID]);
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg("Error while updating pieces : " + e);
                }
            }

            public static void UpdateBodyPart(CarPart orginalPart, ModCarPart newBodyPart, int carLoaderID)
            {
              //  MelonLogger.Msg("Update BodyPart");
                if (GameData.carLoaders != null && ClientData.carOnScene[carLoaderID] != null)
                {
                    if (!String.IsNullOrEmpty(GameData.carLoaders[carLoaderID].carToLoad))
                    {
                        Color color = ModColor.ToColor(newBodyPart.colors);
                        Color tintColor = ModColor.ToColor(newBodyPart.TintColor);

                        if(orginalPart.TunedID != newBodyPart.tunedID)
                            GameData.carLoaders[carLoaderID].TunePart(orginalPart.name, newBodyPart.tunedID);
                        
                        GameData.carLoaders[carLoaderID].SetDent(orginalPart, newBodyPart.dent);
                        GameData.carLoaders[carLoaderID].EnableDust(orginalPart, newBodyPart.Dust);
                        GameData.carLoaders[carLoaderID].SetCondition(orginalPart, newBodyPart.condition);
                        
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
                            GameData.carLoaders[carLoaderID].SetCustomCarPaintType(orginalPart, new ModPaintData().ToGame(newBodyPart.paintData));  
                            GameData.carLoaders[carLoaderID].SetCarColorAndPaintType(orginalPart, color, (PaintType)newBodyPart.paintType);
                        }
                        GameData.carLoaders[carLoaderID].SetCarLivery(orginalPart, newBodyPart.livery, newBodyPart.liveryStrength);
                        
                        if(!orginalPart.Unmounted && newBodyPart.unmounted)
                            GameData.carLoaders[carLoaderID].TakeOffCarPartFromSave(newBodyPart.name);
                        if (orginalPart.Unmounted && !newBodyPart.unmounted)
                        {
                            GameData.carLoaders[carLoaderID].TakeOnCarPartFromSave(newBodyPart.name);
                        }
                                          
                                     
                        
                        if (orginalPart.Switched != newBodyPart.switched)
                            GameData.carLoaders[carLoaderID].SwitchCarPart(orginalPart, false, newBodyPart.switched);


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