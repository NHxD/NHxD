using System;

namespace Ash.System.Diagnostics
{
	public interface ILogger
	{
		LogFilters Level { get; }

		void TraceFormat(string format, params object[] args);
		void Trace(string contents);
		void TraceLineFormat(string format, params object[] args);
		void TraceLine(string contents);
		void LogFormat(string format, params object[] args);
		void Log(string contents);
		void LogLineFormat(string format, params object[] args);
		void LogLine(string contents);
		void InfoFormat(string format, params object[] args);
		void Info(string contents);
		void InfoLineFormat(string format, params object[] args);
		void InfoLine(string contents);
		void WarnFormat(string format, params object[] args);
		void Warn(string contents);
		void WarnLineFormat(string format, params object[] args);
		void WarnLine(string contents);
		void ErrorFormat(string format, params object[] args);
		void Error(string contents);
		void ErrorLineFormat(string format, params object[] args);
		void ErrorLine(string contents);
		void FatalFormat(string format, params object[] args);
		void Fatal(string contents);
		void FatalLineFormat(string format, params object[] args);
		void FatalLine(string contents);
		void WriteSeparator();
		void WriteSeparator(char separator);
		void WriteFormat(string format, params object[] args);
		void WriteLineFormat(string format, params object[] args);
		void Write(string contents);
		void WriteLine(string contents);
		void WriteLine();
	}

	[Flags]
	public enum LogFilters
	{
		None = 0,
		Fatal = 1,
		Error = 2,
		Warn = 4,
		Info = 8,
		Debug = 16,
		Trace = 32,
		All = Fatal | Error | Warn | Info | Debug | Trace,
	}
}
