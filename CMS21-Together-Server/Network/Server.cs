using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Data;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
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
                Console.WriteLine("Server is already running.");
                return;
            }
            isRunning = true;
            MaxPlayers = maxPlayers;
            Port = port;

            Console.WriteLine("Starting TCP socket...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            
            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (Program.USE_STEAM)
            {
                steamTransport = SteamTransport.Initialize(7777);
            }
        }

        private static int GetIDFromSteamID(long steamID)
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].isConnected && Clients[i].SteamID == steamID)
                    return i;
            }
            return -1;
        }
        
        

        private static void TcpConnectCallback(IAsyncResult result)
        {
            try
            {
                if (tcpListener == null || !tcpListener.Server.IsBound)
                    return;

                TcpClient client = tcpListener.EndAcceptTcpClient(result);
                tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);

                Console.WriteLine($"Pending connection: {client.Client.RemoteEndPoint}...");

                for (int i = 1; i <= MaxPlayers; i++)
                {
                    if (!Clients[i].isConnected)
                    {
                        Clients[i].Tcp.Connect(client);
                        Clients[i].isConnected = true;
                        
                        SendToClient(new ConnectPacket()
                        {
                            gameVersion = "",
                            playerGuid = "",
                            username = "",
                            message = "Welcome to server!",
                            modVersion = Program.MOD_VERSION,
                            playerID = Clients[i].ID
                        }, Clients[i].ID);
                        return;
                    }
                }

                Console.WriteLine($"Connection refused : Server full.");
                client.Close();
            }
            catch (ObjectDisposedException) { return; }
            catch (Exception e)
            {
                Console.WriteLine($"Error TCPConnect: {e.Message}");
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
            catch (Exception e)
            {
                Console.WriteLine($"Error on UDP Receive: {e.Message}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
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
                    steamTransport.SendData(Clients[clientID].steamConnection, packet.ToArray(), reliable);
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
                Console.WriteLine($"Error on UDP Send: {e.Message}");
            }
        }

        public static void Stop()
        {
            foreach (Client client in Clients.Values)
            {
                if (!client.isConnected) continue;
                SendToClient(new DisconnectPacket()
                {
                    message = "Server is closing."
                }, client.ID);
                Console.WriteLine($"Sent Disconect to client ID: {client.ID}");
            }
            tcpListener.Stop();
            udpListener?.Close();
            steamTransport?.Shutdown();
            Console.WriteLine("Server Stopped. Press Enter to close...");
        }
	}
}