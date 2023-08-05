using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS21MP.ServerSide;
using CMS21MP.ServerSide.DataHandle;
using Il2CppCMS.Tutorial;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21MP
{
    public static class CallbackHandler
    {
        public static string lobbyCodeString;
        public static ulong lobbyCode;

        public static bool isConnected;
        
        
        public static void InitializeCallbacks() 
        {
            // Lobby Creation
            SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
            SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
            SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
            SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
           // SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
            SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
            SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
            SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
            
            SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequestCallback;
            SteamNetworking.OnP2PConnectionFailed += OnP2PConnectionFailedCallback;
            //SteamApps.OnDlcInstalled += OnDlcInstalledCallback;
         //   SceneManager.sceneLoaded += OnSceneLoaded;

            MelonLogger.Msg("Callbacks initialized");
        }

        private static void OnP2PConnectionFailedCallback(SteamId arg1, P2PSessionError error)
        {
            MelonLogger.Msg("P2P connection failed");
            MelonLogger.Msg("Error:" + error.ToString());
        }

        private static void OnP2PSessionRequestCallback(SteamId askerSteamId)
        {
            MelonLogger.Msg("P2P session requested");
            //AcceptP2P(askerSteamId);
            SteamNetworking.AcceptP2PSessionWithUser(askerSteamId);
        }

        private static void OnLobbyMemberJoinedCallback(Lobby lobby, Friend joinedPlayer) // when someone joins the lobby
        {
            MelonLogger.Msg("Lobby member joined");
            if (SteamNetworking.AcceptP2PSessionWithUser(joinedPlayer.Id))
            {
                SteamData.StartReceivingPacket = true;
                ServerSend.Welcome(joinedPlayer.Id);
            }
            else
            {
                SteamData.StartReceivingPacket = false;
            }
        }

        private static void OnLobbyEnteredCallback(Lobby lobby)
        {
            MelonLogger.Msg("Lobby entered");
        } // when you join a lobby

        private static void OnLobbyMemberDisconnectedCallback(Lobby arg1, Friend arg2)
        {
            MelonLogger.Msg("Lobby member disconnected");
        } // when someone leaves the lobby

        private static void OnLobbyMemberLeaveCallback(Lobby arg1, Friend arg2) // when you leave the lobby
        {
            MelonLogger.Msg("Lobby member left");
        }

        private async static void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId lobbyID)
        {
            MelonLogger.Msg("Lobby join requested");
            SteamNetworking.AllowP2PPacketRelay(true);
            ModUI.Instance.ShowLobbyInterface();
            SteamData.currentLobby = lobby;
            await JoinLobby();
            foreach (Friend member in lobby.Members)
            {
                if (member.Id != SteamData.steamID)
                {
                    SteamNetworking.AcceptP2PSessionWithUser(member.Id);
                    
                }
            }

        } // when someone accept your invite

        private static void OnLobbyCreatedCallback(Result result, Lobby lobby) // when you create a lobby
        {
            MelonLogger.Msg("Lobby createdCallback");
            SteamNetworking.AllowP2PPacketRelay(true);
        }

        private static void OnLobbyGameCreatedCallback(Lobby arg1, uint arg2, ushort arg3, SteamId arg4)
        {
            MelonLogger.Msg("Lobby game created");
        } // when you start the game

        public static async Task<bool> CreateLobby(int lobbyParameters)
        {
            try
            {
                var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(MainMod.MAX_PLAYER);
                if (!createLobbyOutput.HasValue)
                {
                    MelonLogger.Msg("Lobby created but not correctly instantiated");
                    return false;
                }

              //  LobbyPartnerDisconnected = false;  not sure what it does
                SteamData.lobby = createLobbyOutput.Value;
                SteamData.lobby.SetPublic();
                SteamData.lobby.SetJoinable(true);
               // SteamData.lobby.SetData(staticDataString, lobbyParameters) Set custom parameter

               SteamData.currentLobby = SteamData.lobby;
               
               Server.StartSteamServer();
               return true;
            }
            catch (Exception exception)
            {
                MelonLogger.Msg("Failed to create multiplayer lobby");
                MelonLogger.Msg(exception.ToString());
                return false;
            }
        } 

        public static async Task<bool> RefreshMultiplayerLobbies()
        {
            try
            {
                SteamData.lobbies.Clear();
                Lobby[] lobbies = await SteamMatchmaking.LobbyList.RequestAsync();
                if (lobbies != null)
                {
                    foreach (Lobby lobby in lobbies.ToList())
                    {
                        SteamData.lobbies.Add(lobby);
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failed to refresh multiplayer lobbies:" + e);
                return true;
            }
        } 
        
        public static async Task<bool> JoinLobby()
        {
            try
            {
                RoomEnter joinedLobbySuccess = await SteamData.currentLobby.Join();
                if(joinedLobbySuccess != RoomEnter.Success)
                {
                    MelonLogger.Msg("Failed to join lobby");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Msg("Failed to join lobby:" + e);
                return false;
            }
        } // join a lobby
        
        public static bool AcceptP2P(SteamId user)
        {
            try
            { 
                OnP2PSessionRequestCallback(user);
                if (SteamNetworking.AcceptP2PSessionWithUser(user))
                {
                    MelonLogger.Msg("P2P session accepted");
                    return true;
                }
                MelonLogger.Msg("Failed to accept P2P session with user");
                return false;
            }
            catch(Exception e)
            {
                MelonLogger.Msg("Failed to accept P2P session with user:" + e);
                return false;
            }
        } // accept P2P session with a user

        /*

        static void OnLobbyCreated(LobbyCreated_t c_callback)
        {
            // Récupérer l'ID du lobby
            CSteamID _lobbyID = new CSteamID(c_callback.m_ulSteamIDLobby);
            lobbyCode = _lobbyID.m_SteamID;

            //SteamMatchmaking.JoinLobby(lobbyID);
        }
        
        static void OnLobbyList(LobbyMatchList_t callback)
        {
            var lobbies = callback.m_nLobbiesMatching;
            bool foundLobby = false;
                        
            List<CSteamID> LobbiesID = new List<CSteamID>();

            for(int i = 0; i < lobbies; i++)
            {
                CSteamID _lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                LobbiesID.Add(_lobbyID);
            }
            
            CSteamID lobbyFound = LobbiesID.FirstOrDefault(lobby => lobby.m_SteamID.ToString().EndsWith(lobbyCodeString));
            if(lobbyFound.m_SteamID.ToString().EndsWith(lobbyCodeString))
            {
                lobbyCode = lobbyFound.m_SteamID;
                MelonLogger.Msg("Found lobby with code : " + lobbyCode);
                foundLobby = true;
            }
            
            if (foundLobby)
            {
                SteamMatchmaking.JoinLobby(new CSteamID(lobbyCode));
            }
        }*/
    }
}