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
using CMS21Together.Logic;
using CMS21Together.Logic.Garage;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Network.Handlers;

public static class WorldStatesPackets
{
	[PacketHandler(PacketTypes.WorldState)]
	public static void HandleWorldState(long senderId, WorldState packet)
	{
		if (packet.updateGamemode)
		{
			DifficultyManager difficultyManager = Singleton<GameManager>.Instance.DifficultyManager;
			difficultyManager.SetDifficultyLevel((DifficultyLevel)packet.Gamemode);
			difficultyManager.ActivateDifficultyLevel();
		}
		
		MelonLogger.Msg($"Received World State Sync :\nGamemode: {packet.Gamemode.ToString()}\nMoney: {packet.Money}\nLevel: {packet.Level}\n Exp:{packet.Exp}");
		
		GlobalData.PlayerMoney = packet.Money;
		GlobalData.PlayerLevel = packet.Level - 1;
		GlobalData.PlayerExp = packet.Exp;
		
		var profile = Singleton<GameManager>.Instance.GameDataManager.CurrentProfileData;
		if (profile != null)
		{
			profile.globalDataWrapper.PlayerLevel = GlobalData.PlayerLevel;
			profile.globalDataWrapper.PlayerExp = GlobalData.PlayerExp;
			profile.globalDataWrapper.PlayerMoney = GlobalData.PlayerMoney;
		}
		int currentPlatformLevel = Singleton<GameManager>.Instance.PlatformManager.GetStatValue("stat_level");
		int levelDifference = GlobalData.RealPlayerLevel - currentPlatformLevel;
		Singleton<GameManager>.Instance.PlatformManager.IncrementStat("stat_level", levelDifference);

		UIManager.Get().RefreshAllStats();
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
	    MelonCoroutines.Start( GarageUpgrades.SyncUpgrades(packet, tools));
	}
	
	[PacketHandler(PacketTypes.SyncEnd)]
	public static void HandleSyncEnd(long senderId, SyncEnd packet)
	{
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