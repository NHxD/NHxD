using Nhentai;
using NHxD.Formatting;
using NHxD.Formatting.TokenReplacers;
using System;

namespace NHxD.Frontend.Winforms
{
	public class BookmarkFormatter : IBookmarkFormatter
	{
		public Configuration.ConfigBookmarksList BookmarkListSettings { get; }
		public Configuration.ConfigTagsList TagsListSettings { get; }
		public TagsModel TagsModel { get; }
		public IPathFormatter PathFormatter { get; }
		public ITokenModifier[] TokenModifiers { get; }

		public BookmarkFormatter(Configuration.ConfigBookmarksList bookmarkListSettings, Configuration.ConfigTagsList tagsListSettings
			, TagsModel tagsModel
			, IPathFormatter pathFormatter
			, ITokenModifier[] tokenModifiers
			)
		{
			BookmarkListSettings = bookmarkListSettings;
			TagsListSettings = tagsListSettings;
			TagsModel = tagsModel;
			PathFormatter = pathFormatter;
			TokenModifiers = tokenModifiers;
		}

		public string GetRecentSearchText(int pageIndex)
		{
			return GetBookmarkFormatter(new SearchArg(pageIndex)).Format(BookmarkListSettings.LabelFormats.RecentSearch);
		}

		public string GetQuerySearchText(string query, int pageIndex)
		{
			return GetBookmarkFormatter(new SearchArg(query, pageIndex)).Format(BookmarkListSettings.LabelFormats.QuerySearch);
		}

		public string GetTaggedSearchText(int tagId, int pageIndex)
		{
			return GetBookmarkFormatter(new SearchArg(tagId, pageIndex)).Format(BookmarkListSettings.LabelFormats.Tagged);
		}

		public string GetLibraryText(int pageIndex)
		{
			return GetBookmarkFormatter(new SearchArg(pageIndex, true)).Format(BookmarkListSettings.LabelFormats.Library);
		}

		public string GetDetailsText(Metadata metadata)
		{
			return FixDetailsBookmarkText(GetBookmarkFormatter(metadata).Format(BookmarkListSettings.LabelFormats.Details));
		}

		public string GetDownloadText(Metadata metadata)
		{
			return FixDetailsBookmarkText(GetBookmarkFormatter(metadata).Format(BookmarkListSettings.LabelFormats.Download));
		}


		private Formatter GetBookmarkFormatter(SearchArg searchArg)
		{
			return new Formatter(new ITokenReplacer[]
				{
					new SearchTokenReplacer(searchArg, TagsModel)
				}, TokenModifiers);
		}

		private Formatter GetBookmarkFormatter(Metadata metadata)
		{
			return new Formatter(new ITokenReplacer[]
				{
					new MetadataTokenReplacer(metadata, PathFormatter, TagsListSettings.LanguageNames)
				}, TokenModifiers);
		}

		private static string FixDetailsBookmarkText(string result)
		{
			if (result.EndsWith(" by ", StringComparison.InvariantCultureIgnoreCase))
			{
				result = result.Substring(0, result.Length - " by ".Length);
			}

			return result;
		}
	}
}
