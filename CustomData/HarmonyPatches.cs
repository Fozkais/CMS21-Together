using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.CustomData
{
    [HarmonyPatch]
    public class HarmonyPatches
    {
        [HarmonyPatch(typeof(CarLoader), "LoadCar")]
        [HarmonyPostfix]
        public static void LoadCarPatch(string name)
        {
            MelonLogger.Msg("Car loaded: " + name);
        }
        
        [HarmonyPatch(typeof(CarDebug), "LoadCar", new Type[] {typeof(string), typeof(int)})]
        [HarmonyPostfix]
        public static void Debug_LoadCarPatch(string carToLoad, int configVersion)
        {
            MelonLogger.Msg("Car loaded: " + carToLoad + " Config version: " + configVersion);
        }
        
        /*[HarmonyPatch(typeof(CarDebug), "RunLoadCar")]
        [HarmonyPostfix]
        public static void Debug_LoadCarPatch()
        {
            MelonLogger.Msg("Car loaded");
        }*/
    }
}