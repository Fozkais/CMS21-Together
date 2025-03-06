using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla.Cars;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Car;

[HarmonyPatch]
public static class ParkHook
{
	public static bool listen = true;

	/*[HarmonyPatch(typeof(CarLoader), nameof(GameDataManager.SaveCarInParking))]
	[HarmonyPostfix]
	public static void SaveCarInParkingHook(NewCarData carData, int index, GameDataManager __instance)
	{
		if (!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		MelonCoroutines.Start(AddCarToParkHook(carData, index));
	}*/

	private static IEnumerator AddCarToParkHook(NewCarData carData, int index)
	{
		yield return new WaitForEndOfFrame();
		int i = 0;
		while (i < 10)
		{
			yield return new WaitForSeconds(0.10f);
			i++;
		}
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		
		ClientSend.AddCarToParkPacket(new ModNewCarData(carData), index);
	}

	/*[HarmonyPatch(typeof(CarLoader), nameof(GameDataManager.LoadCarInGarage))]
	[HarmonyPostfix]
	public static void LoadCarInGarageHook(int index, GameDataManager __instance)
	{
		if (!Client.Instance.isConnected) return;

		ClientSend.RemoveCarFromParkPacket(index);
	}*/

	public static IEnumerator AddCarToPark(ModNewCarData car, int index)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		GameManager.Instance.GameDataManager.SaveCarInParking(car.ToGame(), index);
	}

	public static IEnumerator RemoveCarFromPark(int index)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		
		GameManager.Instance.GameDataManager.SaveCarInParking(default(NewCarData), index);
	}
}