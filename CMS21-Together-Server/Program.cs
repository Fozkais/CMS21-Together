using System;
using System.IO;
using System.Reflection;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;
using CMS21_Together_Server.Network;

namespace CMS21_Together_Server
{
	internal class Program
	{
		public const string SERVER_VERSION = "1.0";
		public const string MOD_VERSION = "0.5.0";
		public const int PORT = NetworkConstants.DEFAULT_PORT;

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
			
			MultiTextWriter multiWriter = new MultiTextWriter(uniqueLogPath, latestLogPath);
			Console.SetOut(multiWriter);
		}
		
		public static void Main(string[] args)
		{
			Terminal.Gui.Application.Init();
			
			Terminal.Gui.Colors.Base.Normal = Terminal.Gui.Application.Driver.MakeAttribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Black);
			Terminal.Gui.Colors.Base.Focus = Terminal.Gui.Application.Driver.MakeAttribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Black);
			Terminal.Gui.Colors.Base.HotNormal = Terminal.Gui.Application.Driver.MakeAttribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.Black);
			Terminal.Gui.Colors.Base.HotFocus = Terminal.Gui.Application.Driver.MakeAttribute(Terminal.Gui.Color.Cyan, Terminal.Gui.Color.Black);
			
			var window = new ServerWindow();
			
			SetupLogging();
			CMS21_Together_Core.Logging.Log.SetLogger(new ServerLoggerAdapter());
			Logger.Info($"CMS21 Together Server v{SERVER_VERSION}");
			PacketRouter.Initialize(Assembly.GetExecutingAssembly());
			
			Config = ServerConfig.LoadOrCreate();
			Logger.CurrentLogLevel = Config.LogLevel;
			Logger.Info($"Log Level set to: {Logger.CurrentLogLevel}");
			
			GameDatabase.Initialize();
			if (!GameDatabase.isInitialized)
			{
				Logger.Error("Game Data Initialization failed. Closing..");
				Exit();
				return;
			}
			
			Server.Start(Config.MaxPlayers, PORT);
			GameDataManager.TryLoadSession(null);
			Logger.Info($"Server started. Listening port {PORT}");
			
			Terminal.Gui.Application.Run(window);
			Terminal.Gui.Application.Shutdown();
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




