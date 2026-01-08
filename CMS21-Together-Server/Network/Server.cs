using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace CMS21_Together_Server.Network
{
	public static class Server
	{
		public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        
        private static TcpListener tcpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting TCP socket...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            try
            {
                TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
                tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
                
                Console.WriteLine($"Pending connection: {_client.Client.RemoteEndPoint}...");

                for (int i = 1; i <= MaxPlayers; i++)
                {
                    if (clients[i].tcp.socket == null)
                    {
                        clients[i].tcp.Connect(_client);
                        Console.WriteLine($"Client connected on slot {i}");
                        return;
                    }
                }

                Console.WriteLine($"Connection refused : Server full.");
                _client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error TCPConnect: {e.Message}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }

        public static void Stop()
        {
            tcpListener.Stop();
            //TODO: send disconnect to everyone
        }
	}
}