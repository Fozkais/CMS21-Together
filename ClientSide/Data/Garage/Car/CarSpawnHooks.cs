using System;
using CMS21Together.ServerSide;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using IEnumerator = System.Collections.IEnumerator;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class CarSpawnHooks
{
    public static bool listenToLoad = true;

    public static void Reset()
    {
        listenToLoad = true;
    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCarFromFile), new Type[] { typeof(NewCarData) })]
    [HarmonyPostfix]
    public static void LoadCarFromFileHook(NewCarData carDataCheck, CarLoader __instance)
    {
        if(!Client.Instance.isConnected || !listenToLoad) { listenToLoad = true; return;}
        if(String.IsNullOrEmpty(carDataCheck.carToLoad)) return;
            
        MelonLogger.Msg($"[CarSpawnHooks->LoadCarFromFileHook] Triggered:{carDataCheck.carToLoad}");
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        MelonCoroutines.Start(CarSpawnManager.LoadCar(carDataCheck, carLoaderID, __instance.placeNo));

    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
    [HarmonyPrefix]
    public static void DeleteCarHook(CarLoader __instance)
    {
        if(!Client.Instance.isConnected) { return;}
        if (string.IsNullOrEmpty(__instance.carToLoad) || SceneManager.GetActiveScene().name != "garage") return;
        
        MelonLogger.Msg($"[CarSpawnHooks->DeleteCarHook] Triggered.");
    }
}