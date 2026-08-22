using System;
using System.IO;
using CMS21_Together_Core.Network;
using CMS21Together.Data;
using CMS21Together.Log;
using CMS21Together.Managers;
using CMS21Together.Network;
using MelonLoader;
using Steamworks;
using UnityEngine;

// ReSharper disable All

namespace CMS21Together
{
	public class MainMod : MelonMod
	{
		public const int MAX_PLAYER = 4;
		public const int PORT = NetworkConstants.DEFAULT_PORT;
		public const string ASSEMBLY_MOD_VERSION = "0.5.0" + ASSEMBLY_HOTFIX_VERSION;
		public const string ASSEMBLY_HOTFIX_VERSION = "";
		public const string MOD_VERSION = "Together " + ASSEMBLY_MOD_VERSION + ASSEMBLY_HOTFIX_VERSION;
		
		public bool isModInitialized;
		public static bool IsSteamAvailable { get; private set; }

		public override void OnLateInitializeMelon()
		{
			CMS21_Together_Core.Logging.Log.SetLogger(new ClientLoggerAdapter());

			InitializeSteam();

			PacketRouter.Initialize(System.Reflection.Assembly.GetExecutingAssembly());
			Client.Init();
			
			LoggerInstance.Msg("Together Mod Initialized!");
			isModInitialized = true;
		}
		
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
				MelonLogger.Msg("Steamworks initialized successfully.");
			}
			catch (Exception)
			{
				IsSteamAvailable = false;
				SteamClient.Shutdown();
				MelonLogger.Warning("Steamworks could not be initialized (Non-Steam version or Steam not running). Steam features will be disabled.");
			}
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
			if (Input.GetKeyDown(KeyCode.F6) && IsSteamAvailable)
			{
				LoggerInstance.Msg("Steam Connection Attempt...");
				Client.Instance.ConnectToSteamServer();
			}
			
			if (Client.Instance.IsConnectionValid)
				ClientData.Update();

			if (IsSteamAvailable)
			{
				SteamClient.RunCallbacks();
				if (Client.Instance.IsConnected) Client.Instance.Steam?.Receive();
			}
			ThreadManager.UpdateThread();
		}

		public override void OnLateUpdate() { }

		public override void OnInitializeMelon()
		{
			ModConsole.Initialize();
		}

		public override void OnApplicationQuit() { }
	}
}