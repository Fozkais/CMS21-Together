using CMS21Together.ClientSide.Handle;
using CMS21Together.Shared.Data;
using Il2Cpp;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.PlayerData
{
    public class Stats
    {
        public static void HandleMoney()
        {
            int playerMoney = GlobalData.PlayerMoney;
            if (playerMoney > ClientData.playerMoney || playerMoney < ClientData.playerMoney)
            {
                int diff = playerMoney - ClientData.playerMoney;
                ClientData.playerMoney += diff;
                ClientSend.Stats(diff, ModStats.money);
            }
        }
        
        public static void HandleScrap()
        {
            int playerScraps = GlobalData.PlayerScraps;
            if (playerScraps > ClientData.playerScrap || playerScraps < ClientData.playerScrap)
            {
                int diff = playerScraps - ClientData.playerScrap;
                ClientData.playerScrap += diff;
                ClientSend.Stats(diff, ModStats.scrap);
                //MelonLogger.Msg($"Sending scraps!! +{diff}");
            }
        }

        public static void HandleExp()
        {
            int playerExp = GlobalData.PlayerExp;
            if (playerExp > ClientData.playerExp || playerExp <  ClientData.playerExp)
            {
                int diff = playerExp -  ClientData.playerExp;
                ClientData.playerExp += diff;
                ClientSend.Stats(diff, ModStats.exp);
            }
        }
    }
}