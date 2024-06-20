using System;
using System.Linq;
using System.Runtime.InteropServices;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide;
using CMS21Together.ServerSide.Handle;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using SteamClient = Steamworks.SteamClient;
using SteamServer = CMS21Together.ServerSide.SteamServer;

namespace CMS21Together.Shared.Steam
{
    public static class SteamManager
    {
        public static Lobby currentLobby;
        public static string lobbyCode;

        public static void Initialize()
        {
            SteamClient.Init(1190000);
            
            SteamMatchmaking.OnLobbyCreated += LobbyCreated;
            SteamMatchmaking.OnLobbyEntered += LobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += LobbyMemberJoined;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
        }

        private static void LobbyMemberJoined(Lobby lobby, Friend friend)
        {
           
        }

        public static void Close()
        {
            SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequested;
            
            SteamClient.Shutdown();
        }

        private async static void GameLobbyJoinRequested(Lobby lobby, SteamId steamID)
        {
            MelonLogger.Msg($"{steamID.ToString()} is trying to join lobby.");
            await lobby.Join();
            
            
            
            int id = SteamServer.Instance.ConnectClient(steamID);
            ServerSend.Welcome(id, "Welcome to the steam server!");
        }

        private static void LobbyEntered(Lobby lobby)
        {
            currentLobby = lobby;
            Client.Instance.isConnected = true;
            MelonLogger.Msg("Connected to lobby successfully");
        }

        private static void LobbyCreated(Result callback, Lobby lobby)
        {
            if (callback == Result.OK)
            {
                lobby.SetFriendsOnly();
                lobby.SetJoinable(true);

                lobbyCode = SteamUtils.ConvertLobbyID(lobby.Id.Value);
                
                MelonLogger.Msg("Lobby created successfully!");
                MelonLogger.Msg($"Lobby ID: {lobby.Id.ToString()} , Code : {lobbyCode}");
            }
        }
        
        
        public static Result SendSocket(Connection conn, byte[] data, SendType sendType)
        {    
            /*Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = (byte)sendType;*/

            GCHandle pinnedArray = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr pData = pinnedArray.AddrOfPinnedObject();
            Result res = conn.SendMessage( pData, data.Length, sendType);
            if(res != Result.OK)
            {
                MelonLogger.Msg($"Send issue: {res}");
            }

            pinnedArray.Free();
            return res;
        } 
    }
}