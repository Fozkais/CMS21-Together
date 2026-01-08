using System.Net;
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
	
	[PacketHandler(PacketTypes.connect)]
	public static void HandleConnect(long senderId, ConnectPacket packet)
	{
		MelonLogger.Msg($"Server compatible with mod version {packet.modVersion}");
		MelonLogger.Msg($"Received message from server: {packet.message}");
		Client.Instance.id = packet.playerID;
		Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
		
		Client.Instance.SendToServer(new ConnectPacket()
		{
			gameVersion = "",
			message = "",
			playerGuid = "",
			modVersion = MainMod.ASSEMBLY_MOD_VERSION,
			playerID = Client.Instance.id,
			username = $"TestUser{packet.playerID}"
		});
	}
	
	[PacketHandler(PacketTypes.disconnect)]
	public static void HandleDisconnect(long senderId, DisconnectPacket packet)
	{
		MelonLogger.Msg($"[Received From Server] Disconnected from server : {packet.message}");
		Client.Instance.Disconnect();
	}
}