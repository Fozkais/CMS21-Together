using System.Collections;
using CMS;
using CMS.Difficulty;
using CMS.Garage.Customization;
using CMS.Managers;
using CMS.UI;
using CMS.UI.Windows;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Data;

[HarmonyPatch]
public static class LoaderAddition
{
	[HarmonyPrefix]
	[HarmonyPatch(typeof(GarageLoader), nameof(GarageLoader.Start))]
	public static bool GarageStartOverride(GarageLoader __instance)
	{
		MelonCoroutines.Start(CustomPrepareGame(__instance));
		return false;
	}

	private static IEnumerator CustomPrepareGame(GarageLoader __instance)
	{
		if (__instance.loadOnStart)
		{
			ScreenFader.Get().SetBlack();
		}
		yield return __instance.StartCoroutine(Singleton<GameManager>.Instance.GameDataManager.Load(false));
		GlobalData.TrackInteriorVolumeMod = 1f;
		while (!Singleton<GameManager>.Instance.Localization.IsInitialized)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		Singleton<GameManager>.Instance.Localization.Localize();
		Singleton<GameManager>.Instance.PlatformManager.SetPresence("console_richtxt_garage");
		__instance.carLoader = Singleton<GameManager>.Instance.CarLoadersInScene;
		Singleton<GameManager>.Instance.ProfileManager.Load();
		CarLoaderPlaces.Get().PrepareWithoutLoad();
		NotificationCenter.CanMount = true;
		NotificationCenter.CanUnmount = true;
		if (Singleton<GameManager>.Instance.Radio)
		{
			yield return __instance.StartCoroutine(Singleton<GameManager>.Instance.Radio.Init());
		}
		UIManager.Get().RefreshAllStats();
		if (__instance.loadOnStart)
		{
			MelonCoroutines.Start(CustomLoad(__instance));
		}
		else
		{
			__instance.isReady = true;
		}
		while (!__instance.isReady)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		if (!GameMode.Get().CompareWithCurrentMode(gameMode.UI))
		{
			GameMode.Get().SetCurrentMode(gameMode.Garage);
		}
		SceneLoader.BlockProgress = false;
		NotificationCenter.IsGameReady = true;
		yield break;
	}

