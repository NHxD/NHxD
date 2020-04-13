using System;

namespace NHxD.Formatting.TokenModifiers
{
	public class XmlTokenModifier : ITokenModifier
	{
		public const string Namespace = "Xml";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("Escape", StringComparison.OrdinalIgnoreCase))
				{
					result = System.Security.SecurityElement.Escape(value);
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
