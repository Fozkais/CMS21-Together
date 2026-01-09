using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Network;
using CMS21Together.Network.Transport;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace CMS21Together.Network;

public class Client
{
	public static Client Instance;
	public ClientTCP tcp;
	public ClientUDP udp;
	public ClientSteam steam;
	public int id;

	public NetworkType NetworkType;
	public ulong serverID = 90279787059618820;

	public bool isConnected { get; private set; }
	
	public static void Init()
	{
		Instance = new Client();
		Instance.isConnected = false;
		Instance.tcp = new ClientTCP();
		Instance.udp = new ClientUDP();
	}

	public void ConnectToServer(string ip = "127.0.0.1")
	{
		NetworkType = NetworkType.DirectIP;
		tcp.Connect(ip, MainMod.PORT);
		Application.runInBackground = true;
		isConnected = true;
	}

	public void ConnectToSteamServer()
	{
		NetworkType = NetworkType.Steam;
		SteamNetworkingUtils.DebugLevel = NetDebugOutput.Error;
		isConnected = true;
		steam = ClientSteam.ConnectToServer(serverID);
	
		Application.runInBackground = true;
	}
        
	public void Send<T>(T packetData, bool reliable=true) where T : INetworkData
	{
		PacketTypes id = PacketRouter.GetPacketId(packetData);
		using (Packet packet = new Packet((int)id))
		{
			packet.Write(packetData);
			packet.WriteLength();
			if (NetworkType == NetworkType.DirectIP)
			{
				if (reliable)
					tcp.SendData(packet);
				else
					udp.SendData(packet);
			}
			else
				steam.Send(packet, reliable);
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