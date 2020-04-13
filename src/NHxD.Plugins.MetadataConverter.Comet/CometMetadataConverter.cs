using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHxD.Plugin.MetadataConverter.Comet
{
	public sealed class PluginSettings
	{
		public bool Indent { get; set; } = true;
	}

	public sealed class CometMetadataConverter : IMetadataConverter
	{
		public IPluginInfo Info => new PluginInfo("Comet", "Convert metadata to CoMet XML.", "ash", "1.0");
		public string[] Options => new string[] { "indent:bool(true)" };
		public string FileName => "comet.xml";

		private readonly PluginSettings settings = new PluginSettings();

		private Dictionary<string, string> languageNameToIso;
		/*
		public void Initialize(object settingsObject)
		{
			settings = settingsObject as CometMetadataConverterSettings;
		}

		public void Initialize(string settingsJson)
		{
			settings = JsonConvert.DeserializeObject<CometMetadataConverterSettings>(settingsJson);
		}
		*/
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
			string languageIso = GetLanguageIso(metadata, languageNameToIso);
			ILookup<string, string> elements = GetElements(metadata, languageIso).ToLookup(x => x.Item1, x => x.Item2);

			XmlDocument document = new XmlDocument();

			XmlDeclaration declaration = document.CreateXmlDeclaration("1.1", null, null);

			XmlElement root = document.DocumentElement;
			document.InsertBefore(declaration, root);

			XmlNode comet = document.CreateElement("comet");

			XmlAttribute attribute = document.CreateAttribute("xmlns:comet");
			attribute.Value = "http://www.denvog.com/comet/";
			comet.Attributes.Append(attribute);

			attribute = document.CreateAttribute("xmlns:xsi");
			attribute.Value = "http://www.w3.org/2001/XMLSchema-instance";
			comet.Attributes.Append(attribute);

			attribute = document.CreateAttribute("xsi:schemaLocation");
			attribute.Value = "http://www.denvog.com http://www.denvog.com/comet/comet.xsd";
			comet.Attributes.Append(attribute);

			document.AppendChild(comet);

			foreach (IGrouping<string, string> groupings in elements)
			{
				foreach (string value in groupings)
				{
					if (string.IsNullOrEmpty(value))
					{
						continue;
					}

					XmlNode element = document.CreateElement(groupings.Key);
					element.InnerText = value;

					comet.AppendChild(element);
				}
			}

			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Indent = settings.Indent };
			StringBuilder sb = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb, xmlWriterSettings))
			{
				document.WriteContentTo(writer);
			}

			blob = sb.ToString();

			return true;
		}

		private static TupleList<string, string> GetElements(Metadata metadata, string languageIso)
		{
			string date = GetDate(metadata);
			string identifier = GetIdentifier(metadata);
			string parodyGenre = GetParodyGenre(metadata);
			string hentaiGenre = GetHentaiGenre(metadata);

			TupleList<string, string> elements = new TupleList<string, string>()
			{
				{ "title", metadata.Title.Pretty },
				{ "description", "" },
				{ "series", "" },
				{ "issue", "" },
				{ "volume", "" },
				{ "date", date },
				{ "genre", parodyGenre },
				{ "genre", hentaiGenre },
				{ "isVersionOf", "" },
				{ "price", "" },
				{ "format", "manga" },
				{ "language", languageIso },
				{ "rating", "adult" },
				{ "rights", "" },
				{ "identifier", identifier },
				{ "pages", metadata.NumPages.ToString(CultureInfo.InvariantCulture) },
				{ "writer", "" },
				{ "penciller", "" },
				{ "editor", "" },
				{ "coverDesigner", "" },
				{ "inker", "" },
				{ "letterer", "" },
				{ "colorist", "" },
				{ "coverImage", "" },
				{ "lastMark", "" },
				{ "readingDirection", "rtl" },
			};

			AddPublishers(metadata, elements);
			AddCreators(metadata, elements);
			AddCharacters(metadata, elements);

			return elements;
		}

		private static void AddCharacters(Metadata metadata, TupleList<string, string> elements)
		{
			foreach (Tag tag in metadata.Tags.Where(x => x.Type == TagType.Character))
			{
				elements.Add("character", tag.Name.ToTitleCase());
			}
		}

		private static void AddCreators(Metadata metadata, TupleList<string, string> elements)
		{
			foreach (Tag tag in metadata.Tags.Where(x => x.Type == TagType.Artist))
			{
				elements.Add("creator", tag.Name.ToTitleCase());
			}
		}

		private static void AddPublishers(Metadata metadata, TupleList<string, string> elements)
		{
			foreach (Tag tag in metadata.Tags.Where(x => x.Type == TagType.Group))
			{
				elements.Add("publisher", tag.Name.ToTitleCase());
			}
		}

		private static string GetHentaiGenre(Metadata metadata)
		{
			return !metadata.Tags.Any(x => x.Name.Equals("non h", StringComparison.OrdinalIgnoreCase)) ? "hentai" : "";
		}

		private static string GetParodyGenre(Metadata metadata)
		{
			return metadata.Tags.Any(x => x.Type == TagType.Parody) ? "parody" : "";
		}

		private static string GetIdentifier(Metadata metadata)
		{
			return "nhentai:" + metadata.Id.ToString(CultureInfo.InvariantCulture);
		}

		private static string GetDate(Metadata metadata)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(metadata.UploadDate).ToShortDateString();
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

	internal sealed class TupleList<T1, T2> : List<Tuple<T1, T2>>
	{
		internal void Add(T1 item, T2 item2)
		{
			Add(new Tuple<T1, T2>(item, item2));
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
