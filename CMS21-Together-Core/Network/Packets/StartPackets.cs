using System;
using System.Collections.ObjectModel;
using CMS21_Together_Core.Data;

namespace CMS21_Together_Core.Network.Packets;


[Serializable]
[NetworkPacket(PacketTypes.Heartbeat)]
public class HeartbeatPacket : INetworkData { }


[Serializable]
[NetworkPacket(PacketTypes.Connect)]
public class ConnectPacket : INetworkData
{
	public int playerID;
	public string username;
	
	public string message;
	public string gameVersion;
	public string modVersion;
}

[Serializable]
[NetworkPacket(PacketTypes.Disconnect)]
public class DisconnectPacket : INetworkData
{
	public int playerID;
	public string message;
}