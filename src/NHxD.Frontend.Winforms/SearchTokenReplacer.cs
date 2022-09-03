using NHxD.Formatting;
using System;
using System.Globalization;

namespace NHxD.Frontend.Winforms
{
	public class SearchTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Search";

		public ISearchArg SearchArg { get; }
		public TagsModel TagsModel { get; }

		public SearchTokenReplacer(ISearchArg searchArg, TagsModel tagsModel)
		{
			SearchArg = searchArg;
			TagsModel = tagsModel;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("PageIndex", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchArg.PageIndex.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("ValidTagIdOrNothing", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchArg.TagId != -1 ? SearchArg.TagId.ToString(CultureInfo.InvariantCulture) : "";
					}
					else if (namespaces[1].Equals("TagId", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchArg.TagId.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Query", StringComparison.OrdinalIgnoreCase))
					{
						if (SearchArg.Target == SearchTarget.Query)
						{
							result = SearchArg.Query ?? "";
						}
						else if (SearchArg.Target == SearchTarget.Tagged)
						{
							result = SearchArg.TagId.ToString(CultureInfo.InvariantCulture);
						}
						else
						{
							result = "";
						}
					}
					else if (namespaces[1].Equals("QueryOrTagId", StringComparison.OrdinalIgnoreCase))
					{
						if (!string.IsNullOrEmpty(SearchArg.Query))
						{
							result = SearchArg.Query;
						}
						else if (SearchArg.TagId != -1)
						{
							result = SearchArg.TagId.ToString(CultureInfo.InvariantCulture);
						}
						else
						{
							result = "";
						}
					}
					else if (namespaces[1].Equals("Target", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchArg.Target.ToString().ToLowerInvariant();
					}
				}
			}

			return result;
		}
	}
}
