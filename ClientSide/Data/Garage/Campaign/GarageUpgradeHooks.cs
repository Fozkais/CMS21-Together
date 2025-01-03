using CMS.UI.Logic;
using CMS.UI.Logic.Navigation;
using CMS.UI.Logic.Upgrades;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Garage.Campaign;

[HarmonyPatch]
public static class GarageUpgradeHooks
{
	public static bool listenToUpgrades = true;


	[HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.SwitchInteractiveObjects))]
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
	}


	
	[HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.UnlockCurrentSelectedSkillAction))]
	[HarmonyPrefix]
	public static void UnlockCurrentSelectedSkillActionHook(GarageAndToolsTab __instance=null)
	{
		if (!Client.Instance.isConnected || !listenToUpgrades)
		{
			listenToUpgrades = true;
			return;
		}
		
		if (SavesManager.currentSave.Difficulty == DifficultyLevel.Sandbox) return;


		int upgradeCost = __instance.upgradeSystem.GetUpgradeCost(__instance.currentUpgradeItem.UpgradeID, __instance.currentUpgradeItem.UpgradeLevel, UpgradeType.Money);
		if (upgradeCost <= GlobalData.PlayerMoney)
		{
			MelonLogger.Msg($"[GarageUpgradeHooks->UnlockCurrentSelectedSkillActionHook] Post-Triggered: {__instance.currentUpgradeItem.upgradeID}");
			ClientData.Instance.garageUpgrades[__instance.currentUpgradeItem.upgradeID] = new GarageUpgrade(__instance.currentUpgradeItem.upgradeID, true);
			ClientSend.GarageUpgradePacket(ClientData.Instance.garageUpgrades[__instance.currentUpgradeItem.upgradeID]);
		}
	}
}