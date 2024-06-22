using System;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
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
        MelonLogger.Msg($"Sending data throught Steam Relay");
        byte[] data = _packet.ToArray();
        SendType type = reliable ? SendType.Reliable : SendType.Unreliable;
        Result res = connection.SendMessage(data, type);
        if(res != Result.OK)
            MelonLogger.Msg($"Could not send packet:{res.ToString()}.");
        else
            MelonLogger.Msg($"Sent packet. {res.ToString()}");
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