using CMS.UI;
using CMS.UI.Windows;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;
using MelonLoader;

namespace CMS21Together.Network.Handlers
{
    public static class ShopHandlers
    {
        [PacketHandler(PacketTypes.ShopAction)]
        public static void HandleShopAction(long clientId, ShopActionPacket packet)
        {
            if (!ClientData.IsGarageStateSynced) return;

            if (packet.Action == ShopActionType.Buy)
            {
                try
                {
                    if (packet.IsGroupItem)
                    {
                        var gameGrp = packet.GroupItemToBuy.ToGameGroupItem();
                        Singleton<GameManager>.Instance.Inventory.AddGroup(gameGrp);
                        MelonLogger.Msg($"[ShopHandlers] Successfully added GroupItem {gameGrp.ID} to local inventory.");
                    }
                    else
                    {
                        var gameItem = packet.ItemToBuy.ToGameItem();
                        Singleton<GameManager>.Instance.Inventory.Add(gameItem);
                        MelonLogger.Msg($"[ShopHandlers] Successfully added Item {gameItem.ID} to local inventory.");
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"[ShopHandlers] Error during ToGameItem conversion or Add: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else if (packet.Action == ShopActionType.SellSingle)
            {
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
                Singleton<GameManager>.Instance.Inventory.SellPerCondition(packet.SellCondition);
                MelonLogger.Msg($"[ShopHandlers] Received SellCondition override for {packet.SellCondition}");
            }
            
            try
            {
                if (WindowManager.Instance != null)
                {
                    var invWindow = WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory);
                    if (invWindow != null && invWindow.isActive)
                    {
                        invWindow.Refresh(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[ShopHandlers] Error refreshing InventoryWindow: {ex.Message}");
            }
        }

        [PacketHandler(PacketTypes.ItemsExchange)]
        public static void HandleItemsExchange(long clientId, ItemsExchangePacket packet)
        {
            if (!ClientData.IsGarageStateSynced) return;

            foreach (var item in packet.ItemsToBuy)
            {
                Singleton<GameManager>.Instance.Inventory.Add(item.ToGameItem());
            }
            MelonLogger.Msg($"[ShopHandlers] Received {packet.ItemsToBuy.Count} items from ItemsExchange (Junkyard).");
        }
    }
}
