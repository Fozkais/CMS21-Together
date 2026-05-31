using CMS.UI;
using CMS.UI.Windows;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Client.Logic;
using MelonLoader;

namespace CMS21_Together_Client.Network.Handlers
{
    public static class ShopHandlers
    {
        [PacketHandler(PacketTypes.ShopAction)]
        public static void HandleShopAction(long clientId, ShopActionPacket packet)
        {
            if (packet.Action == ShopActionType.Buy)
            {
                if (packet.IsGroupItem)
                {
                    Singleton<GameManager>.Instance.Inventory.AddGroup(packet.GroupItemToBuy.ToGameGroupItem());
                }
                else
                {
                    Singleton<GameManager>.Instance.Inventory.Add(packet.ItemToBuy.ToGameItem());
                }
                MelonLogger.Msg($"[ShopHandlers] Received Bought Item from server.");
            }
            else if (packet.Action == ShopActionType.SellSingle)
            {
                // Delete the item by UID
                var item = Singleton<GameManager>.Instance.Inventory.GetItem(packet.ItemUID);
                if (item != null)
                {
                    Singleton<GameManager>.Instance.Inventory.Delete(item);
                }
                else
                {
                    Singleton<GameManager>.Instance.Inventory.DeleteGroup(packet.ItemUID);
                }
                MelonLogger.Msg($"[ShopHandlers] Received SellSingle override for UID {packet.ItemUID}");
            }
            else if (packet.Action == ShopActionType.SellCondition)
            {
                // The server removed items below condition, but the client should just call its own SellPerCondition?
                // No, the client needs to actually delete them, but wait, the server doesn't broadcast WHICH items were sold.
                // Wait! Server DOES broadcast ShopActionPacket with Action = SellCondition!
                // If the client receives SellCondition, it could just call its own local SellPerCondition, BUT that would cause desync if client and server have different items!
                // Actually, the server should probably just sync the whole inventory, or we can iterate and delete locally.
                Singleton<GameManager>.Instance.Inventory.SellPerCondition(packet.SellCondition);
                MelonLogger.Msg($"[ShopHandlers] Received SellCondition override for {packet.SellCondition}");
            }
            
            // Refresh Inventory Window if open
            if (WindowManager.Instance != null && WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory) != null)
            {
                WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory).Refresh(true);
            }
        }

        [PacketHandler(PacketTypes.ItemsExchange)]
        public static void HandleItemsExchange(long clientId, ItemsExchangePacket packet)
        {
            foreach (var item in packet.ItemsToBuy)
            {
                Singleton<GameManager>.Instance.Inventory.Add(item.ToGameItem());
            }
            MelonLogger.Msg($"[ShopHandlers] Received {packet.ItemsToBuy.Count} items from ItemsExchange (Junkyard).");
            
            // Refresh Warehouse/ItemsExchange Window if open
            if (WindowManager.Instance != null && WindowManager.Instance.GetWindowByID<ItemsExchangeWindow>(WindowID.ItemsExchange) != null)
            {
                WindowManager.Instance.GetWindowByID<ItemsExchangeWindow>(WindowID.ItemsExchange).Refresh(true);
            }
        }
    }
}
