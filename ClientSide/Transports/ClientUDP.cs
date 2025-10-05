using System;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ClientSide.Transports;

public class ClientUDP
{
	public IPEndPoint endPoint;
	public UdpClient socket;

	public void Connect(string ip = "")
	{
		socket = new UdpClient();
		try
		{
			if (string.IsNullOrEmpty(ip))
				endPoint = new IPEndPoint(IPAddress.Parse(ClientData.UserData.ip), MainMod.PORT);
			else
				endPoint = new IPEndPoint(IPAddress.Parse(ip), MainMod.PORT);

			socket.Connect(endPoint);

			socket.BeginReceive(ReceiveCallback, null);

			using var packet = new Packet();
			Send(packet);
		}
		catch (Exception e)
		{
			MelonLogger.Error($"[ClientUDP->Connect] Error on connection:{e}");
		}
	}

	public void Send(Packet packet)
	{
		try
		{
			packet.InsertInt(ClientData.UserData.playerID);
			if (socket != null)
			{
				socket.BeginSend(packet.ToArray(), packet.Length(), (ar) =>
				{
					try
					{
						socket.EndSend(ar);
					}
					catch (Exception ex)
					{
						MelonLogger.Error($"[UDP]Error while writing data : {ex.Message}");
					}
				}, null);
			}
		}
		catch (SocketException ex)
		{
			MelonLogger.Error($"[ClientUDP->SendData] Error sending data to server: {ex}");
		}
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		if (socket == null) return;

		try
		{
			var receivedIP = new IPEndPoint(IPAddress.Any, 0);
			var _data = socket.EndReceive(result, ref receivedIP);

			if (_data.Length < 4)
			{
				Client.Instance.OnDisconnectedInvoke();
				MelonLogger.Error("[ClientUDP->ReceiveCallback] Data invalid");
				Disconnect();
				return;
			}

			HandleData(_data);
			socket.BeginReceive(ReceiveCallback, null);
		}
		catch (ObjectDisposedException)
		{
			MelonLogger.Warning("[ClientUDP->ReceiveCallback] Attempted to access a disposed socket. Ignoring callback.");
		}
		catch (SocketException ex)
		{
			MelonLogger.Error("[ClientUDP->ReceiveCallback] Error while receiving data from server via UDP: " + ex);
		}
		catch (Exception ex)
		{
			MelonLogger.Error("[ClientUDP->ReceiveCallback] Unexpected error: " + ex);
		}
	}

	private void HandleData(byte[] data)
	{
		using (var _packet = new Packet(data))
		{
			var packetLength = _packet.ReadInt();
			data = _packet.ReadBytes(packetLength);
		}

		ThreadManager.ExecuteOnMainThread<Exception>(ex =>
		{
			using (var _packet = new Packet(data))
			{
				var _packetId = _packet.ReadInt();

				if (Client.PacketHandlers.TryGetValue(_packetId, out var handler))
					handler(_packet);
				else
					MelonLogger.Error($"[ClientUDP->HandleData] packet with id:{_packetId} is not valid.");
			}
		}, null);
	}

	public void Disconnect()
	{
		if (socket != null)
		{
			socket.Close();
			socket = null;
		}

		endPoint = null;
	}
}