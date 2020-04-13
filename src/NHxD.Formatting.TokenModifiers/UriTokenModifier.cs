using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace NHxD.Formatting.TokenModifiers
{
	public class UriTokenModifier : ITokenModifier
	{
		public const string Namespace = "Uri";

		private static readonly string InvalidPathCharactersString = Regex.Escape(new string(Path.GetInvalidPathChars()));
		private static readonly Regex InvalidPathCharactersRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", InvalidPathCharactersString), RegexOptions.Compiled);
		//private static readonly string InvalidPathCharactersRegexPattern = string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", InvalidPathCharactersString);

		private static readonly string InvalidFileNameCharactersString = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
		private static readonly Regex InvalidFileNameCharactersRegex = new Regex(string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", InvalidFileNameCharactersString), RegexOptions.Compiled);
		//private static readonly string InvalidFileNameCharactersRegexPattern = string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", InvalidFileNameCharactersString);

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("EscapeDataString", StringComparison.OrdinalIgnoreCase))
				{
					result = Uri.EscapeDataString(value);
				}
				else if (namespaces[1].Equals("FixPath", StringComparison.OrdinalIgnoreCase))
				{
					result = value.Replace('\\', '/');
				}
				else if (namespaces[1].Equals("SanitizePath", StringComparison.OrdinalIgnoreCase))
				{
					result = InvalidPathCharactersRegex.Replace(value, "_");
				}
				else if (namespaces[1].StartsWith("SanitizePath,", StringComparison.OrdinalIgnoreCase))
				{
					string[] formats = namespaces[1].Split(new char[] { ',' });

					if (formats.Length > 1)
					{
						result = InvalidPathCharactersRegex.Replace(value, formats[1]);
					}
				}
				else if (namespaces[1].Equals("SanitizeFileName", StringComparison.OrdinalIgnoreCase))
				{
					result = InvalidFileNameCharactersRegex.Replace(value, "_");
				}
				else if (namespaces[1].StartsWith("SanitizeFileName,", StringComparison.OrdinalIgnoreCase))
				{
					string[] formats = namespaces[1].Split(new char[] { ',' });

					if (formats.Length > 1)
					{
						result = InvalidFileNameCharactersRegex.Replace(value, formats[1]);
					}
				}
			}

			if (result != null)
			{
				value = result;
				return true;
			}

			return false;
		}
	}
}
