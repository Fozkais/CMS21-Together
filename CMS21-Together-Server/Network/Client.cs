using System;
using CMS21_Together_Server.Network.Transport;

namespace CMS21_Together_Server.Network
{
	public class Client
	{
		public int ID;
		public Tcp Tcp;
		// public UDP udp;
		public bool isConnected;

		public Client(int clientId)
		{
			ID = clientId;
			Tcp = new Tcp(ID);
		}

		public void Disconnect()
		{
			Console.WriteLine($"Client {ID} disconnected.");
			Tcp.Disconnect();
			isConnected = false;
		}
	}
}