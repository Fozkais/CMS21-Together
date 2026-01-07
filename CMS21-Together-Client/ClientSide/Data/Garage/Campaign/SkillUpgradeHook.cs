using System.Collections;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Handle;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class SkillUpgradeHook
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(UpgradeSystem), nameof(UpgradeSystem.UnlockUpgrade))]
	[HarmonyPostfix]
	public static void UnlockUpgradeHook( string id, int lvl, UpgradeType upgradeType, UpgradeSystem __instance)
	{
		if (!Client.Instance.isConnected || !listen) { listen = true; return; }
		
		
		if (__instance == null) return;
		var items = __instance.GetUnlocked(id);
		if (items == null) return;
		
		//MelonLogger.Msg($"UnlockedState: ");
		List<bool> skillInfo = new List<bool>();
		for (int i = 0; i < items.Length; i++)
		{
			//MelonLogger.Msg($"{i} : {items[i]}");
			skillInfo.Add(items[i]);
		}
		
		// ReSharper disable once PossibleNullReferenceException
		ClientSend.SkillChangePacket(id, skillInfo);
		MelonCoroutines.Start(UpdateSkillPoint(__instance));
	}

	private static IEnumerator UpdateSkillPoint(UpgradeSystem upgradeSystem)
	{
		yield return new WaitForSeconds(0.5f);
		
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		MelonLogger.Msg($"UpdateSkillPoint: {upgradeSystem.AvailablePoints}");
		ClientSend.PointPacket(upgradeSystem.AvailablePoints);
	}
}