using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using Il2CppSystem.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ClientSide.Transport
{
   public class ClientUDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;
    private Packet receivedData;

    public ClientUDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ip), Client.Instance.port);
    }

    public void Connect(int localPort)
    {
        if (MainMod.isServer)
        {
            socket = new UdpClient();
            MelonLogger.Msg("UDP endpoint to " + "127.0.0.1" + ":" + Client.Instance.port + " ...");
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Client.Instance.port);
        }
        else
        { 
            socket = new UdpClient();
            MelonLogger.Msg("UDP endpoint to " + Client.Instance.ip + ":" + Client.Instance.port + " ...");
            endPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ip), Client.Instance.port);
            
        }
        
        socket.Connect(endPoint);
        
        socket.BeginReceive(ReceiveCallback, null);
        
        using (Packet packet = new Packet((int)PacketTypes.empty))
        {
            SendData(packet);
        }
        
        if(IsConnected())
            MelonLogger.Msg("UDP connected");
    }

    public bool IsConnected()
    {
        return endPoint != null && socket != null;
    }

    public void SendData(Packet _packet)
    {
        try
        {
            //_packet.InsertInt(Client.Instance.Id);
            if (socket != null)
            {
                _packet.InsertInt(Client.Instance.Id);
                _packet.WriteLength();
                _packet.ReadData();
                //socket.Send(_packet.ToArray(), _packet.Length(), endPoint); 
                
                socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
            }
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error sending data to server via UDP: {ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = socket.EndReceive(_result, ref receivedIP);

            if (_data.Length < 4)
            {
                MelonLogger.Msg("UDP connect error , forced to disconnect");
                Disconnect();
                return;
            }

            HandleData(_data);
            socket.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg("error while receiving data from server via UDP: " + ex);
            //MelonCoroutines.Start(ResetConnection());
        }
    }

    private bool HandleData(byte[] _data)
    {
        receivedData = new Packet();
        int _packetLenght = 0;

        receivedData.SetBytes(_data);
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
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    MelonLogger.Msg("Readed _packetId:" + _packetId);
                    Client.PacketHandlers[_packetId](_packet);
                }
            });
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

    private void Disconnect()
    {
        if (socket != null)
        {
            socket.Close();
            socket = null;
        }
        endPoint = null;
    }
}

}