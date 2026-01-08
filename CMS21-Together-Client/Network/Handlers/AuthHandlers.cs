using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using MelonLoader;

namespace CMS21Together.Network.Handlers;

public static class AuthHandler
{
	[PacketHandler(PacketTypes.handshake)]
	public static void HandleHandshake(long senderId, HandshakePacket packet)
	{
		MelonLogger.Msg($"[Received From Server] Message: {packet.username}");
	}
	
	[PacketHandler(PacketTypes.disconnect)]
	public static void HandleDisconnect(long senderId, DisconnectPacket packet)
	{
		MelonLogger.Msg($"[Received From Server] Disconnected from server : {packet.message}");
		Client.Instance.Disconnect();
	}
}