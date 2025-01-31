using CMS.Managers;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class CarPaintLogic
{
	public static bool listen = true;

	public static void Reset() => listen = true;
	
	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.SetCarColor))]
	[HarmonyPostfix]
	public static void SetCarColorHook(CarPart part, Color c, CarLoader __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		MelonLogger.Msg("SetCarColorHook");
	}
	
	[HarmonyPatch(typeof(PaintshopManager), nameof(PaintshopManager.SubmitColor), typeof(bool))]
	[HarmonyPostfix]
	public static void SubmitColorHook(bool setSelected, PaintshopManager __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		MelonLogger.Msg($"SubmitColor : {__instance.paintshopState.Selected.Color}");
	}
	
	[HarmonyPatch(typeof(PaintHelper), nameof(PaintHelper.SetColor), typeof(Renderer), typeof(Color), typeof(bool))]
	[HarmonyPostfix]
	public static void SetColorHook(Renderer renderer, Color c, bool isBodyPart)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		MelonLogger.Msg("SetColorHook");
	}
}