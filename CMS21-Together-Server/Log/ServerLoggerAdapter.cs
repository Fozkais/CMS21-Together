using CoreLog = CMS21_Together_Core.Logging;

namespace CMS21_Together_Server.Log
{
	public class ServerLoggerAdapter : CoreLog.ILogger
	{
		public void Debug(string message) => Logger.Debug(message);
		public void Info(string message) => Logger.Info(message);
		public void Warn(string message) => Logger.Warn(message);
		public void Error(string message) => Logger.Error(message);
		public void Success(string message) => Logger.Success(message);
	}
}
