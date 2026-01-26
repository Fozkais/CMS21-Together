using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;

namespace CMS21_Together_Server.Network.Handlers
{
	public static class PlayerHandlers
	{
		[PacketHandler(PacketTypes.Movement)]
		public static void OnMovementUpdate(long clientId, MovementPacket packet)
		{
			GameDataManager.CurrentState.PlayerState.Positions[(int)clientId] = packet.Position;
			GameDataManager.CurrentState.PlayerState.Velocities[(int)clientId] = packet.Velocity;
			GameDataManager.CurrentState.PlayerState.Rotations[(int)clientId] = packet.Rotation;

			packet.SenderId = (int)clientId;
			Server.SendToClients(packet, (int)clientId, false);
		}
	}
}