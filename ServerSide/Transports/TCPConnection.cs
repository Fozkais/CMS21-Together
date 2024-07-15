using System;
using System.Net.Sockets;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ServerSide.Transports;

public class TCPConnection
{
    
    public static int dataBufferSize = 4096;
            
    public TcpClient socket;

    private readonly int id;
    private NetworkStream stream;
    private Packet receivedData;
    private byte[] receiveBuffer;

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

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int _byteLength = stream.EndRead(result);
            if (_byteLength <= 0)
            {
                if (Server.Instance.clients.TryGetValue(id, out var client))
                    client.Disconnect();
                return;
            }

            byte[] _data = new byte[_byteLength];
            Array.Copy(receiveBuffer, _data, _byteLength);

            receivedData.Reset(HandleData(_data));
            Array.Clear(receiveBuffer, 0, receiveBuffer.Length);
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }
        catch (Exception _ex)
        {
            if(_ex.InnerException is SocketException sockEx && sockEx.ErrorCode != 10054) 
                MelonLogger.Error($"[TCPConnection->ReceiveCallback] Error receiving TCP data: {_ex}"); 
            
            if (Server.Instance.clients.TryGetValue(id, out var client))
            {
                client.Disconnect();
            }
        }
    }

    private bool HandleData(byte[] data)
    {
        int _packetLenght = 0;
                
        receivedData.SetBytes(data);
        if (receivedData.UnreadLength() >= 4)
        {
            _packetLenght = receivedData.ReadInt();
            if (_packetLenght <= 0)
            {
                return true;
            }
        }

        while (_packetLenght > 0 && _packetLenght <= receivedData.UnreadLength())
        {
            byte[] _packetBytes = receivedData.ReadBytes(_packetLenght);
            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    if (Server.packetHandlers.ContainsKey(_packetId))
                        Server.packetHandlers[_packetId](id, _packet);
                }
            }, null);
            _packetLenght = 0;
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLenght = receivedData.ReadInt();
                if (_packetLenght <= 0)
                {
                    return true;
                }
            }
        }
        if (_packetLenght <= 1)
        {
            return true;
        }

        return false;
    }

    public void Send(Packet packet)
    {
        if (socket != null)
        {
            stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
        }
    }

    public void Disconnect()
    {
        socket.Close();
        stream.Close();
        stream = null;
        receivedData = null;
        receiveBuffer = null;
        socket = null;
    }
}