using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CMS21Together.ClientSide.Data.Garage.Car;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;


[HarmonyPatch]
public static class EngineStand
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(EngineStandLogic), nameof(EngineStandLogic.IncreaseEngineStandAngle))] 
	[HarmonyPrefix]
	public static void IncreaseEngineStandAngleHook(float val)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.EngineStandAnglePacket(val);
		MelonLogger.Msg($"[EngineStand->IncreaseEngineStandAngle] Hook: {val}!");
	}
	
	[HarmonyPatch(typeof(EngineStandLogic), nameof(EngineStandLogic.SetGroupOnEngineStand))] 
	[HarmonyPostfix]
	public static void SetGroupOnEngineStand(GroupItem groupItem, bool withFade = true)
	{
		if(!Client.Instance.isConnected) {return;}

		MelonCoroutines.Start(HandleEngineStand());
		if (listen)
			ClientSend.EngineStandSetGroupPacket(new ModGroupItem(groupItem));
		else
			listen = true;
		MelonLogger.Msg("[EngineStand->SetGroupOnEngineStand] Hook!");
	}
	
	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.TakeOffEngineFromStand))]
	[HarmonyPrefix]
	public static void TakeOffEngineFromStandHook()
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.TakeOffEnginePacket();
		MelonLogger.Msg("[EngineStand->TakeOffEngine] Hook!");
	}

	public static IEnumerator TakeOnEngineFromStand(ModGroupItem engineGroup)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		MainMod.StartCoroutine(GameData.Instance.engineStandLogic.SetGroupOnEngineStand(engineGroup.ToGame(), false));
	}
	public static IEnumerator TakeOffEngineFromStand()
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		MainMod.StartCoroutine(NotificationCenter.Get().TakeOffEngineFromStand());
	}
	
	public static IEnumerator IncreaseEngineStandAngle(int angle)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		listen = false;
		GameData.Instance.engineStandLogic.IncreaseEngineStandAngle(angle);
	}
	
	private static IEnumerator HandleEngineStand()
	{
		for (int i = 0; i < 5; i++)
			yield return new WaitForEndOfFrame();

		var es = ClientData.Instance.engineStand = new ModEngineStand();
		
		IEnumerator routine = GetReferencesAndHandle(es.partReferences, es.parts);
		yield return routine;
		yield return new WaitForEndOfFrame();
		
	}

	private static IEnumerator GetReferencesAndHandle(Dictionary<int, PartScript> refs, Dictionary<int, ModPartScript> handle)
	{
		yield return new WaitForSeconds(.5f);
		var engineObj = GameData.Instance.engineStandLogic.engineGameObject;
		if (engineObj == null)
		{
			MelonLogger.Warning("[EngineStand->GetReferences] EngineStand as no engineObject ! aborting...");
			yield break;
		}

		List<PartScript> parts = engineObj.GetComponentsInChildren<PartScript>().ToList();
		yield return new WaitForEndOfFrame();
		for (int i = 0; i < parts.Count; i++)
		{
			if (!refs.ContainsKey(i))
			{
				refs.Add(i, parts[i]);
				handle.Add(i, new ModPartScript(parts[i], i, -1, ModPartType.engineStand));
			}
		}
		
		yield return new WaitForEndOfFrame();
		ClientData.Instance.engineStand.isHandled  = true;
		MelonLogger.Msg("[EngineStand->GetReferences] Finished without error.");
	}
}