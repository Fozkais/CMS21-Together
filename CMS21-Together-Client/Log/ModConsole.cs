using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MelonLoader;

namespace CMS21Together.Logging
{
	// Dedicated log window for this mod, separate from MelonLoader's shared console.
	// MelonLogger's Msg/Warning/Error callbacks fire globally for every loaded mod, so
	// we filter on our own mod name to only capture CMS21-Together's own log lines -
	// existing MelonLogger.Msg/Warning/Error call sites across the mod don't need to change.
	public static class ModConsole
	{
		private const string MOD_NAME = "CMS21-Together";

		private static ModLogWindow _window;
		private static readonly ManualResetEventSlim WindowReady = new ManualResetEventSlim(false);

		public static void Initialize()
		{
			var uiThread = new Thread(() =>
			{
				Application.EnableVisualStyles();
				_window = new ModLogWindow();

				MelonLogger.MsgCallbackHandler += OnMelonMsg;
				MelonLogger.WarningCallbackHandler += OnMelonWarning;
				MelonLogger.ErrorCallbackHandler += OnMelonError;

				WindowReady.Set();
				Application.Run(_window);
			});
			uiThread.IsBackground = true;
			uiThread.SetApartmentState(ApartmentState.STA);
			uiThread.Start();

			WindowReady.Wait(TimeSpan.FromSeconds(5));
		}

		public static void AppendLog(string line, Color color)
		{
			_window?.AppendLog(line, color);
		}

		private static void OnMelonMsg(ConsoleColor melonColor, ConsoleColor color, string melonName, string txt)
		{
			if (melonName != MOD_NAME) return;
			AppendLog(txt, ToDrawingColor(color));
		}

		private static void OnMelonWarning(string melonName, string txt)
		{
			if (melonName != MOD_NAME) return;
			AppendLog(txt, Color.Orange);
		}

		private static void OnMelonError(string melonName, string txt)
		{
			if (melonName != MOD_NAME) return;
			AppendLog(txt, Color.Red);
		}

		private static Color ToDrawingColor(ConsoleColor consoleColor)
		{
			switch (consoleColor)
			{
				case ConsoleColor.Red: return Color.Red;
				case ConsoleColor.Yellow: return Color.Orange;
				case ConsoleColor.Green: return Color.LimeGreen;
				case ConsoleColor.Cyan: return Color.Cyan;
				case ConsoleColor.Gray:
				case ConsoleColor.DarkGray: return Color.Gray;
				default: return Color.Gainsboro;
			}
		}
	}
}
