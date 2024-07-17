using System;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
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
    public static bool listenToSimpleLoad = true;
    public static bool listenToDelete = true;

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
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar))]
    [HarmonyPostfix]
    public static void LoadCarHook(string name, CarLoader __instance)
    {
        if(!Client.Instance.isConnected || ! listenToSimpleLoad) { listenToSimpleLoad = true; return;}
        if(String.IsNullOrEmpty(name)) return;
            
        MelonLogger.Msg($"[CarSpawnHooks->LoadCarHook] Triggered:{name}");
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        MelonCoroutines.Start(CarSpawnManager.LoadJobCar(name, carLoaderID, __instance));

    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
    [HarmonyPrefix]
    public static void DeleteCarHook(CarLoader __instance)
    {
        if(!Client.Instance.isConnected || !listenToDelete) { listenToDelete = true; return;}
        if (string.IsNullOrEmpty(__instance.carToLoad) || SceneManager.GetActiveScene().name != "garage") return;
        
        MelonLogger.Msg($"[CarSpawnHooks->DeleteCarHook] Triggered.");
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out ModCar car))
        {
            ClientSend.DeleteCarPacket(carLoaderID);
            ClientData.Instance.loadedCars.Remove(carLoaderID);
        }
    }
}