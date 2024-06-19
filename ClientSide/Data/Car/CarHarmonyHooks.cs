using System;
using System.Collections;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Car
{
    [HarmonyPatch]
    public static class CarHarmonyHooks
    {
        
        public static bool ListenToDeleteCar = true;
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
        [HarmonyPrefix]
        public static void DeleteCarHook(CarLoader __instance)
        {
            if (!Client.Instance.isConnected) return;
            if (ModSceneManager.currentScene() != GameScene.garage) return;
            
            string carLoaderID = __instance.gameObject.name[10].ToString();
            int convertedLoaderID = CarManager.GetCarLoaderID(carLoaderID);

            if (ListenToDeleteCar)
            {
                if (ClientData.Instance.LoadedCars.ContainsKey(convertedLoaderID))
                {
                    var car = ClientData.Instance.LoadedCars[convertedLoaderID];
                    ClientSend.SendModCar(new ModCar(car), true);
                    ClientData.Instance.LoadedCars.Remove(car.carLoaderID);
                }
            }
                    
            
        }
        
        
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar))]
        [HarmonyPrefix]
        public static void LoadCarHook(string name, CarLoader __instance)
        {
            if(!Client.Instance.isConnected || ModSceneManager.currentScene() != GameScene.garage) return;

            string stringID = __instance.gameObject.name[10].ToString();
            int carLoaderID = CarManager.GetCarLoaderID(stringID);
            MelonLogger.Msg($"[CarHarmonyHooks->LoadCar] {name} is being loaded on CarLoader {carLoaderID}.");
            
            if(ClientData.Instance.tempCarList.Contains((carLoaderID, name))) return;

            MelonCoroutines.Start(InitializeCar(__instance, name, carLoaderID));

        }

        private static IEnumerator InitializeCar(CarLoader carLoader, string name, int carLoaderID)
        {
            if (String.IsNullOrEmpty(name)) yield break;
            if (ClientData.Instance.LoadedCars.ContainsKey(carLoaderID)) yield break;

            ModCar car = new ModCar(carLoaderID, name, carLoader.ConfigVersion, carLoader.placeNo);
            ClientSend.SendModCar(car);
            
            car.isFromServer = false;
            ClientData.Instance.LoadedCars.Add(carLoaderID, car);
        }


        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.PreparePartScriptCuller))]
        [HarmonyPrefix]
        public static void CarLoadedHook(CarLoader __instance)
        {
            if(!Client.Instance.isConnected || ModSceneManager.currentScene() != GameScene.garage) return;
            
            string stringID = __instance.gameObject.name[10].ToString();
            int carLoaderID = CarManager.GetCarLoaderID(stringID);
            MelonLogger.Msg($"[CarHarmonyHooks->CarLoadedHook] {__instance.carToLoad} on CarLoader {carLoaderID} finished loading.");

            if (ClientData.Instance.tempCarList.Contains((carLoaderID, __instance.carToLoad)))
            {
                ClientData.Instance.tempCarList.Remove((carLoaderID,  __instance.carToLoad));
                __instance.DeleteCar();
                return;
            }

            MelonCoroutines.Start(StartHandlingCar(carLoaderID));
        }

        private static IEnumerator StartHandlingCar(int carLoaderID)
        {
            yield return new WaitForSeconds(1f);
            yield return new WaitForEndOfFrame();

            MelonCoroutines.Start(CarReferences.GetCarReferences(carLoaderID));
            object waitCar = MelonCoroutines.Start(CarManager.WaitForCarHandle(carLoaderID));
            yield return waitCar;
            
            
            
            MelonLogger.Msg($"{ClientData.Instance.LoadedCars[carLoaderID].carID} is ready for sync.");
        }
        
        
        [HarmonyPatch(typeof(NotificationCenter), "SelectSceneToLoad", new Type[]{ typeof(string), typeof(SceneType), typeof(bool), typeof(bool)})]
        [HarmonyPrefix]
        public  static void SceneChangeHook( string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
        {
            if (!Client.Instance.isConnected || !ServerData.isRunning) return;
            if (!ClientData.Instance.GameReady) return;
            
            ListenToDeleteCar = false;
            try
            {
                if (newSceneName != "garage")
                {
                    ClientData.Instance.tempCarList.Clear();
                    var profile = SavesManager.currentSave;
                    for (var i = 0; i < profile.carsInGarage.Count; i++)
                    {
                        var SaveCar = profile.carsInGarage[i];
                        if (!String.IsNullOrEmpty(SaveCar.carToLoad))
                        {
                            ClientData.Instance.tempCarList.Add((i, SaveCar.carToLoad));
                        }
                    }
                    ClientSend.SendResyncCars(ClientData.Instance.tempCarList);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Error on sceneChange: " + e);
            }
        }
        
        private static bool ListenToCursorBlock;
        
        [HarmonyPatch(typeof(Cursor3D), "BlockCursor")]
        [HarmonyPrefix]
        public static void BlockCursorPatch(bool block, Cursor3D __instance)
        {
            if (Client.Instance.isConnected)
            {
                if(block)
                    if(ListenToCursorBlock)
                        MelonCoroutines.Start(FixCursorLock());
            }
        }

        private static IEnumerator FixCursorLock()
        {
            yield return new WaitForSeconds(2.5f);
            if (Cursor3D.Get().isCursorBlocked)
            {
                ListenToCursorBlock = false;
                Cursor3D.Get().BlockCursor(false);
                ListenToCursorBlock = true;
            }
        }
    }
}