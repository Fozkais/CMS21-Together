using System;

namespace CMS21_Together_Core;

[Serializable]
public enum PacketTypes
{
	Connect,
	Heartbeat,
	Disconnect,
	
	// World State Sync
	AskForSync,
	WorldState, // Gamemode, Money, Lvl/Exp
	GarageState, // Garage Upgrade, Garage Customization
	CarData, // Individual carInfo (sent 1 time for every carLoader)
	InventoryData, // Item & GroupItem
	SyncEnd, // Signal that initial load is complete
	
	Movement, // Position/Velocity & Rotation
	UpgradeRequest, // Garage/Skill upgrade request
	StatsAction, // Exp and Scrap deltas
	
	InventoryItemAction,
	InventoryGroupItemAction,
	WarehouseAction,
	ShopAction,
	ItemsExchange,
	RegisterModItem,
	
	// Car Packets
	CarSpawnRequest,
	CarSpawnResponse,
	CarSpawnDelete,
	CarSpawnRejected
}