	private static IEnumerator CustomLoad(GarageLoader __instance)
	{ 
		MelonLogger.Msg("Run Custom Load method !!");
		__instance.isReady = false;
		NotificationCenter.IsGameReady = false;
		ScreenFader screenFader = ScreenFader.Get();
		screenFader.SetBlack();
		WindowManager.Instance.DisableAllWindowsOpening();
		CarPlaceManager.ClearAll();
		if (NotificationCenter.BenchmarkActive || BenchmarkManager.Get().BenchmarkActive)
		{
			__instance.StartCoroutine(BenchmarkManager.Get().StartBenchmark());
			yield break;
		}
		global::UnityEngine.Object.Destroy(__instance.GetComponent<BenchmarkManager>());
		Camera mainCamera = Camera.main;
		while (!mainCamera.GetComponent<FPSCamera>().IsReady)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		GlobalData.Load();
		DifficultyManager difficultyManager = Singleton<GameManager>.Instance.DifficultyManager;
		difficultyManager.ActivateDifficultyLevel();
		string selectedProfileName = Singleton<GameManager>.Instance.ProfileManager.GetSelectedProfileName();
		DevSettings.DevMode = selectedProfileName.Equals("VoizePrice");
		if (Singleton<GameManager>.Instance.Radio != null)
		{
			Singleton<GameManager>.Instance.Radio.LoadDataFromSave();
		}
		__instance.LoadMachines();
		Singleton<GameManager>.Instance.Inventory.Load();
		Singleton<GameManager>.Instance.Warehouse.Load();
		TempInventory tempInventory = Singleton<GameManager>.Instance.TempInventory;
		if (tempInventory.GetItemsCount() > 0)
		{
			List<BaseItem> listOfItems = tempInventory.GetListOfItems();
			Singleton<GameManager>.Instance.Inventory.Add(listOfItems);
			tempInventory.ClearListOfItems();
		}
		CarLoaderPlaces.Get().Load();
		__instance.garageLevel.PrepareGarage();
		while (!__instance.garageLevel.IsReady)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		GarageLookManager garageLookManager = __instance.garageLevel.SetupGarageLookManager();
		garageLookManager.Init();
		yield return __instance.StartCoroutine(garageLookManager.Load());
		while (garageLookManager.LoadingSaveInProcess)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		TexturePackManager texturePackManager = __instance.garageLevel.SetupTexturePackManager();
		yield return __instance.StartCoroutine(texturePackManager.Initialize());
		texturePackManager.Load();
		DLCErrorWindow dlcErrorWindow = WindowManager.Instance.GetWindowByID<DLCErrorWindow>(WindowID.DLCError);
		dlcErrorWindow.Clear();
		int carLoaderCount = __instance.carLoader.Count;
		GameSettings.DoNotClearPartsIDCache = true;
		GameSettings.DoNotUnloadAssets = true;
		int num;
		for (int i = 0; i < carLoaderCount; i = num + 1)
		{
			CarLoader car = __instance.carLoader[i];
			car.DeleteCar();
			car.LoadCarFromFile();
			while (!car.IsLoadedFromFile())
			{
				yield return YieldInstructions.WaitForEndOfFrame;
			}
			if (!(car.GetSaveName() != GlobalData.SelectedCarLoader) && GlobalData.NewMileage != 0)
			{
				CarLoader carLoader = car;
				carLoader.CarInfoData = carLoader.CarInfoData with { Mileage = carLoader.CarInfoData.Mileage + GlobalData.NewMileage };
				GlobalData.NewMileage = 0;
				car = null;
			}
			num = i;
		}
		GameSettings.DoNotUnloadAssets = false;
		GameSettings.DoNotClearPartsIDCache = false;
		Helper.ClearCacheForIDs();
		ProfileData currentProfileData = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData;
		PlayerData profileData = currentProfileData.PlayerData;
		CharacterController characterController = global::UnityEngine.Object.FindObjectOfType<CharacterController>();
		if (!profileData.IsDefault())
		{
			characterController.transform.position = profileData.GetPosition();
			mainCamera.GetComponent<FPSCamera>().SetSavedRotation(profileData.GetRotation());
		}
		if (__instance.carLifter[0].HaveToResetPlayer() || __instance.carLifter[1].HaveToResetPlayer())
		{
			characterController.GetComponent<FPSInputController>().ResetPosition();
		}
		GlobalData.SetUnlockedPositions(currentProfileData.unlockedPosition.position);
		Singleton<GameManager>.Instance.OrderGenerator.Load();
		ShopListWindow windowByID = WindowManager.Instance.GetWindowByID<ShopListWindow>(WindowID.ShopList);
		if (windowByID != null)
		{
			windowByID.Load();
		}
		DifficultyLevel difficultyLevel = difficultyManager.GetDifficultyLevel();
		if (difficultyLevel == DifficultyLevel.Easy || difficultyLevel == DifficultyLevel.Normal)
		{
			if (selectedProfileName.Equals("cms2021promo") && GlobalData.PlayerExp == 0)
			{
				GlobalData.AddPlayerExp(50000, true);
				GlobalData.AddPlayerMoney(500000);
			}
			if (selectedProfileName.Equals("cms2021stage1") && GlobalData.PlayerExp == 0)
			{
				GlobalData.AddPlayerExp(2000, true);
				GlobalData.AddPlayerMoney(4000);
			}
			if (selectedProfileName.Equals("cms2021stage2") && GlobalData.PlayerExp == 0)
			{
				GlobalData.AddPlayerExp(8000, true);
				GlobalData.AddPlayerMoney(50000);
			}
			if (selectedProfileName.Equals("cms2021stage3") && GlobalData.PlayerExp == 0)
			{
				GlobalData.AddPlayerExp(13000, true);
				GlobalData.AddPlayerMoney(150000);
			}
		}
		UIManager.Get().RefreshAllStats();
		/*NewCarLifterData[] carLiftersData = currentProfileData.carLiftersData;
		if (carLiftersData != null && carLiftersData.Length != 0)
		{
			int i = 0;
			while (i < __instance.carLifter.Length && i < carLiftersData.Length)
			{
				CarLifter cl = __instance.carLifter[i];
				if (cl.gameObject.activeSelf)
				{
					NewCarLifterData carLifterData = currentProfileData.carLiftersData[i];
					while (cl.isMoving)
					{
						yield return YieldInstructions.WaitForEndOfFrame;
					}
					CarLoader connectedCarLoader = cl.GetConnectedCarLoader();
					if (carLifterData.lifterData == 0 && connectedCarLoader)
					{
						MissingWheelTypes missingWheelTypes = connectedCarLoader.GetMissingWheelTypes();
						if (missingWheelTypes.HasFlagFast(MissingWheelTypes.FrontLeft)
						    || missingWheelTypes.HasFlagFast(MissingWheelTypes.FrontRight)
						    || missingWheelTypes.HasFlagFast(MissingWheelTypes.RearLeft)
						    || missingWheelTypes.HasFlagFast(MissingWheelTypes.RearRight))
						{
							cl.InstantSet(1, true);
							goto IL_06AE;
						}
					}
					cl.InstantSet((__instance.carLifter[i].GetConnectedCarLoader() == null) ? 0 : carLifterData.lifterData, true);
					cl = null;
					carLifterData = null;
				}
				IL_06AE:
				num = i;
				i = num + 1;
			}
		}*/
		
		SceneLoader.BlockProgress = false; // needed to end loading
		NotificationCenter.IsGameReady = true; // needed to end loading
		CameraManager.Get().ChangeCamera(CameraState.FPS);
		yield return new WaitForSeconds(2f);
		screenFader.FadeTo(2f, 1f, 0f, false, true);
		bool canOpenPieMenu = true;
		if (GlobalData.TestToShow == "ExamineReport")
		{
			WindowManager.Instance.EnableWindowOpening(WindowID.ExamineReport);
			GameScript.Get().SetCurrentExamineType(ToolType.TestDrive);
			WindowManager.Instance.Show(WindowID.ExamineReport, false);
			canOpenPieMenu = false;
		}
		if (dlcErrorWindow.ShouldShow())
		{
			WindowManager.Instance.EnableWindowOpening(WindowID.DLCError);
			dlcErrorWindow.ModeToSetAfterClosing = gameMode.Garage;
			dlcErrorWindow.EnablePieMenuAfterClosing = true;
			WindowManager.Instance.ShowAfterWindowClose(WindowID.DLCError, WindowID.ExamineReport);
			canOpenPieMenu = false;
		}
		if (profileData.IsDefault())
		{
			WindowManager.Instance.EnableWindowOpening(WindowID.Intro);
			WindowManager.Instance.Show(WindowID.Intro, false);
			canOpenPieMenu = false;
		}
		__instance.isReady = true; // needed to end loading
		yield return YieldInstructions.WaitForEndOfFrame;
		if (canOpenPieMenu)
		{
			GlobalData.CanOpenPieMenu = true;
			GameScript.Get().CanOpenPieMenu = true;
		}
		while (screenFader.IsRunning())
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
		WindowManager.Instance.EnableAllWindowsOpening();
		Singleton<GameManager>.Instance.ProfileManager.BackupSave();
		
		MelonLogger.Msg("Run Custom Load method successfully !!");
		yield break;
	}
}