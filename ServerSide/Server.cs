using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MelonLoader;
using System.Net;
using CMS21MP.ClientSide;
using CMS21MP.ClientSide.Data;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.ServerSide.DataHandle;
using CMS21MP.SharedData;
using UnityEngine;

namespace CMS21MP.ServerSide
{
    public class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, ServerClient> clients = new Dictionary<int, ServerClient>();

        public delegate void packetHandler(int _fromClient, Packet _packet);

        public static Dictionary<int, packetHandler> packetHandlers;

        public static TcpListener tcpListener;
        private static UdpClient udpListener;
        private static bool isStopping;

        public static void Start()
        {
            MaxPlayers = MainMod.MAX_PLAYER;
            Port = MainMod.PORT;

            MelonLogger.Msg("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            MelonLogger.Msg($"Server started successfully !");
            MainMod.isServer = true;
            isStopping = false;
            
            Application.runInBackground = true;
            
            Client.Instance.ConnectToServer("127.0.0.1");
        }

        public static void StartSteamServer()
        {
            InitializeServerData();
            MainMod.isServer = true;
        }

        public static void Stop()
        {
            Application.runInBackground = false;
            foreach (int id in Server.clients.Keys)
            {
                ServerSend.DisconnectClient(id, "Server is shutting down.");
            }
            
            
            MainMod.isServer = false;
            isStopping = true;

            if(udpListener != null)
                udpListener.Close();
            if(tcpListener != null)
                tcpListener.Stop();
            
            if(clients != null)
                clients.Clear();
            if(packetHandlers != null)
                packetHandlers.Clear();
            
            if(Client.PacketHandlers != null)
                Client.PacketHandlers.Clear();
            GameData.DataInitialzed = false;

            GameData.carLoaders = null;
            
            if(ClientData.carOnScene != null)
                ClientData.carOnScene.Clear();
            if(ClientData.serverPlayers != null)
                ClientData.serverPlayers.Clear();
            if(ClientData.serverPlayerInstances != null)
                ClientData.serverPlayerInstances.Clear();
            if(GameData.localInventory != null)
                GameData.localInventory.DeleteAll();
            if(ClientData.playerGroupInventory != null)
                ClientData.playerGroupInventory.Clear();
            if( ClientData.playerInventory != null)
                ClientData.playerInventory.Clear();

            ClientData.asGameStarted = false;
            
            
            MelonLogger.Msg("Server Closed.");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if(isStopping)
                return;
            
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
           // MelonLogger.Msg($"Incoming connection from {_client.Client.RemoteEndPoint}...");

           foreach (int ClientID in clients.Keys)
           {
               if (clients[ClientID].tcp.socket == null)
               {
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

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                MelonLogger.Msg($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
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
                {(int)PacketTypes.playerPosition, ServerHandle.playerPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.playerRotation},
                {(int)PacketTypes.playerSceneChange, ServerHandle.playerSceneChange},
                {(int)PacketTypes.carInfo, ServerHandle.CarInfo},
                {(int)PacketTypes.carPosition, ServerHandle.CarPosition},
                {(int)PacketTypes.carPart, ServerHandle.CarPart},
                {(int)PacketTypes.bodyPart, ServerHandle.BodyPart},
                {(int)PacketTypes.carParts, ServerHandle.PartScripts},
                {(int)PacketTypes.bodyParts, ServerHandle.BodyParts},
                {(int)PacketTypes.inventoryItem, ServerHandle.InventoryItem},
                {(int)PacketTypes.inventoryGroupItem, ServerHandle.InventoryGroupItem},
                {(int)PacketTypes.lifterPos, ServerHandle.LifterPos},
                {(int)PacketTypes.tireChanger, ServerHandle.TireChanger},
                {(int)PacketTypes.tireChanger_ResetAction, ServerHandle.TireChanger_ResetAction},
                {(int)PacketTypes.wheelBalancer, ServerHandle.WheelBalancer},
                {(int)PacketTypes.wheelBalancer_UpdateWheel, ServerHandle.WheelBalancer_UpdateWheel},
                {(int)PacketTypes.wheelBalancer_ResetAction, ServerHandle.WheelBalancer_ResetAction}
            };
            MelonLogger.Msg("Initialized Packets!");
        }
    }
}