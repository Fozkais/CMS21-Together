using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using Steamworks;
using UnityEngine;

using SteamManager = CMS21Together.Shared.SteamManager;

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

	public string serverID;

	public void StartServer(NetworkType type)
	{
		serverID = null;
		networkType = type;
		ServerData.Instance = new ServerData();
		InitializeServerData();
		StartServer();
	}
	
	private void StartServer()
	{
		if (networkType == NetworkType.Steam)
		{
			steam = SteamNetworkingSockets.CreateRelaySocket<SteamSocket>();
			if (steam != null)
				MelonLogger.Msg($"[Server] Server is running with SteamID: {steam.GetServerID()}");
			else
				MelonLogger.Error("[Server] Failed to create RelaySocket.");
			
			tcp = new TcpListener(IPAddress.Any, MainMod.PORT); // launch tcp for host and maybe other client?
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

	public IEnumerator CloseServer()
	{
		if (!isRunning) yield break;

		MelonLogger.Msg("[Server->CloseServer] Saving players infos...");
		ModSaveData save = SavesManager.ModSaves[SavesManager.currentSaveIndex];
		foreach (var id in clients.Keys)
		{
			if (!ServerData.Instance.connectedClients.ContainsKey(id)) continue;
			
			string playerGuid = ServerData.Instance.connectedClients[id].playerGUID;
			PlayerInfo info = save.playerInfos.First(p => playerGuid == p.id);
			Vector3Serializable pos = ServerData.Instance.connectedClients[id].position;
			QuaternionSerializable rot = ServerData.Instance.connectedClients[id].rotation;
			int lvl = ServerData.Instance.connectedClients[id].playerExp;
			int exp = ServerData.Instance.connectedClients[id].playerLevel;
			int points = ServerData.Instance.connectedClients[id].playerSkillPoints;

			if (save.playerInfos.Any(p => playerGuid == p.id))
				info.UpdateStats(pos, rot, exp , lvl, points);
			if (id != 1) // dont send to host
				ServerSend.DisconnectPacket(id, "Server is shutting down.");
		}
		save.missionFinished = GlobalData.MissionsFinished;
		save.storyMissionInProgress = GlobalData.IsStoryMissionInProgress;
		
		SavesManager.SaveModSave(SavesManager.currentSaveIndex);
		yield return new WaitForSeconds(1);
		MelonLogger.Msg("[Server->CloseServer] Successfully Saved players infos!");
		
		isRunning = false;
		Application.runInBackground = false;
		if (udp != null)
			udp.Close();
		if (tcp != null)
			tcp.Stop();
		if (steam != null)
		  steam.Close();

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
			{ (int)PacketTypes.exp, ServerHandle.ExpPacket },
			{ (int)PacketTypes.point, ServerHandle.PointPacket },
			{ (int)PacketTypes.skillChange, ServerHandle.SkillChangePacket },
			{ (int)PacketTypes.garageUpgrade, ServerHandle.GarageUpgradePacket },
			{ (int)PacketTypes.resync, ServerHandle.ResyncPacket },

			{ (int)PacketTypes.lifter, ServerHandle.LifterPacket },
			{ (int)PacketTypes.setSpringClamp, ServerHandle.SetSpringClampPacket },
			{ (int)PacketTypes.clearSpringClamp, ServerHandle.SpringClampClearPacket },
			{ (int)PacketTypes.setTireChanger, ServerHandle.SetTireChangerPacket },
			{ (int)PacketTypes.clearTireChanger, ServerHandle.ClearTireChangerPacket },
			{ (int)PacketTypes.wheelBalance, ServerHandle.WheelBalancePacket },
			{ (int)PacketTypes.toolMove, ServerHandle.ToolsMovePacket },
			{ (int)PacketTypes.oilBinUse, ServerHandle.OilBin },
			{ (int)PacketTypes.engineCrane, ServerHandle.EngineCraneHandlePacket },
			{ (int)PacketTypes.engineStandSetGroup, ServerHandle.EngineStandSetGroupPacket },
			{ (int)PacketTypes.engineStandTakeOff, ServerHandle.EngineStandTakeOffPacket },
			{ (int)PacketTypes.engineStandAngle, ServerHandle.EngineStandAnglePacket },

			{ (int)PacketTypes.loadJobCar, ServerHandle.LoadJobCarPacket },
			{ (int)PacketTypes.loadCar, ServerHandle.LoadCarPacket },
			{ (int)PacketTypes.bodyPart, ServerHandle.BodyPartPacket },
			{ (int)PacketTypes.partScript, ServerHandle.PartScriptPacket },
			{ (int)PacketTypes.carFluid, ServerHandle.CarFluidPacket },

			{ (int)PacketTypes.deleteCar, ServerHandle.DeleteCarPacket },
			{ (int)PacketTypes.carPosition, ServerHandle.CarPositionPacket },

			{ (int)PacketTypes.newJob, ServerHandle.JobPacket },
			{ (int)PacketTypes.jobAction, ServerHandle.JobActionPacket },
			{ (int)PacketTypes.selectedJob, ServerHandle.SelectedJobPacket },
			{ (int)PacketTypes.endJob, ServerHandle.EndJobPacket }
		};
	}
}