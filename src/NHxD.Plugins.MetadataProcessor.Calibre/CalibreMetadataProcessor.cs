using Nhentai;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NHxD.Plugin.MetadataProcessor.Calibre
{
	public sealed class PluginSettings
	{
		public bool Verbose { get; set; } = true;
		public bool Quiet { get; set; } = false;
		public string CalibreDbPath { get; set; } = "calibredb.exe";
	}

	public sealed class CalibreMetadataProcessor : IMetadataProcessor
    {
		public IPluginInfo Info => new PluginInfo("Calibre", "Add or update a metadata to a Calibre database.", "ash", "1.0");
		public string[] Options => new string[] { "verbose:bool(true)", "quiet:bool(false)", "calibredb:string(\"calibredb.exe\")" };

		private readonly PluginSettings settings = new PluginSettings();

		private bool areCustomColumnsReady;

		public void Initialize(Dictionary<string, string> settingsDictionary)
		{
			string value;

			if (settingsDictionary.TryGetValue("verbose", out value))
			{
				bool boolValue;

				if (bool.TryParse(value, out boolValue))
				{
					settings.Verbose = boolValue;
				}
			}

			if (settingsDictionary.TryGetValue("quiet", out value))
			{
				bool boolValue;

				if (bool.TryParse(value, out boolValue))
				{
					settings.Quiet = boolValue;
				}
			}

			if (settingsDictionary.TryGetValue("calibredb", out value))
			{
				settings.CalibreDbPath = value;
			}
		}

		public void Destroy()
		{

		}

		public bool Run(Metadata metadata, GalleryResourcePaths paths)
		{
			foreach (string archivePath in paths.Archives)
			{
				if (!File.Exists(archivePath))
				{
					continue;
				}

				UpdateCalibreDb(metadata, paths, archivePath);
			}

			return true;
		}

		private void UpdateCalibreDb(Metadata metadata, GalleryResourcePaths paths, string archivePath)
		{
			using (Process calibredb = new Process())
			{
				calibredb.StartInfo = new ProcessStartInfo()
				{
					FileName = settings.CalibreDbPath,
					UseShellExecute = false,
					CreateNoWindow = false,//!Settings.Verbose || Settings.Quiet
					RedirectStandardOutput = true,
					RedirectStandardError = true
				};

				int bookId = ReadBookId(metadata, calibredb);

				if (bookId == -1)
				{
					AddBook(metadata, paths, calibredb, archivePath);
					bookId = ReadBookId(metadata, calibredb);

					if (bookId == -1)
					{
						WriteError("Failed to add book to database.");
						return;
					}
				}

				if (!areCustomColumnsReady)
				{
					CheckColumns(calibredb);
					areCustomColumnsReady = true;
				}

				FillMetadata(bookId, metadata, calibredb, paths);
				FillCustomMetadata(bookId, metadata, calibredb, paths);
			}
		}
		
		private int ReadBookId(Metadata metadata, Process calibredb)
		{
			string[] args = { "search", "identifier:nhentai:" + metadata.Id.ToString(CultureInfo.InvariantCulture) };
			string output, error;

			if (!Execute(calibredb, args, out output, out error))
			{
				return -1;
			}

			if (!string.IsNullOrEmpty(output))
			{
				// NOTE: ignore duplicates.
				output = output.Split(new char[] { ',' })[0];

				int bookId;
				if (int.TryParse(output, out bookId))
				{
					return bookId;
				}
			}

			return -1;
		}

		private void FillMetadata(int bookId, Metadata metadata, Process calibredb, GalleryResourcePaths paths)
		{
			SetMetadata(bookId, calibredb, "title", metadata.Title.Pretty);
			SetMetadata(bookId, calibredb, "title_sort", metadata.Title.Pretty);

			SetMetadataFromTags(bookId, metadata, calibredb, "authors", TagType.Artist, " & ");
			SetAuthorSort(bookId, metadata, calibredb, "author_sort", TagType.Artist, " & ");

			SetMetadataFromTags(bookId, metadata, calibredb, "publisher", TagType.Group, " & ");
			SetMetadataFromTags(bookId, metadata, calibredb, "languages", TagType.Language, ",");
			SetMetadataFromTags(bookId, metadata, calibredb, "tags", TagType.Tag, ",");

			int firstValidCoverIndex = paths.Covers.FindIndex(x => !string.IsNullOrEmpty(x) && File.Exists(x));
			if (firstValidCoverIndex != -1)
			{
				SetMetadata(bookId, calibredb, "cover", paths.Covers[firstValidCoverIndex]);
			}
		}

		private void FillCustomMetadata(int bookId, Metadata metadata, Process calibredb, GalleryResourcePaths paths)
		{
			SetCustomMetadataFromTags(bookId, metadata, calibredb, "categories", TagType.Category);
			SetCustomMetadataFromTags(bookId, metadata, calibredb, "characters", TagType.Character);
			SetCustomMetadataFromTags(bookId, metadata, calibredb, "groups", TagType.Group);
			SetCustomMetadataFromTags(bookId, metadata, calibredb, "parodies", TagType.Parody);
			SetCustomMetadata(bookId, calibredb, "num_pages", metadata.NumPages.ToString(CultureInfo.InvariantCulture));
			//SetCustomMetadata(bookId, calibredb, "title_japanese", metadata.Title.Japanese);
		}

		private void CheckColumns(Process calibredb)
		{
			string[] args = { "custom_columns" };
			string output, error;

			if (!Execute(calibredb, args, out output, out error))
			{
				return;
			}

			EnsureCustomColumnExists(calibredb, output, "categories", "Categories", "text");
			EnsureCustomColumnExists(calibredb, output, "characters", "Characters", "text");
			EnsureCustomColumnExists(calibredb, output, "groups", "Groups", "text");
			EnsureCustomColumnExists(calibredb, output, "parodies", "Parodies", "text");
			EnsureCustomColumnExists(calibredb, output, "num_pages", "Pages", "int", isMultiple: false);
			//EnsureCustomColumnExists(calibredb, output, "title_japanese", "Japanese Title", "text", isMultiple: false);
		}

		private void EnsureCustomColumnExists(Process calibredb, string output, string customColumnName, string customColumnDisplayName, string dataType, bool isMultiple = true)
		{
			string[] lines = output.Split(new char[] { '\n' });

			if (lines.Any(x => x.StartsWith(customColumnName, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}

			string[] args = isMultiple ?
				new string[] { "add_custom_column", "--is-multiple", customColumnName.ToLowerInvariant().Escape(), customColumnDisplayName.Escape(), dataType.Escape() }
				: new string[] { "add_custom_column", customColumnName.ToLowerInvariant().Escape(), customColumnDisplayName.Escape(), dataType.Escape() };
			string result, error;

			if (!Execute(calibredb, args, out result, out error))
			{
				return;
			}
		}

		private void SetAuthorSort(int bookId, Metadata metadata, Process calibredb, string field_name, TagType tagType, string separator)
		{
			IEnumerable<string> artists = metadata.Tags
				.Where(x => x.Type == tagType)
				.Select(x => FormatAuthorSort(x.Name));

			SetMetadata(bookId, calibredb, field_name, string.Join(separator, artists));
		}

		private static string FormatAuthorSort(string name)
		{
			string[] tokens = name.Split(new char[] { ' ' }).Reverse().ToArray();

			if (tokens.Length > 1)
			{
				tokens[0] += ',';
			}

			return string.Join(" ", tokens);
		}

		private void SetMetadataFromTags(int bookId, Metadata metadata, Process calibredb, string field_name, TagType tagType, string separator)
		{
			IEnumerable<string> filteredTags = metadata.Tags
				.Where(x => x.Type == tagType)
				.Select(x => x.Name);

			SetMetadata(bookId, calibredb, field_name, string.Join(separator, filteredTags));
		}

		private void SetMetadata(int bookId, Process calibredb, string fieldname, string value)
		{
			string[] args = { "set_metadata", "--field", (fieldname.ToLowerInvariant() + ":" + value).Escape(), bookId.ToString(CultureInfo.InvariantCulture) };

			Execute(calibredb, args);
		}

		private void SetCustomMetadataFromTags(int bookId, Metadata metadata, Process calibredb, string customColumnName, TagType tagType)
		{
			foreach (Tag tag in metadata.Tags.Where(x => x.Type == tagType))
			{
				SetCustomMetadata(bookId, calibredb, customColumnName, tag.Name);
			}
		}

		private void SetCustomMetadata(int bookId, Process calibredb, string customColumnName, string value)
		{
			string[] args = { "set_custom", "--append", customColumnName.ToLowerInvariant().Escape(), bookId.ToString(CultureInfo.InvariantCulture), value.Escape() };

			Execute(calibredb, args);
		}

		private void AddBook(Metadata metadata, GalleryResourcePaths paths, Process calibredb, string archivePath)
		{
			List<string> args = new List<string>();

			args.Add("add");

			args.Add("--identifier");
			args.Add(("nhentai:" + metadata.Id.ToString(CultureInfo.InvariantCulture)).Escape());

			string[] artists = metadata.Tags.Where(x => x.Type == TagType.Artist).Select(x => x.Name).ToArray();
			if (artists.Length > 0)
			{
				args.Add("--authors");
				args.Add(string.Join(" & ", artists).Escape());
			}

			if (!string.IsNullOrEmpty(metadata.Title.Pretty))
			{
				args.Add("--title");
				args.Add(metadata.Title.Pretty.Escape().Replace("\\", "\\\\"));
			}

			int firstValidCoverPathIndex = paths.Covers.FindIndex(x => !string.IsNullOrEmpty(x) && File.Exists(x));
			if (firstValidCoverPathIndex != -1)
			{
				string coverPath = paths.Covers[firstValidCoverPathIndex];

				args.Add("--cover");
				args.Add(coverPath.Escape());
			}

			/*
			string[] languages = metadata.Tags.Where(x => x.Type == TagType.Language).Select(x => x.Name).ToArray();
			if (languages.Length > 0)
			{
				args.Add("--languages");
				args.Add(string.Join(",", languages).Escape());
			}

			string[] tags = metadata.Tags.Where(x => x.Type == TagType.Artist).Select(x => x.Name).ToArray();
			if (tags.Length > 0)
			{
				args.Add("--tags");
				args.Add(string.Join(",", tags).Escape());
			}
			*/
			/*
			string comments = GetComments(metadata);

			args.Add("--comments");
			args.Add(comments.Escape());
			*/

			args.Add(archivePath.Escape());

			Execute(calibredb, args.ToArray());
		}

		private static string GetComments(Metadata metadata)
		{
			/*
			<div>
				<h4 style="font-size: medium">Title.Pretty</h4>
				<h4 style="font-size: medium">Title.English</h4>
				<h4 style="font-size: medium">Title.Japanese</h4>

				<h4 style="font-size: medium">Parodies</h4>
				<p>
					<a href="https://nhentai.net/parody/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Characters</h4>
				<p>
					<a href="https://nhentai.net/character/subject/">character</a></a>
				</p>

				<h4 style="font-size: medium">Tags</h4>
				<p>
					<a href="https://nhentai.net/tag/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Artists</h4>
				<p>
					<a href="https://nhentai.net/artist/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Groups</h4>
				<p>
					<a href="https://nhentai.net/group/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Parodies</h4>
				<p>
					<a href="https://nhentai.net/parody/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Languages</h4>
				<p>
					<a href="https://nhentai.net/language/subject/">subject</a></a>
				</p>

				<h4 style="font-size: medium">Categories</h4>
				<p>
					<a href="https://nhentai.net/category/subject/">subject</a></a>
				</p>
			</div>
			*/
			/*
			StringBuilder sb = new StringBuilder();

			sb.Append(metadata.Title.Pretty);

			if (!metadata.Title.English.Equals(metadata.Title.Pretty))
			{
				sb.Append(metadata.Title.Pretty);
			}
			*/
			/*
			XmlDocument document = new XmlDocument();

			XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", null, null);

			XmlElement root = document.DocumentElement;
			document.InsertBefore(declaration, root);

			XmlNode div = document.CreateElement("div");

			document.AppendChild(div);

			string[] titles = new string[]
			{
				metadata.Title.Pretty,
				metadata.Title.English,
				metadata.Title.Japanese
			};

			foreach (string title in titles)
			{
				if (string.IsNullOrEmpty(title))
				{
					continue;
				}

				XmlNode element = document.CreateElement("H4");
				element.InnerText = title;

				div.AppendChild(element);
			}

			List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>()
			{
				new KeyValuePair<string, string> {  "Parodies",
					metadata
					.Tags
					.Where(x => x.Type == TagType.Parody)
					.Select(x => new KeyValuePair<string, string>(x.Name, x.Url)) }
				//"Characters",
				//"Tags",
				//"Artists",
				//"Groups",
				//"Languages",
				//"Categories"
			};

			IEnumerable<TupleList<string, string>> parodies = metadata.Tags.Where(x => x.Type == TagType.Parody).Select(x => new TupleList(x.Name, x.Url));

			foreach (string title in titles)
			{
				if (string.IsNullOrEmpty(title))
				{
					continue;
				}

				XmlNode element = document.CreateElement("H4");
				element.InnerText = title;

				XmlAttribute attribute = document.CreateAttribute("Image");
				attribute.Value = i.ToString(CultureInfo.InvariantCulture);
				page.Attributes.Append(attribute);

				div.AppendChild(element);
			}

			XmlWriterSettings settings = new XmlWriterSettings() { Indent = false };
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				document.WriteContentTo(writer);
			}

			return sb.ToString();
			*/
			return "";
		}

		private bool Execute(Process calibredb, string[] args)
		{
			string output, error;

			return Execute(calibredb, args, out output, out error);
		}

		private bool Execute(Process calibredb, string[] args, out string output, out string error)
		{
			calibredb.StartInfo.Arguments = string.Join(" ", args);
			WriteArguments(calibredb.StartInfo.Arguments);

			calibredb.Start();

			output = calibredb.StandardOutput.ReadToEnd();
			WriteOutput(output);

			error = calibredb.StandardError.ReadToEnd();
			calibredb.WaitForExit();

			if (!string.IsNullOrEmpty(error))
			{
				WriteError(error);
				return false;
			}

			return true;
		}

		private void WriteArguments(string text)
		{
			if (!settings.Verbose || settings.Quiet)
			{
				return;
			}

			Console.ForegroundColor = ConsoleColor.White;
			Console.Error.WriteLine(text);
			Console.ResetColor();
		}
		
		private void WriteOutput(string text)
		{
			if (!settings.Verbose || settings.Quiet)
			{
				return;
			}

			Console.ForegroundColor = ConsoleColor.Cyan;

			if (string.IsNullOrEmpty(text))
			{
				Console.Error.WriteLine("(no response)");
			}
			else
			{
				Console.Error.WriteLine(text);
			}
			Console.ResetColor();
		}
		
		private void WriteError(string text)
		{
			if (settings.Quiet)
			{
				return;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(text);
			Console.ResetColor();
		}
	}

	internal static class StringExtensionMethods
	{
		internal static string Escape(this string value)
		{
			// TODO: convert " to \"?

			return string.Format(CultureInfo.InvariantCulture, "\"{0}\"", value);
		}
	}
}
