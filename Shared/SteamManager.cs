using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Shared
{
    public static class SteamManager
    {
        public static Lobby currentLobby;

        public static void Initialize()
        {
            SteamClient.Init(1190000);
            
            SteamMatchmaking.OnLobbyCreated += LobbyCreated;
            SteamMatchmaking.OnLobbyEntered += LobbyEntered;
            SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequested;
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
        }

        private static void LobbyEntered(Lobby lobby)
        {
            currentLobby = lobby;
            MelonLogger.Msg("Connected to lobby successfully");
        }

        private static void LobbyCreated(Result callback, Lobby lobby)
        {
            if (callback == Result.OK)
            {
                lobby.SetFriendsOnly();
                lobby.SetJoinable(true);
                
                MelonLogger.Msg("Lobby created successfully!");
                MelonLogger.Msg($"Lobby ID: {lobby.Id.ToString()}");
            }
        }
    }
}