using System;
using CMS21Together.Shared;
using Steamworks.Data;

namespace CMS21Together.ServerSide.Transport;

public class ServerSteam
{
    public readonly int id;
    public Connection connection;

    public ServerSteam(int _id)
    {
        id = _id;
    }

    public void Link(Connection _connection) { connection = _connection; Server.clients[id].isUsed = true; }

    public void SendData(Packet _packet, bool reliable=true)
    {
        byte[] data = _packet.ToArray();
        SendType type = reliable ? SendType.Reliable : SendType.Unreliable;
        connection.SendMessage(data, type);
    }

    public void HandleData(byte[] _data)
    {
        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetId = _packet.ReadInt();
                if(Server.packetHandlers.ContainsKey(_packetId))
                    Server.packetHandlers[_packetId](id, _packet);
            }
        }, null);
    }

    public void Disconnect() { connection.Close();}
}