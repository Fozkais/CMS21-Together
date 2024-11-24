using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class EngineCrane
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.InsertEngineToCar))]
	[HarmonyPrefix]
	public static void InsertEngineIntoCarHook(GroupItem engine)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}
		
		ClientSend.EngineCraneHandlePacket(1,-1,new ModGroupItem(engine));
	}
        
	[HarmonyPatch(typeof(CarLoader), nameof(CarLoader.UseEngineCrane))]
	[HarmonyPostfix]
	public static void UseEngineCraneHook(CarLoader __instance)
	{
		if(!Client.Instance.isConnected || !listen) { listen = true; return;}

		var carLoaderID = __instance.gameObject.name[10] - '0' - 1;
		ClientSend.EngineCraneHandlePacket(-1, carLoaderID);
	}

	public static IEnumerator UseEngineCrane(int carLoaderID)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		listen = false;
		GameData.Instance.carLoaders[carLoaderID].UseEngineCrane();
	}
	
	public static IEnumerator InsertEngineIntoCar(ModGroupItem engine)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();
		
		listen = false;
		NotificationCenter.Get().InsertEngineToCar(engine.ToGame());
	}
}