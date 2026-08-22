using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;

namespace CMS21_Together_Server.Network.Handlers
{
    public static class InventoryHandlers
    {
        [PacketHandler(PacketTypes.InventoryItemAction)]
        public static void HandleInventoryItemAction(long clientId, InventoryItemActionPacket packet)
        {
            var state = GameDataManager.CurrentState;
            if (packet.Action == ItemActionType.Add)
            {
                state.InventoryState.InventoryItems.Add(packet.Item);
                Server.SendToClients(packet, (int)clientId);
            }
            else if (packet.Action == ItemActionType.Remove)
            {
                var removed = state.InventoryState.InventoryItems.RemoveAll(i => i.UID == packet.Item.UID) > 0;
                if (removed)
                {
                    Server.SendToClients(packet, (int)clientId);
                }
            }
        }

        [PacketHandler(PacketTypes.InventoryGroupItemAction)]
        public static void HandleInventoryGroupItemAction(long clientId, InventoryGroupItemActionPacket packet)
        {
            var state = GameDataManager.CurrentState;
            if (packet.Action == ItemActionType.Add)
            {
                state.InventoryState.InventoryGroupItems.Add(packet.GroupItem);
                Server.SendToClients(packet, (int)clientId);
            }
            else if (packet.Action == ItemActionType.Remove)
            {
                var removed = state.InventoryState.InventoryGroupItems.RemoveAll(i => i.UID == packet.GroupItem.UID) > 0;
                if (removed)
                {
                    Server.SendToClients(packet, (int)clientId);
                }
            }
        }

        [PacketHandler(PacketTypes.WarehouseAction)]
        public static void HandleWarehouseAction(long clientId, WarehouseActionPacket packet)
        {
            var state = GameDataManager.CurrentState;
            
            if (packet.ToWarehouse)
            {
                if (packet.IsGroupItem)
                {
                    if (state.InventoryState.InventoryGroupItems.RemoveAll(i => i.UID == packet.GroupItem.UID) > 0)
                    {
                        state.InventoryState.WarehouseGroupItems.Add(packet.GroupItem);
                        Server.SendToClients(packet); // Send to all including sender so sender can update their local UI
                    }
                }
                else
                {
                    if (state.InventoryState.InventoryItems.RemoveAll(i => i.UID == packet.Item.UID) > 0)
                    {
                        state.InventoryState.WarehouseItems.Add(packet.Item);
                        Server.SendToClients(packet);
                    }
                }
            }
            else
            {
                // From warehouse to inventory
                if (packet.IsGroupItem)
                {
                    if (state.InventoryState.WarehouseGroupItems.RemoveAll(i => i.UID == packet.GroupItem.UID) > 0)
                    {
                        state.InventoryState.InventoryGroupItems.Add(packet.GroupItem);
                        Server.SendToClients(packet);
                    }
                }
                else
                {
                    if (state.InventoryState.WarehouseItems.RemoveAll(i => i.UID == packet.Item.UID) > 0)
                    {
                        state.InventoryState.InventoryItems.Add(packet.Item);
                        Server.SendToClients(packet);
                    }
                }
            }
        }
        
        public static long GenerateNewUID()
        {
            return DateTime.UtcNow.Ticks;
        }
    }
}
