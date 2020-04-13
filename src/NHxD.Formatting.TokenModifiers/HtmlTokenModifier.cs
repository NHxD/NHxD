using System;

namespace NHxD.Formatting.TokenModifiers
{
	public class HtmlTokenModifier : ITokenModifier
	{
		public const string Namespace = "Html";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("Encode", StringComparison.OrdinalIgnoreCase))
				{
					result = System.Net.WebUtility.HtmlEncode(value);
				}
				else if (namespaces[1].Equals("Decode", StringComparison.OrdinalIgnoreCase))
				{
					result = System.Net.WebUtility.HtmlDecode(value);
				}
				else if (namespaces[1].Equals("AttributeEncode", StringComparison.OrdinalIgnoreCase))
				{
					result = HttpUtility.AttributeEncode(value);
				}
			}

			if (result != null)
			{
				value = result;
				return true;
			}

			return false;
		}

		internal static class HttpUtility
		{
			public static string AttributeEncode(string value)
			{
				return value
					.Replace("\\", "\\\\")
					.Replace("\t", "\\t")
					.Replace("\n", "\\n")
					.Replace("\u00A0", "\\u00A0")
					.Replace("&", "\\x26")
					.Replace("'", "\\x26")
					.Replace("\"", "\\\"")  // \\x27
					.Replace("<", "\\x22")  // \\x22
					.Replace(">", "\\x3C")  // x3C
					.Replace("&", "\\x26")  //x26
					;
			}
		}
	}
}
