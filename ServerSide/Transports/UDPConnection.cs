using System;
using System.Net;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ServerSide.Transports;

public class UDPConnection
{
	public readonly int id;
	public IPEndPoint endPoint;

	public UDPConnection(int _id)
	{
		id = _id;
	}

	public void Connect(IPEndPoint ipEndPoint)
	{
		endPoint = ipEndPoint;
	}

	public void Disconnect()
	{
		endPoint = null;
	}

	public void HandleData(Packet packet)
	{
		var _packetLength = packet.ReadInt();
		var _packetBytes = packet.ReadBytes(_packetLength);

		ThreadManager.ExecuteOnMainThread<Exception>(ex =>
		{
			using (var _packet = new Packet(_packetBytes))
			{
				int _packetId = -1;
				try
				{
					_packetId = _packet.ReadInt();
					if (Server.packetHandlers.ContainsKey(_packetId))
						Server.packetHandlers[_packetId](id, _packet);
				}
				catch (System.Exception handlerEx)
				{
					MelonLoader.MelonLogger.Error($"[UDPConnection] Server packet handler {_packetId} threw: {handlerEx}");
				}
			}
		}, null);
	}

	public void Send(Packet packet)
	{
		if (endPoint != null)
		{
			Server.Instance.udp.BeginSend(packet.ToArray(), packet.Length(), endPoint, (ar) =>
			{
				try
				{
					Server.Instance.udp.EndSend(ar);
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"[UDP]Error while writing data : {ex.Message}");
				}
			}, null);
		}
	}
}