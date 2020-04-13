using System;
using System.Diagnostics;
using System.Globalization;

namespace Ash.System.Diagnostics
{
	public sealed class StopwatchLog : IDisposable
	{
		private readonly Stopwatch stopwatch;

		public string Header { get; }
		public Logger Logger { get; }

		public StopwatchLog(Logger logger, string header)
		{
			Logger = logger;
			Header = header;

			stopwatch = Stopwatch.StartNew();
		}

		public void Dispose()
		{
			stopwatch.Stop();

			Logger.WriteLineFormat(string.Format(CultureInfo.InvariantCulture, "{0}: {1}",
				Header.PadRight(32, '.'),
				stopwatch.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture)));
		}
	}
}
