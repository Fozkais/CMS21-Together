using System;
using System.Collections;
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
        
        [HarmonyPatch(typeof(CarLoader), "LoadCar")]
        [HarmonyPrefix]
        public static void LoadCarPrePatch(string name, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, name));
            }
        }
        [HarmonyPatch(typeof(CarLoader), "LoadCar")]
        [HarmonyPostfix]
        public static void LoadCarPostPatch(string name, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePostPatch(__instance, name));
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "LoadCarFromFile", new Type[]{ typeof(NewCarData)})]
        [HarmonyPrefix]
        public static void LoadCarFromFilePrePatch(NewCarData carDataCheck, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePrePatch(__instance, carDataCheck.carToLoad));
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "LoadCarFromFile", new Type[]{ typeof(NewCarData)})]
        [HarmonyPostfix]
        public static void LoadCarFromFilePostPatch(NewCarData carDataCheck, CarLoader __instance)
        {
            if (Client.Instance.isConnected)
            {
                if (ModSceneManager.currentScene() == GameScene.garage)
                    MelonCoroutines.Start(CarInitialization.InitializePostPatch(__instance, carDataCheck.carToLoad));
            }
        }
        
        [HarmonyPatch(typeof(CarLoader), "SetEngine")]
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
        
        public static IEnumerator ResetCursorBlockCoroutine()
        {
            ListenToCursorBlock = true;
            yield return new WaitForSeconds(0.1f);
            ListenToCursorBlock = false;
        }
    }
}