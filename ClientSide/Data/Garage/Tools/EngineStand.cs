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
	private static bool isStopping = false;
	private static Coroutine handleRoutine;
	
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

		if (handleRoutine == null) MelonCoroutines.Start(HandleEngineStand());
		if (listen)
			ClientSend.EngineStandSetGroupPacket(new ModGroupItem(groupItem));
		else
			listen = true;
		MelonLogger.Msg("[EngineStand->SetGroupOnEngineStand] Hook!");
	}
	
	[HarmonyPatch(typeof(PieMenuController), "_GetOnClick_b__72_35")]
	[HarmonyPrefix]
	public static void TakeOffEngineFromStandHook()
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		if (handleRoutine != null) MelonCoroutines.Stop(handleRoutine);
		isStopping = true;
		ClientSend.TakeOffEnginePacket();
		MelonLogger.Msg("[EngineStand->TakeOffEngine] Hook!");
	}

	public static IEnumerator TakeOnEngineFromStand(ModGroupItem engineGroup)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		listen = false;
		MainMod.StartCoroutine(GameData.Instance.engineStandLogic.SetGroupOnEngineStand(engineGroup.ToGame()));
	}
	private static IEnumerator HandleEngineStand()
	{
		for (int i = 0; i < 5; i++)
			yield return new WaitForEndOfFrame();

		var es = ClientData.Instance.engineStand = new ModEngineStand();
		
		IEnumerator routine = GetReferencesAndHandle(es.partReferences, es.parts);
		yield return routine;
		yield return new WaitForEndOfFrame();
		
		MelonCoroutines.Start(UpdateEngineStand(es.partReferences, es.parts));
	}
	
	private static IEnumerator UpdateEngineStand(Dictionary<int, PartScript> refs, Dictionary<int, ModPartScript> handles)
	{
		if (isStopping) yield break;

		/*for (int i = 0; i < refs.Count; i++)
		{
			if (isStopping) yield break;
			
			if (handles[i].unmounted != refs[i].IsUnmounted)
			{
				MelonLogger.Msg("Found Difference on EngineStand");
				handles[i] = new ModPartScript(refs[i], i, -1, ModPartType.engine);
				ClientSend.PartScriptPacket(handles[i], -1);
			}
		}

		yield return new WaitForEndOfFrame();
		MelonCoroutines.Start(UpdateEngineStand(refs, handles));*/
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
		isStopping = false;
		ClientData.Instance.engineStand.isHandled  = true;
		MelonLogger.Msg("[EngineStand->GetReferences] Finished without error.");
	}
}