using System;
using System.Reflection;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Network;
using Steamworks;

namespace CMS21_Together_Server
{
	internal class Program
	{
		public const string SERVER_VERSION = "1.0";
		public const string MOD_VERSION = "0.5.0";
		public const int PORT = 7777;
		public const int MAX_PLAYERS = 4;

		public const bool USE_STEAM = true;
		
		public static void Main(string[] args)
		{
			Console.WriteLine($"- CMS21 Together Server v{SERVER_VERSION} -");
			PacketRouter.Initialize(Assembly.GetExecutingAssembly());

			Server.Start(MAX_PLAYERS, PORT);
			Console.WriteLine($"Server started. Listening port {PORT}");
			
			bool isRunning = true;
			bool wantToExit = false;
			while (isRunning)
			{
				if (USE_STEAM && Server.steamTransport.isInitialized)
					Server.steamTransport.Update();
				
				string cmd = Console.ReadLine();
				if (cmd == "exit")
				{
					wantToExit = true;
					Server.Stop();
				}
				if (Console.ReadLine() == "" && wantToExit)
					isRunning = false;
			}
		}
	}
}




