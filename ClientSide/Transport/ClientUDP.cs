using System;
using System.Net;
using System.Net.Sockets;
using CMS21MP.ClientSide.DataHandle;
using CMS21MP.SharedData;
using MelonLoader;

namespace CMS21MP.ClientSide.Transport
{
   public class ClientUDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;

    public ClientUDP()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.ip), Client.Instance.port);
    }

    public void Connect(int localPort)
    {
        socket = new UdpClient(localPort);
        try
        {
            socket.Connect(endPoint);
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error connecting via UDP: {ex.Message}, forced to disconnect");
            Disconnect();
            return;
        }

        try
        {
            socket.BeginReceive(ReceiveCallback, null);
            using (Packet packet = new Packet())
            {
                SendData(packet);
            }
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error starting UDP receive: {ex.Message}, forced to disconnect");
            Disconnect();
        }
    }

    public bool IsConnected()
    {
        return endPoint != null && socket != null;
    }

    public void SendData(Packet _packet)
    {
        try
        {
            _packet.InsertInt(Client.Instance.Id);
            if (socket != null)
            {
                socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
            }
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error sending data to server via UDP: {ex.Message}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            byte[] _data = socket.EndReceive(_result, ref endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            if (_data.Length < 4)
            {
                MelonLogger.Msg("UDP connect error, forced to disconnect");
                Disconnect();
                return;
            }

            HandleData(_data);
        }
        catch (SocketException ex)
        {
            MelonLogger.Msg($"Error receiving UDP data: {ex.Message}, forced to disconnect");
            Disconnect();
        }
    }

    private void HandleData(byte[] _data)
    {
        try
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
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
            });
        }
        catch (Exception ex)
        {
            MelonLogger.Msg($"Error handling data via UDP: {ex.Message}, forced to disconnect");
            Disconnect();
        }
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