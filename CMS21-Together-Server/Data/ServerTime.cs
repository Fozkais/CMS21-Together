using System.Diagnostics;

namespace CMS21_Together_Server.Data
{
	public static class ServerTime
	{
		private static readonly Stopwatch _stopwatch = new Stopwatch();

		static ServerTime()
		{
			_stopwatch.Start();
		}
		
		/// <summary>
		/// Total seconds since the server started (Equivalent to Unity's Time.time)
		/// </summary>
		public static float Time => (float)_stopwatch.Elapsed.TotalSeconds;

		/// <summary>
		/// Total milliseconds since the server started
		/// </summary>
		public static long TotalMilliseconds => _stopwatch.ElapsedMilliseconds;
	}
}