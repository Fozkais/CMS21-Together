using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.NewUI;
using CMS21Together.ServerSide;
using CMS21Together.Shared.Data;
using MelonLoader;
using Steamworks;
using Steamworks.Data;

namespace CMS21Together.Shared;

public static class SteamworksUtils
{
	private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
	private const string InviteLobbyServerIdKey = "cms21_together_server_id";
	private const string InviteLobbyVersionKey = "cms21_together_version";
	private static readonly Random Random = new Random();
	private static Lobby? inviteLobby;
	private static bool inviteCallbacksRegistered;
	private static bool inviteLobbyCreateInProgress;
	private static bool openInviteOverlayWhenReady;

	public static void RegisterInviteCallbacks()
	{
		if (inviteCallbacksRegistered || !ApiCalls.useSteam)
			return;

		SteamFriends.OnGameLobbyJoinRequested += HandleGameLobbyJoinRequested;
		inviteCallbacksRegistered = true;
		MelonLogger.Msg("[SteamworksUtils] Steam invite callbacks registered.");
	}

	public static void CreateOrUpdateInviteLobby(string serverID)
	{
		if (!CanUseSteamInvites() || string.IsNullOrEmpty(serverID))
			return;

		EnsureInviteLobby(serverID, false);
	}

	public static void OpenInviteOverlay(string serverID)
	{
		if (!CanUseSteamInvites())
		{
			MelonLogger.Warning("[SteamworksUtils] Steam invite overlay requested while Steam is unavailable.");
			return;
		}

		if (string.IsNullOrEmpty(serverID))
		{
			MelonLogger.Warning("[SteamworksUtils] Steam invite overlay requested before a server ID was available.");
			return;
		}

		EnsureInviteLobby(serverID, true);
	}

	public static void LeaveInviteLobby()
	{
		if (!inviteLobby.HasValue)
			return;

		MelonLogger.Msg($"[SteamworksUtils] Leaving Steam invite lobby {inviteLobby.Value.Id}.");
		inviteLobby.Value.Leave();
		inviteLobby = null;
		inviteLobbyCreateInProgress = false;
		openInviteOverlayWhenReady = false;
	}

	private static bool CanUseSteamInvites()
	{
		return ApiCalls.useSteam && SteamClient.IsValid;
	}

	private static void EnsureInviteLobby(string serverID, bool openOverlay)
	{
		openInviteOverlayWhenReady |= openOverlay;

		if (inviteLobby.HasValue)
		{
			ConfigureInviteLobby(inviteLobby.Value, serverID);
			if (openOverlay)
				OpenCurrentInviteOverlay();
			return;
		}

		if (inviteLobbyCreateInProgress)
			return;

		inviteLobbyCreateInProgress = true;
		CreateInviteLobbyAsync(serverID);
	}

	private static async void CreateInviteLobbyAsync(string serverID)
	{
		try
		{
			Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(MainMod.MAX_PLAYER);
			ThreadManager.ExecuteOnMainThread<object>(_ =>
			{
				inviteLobbyCreateInProgress = false;
				if (!lobby.HasValue)
				{
					openInviteOverlayWhenReady = false;
					MelonLogger.Error("[SteamworksUtils] Failed to create Steam invite lobby.");
					UICustomPanel.CreateInfoPanel("Failed to create Steam invite lobby.");
					return;
				}

				inviteLobby = lobby.Value;
				ConfigureInviteLobby(inviteLobby.Value, serverID);
				MelonLogger.Msg($"[SteamworksUtils] Created Steam invite lobby {inviteLobby.Value.Id} for server {serverID}.");

				if (openInviteOverlayWhenReady)
					OpenCurrentInviteOverlay();
				openInviteOverlayWhenReady = false;
			}, null);
		}
		catch (Exception ex)
		{
			ThreadManager.ExecuteOnMainThread<object>(_ =>
			{
				inviteLobbyCreateInProgress = false;
				openInviteOverlayWhenReady = false;
				MelonLogger.Error($"[SteamworksUtils] Failed to create Steam invite lobby: {ex}");
				UICustomPanel.CreateInfoPanel("Failed to create Steam invite lobby.");
			}, null);
		}
	}

	private static void ConfigureInviteLobby(Lobby lobby, string serverID)
	{
		lobby.MaxMembers = MainMod.MAX_PLAYER;
		lobby.SetFriendsOnly();
		lobby.SetJoinable(true);
		lobby.SetData(InviteLobbyServerIdKey, serverID);
		lobby.SetData(InviteLobbyVersionKey, MainMod.ASSEMBLY_MOD_VERSION);
		lobby.SetData("name", $"{SteamClient.Name}'s CMS21 Together lobby");
		lobby.SetGameServer(SteamClient.SteamId);
	}

