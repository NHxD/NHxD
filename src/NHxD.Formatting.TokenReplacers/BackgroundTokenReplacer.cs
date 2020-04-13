using System;
using System.IO;

namespace NHxD.Formatting.TokenReplacers
{
	public class BackgroundTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Background";

		public IPathFormatter PathFormatter { get; }

		public BackgroundTokenReplacer(IPathFormatter pathFormatter)
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
					if (namespaces[1].Equals("Image", StringComparison.OrdinalIgnoreCase))
					{
						try
						{
							string cacheDirectory = PathFormatter.GetCacheDirectory();

							if (Directory.Exists(cacheDirectory))
							{
								DirectoryInfo dirInfo = new DirectoryInfo(cacheDirectory);
								FileInfo[] coverFiles = dirInfo.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
								Random rand = new Random();
								int randomIndex = rand.Next(coverFiles.Length);

								result = coverFiles[randomIndex].FullName;
							}
							else
							{
								result = "";
							}
						}
						catch
						{
							result = "";
						}
					}
				}
			}

			return result;
		}
	}
}
