using System;
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
}

[Serializable]
[NetworkPacket(PacketTypes.GarageState)]
public class GarageState : INetworkData
{
	
}

[Serializable]
[NetworkPacket(PacketTypes.SyncEnd)]
public class SyncEnd : INetworkData { }