using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Transports;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using UnityEngine;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace CMS21Together.ClientSide;

//[RegisterTypeInIl2Cpp]
public class Client
{
	public delegate void PacketHandler(Packet _packet);

	public static Client Instance;
	public static Dictionary<int, PacketHandler> PacketHandlers;
	public bool isConnected;

	public NetworkType networkType;

	public ClientSteam steam;
	public ClientTCP tcp;
	public ClientUDP udp;

	public void ConnectToServer(NetworkType type, string ip = "")
	{
		networkType = type;
		ClientData.Instance = new ClientData();
		ConnectToServer(ip);
	}

	private void ConnectToServer(string ip = "")
	{
		InitializeClientData();

		/*if (networkType == NetworkType.Steam)
		{
		    SteamId lobbyID = SteamworksUtils.DecodeSteamID(ClientData.UserData.lobbyID);
		    steam = SteamNetworkingSockets.ConnectRelay<ClientSteam>(lobbyID);
		}
		else*/
		if (networkType == NetworkType.TCP)
		{
			tcp = new ClientTCP();
			udp = new ClientUDP();

			tcp.Connect();
		}

		isConnected = true;
	}

	public void SendData(Packet packet, bool reliable)
	{
		switch (networkType)
		{
			case NetworkType.TCP:
				if (reliable) tcp.Send(packet);
				else udp.Send(packet);
				break;
			/*case NetworkType.Steam:
			    steam.Send(packet, reliable);
			    break;*/
		}
	}

	private void InitializeClientData()
	{
		PacketHandlers = new Dictionary<int, PacketHandler>
		{
			{ (int)PacketTypes.connect, ClientHandle.ConnectPacket },
			{ (int)PacketTypes.disconnect, ClientHandle.DisconnectPacket },
			{ (int)PacketTypes.userData, ClientHandle.UserDataPacket },
			{ (int)PacketTypes.readyState, ClientHandle.ReadyPacket },
			{ (int)PacketTypes.start, ClientHandle.StartPacket },
			{ (int)PacketTypes.contentInfo, ClientHandle.ContentsInfoPacket },

			{ (int)PacketTypes.position, ClientHandle.PositionPacket },
			{ (int)PacketTypes.rotation, ClientHandle.RotationPacket },
			{ (int)PacketTypes.sceneChange, ClientHandle.SceneChangePacket },

			{ (int)PacketTypes.item, ClientHandle.ItemPacket },
			{ (int)PacketTypes.groupItem, ClientHandle.GroupItemPacket },

			{ (int)PacketTypes.stat, ClientHandle.StatPacket },

			{ (int)PacketTypes.lifter, ClientHandle.LifterPacket },
			{ (int)PacketTypes.setSpringClamp, ClientHandle.SetSpringClampPacket },
			{ (int)PacketTypes.clearSpringClamp, ClientHandle.SpringClampClearPacket },
			{ (int)PacketTypes.setTireChanger, ClientHandle.SetTireChangerPacket },
			{ (int)PacketTypes.clearTireChanger, ClientHandle.ClearTireChangerPacket },
			{ (int)PacketTypes.setWheelBalancer, ClientHandle.SetWheelBalancerPacket },
			{ (int)PacketTypes.balanceWheel, ClientHandle.WheelBalancePacket },
			{ (int)PacketTypes.removeTireWB, ClientHandle.WheelRemovePacket },
			{ (int)PacketTypes.toolMove, ClientHandle.ToolsMovePacket },

			{ (int)PacketTypes.loadCar, ClientHandle.LoadCarPacket },
			{ (int)PacketTypes.bodyPart, ClientHandle.BodyPartPacket },
			{ (int)PacketTypes.partScript, ClientHandle.PartScriptPacket },

			{ (int)PacketTypes.deleteCar, ClientHandle.DeleteCarPacket },
			{ (int)PacketTypes.carPosition, ClientHandle.CarPositionPacket },

			{ (int)PacketTypes.garageUpgrade, ClientHandle.GarageUpgradePacket },
			{ (int)PacketTypes.newJob, ClientHandle.JobPacket },
			{ (int)PacketTypes.jobAction, ClientHandle.JobActionPacket },
			{ (int)PacketTypes.selectedJob, ClientHandle.SelectedJobPacket },
			{ (int)PacketTypes.endJob, ClientHandle.EndJobPacket }
		};
	}

	public void Disconnect()
	{
		if (!isConnected) return;

		Application.runInBackground = false;
		isConnected = false;

		tcp.Disconnect();
		udp.Disconnect();

		if (SceneManager.GetActiveScene().name != "Menu")
		{
			var manager = NotificationCenter.m_instance;
			manager.StartCoroutine(manager.SelectSceneToLoad("Menu", SceneType.Menu, true, true));
		}

		MelonLogger.Msg("[Client->Disconnect] Disconnected from server.");
		ApiCalls.API_M2(ContentManager.Instance.ownedContents);
	}
}