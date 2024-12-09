using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CMS21Together.ClientSide.Data.Garage;

public static class GarageResync
{
	public static IEnumerator ResyncCars()
	{
		while (SceneManager.GetActiveScene().name != "garage")
			yield return new WaitForSeconds(0.5f);
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.5f);
		
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		MelonLogger.Msg("Game should be ready ! resyncing..");
		
		for (int i = 0; i < ClientData.Instance.loadedCars.Count; i++)
		{
			CarSpawnHooks.listenToDelete = false;
			GameData.Instance.carLoaders[ClientData.Instance.loadedCars[i].carLoaderID].DeleteCar();
			ClientData.Instance.loadedCars.Remove(ClientData.Instance.loadedCars[i].carLoaderID);
		}
		yield return new WaitForEndOfFrame();
		ClientSend.AskResync(PacketTypes.loadCar);
		MelonLogger.Msg("Delete all car and resend them!");
	}
}