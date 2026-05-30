using System;

namespace CMS21_Together_Server.Log
{
	public static class Logger
	{
		public static int CurrentLogLevel { get; set; } = 0;
		
		public static void Info(string message)
		{
			WriteLog("INFO", message, ConsoleColor.Cyan);
		}

		public static void Warn(string message)
		{
			WriteLog("WARN", message, ConsoleColor.Yellow);
		}

		public static void Error(string message)
		{
			WriteLog("ERROR", message, ConsoleColor.Red);
		}

		public static void Success(string message)
		{
			WriteLog("SUCCESS", message, ConsoleColor.Green);
		}

		public static void Debug(string message)
		{
			if (CurrentLogLevel < 1) return;
			WriteLog("DEBUG", message, ConsoleColor.DarkGray);
		}
		
		public static void DebugNoNL(string message, string prefix="")
		{
			if (CurrentLogLevel < 1) return;
			WriteLogSameLine(prefix, message, ConsoleColor.DarkGray);
		}

		private static void WriteLogSameLine(string prefix, string message, ConsoleColor color)
		{
			string logLine = message;
			if (prefix != "")
				logLine = $"[{DateTime.Now:HH:mm:ss}] [{prefix}] {message}";
			
			if (ServerWindow.LogView != null)
			{
				ServerWindow.LogView.AddLog(logLine, color);
			}
			// Write to standard console out which is hooked by MultiTextWriter (for file writing)
			// But since we override Console UI, we shouldn't use Console.WriteLine directly anymore.
			// Let's use standard output only for the files.
			Console.Out.Write(logLine);
		}
		
		private static void WriteLog(string prefix, string message, ConsoleColor color)
		{
			string logLine = $"[{DateTime.Now:HH:mm:ss}] [{prefix}] {message}";
			
			if (ServerWindow.LogView != null)
			{
				ServerWindow.LogView.AddLog(logLine, color);
			}
			Console.Out.WriteLine(logLine);
		}
	}
}