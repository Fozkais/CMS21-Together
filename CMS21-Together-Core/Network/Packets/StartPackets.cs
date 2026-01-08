using System;
using System.Collections.ObjectModel;
using CMS21_Together_Core.Data;

namespace CMS21_Together_Core.Network.Packets;


[Serializable]
[NetworkPacket(PacketTypes.handshake)]
public class HandshakePacket : INetworkData
{
	public string username;
}


[Serializable]
[NetworkPacket(PacketTypes.connect)]
public class ConnectPacket : INetworkData
{
	public string playerGuid;
	public int playerID;
	public string username;
	
	public string message;
	public string gameVersion;
	public string modVersion;
}

[Serializable]
[NetworkPacket(PacketTypes.disconnect)]
public class DisconnectPacket : INetworkData
{
	public string message;
}