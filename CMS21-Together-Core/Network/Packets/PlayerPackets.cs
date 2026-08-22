using System;
using CMS21_Together_Core.Data.GameType;

namespace CMS21_Together_Core.Network.Packets;


[Serializable]
[NetworkPacket(PacketTypes.Movement)]
public class MovementPacket : INetworkData
{
	public int SenderId;
	public Vector3Serializable Position;
	public Vector3Serializable Velocity;
	public QuaternionSerializable Rotation;
	
	public float CameraPitch;
	public bool IsGrounded;
	public bool IsCrouching;
	public bool IsRunning;
}