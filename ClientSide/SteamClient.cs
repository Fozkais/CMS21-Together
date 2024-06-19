using System;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.ClientSide
{
    public static class SteamClient
    {
        public static string LobbyID;
        
        public async static void JoinLobbyWithID()
        {
            ulong ID;
            if(!ulong.TryParse(LobbyID, out ID))
                return;
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

            foreach (Lobby lobby in lobbies)
            {
                if (lobby.Id == ID)
                {
                    await lobby.Join();
                    return;
                }
            }
        }
    }
}