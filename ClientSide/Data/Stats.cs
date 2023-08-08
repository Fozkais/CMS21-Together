using CMS21MP.ClientSide.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Data
{
    public static class Stats
    {
        public static void HandleStats()
        {
            HandleMoney();
            HandleScrap();
            HandleExp();
        }
        
        public static void HandleMoney()
        {
            int playerMoney = GlobalData.PlayerMoney;
            if (playerMoney > ClientData.playerMoney || playerMoney < ClientData.playerMoney)
            {
                int diff = playerMoney - ClientData.playerMoney;
                ClientData.playerMoney += diff;
                //ClientSend.Stats(diff, true, 0); TODO: Send money packet
                MelonLogger.Msg($"Sending money!! +{diff}");
            }
        }
        
        public static void HandleScrap()
        {
            int playerScraps = GlobalData.PlayerScraps;
            if (playerScraps > ClientData.playerScrap || playerScraps < ClientData.playerScrap)
            {
                int diff = playerScraps - ClientData.playerScrap;
                ClientData.playerScrap += diff;
                // ClientSend.Stats(diff, true, 1); TODO: Send scrap packet
                MelonLogger.Msg($"Sending scraps!! +{diff}");
            }
        }

        public static void HandleExp()
        {
            int playerExp = GlobalData.PlayerExp;
            if (playerExp > ClientData.playerExp || playerExp <  ClientData.playerExp)
            {
                int diff = playerExp -  ClientData.playerExp;
                ClientData.playerExp += diff;
               // ClientSend.Stats(diff, true, 2); TODO: Send exp packet
                MelonLogger.Msg($"Sending exp!! +{diff}");
            }
        }
    }
}