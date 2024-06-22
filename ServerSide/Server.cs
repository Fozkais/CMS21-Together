using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ServerSide.Data;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace CMS21Together.ServerSide
{
     public static class Server
     {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();
        public delegate void packetHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, packetHandler> packetHandlers;

        public static NetworkType currentType = NetworkType.TcpUdp;
        public static TcpListener tcpListener;
        public static UdpClient udpListener;
        public static SocketManager steamListener;
        
        public static bool isStopping;
        public static string serverID;

        public static void Start()
        {
            MaxPlayers = MainMod.MAX_PLAYER;
            Port = MainMod.PORT;

            MelonLogger.Msg("Starting server...");
            InitializeServerData();

            if (currentType == NetworkType.TcpUdp)
            {
                MelonLogger.Msg("Starting TCP Server...");
                StartTCPServer();
                Client.Instance.ConnectToServer("127.0.0.1", NetworkType.TcpUdp); // Connect host to server
            }
            else
            { 
                MelonLogger.Msg("Starting Steam Server...");
                StartSteamServer();
                StartTCPServer(); // Required for host (cant connect to P2P server on local)
                Client.Instance.ConnectToServer("127.0.0.1", NetworkType.TcpUdp); // Connect host to server
            }
            

            MelonLogger.Msg($"Server started successfully !");
            isStopping = false;
            ServerData.isRunning = true;
            
            Application.runInBackground = true;
            MelonCoroutines.Start(ServerData.CheckForInactiveClientsRoutine());
        }
        
        private static void StartSteamServer()
        {
            steamListener = SteamNetworkingSockets.CreateRelaySocket<SteamNetworkSocket>();
            serverID = NetworkingUtils.ConvertServerID(ModSteamManager.Instance.clientData.PlayerSteamId);
            MelonLogger.Msg($"ServerID: {serverID}");
        }

        private static void StartTCPServer()
        {
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);
            
            MelonLogger.Msg("Started TCP Server.");
        }

        public static void Stop()
        {
            if(!ServerData.isRunning) return;
            
            Application.runInBackground = false;
            foreach (int id in Server.clients.Keys)
            {
                ServerSend.DisconnectClient(id, "Server is shutting down.");
            }
            
            ServerData.ResetData();
            isStopping = true;

            if(udpListener != null)
                udpListener.Close();
            if(tcpListener != null)
                tcpListener.Stop();
            
            if(clients != null)
                clients.Clear();
            if(packetHandlers != null)
                packetHandlers.Clear();

            ModUI.Instance.window = guiWindow.main;
            MelonLogger.Msg("Server Closed.");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if(isStopping)
                return;
            
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            
            MelonLogger.Msg($"Incoming connection from {_client.Client.RemoteEndPoint}...");

           foreach (int ClientID in clients.Keys)
           {
               if (clients[ClientID].isUsed == false)
               {
                   MelonLogger.Msg($"Connecting client with id:{ClientID}.");
                   clients[ClientID].tcp.Connect(_client);
                   return;
               }
           }

            MelonLogger.Msg($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        private static void UDPReceiveCallback(IAsyncResult result)
        {
            if(isStopping)
                return;
            try
            {
                IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(result, ref receivedIP);
                udpListener.BeginReceive(UDPReceiveCallback, null);
                    
                if (_data.Length < 4)
                    return;
                    

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();
                    if (_clientId == 0)
                        return;
                        
                    if (clients[_clientId].udp.endPoint == null)
                    {
                        clients[_clientId].udp.Connect(receivedIP);
                        return;
                    }
                        
                        
                    if (clients[_clientId].udp.endPoint.ToString() == receivedIP.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"Error receiving UDP data: {ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new ServerClient(i));
            }

            packetHandlers = new Dictionary<int, packetHandler>()
            {
                {(int)PacketTypes.empty, ServerHandle.Empty},
                {(int)PacketTypes.welcome, ServerHandle.WelcomeReceived},
                {(int)PacketTypes.readyState, ServerHandle.ReadyState},
                {(int)PacketTypes.keepAlive, ServerHandle.keepAlive},
                {(int)PacketTypes.disconnect, ServerHandle.Disconnect},
                
                {(int)PacketTypes.playerPosition, ServerHandle.playerPosition},
                {(int)PacketTypes.playerInitialPos, ServerHandle.playerInitialPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.playerRotation},
                {(int)PacketTypes.playerSceneChange, ServerHandle.playerSceneChange},
                {(int)PacketTypes.stats, ServerHandle.playerStats},
                {(int)PacketTypes.inventoryItem, ServerHandle.InventoryItem},
                {(int)PacketTypes.inventoryGroupItem, ServerHandle.InventoryGroupItem},
                
                {(int)PacketTypes.lifter, ServerHandle.Lifter},
                {(int)PacketTypes.tireChanger, ServerHandle.TireChanger},
                {(int)PacketTypes.wheelBalancer, ServerHandle.WheelBalancer},
                {(int)PacketTypes.engineStandAngle, ServerHandle.EngineStandAngle},
                {(int)PacketTypes.setEngineOnStand, ServerHandle.SetEngineOnStand},
                {(int)PacketTypes.setGroupEngineOnStand, ServerHandle.SetGroupEngineOnStand},
                {(int)PacketTypes.EngineStandResync, ServerHandle.ResyncEngineStand},
                {(int)PacketTypes.takeOffEngineFromStand, ServerHandle.TakeOffEngineFromStand},
                {(int)PacketTypes.engineCrane, ServerHandle.EngineCrane},
                {(int)PacketTypes.oilBin, ServerHandle.OilBin},
                {(int)PacketTypes.springClampGroup, ServerHandle.SpringClampGroup},
                {(int)PacketTypes.springClampClear, ServerHandle.SpringClampClear},
                {(int)PacketTypes.toolMove, ServerHandle.ToolsMove},
                
                {(int)PacketTypes.carResync, ServerHandle.CarResync},
                {(int)PacketTypes.carInfo, ServerHandle.CarInfo},
                {(int)PacketTypes.carPosition, ServerHandle.CarPosition},
                {(int)PacketTypes.carPart, ServerHandle.CarPart},
                {(int)PacketTypes.carParts, ServerHandle.CarParts},
                {(int)PacketTypes.bodyPart, ServerHandle.BodyPart},
                {(int)PacketTypes.bodyParts, ServerHandle.BodyParts},
            };
            MelonLogger.Msg("Initialized Packets!");
        }
    }
}