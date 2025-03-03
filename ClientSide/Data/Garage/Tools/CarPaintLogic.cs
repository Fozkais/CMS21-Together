using System.Collections;
using CMS.Managers;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class CarPaintLogic
{
	public static bool listen = true;

	public static void Reset() => listen = true;
	
	
	[HarmonyPatch(typeof(PaintshopManager), nameof(PaintshopManager.MakePaintEffects))]
	[HarmonyPostfix]
	public static void MakePaintEffectsHook(PaintshopManager __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		MelonLogger.Msg($"Car color change : {__instance.paintshopState.Selected.Color} !");
		ClientSend.CarPaint(new ModColor(__instance.paintshopState.Selected.Color));
	}

	public static IEnumerator ChangeColor(ModColor color)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		foreach (ModCar car in ClientData.Instance.loadedCars.Values)
		{
			if (car.carPosition == 5)
			{
				GameData.Instance.paintshopManager.paintshopState.SetSelectedColor(color.ToGame());
				GameData.Instance.paintshopManager.UpdateColor(color.ToGame());
				listen = false;
				MainMod.StartCoroutine(GameData.Instance.paintshopManager.MakePaintEffects());
				break;
			}
		}
		MelonLogger.Msg("Painted a car!");
	}
}