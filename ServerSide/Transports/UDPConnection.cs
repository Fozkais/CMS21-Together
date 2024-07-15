using System;
using System.Net;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;

namespace CMS21Together.ServerSide.Transports;

public class UDPConnection
{
    public readonly int id;
    public IPEndPoint endPoint;
    
    public UDPConnection(int _id)
    {
        id = _id;
    }
    public void Connect(IPEndPoint ipEndPoint)
    {
        endPoint = ipEndPoint;
    }
    
    public void Disconnect()
    {
        endPoint = null;
    }
    public void HandleData(Packet packet)
    {
        int _packetLength = packet.ReadInt();
        byte[] _packetBytes = packet.ReadBytes(_packetLength);

        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet _packet = new Packet(_packetBytes))
            {
                int _packetId = _packet.ReadInt();
                Server.packetHandlers[_packetId](id, _packet);
            }
        }, null);
    }

    public void Send(Packet packet)
    {
        if (endPoint != null)
        {
            Server.Instance.udp.BeginSend(packet.ToArray(), packet.Length(), endPoint, null, null);
        }
    }
}