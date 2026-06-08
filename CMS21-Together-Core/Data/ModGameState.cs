using System;
using System.Collections.Generic;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Data;

public class ModGameState
{
	public WorldState WorldState = new WorldState();
	public GarageState GarageState = new GarageState();
	public InventoryState InventoryState = new InventoryState();
	public CarState CarState = new CarState();
	
	[NonSerialized] public PlayerState PlayerState = new PlayerState();
}

public class CarState
{
    // Key: CarLoaderID (e.g. 0 to 4), Value: CarSpawnResponsePacket
    // Using CarSpawnResponsePacket as the state object for simplicity, as it contains all info needed.
    public Dictionary<int, CarSpawnResponsePacket> LoadedCars = new Dictionary<int, CarSpawnResponsePacket>();
}