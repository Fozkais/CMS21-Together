using System.Collections;
using CMS21Together.Shared.Data;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using Type = System.Type;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class PartUpdateHooks
{
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.DoMount))]
    [HarmonyPrefix]
    public static void DoMountHook(PartScript __instance)
    {
        MelonCoroutines.Start(HandleDoMount(__instance));
    }

    private static bool BoltsAreMounted(PartScript partScript)
    {
        bool allMounted = true;
        foreach (MountObject bolt in partScript.MountObjects)
        {
            if (bolt.unmounted)
            {
                allMounted = false;
                break;
            }
        }

        return allMounted;
    }

    private static IEnumerator HandleDoMount(PartScript partScript)
    {
        if (!partScript.oneClickUnmount)
        {
            int counter = 0;
            while (!BoltsAreMounted(partScript) || counter  <= 16)
            {
                yield return new WaitForSeconds(0.25f);
                counter++;
            }
        }
        MelonLogger.Msg("[PartUpdateHooks->DoMountHook] Triggered.");
        
        int loaderID = (partScript.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[loaderID];

        if (FindPartInDictionaries(car, partScript, out string dictionaryName, out int key, out int? index))
        {
            MelonLogger.Msg($"[PartUpdateHooks->DoMountHook] PartScript found in {dictionaryName} at key {key}" + (index.HasValue ? $" and index {index.Value}" : ""));
        }
        else
        {
            MelonLogger.Msg("[PartUpdateHooks->DoMountHook] PartScript not found in any dictionary.");
        }
    }
    
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.Hide))]
    [HarmonyPrefix]
    public static void HideHook(PartScript __instance) // best way i've found to detect when a partScript is unmounted
    {
        MelonLogger.Msg("[PartUpdateHooks->HideHook] Triggered."); // yes
        int loaderID = (__instance.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[loaderID];

        if (FindPartInDictionaries(car, __instance, out string dictionaryName, out int key, out int? index))
        {
            MelonLogger.Msg($"[PartUpdateHooks->HideHook] PartScript found in {dictionaryName} at key {key}" + (index.HasValue ? $" and index {index.Value}" : ""));
        }
        else
        {
            MelonLogger.Msg("[PartUpdateHooks->HideHook] PartScript not found in any dictionary.");
        }
    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TakeOffCarPart), new Type[] { typeof(string), typeof(bool) })]
    [HarmonyPrefix]
    public static void TakeOffCarPartHook(string name, bool off, CarLoader __instance) // handle both Mount/Unmount with the boolean
    {
        MelonLogger.Msg($"[PartUpdateHooks->TakeOffCarPart] Triggered:{off}"); // yes
        
        int loaderID = (__instance.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[loaderID];
        
        if (FindBodyPartInDictionary(car,  name, out int key, out int? index))
        {
            MelonLogger.Msg($"[PartUpdateHooks->TakeOffCarPartHook] BodyPart found at key {key}" + (index.HasValue ? $" and index {index.Value}" : ""));
        }
        else
        {
            MelonLogger.Msg("[PartUpdateHooks->TakeOffCarPartHook] BodyPart not found in dictionary.");
        }
    }
    
    private static bool FindPartInDictionaries(ModCar car, PartScript partScript, out string dictionaryName, out int key, out int? index)
    {
        index = null;

        foreach (var kvp in car.partInfo.OtherPartsReferences)
        {
            int listIndex = kvp.Value.FindIndex(part => part == partScript);
            if (listIndex >= 0)
            {
                dictionaryName = nameof(car.partInfo.OtherPartsReferences);
                key = kvp.Key;
                index = listIndex;
                return true;
            }
        }

        foreach (var kvp in car.partInfo.SuspensionPartsReferences)
        {
            int listIndex = kvp.Value.FindIndex(part => part == partScript);
            if (listIndex >= 0)
            {
                dictionaryName = nameof(car.partInfo.SuspensionPartsReferences);
                key = kvp.Key;
                index = listIndex;
                return true;
            }
        }

        foreach (var kvp in car.partInfo.EnginePartsReferences)
        {
            if (kvp.Value == partScript)
            {
                dictionaryName = nameof(car.partInfo.EnginePartsReferences);
                key = kvp.Key;
                return true;
            }
        }

        foreach (var kvp in car.partInfo.DriveshaftPartsReferences)
        {
            if (kvp.Value == partScript)
            {
                dictionaryName = nameof(car.partInfo.DriveshaftPartsReferences);
                key = kvp.Key;
                return true;
            }
        }

        dictionaryName = null;
        key = 0;
        return false;
    }
    
    private static bool FindBodyPartInDictionary(ModCar car, string carPartName, out int key, out int? index)
    {
        index = null;
        
        foreach (var kvp in car.partInfo.BodyPartsReferences)
        {
            if (kvp.Value.name == carPartName)
            {
                key = kvp.Key;
                return true;
            }
        }
        
        key = 0;
        return false;
    }
    
    
}