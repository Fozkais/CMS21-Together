using System;
using CMS21_Together_Core.Data;

namespace CMS21_Together_Core.Network.Packets;

[Serializable]
[NetworkPacket(PacketTypes.movement)]
public class MovementPacket : INetworkData
{
	public Vector3Serializable position;
	public QuaternionSerializable rotation;
}