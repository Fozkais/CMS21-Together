using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using WelderL = WelderLogic;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class WelderLogic
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(WelderL), nameof(WelderL.DoWorkAnim))]
	[HarmonyPrefix]
	public static void TireChangerFix(CarLoader carLoader, WelderL __instance)
	{
		if (!Client.Instance.isConnected || !listen) { listen = true; return; }

		int carLoaderID = carLoader.gameObject.name[10] - '0' - 1;

		ClientSend.WelderPacket(carLoaderID);
	}

	public static IEnumerator UseWelder(int carLoaderID)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		MainMod.StartCoroutine(GameData.Instance.welderLogic.DoWorkAnim(GameData.Instance.carLoaders[carLoaderID]));
	}
}