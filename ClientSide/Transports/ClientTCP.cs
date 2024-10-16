﻿using System;
using System.Net.Sockets;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared;
using MelonLoader;

namespace CMS21Together.ClientSide.Transports;

public class ClientTCP
{
	private readonly int dataBufferSize = 2048;

	private byte[] receiveBuffer;
	private Packet receivedData;
	public TcpClient socket;

	private NetworkStream stream;

	public void Connect(string ip = "")
	{
		try
		{
			socket = new TcpClient
			{
				ReceiveBufferSize = dataBufferSize,
				SendBufferSize = dataBufferSize
			};
			receiveBuffer = new byte[dataBufferSize];

			if (string.IsNullOrEmpty(ip))
				socket.BeginConnect(ClientData.UserData.ip, MainMod.PORT, ConnectCallback, socket);
			else
				socket.BeginConnect(ip, MainMod.PORT, ConnectCallback, socket);
		}
		catch (Exception e)
		{
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
				MelonLogger.Error("[ClientTCP->ConnectCallback] Cannot connect to server!");
				return;
			}

			stream = socket.GetStream();
			receivedData = new Packet();
			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
		}
		catch (Exception e)
		{
			MelonLogger.Error($"[ClientTCP->ConnectCallback] Failed to connect to server : {e}");
		}
	}

	private void ReceiveCallback(IAsyncResult result)
	{
		if (stream == null) return;

		try
		{
			var byteLenght = stream.EndRead(result);
			if (byteLenght <= 0)
				return;

			var data = new byte[byteLenght];

			Array.Copy(receiveBuffer, data, byteLenght);
			receivedData.Reset(HandleData(data));
			Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
			stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
		}
		catch (Exception e)
		{
			MelonLogger.Error($"[ClientTCP->ReceiveCallback] Error while receiving data : {e}");
			Client.Instance.Disconnect();
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
			stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
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