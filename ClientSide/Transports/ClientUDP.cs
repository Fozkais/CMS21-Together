using System;
using System.Net;
using System.Net.Sockets;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ClientSide.Transports;

public class ClientUDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;

    public void Connect()
    {
        socket = new UdpClient();
        try
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ClientData.UserData.ip), MainMod.PORT);
            socket.Connect(endPoint);

            socket.BeginReceive(ReceiveCallback, null);

            using Packet packet = new Packet();
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
                socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
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
            IPEndPoint receivedIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = socket.EndReceive(result, ref receivedIP);

            if (_data.Length < 4)
            {
                MelonLogger.Error("[ClientUDP->ReceiveCallback] Data invalid");
                Disconnect();
                return;
            }
                
            HandleData(_data);
            socket.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException ex)
        {
            MelonLogger.Error("[ClientUDP->ReceiveCallback] Error while receiving data from server via UDP: " + ex);
        }
    }

    private void HandleData(byte[] data)
    {
        using (Packet _packet = new Packet(data))
        {
            int packetLength = _packet.ReadInt();
            data = _packet.ReadBytes(packetLength);
        }

        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet _packet = new Packet(data))
            {
                int _packetId = _packet.ReadInt();

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