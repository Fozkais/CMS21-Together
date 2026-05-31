using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
public enum ItemActionType
{
    Add,
    Remove
}

[Serializable]
[NetworkPacket(PacketTypes.InventoryData)]
public class InventorySyncPacket : INetworkData
{
    public bool IsFirstBatch;
    public bool IsLastBatch;
    public List<ModItem> InventoryItems;
    public List<ModGroupItem> InventoryGroupItems;
    public List<ModItem> WarehouseItems;
    public List<ModGroupItem> WarehouseGroupItems;
}

[Serializable]
[NetworkPacket(PacketTypes.InventoryItemAction)]
public class InventoryItemActionPacket : INetworkData
{
    public ItemActionType Action;
    public ModItem Item;
}

[Serializable]
[NetworkPacket(PacketTypes.InventoryGroupItemAction)]
public class InventoryGroupItemActionPacket : INetworkData
{
    public ItemActionType Action;
    public ModGroupItem GroupItem;
}

[Serializable]
[NetworkPacket(PacketTypes.WarehouseAction)]
public class WarehouseActionPacket : INetworkData
{
    public bool IsGroupItem;
    public ModItem Item;
    public ModGroupItem GroupItem;
    public bool ToWarehouse; // True if moving from inventory to warehouse
}
