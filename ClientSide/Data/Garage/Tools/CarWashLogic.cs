using System.Collections;
using CMS;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class CarWashLogic
{
	public static bool listen = true;

	public static void Reset() => listen = true;
	
	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.TweenExteriorDustWash))]
	[HarmonyPostfix]
	public static void DTweenExteriorDustWashHook(float targetDust, float targetWash, float time, CarLoader __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		int carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		ClientSend.CarWashPacket(carLoaderID);
	}

	public static IEnumerator WashCar(int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		if (ClientData.Instance.loadedCars.ContainsKey(carLoaderID)) yield break;
		listen = false;
		GameData.Instance.carLoaders[carLoaderID].TweenExteriorDustWash(0f, 1f, 0.1f);
		listen = true;
	}
}