using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network;
using CMS21Together.Data;
using CMS21Together.Network.Transport;
using MelonLoader;
using Steamworks;
using UnityEngine;

namespace CMS21Together.Network;

public class Client
{
	public static Client Instance;
	public ClientTCP Tcp;
	public ClientUDP UDP;
	public ClientSteam Steam;
	public int ID;

	public NetworkType NetworkType;
	public ulong ServerID = 85568392935755356;

	public bool IsConnected { get; private set; }
	public bool IsConnectionValid;
	public Action OnConnectionValidated;
	
	public static void Init()
	{
		Instance = new Client();
		Instance.IsConnected = false;
		Instance.Tcp = new ClientTCP();
		Instance.UDP = new ClientUDP();
	}

	public void ConnectToServer(string ip = "127.0.0.1")
	{
		if (IsConnected) return;
		
		NetworkType = NetworkType.DirectIP;
		Tcp.Connect(ip, MainMod.PORT);
		Application.runInBackground = true;
		OnConnectionValidated += OnConnectionSuccessful;
		IsConnected = true;
	}

	public void ConnectToSteamServer()
	{
		if (IsConnected) return;
		
		NetworkType = NetworkType.Steam;
		SteamNetworkingUtils.DebugLevel = NetDebugOutput.Error;
		IsConnected = true;
		Steam = ClientSteam.ConnectToServer(ServerID);
		OnConnectionValidated += OnConnectionSuccessful;
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
					Tcp.SendData(packet);
				else
					UDP.SendData(packet);
			}
			else
				Steam.Send(packet, reliable);
		}
	}
	
	private void OnConnectionSuccessful()
	{
		IsConnectionValid = true;
		
		ModGameManager.StartGame();
	}

	public void Disconnect()
	{
		if (Tcp.socket != null)
			Tcp.Disconnect();
		if (Steam.Connected)
			Steam.Close();
		Application.runInBackground = false;
		IsConnected = false;
		MelonLogger.Msg("Disconnected from server.");
	}
}