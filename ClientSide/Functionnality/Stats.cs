using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class Stats
    {
        // Money
        public static int moneyHandler;

        public static int scrapHandler;

        public static int ExperienceHandler;

        public static void HandleStats()
        {
            HandleMoney();
            HandleScrap();
            HandleExp();
        }
        
        public static void HandleMoney()
        {
            int playerMoney = GlobalData.PlayerMoney;
            if (playerMoney > moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.Stats(diff, true, 0);
                MelonLogger.Msg($"Sending money!! +{diff}");
            }
            else if (playerMoney < moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.Stats(diff, false , 0);
                MelonLogger.Msg($"Sending money!! {diff}");
            }
        }
        
        public static void HandleScrap()
        {
            int playerScraps = GlobalData.PlayerScraps;
            if (playerScraps > scrapHandler)
            {
                int diff = playerScraps - scrapHandler;
                scrapHandler += diff;
                ClientSend.Stats(diff, true, 1);
                MelonLogger.Msg($"Sending scraps!! +{diff}");
            }
            else if (playerScraps < scrapHandler)
            {
                int diff = playerScraps - scrapHandler;
                scrapHandler += diff;
                ClientSend.Stats(diff, false, 1);
                MelonLogger.Msg($"Sending scraps!! {diff}");
            }
        }
        public static void HandleExp()
        {
            int playerExp = GlobalData.PlayerExp;
            if (playerExp > ExperienceHandler)
            {
                int diff = playerExp - ExperienceHandler;
                ExperienceHandler += diff;
                ClientSend.Stats(diff, true, 2);
                MelonLogger.Msg($"Sending exp!! +{diff}");
            }
            else if (playerExp < ExperienceHandler)
            {
                int diff = playerExp - ExperienceHandler;
                ExperienceHandler += diff;
                ClientSend.Stats(diff, false, 2);
                MelonLogger.Msg($"Sending exp!! -{diff}");
            }
        }
    }
}