using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Network.Transport;

namespace CMS21_Together_Server.Network
{
	public static class Server
	{
		public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        
        private static TcpListener  tcpListener;
        public static SteamTransport steamTransport { get; private set; }
        private static UdpClient udpListener;
        private static bool isRunning;

        public static void Start(int maxPlayers, int port)
        {
            if (isRunning)
            {
                Logger.Info("Server is already running.");
                return;
            }
            isRunning = true;
            MaxPlayers = maxPlayers;
            Port = port;

            Logger.Debug("Starting TCP socket...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (Program.Config.UseSteam)
            {
                steamTransport = SteamTransport.Initialize(7777);
            }
        }

        private static void TcpConnectCallback(IAsyncResult result)
        {
            try
            {
                if (tcpListener == null || !tcpListener.Server.IsBound)
                    return;

                TcpClient client = tcpListener.EndAcceptTcpClient(result);
                tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

                Logger.Debug($"Pending connection: {client.Client.RemoteEndPoint}...");

                for (int i = 1; i <= MaxPlayers; i++)
                {
                    if (!Clients[i].IsConnected)
                    {
                        Clients[i].Tcp.Connect(client);
                        Clients[i].IsConnected = true;
                        
                        SendToClient(new ConnectPacket()
                        {
                            gameVersion = "",
                            username = "",
                            message = "Welcome to server!",
                            modVersion = Program.MOD_VERSION,
                            playerID = Clients[i].ID
                        }, Clients[i].ID);
                        return;
                    }
                }

                Logger.Debug($"Connection refused : Server full.");
                client.Close();
            }
            catch (ObjectDisposedException) { return; }
            catch (Exception e)
            {
                Logger.Error($"Error TCPConnect: {e.Message}");
            }
        }
        
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                
                udpListener.BeginReceive(UDPReceiveCallback, null);
                if (_data.Length < 4) return;

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();
                    if (_clientId == 0) return; 

                    if (Clients.TryGetValue(_clientId, out Client client))
                    {
                        if (client.Udp.endPoint == null)
                        {
                            client.Udp.Connect(_clientEndPoint);
                            return;
                        }
                        
                        if (client.Udp.endPoint.ToString() == _clientEndPoint.ToString())
                        {
                            client.Udp.HandleData(_packet);
                        }
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                Logger.Error($"Error on UDP Receive: {e.Message}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
        }

        public static void SendToClients<T>(T packetData, int exceptClient, bool reliable = true) where T : INetworkData
        {
            foreach (Client client in Clients.Values)
            {
                if (client.IsConnected && client.ID != exceptClient)
                {
                    SendToClient(packetData, client.ID, reliable);
                }
            }
        }
        
        public static void SendToClient<T>(T packetData, int clientID, bool reliable=true) where T : INetworkData
        {
            PacketTypes id = PacketRouter.GetPacketId(packetData);
            using (Packet packet = new Packet((int)id))
            {
                packet.Write(packetData);
                packet.WriteLength();
                if (Clients[clientID].ConnectionType == NetworkType.DirectIP)
                {
                    if (reliable)
                        Clients[clientID].Tcp.SendData(packet);
                    else
                        Clients[clientID].Udp.SendData(packet);
                }
                else
                {
                    steamTransport.SendData(Clients[clientID].SteamConnection, packet.ToArray(), reliable);
                }
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
            catch (Exception e)
            {
                Logger.Error($"Error on UDP Send: {e.Message}");
            }
        }

        public static void Stop()
        {
            if (!isRunning) return;
            isRunning = false;
            
            SendToClients(new DisconnectPacket()
            {
                message = "Server is closing."
            }, -1);
            
            tcpListener.Stop();
            udpListener?.Close();
            steamTransport?.Shutdown();
            Logger.Info("Server Stopped.");
        }

        public static void Update()
        {
            if (!isRunning) return;
            if (Program.Config.UseSteam && steamTransport != null)
                steamTransport.Update();
            
            foreach (var client in Clients.Values)
            {
                if (client.IsConnected)
                {
                    client.Update();
                }
            }
        }
    }
}