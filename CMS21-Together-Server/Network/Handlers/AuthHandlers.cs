using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;

namespace CMS21_Together_Server.Network.Handlers
{
	public static class AuthHandler
	{
		[PacketHandler(PacketTypes.Heartbeat)]
		public static void OnHeartbeat(long clientId, HeartbeatPacket packet)
		{
			Logger.Debug($"Reiceved heartbeat from Client {clientId}");
			Server.Clients[(int)clientId].LastHeartbeatTime = ServerTime.Time;
		}
		
		[PacketHandler(PacketTypes.Connect)]
		public static void OnConnected(long clientId, ConnectPacket packet)
		{
			Logger.Debug($"Reiceved Connection callback from {packet.username}");
			Logger.Debug($"Received info: {packet.modVersion}, {packet.username}, {packet.playerID}");

			if (packet.modVersion != Program.MOD_VERSION)
			{
				Server.SendToClient(new DisconnectPacket()
				{
					message = $"Server require mod version {Program.MOD_VERSION}."
				},(int)clientId);
				return;
			}
			Server.Clients[(int)clientId].OnConnectedSuccessfully.Invoke();
		}
	}
}