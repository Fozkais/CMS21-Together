using System;
using CMS21_Together_Server.Network.Transport;

namespace CMS21_Together_Server.Network
{
	public class Client
	{
		public int ID;
		public Tcp Tcp;
		public Udp Udp;

		public bool isConnected;
		public Action OnConnectedSuccessfully;

		public Client(int clientId)
		{
			ID = clientId;
			Tcp = new Tcp(ID);
			Udp = new Udp(ID);
			OnConnectedSuccessfully += OnConnected;
		}

		private void OnConnected()
		{
			Console.WriteLine($"Client[{ID}] connected successfully!");
		}

		public void Disconnect()
		{
			Console.WriteLine($"Client {ID} disconnected.");
			Tcp.Disconnect();
			isConnected = false;
		}
	}
}