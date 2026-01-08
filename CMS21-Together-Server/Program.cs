using System;
using System.Reflection;
using System.Threading;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;

namespace CMS21_Together_Server
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Running CMS21 Together Server v1.0 !");
			PacketRouter.Initialize(typeof(PacketTypes).Assembly);
			
			while (true)
			{
				if (Console.ReadLine() == "exit")
					break;
				Thread.Sleep(1000);
			}
		}
	}
}