using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CMS.Difficulty;
using CMS.UI.Logic;
using CMS.UI.Logic.Upgrades;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Network.Handlers;

public static class WorldStatesPackets
{
	[PacketHandler(PacketTypes.WorldState)]
	public static void HandleWorldState(long senderId, WorldState packet)
	{
		DifficultyManager difficultyManager = Singleton<GameManager>.Instance.DifficultyManager;
		difficultyManager.SetDifficultyLevel((DifficultyLevel)packet.Gamemode);
		difficultyManager.ActivateDifficultyLevel();
		
		GlobalData.SetPlayerMoney(packet.Money);
		GlobalData.PlayerLevel = packet.Level - 1;
		GlobalData.PlayerExp = packet.Exp;
		int statValue = Singleton<GameManager>.Instance.PlatformManager.GetStatValue("stat_level");
		Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_level", GlobalData.RealPlayerLevel - statValue);
		UIManager.Get().RefreshStatsUICoroutine(StatType.Experience, true);
		
		ClientData.IsWorldStateSynced = true;
	}
	
	[PacketHandler(PacketTypes.GarageState)]
	public static void HandleGarageState(long senderId, GarageState packet)
	{
	    if (packet?.GarageUpgradeLevels == null) {
	       MelonLogger.Error("Packet or GarageUpgradeLevels is null!");
	       return;
	    }
	    if (GameData.Instance == null) {
	        MelonLogger.Error("CRITICAL: GameData.Instance is NULL. Are you in the main menu?");
	        return;
	    }
	    GarageAndToolsTab tools = GameData.Instance.GarageTools;
	    if (tools == null) {
	       MelonLogger.Error("CRITICAL: tools (GarageTools) is NULL!");
	       return;
	    }
	    if (tools.upgradeSystem == null || tools.upgradeItems == null) {
	       MelonLogger.Error($"Internal refs null: System={tools.upgradeSystem==null}, Items={tools.upgradeItems==null}");
	       return;
	    }
	    
	    tools.PrepareItems();
	    MelonCoroutines.Start(SyncUpgrades(packet, tools));
	}

	private static IEnumerator SyncUpgrades(GarageState packet, GarageAndToolsTab tools)
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

		MelonLogger.Msg("Garage and Skills synchronized successfully!");
		ClientData.IsGarageStateSynced = true;
	}
	
	
	[PacketHandler(PacketTypes.SyncEnd)]
	public static void HandleSyncEnd(long senderId, SyncEnd packet)
	{
		// On lance la coroutine pour attendre que les états soient validés
		MelonCoroutines.Start(WaitForSyncCompletion());
	}

	private static IEnumerator WaitForSyncCompletion()
	{
		float timeout = 15f;
		float timer = 0f;

		MelonLogger.Msg("Waiting for World and Garage states to sync...");

		while (timer < timeout)
		{
			if (ClientData.IsWorldStateSynced && ClientData.IsGarageStateSynced)
			{
				ClientData.IsInitialSyncFinished = true;
				MelonLogger.Msg("Initial synchronization finished successfully!");
				yield break;
			}

			timer += Time.deltaTime;
			yield return null; 
		}
		MelonLogger.Error("Sync timed out! Some data might be missing.");
	}
}