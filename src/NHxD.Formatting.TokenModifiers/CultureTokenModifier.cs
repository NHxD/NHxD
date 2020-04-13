using System;
using System.Globalization;

namespace NHxD.Formatting.TokenModifiers
{
	public class CultureTokenModifier : ITokenModifier
	{
		public const string Namespace = "Culture";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("Localize", StringComparison.OrdinalIgnoreCase))
				{
					int intValue;

					if (int.TryParse(value, out intValue))
					{
						result = intValue.ToString("N", CultureInfo.CurrentCulture);
					}
				}
				else if (namespaces[1].StartsWith("Localize,", StringComparison.OrdinalIgnoreCase))
				{
					string[] formats = namespaces[1].Split(new char[] { ',' });
					int intValue;

					if (int.TryParse(value, out intValue))
					{
						try
						{
							result = intValue.ToString(formats[1], CultureInfo.CurrentCulture);
						}
						catch
						{
						}
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
