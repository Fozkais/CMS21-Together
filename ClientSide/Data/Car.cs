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
        public static bool aCarIsSpawning;
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
                   // MelonLogger.Msg("Found Bodypart in buffer!: " + car.partInfo.CarPartsBuffer.Count);
                    if (car.isReady && car.isReferenced)
                    {
                        //MelonLogger.Msg("Car Ready! Updating..");
                        try
                        {
                            for (var index = 0; index < car.partInfo.CarPartsBuffer.Count; index++)
                            {
                                var part = car.partInfo.CarPartsBuffer[index];

                                HandleNewPart(car, null, part);
                                car.partInfo.CarPartsBuffer.Remove(part);
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on CarPartUpdateHandle 28 - Body");
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
                   // MelonLogger.Msg("Found PartScript in buffer!: " + car.partInfo.PartScriptsBuffer.Count);
                    if (car.isReady && car.isReferenced)
                    {
                        try
                        {


                          //  MelonLogger.Msg("Car Ready! Updating..");
                            for (var index = 0; index < car.partInfo.PartScriptsBuffer.Count; index++)
                            {
                                var part = car.partInfo.PartScriptsBuffer[index];
                                HandleNewPart(car, part);
                                car.partInfo.PartScriptsBuffer.Remove(part);
                            }
                        }
                        catch (Exception e)
                        {
                            MelonLogger.Msg("Error on CarPartUpdateHandle 28 - Part");
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
            MelonCoroutines.Start(isCarStillLoaded());
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

                        if (!car.isFromServer)
                        {
                            if (!car.asBeenSent)
                            {
                                ClientSend.SendPartScripts(car.tempOther, car.carLoaderID);
                                car.tempOther = null;
                                ClientSend.SendPartScripts(car.tempEngine, car.carLoaderID);
                                car.tempEngine = null;
                                ClientSend.SendPartScripts(car.tempSuspension, car.carLoaderID);
                                car.tempSuspension = null;
                                
                                ClientSend.SendBodyParts(car.tempBody, car.carLoaderID);
                                car.tempBody = null;

                                car.asBeenSent = true;
                            }
                        }
                        
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
                            _car.tempBody.Add(partConverted);
                        }
                    }
                            
                    if (!_car.isFromServer)
                    {
                        if (!isNewPart)
                        {
                            if (hasDifferences(BodyParts[i], BodyPartsReferences[i]))
                            {
                                BodyParts[i] = partConverted;
                               // MelonLogger.Msg("Body part updated");
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
                                _car.tempSuspension.Add(partConverted);
                            }
                        }
                            
                        if (!_car.isFromServer)
                        {
                            if (!isNewPart)
                            {
                                if (hasDifferences(suspensionParts[i][j], parts[j]))
                                {
                                    suspensionParts[i][j] = partConverted;
                                 //   MelonLogger.Msg("Suspension part updated");
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
                            _car.tempEngine.Add(partConverted);
                        }
                    }
                    else
                    {
                        if (!_car.isFromServer)
                        {
                            if (hasDifferences(engineParts[i], EnginePartsReferences[i]))
                            {
                                engineParts[i] = partConverted;
                             //   MelonLogger.Msg("Engine part updated");
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
                                _car.tempOther.Add(partConverted);
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

            public static IEnumerator isCarStillLoaded()
            {
                foreach (KeyValuePair<int,ModCar> car in ClientData.carOnScene)
                {
                    if (aCarIsSpawning)
                        yield return new WaitForSeconds(1);
                    if (String.IsNullOrEmpty(GameData.carLoaders[car.Value.carLoaderID].carToLoad) && GameData.carLoaders[car.Value.carLoaderID].carParts == null)
                    {
                        
                        MelonLogger.Msg("Detected A removed car");
                        ClientSend.SendCarInfo(new ModCar(car.Value));
                        ClientData.carOnScene.Remove(car.Key);
                    }
                }

                yield return null;
            }
        
        

        #endregion
        
        #region PartUpdate


        public static IEnumerator SetCarReadyAndUpdated(int carLoaderID, List<ModPartScript> partScripts = null,
            List<ModCarPart> carParts = null)
        {
            var HandleNewCar = MelonCoroutines.Start(Car.HandleNewCar(carLoaderID, partScripts, carParts));

            yield return HandleNewCar;
            
            yield return new WaitForEndOfFrame();
            
            if (ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                ClientData.carOnScene[carLoaderID].isUpdated = true;
                MelonLogger.Msg("Car as been Updated!");
            }
        }

        public static IEnumerator HandleNewCar(int carLoaderID, List<ModPartScript> partScripts = null,
            List<ModCarPart> carParts = null)
        {
            if (ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderID))
            {
                var car = ClientData.carOnScene.First(s => s.Value.carLoaderID == carLoaderID).Value;
                while (!car.isReferenced)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                while (!car.isReady )
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                yield return new WaitForEndOfFrame();
                
                MelonLogger.Msg($"Car is ready!");
                try
                {
                    if (partScripts != null)
                    {
                        MelonLogger.Msg("Starting to update part: " + partScripts[0].type);
                        MelonLogger.Msg(partScripts[0].type + " parts Size : " + partScripts.Count);
                        foreach (ModPartScript partScript in partScripts)
                        {
                            if(partScript != null)
                                HandleNewPart(car, partScript);
                            else
                                MelonLogger.Msg("Invalid PartScript.");
                        }
                    }

                    if (carParts != null)
                    {
                        MelonLogger.Msg("Starting to update bodyPart!");
                        foreach (ModCarPart carPart in carParts)
                        {
                            HandleNewPart(car, null, carPart);
                        }
                    }
                }
                catch (Exception e)
                {
                    MelonLogger.Msg("Error on HandleNewCar 567");
                    MelonLogger.Msg("Error:" + e);
                    MelonLogger.Msg("");
                }

                yield return null;
            }
        }

        public static void HandleNewPart(ModCar _car, ModPartScript _partScript = null, ModCarPart _bodyPart = null)
        {
            if (_car == null)
            {
                MelonLogger.Msg("Car is null!");
                return;
            }
            if (_car.partInfo == null)
            {
                MelonLogger.Msg("PartInfo is null!");
                return;
            }

            var partInfo = _car.partInfo;

            if (_partScript != null)
            {
                switch (_partScript.type)
                {
                    case partType.other:
                        if (partInfo.OtherPartsReferences != null)
                        {
                            var references = partInfo.OtherPartsReferences;
                            if (references.TryGetValue(_partScript.partID, out List<PartScript> referencesParts))
                            {
                                if (referencesParts == null)
                                {
                                    MelonLogger.Msg("OtherPart is null!");
                                    return;
                                }

                                if (referencesParts.Count >= _partScript.partIdNumber)
                                {
                                    UpdatePart(referencesParts[_partScript.partIdNumber], _partScript, _car.carLoaderID);
                                }
                                else
                                {
                                    MelonLogger.Msg("Part Index is Invalid!");
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Msg("OtherPartReference is null!");
                        }
                        break;
                    case partType.suspension:
                        if (partInfo.SuspensionPartsReferences != null)
                        {
                            var references = partInfo.SuspensionPartsReferences;
                            if (references.TryGetValue(_partScript.partID, out List<PartScript> referencesParts))
                            {
                                if (referencesParts == null)
                                {
                                    MelonLogger.Msg("SuspensionPart is null!");
                                    return;
                                }

                                if (referencesParts.Count >= _partScript.partIdNumber)
                                {
                                    UpdatePart(referencesParts[_partScript.partIdNumber], _partScript, _car.carLoaderID);
                                }
                                else
                                {
                                    MelonLogger.Msg("Part Index is Invalid!");
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Msg("SuspensionPartReference is null!");
                        }
                        break;
                    case partType.engine:
                        if (partInfo.EnginePartsReferences != null)
                        {
                            var references = partInfo.EnginePartsReferences;
                            if (references.TryGetValue(_partScript.partID, out PartScript referencesPart))
                            {
                                if (referencesPart == null)
                                {
                                    MelonLogger.Msg("EnginePart is null!");
                                    return;
                                }
                                UpdatePart(referencesPart, _partScript, _car.carLoaderID);
                            }
                        }
                        else
                        {
                            MelonLogger.Msg("EnginePartReference is null!");
                        }
                        break;
                }
            }
            else if(_bodyPart != null)
            {
                if (partInfo.BodyPartsReferences != null)
                {
                    var references = partInfo.BodyPartsReferences;
                    if (references.TryGetValue(_bodyPart.carPartID, out CarPart carPart))
                    {
                        if (carPart == null)
                        {
                            MelonLogger.Msg("CarPart is null!");
                            return;
                        }
                        UpdateBodyPart(carPart, _bodyPart, _car.carLoaderID);
                    }
                }
            }
            else
            {
                MelonLogger.Msg("Data is Invalid!");
            }
        }


        public static void UpdatePart(PartScript originalPart, ModPartScript updatedPart, int carLoaderId)
        {
            if (originalPart == null)
            {
                MelonLogger.Msg("OriginalPart is null!");
                return;
            }

            if (updatedPart == null)
            {
                MelonLogger.Msg("UpdatedPart is null!");
                return;
            }

            if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderId))
            {
                MelonLogger.Msg("Invalid CarLoaderID!" + carLoaderId);
                MelonLogger.Msg("Valid LoaderID:");
                foreach (var modCar in ClientData.carOnScene.Values)
                {
                    MelonLogger.Msg("LoaderID: " + modCar.carLoaderID);
                }
                return;
            }
            
            if (!String.IsNullOrEmpty(updatedPart.tunedID))
            {
                if (originalPart.tunedID != updatedPart.tunedID)
                {
                    GameData.carLoaders[carLoaderId].TunePart(originalPart.tunedID, updatedPart.tunedID);
                }
            }

            originalPart.IsExamined = updatedPart.isExamined;

            if (!updatedPart.unmounted)
            {
                originalPart.IsPainted = updatedPart.isPainted;
                if (updatedPart.isPainted)
                {
                    originalPart.CurrentPaintType = (PaintType)updatedPart.paintType;
                    originalPart.CurrentPaintData = new ModPaintData().ToGame(updatedPart.paintData);
                    originalPart.SetColor(ModColor.ToColor(updatedPart.color));
                    if ((PaintType)updatedPart.paintType == PaintType.Custom)
                    {
                        PaintHelper.SetCustomPaintType(originalPart.gameObject, updatedPart.paintData.ToGame(updatedPart.paintData), false);
                    }
                    else
                    {
                        PaintHelper.SetPaintType(originalPart.gameObject, (PaintType)updatedPart.paintType, false);
                    }
                }
                originalPart.Quality = updatedPart.quality;
                originalPart.SetCondition(updatedPart.condition);
                originalPart.UpdateDust(updatedPart.dust, true);
                if (originalPart.IsUnmounted)
                {
                    MelonCoroutines.Start(HarmonyPatches.ResetCursorBlockCoroutine());
                    originalPart.ShowBySaveGame();
                    originalPart.SetCondition(updatedPart.condition);
                    originalPart.ShowMountAnimation();
                    originalPart.FastMount();
                }
                    
                //Wheel Handle
                var wheelData =  GameData.carLoaders[carLoaderId].WheelsData;
                for (int i = 0; i <  GameData.carLoaders[carLoaderId].WheelsData.Wheels.Count; i++)
                {
                    GameData.carLoaders[carLoaderId].SetWheelSize((int)wheelData.Wheels[i].Width, 
                        (int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
                    GameData.carLoaders[carLoaderId].SetET((WheelType)i, wheelData.Wheels[i].ET);
                }
                GameData.carLoaders[carLoaderId].SetWheelSizes();
            }
            else
            {
                
                originalPart.Quality = updatedPart.quality;
                originalPart.SetCondition(updatedPart.condition);
                originalPart.UpdateDust(updatedPart.dust, true);
                if (originalPart.IsUnmounted == false)
                {
                    MelonCoroutines.Start(HarmonyPatches.ResetCursorBlockCoroutine());
                    originalPart.SetCondition(updatedPart.condition);
                    originalPart.HideBySavegame(false, GameData.carLoaders[carLoaderId]);
                }
            }
        }

        public static void UpdateBodyPart(CarPart originalPart, ModCarPart updatedPart, int carLoaderId)
        {
            if (originalPart == null)
            {
                MelonLogger.Msg("OriginalPart is null!");
                return;
            }

            if (updatedPart == null)
            {
                MelonLogger.Msg("UpdatedPart is null!");
                return;
            }

            if (!ClientData.carOnScene.Any(s => s.Value.carLoaderID == carLoaderId))
            {
                MelonLogger.Msg("Invalid CarLoaderID!" + carLoaderId);
                MelonLogger.Msg("Valid LoaderID:");
                foreach (var modCar in ClientData.carOnScene.Values)
                {
                    MelonLogger.Msg("LoaderID: " + modCar.carLoaderID);
                }
                return;
            }
            
            Color color = ModColor.ToColor(updatedPart.colors);
            Color tintColor = ModColor.ToColor(updatedPart.TintColor);

            if(originalPart.TunedID != updatedPart.tunedID)
                GameData.carLoaders[carLoaderId].TunePart(originalPart.name, updatedPart.tunedID);
            
            GameData.carLoaders[carLoaderId].SetDent(originalPart, updatedPart.dent);
            GameData.carLoaders[carLoaderId].EnableDust(originalPart, updatedPart.Dust);
            GameData.carLoaders[carLoaderId].SetCondition(originalPart, updatedPart.condition);
            
            originalPart.IsTinted = updatedPart.isTinted;
            originalPart.TintColor = tintColor;
            originalPart.Color = color;
            originalPart.PaintType = (PaintType)updatedPart.paintType;
            originalPart.OutsideRustEnabled = updatedPart.outsaidRustEnabled;
            originalPart.AdditionalString = updatedPart.additionalString;
            originalPart.Quality = updatedPart.quality;
            originalPart.WashFactor = updatedPart.washFactor;
            
            if (!originalPart.Unmounted && !originalPart.name.StartsWith("license_plate"))
            {
               // GameData.carLoaders[carLoaderId].SetCustomCarPaintType(originalPart, updatedPart.paintData.ToGame(updatedPart.paintData));  
               // GameData.carLoaders[carLoaderId].SetCarColorAndPaintType(originalPart, color, (PaintType)updatedPart.paintType);
               GameData.carLoaders[carLoaderId].SetCarColor(originalPart, color);
            }
            GameData.carLoaders[carLoaderId].SetCarLivery(originalPart, updatedPart.livery, updatedPart.liveryStrength);

            if (!originalPart.Unmounted && updatedPart.unmounted)
            {
                GameData.carLoaders[carLoaderId].TakeOffCarPartFromSave(updatedPart.name);
            }
            
            if (originalPart.Unmounted && !updatedPart.unmounted)
            {
                GameData.carLoaders[carLoaderId].TakeOnCarPartFromSave(updatedPart.name);
            }
            
            if (originalPart.Switched != updatedPart.switched)
                GameData.carLoaders[carLoaderId].SwitchCarPart(originalPart, false, updatedPart.switched);


            if(updatedPart.isTinted)
                PaintHelper.SetWindowProperties(originalPart.handle, (int)(updatedPart.TintColor.a * 255), tintColor);
            
            GameData.carLoaders[carLoaderId].SetCondition(originalPart, updatedPart.condition);
        }
           
        
        #endregion
    }
}