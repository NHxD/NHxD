using Nhentai;
using NHxD.Formatting.TokenReplacers;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace NHxD.Frontend.Winforms
{
	public class PathFormatter : IPathFormatter
	{
		private readonly Regex multiplePathDelimiters1Regex = new Regex(@"(\\+)", RegexOptions.Compiled);
		private readonly Regex multiplePathDelimiters2Regex = new Regex(@"(/+)", RegexOptions.Compiled);
		private readonly Regex pathDelimiterRegex = new Regex(@"(\\)", RegexOptions.Compiled);
		private readonly Regex pathDelimiterAltRegex = new Regex(@"(/)", RegexOptions.Compiled);
		private readonly Regex formatItemRegex = new Regex(@"({([\w.,]+)})", RegexOptions.Compiled);
		private readonly Regex formatCustomItemRegex = new Regex(@"({@([\w.,]+)})", RegexOptions.Compiled);

		public string ApplicationPath { get; }
		public string SourcePath { get; }
		public Dictionary<string, string> CustomPaths { get; }
		public Configuration.ConfigPathFormatter ConfigPaths { get; }
		public string[] LanguageNames { get; }
		public bool IsEnabled { get; }

		public PathFormatter(string applicationPath, string currentWorkingDirectory, Dictionary<string, string> customPaths, Configuration.ConfigPathFormatter configPaths, string[] languageNames, bool isEnabled)
		{
			ApplicationPath = pathDelimiterRegex.Replace(applicationPath, "/");
			SourcePath = pathDelimiterRegex.Replace(currentWorkingDirectory, "/");
			CustomPaths = customPaths;
			ConfigPaths = configPaths;
			LanguageNames = languageNames;
			IsEnabled = isEnabled;
		}

		public static int GetBaseCount(int count)
		{
			return (count < 10) ? 1
				: (count < 100) ? 2
				: (count < 1000) ? 3 : 4;
		}

		public string GetCacheDirectory()
		{
			return GetPath(ConfigPaths?.CachePath);
		}

		public string GetSessionDirectory()
		{
			return GetPath(ConfigPaths?.SessionPath);
		}

		public string GetSessionDirectory(string sessionType)
		{
			return GetPath(ConfigPaths?.SessionSubPath, new Dictionary<string, string>() {
					{ "SessionType", sessionType },
				});
		}

		public string GetPluginDirectory()
		{
			return GetPath(ConfigPaths?.PluginPath);
		}

		public string GetMetadataDirectory()
		{
			return GetPath(ConfigPaths?.MetadataPath);
		}

		public string GetConvertedMetadataDirectory()
		{
			return GetPath(ConfigPaths?.ConvertedMetadataPath);
		}

		public string GetCoverDirectory()
		{
			return GetPath(ConfigPaths?.CoverPath);
		}

		public string GetPagesDirectory()
		{
			return GetPath(ConfigPaths?.PagesPath);
		}

		public string GetArchiveDirectory()
		{
			return GetPath(ConfigPaths?.ArchivePath);
		}

		public string GetResourceDirectory()
		{
			return GetPath(ConfigPaths?.ResourcePath);
		}

		public string GetTemplateDirectory()
		{
			return GetPath(ConfigPaths?.TemplatePath);
		}

		public string GetPlugin(string pluginName)
		{
			return GetPath(ConfigPaths?.Plugin, new Dictionary<string, string>() {
					{ "PluginName", pluginName },
					{ "FileExt", ".dll" }
				});
		}

		public string GetMetadata(int galleryId)
		{
			return GetPath(ConfigPaths?.Metadata, new Dictionary<string, string>() {
					{ "Id", galleryId.ToString(CultureInfo.InvariantCulture) },
					{ "FileExt", ".json" }
				});
		}

		public string GetCover(Metadata metadata)
		{
			return GetPath(ConfigPaths?.Page, metadata, new Dictionary<string, string>() {
					{ "FileExt", metadata.Images.Cover.GetFileExtension() }
				});
		}

		public string GetPages(Metadata metadata)
		{
			return GetPath(ConfigPaths?.Pages, metadata);
		}

		public string GetPage(Metadata metadata, int pageIndex)
		{
			return GetPath(ConfigPaths?.Page, metadata, new Dictionary<string, string>() {
					{ "PageIndex", (pageIndex + 1).ToString(CultureInfo.InvariantCulture) },
					{ "PageIndex,0", (pageIndex + 1).ToString(CultureInfo.InvariantCulture).PadLeft(GetBaseCount(metadata.Images.Pages.Count), '0') },
					{ "FileExt", metadata.Images.Pages[pageIndex].GetFileExtension() }
				});
		}

		public string GetArchive(Metadata metadata, IArchiveWriter archiveWriter)
		{
			return GetPath(ConfigPaths?.Archive, metadata, new Dictionary<string, string>() {
					{ "FileExt", archiveWriter.FileExtension }
				});
		}

		public string GetConvertedMetadata(Metadata metadata, IMetadataConverter metadataConverter)
		{
			return GetPath(ConfigPaths?.ConvertedMetadata, metadata, new Dictionary<string, string>() {
					{ "FileName", metadataConverter.FileName },
					{ "FileTitle", Path.GetFileNameWithoutExtension(metadataConverter.FileName) },
					{ "FileExt", Path.GetExtension(metadataConverter.FileName) },
				});
		}

		public string GetSession(string sessionQuery)
		{
			return GetPath(ConfigPaths?.Session, new Dictionary<string, string>() {
					{ "SessionQuery", sessionQuery },
					{ "FileExt", ".json" }
				});
		}

		public string GetDefaultConfiguration(string name)
		{
			return GetPath(ConfigPaths?.DefaultConfiguration, new Dictionary<string, string>() {
					{ "FileTitle", name },
					{ "FileExt", ".json" }
				});
		}

		public string GetConfiguration(string name)
		{
			return GetPath(ConfigPaths?.Configuration, new Dictionary<string, string>() {
					{ "FileTitle", name },
					{ "FileExt", ".json" }
				});
		}

		public string GetLog()
		{
			return GetPath(ConfigPaths?.Log, new Dictionary<string, string>() {
					{ "FileTitle", "debug" },
					{ "FileExt", ".log" }
				});
		}

		public string GetLog(DateTime dateTime)
		{
			return GetPath(ConfigPaths?.Log, new Dictionary<string, string>() {
					{ "FileTitle", dateTime.ToString("s").Replace('/', '-').Replace(':', '.') },
					{ "FileExt", ".log" }
				});
		}

		public string GetResource(string name, string fileExt)
		{
			return GetPath(ConfigPaths?.Resource, new Dictionary<string, string>() {
					{ "FileTitle", name },
					{ "FileExt", fileExt }
				});
		}

		public string GetTemplate(string name)
		{
			return GetPath(ConfigPaths?.Template, new Dictionary<string, string>() {
					{ "FileTitle", name },
					{ "FileExt", ".html" }
				});
		}


		public string GetPath(string format, Metadata metadata)
		{
			return GetPath(format, metadata, null, CustomPaths);
		}

		public string GetPath(string format)
		{
			return GetPath(format, null, null, CustomPaths);
		}

		public string GetPath(string format, Dictionary<string, string> contextMapping)
		{
			return GetPath(format, null, contextMapping, CustomPaths);
		}

		public string GetPath(string format, Metadata metadata, Dictionary<string, string> contextMapping)
		{
			return GetPath(format, metadata, contextMapping, CustomPaths);
		}

		public string GetPath(string format, Dictionary<string, string> contextMapping, Dictionary<string, string> customMapping)
		{
			return GetPath(format, null, contextMapping, customMapping);
		}

		public string GetPath(string format, Metadata metadata, Dictionary<string, string> contextMapping, Dictionary<string, string> customMapping)
		{
			if (string.IsNullOrEmpty(format))
			{
				return "";
			}

			if (customMapping != null
				&& customMapping.Count > 0)
			{
				format = formatCustomItemRegex.Replace(format,
					new MatchEvaluator(
						(Match match) =>
						{
							string symbol = match.Groups[2].Value;
							string value;

							if (customMapping.TryGetValue(symbol, out value))
							{
								return value;
							}

							return match.Value;
						}
					)
				);
			}

			format = formatItemRegex.Replace(format,
				new MatchEvaluator(
					(Match match) =>
					{
						string[] tokens = match.Groups[2].Value.Split(new char[] { ':' });
						string[] namespaces = tokens[0].Split(new char[] { '.' });
						string result = null;

						if (namespaces[0].Equals(PathTokenReplacer.Namespace, StringComparison.OrdinalIgnoreCase))
						{
							PathTokenReplacer replacer = new PathTokenReplacer(this);

							result = replacer.Replace(tokens, namespaces);
						}
						else if (namespaces[0].Equals(SpecialFolderTokenReplacer.Namespace, StringComparison.OrdinalIgnoreCase))
						{
							SpecialFolderTokenReplacer replacer = new SpecialFolderTokenReplacer();

							result = replacer.Replace(tokens, namespaces);
						}
						else if (namespaces[0].Equals(MetadataTokenReplacer.Namespace, StringComparison.OrdinalIgnoreCase))
						{
							MetadataTokenReplacer replacer = new MetadataTokenReplacer(metadata, this, LanguageNames);

							result = replacer.Replace(tokens, namespaces);
						}
						else if (contextMapping != null)
						{
							string value;

							if (contextMapping.TryGetValue(tokens[0], out value))
							{
								result = value;
							}
						}

						if (result == null)
						{
							result = match.Value;
						}

						return result;
					}
				)
			);

			if (Path.DirectorySeparatorChar == '\\'
				&& Path.AltDirectorySeparatorChar == '/')
			{
				format = pathDelimiterAltRegex.Replace(format, "\\");
			}
			else if (Path.DirectorySeparatorChar == '/'
				&& Path.AltDirectorySeparatorChar == '\\')
			{
				format = pathDelimiterRegex.Replace(format, "/");
			}

			// NOTE: this prevent support for UNC paths.
			format = multiplePathDelimiters1Regex.Replace(format, "\\");
			format = multiplePathDelimiters2Regex.Replace(format, "/");

			// HACK: force alt delimiter.
			format = pathDelimiterRegex.Replace(format, "/");

			return format;
		}
	}
}
