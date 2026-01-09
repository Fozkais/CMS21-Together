using System;
using System.Net;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Data;

namespace CMS21_Together_Server.Network.Transport
{
	public class Udp
	{
		public IPEndPoint endPoint;
		private int id;

		public Udp(int _id)
		{
			id = _id;
		}

		public void Connect(IPEndPoint _endPoint)
		{
			endPoint = _endPoint;
			Logger.Debug($"Client {id} connected with UDP by {endPoint}");
		}

		public void SendData(Packet _packet)
		{
			Server.SendUDPData(endPoint, _packet);
		}

		public void HandleData(Packet _packetData)
		{
			int _packetLength = _packetData.ReadInt();
			byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

			using (Packet _packet = new Packet(_packetBytes))
			{
				int _packetId = _packet.ReadInt();
				try 
				{
					object dataObject = _packet.Read<object>();
					PacketRouter.Dispatch((PacketTypes)_packetId, dataObject, id);
				}
				catch (Exception ex)
				{
					Logger.Error($"Error UDP Packet {_packetId}: {ex.Message}");
				}
			}
		}
	}
}