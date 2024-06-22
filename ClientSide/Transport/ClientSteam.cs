using System;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ClientSide.Transport;

public class ClientSteam : ConnectionManager
{
    public Connection server;
    
    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        MelonLogger.Msg("Connecting to server.");
    }

    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        MelonLogger.Msg("Successfully connected to server.");
    }

    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        MelonLogger.Msg("Successfully disconnected from server.");
    }

    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        MelonLogger.Msg("Received data from server.");
        base.OnMessage(data, size, messageNum, recvTime, channel);
            
        byte[] _data =  NetworkingUtils.ConvertIntPtrToByteArray(data, size);
            
        ThreadManager.ExecuteOnMainThread<Exception>(ex =>
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetId = _packet.ReadInt();
                // MelonLogger.Msg("Received a packet with id:" + _packetId + " !");
                if (Client.PacketHandlers.ContainsKey(_packetId))
                {
                    Client.PacketHandlers[_packetId](_packet);
                }
                else
                    MelonLogger.Msg("packet is Invalid !!!");
            }
        },  null);
    }

    public void SendData(Packet _packet, bool reliable)
    {
        SendType sendType = reliable ? SendType.Reliable : SendType.Unreliable; // Reliable=TCP , Unrealiable=UDP
        server.SendMessage(NetworkingUtils.ConvertByteArrayToIntPtr(_packet.ToArray()), _packet.Length(), sendType);
    }
}