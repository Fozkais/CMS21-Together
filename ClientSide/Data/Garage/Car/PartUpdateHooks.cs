using System.Collections;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class PartUpdateHooks
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(FluidsData), nameof(FluidsData.SetLevel))]
	[HarmonyPostfix]
	public static void SetLevelAltHook(float level, CarFluidType carFluidType, int id, FluidsData __instance)
	{
		if (!Client.Instance.isConnected || !listen) {listen = true; return;}

		if (carFluidType == CarFluidType.EngineOil && __instance.Oil.CarFluid != null)
		{
			int carLoaderID = __instance.Oil.CarFluid.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0' - 1;
			ClientSend.CarFluid(carLoaderID, new ModFluidData(__instance.Oil));
		}

	}
	
	[HarmonyPatch(typeof(FluidData), nameof(FluidData.SetLevel))]
	[HarmonyPostfix]
	public static void SetLevelHook(float level, FluidData __instance)
	{
		if (!Client.Instance.isConnected || !listen) {listen = true; return;}

		if (__instance != null && __instance.CarFluid != null)
		{
			int carLoaderID = __instance.CarFluid.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0' - 1;
			ClientSend.CarFluid(carLoaderID, new ModFluidData(__instance));
		}

	}

	[HarmonyPatch(typeof(PartScript), nameof(PartScript.DoMount))]
	[HarmonyPostfix]
	public static void DoMountHook(PartScript __instance)
	{
		if (!Client.Instance.isConnected) return;
		MelonCoroutines.Start(HandleDoMount(__instance));
	}

	private static bool AreBoltsMounted(PartScript partScript)
	{
		var allMounted = true;
		foreach (var bolt in partScript.MountObjects)
			if (bolt.unmounted)
			{
				allMounted = false;
				break;
			}

		return allMounted;
	}

	private static IEnumerator HandleDoMount(PartScript partScript)
	{
		if (!partScript.oneClickUnmount)
		{
			var counter = 0;
			while (!AreBoltsMounted(partScript) || counter <= 16)
			{
				yield return new WaitForSeconds(0.25f);
				counter++;
			}
		}

		MelonLogger.Msg("[PartUpdateHooks->DoMountHook] Triggered.");
		if (partScript.GetComponentInParent<CarLoaderOnCar>())
		{
			var carLoaderID = partScript.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0' - 1;
			var car = ClientData.Instance.loadedCars[carLoaderID];

			if (FindPartInDictionaries(car, partScript, out var partType, out var key, out var index))
				MelonCoroutines.Start(SendPartUpdate(car, carLoaderID, key, index, partType));
			else
				MelonLogger.Warning("[PartUpdateHooks->DoMountHook] PartScript not found in any dictionary.");
		}
		else
		{
			ModEngineStand stand;
			if (EngineStand.useAlt)
				stand = ClientData.Instance.engineStand2;
			else
				stand = ClientData.Instance.engineStand;
			foreach (var kvp in stand.partReferences)
			{
				if (kvp.Value == partScript)
				{
					MelonLogger.Msg($"Sending part:{partScript.id} , {kvp.Key}.");
					MelonCoroutines.Start(SendPartUpdate(null, EngineStand.useAlt ? -2 : -1, kvp.Key, null, ModPartType.engineStand));
					break;
				}
			}
			
		}
	}

	[HarmonyPatch(typeof(PartScript), nameof(PartScript.Hide))]
	[HarmonyPostfix]
	public static void HideHook(PartScript __instance) // best way i've found to detect when a partScript is unmounted
	{
		if (!Client.Instance.isConnected) return;
		
		if (__instance.GetComponentInParent<CarLoaderOnCar>())
		{
			var carLoaderID = __instance.GetComponentInParent<CarLoaderOnCar>().CarLoader.gameObject.name[10] - '0' - 1;
			var car = ClientData.Instance.loadedCars[carLoaderID];

			if (FindPartInDictionaries(car, __instance, out var partType, out var key, out var index))
				MelonCoroutines.Start(SendPartUpdate(car, carLoaderID, key, index, partType));
		}
		else // engine stand
		{
			foreach (var kvp in ClientData.Instance.engineStand.partReferences)
			{
				if (kvp.Value == __instance)
				{
					MelonCoroutines.Start(SendPartUpdate(null, -1, kvp.Key, null, ModPartType.engineStand));
					break;
				}
			}

		}
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TakeOffCarPart), typeof(string), typeof(bool))]
	[HarmonyPostfix]
	public static void TakeOffCarPartHook(string name, bool off, CarLoader __instance) // handle both Mount/Unmount with the boolean
	{
		if (!Client.Instance.isConnected) return;

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		var car = ClientData.Instance.loadedCars[carLoaderID];

		if (FindBodyPartInDictionary(car, name, out var key))
		{
			var part = car.CarPartInfo.BodyPartsReferences[key];
			MelonCoroutines.Start(SendBodyPart(part, key, carLoaderID));
		}
	}

	public static bool FindPartInDictionaries(ModCar car, PartScript partScript, out ModPartType partType, out int key, out int? index)
	{
		index = null;

		foreach (var kvp in car.CarPartInfo.OtherPartsReferences)
		{
			var listIndex = kvp.Value.FindIndex(part => part == partScript);
			if (listIndex >= 0)
			{
				partType = ModPartType.other;
				key = kvp.Key;
				index = listIndex;
				return true;
			}
		}

		foreach (var kvp in car.CarPartInfo.SuspensionPartsReferences)
		{
			var listIndex = kvp.Value.FindIndex(part => part == partScript);
			if (listIndex >= 0)
			{
				partType = ModPartType.suspension;
				key = kvp.Key;
				index = listIndex;
				return true;
			}
		}

		foreach (var kvp in car.CarPartInfo.EnginePartsReferences)
			if (kvp.Value == partScript)
			{
				partType = ModPartType.engine;
				key = kvp.Key;
				return true;
			}

		foreach (var kvp in car.CarPartInfo.DriveshaftPartsReferences)
			if (kvp.Value == partScript)
			{
				partType = ModPartType.driveshaft;
				key = kvp.Key;
				return true;
			}

		partType = default;
		key = 0;
		MelonLogger.Warning("[PartUpdateHooks->FindPartInDictionaries] PartScript not found in any dictionary.");
		return false;
	}

	public static bool FindBodyPartInDictionary(ModCar car, string carPartName, out int key)
	{
		foreach (var kvp in car.CarPartInfo.BodyPartsReferences)
			if (kvp.Value.name == carPartName)
			{
				key = kvp.Key;
				return true;
			}

		key = 0;
		MelonLogger.Warning("[PartUpdateHooks->FindBodyPartInDictionary] BodyPart not found in dictionary.");
		return false;
	}

	private static IEnumerator SendPartUpdate(ModCar car, int carLoaderID, int key, int? index, ModPartType partType)
	{
		yield return new WaitForEndOfFrame();

		PartScript part;
		switch (partType)
		{
			case ModPartType.engine:
				part = car.CarPartInfo.EnginePartsReferences[key];
				break;
			case ModPartType.engineStand:
				if (carLoaderID == -1)
					part = ClientData.Instance.engineStand.partReferences[key];
				else
					part = ClientData.Instance.engineStand2.partReferences[key];
				break;
			case ModPartType.suspension:
				part = car.CarPartInfo.SuspensionPartsReferences[key][index.Value];
				break;
			case ModPartType.other:
				part = car.CarPartInfo.OtherPartsReferences[key][index.Value];
				break;
			case ModPartType.driveshaft:
				part = car.CarPartInfo.DriveshaftPartsReferences[key];
				break;
			default:
				yield break;
		}

		yield return new WaitForEndOfFrame();

		if (index.HasValue && partType != ModPartType.engineStand)
			ClientSend.PartScriptPacket(new ModPartScript(part, key, index.Value, partType), carLoaderID);
		else if (partType != ModPartType.engineStand)
			ClientSend.PartScriptPacket(new ModPartScript(part, key, -1, partType), carLoaderID);
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