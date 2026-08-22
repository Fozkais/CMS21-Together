using System;
using System.Collections.Generic;
using CMS21_Together_Core.Network.Packets;

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

    // Per-loader last known state of each body part (key: PartIndex) and sub-part
    // (key: PartIndexPath joined as "0.1.2") - used both to broadcast live updates
    // and to replay the full car state to a client joining mid-session.
    public Dictionary<int, Dictionary<int, CarBodyPartUpdatePacket>> BodyParts = new Dictionary<int, Dictionary<int, CarBodyPartUpdatePacket>>();
    public Dictionary<int, Dictionary<string, CarSubPartUpdatePacket>> SubParts = new Dictionary<int, Dictionary<string, CarSubPartUpdatePacket>>();
}