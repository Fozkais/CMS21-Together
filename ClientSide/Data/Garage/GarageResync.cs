using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;


namespace CMS21Together.ClientSide.Data.Garage;

public static class GarageResync
{
	public static void ClearOutdatedCars()
	{
		List<ModCar> carsToCheck = ClientData.Instance.loadedCars.Values.ToList();
		for (int i = 0; i < carsToCheck.Count; i++)
		{
			ModCar car = carsToCheck[i];
			if (car.needResync)
			{
				CarSpawnHooks.listenToDelete = false;
				GameData.Instance.carLoaders[car.carLoaderID].DeleteCar();
				ClientData.Instance.loadedCars.Remove(car.carLoaderID);
			}
		}

	}

	public static IEnumerator ResyncGarage()
	{
		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForSeconds(0.5f);
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.5f);
		ClearOutdatedCars();
		ClientSend.AskFullSync();
	}


}