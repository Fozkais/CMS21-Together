using CMS.UI.Windows;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;

namespace CMS21Together.Logic.Patch
{
    [HarmonyPatch(typeof(ShopBuyWindow), nameof(ShopBuyWindow.BuyItem))]
    public static class ShopBuyWindow_BuyItem_Patch
    {
        public static bool Prefix(ShopBuyWindow __instance, 
            ref string ___itemID, 
            ref int ___currentAmount,
            ref int ___currentWidth,
            ref int ___currentSize,
            ref int ___currentProfile,
            ref int ___currentET,
            ref float ___currentPrice)
        {
            if (Client.Instance.IsConnected)
            {
                // Create a ModItem representation
                ModItem modItem = new ModItem();
                modItem.ID = ___itemID;
                modItem.Condition = 1f;
                
                // Add Wheel properties
                modItem.WheelData = new ModWheelData
                {
                    Width = ___currentWidth,
                    Size = ___currentSize,
                    Profile = ___currentProfile,
                    ET = ___currentET
                };

                // Send 1 packet per amount (or a bulk buy packet if we adapt it later)
                // For now we just loop and send packets since ShopActionPacket takes 1 Item
                for (int i = 0; i < ___currentAmount; i++)
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
