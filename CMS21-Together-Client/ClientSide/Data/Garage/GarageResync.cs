using System.Collections;
using System.Linq;
using CMS21_Together_Core.Data;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Handle;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage;

public static class GarageResync
{
	public static IEnumerator ResyncCars()
	{
		yield return new WaitForEndOfFrame();
		MelonLogger.Msg("Remove all car !");
		var carsToCheck = ClientData.Instance.loadedCars.Values.ToList();
		for (var i = 0; i < carsToCheck.Count; i++)
		{
			var car = carsToCheck[i];
			if (car.needResync)
			{
				CarSpawnHooks.listenToDelete = false;
				GameData.Instance.carLoaders[car.carLoaderID].DeleteCar();
				yield return new WaitForEndOfFrame();
				ClientData.Instance.loadedCars.Remove(car.carLoaderID);
				ClientSend.ResyncCar(car.carLoaderID);
				MelonLogger.Msg($"Asked resync for {car.carLoaderID} ({car.carID}) to server!");
			}
		}
	}

	public static IEnumerator ResyncGarage()
	{
		while (SceneManager.CurrentScene() != GameScene.garage)
			yield return new WaitForSeconds(0.5f);
		while (!NotificationCenter.IsGameReady)
			yield return new WaitForSeconds(0.25f);
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.5f);

		MelonCoroutines.Start(ResyncCars());
		yield return new WaitForEndOfFrame();
		ClientSend.ResyncTools();
		/*yield return new WaitForEndOfFrame();
		ClientSend.ResyncPark();*/
		yield return new WaitForEndOfFrame();
		ClientSend.ResyncUpgrade();
		yield return new WaitForEndOfFrame();
		ClientSend.ResyncEngineStandPacket(true);
		ClientSend.ResyncEngineStandPacket(false);
	}
}