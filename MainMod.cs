using CMS21Together.ClientSide;
using CMS21Together.ClientSide.Data;
using CMS21Together.ClientSide.Data.CustomUI;
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
		public const string ASSEMBLY_MOD_VERSION = "0.4.9";
		public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION;
		public bool isModInitialized;
		
		public static bool isClosing;

		public override void OnLateInitializeMelon()
		{
			Client.Instance = new Client();
			Server.Instance = new Server();
			ContentManager.Instance = new ContentManager();

			ClientData.UserData = TogetherModManager.LoadUserData();
			if (ApiCalls.useSteam)
				Shared.SteamManager.Instance = new Shared.SteamManager();
			isModInitialized = true;
			LoggerInstance.Msg("Together Mod Initialized!");
		}

		public override void OnSceneWasLoaded(int buildindex, string sceneName)
		{
			if (!isModInitialized)
			{
				return;
			}

			CustomUIManager.OnSceneChange(sceneName);

			if (sceneName == "Menu")
			{
				ContentManager.Instance.Initialize();

				ClientData.UserData.scene = SceneManager.UpdateScene(sceneName);
				Application.runInBackground = false;
			}

			if (Client.Instance.isConnected)
			{
				if (sceneName == "garage" && ClientData.Instance.playerPrefab == null)
					ClientData.Instance.LoadPlayerPrefab();

				ClientData.UserData.UpdateScene(sceneName);
			}
		}

		public override void OnUpdate()
		{
			if (!isModInitialized || !Client.Instance.isConnected)
				return;

			if (SceneManager.CurrentScene() == GameScene.garage)
				ClientData.Instance.UpdateClient();


			if (ApiCalls.useSteam)
			{
				SteamClient.RunCallbacks();
				if (Client.Instance.steam != null) Client.Instance.steam.Receive();
				if (Server.Instance.steam != null) Server.Instance.steam.Receive();
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
			isClosing = true;
			TogetherModManager.SavePreferences();
			if (Server.Instance.isRunning)
				MelonCoroutines.Start(Server.Instance.CloseServer());
		}
	}
}