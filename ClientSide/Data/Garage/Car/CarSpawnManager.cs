using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public static IEnumerator LoadCar(NewCarData carData, int carLoaderID, int placeNo)
	{
		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;

		var car = new ModCar(carLoaderID, carData.carToLoad, carData.configVersion, placeNo, carData.customerCar);
		ClientSend.LoadCarPacket(new ModNewCarData(carData, placeNo), carLoaderID);
		
		while (!GameData.Instance.carLoaders[carLoaderID].IsCarLoaded()) yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;
		
		if (!ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
		{
			ClientData.Instance.loadedCars.Add(carLoaderID, car);
			MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
		}
	}

	public static IEnumerator LoadJobCar(string name, int carLoaderID, CarLoader carLoader)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;
		yield return YieldInstructions.WaitForEndOfFrame;

		while (!carLoader.IsCarLoaded()) yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;


		var car = new ModCar(carLoaderID, name, carLoader.ConfigVersion, carLoader.placeNo, carLoader.customerCar);
		ClientSend.LoadJobCarPacket(car);

		yield return new WaitForEndOfFrame();
		if (!ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
		{
			ClientData.Instance.loadedCars.Add(carLoaderID, car);
			MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
		}
	}

	public static IEnumerator LoadCarFromServer(ModNewCarData data, int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		var carLoader = GameData.Instance.carLoaders[carLoaderID];
		carLoader.placeNo = data.carPosition;
		carLoader.ConfigVersion = data.configVersion;
		var carData = data.ToGame();

		yield return new WaitForEndOfFrame();
		
		CarSpawnHooks.listenToSimpleLoad = false;
		MainMod.StartCoroutine(carLoader.LoadCarFromFile(carData));
		var car = new ModCar(carLoaderID, data.carToLoad, data.configVersion);
		ClientData.Instance.loadedCars.Add(carLoaderID, car);
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
		
		/*CarSpawnHooks.listenToSimpleLoad = false;
		carLoader.ConfigVersion = carData.configVersion;
		carLoader.placeNo = data.carPosition;
		carLoader.StartCoroutine(carLoader.LoadCar(carData.carToLoad));
		while (!carLoader.done)
		{
			yield return YieldInstructions.WaitForEndOfFrame;
		}
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

		var car = new ModCar(carLoaderID, data.carToLoad, data.configVersion);
		ClientData.Instance.loadedCars.Add(carLoaderID, car);
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));*/

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
		string[] ids = carData.BonusPartsData.IDs;
		BonusPartsData bonusPartsData = carData.BonusPartsData;
		if (idFromConfig == null || idFromConfig.Length == 0)
		{
			if (ids != null)
			{
				for (int j = 0; j < carLoader.bonusParts.Count; j++)
				{
					if (j >= ids.Length || j >= carLoader.bonusParts.Count)
					{
						break;
					}
					BonusPart bonusPart = carLoader.bonusParts.ToArray()[j];
					string text = ids[j];
					bool flag2;
					CustomColor customColor;
					PaintType paintType;
					PaintData paintData;
					if (!(bonusPart.ID == text) && bonusPartsData.GetIsPainted(j, out flag2) && bonusPartsData.GetColor(j, out customColor) && bonusPartsData.GetPaintType(j, out paintType) && bonusPartsData.GetPaintData(j, out paintData))
					{
						if (bonusPart.Change(text, false))
						{
							bonusPart.TakeOn(true);
							bonusPart.Paint(flag2, customColor, paintData, paintType);
						}
						if (bonusPart.Change(text, false))
						{
							bonusPart.TakeOn(true);
						}
					}
				}
			}
		}
		else if (ids != null && idFromConfig != null)
		{
			for (int k = 0; k < bonusPartsData.IDs.Length; k++)
			{
				for (int l = 0; l < carLoader.bonusParts.Count; l++)
				{
					BonusPart bonusPart2 = carLoader.bonusParts.ToArray()[l];
					if (bonusPart2.IdFromConfig == bonusPartsData.IdFromConfig[k])
					{
						string text2 = bonusPartsData.IDs[k];
						bool flag3;
						CustomColor customColor2;
						PaintType paintType2;
						PaintData paintData2;
						if (!(bonusPart2.ID == text2) && bonusPartsData.GetIsPainted(k, out flag3) && bonusPartsData.GetColor(k, out customColor2) && bonusPartsData.GetPaintType(k, out paintType2) && bonusPartsData.GetPaintData(k, out paintData2))
						{
							if (bonusPart2.Change(text2, false))
							{
								bonusPart2.TakeOn(true);
								bonusPart2.Paint(flag3, customColor2, paintData2, paintType2);
							}
							if (bonusPart2.Change(text2, false))
							{
								bonusPart2.TakeOn(true);
							}
						}
					}
				}
			}
		}
	}
	private static void LoadBodyParts(CarLoader carLoader, NewCarData carData)
	{
		bool flag = true;
		for (int i = 0; i < carData.BodyPartsData.Count; i++)
		{
			if (string.IsNullOrEmpty(carData.BodyPartsData.ToArray()[i].Id))
			{
				flag = false;
				break;
			}
		}
		
	    for (int m = 0; m < carLoader.carParts.Count; m++)
		{
			CarPart carPart = carLoader.carParts.ToArray()[m];
			if (carPart != null && !(carPart.handle == null))
			{
				if (carData.BodyPartsData.Count <= m)
				{
					break;
				}
				BodyPartData bodyPartData = default(BodyPartData);
				if (flag)
				{
					bool flag4 = false;
					for (int n = 0; n < carData.BodyPartsData.Count; n++)
					{
						BodyPartData bodyPartData2 = carData.BodyPartsData.ToArray()[n];
						if (!(bodyPartData2.Id != carPart.name))
						{
							flag4 = true;
							bodyPartData = bodyPartData2;
						}
					}
					if (!flag4)
					{
						continue;
					}
				}
				else
				{
					bodyPartData = carData.BodyPartsData.ToArray()[m];
				}
				carPart.Clone(bodyPartData);
				if (carPart.Switched)
				{
					carLoader.SwitchCarPart(carPart, true, true);
				}
				GameInventory instance = GameInventory.Instance;
				if (!string.IsNullOrEmpty(bodyPartData.TunedID))
				{
					if (instance.IsTuning(carLoader.carToLoad + "-" + bodyPartData.TunedID, carLoader.carToLoad + "-" + carPart.handle.name, true))
					{
						carLoader.TunePart(carPart.handle.name, bodyPartData.TunedID);
						InteractiveObject component = carPart.handle.GetComponent<InteractiveObject>();
						if (component)
						{
							component.SetID(bodyPartData.TunedID);
						}
					}
					else
					{
						Debug.Log(string.Concat(new string[]
						{
							"[CarLoader] -> LoadCarFromFile() Skip tuning part ",
							carLoader.carToLoad,
							"-",
							carPart.handle.name,
							" on ",
							carLoader.carToLoad,
							"-",
							bodyPartData.TunedID
						}));
					}
				}
				carLoader.SetCarColor(carPart, carPart.Color);
				if (carPart.IsTinted)
				{
					PaintHelper.SetWindowProperties(carPart.handle, carPart.TintColor);
				}
				if (!string.IsNullOrEmpty(carPart.Livery))
				{
					carLoader.SetCarLivery(carPart, bodyPartData.Livery, bodyPartData.LiveryStrength);
				}
				if (carPart.OutsideRustEnabled)
				{
					carLoader.EnableRustOutside(carPart, true);
				}
				if (carPart.Unmounted)
				{
					carLoader.TakeOffCarPartFromSave(carPart.handle.name);
				}
				if (carPart.Dust > 0f)
				{
					carLoader.EnableDust(carPart, carPart.Dust);
				}
				if (carPart.WashFactor < 1f)
				{
					carLoader.SetWashFactor(carPart, carPart.WashFactor);
				}
				if (carPart.Dent < 1f)
				{
					carLoader.SetDent(carPart, carPart.Dent);
				}
			}
		}
		carLoader.UpdateCarBodyParts();
	}
	private static void LoadCarParts(CarLoader carLoader, NewCarData carData)
	{
		for (int num2 = 0; num2 < carData.PartData.Count; num2++)
		{
			PartData partData = carData.PartData.ToArray()[num2];
			Transform transform2 = carLoader.transform.Find(partData.Path);
			if (transform2 == null || !transform2.gameObject.activeSelf)
			{
				Debug.LogWarning("Part " + partData.Path + " not found.");
			}
			else
			{
				PartScript component2 = transform2.GetComponent<PartScript>();
				if (!(component2 == null))
				{
					component2.Clone(partData);
				}
			}
		}
		for (int num3 = 0; num3 < carData.PartData.Count; num3++)
		{
			MountObjectData mountObjectData = carData.PartData.ToArray()[num3].MountObjectData;
			if (mountObjectData != null && mountObjectData.Condition != null && mountObjectData.IsStuck != null)
			{
				Transform transform3 = carLoader.root.transform.Find(mountObjectData.ParentPath);
				if (transform3 == null)
				{
					Debug.LogWarning(string.Format("{0} not found.", transform3));
				}
				else
				{
					PartScript component3 = transform3.GetComponent<PartScript>();
					if (!(component3 == null) && !component3.IsUnmounted)
					{
						component3.SetMountObjectData(mountObjectData);
					}
				}
			}
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
		ToolsMoveManager toolsMoveManager = ToolsMoveManager.Get();
		if (toolsMoveManager)
		{
			CarPlace carPlace = (CarPlace)carLoader.placeNo;
			if (carData.TooolsData.WelderIsConnected)
			{
				toolsMoveManager.MoveTo(IOSpecialType.Welder, carPlace, false);
			}
			if (carData.TooolsData.InteriorDetailingToolkitIsConnected)
			{
				toolsMoveManager.MoveTo(IOSpecialType.InteriorDetailingToolkit, carPlace, false);
			}
			if (carData.TooolsData.OilbinIsConnected && carLoader.e_engine_h != null && carLoader.e_engine_h.name != "#Dummy")
			{
				toolsMoveManager.MoveTo(IOSpecialType.Oilbin, carPlace, false);
			}
			if (carData.TooolsData.EngineCraneIsConnected)
			{
				toolsMoveManager.MoveTo(IOSpecialType.EngineCrane, carPlace, false);
			}
			if (carData.TooolsData.HeadlampAlignmentSystemIsConnected)
			{
				toolsMoveManager.MoveTo(IOSpecialType.HeadlampAlignmentSystem, carPlace, false);
			}
			if (carData.TooolsData.WindowTintingToolkitIsConnected)
			{
				toolsMoveManager.MoveTo(IOSpecialType.WindowTint, carPlace, false);
			}
		}
	}
}