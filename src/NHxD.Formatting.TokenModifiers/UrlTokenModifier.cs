using System;

namespace NHxD.Formatting.TokenModifiers
{
	public class UrlTokenModifier : ITokenModifier
	{
		public const string Namespace = "Url";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("Encode", StringComparison.OrdinalIgnoreCase))
				{
					result = System.Net.WebUtility.UrlEncode(value);
				}
				else if (namespaces[1].Equals("Decode", StringComparison.OrdinalIgnoreCase))
				{
					result = System.Net.WebUtility.UrlDecode(value);
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
