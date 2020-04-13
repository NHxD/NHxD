using Newtonsoft.Json;
using Nhentai;
using NHxD.Formatting;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace NHxD.Frontend.Winforms
{
	public class SearchResultTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "SearchResult";

		public ISearchProgressArg SearchProgressArg { get; }
		public string Target { get; }
		public MetadataTextFormatter MetadataTextFormatter { get; }
		public DocumentTemplate<Metadata> SearchCovergridItemDocumentTemplate { get; }
		public DocumentTemplate<Metadata> LibraryCovergridItemDocumentTemplate { get; }

		public SearchResultTokenReplacer(ISearchProgressArg searchProgressArg, string target, MetadataTextFormatter metadataTextFormatter, DocumentTemplate<Metadata> searchCovergridItemDocumentTemplate, DocumentTemplate<Metadata> libraryCovergridItemDocumentTemplate)
		{
			SearchProgressArg = searchProgressArg;
			Target = target;
			MetadataTextFormatter = metadataTextFormatter;
			SearchCovergridItemDocumentTemplate = searchCovergridItemDocumentTemplate;
			LibraryCovergridItemDocumentTemplate = libraryCovergridItemDocumentTemplate;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (namespaces.Length == 1)
				{
					bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

					result = JsonConvert.SerializeObject(SearchProgressArg.SearchResult, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
				}
				else if (namespaces.Length >= 2)
				{
					if (namespaces[1].Equals("PerPage", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchProgressArg.SearchResult.PerPage.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("NumPages", StringComparison.OrdinalIgnoreCase))
					{
						result = SearchProgressArg.SearchResult.NumPages.ToString(CultureInfo.InvariantCulture);
					}
					else if (namespaces[1].Equals("Items", StringComparison.OrdinalIgnoreCase))
					{
						string templateText = Target.Equals("library", StringComparison.OrdinalIgnoreCase)
							? LibraryCovergridItemDocumentTemplate.GetFormattedText()
							: SearchCovergridItemDocumentTemplate.GetFormattedText();
						StringBuilder sb = new StringBuilder();

						for (int i = 0, len = SearchProgressArg.SearchResult.Result.Count; i < len; ++i)
						{
							Metadata metadata = SearchProgressArg.SearchResult.Result[i];
							string formattedText = MetadataTextFormatter.Format(templateText, metadata);

							sb.AppendLine(formattedText);
						}

						result = sb.ToString();
					}
					else if (namespaces[1].Equals("Target", StringComparison.OrdinalIgnoreCase))
					{
						result = Target;
					}
				}
			}

			return result;
		}
	}
}
