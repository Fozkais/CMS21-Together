using System;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;
using CMS21Together.Network;
using HarmonyLib;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class NotificationCenterItemsHook
    {
        [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SellItem), new Type[] { typeof(Item), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        public static bool SellItemPrefix(Item item, bool warehouse, bool fromInventory)
        {
            if (Client.Instance.IsConnected)
            {
                var packet = new ShopActionPacket
                {
                    Action = ShopActionType.SellSingle,
                    IsGroupItem = false,
                    ItemUID = item.UID
                };
                Client.Instance.Send(packet);
                return false;
            }
            return true;
        }
    

        [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SellItem), new Type[] { typeof(GroupItem), typeof(bool), typeof(bool) })]
        [HarmonyPrefix]
        public static bool SellGroupItemPrefix(GroupItem groupItem, bool warehouse, bool fromInventory)
        {
            if (Client.Instance.IsConnected)
            {
                var packet = new ShopActionPacket
                {
                    Action = ShopActionType.SellSingle,
                    IsGroupItem = true,
                    ItemUID = groupItem.UID
                };
                Client.Instance.Send(packet);
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.MoveItem), new Type[] { typeof(Item), typeof(bool), typeof(string) })]
        [HarmonyPrefix]
        public static bool MoveItemPrefix(Item itemToMove, bool toWarehouse, string windowType)
        {
            if (Client.Instance.IsConnected)
            {
                if (windowType == "Warehouse")
                {
                    var packet = new WarehouseActionPacket
                    {
                        ToWarehouse = toWarehouse,
                        IsGroupItem = false,
                        Item = itemToMove.ToModItem()
                    };
                    Client.Instance.Send(packet);
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.MoveItem), new Type[] { typeof(GroupItem), typeof(bool), typeof(string) })]
        [HarmonyPrefix]
        public static bool MoveGroupItemPrefix(GroupItem itemToMove, bool toWarehouse, string windowType)
        {
            if (Client.Instance.IsConnected)
            {
                if (windowType == "Warehouse")
                {
                    var packet = new WarehouseActionPacket
                    {
                        ToWarehouse = toWarehouse,
                        IsGroupItem = true,
                        GroupItem = itemToMove.ToModGroupItem()
                    };
                    Client.Instance.Send(packet);
                    return false;
                }
            }
            return true;
        }
    }
}
