using CMS.UI.Windows;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class SellPerConditionWindowHook
    {
        [HarmonyPatch(typeof(SellPerConditionWindow), nameof(SellPerConditionWindow.AcceptAction))]
        [HarmonyPrefix]
        public static bool Prefix(SellPerConditionWindow __instance)
        {
            if (Client.Instance.IsConnected)
            {
                var packet = new ShopActionPacket
                {
                    Action = ShopActionType.SellCondition,
                    SellCondition = __instance.currentSliderValue / 100f // In GameInventory.SellPerCondition, it converts the 1-100 to 0-1 (e.g., if condition <= slider / 100f)
                };
                Client.Instance.Send(packet);

                __instance.Hide(false);
                return false;
            }
            return true;
        }
    }
}
