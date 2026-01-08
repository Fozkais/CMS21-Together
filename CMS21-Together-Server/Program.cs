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
			try 
			{
				// Attempt to initialize
				PacketRouter.Initialize(Assembly.GetExecutingAssembly());
			}
			catch (ReflectionTypeLoadException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("!!! FATAL ERROR: MISSING DEPENDENCIES IN CORE !!!");
				foreach (Exception loaderEx in ex.LoaderExceptions)
				{
					Console.WriteLine($"- {loaderEx.Message}");
				}
				Console.ResetColor();
				Console.ReadLine();
				return;
			}

			Server.Start(4, 7777);
			Console.WriteLine("Server started. Listening port 7777");
			
			bool isRunning = true;
			while (isRunning)
			{
				string cmd = Console.ReadLine();
				if (cmd == "exit")
				{
					isRunning = false;
					Server.Stop();
				}
			}
		}
	}
}