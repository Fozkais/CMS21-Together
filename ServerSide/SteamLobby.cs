using System.Collections.Generic;
using CMS21Together.ClientSide;
using CMS21Together.ServerSide.Handle;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;
using SteamUtils = Steamworks.SteamUtils;

namespace CMS21Together.ServerSide
{
    public class SteamLobby
    {
        public static SteamLobby Instance;
        public Dictionary<int, SteamId> clients = new Dictionary<int, SteamId>();
        
        public async void HostLobby()
        {
            Instance = this;
            
            await SteamMatchmaking.CreateLobbyAsync(MainMod.MAX_PLAYER);
            clients = new Dictionary<int, SteamId>();
            for (int i = 0; i < MainMod.MAX_PLAYER; i++)
            {
                clients[i] = new SteamId();
            }
            
        }
        
        public async static void GameLobbyJoinRequested(Lobby lobby, SteamId steamID)
        {
            MelonLogger.Msg($"{steamID.ToString()} is trying to join lobby.");
            await lobby.Join();
            
        }

        public static void LobbyEntered(Lobby lobby)
        {
            SteamManager.currentLobby = lobby;
            Client.Instance.isConnected = true;
            MelonLogger.Msg("Connected to lobby successfully");
            ServerSend.Welcome(0);
        }

        public static void LobbyCreated(Result callback, Lobby lobby)
        {
            if (callback == Result.OK)
            {
                lobby.SetFriendsOnly();
                lobby.SetJoinable(true);

                SteamManager.lobbyCode = NetworkingUtils.ConvertLobbyID(lobby.Id.Value);
                
                MelonLogger.Msg("Lobby created successfully!");
                MelonLogger.Msg($"Lobby ID: {lobby.Id.ToString()} , Code : {SteamManager.lobbyCode}");
            }
        }

        public void HandleMessage(string message)
        {
            throw new System.NotImplementedException();
        }
    }
}