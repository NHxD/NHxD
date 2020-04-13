using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHxD.Plugin.MetadataConverter.ComicInfo
{
	public sealed class PluginSettings
	{
		public bool Indent { get; set; } = true;
	}

	public sealed class ComicInfoMetadataConverter : IMetadataConverter
	{
		public IPluginInfo Info => new PluginInfo("ComicInfo", "Convert metadata to ComicRack XML.", "ash", "1.0");
		public string[] Options => new string[] { "indent:bool(true)" };
		public string FileName => "ComicInfo.xml";

		private readonly PluginSettings settings = new PluginSettings();

		private Dictionary<string, string> languageNameToIso;

		public void Initialize(Dictionary<string, string> settingsDictionary)
		{
			string value;

			if (settingsDictionary.TryGetValue("indent", out value))
			{
				bool boolValue;

				if (bool.TryParse(value, out boolValue))
				{
					settings.Indent = boolValue;
				}
			}

			languageNameToIso = new Dictionary<string, string>()
			{
				{  "english", "en" },
				{  "japanese", "ja" },
				{  "korean", "ko" },
				{  "chinese", "zh" },
				//{  "cebuano", "ceb" },	// not supported by ISO 639-1.
			};
		}

		public void Destroy()
		{

		}

		public bool Write(Metadata metadata, out string blob)
		{
			Dictionary<string, string> elements = GetElements(metadata);

			XmlDocument document = new XmlDocument();

			XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", null, null);

			XmlElement root = document.DocumentElement;
			document.InsertBefore(declaration, root);

			XmlNode comicInfo = document.CreateElement("ComicInfo");

			XmlAttribute attribute = document.CreateAttribute("xmlns:xsd");
			attribute.Value = "http://www.w3.org/2001/XMLSchema";
			comicInfo.Attributes.Append(attribute);

			attribute = document.CreateAttribute("xmlns:xsi");
			attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
			comicInfo.Attributes.Append(attribute);

			document.AppendChild(comicInfo);

			foreach (KeyValuePair<string, string> kvp in elements)
			{
				if (string.IsNullOrEmpty(kvp.Value))
				{
					continue;
				}

				XmlNode element = document.CreateElement(kvp.Key);
				element.InnerText = kvp.Value;

				comicInfo.AppendChild(element);
			}
			/*
			XmlNode pages = document.CreateElement("Pages");

			for (int i = 0; i < metadata.Images.Pages.Count; ++i)
			{
				Image image = metadata.Images.Pages[i];

				XmlNode page = document.CreateElement("Page");

				attribute = document.CreateAttribute("Image");
				attribute.Value = i.ToString(CultureInfo.InvariantCulture);
				page.Attributes.Append(attribute);

				//attribute = document.CreateAttribute("ImageSize");
				//attribute.Value = ...;
				//page.Attributes.Append(attribute);

				attribute = document.CreateAttribute("ImageWidth");
				attribute.Value = metadata.Images.Pages[i].Width.ToString(CultureInfo.InvariantCulture);
				page.Attributes.Append(attribute);

				attribute = document.CreateAttribute("ImageHeight");
				attribute.Value = metadata.Images.Pages[i].Height.ToString(CultureInfo.InvariantCulture);
				page.Attributes.Append(attribute);

				//attribute = document.CreateAttribute("Type");
				//page.Attributes.Append(attribute);

				//attribute = document.CreateAttribute("Bookmark");
				//page.Attributes.Append(attribute);

				pages.AppendChild(page);
			}

			comicInfo.AppendChild(pages);
			*/

			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Indent = settings.Indent };
			StringBuilder sb = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
			{
				document.WriteContentTo(writer);
			}

			blob = sb.ToString();

			return true;
		}

		private Dictionary<string, string> GetElements(Metadata metadata)
		{
			string artists = GetArtists(metadata);
			DateTime uploadDate = GetUploadDate(metadata);
			string languageIso = GetLanguageIso(metadata, languageNameToIso);
			string publisher = GetPublisher(metadata);
			string genre = GetGenre(metadata);
			string websiteAddress = GetWebsiteAddress(metadata);
			string format = GetFormat(metadata);
			string ageRating = GetAgeRating(metadata);
			string blackAndWhite = GetBlackAndWhite(metadata);
			string characters = GetCharacters(metadata);
			string tags = GetTags(metadata);

			Dictionary<string, string> elements = new Dictionary<string, string>()
			{
				{  "Title", metadata.Title.Pretty },
				{  "Series", "" },
				{  "Count", "" },
				{  "Volume", "" },
				{  "AlternateSeries", "" },
				{  "AlternateNumber", "" },
				{  "StoryArc", "" },
				{  "SeriesGroup", "" },
				{  "AlternateCount", "" },
				{  "Summary", "" },
				{  "Notes", "" },
				{  "Year", uploadDate.Year.ToString(CultureInfo.InvariantCulture) },
				{  "Month", uploadDate.Month.ToString(CultureInfo.InvariantCulture) },
				{  "Day", uploadDate.Day.ToString(CultureInfo.InvariantCulture) },
				{  "Writer", artists },
				{  "Penciller", "" },
				{  "Inker", "" },
				{  "Colorist", "" },
				{  "Letterer", "" },
				{  "CoverArtist", "" },
				{  "Editor", "" },
				{  "Publisher", publisher },
				{  "Imprint", "" },
				{  "Genre", genre },
				{  "Web", websiteAddress },
				{  "PageCount", metadata.NumPages.ToString(CultureInfo.InvariantCulture) },
				{  "LanguageISO", languageIso },
				{  "Format", format },
				{  "AgeRating", ageRating },
				{  "BlackAndWhite", blackAndWhite },
				{  "Manga", "Yes" },
				{  "Characters", characters },
				{  "Teams", "" },
				{  "Locations", "" },
				{  "ScanInformation", metadata.Scanlator },
				{  "Tags", tags },
			};
			return elements;
		}

		private static DateTime GetUploadDate(Metadata metadata)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(metadata.UploadDate);
		}

		private static string GetArtists(Metadata metadata)
		{
			return string.Join(" & ", metadata.Tags.Where(x => x.Type == TagType.Artist).Select(x => x.Name.ToTitleCase()));
		}

		private static string GetTags(Metadata metadata)
		{
			return string.Join(", ", metadata.Tags.Where(x => x.Type == TagType.Tag).Select(x => x.Name));
		}

		private static string GetCharacters(Metadata metadata)
		{
			return string.Join(", ", metadata.Tags.Where(x => x.Type == TagType.Character).Select(x => x.Name.ToTitleCase()));
		}

		private static string GetBlackAndWhite(Metadata metadata)
		{
			return (metadata.Tags.Where(x => x.Name.EndsWith(" color", StringComparison.OrdinalIgnoreCase)).Count() > 0) ? "No" : "Yes";
		}

		private static string GetAgeRating(Metadata metadata)
		{
			return (metadata.Tags.Where(x => x.Name.Equals("non h", StringComparison.OrdinalIgnoreCase)).Count() > 0) ? "Everyone" : "Adults Only 18+";
		}

		private static string GetFormat(Metadata metadata)
		{
			return (metadata.Title.Pretty.IndexOf("Anthology", StringComparison.InvariantCultureIgnoreCase) != -1) ? "Anthology"
								: metadata.Title.Pretty.StartsWith("COMIC ", StringComparison.InvariantCultureIgnoreCase) ? "Magazine"
								: (metadata.Title.Pretty.IndexOf("Preview", StringComparison.InvariantCultureIgnoreCase) != -1) ? "Preview"
								: (metadata.Tags.Where(x => x.Name.Equals("webtoon", StringComparison.OrdinalIgnoreCase)).Count() > 0) ? "Web Comic"
								: "NSFW";
		}

		private static string GetWebsiteAddress(Metadata metadata)
		{
			return string.Format(CultureInfo.InvariantCulture, "https://nhentai.net/g/{0}/", metadata.Id);
		}

		private static string GetGenre(Metadata metadata)
		{
			List<string> genres = new List<string>();

			if (metadata.Tags.Where(x => x.Type == TagType.Parody).Count() > 0)
			{
				genres.Add("Parody");
			}

			if (metadata.Tags.Where(x => x.Name.Equals("non h", StringComparison.OrdinalIgnoreCase)).Count() == 0)
			{
				genres.Add("Hentai");
			}

			return string.Join(", ", genres);
		}

		private static string GetPublisher(Metadata metadata)
		{
			return string.Join(", ", metadata.Tags.Where(x => x.Type == TagType.Group).Select(x => x.Name.ToTitleCase()));
		}

		private static string GetLanguageIso(Metadata metadata, Dictionary<string, string> languageNameToIso)
		{
			foreach (Tag tag in metadata.Tags.Where(x => x.Type == TagType.Language))
			{
				string languageIso;

				if (languageNameToIso.TryGetValue(tag.Name, out languageIso))
				{
					return languageIso;
				}
			}

			return "";
		}
	}

	internal static class StringExtensionMethods
	{
		internal static string ToTitleCase(this string value)
		{
			string result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);

			if (!char.IsLower(value[0]))
			{
				result = Char.ToLower(result[0], CultureInfo.CurrentCulture) + result.Substring(1);
			}

			return result;
		}
	}
}
