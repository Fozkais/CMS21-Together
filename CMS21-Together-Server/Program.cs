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
			PacketRouter.Initialize(Assembly.GetExecutingAssembly());
			Logger.Info($"CMS21 Together Server v{SERVER_VERSION}");
			
			Config = ServerConfig.LoadOrCreate();

			Server.Start(Config.MaxPlayers, PORT);
			Logger.Info($"Server started. Listening port {PORT}");
			
			bool isRunning = true;
			while (isRunning)
			{
				if (Config.UseSteam && Server.steamTransport != null)
				{
					Server.steamTransport.Update();
				}
				
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
			Server.Stop();
		}
	}
}




