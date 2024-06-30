using System.Collections.Generic;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Transports;
using CMS21Together.Shared.Data;
using MelonLoader;
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

    public void ConnectToServer()
    {
        InitializeClientData();

        if (networkType == NetworkType.steam)
        {
            // add when steamNetworking is implemented
        }
        else if (networkType == NetworkType.tcp)
        {
            tcp = new ClientTCP();
            udp = new ClientUDP();

            tcp.Connect();
        }

        isConnected = true;
    }

    private void InitializeClientData()
    {
        PacketHandlers = new Dictionary<int, PacketHandler>()
        {
        };
    }

    public void Disconnect()
    {
        // TODO: Implement Disconnect
    }
}