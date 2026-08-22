using System.Drawing;
using CoreLog = CMS21_Together_Core.Logging;

namespace CMS21Together.Logging
{
	public class ClientLoggerAdapter : CoreLog.ILogger
	{
		public void Debug(string message) => ModConsole.AppendLog(message, Color.Gray);
		public void Info(string message) => ModConsole.AppendLog(message, Color.Gainsboro);
		public void Warn(string message) => ModConsole.AppendLog(message, Color.Orange);
		public void Error(string message) => ModConsole.AppendLog(message, Color.Red);
		public void Success(string message) => ModConsole.AppendLog(message, Color.LimeGreen);
	}
}
