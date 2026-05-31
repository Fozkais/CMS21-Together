using CMS.UI.Windows;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;
using MelonLoader;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class ShopBuyWindowHook
    {
        [HarmonyPatch(typeof(ShopBuyWindow), nameof(ShopBuyWindow.BuyItem))]
        [HarmonyPrefix]
        public static bool BuyItemPrefix(ShopBuyWindow __instance)
        {
            if (Client.Instance.IsConnected)
            {
                ModItem modItem = new ModItem();
                modItem.ID = __instance.itemID;
                modItem.Condition = 1f;
                modItem.ConditionToShow = 1f;
                modItem.Dent = 1f;
                modItem.WashFactor = 1f;
                
                MelonLogger.Msg($"[ShopBuyWindowHook] Sending buy packet for {modItem.ID} (Amount: {__instance.currentAmount})");
                
                // Add Wheel properties
                modItem.WheelData = new ModWheelData
                {
                    Width = __instance.currentWidth,
                    Size = __instance.currentSize,
                    Profile = __instance.currentProfile,
                    ET = __instance.currentET
                };

                // Send 1 packet per amount (or a bulk buy packet if we adapt it later)
                // For now we just loop and send packets since ShopActionPacket takes 1 Item
                for (int i = 0; i < __instance.currentAmount; i++)
                {
                    var packet = new ShopActionPacket
                    {
                        Action = ShopActionType.Buy,
                        IsGroupItem = false,
                        ItemToBuy = modItem
                    };
                    
                    Client.Instance.Send(packet);
                }

                // Close the buy window just like the original logic did
                __instance.Hide(false);

                // We return false to prevent the local logic from adding the item and subtracting money locally.
                // The server will handle it and broadcast the inventory update and money update.
                return false;
            }
            
            return true;
        }
    }
}
