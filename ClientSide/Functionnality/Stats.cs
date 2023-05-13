using CMS21MP.DataHandle;
using Il2Cpp;
using MelonLoader;

namespace CMS21MP.ClientSide.Functionnality
{
    public static class Stats
    {
        // Money
        public static int moneyHandler;
        public static int serverMoney;

        public static void HandleStats()
        {
            HandleMoney();
        }
        
        public static void HandleMoney()
        {
            int playerMoney = GlobalData.PlayerMoney;
            if (playerMoney > moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.PlayerMoney(diff, true);
                MelonLogger.Msg($"Sending money!! +{diff}");
            }
            else if (playerMoney < moneyHandler)
            {
                int diff = playerMoney - moneyHandler;
                moneyHandler += diff;
                ClientSend.PlayerMoney(diff, false);
                MelonLogger.Msg($"Sending money!! {diff}");
            }
        }
    }
}