using System;
using System.IO;

namespace NHxD.Formatting.TokenReplacers
{
	public class PathTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Path";

		public IPathFormatter PathFormatter { get; }

		public PathTokenReplacer(IPathFormatter pathFormatter)
		{
			PathFormatter = pathFormatter;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("Application", StringComparison.OrdinalIgnoreCase))
					{
						result = PathFormatter.ApplicationPath;
					}
					else if (namespaces[1].Equals("Source", StringComparison.OrdinalIgnoreCase))
					{
						result = PathFormatter.SourcePath;
					}
					else if (namespaces[1].Equals("TempPath", StringComparison.OrdinalIgnoreCase))
					{
						result = Path.GetTempPath();
					}
					else if (namespaces[1].Equals("TempFileName", StringComparison.OrdinalIgnoreCase))
					{
						result = Path.GetTempFileName();
					}
					else if (namespaces[1].Equals("InvalidPath", StringComparison.OrdinalIgnoreCase))
					{
						result = new string(Path.GetInvalidPathChars());
					}
					else if (namespaces[1].Equals("InvalidFileName", StringComparison.OrdinalIgnoreCase))
					{
						result = new string(Path.GetInvalidFileNameChars());
					}
				}
			}

			return result;
		}
	}
}
