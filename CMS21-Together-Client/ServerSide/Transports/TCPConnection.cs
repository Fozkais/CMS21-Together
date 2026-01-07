using System;
using System.Net.Sockets;
using System.Threading;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ServerSide.Transports;

public class TCPConnection
{
	public static int dataBufferSize = 4096;

	private readonly int id;
	private byte[] receiveBuffer;
	private Packet receivedData;

	public TcpClient socket;
	private NetworkStream stream;

	public TCPConnection(int _id)
	{
		id = _id;
	}

	public void Connect(TcpClient client)
	{
		socket = client;
		socket.ReceiveBufferSize = dataBufferSize;
		socket.SendBufferSize = dataBufferSize;

		stream = socket.GetStream();
		stream.ReadTimeout = 200;

		receivedData = new Packet();
		receiveBuffer = new byte[dataBufferSize];

		stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
	}
	
	public void BeginHandshake(TcpClient tcpClient)
	{
		socket = tcpClient;
		stream = socket.GetStream();
		receiveBuffer = new byte[4096];
		
		stream.BeginRead(receiveBuffer, 0, receiveBuffer.Length, HandshakeCallback, null);
	}

	private void HandshakeCallback(IAsyncResult result)
	{
		try
		{
			int byteLength = stream.EndRead(result);
			if (byteLength <= 0)
			{
				MelonLogger.Msg($"[Server->Handshake] Phantom connection ignored (id:{id})");
				Disconnect(true);
				return;
			}
			
			byte[] data = new byte[byteLength];
			Array.Copy(receiveBuffer, data, byteLength);
			
			using (Packet packet = new Packet(data))
			{
				if (packet.UnreadLength() < 8)
				{
					MelonLogger.Warning($"[Server->Handshake] Invalid packet received during handshake (too short)");
					Disconnect();
					return;
				}
				
				int packetLength;
				if (packet.UnreadLength() >= 4)
				{
					packetLength = packet.ReadInt();
					if (packetLength <= 0)
					{
						MelonLogger.Warning($"[Server->Handshake] Invalid packet length");
						Disconnect();
						return;
					}
				}
				int packetId = packet.ReadInt();
				if (packetId == (int)PacketTypes.handshake)
				{
					MelonLogger.Msg($"[Server->Handshake] Handshake OK for id:{id}");
					Thread.Sleep(150);
					Server.Instance.clients[id].Connect(socket);
				}
				else
				{
					MelonLogger.Warning($"[Server->TCPConnection->HandshakeCallback] Unexpected packet during handshake for id:{id}");
					Disconnect();
				}
			}
		}
		catch
		{
			MelonLogger.Warning($"[Server->TCPConnection->HandshakeCallback] Handshake failed for id:{id}");
			Disconnect();
		}
	}


	private void ReceiveCallback(IAsyncResult result)
	{
		try
		{
			if (stream == null) return;
			
			var _byteLength = stream.EndRead(result);
			if (_byteLength <= 0)
			{
				if (Server.Instance.clients.TryGetValue(id, out var client))
					client.Disconnect();
				return;
			}

			var _data = new byte[_byteLength];
			Array.Copy(receiveBuffer, _data, _byteLength);

			receivedData.Reset(HandleData(_data));
			Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
		}
		catch (Exception _ex)
		{
			if (_ex.InnerException is SocketException sockEx && sockEx.ErrorCode != 10054)
				MelonLogger.Error($"[TCPConnection->ReceiveCallback] Error receiving TCP data: {_ex}");

			if (Server.Instance.clients.TryGetValue(id, out var client)) client.Disconnect();
		}
	}

	private bool HandleData(byte[] data)
	{
		var _packetLenght = 0;

		receivedData.SetBytes(data);
		if (receivedData.UnreadLength() >= 4)
		{
			_packetLenght = receivedData.ReadInt();
			if (_packetLenght <= 0) return true;
		}

		while (_packetLenght > 0 && _packetLenght <= receivedData.UnreadLength())
		{
			var _packetBytes = receivedData.ReadBytes(_packetLenght);
			ThreadManager.ExecuteOnMainThread<Exception>(ex =>
			{
				using (var _packet = new Packet(_packetBytes))
				{
					var _packetId = _packet.ReadInt();
					if (Server.packetHandlers.ContainsKey(_packetId))
						Server.packetHandlers[_packetId](id, _packet);
				}
			}, null);
			_packetLenght = 0;
			if (receivedData.UnreadLength() >= 4)
			{
				_packetLenght = receivedData.ReadInt();
				if (_packetLenght <= 0) return true;
			}
		}

		if (_packetLenght <= 1) return true;

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
					stream.EndWrite(ar);
				}
				catch (Exception ex)
				{
					MelonLogger.Error($"Error while writing data : {ex.Message}");
				}
			}, null);
		}
	}
		

	public void Disconnect(bool phantom=false)
	{
		try
		{
			if (socket != null)
			{
				if (socket.Connected) socket.Close();
				socket = null;
			}
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			receivedData = null;
			receiveBuffer = null;
			if (id == 1 && !phantom)
				MelonCoroutines.Start(Server.Instance.CloseServer());
		}
		catch (Exception ex)
		{
			MelonLogger.Error($"[TCPConnection->Disconnect] Exception: {ex}");
		}
	}

}