using System;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using Il2Cpp;

namespace CMS21Together.ClientSide.Data.Player;

public static class Stats
{
    public static void SyncStats()
    {
        Gamemode gamemode = SavesManager.GetGamemodeFromDifficulty(SavesManager.currentSave.Difficulty);
        if (gamemode == Gamemode.Campaign)
        {
            SyncMoney();
            SyncScrap();
        }
    }

    private static void SyncScrap()
    {
        int playerScraps = GlobalData.PlayerScraps;
        if (playerScraps != ClientData.Instance.scrap)
        {
            int diff = playerScraps - ClientData.Instance.scrap;
            ClientData.Instance.scrap += diff;

            ClientSend.StatPacket(diff, ModStats.scrap);
        }
    }

    private static void SyncMoney()
    {
        int playerMoney = GlobalData.PlayerMoney;
        if (playerMoney != ClientData.Instance.money)
        {
            int diff = playerMoney - ClientData.Instance.money;
            ClientData.Instance.money += diff;

            ClientSend.StatPacket(diff, ModStats.money);
        }
    }
}

public enum StatSyncConfig
{
    Campaign,
    Sandbox
}