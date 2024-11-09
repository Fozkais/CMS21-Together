using System;
using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCarFromFile), typeof(string))]
	[HarmonyPrefix]
	public static void LoadCarFromFileHook(string file, CarLoader __instance)
	{
		if (!Client.Instance.isConnected || !listenToLoad)
		{
			listenToLoad = true;
			return;
		}

		MelonCoroutines.Start(LoadCarFromFile(file, __instance)); 
	}

	private static IEnumerator LoadCarFromFile(string file, CarLoader __instance)
	{
		yield return new WaitForEndOfFrame();
		MelonLogger.Msg($"[CarSpawnHooks->LoadCarFromFileHook] Triggered:{__instance.carToLoad}");
		if (string.IsNullOrEmpty(__instance.carToLoad)) yield break;

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		
		int indexFromCarLoaderName = Helper.GetIndexFromCarLoaderName(file);
		NewCarData data =  Singleton<GameManager>.Instance.GameDataManager.LoadCar(indexFromCarLoaderName, false);
		
		MelonCoroutines.Start(CarSpawnManager.LoadCar(data, carLoaderID, __instance.placeNo));
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.LoadCar))]
	[HarmonyPostfix]
	public static void LoadCarHook(string name, CarLoader __instance)
	{
		if (!Client.Instance.isConnected || !listenToSimpleLoad)
		{
			listenToSimpleLoad = true;
			return;
		}
		
		if (string.IsNullOrEmpty(name)) return;

		MelonLogger.Msg($"[CarSpawnHooks->LoadJobCar] Triggered:{name}");
		//if (!Shared.SceneManager.IsInGarage()) return;

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		MelonCoroutines.Start(CarSpawnManager.LoadJobCar(name, carLoaderID, __instance));
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
	[HarmonyPostfix]
	public static void DeleteCarHook(CarLoader __instance)
	{
		if (!Client.Instance.isConnected || !listenToDelete)
		{
			listenToDelete = true;
			return;
		}


		MelonLogger.Msg("[CarSpawnHooks->DeleteCarHook] Triggered.");
		if (string.IsNullOrEmpty(__instance.carToLoad) || SceneManager.GetActiveScene().name != "garage") return;

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			ClientSend.DeleteCarPacket(carLoaderID);
			ClientData.Instance.loadedCars.Remove(carLoaderID);
		}
	}
}