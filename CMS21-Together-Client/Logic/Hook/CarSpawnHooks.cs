using System.Collections.Generic;
using CMS21Together.Data;
using CMS21Together.Logic.Car;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class CarSpawnHooks
    {
        // Per-loader suppression: prevents the hooks below from re-emitting a
        // network request when the mod itself applies an update received from
        // the server. Keyed by CarLoaderID rather than a single global flag so
        // that concurrent updates on different loaders don't interfere with
        // each other's suppression window.
        private static readonly HashSet<int> SuppressedLoaders = new HashSet<int>();

        public static void Suppress(int carLoaderId)
        {
            lock (SuppressedLoaders) SuppressedLoaders.Add(carLoaderId);
        }

        public static void Release(int carLoaderId)
        {
            lock (SuppressedLoaders) SuppressedLoaders.Remove(carLoaderId);
        }

        private static bool IsSuppressed(int carLoaderId)
        {
            lock (SuppressedLoaders) return SuppressedLoaders.Contains(carLoaderId);
        }

        // Hook the LoadCar(string) method
        [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar), typeof(string))]
        [HarmonyPrefix]
        public static bool LoadCarHook(string name, CarLoader __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (GameScript.Get() == null || GameScript.Get().CurrentSceneType != SceneType.Garage) return true;
            if (!ClientData.IsInventorySynced || !ClientData.IsGarageStateSynced) return true;
            if (IsSuppressed(CarLoaderPlaces.Get().GetCarLoaderId(__instance))) return true;

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
            if (__instance == null || string.IsNullOrEmpty(__instance.carToLoad)) return true;
            if (IsSuppressed(CarLoaderPlaces.Get().GetCarLoaderId(__instance))) return true;

            // Request delete from server to notify OTHER clients
            MelonCoroutines.Start(CarSpawnManager.RequestCarDelete(__instance));

            // Return true to allow the sender to execute the native DeleteCar!
            return true;
        }
    }
}
