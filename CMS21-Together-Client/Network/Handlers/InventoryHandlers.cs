using CMS.UI;
using CMS.UI.Windows;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Client.Logic;
using MelonLoader;

namespace CMS21_Together_Client.Network.Handlers
{
    public static class InventoryHandlers
    {
        [PacketHandler(PacketTypes.InventoryItemAction)]
        public static void HandleInventoryItemAction(long clientId, InventoryItemActionPacket packet)
        {
            if (packet.Action == ItemActionType.Add)
            {
                Singleton<GameManager>.Instance.Inventory.Add(packet.Item.ToGameItem());
            }
            else if (packet.Action == ItemActionType.Remove)
            {
                var item = Singleton<GameManager>.Instance.Inventory.GetItem(packet.Item.UID);
                if (item != null)
                    Singleton<GameManager>.Instance.Inventory.Delete(item);
            }
            
            RefreshInventoryWindow();
        }

        [PacketHandler(PacketTypes.InventoryGroupItemAction)]
        public static void HandleInventoryGroupItemAction(long clientId, InventoryGroupItemActionPacket packet)
        {
            if (packet.Action == ItemActionType.Add)
            {
                Singleton<GameManager>.Instance.Inventory.AddGroup(packet.GroupItem.ToGameGroupItem());
            }
            else if (packet.Action == ItemActionType.Remove)
            {
                Singleton<GameManager>.Instance.Inventory.DeleteGroup(packet.GroupItem.UID);
            }
            
            RefreshInventoryWindow();
        }

        [PacketHandler(PacketTypes.WarehouseAction)]
        public static void HandleWarehouseAction(long clientId, WarehouseActionPacket packet)
        {
            if (packet.ToWarehouse)
            {
                // Remove from Inventory, Add to Warehouse
                if (packet.IsGroupItem)
                {
                    Singleton<GameManager>.Instance.Inventory.DeleteGroup(packet.GroupItem.UID); 
                    Singleton<GameManager>.Instance.Warehouse.Add(packet.GroupItem.ToGameGroupItem());
                }
                else
                {
                    var item = Singleton<GameManager>.Instance.Inventory.GetItem(packet.Item.UID);
                    if (item != null)
                        Singleton<GameManager>.Instance.Inventory.Delete(item);
                    Singleton<GameManager>.Instance.Warehouse.Add(packet.Item.ToGameItem());
                }
            }
            else
            {
                // From Warehouse to Inventory
                if (packet.IsGroupItem)
                {
                    var grp = packet.GroupItem.ToGameGroupItem();
                    Singleton<GameManager>.Instance.Warehouse.Delete(grp);
                    Singleton<GameManager>.Instance.Inventory.AddGroup(grp);
                }
                else
                {
                    var item = packet.Item.ToGameItem();
                    Singleton<GameManager>.Instance.Warehouse.Delete(item);
                    Singleton<GameManager>.Instance.Inventory.Add(item);
                }
            }
            
            RefreshInventoryWindow();
            RefreshWarehouseWindow();
        }

        private static void RefreshInventoryWindow()
        {
            if (WindowManager.Instance != null && WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory) != null)
            {
                WindowManager.Instance.GetWindowByID<InventoryWindow>(WindowID.Inventory).Refresh(true);
            }
        }
        
        private static void RefreshWarehouseWindow()
        {
            if (WindowManager.Instance != null && WindowManager.Instance.GetWindowByID<WarehouseWindow>(WindowID.Warehouse) != null)
            {
                WindowManager.Instance.GetWindowByID<WarehouseWindow>(WindowID.Warehouse).Refresh(true);
            }
        }
    }
}
