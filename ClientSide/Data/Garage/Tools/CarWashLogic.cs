using System.Collections;
using CMS;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class CarWashLogic
{
	public static bool listen = true;

	public static void Reset() => listen = true;
	
	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TweenExteriorDustWash))]
	[HarmonyPostfix]
	public static void DTweenExteriorDustWashHook(float targetDust, float targetWash, float time, CarLoader __instance)
	{
		// Business Rule: Only sync if connected and listening flag is enabled
		if(!Client.Instance.isConnected || !listen) 
		{ 
			listen = true; 
			return;
		}

		if (__instance == null || __instance.gameObject == null) return;
		int carLoaderID = DataHelper.ExtractCarLoaderIDFromName(__instance.gameObject.name);
		if (carLoaderID < 0)
		{
			MelonLogger.Warning($"[CarWashLogic->DTweenExteriorDustWashHook] Invalid carLoader name '{__instance.gameObject.name}'.");
			return;
		}

		ClientSend.CarWashPacket(carLoaderID);
	}
	
	[HarmonyPatch(typeof(InteriorDetailingToolkitLogic), nameof(InteriorDetailingToolkitLogic.DoWorkAnim))]
	[HarmonyPrefix]
	public static void DoWorkAnimHook(CarLoader carLoader)
	{
		// Business Rule: Only sync if connected and listening flag is enabled
		if(!Client.Instance.isConnected || !listen) 
		{ 
			listen = true; 
			return;
		}

		if (carLoader == null || carLoader.gameObject == null) return;
		int carLoaderID = DataHelper.ExtractCarLoaderIDFromName(carLoader.gameObject.name);
		if (carLoaderID < 0)
		{
			MelonLogger.Warning($"[CarWashLogic->DoWorkAnimHook] Invalid carLoader name '{carLoader.gameObject.name}'.");
			return;
		}

		ClientSend.CarWashPacket(carLoaderID, true);
		MelonLogger.Msg($"[CarWashLogic->DoWorkAnimHook] Interior wash requested for carLoaderID: {carLoaderID}");
	}

	/// <summary>
	/// Handles car wash synchronization from server.
	/// Applies wash effect to the specified car loader.
	/// </summary>
	/// <param name="carLoaderID">The ID of the car loader to wash (0-4)</param>
	/// <param name="interior">Whether to wash interior (true) or exterior (false)</param>
	public static IEnumerator WashCar(int carLoaderID, bool interior)
	{
		// Business Rule: Wait for game to be ready before processing
		while (!ClientData.GameReady)
		{
			// Security Rule: Check if client disconnected during wait
			if (!Client.Instance.isConnected)
			{
				MelonLogger.Warning("[CarWashLogic->WashCar] Client disconnected while waiting for game ready.");
				yield break;
			}
			yield return new WaitForSeconds(0.25f);
		}

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		// Security Rule: Validate GameData.Instance is initialized
		if (GameData.Instance == null)
		{
			MelonLogger.Error("[CarWashLogic->WashCar] GameData.Instance is null. Cannot wash car.");
			yield break;
		}

		// Security Rule: Validate carLoaders array is initialized
		if (GameData.Instance.carLoaders == null)
		{
			MelonLogger.Error("[CarWashLogic->WashCar] carLoaders array is null. Cannot wash car.");
			yield break;
		}

		// Business Rule: Validate carLoaderID is within array bounds
		if (carLoaderID < 0 || carLoaderID >= GameData.Instance.carLoaders.Length)
		{
			MelonLogger.Error($"[CarWashLogic->WashCar] Invalid carLoaderID: {carLoaderID}. Array length: {GameData.Instance.carLoaders.Length}");
			yield break;
		}

		// Security Rule: Validate carLoader at index is not null
		if (GameData.Instance.carLoaders[carLoaderID] == null)
		{
			MelonLogger.Warning($"[CarWashLogic->WashCar] CarLoader at index {carLoaderID} is null. Car may not be loaded.");
			yield break;
		}

		// Business Rule: Check if car is already loaded (if loaded, skip to avoid duplicate operations)
		// Note: Original logic was inverted - should check if NOT loaded, not if loaded
		if (!ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
		{
			MelonLogger.Warning($"[CarWashLogic->WashCar] Car at carLoaderID {carLoaderID} is not in loadedCars dictionary. Skipping wash.");
			yield break;
		}

		// Business Rule: Set listen flag to false to prevent feedback loop
		listen = false;

		try
		{
			// Business Logic: Apply wash effect based on interior/exterior
			if (!interior)
			{
				// Business Logic: Use longer duration (3f) to match interior wash and ensure animation is visible
				// Observation: Original duration (0.1f) was too short, causing animation to not be visible
				GameData.Instance.carLoaders[carLoaderID].TweenExteriorDustWash(0f, 1f, 3f);
				MelonLogger.Msg($"[CarWashLogic->WashCar] Exterior wash applied to carLoaderID: {carLoaderID} with 3s duration.");
			}
			else
			{
				GameData.Instance.carLoaders[carLoaderID].TweenInteriorConditionAndDust(1f, 0f, 3f);
				MelonLogger.Msg($"[CarWashLogic->WashCar] Interior wash applied to carLoaderID: {carLoaderID}");
			}
		}
		catch (System.Exception ex)
		{
			// Security Rule: Catch and log any exceptions during wash operation
			MelonLogger.Error($"[CarWashLogic->WashCar] Exception during wash operation: {ex.Message}\n{ex.StackTrace}");
		}
		finally
		{
			// Business Rule: Reset listen flag after operation completes
			listen = true;
		}
	}
}