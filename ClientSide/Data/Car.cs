using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.CustomData;
using CMS21MP.SharedData;
using Il2Cpp;
using Il2CppCMS.Tutorial;
using MelonLoader;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CMS21MP.ClientSide.Data
{
    public static class Car
    {
        
        public static Dictionary<int, CarLoader> loaderWithCar = new Dictionary<int, CarLoader>();
        
        private static bool carSpawnHandleRunning;
        private static bool partReferenceHandleRunning;
        private static bool partUpdateHandleRunning;
        private static bool carMovementHandleRunning;
        
        public static void UpdateCarInfo()
        {
            if (!carSpawnHandleRunning)
                MelonCoroutines.Start(CarSpawnHandle(2));
            if(!partReferenceHandleRunning)
                MelonCoroutines.Start(CarPartReferenceHandle(2));
            if(!partUpdateHandleRunning)
                MelonCoroutines.Start(CarPartUpdateHandle(0.5f));
            if (!carMovementHandleRunning)
                MelonCoroutines.Start(CarMovementHandle(1));
            
        }

        private static IEnumerator CarSpawnHandle(int updateRate)
        {
            carSpawnHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            HandleCarSpawn();
            carSpawnHandleRunning = false;
        }
        
        private static IEnumerator CarPartReferenceHandle(int updateRate)
        {
            partReferenceHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            GetPartsReferences();
            partReferenceHandleRunning = false;
        }
        private static IEnumerator CarPartUpdateHandle(float updateRate)
        {
            partUpdateHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            UpdateParts();
            partUpdateHandleRunning = false;
        }
        
        private static IEnumerator CarMovementHandle(int updateRate)
        {
            carMovementHandleRunning = true;
            yield return new WaitForSeconds(updateRate);
            HandleCarPos();
            carMovementHandleRunning = false;
        }

        

        #region Handle car spawning
        public static bool hasCar(CarLoader loader)
            {
                bool _hasCar = false;
                if(loader.placeNo != -1 && loader.modelLoaded && loader.e_engine_h != null
                   && loader.Parts._items.Length > 6 && loader.carParts._items.Length > 20)
                    _hasCar = true;

                return _hasCar;
            }

            private static void HandleCarSpawn()
            {
                for (int i = 0; i < ClientData.carLoaders.Length; i++)
                {
                    if (hasCar(ClientData.carLoaders[i]) && !loaderWithCar.ContainsKey(i))
                    {
                        loaderWithCar.Add(i, ClientData.carLoaders[i]);
                    }
                    if(loaderWithCar.ContainsKey(i) && !hasCar(ClientData.carLoaders[i]))
                    {
                        loaderWithCar.Remove(i);
                    }
                }

                for (int i = 0; i < loaderWithCar.Count; i++) // CarSpawn
                {
                    if (!ClientData.carOnScene.Any(s => s.carLoaderID == i))
                    {
                        ModCar newCar = new ModCar(i, loaderWithCar[i].ConfigVersion, SceneManager.GetActiveScene().name, loaderWithCar[i].placeNo);
                        ClientData.carOnScene.Add(newCar);
                        ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[i]));
                        MelonLogger.Msg("Car Spawned");
                    }
                }

                for (int i = 0; i < ClientData.carOnScene.Count; i++) // CarDespawn
                {
                    if (!loaderWithCar.ContainsKey(ClientData.carOnScene[i].carLoaderID))
                    {
                        ClientSend.SendCarInfo(new ModCar(ClientData.carOnScene[i]));
                        ClientData.carOnScene.Remove(ClientData.carOnScene[i]);
                        MelonLogger.Msg("Car Despawned");
                    }
                }
            }

        #endregion
        
        
        #region Get Parts References

            private static void GetPartsReferences()
            {
                foreach (var car in ClientData.carOnScene)
                {
                    if (!car.isReferences)
                    {
                        GetOtherPartsReferences(car);
                        GetEnginePartsReferences(car);
                        GetSuspensionsPartsReferences(car);
                        //TODO: Handle body parts references
                        car.isReferences = true;
                    }
                }
            }

            private static void GetSuspensionsPartsReferences(ModCar _car)
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
                        if (!suspensionReferencePoint.ContainsKey(j))
                        {
                            suspensionReferencePoint.Add(j, new List<PartScript>() { partsInSuspension[j] });
                        }
                        if (!suspensionReferencePoint[j].Contains(partsInSuspension[j]))
                        {
                            suspensionReferencePoint[j].Add(partsInSuspension[j]);
                        }
                    }
                }
            }

            private static void GetEnginePartsReferences(ModCar _car)
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
            }

            private static void GetOtherPartsReferences(ModCar _car)
            {
                var otherPartsObjects = ClientData.carLoaders[_car.carLoaderID].Parts;
                var otherPartsReferencePoint = _car.partInfo.OtherPartsReferences;

                for (int i = 0; i < otherPartsObjects.Count; i++)
                {
                    var partObject = ClientData.carLoaders[_car.carLoaderID].Parts._items[i].p_handle;
                    var _parts = partObject.GetComponentsInChildren<PartScript>().ToList();

                    for (int j = 0; j < _parts.Count; j++)
                    {
                        if (!otherPartsReferencePoint.ContainsKey(j))
                        {
                            otherPartsReferencePoint.Add(j, new List<PartScript>() { _parts[j] });
                        }
                        if (!otherPartsReferencePoint[j].Contains(_parts[j]))
                        {
                            otherPartsReferencePoint[j].Add(_parts[j]);
                        }
                    }
                }
            }

        #endregion
        
        #region Handle Part

            private static void UpdateParts()
            {
                foreach (var car in ClientData.carOnScene)
                {
                    if (car.isReferences)
                    {
                        UpdateOtherParts(car);
                        UpdateEngineParts(car);
                        UpdateSuspensionParts(car);
                        //TODO: Handle body parts
                    }
                }
            }

            private static void UpdateSuspensionParts(ModCar _car)
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
                        if(!suspensionParts[i].Contains(partConverted))
                        {
                            suspensionParts[i].Add(partConverted); 
                            //TODO: Send part new to server
                        }
                        
                        if (hasDifferences(suspensionParts[i][j], parts[j]))
                        {
                            suspensionParts[i][j] = partConverted;
                            MelonLogger.Msg("Suspension part updated");
                            //TODO: Send part update to server
                        }
                    }
                }
            }

            private static void UpdateEngineParts(ModCar _car)
            {
                var EnginePartsReferences = _car.partInfo.EnginePartsReferences;
                var engineParts = _car.partInfo.EngineParts;
                
                for (int i = 0; i < EnginePartsReferences.Count; i++)
                {
                    var partConverted = new ModPartScript(EnginePartsReferences[i], i, -1, partType.other);
                    if (!engineParts.ContainsKey(i))
                    {
                        engineParts.Add(i, partConverted);
                        //TODO: Send part new to server
                    }
                    else
                    {
                        if (hasDifferences(engineParts[i], EnginePartsReferences[i]))
                        {
                            engineParts[i] = partConverted;
                            MelonLogger.Msg("Engine part updated");
                            //TODO: Send part update to server
                        }
                    }
                }
            }

            private static void UpdateOtherParts(ModCar _car)
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
                        if(!otherParts[i].Contains(partConverted))
                        {
                            otherParts[i].Add(partConverted); 
                            //TODO: Send part new to server
                        }
                        
                        if (hasDifferences(otherParts[i][j], parts[j]))
                        {
                            otherParts[i][j] = partConverted;
                            MelonLogger.Msg("Other part updated");
                            //TODO: Send part update to server
                        }
                    }
                }
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
                }
                
                car.carPosition = ClientData.carLoaders[car.carLoaderID].placeNo;
                    
            }
        }

        #endregion
    }
}