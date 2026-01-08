using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;

namespace CMS21_Together_Server.Network
{
	public static class Server
	{
		public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        
        private static TcpListener tcpListener;
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
                        Console.WriteLine($"Client connected on slot {i}");
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

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }
        }
        
        public static void SendToClient<T>(T packetData, int clientID) where T : INetworkData
        {
            PacketTypes id = PacketRouter.GetPacketId(packetData);
            using (Packet packet = new Packet((int)id))
            {
                packet.Write(packetData);
                Clients[clientID].Tcp.SendData(packet);
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
            Console.WriteLine("Server Stopped. Press Enter to close...");
        }
	}
}