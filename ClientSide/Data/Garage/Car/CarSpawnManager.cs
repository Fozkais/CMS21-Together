using System.Collections;
using System.Collections.Generic;
using CMS;
using CMS.PartModules;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class CarSpawnManager
{
	public static List<(int, string)> tempCarList = new();

	public static IEnumerator LoadCar(NewCarData carData, int carLoaderID, int placeNo)
	{
		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;

		var car = new ModCar(carLoaderID, carData.carToLoad, carData.configVersion, placeNo, carData.customerCar);
		ClientSend.LoadCarPacket(new ModNewCarData(carData, placeNo), carLoaderID);

		yield return new WaitForEndOfFrame();

		ClientData.Instance.loadedCars.Add(carLoaderID, car);
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
	}

	public static IEnumerator LoadJobCar(string name, int carLoaderID, CarLoader carLoader)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;

		yield return YieldInstructions.WaitForEndOfFrame;

		while (!carLoader.IsCarLoaded()) yield return YieldInstructions.WaitForEndOfFrame;

		yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;


		var car = new ModCar(carLoaderID, name, carLoader.ConfigVersion, carLoader.placeNo, carLoader.customerCar);
		ClientSend.LoadJobCarPacket(car);

		yield return new WaitForEndOfFrame();

		ClientData.Instance.loadedCars.Add(carLoaderID, car);
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
	}

	public static IEnumerator LoadCarFromServer(ModNewCarData data, int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		var carLoader = GameData.Instance.carLoaders[carLoaderID];
		var carData = data.ToGame();

		if (!ScreenFader.m_instance.IsRunning() || !ScreenFader.m_instance.IsFadedIn())
			ScreenFader.m_instance.ShortFadeIn();

		yield return YieldInstructions.WaitForEndOfFrame;

		CarSpawnHooks.listenToSimpleLoad = false;
		carLoader.ConfigVersion = carData.configVersion;
		carLoader.placeNo = data.carPosition;
		carLoader.StartCoroutine(carLoader.LoadCar(carData.carToLoad));
		while (!carLoader.IsCarLoaded()) yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;

		carLoader.WheelsData.Wheels[0].ET = carData.tiresET[0];
		carLoader.WheelsData.Wheels[1].ET = carData.tiresET[1];
		carLoader.WheelsData.Wheels[2].ET = carData.tiresET[2];
		carLoader.WheelsData.Wheels[3].ET = carData.tiresET[3];

		LoadCarConfig(carLoader, carData);
		yield return YieldInstructions.WaitForEndOfFrame;
		LoadBonusParts(carLoader, carData);
		yield return YieldInstructions.WaitForEndOfFrame;
		LoadBodyParts(carLoader, carData);
		yield return YieldInstructions.WaitForEndOfFrame;
		LoadCarParts(carLoader, carData);
		yield return YieldInstructions.WaitForEndOfFrame;
		LoadCarAdditionalInfo(carLoader, carData);

		yield return YieldInstructions.WaitForEndOfFrame;
		if (ScreenFader.m_instance.IsRunning() || ScreenFader.m_instance.IsFadedIn())
			ScreenFader.m_instance.ShortFadeOut();

		var car = new ModCar(carLoaderID, data.carToLoad, data.configVersion);
		ClientData.Instance.loadedCars.Add(carLoaderID, car);
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));

		MelonLogger.Msg($"[CarManager->LoadCarFromServer] Loading {data.carToLoad} from server...");
	}

	private static void LoadCarConfig(CarLoader carLoader, NewCarData carData)
	{
		carLoader.color = carData.GetColor();
		carLoader.factoryColor = carData.GetFactoryColor();
		carLoader.factoryPaintType = carData.GetFactoryPaintType();
		carLoader.paintData = carData.PaintData;
		carLoader.IsCustomPaintType = carData.HasCustomPaintType;
		/*carLoader.LicensePlatesData = new LicensePlatesData(carData.LicensePlatesData); make crash
		carLoader.ChangeLicencePlateTexture(carLoader.GetCarPart("license_plate_front"), carLoader.LicensePlatesData.LicensePlateFrontTex);
		carLoader.ChangeLicencePlateTexture(carLoader.GetCarPart("license_plate_rear"), carLoader.LicensePlatesData.LicensePlateRearTex);
		carLoader.SetLicensePlateNumber();*/
		carLoader.customerCar = carData.customerCar;
		carLoader.orderConnection = carData.orderConnection;
		carLoader.FluidsData.Copy(carData.FluidsData);
		carLoader.HeadlampLeftAlignment = new HeadlampAlignment(carData.HeadlampLeftAlignmentData);
		carLoader.HeadlampRightAlignment = new HeadlampAlignment(carData.HeadlampRightAlignmentData);
		carLoader.WheelsAlignment = new WheelsAlignment(carData.WheelsAlignment);
		carLoader.CarInfoData = carData.CarInfoData;
	}

	private static void LoadBonusParts(CarLoader carLoader, NewCarData carData)
	{
		int[] idFromConfig = carData.BonusPartsData.IdFromConfig;
		string[] iDs = carData.BonusPartsData.IDs;
		var bonusPartsData = carData.BonusPartsData;
		if (idFromConfig == null || idFromConfig.Length == 0)
		{
			if (iDs != null)
				for (var j = 0; j < carLoader.bonusParts.Count && j < iDs.Length && j < carLoader.bonusParts.Count; j++)
				{
					var bonusPart = carLoader.bonusParts._items[j];
					var text = iDs[j];
					if (!(bonusPart.ID == text) && bonusPartsData.GetIsPainted(j, out var isPainted) && bonusPartsData.GetColor(j, out var customColor) && bonusPartsData.GetPaintType(j, out var paintType) && bonusPartsData.GetPaintData(j, out var paintData))
					{
						if (bonusPart.Change(text))
						{
							bonusPart.TakeOn(true);
							bonusPart.Paint(isPainted, customColor, paintData, paintType);
						}

						if (bonusPart.Change(text)) bonusPart.TakeOn(true);
					}
				}
		}
		else if (iDs != null)
		{
			for (var k = 0; k < bonusPartsData.IDs.Length; k++)
			for (var l = 0; l < carLoader.bonusParts.Count; l++)
			{
				var bonusPart2 = carLoader.bonusParts._items[l];
				if (bonusPart2.IdFromConfig != bonusPartsData.IdFromConfig[k]) continue;
				var text2 = bonusPartsData.IDs[k];
				if (!(bonusPart2.ID == text2) && bonusPartsData.GetIsPainted(k, out var isPainted2) && bonusPartsData.GetColor(k, out var customColor2) && bonusPartsData.GetPaintType(k, out var paintType2) && bonusPartsData.GetPaintData(k, out var paintData2))
				{
					if (bonusPart2.Change(text2))
					{
						bonusPart2.TakeOn(true);
						bonusPart2.Paint(isPainted2, customColor2, paintData2, paintType2);
					}

					if (bonusPart2.Change(text2)) bonusPart2.TakeOn(true);
				}
			}
		}
	}

	private static void LoadBodyParts(CarLoader carLoader, NewCarData carData)
	{
		var flag = true;
		for (var i = 0; i < carData.BodyPartsData.Count; i++)
			if (string.IsNullOrEmpty(carData.BodyPartsData._items[i].Id))
			{
				flag = false;
				break;
			}

		for (var m = 0; m < carLoader.carParts.Count; m++)
		{
			var carPart = carLoader.carParts._items[m];
			if (carPart == null || carPart.handle == null)
				continue;
			if (carData.BodyPartsData.Count <= m)
				break;

			var carBodyPartData = default(BodyPartData);
			if (flag)
			{
				var flag2 = false;
				for (var n = 0; n < carData.BodyPartsData.Count; n++)
				{
					var bodyPartData = carData.BodyPartsData._items[n];
					if (!(bodyPartData.Id != carPart.name))
					{
						flag2 = true;
						carBodyPartData = bodyPartData;
					}
				}

				if (!flag2) continue;
			}
			else
			{
				carBodyPartData = carData.BodyPartsData._items[m];
			}

			carPart.Clone(carBodyPartData);
			if (carPart.Switched)
				carLoader.SwitchCarPart(carPart, true, true);

			var instance = GameInventory.Instance;

			if (!string.IsNullOrEmpty(carBodyPartData.TunedID))
			{
				var id1 = carLoader.carToLoad + "-" + carBodyPartData.TunedID;
				var id2 = carLoader.carToLoad + "-" + carPart.handle.name;
				if (instance.IsTuning(id1, id2, true))
				{
					carLoader.TunePart(carPart.handle.name, carBodyPartData.TunedID);
					if (carPart.handle.GetComponent<InteractiveObject>()) carPart.handle.GetComponent<InteractiveObject>().SetID(carBodyPartData.TunedID);
				}
			}

			carLoader.SetCarColor(carPart, carPart.Color);
			if (carPart.IsTinted)
				PaintHelper.SetWindowProperties(carPart.handle, carPart.TintColor);
			if (!string.IsNullOrEmpty(carPart.Livery))
				carLoader.SetCarLivery(carPart, carBodyPartData.Livery, carBodyPartData.LiveryStrength);
			if (carPart.OutsideRustEnabled)
				carLoader.EnableRustOutside(carPart, true);
			if (carPart.Unmounted)
				carLoader.TakeOffCarPartFromSave(carPart.handle.name);
			carLoader.EnableDust(carPart, carPart.Dust);
			carLoader.SetWashFactor(carPart, carPart.WashFactor);
			carLoader.SetDent(carPart, carPart.Dent);
		}

		carLoader.UpdateCarBodyParts();
	}

	private static void LoadCarParts(CarLoader carLoader, NewCarData carData)
	{
		var transform = carLoader.root.transform;
		for (var i = 0; i < carData.PartData.Count; i++)
		{
			var partData = carData.PartData._items[i];
			var val = transform.Find(partData.Path);

			var component2 = val.GetComponent<PartScript>();
			if (component2) component2.Clone(partData);
		}

		for (var j = 0; j < carData.PartData.Count; j++)
			if (carData.PartData._items[j].MountObjectData != null)
			{
				var mountObjectData = carData.PartData._items[j].MountObjectData;
				if (mountObjectData.Condition == null || mountObjectData.IsStuck == null) continue;
				var val2 = carLoader.transform.Find(mountObjectData.ParentPath);
				if (val2 == null) continue;

				var component3 = val2.GetComponent<PartScript>();
				if (component3 && !component3.IsUnmounted) component3.SetMountObjectData(mountObjectData);
			}
	}

	private static void LoadCarAdditionalInfo(CarLoader carLoader, NewCarData carData)
	{
		carLoader.SetWheelSize(carData.wheelsWidth[0], carData.rimsSize[0], carData.tiresSize[0], WheelType.FrontLeft);
		carLoader.SetWheelSize(carData.wheelsWidth[1], carData.rimsSize[1], carData.tiresSize[1], WheelType.FrontRight);
		carLoader.SetWheelSize(carData.wheelsWidth[2], carData.rimsSize[2], carData.tiresSize[2], WheelType.RearLeft);
		carLoader.SetWheelSize(carData.wheelsWidth[3], carData.rimsSize[3], carData.tiresSize[3], WheelType.RearRight);
		carLoader.UpdateWheelMeshCollider(WheelType.FrontLeft);
		carLoader.UpdateWheelMeshCollider(WheelType.FrontRight);
		carLoader.UpdateWheelMeshCollider(WheelType.RearLeft);
		carLoader.UpdateWheelMeshCollider(WheelType.RearRight);
		carLoader.UpdateET();
		if (CarLoaderPlaces.Get()) carLoader.ChangePosition();
		carLoader.PlaceAtPosition(false);
		carLoader.SetAdditionalCarRot(false, carData.AdditionalCarRot);
		carLoader.SetupCarSupport();
		carLoader.EngineData = new EngineData(carData.EngineData);
		carLoader.EngineData.ChangeOnDefaultIfZero(carLoader);
		var componentInChildren = carLoader.root.GetComponentInChildren<GearboxHandle>();
		if (componentInChildren)
		{
			componentInChildren.gearRatio = carData.gearRatio;
			componentInChildren.finalDriveRatio = carData.finalDriveRatio;
		}

		var componentInChildren2 = carLoader.root.GetComponentInChildren<EcuModule>();
		if (componentInChildren2 != null)
		{
			var carDataEcuData = carData.ecuData;
			componentInChildren2.CopyDataFrom(ref carDataEcuData);
		}

		carLoader.MeasuredDragIndex = carData.measuredDragIndex;
		var toolsMoveManager = ToolsMoveManager.Get();
		if (toolsMoveManager)
		{
			var place = (CarPlace)carLoader.placeNo;
			if (carData.TooolsData.WelderIsConnected) toolsMoveManager.MoveTo(IOSpecialType.Welder, place, false);
			if (carData.TooolsData.InteriorDetailingToolkitIsConnected) toolsMoveManager.MoveTo(IOSpecialType.InteriorDetailingToolkit, place, false);
			if (carData.TooolsData.OilbinIsConnected && carLoader.e_engine_h != null && carLoader.e_engine_h.name != "#Dummy") toolsMoveManager.MoveTo(IOSpecialType.Oilbin, place, false);
			if (carData.TooolsData.EngineCraneIsConnected) toolsMoveManager.MoveTo(IOSpecialType.EngineCrane, place, false);
			if (carData.TooolsData.HeadlampAlignmentSystemIsConnected) toolsMoveManager.MoveTo(IOSpecialType.HeadlampAlignmentSystem, place, false);
			if (carData.TooolsData.WindowTintingToolkitIsConnected) toolsMoveManager.MoveTo(IOSpecialType.WindowTint, place, false);
		}
	}

	public static void Reset()
	{
		tempCarList.Clear();
	}
}