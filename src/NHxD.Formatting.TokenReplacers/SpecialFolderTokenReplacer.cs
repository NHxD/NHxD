using System;

namespace NHxD.Formatting.TokenReplacers
{
	public class SpecialFolderTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "SpecialFolder";

		public SpecialFolderTokenReplacer()
		{
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("MyDocuments", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/";
					}
					else if (namespaces[1].Equals("MyPictures", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "/";
					}
					else if (namespaces[1].Equals("UserProfile", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/";
					}
					else if (namespaces[1].Equals("ApplicationData", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/";
					}
					else if (namespaces[1].Equals("CommonApplicationData", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "/";
					}
					else if (namespaces[1].Equals("LocalApplicationData", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/";
					}
					else if (namespaces[1].Equals("Desktop", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/";
					}
					else if (namespaces[1].Equals("InternetCache", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "/";
					}
					else if (namespaces[1].Equals("Cookies", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.Cookies) + "/";
					}
					else if (namespaces[1].Equals("History", StringComparison.OrdinalIgnoreCase))
					{
						result = Environment.GetFolderPath(Environment.SpecialFolder.History) + "/";
					}
				}
			}

			return result;
		}
	}
}
