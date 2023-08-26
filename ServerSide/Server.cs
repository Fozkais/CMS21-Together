using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MelonLoader;
using System.Net;
using CMS21MP.ClientSide;
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
        private static bool _isStopping;

        public static void Start()
        {
            MaxPlayers = MainMod.MAX_PLAYER;
            Port = MainMod.PORT;

            MelonLogger.Msg("Starting server...");
            InitializeServerData();
            _isStopping = false;

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            MelonLogger.Msg($"Server started successfully !");
            MainMod.isServer = true;
            
            Client.Instance.ConnectToServer("127.0.0.1");
        }

        public static void StartSteamServer()
        {
            InitializeServerData();
            MainMod.isServer = true;
        }

        public static void Stop()
        {
            MainMod.isServer = false;
            _isStopping = true;

            udpListener.Close();
            tcpListener.Stop();
            
            
            clients.Clear();
            packetHandlers.Clear();
            
            Client.PacketHandlers.Clear();
            MelonLogger.Msg("Server Closed.");

        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            if (_isStopping) return;
            
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            MelonLogger.Msg($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    // Récupérer son endpoint TCP
                    IPEndPoint tcpEndpoint = (IPEndPoint)_client.Client.RemoteEndPoint;
                    // L'associer au socket UDP
                    clients[i].udp.Connect(tcpEndpoint);
                    
                    return;
                }
            }

            MelonLogger.Msg($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }
        
        private static void UDPReceiveCallback(IAsyncResult result)
        {
            if (_isStopping) return;

            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                    return;

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                        return;

                    clients[_clientId].udp.HandleData(_packet);
                }
            }
            catch (Exception _ex)
            {
                MelonLogger.Msg($"Error receiving UDP data: {_ex}");
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
                {(int)PacketTypes.welcome, ServerHandle.WelcomeReceived},
                {(int)PacketTypes.readyState, ServerHandle.ReadyState},
                {(int)PacketTypes.playerPosition, ServerHandle.playerPosition},
                {(int)PacketTypes.playerRotation, ServerHandle.playerRotation},
                {(int)PacketTypes.carInfo, ServerHandle.CarInfo},
                {(int)PacketTypes.carPosition, ServerHandle.CarPosition},
                {(int)PacketTypes.carPart, ServerHandle.CarPart},
                {(int)PacketTypes.carPartSize, ServerHandle.CarPartSize}
            };
            MelonLogger.Msg("Initialized Packets!");
        }
    }
}