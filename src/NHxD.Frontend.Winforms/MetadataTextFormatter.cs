using Nhentai;
using NHxD.Formatting;
using NHxD.Formatting.TokenModifiers;
using NHxD.Formatting.TokenReplacers;

namespace NHxD.Frontend.Winforms
{
	public class MetadataTextFormatter
	{
		public IPathFormatter PathFormatter { get; }
		public string[] LanguageNames { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public ITokenModifier[] TokenModifiers { get; }

		public MetadataTextFormatter(IPathFormatter pathFormatter, string[] languageNames, MetadataKeywordLists metadataKeywordLists, ITokenModifier[] tokenModifiers)
		{
			PathFormatter = pathFormatter;
			LanguageNames = languageNames;
			MetadataKeywordLists = metadataKeywordLists;
			TokenModifiers = tokenModifiers;
		}

		public string Format(string text, Metadata metadata)
		{
			return new Formatter(new ITokenReplacer[]
			{
				new PathTokenReplacer(PathFormatter),
				new MetadataKeywordListTokenReplacer(MetadataKeywordLists.Whitelist, MetadataKeywordLists.Blacklist, MetadataKeywordLists.Ignorelist, MetadataKeywordLists.Hidelist),
				new MetadataTokenReplacer(metadata, PathFormatter, LanguageNames)
			}, TokenModifiers).Format(text);
		}
	}
}
