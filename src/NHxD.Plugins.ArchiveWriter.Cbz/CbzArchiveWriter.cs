using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace NHxD.Plugin.ArchiveWriter.Cbz
{
	public sealed class PluginSettings
	{
		public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Fastest;
	}

	public sealed class CbzArchiveWriter : IArchiveWriter
	{
		public IPluginInfo Info => new PluginInfo("Cbz", "Write CBZ archive.", "ash", "1.0");
		public string[] Options => new string[] { "compressionLevel:[NoCompression|Fastest|Optimal](Fastest)" };
		public string FileExtension => ".cbz";
		public bool SupportsCreateEmpty => true;
		public bool SupportsCreateFromDirectory => true;
		public bool SupportsCreateEntryFromFile => true;

		private readonly PluginSettings settings = new PluginSettings();

		public void Initialize(Dictionary<string, string> settingsDictionary)
		{
			string value;

			if (settingsDictionary.TryGetValue("compressionLevel", out value))
			{
				CompressionLevel enumValue;

				if (Enum.TryParse(value, true, out enumValue))
				{
					settings.CompressionLevel = enumValue;
				}
			}
		}

		public void Destroy()
		{

		}

		public bool CreateEmpty(string archiveFilePath)
		{
			File.Delete(archiveFilePath);
			Directory.CreateDirectory(Path.GetDirectoryName(archiveFilePath));

			using (ZipArchive archive = ZipFile.Open(archiveFilePath, ZipArchiveMode.Create))
			{
			}

			return true;
		}

		public bool CreateFromDirectory(string directoryPath, string archiveFilePath)
		{
			File.Delete(archiveFilePath);
			Directory.CreateDirectory(Path.GetDirectoryName(archiveFilePath));
			ZipFile.CreateFromDirectory(directoryPath, archiveFilePath, settings.CompressionLevel, includeBaseDirectory: false);

			return true;
		}

		public bool CreateEntryFromFile(string entryName, string entryFilePath, string archiveFilePath)
		{
			using (ZipArchive archive = ZipFile.Open(archiveFilePath, ZipArchiveMode.Update))
			{
				archive.CreateEntryFromFile(entryFilePath, entryName);
			}

			return true;
		}
	}
}
