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

		[PacketHandler(PacketTypes.AskForSync)]
		public static void OnAskForSync(long clientId, AskForSync packet)
		{
			Server.SendToClient(ServerGameState.CurrentState.WorldState, (int)clientId);
			Server.SendToClient(ServerGameState.CurrentState.GarageState, (int)clientId);
			Server.SendToClient(new SyncEnd(), (int)clientId);
		}
	}
}