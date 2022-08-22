using Nhentai;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHxD.Frontend.Winforms
{
	public class QueryParser : IQueryParser
	{
		private static readonly Regex NonWordCharRegex = new Regex(@"\W+", RegexOptions.Compiled);
		//private static Regex TagInvalidCharRegex { get; } = new Regex(@"[|&:]", RegexOptions.Compiled);
		//private static Regex WhitespaceRegex { get; } = new Regex(@"\s+", RegexOptions.Compiled);

		public TagsModel TagsModel { get; }

		public QueryParser(TagsModel tagsModel)
		{
			TagsModel = tagsModel;
		}

		// search:<query>[:page]
		public bool ParseQuerySearch(string[] tokens, out string query, out int pageIndex)
		{
			query = "";
			pageIndex = -1;

			if (!tokens[0].Equals("search", StringComparison.OrdinalIgnoreCase)
				&& !tokens[0].Equals("query", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length < 2)
			{
				throw NewArgumentCountException(tokens.Length, 2);
			}

			if (string.IsNullOrEmpty(tokens[1]))
			{
				throw NewEmptyQueryException();
			}

			query = tokens[1];
			pageIndex = 1;

			if (tokens.Length >= 3)
			{
				int num;

				if (!int.TryParse(tokens[2], out num))
				{
					throw NewPageIndexException(tokens[2]);
				}

				pageIndex = num;
			}

			return true;
		}

		// tag[:type]:<id|name>[:page]
		public bool ParseTaggedSearch(string[] tokens, out int tagId, out string tagType, out string tagName, out int pageIndex)
		{
			tagId = -1;
			tagName = "";
			tagType = "";
			pageIndex = -1;

			if (!tokens[0].Equals("tagged", StringComparison.OrdinalIgnoreCase)
				&& !tokens[0].Equals("tag", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length < 2)
			{
				throw NewArgumentCountException(tokens.Length, 2);
			}

			tagId = -1;
			tagName = "";
			pageIndex = 1;

			if (TagsModel == null)
			{
				throw new InvalidOperationException("Tags are unset.");
			}

			if (TagsModel.AllTags.Any(x => SanitizeTagName(x.Name).Equals(tokens[1], StringComparison.OrdinalIgnoreCase)))
			{
				tagName = tokens[1];
				tagId = TagsModel.AllTags.First(x => SanitizeTagName(x.Name).Equals(tokens[1], StringComparison.OrdinalIgnoreCase)).Id;
			}

			int typePartIndex = -1;
			int namePartIndex = -1;
			int pagePartIndex = -1;

			// tagged:name
			if (tokens.Length == 2)
			{
				namePartIndex = 1;
			}
			// tagged:name:page | tagged:type:name
			else if (tokens.Length == 3)
			{
				// tagged:name:page
				int num;
				if (int.TryParse(tokens[2], out num))
				{
					namePartIndex = 1;
					pagePartIndex = 2;
				}
				// tagged:type:name
				else
				{
					typePartIndex = 1;
					namePartIndex = 2;
				}
			}
			// tagged:type:name:page
			else if (tokens.Length >= 4)
			{
				typePartIndex = 1;
				namePartIndex = 2;
				pagePartIndex = 3;
			}

			if (typePartIndex != -1)
			{
				TagType tagTypeEnum;

				if (Enum.TryParse(tokens[typePartIndex], true, out tagTypeEnum))
				{
					tagType = tokens[typePartIndex];

					if (TagsModel.AllTags
						.Where(x => x.Type == tagTypeEnum)
						.Any(x => SanitizeTagName(x.Name).Equals(tokens[namePartIndex], StringComparison.OrdinalIgnoreCase)))
					{
						tagName = tokens[namePartIndex];
						tagId = TagsModel.AllTags
							.Where(x => x.Type == tagTypeEnum)
							.First(x => SanitizeTagName(x.Name).Equals(tokens[namePartIndex], StringComparison.OrdinalIgnoreCase)).Id;
					}
				}
				else
				{
					throw NewInvalidTagTypeException(tokens[typePartIndex]);
				}
			}
			else
			{
				if (TagsModel.AllTags.Any(x => SanitizeTagName(x.Name).Equals(tokens[namePartIndex], StringComparison.OrdinalIgnoreCase)))
				{
					tagName = tokens[namePartIndex];
					tagId = TagsModel.AllTags.First(x => SanitizeTagName(x.Name).Equals(tokens[namePartIndex], StringComparison.OrdinalIgnoreCase)).Id;
				}
			}

			// a tag id was specified instead of a name.
			if (tagId == -1)
			{
				int num = 0;

				if (!int.TryParse(tokens[1], out num))
				{
					throw NewInvalidTagException(tokens[1]);
				}

				tagId = num;
			}

			if (tagId < 0)
			{
				throw NewInvalidTagException(tokens[1]);
			}

			if (pagePartIndex != -1)
			{
				int num = 0;

				if (!int.TryParse(tokens[pagePartIndex], out num))
				{
					throw NewPageIndexException(tokens[pagePartIndex]);
				}

				pageIndex = num;
			}

			return true;
		}

		// recent[:page]
		public bool ParseRecentSearch(string[] tokens, out int pageIndex)
		{
			pageIndex = 1;

			if (!tokens[0].Equals("all", StringComparison.OrdinalIgnoreCase)
				&& !tokens[0].Equals("recent", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length >= 2)
			{
				int num;

				if (!int.TryParse(tokens[1], out num))
				{
					throw NewPageIndexException(tokens[1]);
				}

				pageIndex = num;
			}

			return true;
		}

		// library[:page]
		public bool ParseLibrarySearch(string[] tokens, out int pageIndex)
		{
			pageIndex = 1;

			if (!tokens[0].Equals("library", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length >= 2)
			{
				int num;

				if (!int.TryParse(tokens[1], out num))
				{
					throw NewPageIndexException(tokens[1]);
				}

				pageIndex = num;
			}

			return true;
		}

		// details[:id]
		public bool ParseDetailsSearch(string[] tokens, out int galleryId)
		{
			galleryId = 1;

			if (!tokens[0].Equals("details", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length >= 2)
			{
				int num;

				if (!int.TryParse(tokens[1], out num))
				{
					throw NewGalleryException(tokens[1]);
				}

				galleryId = num;
			}

			return true;
		}

		// download[:id]
		public bool ParseDownloadSearch(string[] tokens, out int galleryId)
		{
			galleryId = 1;

			if (!tokens[0].Equals("download", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (tokens.Length >= 2)
			{
				int num;

				if (!int.TryParse(tokens[1], out num))
				{
					throw NewGalleryException(tokens[1]);
				}

				galleryId = num;
			}

			return true;
		}

		public static string SanitizeTagName(string tagName)
		{
			return NonWordCharRegex.Replace(tagName, " ");
		}

		private static Exception NewArgumentCountException(int gotCount, int expectedCount)
		{
			return new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Wrong number of arguments. Got {0}, expected {1}.", gotCount, expectedCount));
		}

		private static Exception NewPageIndexException(string pageIndex)
		{
			return new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Page index must be a valid number. Got {0}.", pageIndex));
		}

		private static Exception NewInvalidTagException(string tagId)
		{
			return new ArgumentOutOfRangeException("tagId", tagId, "Tag ID must be a positive integer value.");
		}

		private static Exception NewGalleryException(string galleryId)
		{
			return new ArgumentOutOfRangeException("galleryId", galleryId, "Gallery ID must be a positive integer value.");
		}

		private static Exception NewInvalidTagTypeException(string tagType)
		{
			return new ArgumentOutOfRangeException("tagType", tagType, "Tag Type must be one of: " + string.Join(", ", Enum.GetNames(typeof(TagType))));
		}

		private static Exception NewEmptyQueryException()
		{
			return new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Query must contain one or more characters."));
		}
	}
}
