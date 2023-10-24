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
            
            Client.Instance.ConnectToServer("127.0.0.1");
        }

        public static void StartSteamServer()
        {
            InitializeServerData();
            MainMod.isServer = true;
        }

        public static void Stop()
        {
            ServerSend.DisconnectClient(0, "Server is shutting down.");
            
            
            MainMod.isServer = false;
            isStopping = true;

            udpListener.Close();
            tcpListener.Stop();
            
            
            clients.Clear();
            packetHandlers.Clear();
            
            Client.PacketHandlers.Clear();
            GameData.DataInitialzed = false;
            MelonLogger.Msg("Server Closed.");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if(isStopping)
                return;
            
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
           // MelonLogger.Msg($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
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
                {(int)PacketTypes.playerPosition, ServerHandle.playerPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.playerRotation},
                {(int)PacketTypes.playerSceneChange, ServerHandle.playerSceneChange},
                {(int)PacketTypes.carInfo, ServerHandle.CarInfo},
                {(int)PacketTypes.carPosition, ServerHandle.CarPosition},
                {(int)PacketTypes.carPart, ServerHandle.CarPart},
                {(int)PacketTypes.bodyPart, ServerHandle.BodyPart},
                {(int)PacketTypes.inventoryItem, ServerHandle.InventoryItem},
                {(int)PacketTypes.inventoryGroupItem, ServerHandle.InventoryGroupItem},
                {(int)PacketTypes.lifterPos, ServerHandle.LifterPos}
            };
            MelonLogger.Msg("Initialized Packets!");
        }
    }
}