	private static void OpenCurrentInviteOverlay()
	{
		if (!inviteLobby.HasValue)
			return;

		SteamFriends.OpenGameInviteOverlay(inviteLobby.Value.Id);
		MelonLogger.Msg($"[SteamworksUtils] Opened Steam invite overlay for lobby {inviteLobby.Value.Id}.");
	}

	private static void HandleGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
	{
		MelonLogger.Msg($"[SteamworksUtils] Steam lobby join requested from {friendId} for lobby {lobby.Id}.");
		JoinInvitedLobbyAsync(lobby);
	}

	private static async void JoinInvitedLobbyAsync(Lobby lobby)
	{
		try
		{
			RoomEnter result = await lobby.Join();
			ThreadManager.ExecuteOnMainThread<object>(_ =>
			{
				if (result != RoomEnter.Success)
				{
					MelonLogger.Error($"[SteamworksUtils] Failed to join Steam invite lobby {lobby.Id}: {result}");
					UICustomPanel.CreateInfoPanel("Failed to join Steam invite lobby.");
					return;
				}

				inviteLobby = lobby;
				inviteLobby.Value.Refresh();
				string serverID = inviteLobby.Value.GetData(InviteLobbyServerIdKey);
				if (string.IsNullOrEmpty(serverID))
				{
					MelonLogger.Error($"[SteamworksUtils] Steam invite lobby {lobby.Id} did not contain a server ID.");
					UICustomPanel.CreateInfoPanel("Steam invite did not contain a server ID.");
					return;
				}

				if (Client.Instance.isConnected)
				{
					MelonLogger.Warning("[SteamworksUtils] Ignoring Steam invite because the client is already connected.");
					UICustomPanel.CreateInfoPanel("Already connected to a lobby.");
					return;
				}

				ClientData.UserData.selectedNetworkType = NetworkType.Steam;
				string username = string.IsNullOrEmpty(ClientData.UserData.username)
					? SteamClient.Name
					: ClientData.UserData.username;
				UIActions.StartClient(username, serverID);
			}, null);
		}
		catch (Exception ex)
		{
			ThreadManager.ExecuteOnMainThread<object>(_ =>
			{
				MelonLogger.Error($"[SteamworksUtils] Failed to join Steam invite lobby {lobby.Id}: {ex}");
				UICustomPanel.CreateInfoPanel("Failed to join Steam invite lobby.");
			}, null);
		}
	}
	
	public static byte[] ConvertIntPtrToByteArray(IntPtr ptr, int size)
	{
		byte[] byteArray = new byte[size];
		Marshal.Copy(ptr, byteArray, 0, size);
		return byteArray;
	}
        
	public static IntPtr ConvertByteArrayToIntPtr(byte[] byteArray)
	{
		IntPtr ptr = Marshal.AllocHGlobal(byteArray.Length);
		Marshal.Copy(byteArray, 0, ptr, byteArray.Length);
		return ptr;
	}

	public static ServerConnection GetClientFromConnection(Connection connection)
	{
		ServerConnection SV_client = Server.Instance.clients.First(s => s.Value.steam.connection.Id == connection.Id).Value;
		if(SV_client == null)
			MelonLogger.Warning($"[SteamworksUtils->GetClientFromConnection] Did not found a valid client.");
		return SV_client;
	}
	
	public static string GetServerID(ulong lobbyID)
	{
		int offset = Random.Next(0, 62);
		
		StringBuilder result = new StringBuilder();
		do
		{
			int index = (int)(lobbyID % 62);

			char newChar = Characters[(index + offset) % 62];
			result.Insert(0, newChar);
			lobbyID /= 62;
		} while (lobbyID > 0);

		// Ajouter le caractère correspondant au décalage à la fin de la chaîne
		result.Append(Characters[offset]);

		return result.ToString();
	}

	public static ulong ConvertServerID(string code)
	{
		char offsetChar = code[code.Length - 1];
		int offset = Characters.IndexOf(offsetChar);
		
		ulong result = 0;
		for (int i = 0; i < code.Length - 1; i++)
		{
			int index = Characters.IndexOf(code[i]);

			int originalIndex = (index - offset + 62) % 62;
			result = result * 62 + (ulong)originalIndex;
		}

		return result;
	}
}
