using System.Collections;
using CMS21Together.Shared;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

public static class CarSyncManager
{
	public static IEnumerator ChangePosition(int carLoaderID, int placeNo)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (!DataHelper.IsValidCarLoaderID(carLoaderID) ||
		    GameData.Instance?.carLoaders == null ||
		    carLoaderID >= GameData.Instance.carLoaders.Length)
		{
			MelonLogger.Warning($"[CarSyncManager] ChangePosition: invalid carLoaderID {carLoaderID}.");
			yield break;
		}

		if (ClientData.Instance.loadedCars.TryGetValue(carLoaderID, out var car))
		{
			if (placeNo != car.carPosition)
			{
				MelonLogger.Msg($"Change {car.carID} position to {placeNo}.");
				car.carPosition = placeNo;
				CarSyncHooks.listenToChangePosition = false;
				GameData.Instance.carLoaders[carLoaderID].ChangePosition(placeNo);
			}
		}
	}

	public static IEnumerator DeleteCar(int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (!DataHelper.IsValidCarLoaderID(carLoaderID) ||
		    GameData.Instance?.carLoaders == null ||
		    carLoaderID >= GameData.Instance.carLoaders.Length)
		{
			MelonLogger.Warning($"[CarSyncManager] DeleteCar: invalid carLoaderID {carLoaderID}.");
			yield break;
		}

		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID))
			ClientData.Instance.loadedCars.Remove(carLoaderID);
		CarSpawnHooks.listenToDelete = false;
		GameData.Instance.carLoaders[carLoaderID].DeleteCar();
	}
}