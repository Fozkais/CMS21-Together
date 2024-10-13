using System;
using System.Collections;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Stats
{
    private static bool initialStatSent;

    public static void Reset() { initialStatSent = false; }


    public static void SendInitialStats()
    {
        if(initialStatSent || !Server.Instance.isRunning) return;
        
        initialStatSent = true;
        ClientSend.StatPacket(GlobalData.PlayerMoney, ModStats.money);
        ClientSend.StatPacket(GlobalData.PlayerScraps, ModStats.scrap);

    }

    public static IEnumerator UpdateStats(ModStats type, int value)
    {
        while (!ClientData.GameReady)
            yield return new WaitForSeconds(0.25f);
        yield return new WaitForEndOfFrame();
        
        switch (type)
        {
            case ModStats.money:
                ClientData.Instance.money = value;
                GlobalData.SetPlayerMoney(value);
                break;
            case ModStats.scrap:
                ClientData.Instance.scrap = value;
                GlobalData.SetPlayerScraps(value);
                break;
        }
    }
    
    
    [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerMoney))]
    [HarmonyPostfix]
    public static void AddPlayerMoneyHook(int money)
    {
        if(!Client.Instance.isConnected) return;
        if (ClientData.Instance.gamemode != Gamemode.Campaign) return;

        ClientData.Instance.money = GlobalData.PlayerMoney;
        ClientSend.StatPacket(ClientData.Instance.money, ModStats.money);
    }
    
    [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerScraps))]
    [HarmonyPostfix]
    public static void AddPlayerScrapsHook(int amount)
    {
        if(!Client.Instance.isConnected) return;
        if (ClientData.Instance.gamemode != Gamemode.Campaign) return;

        ClientData.Instance.scrap = GlobalData.PlayerScraps;
        ClientSend.StatPacket(amount, ModStats.scrap);

    }
}
