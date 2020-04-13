using System;
using System.Globalization;

namespace NHxD.Formatting.TokenReplacers
{
	public class TagTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Tag";

		public TagInfo Tag { get; }

		public TagTokenReplacer(TagInfo tag)
		{
			Tag = tag;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("Id", StringComparison.OrdinalIgnoreCase))
					{
						result = Tag.Id.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Name", StringComparison.OrdinalIgnoreCase))
					{
						result = Tag.Name;
					}
					else if (namespaces[1].Equals("Type", StringComparison.OrdinalIgnoreCase))
					{
						result = Tag.Type.ToString();
					}
					else if (namespaces[1].Equals("Count", StringComparison.OrdinalIgnoreCase))
					{
						result = Tag.Count.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Url", StringComparison.OrdinalIgnoreCase))
					{
						result = Tag.Url;
					}
				}
			}

			return result;
		}
	}
}
