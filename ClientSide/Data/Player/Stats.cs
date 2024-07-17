using System;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using HarmonyLib;
using Il2Cpp;

namespace CMS21Together.ClientSide.Data.Player;

[HarmonyPatch]
public static class Stats
{

    public static bool listentoAddMoney = true;
    public static bool listentoAddScrap = true;
    
    [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerMoney))]
    [HarmonyPostfix]
    public static void AddPlayerMoneyHook(int money)
    {
        if(!Client.Instance.isConnected) return;
        if (ClientData.Instance.gamemode != Gamemode.Campaign) return;

        ClientData.Instance.money = GlobalData.PlayerMoney;
        
        if(!listentoAddMoney)  { listentoAddMoney = true; return;}
        ClientSend.StatPacket(money, ModStats.money);

    }
    
    [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerScraps))]
    [HarmonyPostfix]
    public static void AddPlayerScrapsHook(int amount)
    {
        if(!Client.Instance.isConnected) return;
        if (ClientData.Instance.gamemode != Gamemode.Campaign) return;

        ClientData.Instance.scrap = GlobalData.PlayerScraps;
        if(!listentoAddScrap)  { listentoAddScrap = true; return;}
        ClientSend.StatPacket(amount, ModStats.scrap);

    }
}
