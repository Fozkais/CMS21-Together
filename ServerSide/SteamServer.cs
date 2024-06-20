using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using SteamUtils = CMS21Together.Shared.Steam.SteamUtils;

namespace CMS21Together.ServerSide
{
    public class SteamServer : SocketManager
    {
        public static SteamServer Instance;
        public SocketManager socket;
        
        public Dictionary<int, SteamConnection> clients = new Dictionary<int, SteamConnection>();
        private static int nextId = 0;
        public bool isLobby;
        
        public async void HostLobby()
        {
            Instance = this;
            
            await SteamMatchmaking.CreateLobbyAsync(MainMod.MAX_PLAYER);
            clients = new Dictionary<int, SteamConnection>();
            for (int i = 0; i < MainMod.MAX_PLAYER; i++)
            {
                clients[i] = new SteamConnection();
            }

            isLobby = true;
            CreateServer();
        }
        
        public void CreateServer()
        {
            socket = SteamNetworkingSockets.CreateRelaySocket<SteamServer>();
        }

        public int ConnectClient(SteamId steamId)
        {
            int id = nextId++;
            clients[id] = new SteamConnection(steamId);
            return id;
        }
        
        public  void Send(int connectionId, byte[] data, bool reliable)
        {
            if (clients.TryGetValue(connectionId, out SteamConnection steamConn))
            {
                var conn = steamConn.Connection;
                
                Result res;
                if(!reliable)
                    res = SteamManager.SendSocket(conn, data, SendType.Unreliable);
                else
                     res = SteamManager.SendSocket(conn, data, SendType.Reliable);
                
                if (res == Result.NoConnection || res == Result.InvalidParam)
                {
                    MelonLogger.Msg($"Connection to {connectionId} was lost.");
                    InternalDisconnect(connectionId, conn);
                }
                else if (res != Result.OK)
                {
                    MelonLogger.Msg($"Could not send: {res.ToString()}");
                }
            }
            else
            {
                MelonLogger.Msg("Trying to send on unknown connection: " + connectionId);
            }
        }
        
        private void InternalDisconnect(int connId, Connection socket)
        {
            socket.Close(false, 0, "Graceful disconnect");
            clients.Remove(connId);
            Debug.Log($"Client with SteamID {connId} disconnected.");
        }

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            throw new NotImplementedException();
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            throw new NotImplementedException();
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            throw new NotImplementedException();
        }

        public override void OnMessage( Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel )
        {
            MelonLogger.Msg( $"We got a message from {identity}!" );

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(SteamUtils.ConvertIntPtrToByteArray(data, size)))
                {
                    int _packetId = _packet.ReadInt();
                    int id = _packet.ReadInt(); // TODO: ensure clientID is written on send Method
                    Server.packetHandlers[_packetId](id, _packet);
                }
            }, null);
            
            
            // Send it right back
            // connection.SendMessage( data, size, SendType.Reliable); TODO: Adapt ServerSend to use this method.
        }
    }

    public struct SteamConnection
    {
        public SteamId SteamId;
        public Connection Connection;

        public SteamConnection(SteamId id, Connection conn)
        {
            SteamId = id;
            Connection = conn;
        }
        public SteamConnection(SteamId id)
        {
            SteamId = id;
        }
    }
}