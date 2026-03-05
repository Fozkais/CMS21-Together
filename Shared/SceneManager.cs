using System.Collections.Generic;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.Shared;

[HarmonyPatch]
public static class SceneManager
{
	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SelectSceneToLoad),
		new []{typeof(string), typeof(SceneType), typeof(bool), typeof(bool)})]
	[HarmonyPrefix]
	public static void SelectSceneToLoadHook(string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
	{
		// Business Rule: Only process scene changes if client is connected
		if (!Client.Instance.isConnected) return;

		// Security Rule: Validate scene name is not null or empty
		if (string.IsNullOrEmpty(newSceneName))
		{
			MelonLogger.Warning("[SceneManager->SelectSceneToLoadHook] Scene name is null or empty.");
			return;
		}

		MelonLogger.Msg($"[SceneManager->SelectSceneToLoadHook] Scene change requested: {newSceneName}");
		GameLoadHook.Reset();
		if (newSceneName == "Menu")
		{
			// Business Rule: Disconnect and cleanup when returning to menu
			MelonLogger.Msg("[SceneManager->SelectSceneToLoadHook] Going to menu! Disconnecting...");
			
			// Security Rule: Stop outdoor operations before disconnecting
			StopOutdoorOperations();
			
			if (Server.Instance != null && Server.Instance.isRunning)
				MelonCoroutines.Start(Server.Instance.CloseServer());
			
			if (Client.Instance.isConnected)
				Client.Instance.Disconnect();
		}
		else if (newSceneName == "garage" || newSceneName == "Christmas" || newSceneName == "Easter" || newSceneName == "Halloween")
		{
			if (GameLoadHook.IsGameReady())
			{
				StopOutdoorOperations();
				MelonCoroutines.Start(GarageResync.ResyncGarage());
			}
		}
		else
		{
			if (ClientData.Instance != null && ClientData.Instance.loadedCars != null)
			{
				StopOutdoorOperations();
				
				foreach (ModCar loadedCar in ClientData.Instance.loadedCars.Values)
				{
					if (loadedCar != null)
					{
						loadedCar.needResync = true;
					}
				}
				MelonLogger.Msg($"[SceneManager->SelectSceneToLoadHook] Marked {ClientData.Instance.loadedCars.Count} cars for resync.");
			}
			else
			{
				MelonLogger.Warning("[SceneManager->SelectSceneToLoadHook] ClientData.Instance or loadedCars is null. Cannot mark cars for resync.");
			}
		}
	}
	
	private static void StopOutdoorOperations()
	{
		try
		{
			if (GameData.Instance != null)
			{
				CMS21Together.ClientSide.Data.Garage.Tools.CarWashLogic.Reset();
				CarPaintLogic.Reset();
				
				MelonLogger.Msg("[SceneManager->StopOutdoorOperations] Stopped outdoor operations (car wash, paint).");
			}
		}
		catch (System.Exception ex)
		{
			MelonLogger.Warning($"[SceneManager->StopOutdoorOperations] Error stopping outdoor operations: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates the current scene based on scene name.
	/// Resets GameData readiness when entering garage.
	/// </summary>
	/// <param name="scene">The name of the scene being loaded</param>
	/// <returns>The corresponding GameScene enum value</returns>
	public static GameScene UpdateScene(string scene)
	{
		// Security Rule: Validate scene name is not null or empty
		if (string.IsNullOrEmpty(scene))
		{
			MelonLogger.Warning("[SceneManager->UpdateScene] Scene name is null or empty. Returning unknown.");
			return GameScene.unknow;
		}

		MelonLogger.Msg($"[SceneManager->UpdateScene] Changed scene: {scene}!");
		
		// Business Rule: Map scene names to GameScene enum values
		if (scene == "Barn")
			return GameScene.barn;
		
		if (scene == "garage" || scene == "Christmas" || scene == "Easter" || scene == "Halloween")
		{
			return GameScene.garage;
		}
		
		if (scene == "Junkyard")
			return GameScene.junkyard;
		
		if (scene == "Auto_salon")
			return GameScene.auto_salon;
		
		if (scene == "Menu")
			return GameScene.menu;

		// Business Rule: Return unknown for unrecognized scene names
		MelonLogger.Warning($"[SceneManager->UpdateScene] Unknown scene name: {scene}. Returning unknown.");
		return GameScene.unknow;
	}

	public static GameScene CurrentScene(UserData user = null)
	{
		if (IsInBarn(user))
			return GameScene.barn;
		if (IsInGarage(user))
			return GameScene.garage;
		if (IsInJunkyard(user))
			return GameScene.junkyard;
		if (IsInDealer(user))
			return GameScene.auto_salon;
		if (IsInMenu(user))
			return GameScene.menu;

		return GameScene.unknow;
	}

	public static bool IsInMenu(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.menu;

		return ClientData.UserData.scene == GameScene.menu;
	}

	public static bool IsInGarage(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.garage;

		return ClientData.UserData.scene == GameScene.garage;
	}

	public static bool IsInJunkyard(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.junkyard;

		return ClientData.UserData.scene == GameScene.junkyard;
	}

	public static bool IsInDealer(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.auto_salon;

		return ClientData.UserData.scene == GameScene.auto_salon;
	}

	public static bool IsInBarn(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.barn;

		return ClientData.UserData.scene == GameScene.barn;
	}
}