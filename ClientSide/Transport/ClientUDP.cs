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

    public void Connect(int localPort)
    {
        socket = new UdpClient();
        try
        {
            MelonLogger.Msg("Connecting UDP...");
            endPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ip), Client.Instance.port);
            MelonLogger.Msg("Endpoint:" + endPoint.Address + " , " + endPoint.Port);
            socket.Connect(endPoint);

            socket.BeginReceive(ReceiveCallback, null);
            
            using(Packet packet = new Packet())
            {
                SendData(packet);
            }
        }
        catch (SocketException e)
        {
            MelonLogger.Msg("Error while connecting UDP: " + e);
        }
    }

    public void SendData(Packet _packet)
    {
        try
        {
            _packet.InsertInt(Client.Instance.Id);
            if (socket != null)
            {
                socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                MelonLogger.Msg("Sending packet!");
            }
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error sending data to server via UDP: {ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        if (socket != null)
        {
            try
            {
                IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = socket.EndReceive(_result, ref receivedIP);

                if (_data.Length < 4)
                {
                    MelonLogger.Msg("UDP Data invalid");
                   // Disconnect();
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
    }

    private void HandleData(byte[] _data)
    {
        using (Packet _packet = new Packet(_data))
        {
            int _packetLength = _packet.ReadInt();
            _data = _packet.ReadBytes(_packetLength);
        }

        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetId = _packet.ReadInt();

                if (Client.PacketHandlers.TryGetValue(_packetId, out var handler))
                {
                    handler(_packet);
                }
                else
                {
                    MelonLogger.Msg($"Packet with id {_packetId} not found in packetHandlers dictionary.");
                }
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

}