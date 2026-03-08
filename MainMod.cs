using System;
using System.IO;
using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.NewUI;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.ServerSide;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using Il2CppSystem.Collections;
using MelonLoader;
using Steamworks;
using UnhollowerRuntimeLib;
using UnityEngine;

// ReSharper disable All

namespace CMS21Together
{
	public class MainMod : MelonMod
	{
		public const int MAX_SAVE_COUNT = 22;
		public const int MAX_PLAYER = 4;
		public const int PORT = 7777;
		public const string ASSEMBLY_MOD_VERSION = "0.4.17" + ASSEMBLY_HOTFIX_VERSION;
		public const string ASSEMBLY_HOTFIX_VERSION = "";
		public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION + ASSEMBLY_HOTFIX_VERSION;
		public bool isModInitialized;
		public static bool IsSteamAvailable { get; private set; }
		public static string? PendingJoinSteamID = null;


		private void InitializeSteam()
		{
			string dllPath = Path.Combine(Directory.GetCurrentDirectory(), "UserLibs", "steam_api64.dll");

			if (!File.Exists(dllPath))
			{
				IsSteamAvailable = false;
				MelonLogger.Warning("Steam DLL not found in UserLibs. Switching to Non-Steam mode.");
				return;
			}
			
			try 
			{
				SteamClient.Init(1190000);
				if (!SteamClient.IsValid || SteamClient.AppId.Value != 1190000)
				{
					IsSteamAvailable = false;
					SteamClient.Shutdown();
					MelonLogger.Warning("Steam environment invalid or emulated. Features disabled.");
					return;
				}
				
				SteamNetworkingUtils.InitRelayNetworkAccess();
				IsSteamAvailable = true;
				
				
				SteamFriends.OnGameRichPresenceJoinRequested += (friend, connectString) =>
				{
					MelonLogger.Msg($"[Steam] Joining {friend.Name} with data: {connectString}");
					PendingJoinSteamID = connectString;
				};
				
				MelonLogger.Msg("Steamworks initialized successfully.");
			}
			catch (Exception)
			{
				IsSteamAvailable = false;
				SteamClient.Shutdown();
				MelonLogger.Warning("Steamworks could not be initialized (Non-Steam version or Steam not running). Steam features will be disabled.");
			}
		}
		
		public static void TryExecutePendingJoin()
		{
			if (PendingJoinSteamID == null) return;


			if (Singleton<GameManager>.Instance == null) return;
			
			string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			if (sceneName != "Menu") return;
			
			if (UICore.MP_Lobby == null) return;

			ClientData.UserData.selectedNetworkType = NetworkType.Steam;
			UIActions.StartClient(ClientData.UserData.username, PendingJoinSteamID);
			MelonLogger.Msg($"[Steam] Game is ready! Joining {PendingJoinSteamID}...");
			PendingJoinSteamID = null;

		}
		
		public override void OnLateInitializeMelon()
		{
			InitializeSteam();
			Client.Instance = new Client();
			Server.Instance = new Server();
			ContentManager.Instance = new ContentManager();

			ClientData.UserData = TogetherModManager.LoadUserData();
			isModInitialized = true;
			LoggerInstance.Msg("Together Mod Initialized!");
		}

		public override void OnSceneWasLoaded(int buildindex, string sceneName)
		{
			if (!isModInitialized) return;
			
			if (sceneName == "Menu")
			{
				SaveSystem.Initialize();
				ContentManager.Instance.Initialize();

				ClientData.UserData.scene = SceneManager.UpdateScene(sceneName);
				Application.runInBackground = false;
			}
			UICore.InitializeUI(sceneName);
			if (Client.Instance.isConnected)
			{
				ClientData.UserData.UpdateScene(sceneName);
				
				if (SceneManager.CurrentScene() == GameScene.garage && ClientData.Instance.playerPrefab == null)
					ClientData.Instance.LoadPlayerPrefab();

			}
		}

		public override void OnUpdate()
		{
			if (!isModInitialized || !Client.Instance.isConnected)
				return;

			if (SceneManager.CurrentScene() == GameScene.garage)
				ClientData.Instance.UpdateClient();


			if (MainMod.IsSteamAvailable)
			{
				SteamClient.RunCallbacks();
				if (Client.Instance.steam != null) Client.Instance.steam.Receive();
				if (Server.Instance.steam != null) Server.Instance.steam.Receive();
				TryExecutePendingJoin();
			}
			
			ThreadManager.UpdateThread();
		}


		public static void StartCoroutine(IEnumerator routine)
		{
			GameManager.Instance.StartCoroutine(routine);
		}

		public override void OnLateUpdate()
		{
			if (!isModInitialized)
			{
				return;
			}
		}
		
		public override void OnInitializeMelon()
		{
			ClassInjector.RegisterTypeInIl2Cpp<InfoBillboard>();
		}

		public override void OnApplicationQuit()
		{
			TogetherModManager.SavePreferences();
			if (Server.Instance.isRunning)
				MelonCoroutines.Start(Server.Instance.CloseServer());
		}
	}
}