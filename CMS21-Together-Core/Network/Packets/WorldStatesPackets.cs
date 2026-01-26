using System;
using System.Collections.Generic;
using CMS21_Together_Core.Data.Enum;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
[NetworkPacket(PacketTypes.AskForSync)]
public class AskForSync : INetworkData { }

[Serializable]
[NetworkPacket(PacketTypes.WorldState)]
public class WorldState : INetworkData
{
	public Gamemode Gamemode;
	public int Money;
	public int Level;
	public int Exp;
}

[Serializable]
[NetworkPacket(PacketTypes.GarageState)]
public class GarageState : INetworkData
{
	public Dictionary<string, int> GarageUpgradeLevels = new Dictionary<string, int>();
	public Dictionary<string, int> PlayerUpgradeLevels = new Dictionary<string, int>();
}

[Serializable]
[NetworkPacket(PacketTypes.SyncEnd)]
public class SyncEnd : INetworkData { }