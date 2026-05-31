using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
public enum ShopActionType
{
    Buy,
    SellSingle,
    SellCondition
}

[Serializable]
[NetworkPacket(PacketTypes.ShopAction)]
public class ShopActionPacket : INetworkData
{
    public ShopActionType Action;
    
    // For Buy
    public ModItem ItemToBuy;
    public ModGroupItem GroupItemToBuy;
    public bool IsGroupItem;

    // For SellSingle
    public long ItemUID; // UID of the item or group item to sell

    // For SellCondition
    public float SellCondition;
}

[Serializable]
[NetworkPacket(PacketTypes.ItemsExchange)]
public class ItemsExchangePacket : INetworkData
{
    public List<ModItem> ItemsToBuy;
}

[Serializable]
[NetworkPacket(PacketTypes.RegisterModItem)]
public class RegisterModItemPacket : INetworkData
{
    public PartProperty ItemProperty;
}
