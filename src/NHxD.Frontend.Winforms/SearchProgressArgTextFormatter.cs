using Nhentai;
using NHxD.Formatting;
using NHxD.Formatting.TokenReplacers;

namespace NHxD.Frontend.Winforms
{
	public class SearchProgressArgTextFormatter
	{
		public IPathFormatter PathFormatter { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public TagsModel TagsModel { get; }
		public MetadataTextFormatter MetadataTextFormatter { get; }
		public ITokenModifier[] SearchTokenModifiers { get; }
		public DocumentTemplate<Metadata> SearchCoverGridItemDocumentTemplate { get; }
		public DocumentTemplate<Metadata> LibraryCoverGridItemDocumentTemplate { get; }

		public SearchProgressArgTextFormatter(IPathFormatter pathFormatter, MetadataKeywordLists metadataKeywordLists, TagsModel tagsModel, MetadataTextFormatter metadataTextFormatter, ITokenModifier[] searchTokenModifiers, DocumentTemplate<Metadata> searchCoverGridItemDocumentTemplate, DocumentTemplate<Metadata> libraryCoverGridItemDocumentTemplate)
		{
			PathFormatter = pathFormatter;
			MetadataKeywordLists = metadataKeywordLists;
			TagsModel = tagsModel;
			MetadataTextFormatter = metadataTextFormatter;
			SearchTokenModifiers = searchTokenModifiers;
			SearchCoverGridItemDocumentTemplate = searchCoverGridItemDocumentTemplate;
			LibraryCoverGridItemDocumentTemplate = libraryCoverGridItemDocumentTemplate;
		}

		public string Format(string text, ISearchProgressArg searchProgressArg, string target)
		{
			return new Formatter(new ITokenReplacer[]
			{
				new PathTokenReplacer(PathFormatter),
				new MetadataKeywordListTokenReplacer(MetadataKeywordLists.Whitelist, MetadataKeywordLists.Blacklist, MetadataKeywordLists.Ignorelist, MetadataKeywordLists.Hidelist),
				new SearchResultTokenReplacer(searchProgressArg, target, MetadataTextFormatter, SearchCoverGridItemDocumentTemplate, LibraryCoverGridItemDocumentTemplate),
				new SearchTokenReplacer(searchProgressArg.SearchArg, TagsModel)
			}, SearchTokenModifiers).Format(text);
		}
	}
}
