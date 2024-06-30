using System;
using System.Net.Sockets;
using MelonLoader;

namespace CMS21Together.ClientSide.Transports;

public class ClientTCP
{
    public TcpClient socket;

    private NetworkStream stream;
    private int dataBufferSize = 2048;
    private byte[] receiveBuffer;
    public void Connect()
    {
        try
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
            receiveBuffer = new byte[dataBufferSize];

            socket.BeginConnect("", MainMod.PORT, ConnectCallback, socket);
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
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[ClientTCP->ConnectCallback] Failed to connect to server : {e}");
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        if(stream == null) return;

        try
        {
            int byteLenght = stream.EndRead(result);
            if(byteLenght <= 0)
                return;

            byte[] data = new byte[byteLenght];
            
            Array.Copy(receiveBuffer, data, byteLenght);
            HandleData(data);
            Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        catch (Exception e)
        {
            MelonLogger.Error($"[ClientTCP->ReceiveCallback] Error while receiving data : {e}");
            Client.Instance.Disconnect();
        }
    }

    private void HandleData(byte[] data)
    {
        int packetLenght = 0;
        
        // TODO: Implement Packet Class
    }
}