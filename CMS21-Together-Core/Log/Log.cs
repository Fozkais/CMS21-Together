namespace CMS21_Together_Core.Logging;

// Shared logging gateway. Client and Server each install their own ILogger
// implementation at startup (see ServerLoggerAdapter / ClientLoggerAdapter),
// so Core code (PacketRouter, etc.) can log without depending on either side.
public static class Log
{
	private static ILogger _logger;

	public static void SetLogger(ILogger logger)
	{
		_logger = logger;
	}

	public static void Debug(string message) => _logger?.Debug(message);
	public static void Info(string message) => _logger?.Info(message);
	public static void Warn(string message) => _logger?.Warn(message);
	public static void Error(string message) => _logger?.Error(message);
	public static void Success(string message) => _logger?.Success(message);
}
