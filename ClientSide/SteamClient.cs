using System;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using SteamUtils = CMS21Together.Shared.Steam.SteamUtils;

namespace CMS21Together.ClientSide
{
    public class SteamClient : ConnectionManager
    {
        public static string LobbyID;
        public static ConnectionManager socket;
        
        public async static void JoinLobbyWithID()
        {
            ulong ID = SteamUtils.ConvertCode(LobbyID);
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Id == ID)
                {
                    await lobby.Join();
                    socket = SteamNetworkingSockets.ConnectRelay<SteamClient>(lobby.Id);
                    
                    MelonLogger.Msg($"SteamClient connected ? : {socket.Connected}");
                    return;
                }
            }
            
        }
        
        public static void OnMessage( Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel )
        {
            MelonLogger.Msg( $"We got a message from {identity}!" );

            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(SteamUtils.ConvertIntPtrToByteArray(data, size)))
                {
                    int _packetId = _packet.ReadInt();
                    Client.PacketHandlers[_packetId](_packet);
                }
            }, null);
            
            
            // Send it right back
            // connection.SendMessage( data, size, SendType.Reliable); TODO: Adapt ServerSend to use this method.
        }
    }
}