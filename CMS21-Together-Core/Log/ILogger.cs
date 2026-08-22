namespace CMS21_Together_Core.Logging;

public interface ILogger
{
	void Debug(string message);
	void Info(string message);
	void Warn(string message);
	void Error(string message);
	void Success(string message);
}
