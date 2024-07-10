using System;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ServerSide;

public class SteamSocket : SocketManager
{
    public override void OnConnectionChanged(Connection connection, ConnectionInfo info)
    {
        ulong clientSteamID = info.Identity.SteamId;
            
        if (info.State == ConnectionState.Connecting)
        {
            OnConnecting(connection, info);
            bool isFull = true;
            foreach (int ClientID in Server.Instance.clients.Keys)
            {
                if (Server.Instance.clients[ClientID].isConnected == false)
                {
                    isFull = false;
                }
            }
            if (isFull)
            {
                MelonLogger.Warning($"[SteamSocket->OnConnectionChanged] Incoming connection {clientSteamID} would exceed max connection count. Rejecting.");
                connection.Close(false, 0, "Max Connection Count");
                return;
            }
            Result res;
        
            if((res = connection.Accept()) == Result.OK)
            {
                MelonLogger.Msg($"[SteamSocket->OnConnectionChanged] Accepting connection for SteamID:{clientSteamID}");         
            }
            else
            {          
                MelonLogger.Error($"[SteamSocket->OnConnectionChanged] Connection {clientSteamID} could not be accepted: {res.ToString()}");
            }
        }
        else if (info.State == ConnectionState.Connected)
        {
            foreach (int ClientID in Server.Instance.clients.Keys)
            {
                if (Server.Instance.clients[ClientID].isConnected == false)
                {
                    MelonLogger.Msg($"[OnConnectionChanged]Connecting Client.");
                    Server.Instance.clients[ClientID].steam.connection = connection;
                    Server.Instance.clients[ClientID].Connect();
                    OnConnected(connection, info);
                    break;
                }
            }
        }
    }
    
    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        MelonLogger.Msg("[SteamSocket->OnConnecting] A client is trying to connect...");
        
        base.OnConnecting(connection, info);
    }

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        MelonLogger.Msg("[SteamSocket->OnConnected] A client connected successfully.");
        base.OnConnected(connection, info);
    }

    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        MelonLogger.Msg($"[SteamSocket->OnDisconnected] Client:{SteamworksUtils.GetClientFromConnection(connection).id} disconnected from server.");
    }

    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        byte[] _data =  SteamworksUtils.ConvertIntPtrToByteArray(data, size);

        SteamworksUtils.GetClientFromConnection(connection).steam.HandleData(_data);
    }
}