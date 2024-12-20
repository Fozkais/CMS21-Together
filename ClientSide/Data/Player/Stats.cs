using System.Collections;
using CMS.UI.Logic.Upgrades;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Stats
{
	public static bool listentoAddMoney = true;
	public static bool listentoAddScrap = true;

	public static void Reset()
	{
		listentoAddMoney = true;
		listentoAddScrap = true;
	}


	public static void SendInitialStats()
	{
		if (!Server.Instance.isRunning) return;
		
		ClientSend.StatPacket(GlobalData.PlayerMoney, ModStats.money, true);
		ClientSend.StatPacket(GlobalData.PlayerScraps, ModStats.scrap, true);
	}

	public static IEnumerator UpdateStats(ModStats type, int value, bool initial)
	{
		while (!ClientData.GameReady)
			yield return new WaitForSeconds(0.25f);
		yield return new WaitForEndOfFrame();

		if (initial)
		{
			switch (type)
			{
				case ModStats.money:
					ClientData.Instance.money = value;
					GlobalData.SetPlayerMoney(value);
					UIManager.Get().RefreshStatsUICoroutine(StatType.Money);
					break;
				case ModStats.scrap:
					ClientData.Instance.scrap = value;
					GlobalData.SetPlayerScraps(value);
					UIManager.Get().RefreshStatsUICoroutine(StatType.Scraps);
					break;
			}

			yield break;
		}

		switch (type)
		{
			case ModStats.money:
				listentoAddMoney = false;
				GlobalData.AddPlayerMoney(value);
				break;
			case ModStats.scrap:
				listentoAddScrap = false;
				GlobalData.AddPlayerScraps(value);
				break;
		}
	}

	[HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerExp))]
	[HarmonyPostfix]
	public static void AddPlayerExpHook(int exp, bool instant = false)
	{
		if (!Client.Instance.isConnected) return;
		if (ClientData.Instance.gamemode == Gamemode.Sandbox) return;

		ClientData.Instance.exp = GlobalData.PlayerExp;

		MelonLogger.Msg($"Send XP Packet : {GlobalData.PlayerExp} , {GlobalData.PlayerLevel}");
		ClientSend.ExpPacket(GlobalData.PlayerExp, GlobalData.PlayerLevel);
	}
	
	[HarmonyPatch(typeof(UpgradeSystem), nameof(UpgradeSystem.AddPoints))]
	[HarmonyPostfix]
	public static void AddPointsHook(UpgradeSystem __instance)
	{
		if (!Client.Instance.isConnected) return;
		if (ClientData.Instance.gamemode == Gamemode.Sandbox) return;
		
		MelonLogger.Msg($"Send Point Packet : {GlobalData.PlayerExp}");
		ClientSend.PointPacket(__instance.availablePoints);
	}
	
	[HarmonyPatch(typeof(SkillsTab), nameof(SkillsTab.UnlockSkillAction))]
	[HarmonyPostfix]
	public static void UnlockSkillActionHook(SkillsTab __instance)
	{
		if (!Client.Instance.isConnected) return;
		if (ClientData.Instance.gamemode == Gamemode.Sandbox) return;
		
		MelonLogger.Msg($"Send Point Packet : {GlobalData.PlayerExp}");
		ClientSend.PointPacket(__instance.upgradeSystem.availablePoints);
	}
	
	[HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerMoney))]
	[HarmonyPostfix]
	public static void AddPlayerMoneyHook(int money)
	{
		if (!Client.Instance.isConnected) return;
		if (ClientData.Instance.gamemode == Gamemode.Sandbox) return;

		ClientData.Instance.money = GlobalData.PlayerMoney;

		if (!listentoAddMoney)
		{
			listentoAddMoney = true;
			return;
		}

		ClientSend.StatPacket(money, ModStats.money, false);
	}

	[HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerScraps))]
	[HarmonyPostfix]
	public static void AddPlayerScrapsHook(int amount)
	{
		if (!Client.Instance.isConnected) return;
		if (ClientData.Instance.gamemode == Gamemode.Sandbox) return;

		ClientData.Instance.scrap = GlobalData.PlayerScraps;

		if (!listentoAddScrap)
		{
			listentoAddScrap = true;
			return;
		}

		ClientSend.StatPacket(amount, ModStats.scrap, false);
	}
}