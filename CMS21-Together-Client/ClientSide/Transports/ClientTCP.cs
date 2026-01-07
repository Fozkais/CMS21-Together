using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.Handle;
using CMS21Together.ClientSide.Data.NewUI;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ClientSide.Transports;

public class ClientTCP
{
	private readonly int dataBufferSize = 4096;

	private byte[] receiveBuffer;
	private Packet receivedData;
	public TcpClient socket;

	private NetworkStream stream;

	public void Connect(string ip = "")
	{
		try
		{
			if (!System.Net.IPAddress.TryParse(ip, out _))
			{
				Client.Instance.OnDisconnectedInvoke();
				UICustomPanel.CreateInfoPanel("Invalid IP address.");
				MelonLogger.Error($"[ClientTCP->Connect] Invalid IP address: {ip}");
				return;
			}
			
			using (var testSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				var result = testSocket.BeginConnect(ip, MainMod.PORT, null, null);
				bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
				if (!success || !testSocket.Connected)
				{
					Client.Instance.OnDisconnectedInvoke();
					MelonLogger.Error($"[ClientTCP->Connect] Cannot reach {ip}:{MainMod.PORT}");
					return;
				}
				testSocket.Close();
			}
			
			socket = new TcpClient
			{
				ReceiveBufferSize = dataBufferSize,
				SendBufferSize = dataBufferSize
			};
			receiveBuffer = new byte[dataBufferSize];

			MelonLogger.Msg($"[ClientTCP->ConnectCallback] Trying to connect to server...");
			
			if (string.IsNullOrEmpty(ip))
				socket.BeginConnect(ClientData.UserData.ip, MainMod.PORT, ConnectCallback, socket);
			else
				socket.BeginConnect(ip, MainMod.PORT, ConnectCallback, socket);
		}
		catch (Exception e)
		{
			Client.Instance.OnDisconnectedInvoke();
			MelonLogger.Error($"[ClientTCP->Connect] Failed to connect to server : {e}");
		}
	}

	private void ConnectCallback(IAsyncResult result)
	{
		try
		{
			socket.EndConnect(result);
			if (!socket.Connected)
			{
				Client.Instance.OnDisconnectedInvoke();
				MelonLogger.Error("[ClientTCP->ConnectCallback] Cannot connect to server!");
				return;
			}

			stream = socket.GetStream();
			receivedData = new Packet();
			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
			ClientSend.HandShake();
			MelonLogger.Msg($"[ClientTCP->ConnectCallback] Connection etablished with server");
		}
		catch (Exception e)
		{
			Client.Instance.OnDisconnectedInvoke();
			MelonLogger.Error($"[ClientTCP->ConnectCallback] Failed to connect to server : {e}");
		}
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		if (stream == null) return;

		try
		{
			var byteLength = stream.EndRead(result);
			if (byteLength <= 0)
			{
				if (socket != null && !socket.Connected)
				{
					MelonLogger.Warning("[ClientTCP->ReceiveCallback] Connection closed by server (confirmed).");
					Client.Instance.OnDisconnectedInvoke();
					Client.Instance.Disconnect(true);
					return;
				}
				Thread.Sleep(100);
				stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
				return;
			}

			var data = new byte[byteLength];
			Array.Copy(receiveBuffer, data, byteLength);
			receivedData.Reset(HandleData(data));
			Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
			
			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
		}
		catch (IOException ioEx)
		{
			MelonLogger.Error($"[ClientTCP->ReceiveCallback] IOException: {ioEx.Message}");
			Client.Instance.Disconnect(true);
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[ClientTCP->ReceiveCallback] Unexpected error: {ex.Message}");
			Client.Instance.Disconnect(true);
		}
	}

	private bool HandleData(byte[] data)
	{
		var packetLenght = 0;

		receivedData.SetBytes(data);
		if (receivedData.UnreadLength() >= 4)
		{
			packetLenght = receivedData.ReadInt();
			if (packetLenght <= 0) return true;
		}
		
		while (packetLenght > 0 && packetLenght <= receivedData.UnreadLength())
		{
			var _packetBytes = receivedData.ReadBytes(packetLenght);
			ThreadManager.ExecuteOnMainThread<Exception>(_ =>
			{
				using (var _packet = new Packet(_packetBytes))
				{
					var _packetId = _packet.ReadInt();
					if (Client.PacketHandlers.ContainsKey(_packetId))
						Client.PacketHandlers[_packetId](_packet);
					else
						MelonLogger.Error($"[ClientTCP->HandleData] packet with id:{_packetId} is not valid.");
				}
			}, null);

			packetLenght = 0;
			if (receivedData.UnreadLength() >= 4)
			{
				packetLenght = receivedData.ReadInt();
				if (packetLenght <= 0)
					return true;
			}
		}

		if (packetLenght <= 1)
			return true;
		return false;
	}

	public void Send(Packet packet)
	{
		if (socket != null)
		{
			stream.BeginWrite(packet.ToArray(), 0, packet.Length(), (ar) =>
			{
				try
				{
					stream?.EndWrite(ar);
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"[TCP]Error while writing data : {ex.Message}");
				}
			}, null);
		}
	}

	public void Disconnect()
	{
		if (socket != null)
			socket.Close();

		stream = null;
		receivedData = null;
		receiveBuffer = null;
		socket = null;
	}
}