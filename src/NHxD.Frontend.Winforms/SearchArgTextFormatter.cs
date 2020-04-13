using NHxD.Formatting;
using NHxD.Formatting.TokenModifiers;
using NHxD.Formatting.TokenReplacers;

namespace NHxD.Frontend.Winforms
{
	public class SearchArgTextFormatter
	{
		public IPathFormatter PathFormatter { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public TagsModel TagsModel { get; }
		public ITokenModifier[] TokenModifiers { get; }

		public SearchArgTextFormatter(IPathFormatter pathFormatter, MetadataKeywordLists metadataKeywordLists, TagsModel tagsModel, ITokenModifier[] tokenModifiers)
		{
			PathFormatter = pathFormatter;
			MetadataKeywordLists = metadataKeywordLists;
			TagsModel = tagsModel;
			TokenModifiers = tokenModifiers;
		}

		public string Format(string text, ISearchArg searchArg)
		{
			return new Formatter(new ITokenReplacer[]
				{
					new PathTokenReplacer(PathFormatter),
					new MetadataKeywordListTokenReplacer(MetadataKeywordLists.Whitelist, MetadataKeywordLists.Blacklist, MetadataKeywordLists.Ignorelist, MetadataKeywordLists.Hidelist),
					new SearchTokenReplacer(searchArg, TagsModel)
				},
				TokenModifiers).Format(text);
		}
	}
}
