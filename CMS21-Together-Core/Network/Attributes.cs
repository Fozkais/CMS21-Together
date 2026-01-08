using System;

namespace CMS21_Together_Core.Network;

[AttributeUsage(AttributeTargets.Class)]
public class NetworkPacket : Attribute
{
	public PacketTypes Type { get; }
	public NetworkPacket(PacketTypes type) => Type = type;
}

[AttributeUsage(AttributeTargets.Method)]
public class PacketHandler : Attribute
{
	public PacketTypes Type { get; }
	public PacketHandler(PacketTypes type) => Type = type;
}

public interface INetworkData { }