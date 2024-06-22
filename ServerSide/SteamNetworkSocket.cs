using System;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ServerSide
{
    public class SteamNetworkSocket : SocketManager
    {
        public override void OnConnectionChanged(Connection connection, ConnectionInfo info)
        {
            ulong clientSteamID = info.Identity.SteamId;
            
            if (info.State == ConnectionState.Connecting)
            {
                bool isFull = true;
                foreach (int ClientID in Server.clients.Keys)
                {
                    if (Server.clients[ClientID].isUsed == false)
                    {
                        isFull = false;
                    }
                }
                if (isFull)
                {
                    MelonLogger.Msg($"Incoming connection {clientSteamID} would exceed max connection count. Rejecting.");
                    connection.Close(false, 0, "Max Connection Count");
                    return;
                }
                Result res;
        
                if((res = connection.Accept()) == Result.OK)
                {
                    MelonLogger.Msg($"Accepting connection for SteamID:{clientSteamID}");         
                }
                else
                {          
                    MelonLogger.Msg($"Connection {clientSteamID} could not be accepted: {res.ToString()}");
                }
            }
            else if (info.State == ConnectionState.Connected)
            {
                MelonLogger.Msg("A client is successfully Connected.");
                foreach (int ClientID in Server.clients.Keys)
                {
                    if (Server.clients[ClientID].isUsed == false)
                    {
                        Server.clients[ClientID].steam.Link(connection);
                        Server.clients[ClientID].connectedType = NetworkType.steamNetworking;
                        ServerSend.Welcome(ClientID);
                        break;
                    }
                }
            }
        }

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            MelonLogger.Msg("A client is connecting...");
            /*bool isFull = true;
            foreach (int ClientID in Server.clients.Keys)
            {
                if (Server.clients[ClientID].isUsed == false)
                {
                    isFull = false;
                }
            }

            if (!isFull)
                connection.Accept();
            else
                connection.Close();*/
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            MelonLogger.Msg("A client connected successfully.");
            /*int id = 0;
            foreach (int ClientID in Server.clients.Keys)
            {
                if (Server.clients[ClientID].isUsed == false)
                {
                    Server.clients[ClientID].steam.Link(connection);
                    Server.clients[ClientID].connectedType = NetworkType.steamNetworking;
                    id = ClientID;
                }
            }
            
            ServerSend.Welcome(id);*/
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            MelonLogger.Msg($"Client:{NetworkingUtils.GetClientFromConnection(connection).id} disconnected from server.");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] _data =  NetworkingUtils.ConvertIntPtrToByteArray(data, size);

            NetworkingUtils.GetClientFromConnection(connection).steam.HandleData(_data);
        }
    }
}