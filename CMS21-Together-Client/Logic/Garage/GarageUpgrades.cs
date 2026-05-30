using System.Collections;
using CMS.UI.Logic.Upgrades;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using upgType = CMS21_Together_Core.Data.Enum.UpgradeType;

namespace CMS21Together.Logic.Garage;

[HarmonyPatch]
public static class GarageUpgrades
{
	public static bool IsSyncing = false;
	
	[HarmonyPatch(typeof(GarageAndToolsTab), nameof(GarageAndToolsTab.UnlockCurrentSelectedSkillAction))]
	[HarmonyPrefix]
	public static bool UnlockUpgradeActionHook(GarageAndToolsTab __instance)
	{
		if (!Client.Instance.IsConnected || IsSyncing) return true;


		var currentItem = __instance.currentUpgradeItem;
		if (currentItem == null)
		{
			MelonLogger.Msg($"[Client] Requesting invalid upgrade.");
			return false;
		}
		
		MelonLogger.Msg($"[Client] Requesting upgrade: {currentItem.UpgradeID} Lvl {currentItem.UpgradeLevel}");
        
		Client.Instance.Send(new UpgradeRequest() {
			id = currentItem.UpgradeID,
			level = currentItem.UpgradeLevel,
			type = upgType.Money
		});
		
		return false; 
	}
	
	[HarmonyPatch(typeof(SkillsTab), nameof(SkillsTab.UnlockCurrentSelectedSkillAction))]
	[HarmonyPrefix]
	public static bool UnlockSkillActionHook(SkillsTab __instance)
	{
		if (!Client.Instance.IsConnected || IsSyncing) return true;

		var currentItem = __instance.currentUpgradeItem;
		if (currentItem == null)
		{
			MelonLogger.Msg($"[Client] Requesting invalid skill.");
			return false;
		}
		
		MelonLogger.Msg($"[Client] Requesting Skill: {currentItem.UpgradeID} Lvl {currentItem.UpgradeLevel}");
		
		Client.Instance.Send(new UpgradeRequest()
		{
			id = currentItem.UpgradeID,
			level = currentItem.UpgradeLevel,
			type = upgType.Points 
		});
		
		return false; 
	}
	
	
	public static IEnumerator SyncUpgrades(GarageState packet, GarageAndToolsTab tools)
	{
		yield return new WaitForEndOfFrame();
		float timeout = 10f;
		float timer = 0f;
		
		while (timer < timeout)
		{
			if (tools.upgradeSystem != null && tools.upgradeItems != null && tools.upgradeItems.Length > 0)
				break;

			timer += Time.deltaTime;
			yield return null;
		}
        
		if (tools.upgradeSystem == null)
		{
			MelonLogger.Error("Failed to sync: UpgradeSystem did not initialize in time.");
			yield break;
		}

		IsSyncing = true;
		
		foreach (var upgradeEntry in packet.GarageUpgradeLevels)
		{
			foreach (var upgradeData in tools.upgradeSystem.UpgradesForMoney)
			{
				if (upgradeData != null && upgradeData.ID == upgradeEntry.Key)
				{
					for (int i = 0; i < upgradeEntry.Value.Length; i++)
					{
						if (upgradeData.Unlocked.Length > i)
						{
							upgradeData.Unlocked[i] = upgradeEntry.Value[i];
						}
					}
					break;
				}
			}
		}

		foreach (var skillEntry in packet.PlayerUpgradeLevels)
		{
			foreach (var skillData in tools.upgradeSystem.UpgradesForPoints)
			{
				if (skillData != null && skillData.ID == skillEntry.Key)
				{
					for (int i = 0; i < skillEntry.Value.Length; i++)
					{
						if (skillData.Unlocked.Length > i)
						{
							skillData.Unlocked[i] = skillEntry.Value[i];
						}
					}
					break;
				}
			}
		}

		tools.SwitchIfUnlocked();
		tools.PrepareItems();

		tools.upgradeSystem.AvailablePoints = packet.AvailablePoints;
		var skillsTab = Object.FindObjectOfType<SkillsTab>();
		if (skillsTab != null && skillsTab.isActiveAndEnabled)
		{
			skillsTab.Invoke(nameof(SkillsTab.RefreshGUI), 0f);
		}

		IsSyncing = false;
		MelonLogger.Msg("Garage and Skills synchronized successfully!");
		ClientData.IsGarageStateSynced = true;
	}
}