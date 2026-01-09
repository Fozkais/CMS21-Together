using System;
using CMS21_Together_Core.Data;
using CMS21_Together_Server.Network.Transport;
using Steamworks.Data;

namespace CMS21_Together_Server.Network
{
	public class Client
	{
		public int ID;
		public long SteamID { get; set; }
		public NetworkType ConnectionType;
		
		public Tcp Tcp;
		public Udp Udp;
		
		public Connection steamConnection;

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
			Logger.Debug($"Client[{ID}] connected successfully!");
		}

		public void Disconnect()
		{
			Logger.Debug($"Client {ID} disconnected.");
			Tcp.Disconnect();
			isConnected = false;
		}
	}
}