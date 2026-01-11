using CMS21_Together_Core.Network;
using CMS21Together.Network;
using MelonLoader;
using Steamworks;
using UnityEngine;

// ReSharper disable All

namespace CMS21Together
{
	public class MainMod : MelonMod
	{
		public const int MAX_SAVE_COUNT = 22;
		public const int MAX_PLAYER = 4;
		public const int PORT = 7777;
		public const string ASSEMBLY_MOD_VERSION = "0.5.0" + ASSEMBLY_HOTFIX_VERSION;
		public const string ASSEMBLY_HOTFIX_VERSION = "";
		public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION + ASSEMBLY_HOTFIX_VERSION;
		
		public bool isModInitialized;

		public override void OnLateInitializeMelon()
		{
			SteamClient.Init(1190000);
			SteamNetworkingUtils.InitRelayNetworkAccess();
			
			PacketRouter.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
			Client.Init();
			
			LoggerInstance.Msg("Together Mod Initialized!");
			isModInitialized = true;
		}

		public override void OnSceneWasLoaded(int buildindex, string sceneName) { }

		public override void OnUpdate()
		{
			if (!isModInitialized )
				return;
			
			if (Input.GetKeyDown(KeyCode.F5))
			{
				LoggerInstance.Msg("Local Connection Attempt...");
				Client.Instance.ConnectToServer("127.0.0.1");
			}
			if (Input.GetKeyDown(KeyCode.F6))
			{
				LoggerInstance.Msg("Steam Connection Attempt...");
				Client.Instance.ConnectToSteamServer();
			}
			
			SteamClient.RunCallbacks();
			if (Client.Instance.isConnected) Client.Instance.steam.Receive();
			ThreadManager.UpdateThread();
		}

		public override void OnLateUpdate() { }

		public override void OnInitializeMelon() { }

		public override void OnApplicationQuit() { }
	}
}