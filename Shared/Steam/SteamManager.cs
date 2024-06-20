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
            
            SteamMatchmaking.OnLobbyCreated += SteamLobby.LobbyCreated;
            SteamMatchmaking.OnLobbyEntered += SteamLobby.LobbyEntered;
            SteamMatchmaking.OnChatMessage += ChatMessage;
            SteamFriends.OnGameLobbyJoinRequested += SteamLobby.GameLobbyJoinRequested;
        }

        private static void ChatMessage(Lobby lobby, Friend from, string message)
        {
            string messageFor = message[0].ToString() + message[1];
            if(messageFor == "-1")
                SteamLobby.Instance.HandleMessage(message);
            else
                SteamLobbyClient.Instance.HandleMessage(message);
        }

        public static void Close()
        {
            SteamMatchmaking.OnLobbyCreated -= SteamLobby.LobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= SteamLobby.LobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested -= SteamLobby.GameLobbyJoinRequested;
            
            SteamClient.Shutdown();
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