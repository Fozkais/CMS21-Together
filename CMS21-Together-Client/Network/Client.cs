using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21Together.Network.Transport;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.Network;

public class Client
{
	public static Client Instance;
	public ClientTCP tcp;
	public ClientUDP udp;
	public ClientSteam steam;
	public int id;

	public bool isConnected { get; private set; }
	
	public static void Init()
	{
		Instance = new Client();
		Instance.tcp = new ClientTCP();
		Instance.udp = new ClientUDP();
	}

	public void ConnectToServer(string ip = "127.0.0.1")
	{
		tcp.Connect(ip, MainMod.PORT);
		Application.runInBackground = true;
		isConnected = true;
	}

	public void ConnectToSteamServer(string serverID)
	{
		if (ulong.TryParse(serverID, out ulong value))
		{
			steam = ClientSteam.ConnectToServer(value);
			isConnected = true;
		}
		else
		{
			MelonLogger.Msg("Invalid steamID");
		}
		Application.runInBackground = true;
	}
        
	public void SendToServer<T>(T packetData) where T : INetworkData
	{
		PacketTypes id = PacketRouter.GetPacketId(packetData);
		using (Packet packet = new Packet((int)id))
		{
			packet.Write(packetData);
			tcp.SendData(packet);
		}
	}

	public void Disconnect()
	{
		if (tcp.socket != null)
			tcp.Disconnect();
		if (steam.Connected)
			steam.Close();
		Application.runInBackground = false;
		isConnected = false;
		MelonLogger.Msg("Disconnected from server.");
	}
}