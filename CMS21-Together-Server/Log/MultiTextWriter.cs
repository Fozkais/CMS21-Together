using System.IO;
using System.Text;

namespace CMS21_Together_Server.Log
{
	public class MultiTextWriter : TextWriter
	{
		private readonly TextWriter _originalConsole;
		private readonly StreamWriter _logFileWriter;
		private readonly StreamWriter _latestFileWriter;

		public MultiTextWriter(string logPath, string latestPath)
		{
        
			// Open the specific log file (append mode)
			_logFileWriter = new StreamWriter(logPath, true) { AutoFlush = true };
        
			// Open the 'Latest' file (create or overwrite)
			_latestFileWriter = new StreamWriter(latestPath, false) { AutoFlush = true };
		}

		public override Encoding Encoding => _originalConsole.Encoding;

		// Override Write(char) - the most basic method
		public override void Write(char value)
		{
			_logFileWriter.Write(value);
			_latestFileWriter.Write(value);
		}

		// Override Write(string) for better performance with strings
		public override void Write(string value)
		{
			_logFileWriter.Write(value);
			_latestFileWriter.Write(value);
		}

		// Override WriteLine for convenience
		public override void WriteLine(string value)
		{
			_logFileWriter.WriteLine(value);
			_latestFileWriter.WriteLine(value);
		}

		// Clean up resources
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_logFileWriter?.Dispose();
				_latestFileWriter?.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}