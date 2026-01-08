using System;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using MelonLoader;

namespace CMS21Together.Network;

public class ClientTCP
{
	public TcpClient socket;
    private NetworkStream stream;
    private Packet receivedData;
    private byte[] receiveBuffer;

    public void Connect(string ip, int port)
    {
        try
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuffer = new byte[4096];
            receivedData = new Packet();

            socket.BeginConnect(ip, port, ConnectCallback, null);
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Connection Error : {ex.Message}");
        }
    }

    private void ConnectCallback(IAsyncResult result)
    {
        try
        {
            socket.EndConnect(result);

            if (!socket.Connected) return;

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            
            MelonLogger.Msg("Connected to TCP server.");
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error ConnectCallback : {ex.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int byteLength = stream.EndRead(result);
            if (byteLength <= 0)
            {
                Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(receiveBuffer, data, byteLength);
            
            receivedData.Reset(HandleData(data));
            
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }
        catch
        {
            Disconnect();
        }
    }

    private bool HandleData(byte[] data)
    {
        int packetLength = 0;
        receivedData.SetBytes(data);

        if (receivedData.UnreadLength() >= 4)
        {
            packetLength = receivedData.ReadInt();
            if (packetLength <= 0) return true;
        }

        while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
        {
            byte[] packetBytes = receivedData.ReadBytes(packetLength);

            // ---------------------------------------------------------
            // IMPORTANT : On passe sur le Thread Principal via ton ThreadManager
            // ---------------------------------------------------------
            ThreadManager.ExecuteOnMainThread<object>((_) =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    try 
                    {
                        object dataObject = packet.Read<object>();
                        PacketRouter.Dispatch((PacketTypes)packetId, dataObject, 0);
                    }
                    catch (Exception ex)
                    {
                        MelonLogger.Error($"Error reading packet {packetId}: {ex.Message}");
                    }
                }
            }, null); 
            // ---------------------------------------------------------

            packetLength = 0;
            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) return true;
            }
        }

        if (packetLength <= 1) return true;
        return false;
    }

    public void SendData(Packet packet)
    {
        try
        {
            if (socket != null)
            {
                packet.WriteLength(); // Ajoute la longueur au début
                byte[] buffer = packet.ToArray();
                stream.BeginWrite(buffer, 0, buffer.Length, null, null);
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Error sending: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        socket?.Close();
        stream = null;
        receivedData = null;
        receiveBuffer = null;
        socket = null;
        MelonLogger.Msg("Disconnected from server.");
    }
}