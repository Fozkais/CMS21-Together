using System;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class PartUpdateHooks
{
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.DoMount))]
    [HarmonyPrefix]
    public static void DoMountHook()
    {
        MelonLogger.Msg("[PartUpdateHooks->DoMountHook] Triggered."); // yes
    }
    
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.Hide))]
    [HarmonyPrefix]
    public static void HideHook() // best way i've found to detect when a partScript is unmounted
    {
        MelonLogger.Msg("[PartUpdateHooks->HideHook] Triggered."); // yes
    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TakeOffCarPart), new Type[] { typeof(string), typeof(bool) })]
    [HarmonyPrefix]
    public static void TakeOffCarPartHook(string name, bool off) // handle both Mount/Unmount with the boolean
    {
        MelonLogger.Msg($"[PartUpdateHooks->TakeOffCarPart] Triggered:{off}"); // yes
    }
    
    
}