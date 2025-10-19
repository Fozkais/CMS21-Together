using System.Collections;
using System.Linq;
using CMS.UI.Logic;
using CMS.UI.Logic.RepairPart;
using CMS.UI.Windows;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Tools;

[HarmonyPatch]
public static class RepairPartLogic
{
	public static bool listen = true;
	
	[HarmonyPatch(typeof(RepairPartWindow), nameof(RepairPartWindow.ProcessGameResult))]
	[HarmonyPrefix]
	public static void ProcessGameResultHook(BarType result, RepairPartWindow __instance)
	{
		if(!Client.Instance.isConnected) return;
		if (!listen) { listen = true; return;}

		bool success = result == BarType.Success;
		
		ClientSend.RepairPart(new ModPartInfo(__instance.currentItemInfo), __instance.currentRepairType == RepairType.Body, success);
		MelonLogger.Msg("Sent a part repair !");
	}

	public static IEnumerator RepairAction(ModPartInfo info, bool isBody, bool success)
	{
		while (!GameData.isReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (Player.Inventory.modItems.Any(i => i.UID == info.Item.UID))
		{
			ModItem item = Player.Inventory.modItems.First(i => i.UID == info.Item.UID);
			item.Condition = (success ? info.SuccessCondition : info.FailCondition);
			item.RepairAmount++;
			if (isBody)
			{
				item.Dent = (success ? info.DentSuccessCondition : info.DentFailCondition);
			}
		}
	}
}