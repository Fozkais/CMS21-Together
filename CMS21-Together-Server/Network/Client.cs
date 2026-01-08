using System;
using CMS21_Together_Server.Network.Transport;

namespace CMS21_Together_Server.Network
{
	public class Client
	{
		public int id;
		public TCP tcp;
		// public UDP udp;

		public Client(int _clientId)
		{
			id = _clientId;
			tcp = new TCP(id);
		}

		public void Disconnect()
		{
			Console.WriteLine($"Client {id} disconnected.");
			tcp.Disconnect();
		}
	}
}