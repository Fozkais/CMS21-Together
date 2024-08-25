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
            if (playerMoney > ClientData.Instance.playerMoney || playerMoney < ClientData.Instance.playerMoney)
            {
                int diff = playerMoney - ClientData.Instance.playerMoney;
               // ClientData.Instance.playerMoney += diff;
               // ClientSend.Stats(diff, ModStats.money);
            }
        }
        
        public static void HandleScrap()
        {
            int playerScraps = GlobalData.PlayerScraps;
            if (playerScraps > ClientData.Instance.playerScrap || playerScraps < ClientData.Instance.playerScrap)
            {
                int diff = playerScraps - ClientData.Instance.playerScrap;
                //ClientData.Instance.playerScrap += diff;
                //ClientSend.Stats(diff, ModStats.scrap);
                //MelonLogger.Msg($"Sending scraps!! +{diff}");
            }
        }

        public static void HandleExp()
        {
            int playerExp = GlobalData.PlayerExp;
            if (playerExp > ClientData.Instance.playerExp || playerExp <  ClientData.Instance.playerExp)
            {
                int diff = playerExp -  ClientData.Instance.playerExp;
               // ClientData.Instance.playerExp += diff;
               // ClientSend.Stats(diff, ModStats.exp);
            }
        }
    }
}