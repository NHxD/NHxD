#define TRACE

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Ash.System.Diagnostics
{
	public class Logger : ILogger
	{
		public const char DefaultSeparatorChar = '-';
		public static readonly string DefaultSeparatorString = new string(DefaultSeparatorChar, 1);

		public string FileName { get; }
		public LogFilters Level { get; }

		public Logger()
		{
		}

		public Logger(string fileName, LogFilters level)
		{
			FileName = fileName;
			Level = level;

			Create();
		}

		public void TraceFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Trace, format, args);
		}

		public void Trace(string contents)
		{
			Write(LogFilters.Trace, contents);
		}

		public void TraceLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Trace, format, args);
		}

		public void TraceLine(string contents)
		{
			WriteLine(LogFilters.Trace, contents);
		}


		public void LogFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Debug, format, args);
		}

		public void Log(string contents)
		{
			Write(LogFilters.Debug, contents);
		}

		public void LogLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Debug, format, args);
		}

		public void LogLine(string contents)
		{
			WriteLine(LogFilters.Debug, contents);
		}


		public void InfoFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Info, format, args);
		}

		public void Info(string contents)
		{
			Write(LogFilters.Info, contents);
		}

		public void InfoLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Info, format, args);
		}

		public void InfoLine(string contents)
		{
			WriteLine(LogFilters.Info, contents);
		}


		public void WarnFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Warn, format, args);
		}

		public void Warn(string contents)
		{
			Write(LogFilters.Warn, contents);
		}

		public void WarnLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Warn, format, args);
		}

		public void WarnLine(string contents)
		{
			WriteLine(LogFilters.Warn, contents);
		}


		public void ErrorFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Error, format, args);
		}

		public void Error(string contents)
		{
			Write(LogFilters.Error, contents);
		}

		public void ErrorLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Error, format, args);
		}

		public void ErrorLine(string contents)
		{
			WriteLine(LogFilters.Error, contents);
		}


		public void FatalFormat(string format, params object[] args)
		{
			WriteFormat(LogFilters.Fatal, format, args);
		}

		public void Fatal(string contents)
		{
			Write(LogFilters.Fatal, contents);
		}

		public void FatalLineFormat(string format, params object[] args)
		{
			WriteLineFormat(LogFilters.Fatal, format, args);
		}

		public void FatalLine(string contents)
		{
			WriteLine(LogFilters.Fatal, contents);
		}

		public void WriteSeparator()
		{
			WriteLine(DefaultSeparatorString.PadRight(80, DefaultSeparatorChar));
		}

		public void WriteSeparator(char separator)
		{
			WriteLine((new string(separator, 1)).PadRight(80, separator));
		}


		public void WriteFormat(string format, params object[] args)
		{
			Write(string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void WriteLineFormat(string format, params object[] args)
		{
			WriteLine(string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void Write(string contents)
		{
			if (!IsValid())
			{
				return;
			}

			try
			{
				global::System.Diagnostics.Trace.Write(contents);
			}
			catch (Exception)
			{

			}
		}

		public void WriteLine(string contents)
		{
			if (!IsValid())
			{
				return;
			}

			StringBuilder sb = new StringBuilder();

			sb.AppendLine(contents);

			try
			{
				global::System.Diagnostics.Trace.Write(sb.ToString());
			}
			catch (Exception)
			{

			}
		}

		public void WriteLine()
		{
			Write(Environment.NewLine);
		}




		private void WriteFormat(LogFilters filters, string format, params object[] args)
		{
			if (!Level.HasFlag(filters))
			{
				return;
			}

			WriteFormat(string.Format(CultureInfo.InvariantCulture, "[{0}] ({1}) {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), filters, format), args);
		}

		private void Write(LogFilters filters, string contents)
		{
			if (!Level.HasFlag(filters))
			{
				return;
			}

			WriteFormat(string.Format(CultureInfo.InvariantCulture, "[{0}] ({1}) {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), filters, contents));
		}

		private void WriteLineFormat(LogFilters filters, string format, params object[] args)
		{
			if (!Level.HasFlag(filters))
			{
				return;
			}

			WriteLineFormat(string.Format(CultureInfo.InvariantCulture, "[{0}] ({1}) {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), filters, format), args);
		}

		private void WriteLine(LogFilters filters, string contents)
		{
			if (!Level.HasFlag(filters))
			{
				return;
			}

			WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}] ({1}) {2}", DateTime.Now.ToString(CultureInfo.InvariantCulture), filters, contents));
		}

		private void EnsureDirectoryExists()
		{
			string directoryName = Path.GetDirectoryName(FileName);

			Directory.CreateDirectory(directoryName);
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(FileName);
		}

		private void Create()
		{
			if (!IsValid())
			{
				return;
			}

			try
			{
				EnsureDirectoryExists();
				global::System.Diagnostics.Trace.AutoFlush = true;
				global::System.Diagnostics.Trace.Listeners.Add(new TextWriterTraceListener(FileName));
			}
			catch (Exception)
			{

			}
		}
	}
}
