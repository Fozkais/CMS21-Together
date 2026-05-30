using System.Net;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21Together.Logic;
using CMS21Together.Logic.Player;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Network.Handlers;

public static class AuthHandler
{
	[PacketHandler(PacketTypes.Heartbeat)]
	public static void HandleHeartbeat(long senderId, HeartbeatPacket packet)
	{
		if (!Client.Instance.IsConnectionValid)
			Client.Instance.OnConnectionValidated.Invoke();
		Client.Instance.Send(new HeartbeatPacket(), false);
	}
	
	[PacketHandler(PacketTypes.Connect)]
	public static void HandleConnect(long senderId, ConnectPacket packet)
	{
		MelonLogger.Msg($"Server compatible with mod version {packet.modVersion}");
		MelonLogger.Msg($"Received message from server: {packet.message}");
		Client.Instance.ID = packet.playerID;
		if (Client.Instance.NetworkType == NetworkType.DirectIP)
		{
			Client.Instance.UDP.Connect(((IPEndPoint)Client.Instance.Tcp.socket.Client.LocalEndPoint).Port);
			Client.Instance.Send(new ConnectPacket()
			{
				gameVersion = "",
				message = "",
				modVersion = MainMod.ASSEMBLY_MOD_VERSION,
				playerID = Client.Instance.ID,
				username = $"TestUser{packet.playerID}"
			});
		}
	}
	
	[PacketHandler(PacketTypes.Disconnect)]
	public static void HandleDisconnect(long senderId, DisconnectPacket packet)
	{
		if (packet.playerID == Client.Instance.ID || packet.playerID == -1)
		{
			MelonLogger.Msg($"[Received From Server] Disconnected from server : {packet.message}");
			Client.Instance.Disconnect();
			NotificationCenter.m_instance.StartCoroutine(NotificationCenter.m_instance.SelectSceneToLoad("Menu", SceneType.Menu, true, false));
		}
		else
		{
			if (ClientData.Players.TryGetValue(packet.playerID, out PlayerInstance instance))
			{
				MelonLogger.Msg($"Player {packet.playerID} disconnected.");
				Object.Destroy(instance.gameObject);
				ClientData.Players.Remove(packet.playerID);
			}
		}
	}
}