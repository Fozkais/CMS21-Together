using Il2Cpp;
using MelonLoader;

namespace CMS21MP.DataHandle.CL_Handle
{
    public class StatsHandle
    {
        public static void UpdateStats(int stats, bool status, int type)
        {
            if (status)
            {
                if (type == 0)
                {
                    MelonLogger.Msg($"Adding Money!! +{stats}");
                    GlobalData.AddPlayerMoney(stats);
                    ClientSide.Functionnality.Stats.moneyHandler = GlobalData.PlayerMoney;
                }
                else if (type == 1)
                {
                    MelonLogger.Msg($"Adding Scrap!! +{stats}");
                    GlobalData.AddPlayerScraps(stats);
                    ClientSide.Functionnality.Stats.scrapHandler = GlobalData.PlayerScraps;
                }
                else if (type == 2)
                {
                    MelonLogger.Msg($"Adding Exp!! +{stats}");
                    GlobalData.AddPlayerExp(stats);
                    ClientSide.Functionnality.Stats.ExperienceHandler = GlobalData.PlayerExp;
                }
                    
            }
            else
            {
                if (type == 0)
                {
                    MelonLogger.Msg($"Removing Money!! -{stats}");
                    GlobalData.AddPlayerMoney(stats);
                    ClientSide.Functionnality.Stats.moneyHandler = GlobalData.PlayerMoney;
                }
                else if (type == 1)
                {
                    MelonLogger.Msg($"Removing Scrap!! -{stats}");
                    GlobalData.AddPlayerScraps(stats);
                    ClientSide.Functionnality.Stats.scrapHandler = GlobalData.PlayerScraps;
                }
                else if (type == 2)
                {
                    MelonLogger.Msg($"Removing Exp!! -{stats}");
                    GlobalData.AddPlayerExp(stats);
                    ClientSide.Functionnality.Stats.ExperienceHandler = GlobalData.PlayerExp;
                }
            }
        }
    }
}