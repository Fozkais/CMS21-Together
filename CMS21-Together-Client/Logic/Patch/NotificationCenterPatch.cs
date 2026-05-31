using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;
using CMS21_Together_Core.Network.Packets;
using HarmonyLib;
using CMS21Together.Network;
using CMS21_Together_Client.Logic;

namespace CMS21_Together_Client.Logic.Patch
{
    [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SellItem), new Type[] { typeof(Item), typeof(bool), typeof(bool) })]
    public static class NotificationCenter_SellItem_Item_Patch
    {
        public static bool Prefix(Item item, bool warehouse, bool fromInventory)
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
    }

    [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SellItem), new Type[] { typeof(GroupItem), typeof(bool), typeof(bool) })]
    public static class NotificationCenter_SellItem_GroupItem_Patch
    {
        public static bool Prefix(GroupItem groupItem, bool warehouse, bool fromInventory)
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
    }

    [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.MoveItem), new Type[] { typeof(Item), typeof(bool), typeof(string) })]
    public static class NotificationCenter_MoveItem_Item_Patch
    {
        public static bool Prefix(Item itemToMove, bool toWarehouse, string windowType)
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
                else if (windowType == "ItemsExchange")
                {
                    if (toWarehouse)
                    {
                        var packet = new ItemsExchangePacket
                        {
                            ItemsToBuy = new List<ModItem> { itemToMove.ToModItem() }
                        };
                        Client.Instance.Send(packet);
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.MoveItem), new Type[] { typeof(GroupItem), typeof(bool), typeof(string) })]
    public static class NotificationCenter_MoveItem_GroupItem_Patch
    {
        public static bool Prefix(GroupItem itemToMove, bool toWarehouse, string windowType)
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
                else if (windowType == "ItemsExchange")
                {
                    if (toWarehouse)
                    {
                        var packet = new ItemsExchangePacket
                        {
                            ItemsToBuy = new List<ModItem>()
                        };
                        foreach (var item in itemToMove.ItemList)
                        {
                            packet.ItemsToBuy.Add(item.ToModItem());
                        }
                        Client.Instance.Send(packet);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
