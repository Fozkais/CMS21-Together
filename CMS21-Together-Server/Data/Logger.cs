using System;

namespace CMS21_Together_Server.Data
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
		
		public static void DebugNoLine(string message, string prefix="")
		{
			if (CurrentLogLevel < 1) return;
			WriteLogSameLine(prefix, message, ConsoleColor.DarkGray);
		}

		private static void WriteLogSameLine(string prefix, string message, ConsoleColor color)
		{
			ConsoleColor originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;

			string logLine = message;
			if (prefix != "")
				logLine = $"[{DateTime.Now:HH:mm:ss}] [{prefix}] {message}";
			Console.Write(logLine);
			
			Console.ForegroundColor = originalColor;
		}
		
		private static void WriteLog(string prefix, string message, ConsoleColor color)
		{
			ConsoleColor originalColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			
			string logLine = $"[{DateTime.Now:HH:mm:ss}] [{prefix}] {message}";
			Console.WriteLine(logLine);
			
			Console.ForegroundColor = originalColor;
		}
	}
}