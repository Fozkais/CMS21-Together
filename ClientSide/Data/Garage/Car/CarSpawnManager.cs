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
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;
		yield return YieldInstructions.WaitForEndOfFrame;

		while (!carLoader.IsCarLoaded()) yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;
		yield return YieldInstructions.WaitForEndOfFrame;
		
		var car = new ModCar(carLoaderID, name, carLoader.ConfigVersion, carLoader.placeNo, carLoader.customerCar);
		
		if (carLoader.customerCar)
			ClientSend.LoadJobCarPacket(car);

		yield return new WaitForEndOfFrame();
		if (!ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
		{
			ClientData.Instance.loadedCars.Add(carLoaderID, car);
			MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));
			
			while (!ClientData.Instance.loadedCars[carLoaderID].isReady)
				yield return new WaitForSeconds(0.25f);
			yield return new WaitForEndOfFrame();
			carLoader.SaveCarToFile();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			NewCarData carData = GameManager.Instance.GameDataManager.CurrentProfileData.carsInGarage[Helper.GetIndexFromCarLoaderName(carLoader.name)];
			ModNewCarData modCarData = new ModNewCarData(carData, carLoader.placeNo, carLoader.orderConnection);
			
			ClientSend.LoadCarPacket(modCarData, carLoaderID);
		}
	}

	public static IEnumerator LoadCarFromServer(ModNewCarData data, int carLoaderID)
	{
		while (!GameLoadHook.IsGameReady())
			yield return new WaitForSeconds(0.25f);

		yield return new WaitForEndOfFrame();

		var carLoader = GameData.Instance.carLoaders[carLoaderID];
		carLoader.placeNo = data.carPosition;
		carLoader.ConfigVersion = data.configVersion;
		var carData = data.ToGame();

		yield return new WaitForEndOfFrame();
		
		CarSpawnHooks.listenToSimpleLoad = false;
		if (data.jobID != -1)
			carLoader.SetCustomerCar(true, data.jobID);
		MainMod.StartCoroutine(carLoader.LoadCarFromFile(carData));
		var car = new ModCar(carLoaderID, data.carToLoad, data.configVersion);
		ClientData.Instance.loadedCars[carLoaderID] = car;
		MelonCoroutines.Start(PartsReferencer.GetPartReferences(ClientData.Instance.loadedCars[carLoaderID]));

		MelonLogger.Msg($"[CarManager->LoadCarFromServer] Loading {data.carToLoad} from server...");
	}
}