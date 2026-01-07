using System.Collections;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class PartsUpdater
{
	public static IEnumerator UpdatePartScripts(ModPartScript partScript, int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		if (carLoaderID == -1)
		{
			while (!ClientData.Instance.engineStand.isHandled)
				yield return new WaitForSeconds(.15f);
			yield return new WaitForEndOfFrame();
			
			MelonLogger.Msg($"[PartsUpdater->UpdatePartScripts]{partScript.id} for es 1");
			UpdatePartScript(partScript, ClientData.Instance.engineStand.partReferences[partScript.partID], -1);
			yield break;
		}
		if (carLoaderID == -2)
		{
			int counter = 0;
			while (counter < 25 && !ClientData.Instance.engineStand2.isHandled)
			{
				yield return new WaitForSeconds(.5f);
				counter++;
			}
			yield return new WaitForEndOfFrame();
			
			MelonLogger.Msg($"[PartsUpdater->UpdatePartScripts] {partScript.id} for es 2");
			UpdatePartScript(partScript, ClientData.Instance.engineStand2.partReferences[partScript.partID], -2);
			yield break;
		}
		
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var _car))
			while (!_car.isReady)
				yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		//MelonLogger.Msg("[PartsUpdater->UpdatePartScripts] Car ready, updating..");
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			var key = partScript.partID;
			var index = partScript.partIdNumber;

			PartScript reference;

			switch (partScript.type)
			{
				case ModPartType.engine:
					reference = car.CarPartInfo.EnginePartsReferences[key];
					break;
				case ModPartType.suspension:
					reference = car.CarPartInfo.SuspensionPartsReferences[key][index];
					break;
				case ModPartType.other:
					reference = car.CarPartInfo.OtherPartsReferences[key][index];
					break;
				case ModPartType.driveshaft:
					reference = car.CarPartInfo.DriveshaftPartsReferences[key];
					break;
				default:
					yield break;
			}

			MelonLogger.Msg("[PartsUpdater->UpdatePartScripts] updating PartScript..");
			UpdatePartScript(partScript, reference, carLoaderID);
		}
	}

	private static void UpdatePartScript(ModPartScript part, PartScript reference, int carLoaderID)
	{
		if (part == null || reference == null)
		{
			MelonLogger.Msg("Invalid part!");
			return;
		}

		if(carLoaderID != -1 && carLoaderID != -2)
			if (!string.IsNullOrEmpty(part.tunedID) && !string.IsNullOrEmpty(reference.tunedID))
				if (reference.tunedID != part.tunedID)
					GameData.Instance.carLoaders[carLoaderID].TunePart(reference.id, part.tunedID);
		reference.IsExamined = part.isExamined;
		reference.Quality = part.quality;
		reference.SetCondition(part.condition);
		reference.UpdateDust(part.dust, true);
		reference.SetConditionNormal(part.condition);

		if (!part.unmounted)
		{
			reference.IsPainted = part.isPainted;
			if (part.isPainted)
			{
				reference.CurrentPaintType = (PaintType)part.paintType;
				reference.CurrentPaintData = new ModPaintData().ToGame(part.paintData);
				reference.SetColor(part.color.ToGame());
				if ((PaintType)part.paintType == PaintType.Custom)
					PaintHelper.SetCustomPaintType(reference.gameObject, part.paintData.ToGame(part.paintData), false);
				else
					PaintHelper.SetPaintType(reference.gameObject, (PaintType)part.paintType, false);
			}

			if (reference.IsUnmounted)
			{
				reference.ShowBySaveGame();

				reference.Show();
				MelonCoroutines.Start(CustomPartScriptMethod.ShowMounted(reference));
				reference.ShowMountAnimation();

				reference.SetCondition(part.condition);
				reference.SetConditionNormal(part.condition);
			}

			if (carLoaderID != -1 && carLoaderID != -2)
			{
				var wheelData = GameData.Instance.carLoaders[carLoaderID].WheelsData;
				for (var i = 0; i < GameData.Instance.carLoaders[carLoaderID].WheelsData.Wheels.Count; i++)
				{
					GameData.Instance.carLoaders[carLoaderID].SetWheelSize((int)wheelData.Wheels[i].Width,
						(int)wheelData.Wheels[i].Size, (int)wheelData.Wheels[i].Profile, (WheelType)i);
					GameData.Instance.carLoaders[carLoaderID].SetET((WheelType)i, wheelData.Wheels[i].ET);
				}

				GameData.Instance.carLoaders[carLoaderID].SetWheelSizes();
			}
		}
		else
		{
			if (reference.IsUnmounted == false)
			{
				if (carLoaderID != -1 && carLoaderID != -2)
					reference.HideBySavegame(false, GameData.Instance.carLoaders[carLoaderID]);
				else
					reference.HideBySavegame(false);
			}
		}

		if (part.unmountWith != null)
		{
			for (var index = 0; index < part.unmountWith.Count; index++)
			{
				var partScript = part.unmountWith[index];
				UpdatePartScript(partScript, reference.unmountWith.ToArray()[index], carLoaderID);
			}
		}
	}

	public static IEnumerator UpdateBodyParts(ModCarPart carPart, int carLoaderID)
	{
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var _car))
			while (!_car.isReady)
				yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Car ready, updating..");
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			var key = carPart.carPartID;

			var reference = car.CarPartInfo.BodyPartsReferences[key];
			MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Updating BodyPart..");
			UpdateBodyPart(carPart, reference, carLoaderID);
		}
	}

	private static void UpdateBodyPart(ModCarPart carPart, CarPart reference, int carLoaderID)
	{
		if (carPart == null || reference == null)
		{
			MelonLogger.Msg("Invalid bodyPart!");
			return;
		}

		var color = carPart.colors.ToGame();
		var tintColor = carPart.TintColor.ToGame();

		if (reference.TunedID != carPart.tunedID)
			GameData.Instance.carLoaders[carLoaderID].TunePart(reference.name, carPart.tunedID);

		GameData.Instance.carLoaders[carLoaderID].SetDent(reference, carPart.dent);
		GameData.Instance.carLoaders[carLoaderID].EnableDust(reference, carPart.Dust);
		GameData.Instance.carLoaders[carLoaderID].SetCondition(reference, carPart.condition);
		GameData.Instance.carLoaders[carLoaderID].SetCarLivery(reference, carPart.livery, carPart.liveryStrength);

		reference.name = carPart.name;
		reference.IsTinted = carPart.isTinted;
		reference.PaintType = (PaintType)carPart.paintType;
		reference.OutsideRustEnabled = carPart.outsaidRustEnabled;
		reference.AdditionalString = carPart.additionalString;
		reference.Quality = carPart.quality;
		reference.WashFactor = carPart.washFactor;
		reference.StructureCondition = carPart.conditionStructure;
		reference.ConditionPaint = carPart.conditionPaint;

		if (!carPart.unmounted && !reference.name.StartsWith("license_plate"))
		{
			if (carPart.colors != null)
				GameData.Instance.carLoaders[carLoaderID].SetCarColor(reference, color);
			if (carPart.TintColor != null)
				GameData.Instance.carLoaders[carLoaderID].SetCarPaintType(reference, (PaintType)carPart.paintType);
		}

		if (!reference.Unmounted && carPart.unmounted)
			GameData.Instance.carLoaders[carLoaderID].TakeOffCarPartFromSave(reference.name);

		if (reference.Unmounted && !carPart.unmounted)
			GameData.Instance.carLoaders[carLoaderID].TakeOnCarPartFromSave(reference.name);

		if (reference.Switched != carPart.switched)
			GameData.Instance.carLoaders[carLoaderID].SwitchCarPart(reference, false, carPart.switched);

		foreach (var _carPart in carPart.connectedParts)
		{
			var key = _carPart.carPartID;
			var _reference = ClientData.Instance.loadedCars[carLoaderID].CarPartInfo.BodyPartsReferences[key];
			MelonLogger.Msg("[PartsUpdater->UpdateBodyParts] Updating BodyPart..");
			UpdateBodyPart(_carPart, _reference, carLoaderID);
		}

		if (carPart.isTinted && carPart.TintColor != null)
			PaintHelper.SetWindowProperties(reference.handle, (int)(carPart.TintColor.a * 255), tintColor);

		GameData.Instance.carLoaders[carLoaderID].SetCondition(reference, carPart.condition);
		GameData.Instance.carLoaders[carLoaderID].UpdateCarBodyPart(reference);
	}

	public static IEnumerator UpdateFluid(ModFluidData fluid, int carLoaderID)
	{
		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var _car))
			while (!_car.isReady)
				yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		PartUpdateHooks.listen = false;
		GameData.Instance.carLoaders[carLoaderID].FluidsData
			.SetLevelAndCondition(fluid.Level, fluid.Condition, (CarFluidType)fluid.CarFluid.FluidType);
	}
}