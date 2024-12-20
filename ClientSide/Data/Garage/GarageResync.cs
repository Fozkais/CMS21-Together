using System.Collections;
using System.Collections.Generic;
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
		yield return new WaitForEndOfFrame();

		foreach (int carLoaderID in ClientData.Instance.loadedCars.Keys)
		{
			if (!ClientData.Instance.loadedCars[carLoaderID].needResync) continue;
			
			CarSpawnHooks.listenToDelete = false;
			GameData.Instance.carLoaders[carLoaderID].DeleteCar();
		}

		yield return new WaitForEndOfFrame();
		ClientSend.AskResync(PacketTypes.loadCar);
		MelonLogger.Msg("Delete all car and resend them!");
	}
}