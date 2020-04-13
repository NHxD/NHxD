using Newtonsoft.Json;
using System;
using System.Linq;

namespace NHxD.Formatting.TokenReplacers
{
	public class MetadataKeywordListTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "MetadataKeywordList";

		public MetadataKeywordList Whitelist { get; }
		public MetadataKeywordList Blacklist { get; }
		public MetadataKeywordList Ignorelist { get; }
		public MetadataKeywordList Hidelist { get; }

		public MetadataKeywordListTokenReplacer(MetadataKeywordList whitelist, MetadataKeywordList blacklist, MetadataKeywordList ignorelist, MetadataKeywordList hidelist)
		{
			Whitelist = whitelist;
			Blacklist = blacklist;
			Ignorelist = ignorelist;
			Hidelist = hidelist;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("Whitelist", StringComparison.OrdinalIgnoreCase))
					{
						bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

						result = JsonConvert.SerializeObject(Whitelist, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None) ?? "";
					}
					else if (namespaces[1].Equals("Blacklist", StringComparison.OrdinalIgnoreCase))
					{
						bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

						result = JsonConvert.SerializeObject(Blacklist, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None) ?? "";
					}
					else if (namespaces[1].Equals("Ignorelist", StringComparison.OrdinalIgnoreCase))
					{
						bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

						result = JsonConvert.SerializeObject(Ignorelist, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None) ?? "";
					}
					else if (namespaces[1].Equals("Hidelist", StringComparison.OrdinalIgnoreCase))
					{
						bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

						result = JsonConvert.SerializeObject(Hidelist, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None) ?? "";
					}
				}
			}

			return result;
		}
	}
}
