using System;
using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = CMS21Together.Shared.SceneManager;

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
		// Originally this branch only reset the flag but fell through; meaning an unlisten
		// request still queued a LoadCar coroutine. Return to honor the one-shot suppression.
		if (!Client.Instance.isConnected || !listenToLoad)
		{
			listenToLoad = true;
			return;
		}

		int indexFromCarLoaderName = Helper.GetIndexFromCarLoaderName(file);
		NewCarData newCarData = Singleton<GameManager>.Instance.GameDataManager.LoadCar(indexFromCarLoaderName, false);
		MainMod.StartCoroutine(__instance.LoadCarFromFile(newCarData));
		MelonCoroutines.Start(LoadCarFromFile(newCarData, __instance));
	}

	private static IEnumerator LoadCarFromFile(NewCarData carDataCheck, CarLoader __instance)
	{
		yield return new WaitForEndOfFrame();
		if (string.IsNullOrEmpty(__instance.carToLoad)) yield break;
		if (!SceneManager.IsInGarage()) yield break;

		var carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0)
		{
			MelonLogger.Warning($"[CarSpawnHooks->LoadCarFromFile] Bad CarLoader name '{__instance.gameObject.name}', skipping.");
			yield break;
		}
		//MelonLogger.Msg($"[CarSpawnHooks->LoadCarFromFileHook] Triggered:{__instance.carToLoad} , {carLoaderID}");
		MelonCoroutines.Start(CarSpawnManager.LoadCar(carDataCheck, carLoaderID, __instance.placeNo));
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
		if (!SceneManager.IsInGarage()) return;

		//MelonLogger.Msg($"[CarSpawnHooks->LoadJobCar] Triggered:{name}");

		var carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0)
		{
			MelonLogger.Warning($"[CarSpawnHooks->LoadCarHook] Bad CarLoader name '{__instance.gameObject.name}', skipping.");
			return;
		}
		MelonCoroutines.Start(CarSpawnManager.LoadJobCar(name, carLoaderID, __instance));
	}

	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.DeleteCar), new Type[] { })]
	[HarmonyPrefix]
	public static void DeleteCarHook(CarLoader __instance)
	{
		if (MainMod.isClosing || Client.Instance == null) return;
		if (!Client.Instance.isConnected || !listenToDelete)
		{
			listenToDelete = true;
			return;
		}

		if (!NotificationCenter.IsGameReady) return;
		if (__instance == null || string.IsNullOrEmpty(__instance.carToLoad)) return;

		if (SceneManager.CurrentScene() != GameScene.garage) return;

		var carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0) return;
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			if (!car.needResync)
			{
				MelonLogger.Msg("Sent Delete car packet.");
				ClientSend.DeleteCarPacket(carLoaderID);
				ClientData.Instance.loadedCars.Remove(carLoaderID);
			}
		}
	}
}