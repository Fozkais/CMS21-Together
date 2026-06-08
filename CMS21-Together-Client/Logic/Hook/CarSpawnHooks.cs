using System.Collections;
using CMS21Together.Data;
using CMS21Together.Logic.Car;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class CarSpawnHooks
    {
        public static bool IgnoreCarSpawnHooks = false;
        

        // Hook the LoadCar(string) method
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar), typeof(string))]
        [HarmonyPrefix]
        public static bool LoadCarHook(string name, CarLoader __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (GameScript.Get() == null || GameScript.Get().CurrentSceneType != SceneType.Garage) return true;
            if (!ClientData.IsInventorySynced || !ClientData.IsGarageStateSynced) return true;
            if (IgnoreCarSpawnHooks) return true;

            // Request spawn from server to notify OTHER clients
            MelonCoroutines.Start(CarSpawnManager.RequestCarSpawn(name, __instance));

            // Return true to allow the sender to execute the native LoadCar!
            // This prevents the sender's TakeJob coroutine from softlocking.
            return true;
        }

        // Hook the delete
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new System.Type[] { })]
        [HarmonyPrefix]
        public static bool DeleteCarHook(CarLoader __instance)
        {
            if (GameScript.Get() == null || GameScript.Get().CurrentSceneType != SceneType.Garage) return true;
            if (!ClientData.IsInventorySynced || !ClientData.IsGarageStateSynced) return true;
            if (IgnoreCarSpawnHooks) return true;
            
            if (__instance == null || string.IsNullOrEmpty(__instance.carToLoad)) return true;

            // Request delete from server to notify OTHER clients
            MelonCoroutines.Start(CarSpawnManager.RequestCarDelete(__instance));

            // Return true to allow the sender to execute the native DeleteCar!
            return true;
        }
    }
}
