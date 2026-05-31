using CMS.UI.Windows;
using CMS21_Together_Core.Network.Packets;
using HarmonyLib;
using CMS21_Together_Client.Network;

namespace CMS21_Together_Client.Logic.Patch
{
    [HarmonyPatch(typeof(SellPerConditionWindow), "AcceptAction")]
    public static class SellPerConditionWindow_AcceptAction_Patch
    {
        public static bool Prefix(SellPerConditionWindow __instance, float ___currentSliderValue)
        {
            if (Client.Instance.isConnected)
            {
                var packet = new ShopActionPacket
                {
                    Action = ShopActionType.SellCondition,
                    SellCondition = ___currentSliderValue / 100f // In GameInventory.SellPerCondition, it converts the 1-100 to 0-1 (e.g., if condition <= slider / 100f)
                };
                Client.Instance.Send(packet);

                __instance.Hide(false);
                return false;
            }
            return true;
        }
    }
}
