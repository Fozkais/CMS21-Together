using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21MP.SharedData;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Enumerable = System.Linq.Enumerable;

namespace CMS21MP.ClientSide.Data
{
    public static class Car
    {
        
        public static Dictionary<int, CarLoader> loaderWithCar = new Dictionary<int, CarLoader>();
        
        private static bool carSpawnHandleRunning;
        private static bool partHandleRunning;
        private static bool carMovementHandleRunning;
        
        public static void UpdateCarInfo()
        {
            if (!carSpawnHandleRunning)
                MelonCoroutines.Start(CarSpawnHandle(2));
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
                    else if(loaderWithCar.ContainsKey(i) && !hasCar(ClientData.carLoaders[i]))
                    {
                        loaderWithCar.Remove(i);
                    }
                }

                for (int i = 0; i < loaderWithCar.Count; i++) // CarSpawn
                {
                    if (!ClientData.carOnScene.Any(s => s.carLoaderID == i))
                    {
                        ClientData.carOnScene.Add(new ModCar(i));
                        // TODO: Send car spawn to server
                    }
                }

                for (int i = 0; i < ClientData.carOnScene.Count; i++) // CarDespawn
                {
                    if (!loaderWithCar.ContainsKey(ClientData.carOnScene[i].carLoaderID))
                    {
                        ClientData.carOnScene.Remove(ClientData.carOnScene[i]);
                        // TODO: Send car destroy to server
                    }
                }
            }

        #endregion
        
        
        #region Get Parts References

        

        #endregion
        
        #region Handle Part
        
        
        #endregion

        #region Car Moving

        public static void HandleCarPos()
        {
            foreach (var car in ClientData.carOnScene)
            {
                if (loaderWithCar[car.carLoaderID].placeNo != car.carPosition)
                {
                    car.carPosition = loaderWithCar[car.carLoaderID].placeNo;
                    //TODO: Send car position to server
                }
                    
            }
        }

        #endregion
    }
}