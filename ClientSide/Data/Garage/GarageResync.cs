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
	public static IEnumerator ResyncCars()
	{
		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForSeconds(0.5f);
		while (!NotificationCenter.IsGameReady)
			yield return new WaitForSeconds(0.25f);
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.5f);
		yield return new WaitForEndOfFrame();
		MelonLogger.Msg("Remove all car !");
		for (int i = 0; i < ClientData.Instance.loadedCars.Values.Count; i++)
		{
			ModCar car = ClientData.Instance.loadedCars.Values.ToList()[i];
			if (car.needResync)
			{
				CarSpawnHooks.listenToDelete = false;
				GameData.Instance.carLoaders[car.carLoaderID].DeleteCar();
				yield return new WaitForEndOfFrame();
				ClientData.Instance.loadedCars.Remove(car.carLoaderID);
				ClientSend.ResyncCar(car.carLoaderID);
			}
		}
		MelonLogger.Msg("Asked resync to server!");
	}
}