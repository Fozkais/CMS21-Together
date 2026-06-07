using System;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Data;
using CMS21Together.Network;
using CMS21Together.Network.Handlers;
using HarmonyLib;

namespace CMS21Together.Logic.Hook
{
    [HarmonyPatch]
    public static class InventoryHook
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Add), new Type[] { typeof(Item), typeof(bool) })]
        [HarmonyPrefix]
        public static bool AddItemPrefix(Item item, bool showPopup)
        {
            if (Client.Instance.IsConnected && !InventoryHandlers.IgnoreInventoryHooks)
            {
                var packet = new InventoryItemActionPacket
                {
                    Action = ItemActionType.Add,
                    Item = item.ToModItem()
                };
                Client.Instance.Send(packet);
            }
            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.Delete), new Type[] { typeof(Item) })]
        [HarmonyPrefix]
        public static bool DeleteItemPrefix(Item item)
        {
            if (Client.Instance.IsConnected && !InventoryHandlers.IgnoreInventoryHooks)
            {
                var packet = new InventoryItemActionPacket
                {
                    Action = ItemActionType.Remove,
                    Item = item.ToModItem()
                };
                Client.Instance.Send(packet);
            }
            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddGroup), new Type[] { typeof(GroupItem) })]
        [HarmonyPrefix]
        public static bool AddGroupItemPrefix(GroupItem group)
        {
            if (Client.Instance.IsConnected && !InventoryHandlers.IgnoreInventoryHooks)
            {
                var packet = new InventoryGroupItemActionPacket
                {
                    Action = ItemActionType.Add,
                    GroupItem = group.ToModGroupItem()
                };
                Client.Instance.Send(packet);
            }
            return true;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.DeleteGroup), new Type[] { typeof(long) })]
        [HarmonyPrefix]
        public static bool DeleteGroupItemPrefix(long UId)
        {
            if (Client.Instance.IsConnected && !InventoryHandlers.IgnoreInventoryHooks)
            {
                // We only need the UID to delete it
                var packet = new InventoryGroupItemActionPacket
                {
                    Action = ItemActionType.Remove,
                    GroupItem = new CMS21_Together_Core.Data.GameType.ModGroupItem { UID = UId }
                };
                Client.Instance.Send(packet);
            }
            return true;
        }
    }
}
