using System;
using System.Collections;
using System.Collections.Generic;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class CarManager
    {
        private static bool partHandleRunning;
        
        public static void CarsUpdate()
        {
            if(ModSceneManager.currentScene() != GameScene.garage) return;
            
            if (!partHandleRunning)
                MelonCoroutines.Start(CarPartUpdate());
            
        }

        private static IEnumerator CarPartUpdate()
        {
            partHandleRunning = true;

            var handleParts = MelonCoroutines.Start(CarHandle.UpdatePartHandle());
            yield return handleParts;

            yield return new WaitForEndOfFrame();
            partHandleRunning = false;
        }

        public static IEnumerator SpawnCar(ModCar car)
        {
            if(!ScreenFader.Get().IsRunning())
                ScreenFader.Get().ShortFadeIn();

            MelonCoroutines.Start(LoadCar(car.carID, car.carLoaderID, car.carVersion));
            
            object waitCar = MelonCoroutines.Start(CarManager.WaitForCarHandle(car.carLoaderID));
            yield return waitCar;
            
            int waiter = 0;
            while (!car.CarFullyReceived || waiter < 20)
            {
                yield return new WaitForSeconds(0.2f);
                waiter += 1;
            }
            
            yield return new WaitForEndOfFrame();
            ScreenFader.Get().ShortFadeOut();
            MelonLogger.Msg("[CarManager->SpawnCar] Car should be loaded and synced.");
        }

        private static IEnumerator LoadCar(string id, int carLoaderID, int version)
        {
            var carDebug = GameData.Instance.carLoaders[carLoaderID].GetComponent<CarDebug>();
            carDebug.LoadCar(id, version);
            
            yield return new WaitForEndOfFrame();
            
            while (!GameData.Instance.carLoaders[carLoaderID].IsCarLoaded())
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            
            GameData.Instance.carLoaders[carLoaderID].SetWheelsColorsFromConfig();
            GameData.Instance.carLoaders[carLoaderID].PlaceAtPosition();
            
            yield return new WaitForEndOfFrame();
        }
       
        
        
        public static int GetCarLoaderID(string id)
        {
            return id switch
            {
                "1" => 0,
                "2" => 1,
                "3" => 2,
                "4" => 3,
                "5" => 4,
                _ => throw new ArgumentException($"Invalid carLoaderID: {id}")
            };
        }

        public static IEnumerator WaitForCarHandle(int carLoaderID)
        {
            int waiter = 0;
            if (!ClientData.Instance.LoadedCars.ContainsKey(carLoaderID))
            {
                while (waiter < 30)
                {
                    yield return new WaitForSeconds(0.1f);
                    if (ClientData.Instance.LoadedCars.ContainsKey(carLoaderID))
                    {
                        waiter = 0;
                        break;
                    }
                    waiter++;
                }
            }
            
            while(ClientData.Instance.GameReady == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();

            var car = ClientData.Instance.LoadedCars[carLoaderID];
            if (car.isHandled == false)
            {
                while (waiter < 40)
                {
                    yield return new WaitForSeconds(.1f);
                    if(car.isHandled)
                        break; 
                    waiter++;
                }
            }
        }

        public static IEnumerator HandlePartScripts(List<ModPartScript> partScripts, int carLoaderID)
        {
            var waitforCar = MelonCoroutines.Start(WaitForCarHandle(carLoaderID));
            yield return waitforCar;

            yield return new WaitForEndOfFrame();

            object handlePartScripts = MelonCoroutines.Start(CarUpdater.UpdatePartScripts(partScripts, carLoaderID));

            yield return handlePartScripts;
            yield return new WaitForEndOfFrame();
            
            SetPartsReceived(partScripts[0].type, carLoaderID);
        }
        
        public static IEnumerator HandlePartScript(ModPartScript partScripts, int carLoaderID)
        {
            var waitforCar = MelonCoroutines.Start(WaitForCarHandle(carLoaderID));
            yield return waitforCar;

            yield return new WaitForEndOfFrame();

             CarUpdater.UpdatePartScript(partScripts, ClientData.Instance.LoadedCars[carLoaderID]);
        }
        
        private static void SetPartsReceived(ModPartType carPart, int carLoaderID)
        {
            ModCar car = ClientData.Instance.LoadedCars[carLoaderID];
            switch (carPart)
            {
                case ModPartType.other:
                    car.receivedOtherParts = true;
                    break;
                case ModPartType.engine:
                    car.receivedEngineParts = true;
                    break;
                case ModPartType.suspension:
                    car.receivedSuspensionParts = true;
                    break;
                case ModPartType.driveshaft:
                    car.receivedDriveshaftParts = true;
                    break;
            }
        }

        public static IEnumerator HandleCarParts(List<ModCarPart> carParts, int carLoaderID)
        {
            var waitforCar = MelonCoroutines.Start(WaitForCarHandle(carLoaderID));
            yield return waitforCar;

            yield return new WaitForEndOfFrame();

            object handleCarParts = MelonCoroutines.Start(CarUpdater.UpdateCarParts(carParts, carLoaderID));

            yield return handleCarParts;
            yield return new WaitForEndOfFrame();

            ClientData.Instance.LoadedCars[carLoaderID].receivedBodyParts = true;
        }
        
        public static IEnumerator HandleCarPart(ModCarPart carParts, int carLoaderID)
        {
            var waitforCar = MelonCoroutines.Start(WaitForCarHandle(carLoaderID));
            yield return waitforCar;

            yield return new WaitForEndOfFrame();

            CarUpdater.UpdateCarPart(carParts, ClientData.Instance.LoadedCars[carLoaderID]);
        }
        
        public static IEnumerator UpdateCarOnSceneChange()
        {
            if (ClientData.Instance.tempCarList.Count > 0)
            {
                
                while (ClientData.Instance.tempCarList.Count > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                ClientSend.SendResyncCars();
            }
        }
    }
}