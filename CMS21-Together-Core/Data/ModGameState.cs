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
	
	[NonSerialized] public PlayerState PlayerState = new PlayerState();
}