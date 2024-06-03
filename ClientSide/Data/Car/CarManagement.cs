using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2Cpp;
using Il2CppCMS;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    public static class CarManagement
    {
        private static bool partHandleRunning;
        private static bool detectCarMove;

        public static void UpdateCars()
        {
            if(ModSceneManager.currentScene() != GameScene.garage) return;
            
            if (!partHandleRunning)
                MelonCoroutines.Start(CarPartUpdateHandle());
            if (!detectCarMove)
                MelonCoroutines.Start(CarMoveAndLoadHandle());
        }

        private static IEnumerator CarMoveAndLoadHandle()
        {
            detectCarMove = true;
            DetectCarMoves();
            yield return new WaitForSeconds(1);
            detectCarMove = false;
        }

        private static IEnumerator CarPartUpdateHandle()
        {
            partHandleRunning = true;

            foreach (ModCar car in ClientData.Instance.LoadedCars.Values)
            {
                MelonCoroutines.Start(PartsHandle.HandleParts(car));
            }

            yield return new WaitForSeconds(0.20f);
            partHandleRunning = false;
        }


        public static IEnumerator LoadCarParts(List<ModPartScript> carParts, int carLoaderID)
        {
            
            var waitforCar = MelonCoroutines.Start(WaitCarToBeReady(carLoaderID));
            yield return waitforCar;

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(.5f);
            yield return new WaitForEndOfFrame();
            
            MelonLogger.Msg($"Processing parts: {carParts[0].type}, {carParts.Count}.");
            
            foreach (ModPartScript part in carParts)
            {
                MelonCoroutines.Start(CarUpdate.HandleNewPart(part, carLoaderID));
            }
            MelonLogger.Msg($"Finished parts: {carParts[0].type}.");
        }
        
        public static IEnumerator LoadBodyParts(List<ModCarPart> carParts, int carLoaderID)
        {
            var waitforCar = MelonCoroutines.Start(WaitCarToBeReady(carLoaderID));
            yield return waitforCar;
            
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(.5f);
            yield return new WaitForEndOfFrame();
            
            MelonLogger.Msg($"Processing BodyParts : {carParts.Count}.");
            
            foreach (ModCarPart part in carParts)
            {
                MelonCoroutines.Start(CarUpdate.HandleNewBodyPart(part, carLoaderID));
            }
            MelonLogger.Msg("Finished BodyParts.");
        }

        public static IEnumerator WaitCarToBeReady(int carLoaderID)
        {
            
            if (!ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
            {
                int waiter1 = 0;
                while (waiter1 < 40)
                {
                    yield return new WaitForSeconds(.1f);
                    if(ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
                        break;
                    waiter1++;
                }
            }
            
            var car = ClientData.Instance.LoadedCars.First(s => s.Key == carLoaderID).Value;
            
            if (!car.isCarReady)
            {
                int waiter = 0;
                while (waiter < 80)
                {
                    yield return new WaitForSeconds(.1f);
                    if(car.isCarReady)
                        break; 
                    waiter++;
                }
            }
        }

        public static IEnumerator WaitForCarHandle(int carLoaderID)
        {
            if (!ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
            {
                int waiter1 = 0;
                while (waiter1 < 40)
                {
                    yield return new WaitForSeconds(.1f);
                    if(ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
                        break;
                    waiter1++;
                }
            }
            
            while(ClientData.Instance.GameReady == false) // DO NOT REMOVE!
                yield return new WaitForSeconds(.5f);
                
            yield return new WaitForSeconds(1);
            yield return new WaitForEndOfFrame();
            
            if (!ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
            {
                int waiter1 = 0;
                while (waiter1 < 40)
                {
                    yield return new WaitForSeconds(.1f);
                    if(ClientData.Instance.LoadedCars.Any(s => s.Key == carLoaderID))
                        break;
                    waiter1++;
                }
            }
            var car = ClientData.Instance.LoadedCars.First(s => s.Key == carLoaderID).Value;
            
            if (!car.isHandled)
            {
                int waiter = 0;
                while (waiter < 80)
                {
                    yield return new WaitForSeconds(.1f);
                    if(car.isHandled)
                        break; 
                    waiter++;
                }
            }
            
            
            MelonLogger.Msg($"{car.carID} is handled and ready!");
        }
        
        
        
        /*public static IEnumerator DetectUnloadedCar()
        {
            while (!ClientData.Instance.GameReady)
                yield return new WaitForSeconds(1);
            
            yield return new WaitForSeconds(2);
            
            if(!Client.Instance.isConnected) yield break;
            
            for (int i = 0; i < ClientData.Instance.LoadedCars.Count; i++)
            {
                ModCar car = ClientData.Instance.LoadedCars[i];
                if (!car.isFromServer)
                {
                    int count = 0;
                    while (!car.isHandled && count < 30)
                    {
                        count += 1;
                        yield return new WaitForSeconds(0.1f);
                    }

                    if (String.IsNullOrEmpty(GameData.Instance.carLoaders[car.carLoaderID].carToLoad) &&
                        GameData.Instance.carLoaders[car.carLoaderID].carParts == null)
                    {
                        ClientSend.SendModCar(new ModCar(car), true);
                        ClientData.Instance.LoadedCars.Remove(car.carLoaderID);
                    }
                }
            }
        }*/
        
        public static IEnumerator LoadCar(string carName, int id, int configVersion)
        {
            if(!ScreenFader.Get().isFadedIn)
                ScreenFader.Get().ShortFadeIn();
            
            var carDebug = GameData.Instance.carLoaders[id].GetComponent<CarDebug>();

            var loadCar = MelonCoroutines.Start(RunLoadCar(carDebug, carName, configVersion));

            yield return loadCar;
            
            while (!GameData.Instance.carLoaders[id].IsCarLoaded())
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            
            GameData.Instance.carLoaders[id].SetWheelsColorsFromConfig();
            GameData.Instance.carLoaders[id].PlaceAtPosition();
            
            yield return new WaitForEndOfFrame();
            
            ScreenFader.Get().ShortFadeOut();
        }
        
        private static IEnumerator RunLoadCar(CarDebug carDebug, string carToLoad, int configVersion)
        {
            CarLoader carLoader = carDebug.GetComponent<CarLoader>();
            carLoader.ConfigVersion = configVersion;
            carDebug.StartCoroutine(carLoader.LoadCar(carToLoad));
            while (!carLoader.IsCarLoaded())
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
           // carLoader.SetRandomCarColor();
           // carLoader.FluidsData.SetLevelAndCondition(1f, 1f, CarFluidType.All);
            carLoader.SetWheelsColorsFromConfig();
            carLoader.PlaceAtPosition();
        }
        
        public static void DetectCarMoves()
        {
            foreach (var car in ClientData.Instance.LoadedCars)
            { 
                if (!car.Value.isFromServer)
                {
                    if (GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo != car.Value.carPosition 
                        && GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo != -1)
                    {
                        ClientSend.SendCarPosition(car.Value.carLoaderID, GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo);
                        car.Value.carPosition = GameData.Instance.carLoaders[car.Value.carLoaderID].placeNo;
                    }
                }
                    
                        
            }
        }
        
        public static IEnumerator CarSpawnFade(ModCar car)
        {
            if(!ScreenFader.Get().isFadedIn)
                ScreenFader.Get().ShortFadeIn();


            MelonCoroutines.Start(CarManagement.LoadCar(car.carID, car.carLoaderID, car.carVersion));
            // carLoader.gameObject.GetComponentInChildren<CarDebug>().LoadCar(car.carID, car.carVersion);

            int count = 0;
            while (!(car.isReferenced && car.isHandled) && count < 20)
            {
                count += 1;
                yield return new WaitForSeconds(0.1f);
            }
            
            int _count = 0;
            while (!(car.receivedOtherParts && car.receivedEngineParts 
                                            && car.receivedSuspensionParts && car.receivedBodyParts
                                            && car.receivedDriveshaftParts) && _count < 25)
            {
                _count += 1;
                yield return new WaitForSeconds(0.1f);
            }

            yield return new WaitForEndOfFrame();
            ScreenFader.Get().ShortFadeOut();
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