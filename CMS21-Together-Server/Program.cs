using System;
using System.Reflection;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Server.Network;

namespace CMS21_Together_Server
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("- CMS21 Together Server v1.0 -");
			PacketRouter.Initialize(Assembly.GetExecutingAssembly());

			Server.Start(4, 7777);
			Console.WriteLine("Server started. Listening port 7777");
			
			bool isRunning = true;
			bool wantToExit = false;
			while (isRunning)
			{
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