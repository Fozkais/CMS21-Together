using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;

namespace CMS21_Together_Server.Network.Handlers
{
	public static class AuthHandler
	{
		[PacketHandler(PacketTypes.handshake)]
		public static void OnHandshake(long clientId, HandshakePacket packet)
		{
			Console.WriteLine($"Reiceved handshake from {packet.username} (Client {clientId})");
		}
		
		[PacketHandler(PacketTypes.connect)]
		public static void OnConnected(long clientId, ConnectPacket packet)
		{
			Console.WriteLine($"Reiceved Connection callback from {packet.username}");
			Console.WriteLine($"Received info: {packet.modVersion}, {packet.username}, {packet.playerID}");
			
			if (packet.modVersion != Program.MOD_VERSION)
				Server.SendToClient(new DisconnectPacket()
				{
					message = $"Server require mod version {Program.MOD_VERSION}."
				},(int)clientId);
			
			Server.Clients[(int)clientId].OnConnectedSuccessfully.Invoke();
		}
	}
}