using System;
using System.Text;

namespace NHxD.Formatting.TokenModifiers
{
	public class EncodingTokenModifier : ITokenModifier
	{
		public const string Namespace = "Encoding";

		public bool Mutate(string[] tokens, string[] namespaces, ref string value, ref int tokenIndex)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces[1].Equals("Base64", StringComparison.OrdinalIgnoreCase))
				{
					if (namespaces[2].Equals("Encode", StringComparison.OrdinalIgnoreCase))
					{
						byte[] bytes = Encoding.UTF8.GetBytes(value);

						result = Convert.ToBase64String(bytes);
					}
					else if (namespaces[2].Equals("Decode", StringComparison.OrdinalIgnoreCase))
					{
						byte[] bytes = Convert.FromBase64String(value);

						result = Encoding.UTF8.GetString(bytes);
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
