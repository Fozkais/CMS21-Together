using System;
using CMS21Together.Shared;
using CMS21Together.Shared.Steam;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ClientSide
{
    public class SteamLobbyClient
    {
        public static SteamLobbyClient Instance;
        public static string LobbyID;
        
        public async void JoinLobbyWithID()
        {
            ulong ID = NetworkingUtils.ConvertCode(LobbyID);
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Id == ID)
                {
                    await lobby.Join();
                    Instance = this;
                    return;
                }
            }
        }

        public void HandleMessage(string message)
        {
            MelonLogger.Msg("CL: Received message!");
            
            ThreadManager.ExecuteOnMainThread<Exception>(ex =>
            {
                using (Packet _packet = new Packet(Convert.FromBase64String(message)))
                {
                    int _packetId = _packet.ReadInt();
                    Client.PacketHandlers[_packetId](_packet);
                }
            }, null);
        }
    }
}