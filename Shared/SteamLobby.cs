using MelonLoader;
using Steamworks;

namespace CMS21Together.Shared;

public static class SteamLobby
{
	private static Callback<LobbyCreated_t> c_OnLobbyCreated;
	public static string lobbyID = "";
	public static void CreateLobby(ELobbyType lobbyType)
	{
		SteamMatchmaking.CreateLobby(lobbyType, MainMod.MAX_PLAYER);

		c_OnLobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
	}

	private static void OnLobbyCreated(LobbyCreated_t result)
	{
		if (result.m_eResult == EResult.k_EResultOK)
		{
			MelonLogger.Msg($"Lobby Created Successfully ! {result.m_ulSteamIDLobby}");
			lobbyID = result.m_ulSteamIDLobby.ToString();
		}
		else
			MelonLogger.Msg($"Lobby Error ! {result.m_eResult}");
	}
}