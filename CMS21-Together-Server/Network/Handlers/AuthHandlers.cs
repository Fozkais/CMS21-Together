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
	}
}