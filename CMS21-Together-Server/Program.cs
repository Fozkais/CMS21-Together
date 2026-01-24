using System;
using System.IO;
using System.Reflection;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Network;
using Steamworks;

namespace CMS21_Together_Server
{
	internal class Program
	{
		public const string SERVER_VERSION = "1.0";
		public const string MOD_VERSION = "0.5.0";
		public const int PORT = 7777;

		public const int CONNECTION_TIMEOUT = 10;

		public static ServerConfig Config { get; private set; }
		
		static void SetupLogging()
		{
			string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
			if (!Directory.Exists(logDirectory))
			{
				Directory.CreateDirectory(logDirectory);
			}
			
			string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
			string uniqueLogFileName = $"Log_{timestamp}.txt";
			string uniqueLogPath = Path.Combine(logDirectory, uniqueLogFileName);
			
			string latestLogPath = Path.Combine(logDirectory, "Latest.txt");
			
			MultiTextWriter multiWriter = new MultiTextWriter(Console.Out, uniqueLogPath, latestLogPath);
			Console.SetOut(multiWriter);
		}
		
		public static void Main(string[] args)
		{
			SetupLogging();
			Logger.Info($"CMS21 Together Server v{SERVER_VERSION}");
			PacketRouter.Initialize(Assembly.GetExecutingAssembly());
			
			Config = ServerConfig.LoadOrCreate();
			Logger.CurrentLogLevel = Config.LogLevel;
			Logger.Info($"Log Level set to: {Logger.CurrentLogLevel}");
			
			GameDataManager.Initialize();
			if (!GameDataManager.isInitialized)
			{
				Logger.Error("Game Data Initialization failed. Closing..");
				Exit();
				return;
			}
			
			Server.Start(Config.MaxPlayers, PORT);
			ServerGameState.TryLoadSession(null);
			Logger.Info($"Server started. Listening port {PORT}");
			
			bool isRunning = true;
			while (isRunning)
			{
				Server.Update();
				if (Console.KeyAvailable)
				{
					string cmd = Console.ReadLine();
					if (cmd == "exit")
					{
						isRunning = false;
					}
				}
				Thread.Sleep(10);
			}
			Exit();
		}

		private static void Exit()
		{
			Server.Stop();
			Logger.Info("Press Any key to exit..");
			Console.ReadKey();
		}
	}
}




