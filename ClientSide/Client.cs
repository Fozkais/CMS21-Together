using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Transports;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using Steamworks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CMS21Together.ClientSide;

[RegisterTypeInIl2Cpp]
public class Client : MonoBehaviour
{
    public static Client Instance;
    public bool isConnected;

    public delegate void PacketHandler(Packet _packet);
    public static Dictionary<int, PacketHandler> PacketHandlers;
    
    public NetworkType networkType;

    public ClientSteam steam;
    public ClientTCP tcp;
    public ClientUDP udp;

    public void Initialize()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            MelonLogger.Msg("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer(NetworkType type)
    {
        networkType = type;
        ClientData.Instance = new ClientData();
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        InitializeClientData();

        if (networkType == NetworkType.steam)
        {
            SteamId lobbyID = SteamworksUtils.ConvertLobbyID(ClientData.UserData.lobbyID);
            steam = SteamNetworkingSockets.ConnectRelay<ClientSteam>(lobbyID);
        }
        else if (networkType == NetworkType.tcp)
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
            case NetworkType.tcp:
                if(reliable) tcp.Send(packet);
                else udp.Send(packet);
                break;
            case NetworkType.steam:
                steam.Send(packet, reliable);
                break;
        }
    }

    private void InitializeClientData()
    {
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)PacketTypes.connect, ClientHandle.ConnectPacket },
            { (int)PacketTypes.disconnect, ClientHandle.DisconnectPacket },
            { (int)PacketTypes.userData, ClientHandle.UserDataPacket },
            
            { (int)PacketTypes.position, ClientHandle.PositionPacket },
            { (int)PacketTypes.rotation, ClientHandle.RotationPacket },
            
            { (int)PacketTypes.item, ClientHandle.ItemPacket },
            { (int)PacketTypes.groupItem, ClientHandle.GroupItemPacket },
        };
    }

    public void Disconnect()
    {
        if(!isConnected) return;

        Application.runInBackground = false;
        isConnected = false;
        
        tcp.Disconnect();
        udp.Disconnect();
        MelonLogger.Msg("[Client->Disconnect] Disconnected from server.");
        // ApiCalls.API_M2(ContentManager.Instance.OwnedContents); TODO: Fix

    }
}