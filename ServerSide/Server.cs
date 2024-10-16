using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;

namespace CMS21Together.ServerSide;

//[RegisterTypeInIl2Cpp]
public class Server 
{
	public delegate void packetHandler(int fromClient, Packet packet);

	public static Server Instance;
	public static Dictionary<int, packetHandler> packetHandlers;
	public NetworkType networkType;

	public bool isRunning;

	public Dictionary<int, ServerConnection> clients = new();
	public SteamSocket steam;
	public TcpListener tcp;
	public UdpClient udp;

	public void StartServer(NetworkType type)
	{
		networkType = type;
		ServerData.Instance = new ServerData();
		InitializeServerData();
		StartServer();
	}

	private void StartServer()
	{
		if (networkType == NetworkType.Steam)
		{
			//  steam = SteamNetworkingSockets.CreateRelaySocket<SteamSocket>();
			//  steam.serverID = SteamworksUtils.EncodeSteamID(SteamManager.Instance.clientData.PlayerSteamId);

			tcp = new TcpListener(IPAddress.Any, MainMod.PORT);
			tcp.Start();
			tcp.BeginAcceptTcpClient(TCPConnectCallback, null);

			udp = new UdpClient(MainMod.PORT);
			udp.BeginReceive(UDPReceiveCallback, null);

			Client.Instance.ConnectToServer(NetworkType.TCP, "127.0.0.1");
		}
		else if (networkType == NetworkType.TCP)
		{
			tcp = new TcpListener(IPAddress.Any, MainMod.PORT);
			tcp.Start();
			tcp.BeginAcceptTcpClient(TCPConnectCallback, null);

			udp = new UdpClient(MainMod.PORT);
			udp.BeginReceive(UDPReceiveCallback, null);

			Client.Instance.ConnectToServer(NetworkType.TCP, "127.0.0.1");
		}

		Application.runInBackground = true;
		isRunning = true;
		MelonLogger.Msg("[Server->StartServer] Server started Succefully.");
	}

	public void CloseServer()
	{
		if (!isRunning) return;

		foreach (var id in clients.Keys) ServerSend.DisconnectPacket(id, "Server is shutting down.");
		isRunning = false;
		Application.runInBackground = false;

		if (udp != null)
			udp.Close();
		if (tcp != null)
			tcp.Stop();
		//if (steam != null)
		//  steam.Close();

		if (clients != null)
			clients.Clear();
		if (packetHandlers != null)
			packetHandlers.Clear();

		MelonLogger.Msg("[Server->CloseServer] Server Closed.");
	}

	private void UDPReceiveCallback(IAsyncResult result)
	{
		if (!isRunning) return;
		try
		{
			var receivedIP = new IPEndPoint(IPAddress.Any, 0);
			var _data = udp.EndReceive(result, ref receivedIP);
			udp.BeginReceive(UDPReceiveCallback, null);

			if (_data.Length < 4)
				return;

			using (var _packet = new Packet(_data))
			{
				var _clientId = _packet.ReadInt();
				if (_clientId == 0)
					return;

				if (clients[_clientId].udp.endPoint == null)
				{
					MelonLogger.Msg("[UDPReceiveCallback]Connecting Client.");
					clients[_clientId].Connect(receivedIP);
					return;
				}

				if (clients[_clientId].udp.endPoint.ToString() == receivedIP.ToString())
					clients[_clientId].udp.HandleData(_packet);
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Msg($"[Server->UDPReceiveCallback] Error receiving UDP data: {ex}");
		}
	}

	private void TCPConnectCallback(IAsyncResult result)
	{
		if (!isRunning) return;

		var _client = tcp.EndAcceptTcpClient(result);
		tcp.BeginAcceptTcpClient(TCPConnectCallback, null);

		MelonLogger.Msg($"[Server->TCPConnectCallback] Incoming connection from {_client.Client.RemoteEndPoint}...");

		foreach (var ClientID in clients.Keys)
			if (clients[ClientID].isConnected == false)
			{
				clients[ClientID].Connect(_client);
				MelonLogger.Msg($"[Server->TCPConnectCallback] Connecting client with id:{ClientID}.");
				return;
			}

		MelonLogger.Warning($"[Server->TCPConnectCallback] {_client.Client.RemoteEndPoint} failed to connect: Server full!");
	}

	private void InitializeServerData()
	{
		for (var i = 1; i <= MainMod.MAX_PLAYER; i++) clients.Add(i, new ServerConnection(i));

		packetHandlers = new Dictionary<int, packetHandler>
		{
			{ (int)PacketTypes.connect, ServerHandle.ConnectValidationPacket },
			{ (int)PacketTypes.disconnect, ServerHandle.DisconnectPacket },
			{ (int)PacketTypes.readyState, ServerHandle.ReadyPacket },

			{ (int)PacketTypes.position, ServerHandle.PositionPacket },
			{ (int)PacketTypes.rotation, ServerHandle.RotationPacket },
			{ (int)PacketTypes.sceneChange, ServerHandle.SceneChangePacket },

			{ (int)PacketTypes.item, ServerHandle.ItemPacket },
			{ (int)PacketTypes.groupItem, ServerHandle.GroupItemPacket },

			{ (int)PacketTypes.stat, ServerHandle.StatPacket },

			{ (int)PacketTypes.lifter, ServerHandle.LifterPacket },
			{ (int)PacketTypes.setSpringClamp, ServerHandle.SetSpringClampPacket },
			{ (int)PacketTypes.clearSpringClamp, ServerHandle.SpringClampClearPacket },
			{ (int)PacketTypes.setTireChanger, ServerHandle.SetTireChangerPacket },
			{ (int)PacketTypes.clearTireChanger, ServerHandle.ClearTireChangerPacket },
			{ (int)PacketTypes.setWheelBalancer, ServerHandle.SetWheelBalancerPacket },
			{ (int)PacketTypes.balanceWheel, ServerHandle.WheelBalancePacket },
			{ (int)PacketTypes.removeTireWB, ServerHandle.WheelRemovePacket },
			{ (int)PacketTypes.toolMove, ServerHandle.ToolsMovePacket },

			{ (int)PacketTypes.loadJobCar, ServerHandle.LoadJobCarPacket },
			{ (int)PacketTypes.loadCar, ServerHandle.LoadCarPacket },
			{ (int)PacketTypes.bodyPart, ServerHandle.BodyPartPacket },
			{ (int)PacketTypes.partScript, ServerHandle.PartScriptPacket },

			{ (int)PacketTypes.deleteCar, ServerHandle.DeleteCarPacket },
			{ (int)PacketTypes.carPosition, ServerHandle.CarPositionPacket },

			{ (int)PacketTypes.garageUpgrade, ServerHandle.GarageUpgradePacket },
			{ (int)PacketTypes.newJob, ServerHandle.JobPacket },
			{ (int)PacketTypes.jobAction, ServerHandle.JobActionPacket },
			{ (int)PacketTypes.selectedJob, ServerHandle.SelectedJobPacket },
			{ (int)PacketTypes.endJob, ServerHandle.EndJobPacket }
		};
	}
}