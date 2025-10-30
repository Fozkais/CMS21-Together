using System.Collections.Generic;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Garage;
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
		if (!Client.Instance.isConnected) return;

		if (newSceneName == "Menu")
		{
			MelonLogger.Msg("Going to menu! disconnect..");
			if (Server.Instance.isRunning)
				MelonCoroutines.Start(Server.Instance.CloseServer());
			if (Client.Instance.isConnected)
				Client.Instance.Disconnect();

			ClientData.GameReady = false;
		}
		else if (newSceneName == "garage" || newSceneName == "Christmas" || newSceneName == "Easter" || newSceneName == "Halloween")
		{
			if (ClientData.GameReady)
				MelonCoroutines.Start(GarageResync.ResyncGarage());
		}
		else
		{
			foreach (ModCar loadedCar in ClientData.Instance.loadedCars.Values)
			{
				loadedCar.needResync = true;
			}
		}
	}

	public static GameScene UpdateScene(string scene)
	{
		MelonLogger.Msg($"[SceneManager->UpdateScene] changed scene : {scene}!");
		
		if (scene == "Barn")
			return GameScene.barn;
		if (scene == "garage" || scene == "Christmas" || scene == "Easter" || scene == "Halloween")
		{
			GameData.isReady = false;
			return GameScene.garage;
		}
		if (scene == "Junkyard")
			return GameScene.junkyard;
		if (scene == "Auto_salon")
			return GameScene.auto_salon;
		if (scene == "Menu")
			return GameScene.menu;
		
		

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