using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHxD.Formatting.TokenReplacers
{
	public class MetadataTokenReplacer : ITokenReplacer
	{
		public const string Namespace = "Metadata";

		public Metadata Metadata { get; }
		public IPathFormatter PathFormatter { get; }
		public string[] LanguageNames { get; }

		public MetadataTokenReplacer(Metadata metadata, IPathFormatter pathFormatter, string[] languageNames)
		{
			Metadata = metadata;
			PathFormatter = pathFormatter;
			LanguageNames = languageNames;
		}

		public string Replace(string[] tokens, string[] namespaces)
		{
			string result = null;

			if (namespaces[0].Equals(Namespace, StringComparison.OrdinalIgnoreCase))
			{
				if (Metadata != null)
				{
					if (namespaces.Length == 1)
					{
						bool indented = tokens.Skip(1).Any(x => x.Equals("indented", StringComparison.OrdinalIgnoreCase));

						result = JsonConvert.SerializeObject(Metadata, indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None);
					}
					else if (namespaces.Length >= 2)
					{
						if (namespaces[1].Equals("Id", StringComparison.OrdinalIgnoreCase))
						{
							result = Metadata.Id.ToString(CultureInfo.InvariantCulture);
						}
						else if (namespaces[1].Equals("MediaId", StringComparison.OrdinalIgnoreCase))
						{
							result = Metadata.MediaId.ToString(CultureInfo.InvariantCulture);
						}
						else if (namespaces[1].Equals("UploadDate", StringComparison.OrdinalIgnoreCase))
						{
							result = GetUploadDate();
						}
						else if (namespaces[1].Equals("NumPages", StringComparison.OrdinalIgnoreCase))
						{
							result = Metadata.NumPages.ToString(CultureInfo.InvariantCulture);
						}
						else if (namespaces[1].Equals("NumFavorites", StringComparison.OrdinalIgnoreCase))
						{
							result = Metadata.NumFavorites.ToString(CultureInfo.InvariantCulture);
						}
						else if (namespaces[1].Equals("Scanlator", StringComparison.OrdinalIgnoreCase))
						{
							result = Metadata.Scanlator ?? "";
						}
						else if (namespaces[1].Equals("Titles", StringComparison.OrdinalIgnoreCase))
						{
							result = GetTitles();
						}
						else if (namespaces[1].Equals("Title", StringComparison.OrdinalIgnoreCase))
						{
							if (namespaces.Length >= 3)
							{
								if (namespaces[2].Equals("English", StringComparison.OrdinalIgnoreCase))
								{
									result = Metadata.Title.English ?? "";
								}
								else if (namespaces[2].Equals("Japanese", StringComparison.OrdinalIgnoreCase))
								{
									result = Metadata.Title.Japanese ?? "";
								}
								else if (namespaces[2].Equals("Pretty", StringComparison.OrdinalIgnoreCase))
								{
									result = Metadata.Title.Pretty ?? "";
								}
							}
						}
						else if (namespaces[1].Equals("Language", StringComparison.OrdinalIgnoreCase))
						{
							if (namespaces.Length >= 3)
							{
								if (namespaces[2].Equals("Primary", StringComparison.OrdinalIgnoreCase))
								{
									result = GetPrimaryLanguage(result);
								}
							}
						}
						else if (namespaces[1].Equals("Languages", StringComparison.OrdinalIgnoreCase))
						{
							result = GetLanguages(tokens);
						}
						else if (namespaces[1].Equals("Artists", StringComparison.OrdinalIgnoreCase))
						{
							result = GetArtist(tokens);
						}
						else if (namespaces[1].Equals("Groups", StringComparison.OrdinalIgnoreCase))
						{
							result = GetGroup(tokens);
						}
						else if (namespaces[1].Equals("Categories", StringComparison.OrdinalIgnoreCase))
						{
							result = GetCategory(tokens);
						}
						else if (namespaces[1].Equals("Parodies", StringComparison.OrdinalIgnoreCase))
						{
							result = GetParody(tokens);
						}
						else if (namespaces[1].Equals("Characters", StringComparison.OrdinalIgnoreCase))
						{
							result = GetCharacters(tokens);
						}
						else if (namespaces[1].Equals("Tags", StringComparison.OrdinalIgnoreCase))
						{
							result = GetTags(tokens);
						}
						else if (namespaces[1].Equals("Cover", StringComparison.OrdinalIgnoreCase))
						{
							if (namespaces.Length >= 3)
							{
								if (namespaces[2].Equals("CachedPath", StringComparison.OrdinalIgnoreCase))
								{
									result = CoverCoverCachedPath();
								}
								else if (namespaces[2].Equals("Width", StringComparison.OrdinalIgnoreCase))
								{
									result = GetCoverWidth();
								}
								else if (namespaces[2].Equals("Height", StringComparison.OrdinalIgnoreCase))
								{
									result = GetCoverHeight();
								}
							}
						}
					}
				}
			}

			return result;
		}

		private string GetUploadDate()
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Metadata.UploadDate).ToShortDateString();
		}

		private string GetTitles()
		{
			return string.Join(", ", new string[] { Metadata.Title.English ?? "", Metadata.Title.Japanese ?? "", Metadata.Title.Pretty ?? "" }.Distinct());
		}

		private string GetPrimaryLanguage(string result)
		{
			List<Tag> languages = Metadata.Tags.Where(x => x.Type == TagType.Language).ToList();
			Tag filteredLanguages = languages.FirstOrDefault(x => LanguageNames.Contains(x.Name));

			if (filteredLanguages != null)
			{
				result = filteredLanguages.Id.ToString(CultureInfo.InvariantCulture);
			}

			return result;
		}

		private string GetLanguages(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Language), ", ");
		}

		private string GetArtist(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Artist), " & ");
		}

		private string GetGroup(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Group), ", ");
		}

		private string GetCategory(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Category), ", ");
		}

		private string GetParody(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Parody), ", ");
		}

		private string GetCharacters(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Character), ", ");
		}

		private string GetTags(string[] tokens)
		{
			return TagFormattingUtility.GetTagsReplacement(tokens.Skip(1), Metadata.Tags.Where(x => x.Type == TagType.Tag), ", ");
		}

		private string CoverCoverCachedPath()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), Metadata.Id.ToString(CultureInfo.InvariantCulture), Metadata.Images.Cover.GetFileExtension());
		}

		private string GetCoverWidth()
		{
			return Metadata.Images.Cover.Width.ToString(CultureInfo.InvariantCulture);
		}

		private string GetCoverHeight()
		{
			return Metadata.Images.Cover.Height.ToString(CultureInfo.InvariantCulture);
		}
	}
}
