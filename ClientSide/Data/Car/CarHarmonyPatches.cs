using System;
using System.Collections;
using CMS21Together.ClientSide.Handle;
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
    public static class CarHarmonyPatches
    {
        public static bool ListenToCursorBlock;
        private static bool screenfadeFix;
        
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar))]
        [HarmonyPrefix]
        public static void LoadCarPrePatch(string name, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, name));
            }
        }
        
        [HarmonyPatch(typeof(CarDebug), nameof(CarDebug.RunLoadCar),
            new Type[]{ typeof(string), typeof(int)})]
        [HarmonyPrefix]
        public static void RunLoadCarPrePatch(string carToLoad, int configVersion, CarDebug __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance.GetComponent<CarLoader>(), carToLoad));
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "LoadCarFromFile", new Type[]{ typeof(NewCarData)})]
        [HarmonyPrefix]
        public static void LoadCarFromFilePrePatch(NewCarData carDataCheck, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                {
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, carDataCheck.carToLoad));
                    //MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, carDataCheck));
                    
                }
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "SetUnmountWithCarParts")]
        [HarmonyPostfix]
        public static void SetEnginePatch(CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.LoadCar(__instance));
            }
        }
        
        [HarmonyPatch(typeof(Cursor3D), "BlockCursor")]
        [HarmonyPrefix]
        public static bool EnableGamepadMountObjectPatch(bool block, Cursor3D __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ListenToCursorBlock)
                {
                    MelonLogger.Msg("BlockCursor Bypass!");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(ScreenFader), "NormalFadeIn")]
        [HarmonyPrefix]
        public static void FadeInFix(ScreenFader __instance)
        {       
            if (Client.Instance.isConnected)
                if (ModSceneManager.currentScene() == GameScene.garage)
                {
                    screenfadeFix = true;
                    MelonCoroutines.Start(FadeSoftLockCoroutine());
                }
        }
        
        [HarmonyPatch(typeof(ScreenFader), "NormalFadeOut")]
        [HarmonyPrefix]
        public static void FadeOutFix(ScreenFader __instance)
        {
            if (Client.Instance.isConnected)
                if (ModSceneManager.currentScene() == GameScene.garage)
                    screenfadeFix = false;
        }
        
        public static IEnumerator ResetCursorBlockCoroutine()
        {
            ListenToCursorBlock = true;
            yield return new WaitForSeconds(0.1f);
            ListenToCursorBlock = false;
        }
        
        public static IEnumerator FadeSoftLockCoroutine()
        {
            
            yield return new WaitForSeconds(8f);
            if(screenfadeFix)
                GameData.Instance.screenFader.NormalFadeOut();
                
        }
        
        [HarmonyPatch(typeof(NotificationCenter), "SelectSceneToLoad", 
            new Type[]{ typeof(string), typeof(SceneType), typeof(bool), typeof(bool)})]
        [HarmonyPrefix]
        public  static void SceneChangePatch( string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
        {
            if (Client.Instance.isConnected || ServerData.isRunning)
            {
                if (ClientData.Instance.asGameStarted)
                {
                    try
                    {
                        if (newSceneName != "garage")
                        {
                            ClientData.Instance.tempCarList.Clear();
                            var profile = SavesManager.currentSave;
                            for (var i = 0; i < profile.carsInGarage.Count; i++)
                            {
                                var SaveCar = profile.carsInGarage[i];
                                ClientData.Instance.tempCarList.Add((SaveCar.index, SaveCar.carToLoad));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MelonLogger.Msg("Error on sceneChange: " + e);
                    }
                }
            }
        }
    }
}