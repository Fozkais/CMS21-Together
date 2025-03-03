using System.Collections;
using CMS.UI.Logic;
using CMS.UI.Logic.Navigation;
using CMS.UI.Logic.Upgrades;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class GarageUpgradeHooks
{
	public static bool listenToUpgrades = true;
	public static bool sentInitial;
	public static bool receivedInitial;
	
	public static void Reset()
	{
		listenToUpgrades = false;
		sentInitial = false;
		receivedInitial = false;
	}


	/*[HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.SwitchInteractiveObjects))]
	[HarmonyPrefix]
	public static void SwitchInteractiveObjectsHook(string upgradeID, bool on)
	{
		if (!Client.Instance.isConnected || !listenToUpgrades)
		{
			listenToUpgrades = true;
			return;
		}
		if (SavesManager.currentSave.Difficulty == DifficultyLevel.Sandbox) return;

		MelonLogger.Msg($"[GarageUpgradeHooks-> SwitchInteractiveObjectsHook] Triggered: {upgradeID}, {on}");
		ClientData.Instance.garageUpgrades[upgradeID] = new GarageUpgrade(upgradeID, on);
		ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[upgradeID]);
	}*/


	
	[HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.UnlockCurrentSelectedSkillAction))]
	[HarmonyPrefix]
	public static void UnlockCurrentSelectedSkillActionHook(GarageAndToolsTab __instance=null)
	{
		if (!Client.Instance.isConnected || !listenToUpgrades)
		{
			if (sentInitial || receivedInitial) listenToUpgrades = true;
			return;
		}
		
		if (SavesManager.currentSave.Difficulty == DifficultyLevel.Sandbox) return;

		int upgradeCost = __instance.upgradeSystem.GetUpgradeCost(__instance.currentUpgradeItem.UpgradeID, __instance.currentUpgradeItem.UpgradeLevel, UpgradeType.Money);
		if (upgradeCost <= GlobalData.PlayerMoney)
		{
			if (__instance.currentUpgradeItem.UpgradeID == "crane")
				GameData.Instance.engineStandLogic2.gameObject.SetActive(true);
			
			//MelonLogger.Msg($"[GarageUpgradeHooks->UnlockCurrentSelectedSkillActionHook] Triggered: {__instance.currentUpgradeItem.upgradeID}");
			ClientData.Instance.garageUpgrades[__instance.currentUpgradeItem.upgradeID] = new GarageUpgrade(__instance.currentUpgradeItem.upgradeID, true);
			ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[__instance.currentUpgradeItem.upgradeID]);
		}
	}

	public static IEnumerator SendInitial()
	{
		if (sentInitial || !Server.Instance.isRunning) yield break;

		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.2f);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();

		GameData.Instance.upgradeTools.PrepareItems();
		yield return new WaitForEndOfFrame();
		
		foreach (UpgradeItem item in GameData.Instance.upgradeTools.upgradeItems)
		{
			//MelonLogger.Msg($"Upgrade : {item.upgradeID} , state : {item.upgradeState}.");
			ClientData.Instance.garageUpgrades[item.upgradeID] = new GarageUpgrade(item.upgradeID, item.upgradeState == UpgradeState.Unlocked);
			ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[item.upgradeID]);
			
			if (item.upgradeID == "crane" && item.upgradeState != UpgradeState.Unlocked)
				GameData.Instance.engineStandLogic2.gameObject.SetActive(false);
			else if (item.upgradeID == "crane" && item.upgradeState == UpgradeState.Unlocked)
				GameData.Instance.engineStandLogic2.gameObject.SetActive(true);
		}
		ClientSend.GarageUpgradePacket(new GarageUpgrade("initialSent", false));
		yield return new WaitForEndOfFrame();
		sentInitial = true;
		listenToUpgrades = true;
		MelonLogger.Msg($"[GarageUpgradeHooks->SendInitial] Sent initials upgrades to server");
	}
}