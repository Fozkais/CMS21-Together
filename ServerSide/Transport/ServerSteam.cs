using System;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
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
        SendType type = reliable ? SendType.Reliable : SendType.Unreliable;
        
        byte[] data = _packet.ToArray();
        IntPtr _data = NetworkingUtils.ConvertByteArrayToIntPtr(data);
        
        Result res = connection.SendMessage(_data, data.Length, type);
        if(res != Result.OK)
            MelonLogger.Msg($"Could not send packet:{res.ToString()}.");
        
        NetworkingUtils.FreeIntPtr(_data);
    }

    public void HandleData(byte[] _data)
    {
        int _packetLenght = 0;
        Packet receivedData = new Packet();
                
        receivedData.SetBytes(_data);
        if (receivedData.UnreadLength() >= 4)
        {
            _packetLenght = receivedData.ReadInt();
            if (_packetLenght <= 0)
            {
                return;
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
        }
    }

    public void Disconnect() { connection.Close();}
}