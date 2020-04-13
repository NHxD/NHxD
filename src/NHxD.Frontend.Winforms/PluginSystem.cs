using Ash.System.Diagnostics;
using Nhentai;
using NHxD.Plugin;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using NHxD.Plugin.MetadataProcessor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NHxD.Frontend.Winforms
{
	public class PluginSystem
	{
		public ILogger Logger { get; }
		public IPathFormatter PathFormatter { get; }
		public ICacheFileSystem CacheFileSystem { get; }
		public List<IArchiveWriter> ArchiveWriters { get; }
		public List<IMetadataConverter> MetadataConverters { get; }
		public List<IMetadataProcessor> MetadataProcessors { get; }

		public PluginSystem(ILogger logger
			, IPathFormatter pathFormatter
			, ICacheFileSystem cacheFileSystem
			, List<IArchiveWriter> archiveWriters
			, List<IMetadataConverter> metadataConverters
			, List<IMetadataProcessor> metadataProcessors)
		{
			Logger = logger;
			PathFormatter = pathFormatter;
			CacheFileSystem = cacheFileSystem;
			ArchiveWriters = archiveWriters;
			MetadataConverters = metadataConverters;
			MetadataProcessors = metadataProcessors;
		}

		public void LoadPlugins<TPlugin>(List<TPlugin> cachedPlugins, Dictionary<string, Dictionary<string, string>> configList)
			where TPlugin : IPlugin
		{
			cachedPlugins.Clear();

			foreach (KeyValuePair<string, Dictionary<string, string>> kvp in configList)
			{
				if (kvp.Value.ContainsKey("enabled"))
				{
					bool boolValue;

					if (bool.TryParse(kvp.Value["enabled"], out boolValue))
					{
						if (!boolValue)
						{
							continue;
						}
					}
				}

				string pluginFilePath = PathFormatter.GetPlugin(kvp.Key);
				Assembly assembly = null;

				try
				{
					assembly = Assembly.LoadFile(pluginFilePath);
				}
				catch (Exception ex)
				{
					Logger.WarnLineFormat("Couldn't load assembly from plugin {0} ({1})", kvp.Key, pluginFilePath);
					Logger.ErrorLineFormat(ex.ToString());
					continue;
				}

				Type pluginType = typeof(TPlugin);
				IEnumerable<Type> pluginImplementationTypes = assembly.ExportedTypes
					.Where(x => pluginType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

				foreach (Type type in pluginImplementationTypes)
				{
					TPlugin plugin = (TPlugin)Activator.CreateInstance(type);

					plugin.Initialize(kvp.Value);

					cachedPlugins.Add(plugin);

					try
					{
						Logger.LogLineFormat("Loaded plugin \"{0}\" version {1} from library \"{2}\"", plugin.Info.Name, plugin.Info.Version, Path.GetFileNameWithoutExtension(pluginFilePath));
					}
					catch { }
				}
			}
		}

		public void CreateArchive(IArchiveWriter archiveWriter, Metadata metadata)
		{
			string pagesPath;

			if (PathFormatter.IsEnabled)
			{
				pagesPath = PathFormatter.GetPages(metadata);
			}
			else
			{
				pagesPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/", PathFormatter.GetCacheDirectory(), metadata.Id);
			}

			CreateArchive(archiveWriter, metadata, pagesPath);
		}

		public void CreateArchive(Metadata metadata, bool deletePagesDirectoryIfNecessary)
		{
			string pagesPath;

			if (PathFormatter.IsEnabled)
			{
				pagesPath = PathFormatter.GetPages(metadata);
			}
			else
			{
				pagesPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/", PathFormatter.GetCacheDirectory(), metadata.Id);
			}

			foreach (IArchiveWriter archiveWriter in ArchiveWriters)
			{
				CreateArchive(archiveWriter, metadata, pagesPath);
			}

			if (deletePagesDirectoryIfNecessary)
			{
				int[] cachedPageIndices = CacheFileSystem.GetCachedPageIndices(metadata.Id);

				if (cachedPageIndices.Length == metadata.Images.Pages.Count)
				{
					Directory.Delete(pagesPath, true);
				}
			}
		}

		public void CreateArchive(IArchiveWriter archiveWriter, Metadata metadata, string pagesPath)
		{
			string archiveFilePath;

			if (PathFormatter.IsEnabled)
			{
				archiveFilePath = PathFormatter.GetArchive(metadata, archiveWriter);
			}
			else
			{
				archiveFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), metadata.Id, archiveWriter.FileExtension);
			}

			if (Directory.Exists(pagesPath))
			{
				archiveWriter.CreateFromDirectory(pagesPath, archiveFilePath);
			}
			else
			{
				archiveWriter.CreateEmpty(archiveFilePath);
			}
		}

		public void ConvertMetadata(Metadata metadata)
		{
			foreach (IMetadataConverter metadataConverter in MetadataConverters)
			{
				CreateMetadataEmbed(metadataConverter, metadata);
			}
		}

		public void CreateMetadataEmbed(IMetadataConverter metadataConverter, Metadata metadata)
		{
			string result;

			if (metadataConverter.Write(metadata, out result))
			{
				string filePath;
				if (PathFormatter.IsEnabled)
				{
					filePath = PathFormatter.GetConvertedMetadata(metadata, metadataConverter);
				}
				else
				{
					filePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/{2}", PathFormatter.GetCacheDirectory(), metadata.Id, metadataConverter.FileName);
				}

				try
				{
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
					File.WriteAllText(filePath, result);
				}
				catch (Exception ex)
				{
					Logger.ErrorLineFormat("Failed to write metadata embed: {0}", filePath);
					Logger.ErrorLineFormat(ex.ToString());
				}
			}
		}
	}
}
