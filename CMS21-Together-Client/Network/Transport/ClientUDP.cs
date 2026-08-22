using System;
using System.Net;
using System.Net.Sockets;
using CMS21_Together_Core;
using CMS21_Together_Core.Logging;
using CMS21_Together_Core.Network;
using CMS21Together.Managers;

namespace CMS21Together.Network.Transport;

public class ClientUDP
{
	public UdpClient socket;
    public IPEndPoint endPoint;

    public ClientUDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), MainMod.PORT);
    }

    public void Connect(int _localPort)
    {
        socket = new UdpClient(_localPort);
        socket.Connect(endPoint);
        socket.BeginReceive(ReceiveCallback, null);
        
        using (Packet _packet = new Packet())
        {
            SendData(_packet);
        }
    }

    public void SendData(Packet _packet)
    {
        try
        {
            _packet.InsertInt(Client.Instance.ID); 

            byte[] _data = _packet.ToArray();
            socket.BeginSend(_data, _data.Length, null, null);
        }
        catch (Exception e)
        {
            Log.Error($"Error on UDP Send: {e.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            byte[] _data = socket.EndReceive(_result, ref endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            if (_data.Length < 4) return;
            
            HandleData(_data);
        }
        catch (Exception)
        {
            Disconnect();
        }
    }

    private void HandleData(byte[] _data)
    {
        using (Packet _packet = new Packet(_data))
        {
            int _packetLength = _packet.ReadInt();
            byte[] _dataBytes = _packet.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread<object>((_) =>
            {
                using (Packet packet = new Packet(_dataBytes))
                {
                    int packetId = packet.ReadInt();
                    try 
                    {
                        object dataObject = packet.Read<object>();
                        PacketRouter.Dispatch((PacketTypes)packetId, dataObject, 0);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error handling UDP packet {(PacketTypes)packetId}: {e.Message}");
                    }
                }
            }, null);
        }
    }
    
    public void Disconnect()
    {
        socket?.Close();
        socket = null;
    }
}