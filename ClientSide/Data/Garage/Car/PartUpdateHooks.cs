using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using Type = System.Type;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class PartUpdateHooks
{
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.DoMount))]
    [HarmonyPostfix]
    public static void DoMountHook(PartScript __instance)
    {
        if(!Client.Instance.isConnected) { return;}
        
        MelonCoroutines.Start(HandleDoMount(__instance));
    }
    private static bool AreBoltsMounted(PartScript partScript)
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
            while (!AreBoltsMounted(partScript) || counter  <= 16)
            {
                yield return new WaitForSeconds(0.25f);
                counter++;
            }
        }
        MelonLogger.Msg("[PartUpdateHooks->DoMountHook] Triggered.");
        
        int carLoaderID = (partScript.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[carLoaderID];

        if (FindPartInDictionaries(car, partScript, out ModPartType partType, out int key, out int? index))
            MelonCoroutines.Start(SendPartUpdate(car, carLoaderID, key, index, partType));
        else
            MelonLogger.Msg("[PartUpdateHooks->DoMountHook] PartScript not found in any dictionary.");
    }
    
    [HarmonyPatch(typeof(PartScript), nameof(PartScript.Hide))]
    [HarmonyPostfix]
    public static void HideHook(PartScript __instance) // best way i've found to detect when a partScript is unmounted
    {
        if(!Client.Instance.isConnected) { return;}
        
        int carLoaderID = (__instance.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[carLoaderID];

        if (FindPartInDictionaries(car, __instance, out ModPartType partType, out int key, out int? index))
           MelonCoroutines.Start(SendPartUpdate(car, carLoaderID, key, index, partType));
            
    }
    
    [HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TakeOffCarPart), new Type[] { typeof(string), typeof(bool) })]
    [HarmonyPostfix]
    public static void TakeOffCarPartHook(string name, bool off, CarLoader __instance) // handle both Mount/Unmount with the boolean
    {
        if(!Client.Instance.isConnected) { return;}
        
        int carLoaderID = (__instance.gameObject.name[10] - '0') - 1;
        ModCar car = ClientData.Instance.loadedCars[carLoaderID];
        
        if (FindBodyPartInDictionary(car,  name, out int key))
        {
            CarPart part = car.partInfo.BodyPartsReferences[key];
            MelonCoroutines.Start(SendBodyPart(part, key, carLoaderID));
        }
    }
    
    public static bool FindPartInDictionaries(ModCar car, PartScript partScript, out ModPartType partType, out int key, out int? index)
    {
        index = null;

        foreach (var kvp in car.partInfo.OtherPartsReferences)
        {
            int listIndex = kvp.Value.FindIndex(part => part == partScript);
            if (listIndex >= 0)
            {
                partType = ModPartType.other;
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
                partType = ModPartType.suspension;
                key = kvp.Key;
                index = listIndex;
                return true;
            }
        }

        foreach (var kvp in car.partInfo.EnginePartsReferences)
        {
            if (kvp.Value == partScript)
            {
                partType = ModPartType.engine;
                key = kvp.Key;
                return true;
            }
        }

        foreach (var kvp in car.partInfo.DriveshaftPartsReferences)
        {
            if (kvp.Value == partScript)
            {
                partType = ModPartType.driveshaft;
                key = kvp.Key;
                return true;
            }
        }

        partType = default;
        key = 0;
        MelonLogger.Msg("[PartUpdateHooks->FindPartInDictionaries] PartScript not found in any dictionary.");
        return false;
    }
    public static bool FindBodyPartInDictionary(ModCar car, string carPartName, out int key)
    {
        foreach (var kvp in car.partInfo.BodyPartsReferences)
        {
            if (kvp.Value.name == carPartName)
            {
                key = kvp.Key;
                return true;
            }
        }
        
        key = 0;
        MelonLogger.Msg("[PartUpdateHooks->FindBodyPartInDictionary] BodyPart not found in dictionary.");
        return false;
    }
    
    private static IEnumerator SendPartUpdate(ModCar car, int carLoaderID, int key, int? index, ModPartType partType)
    {
        yield return new WaitForEndOfFrame();
        
        PartScript part;
        switch (partType)
        {
            case ModPartType.engine:
                part = car.partInfo.EnginePartsReferences[key];
                break;
            case ModPartType.suspension:
                part = car.partInfo.SuspensionPartsReferences[key][index.Value];
                break;
            case ModPartType.other:
                part = car.partInfo.OtherPartsReferences[key][index.Value];
                break;
            case ModPartType.driveshaft:
                part = car.partInfo.DriveshaftPartsReferences[key];
                break;
            default:
                yield break;
        }
        
        yield return new WaitForEndOfFrame();
        
        if(index.HasValue)
            ClientSend.PartScriptPacket(new ModPartScript(part, key, index.Value, partType), carLoaderID);
        else
            ClientSend.PartScriptPacket(new ModPartScript(part, key, -1, partType), carLoaderID);
    }
    public static IEnumerator SendBodyPart(CarPart part, int key, int carLoaderID)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        ClientSend.BodyPartPacket(new ModCarPart(part, key, carLoaderID), carLoaderID);
    }
}