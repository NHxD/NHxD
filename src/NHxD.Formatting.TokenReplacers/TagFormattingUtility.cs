using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHxD.Formatting.TokenReplacers
{
	internal static class TagFormattingUtility
	{
		public static string GetTagsReplacement(IEnumerable<string> symbols, IEnumerable<Tag> tags, string separator)
		{
			return string.Join(separator, tags.Select(x => GetTagReplacement(symbols, x)));
		}

		public static string GetTagReplacement(IEnumerable<string> symbols, Tag tag)
		{
			if (symbols.Any(x => x.Equals("Id", StringComparison.OrdinalIgnoreCase)))
			{
				return tag.Id.ToString(CultureInfo.InvariantCulture);
			}
			else if (symbols.Any(x => x.Equals("Count", StringComparison.OrdinalIgnoreCase)))
			{
				return tag.Count.ToString(CultureInfo.InvariantCulture);
			}
			else if (symbols.Any(x => x.Equals("Type", StringComparison.OrdinalIgnoreCase)))
			{
				return tag.Type.ToString();
			}
			else if (symbols.Any(x => x.Equals("Url", StringComparison.OrdinalIgnoreCase)))
			{
				return tag.Url;
			}
			// default to using the tag name
			else
			{
				return tag.Name;
			}
		}
	}
}
