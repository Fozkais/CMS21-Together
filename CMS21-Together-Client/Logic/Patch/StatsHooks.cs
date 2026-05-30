using HarmonyLib;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;

namespace CMS21Together.Logic.Patch
{
    [HarmonyPatch]
    public static class StatsHooks
    {
        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerExp))]
        [HarmonyPrefix]
        public static bool AddPlayerExpPrefix(int exp, bool instant)
        {
            if (ClientData.IsServerUpdating) 
            {
                return true;
            }

            if (exp > 0)
            {
                Client.Instance.Send(new StatsActionPacket { ExpDelta = exp, ScrapsDelta = 0 });
            }
            return true; // Still allow local execution for prediction
        }

        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.AddPlayerScraps))]
        [HarmonyPrefix]
        public static bool AddPlayerScrapsPrefix(int amount)
        {
            if (ClientData.IsServerUpdating) return true;

            if (amount != 0)
            {
                Client.Instance.Send(new StatsActionPacket { ExpDelta = 0, ScrapsDelta = amount });
            }
            return true;
        }

        [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.SetPlayerScraps))]
        [HarmonyPrefix]
        public static bool SetPlayerScrapsPrefix(int scraps)
        {
            if (ClientData.IsServerUpdating) return true;

            int delta = scraps - GlobalData.PlayerScraps;
            if (delta != 0)
            {
                Client.Instance.Send(new StatsActionPacket { ExpDelta = 0, ScrapsDelta = delta });
            }
            return true;
        }
    }
}
