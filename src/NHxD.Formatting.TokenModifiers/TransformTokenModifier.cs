using System;
using System.Globalization;

namespace NHxD.Formatting.TokenModifiers
{
	public class TransformTokenModifier : ITokenModifier
	{
		public const string Namespace = "Transform";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("ToLowerCase", StringComparison.OrdinalIgnoreCase))
				{
					result = value.ToLower(CultureInfo.CurrentCulture);
				}
				else if (namespaces[1].Equals("ToUpperCase", StringComparison.OrdinalIgnoreCase))
				{
					result = value.ToUpper(CultureInfo.CurrentCulture);
				}
				else if (namespaces[1].Equals("ToTitleCase", StringComparison.OrdinalIgnoreCase))
				{
					result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
				}
				else if (namespaces[1].Equals("ToCamelCase", StringComparison.OrdinalIgnoreCase)
					|| namespaces[1].Equals("ToPascalCase", StringComparison.OrdinalIgnoreCase)
					|| namespaces[1].Equals("ToTitleCase", StringComparison.OrdinalIgnoreCase))
				{
					result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);

					if (!char.IsLower(value[0]))
					{
						result = Char.ToLower(result[0], CultureInfo.CurrentCulture) + result.Substring(1);
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
