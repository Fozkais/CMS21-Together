using System;
using CMS21_Together_Core.Data.Enum;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;
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
		
		public Connection SteamConnection;

		public bool IsConnected;
		public Action OnConnectedSuccessfully;

		public float LastHeartbeatTime { get; set; }
		private float lastHeartbeatTime;
		private bool ConnectionValid;

		public Client(int clientId)
		{
			ID = clientId;
			// Tcp/Udp are always allocated: a slot may be claimed by either a
			// DirectIP or Steam connection, and ConnectionType isn't known yet here.
			Tcp = new Tcp(ID);
			Udp = new Udp(ID);
			OnConnectedSuccessfully += OnConnected;
		}
		
		private void OnConnected()
		{
			float currentTime = ServerTime.Time;
    
			lastHeartbeatTime = currentTime; 
			LastHeartbeatTime = currentTime;
			ConnectionValid = true;
			Logger.Debug($"Client[{ID}] connected successfully!");
			Server.SendToClient(new HeartbeatPacket(), ID);
		}

		public void Update()
		{
			if (!ConnectionValid) return;
			
			if (ServerTime.Time - lastHeartbeatTime >= 3)
			{
				lastHeartbeatTime = ServerTime.Time;
				Server.SendToClient(new HeartbeatPacket(), ID, false);
			}
			
			if (ServerTime.Time - LastHeartbeatTime > Program.CONNECTION_TIMEOUT)
			{
				// Log in English
				Logger.Warn($"Client[{ID}] timed out (No response for {Program.CONNECTION_TIMEOUT}s).");
				Disconnect();
			}
		}

		public void Disconnect()
		{
			Logger.Debug($"Client {ID} disconnected.");
			Tcp.Disconnect();
			Udp?.Disconnect();
			IsConnected = false;
			ConnectionValid = false;
			lastHeartbeatTime = 0;
			LastHeartbeatTime = 0;
			
			Server.SendToClients(new DisconnectPacket()
			{
				playerID = ID,
				message = "Disconnected"
			}, ID);
		}
	}
}