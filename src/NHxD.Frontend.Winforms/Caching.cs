using Nhentai;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NHxD.Frontend.Winforms
{
	public class MetadataCache : IMetadataCache
	{
		private readonly Dictionary<int, Metadata> items;

		public Dictionary<int, Metadata> Items => items;

		public MetadataCache()
		{
			items = new Dictionary<int, Metadata>();
		}

		public bool Contains(int galleryId)
		{
			return items.ContainsKey(galleryId);
		}

		public bool TryGet(int galleryId, out Metadata metadata)
		{
			return items.TryGetValue(galleryId, out metadata);
		}
	}

	public class SearchResultCache : ISearchResultCache
	{
		private readonly Dictionary<string, SearchResult> items;

		public Dictionary<string, SearchResult> Items => items;

		public IMetadataCache MetadataCache { get; }

		public SearchResultCache(IMetadataCache metadataCache)
		{
			MetadataCache = metadataCache;

			items = new Dictionary<string, SearchResult>();
		}

		public bool Contains(string uri)
		{
			return items.ContainsKey(uri);
		}

		public void Add(string uri, SearchResult cachedSearchResult)
		{
			items.Add(uri, cachedSearchResult);
		}

		public bool TryGet(string uri, out SearchResult searchResult)
		{
			return items.TryGetValue(uri, out searchResult);
		}

		public Metadata Find(int galleryId)
		{
			foreach (KeyValuePair<string, SearchResult> kvp in items)
			{
				SearchResult searchResult = kvp.Value;

				foreach (Metadata metadata in searchResult.Result)
				{
					if (metadata.Id == galleryId)
					{
						return metadata;
					}
				}
			}

			return null;
		}

		// HACK: temporary quick solution - will probably be removed later.
		public void CacheRuntimeMetadata(Metadata metadata)
		{
			SearchResult searchResult;

			if (items.TryGetValue("__rt", out searchResult))
			{
				if (!searchResult.Result.Any(x => x.Id == metadata.Id))
				{
					searchResult.Result.Add(metadata);

					++searchResult.PerPage;
				}
			}
			else
			{
				searchResult = new SearchResult();

				searchResult.Result = new List<Metadata>();
				searchResult.Result.Add(metadata);
				searchResult.PerPage = 1;
				searchResult.NumPages = 1;

				items.Add("__rt", searchResult);
			}
		}
	}

	public class MetadataCacheSnapshot
	{
		private readonly string name;

		//private bool isReady;

		public string Name => name;
		//public bool IsReady => isReady;
		public bool IsReady { get; set; }

		public IPathFormatter PathFormatter { get; }
		public IMetadataCache MetadataCache { get; }
		public ISearchResultCache SearchResultCache { get; }
		public Configuration.ConfigMetadataCache MetadataCaheSettings { get; }

		public MetadataCacheSnapshot(string name, IPathFormatter pathFormatter, IMetadataCache metadataCache, ISearchResultCache searchResultCache, Configuration.ConfigMetadataCache metadataCaheSettings)
		{
			PathFormatter = pathFormatter;
			MetadataCache = metadataCache;
			SearchResultCache = searchResultCache;
			MetadataCaheSettings = metadataCaheSettings;

			this.name = name;
		}

		public string CachedFullDatabaseFilePath => ((PathFormatter)PathFormatter).GetPath(PathFormatter.GetMetadataDirectory() + "/" + name + ".json.gz");

		public bool DoesExist => File.Exists(CachedFullDatabaseFilePath);

		public void EnsureReady()
		{
			if (IsReady)
			{
				return;
			}

			if (DoesExist)
			{
				LoadFromFile();
			}
			else
			{
				AggregateFromFiles();
				SaveToFile();
			}
		}

		public void LoadFromFile()
		{
			string cachedFullDatabaseFilePath = CachedFullDatabaseFilePath;

			if (!DoesExist)
			{
				return;
			}

			List<Metadata> allMetadata = JsonUtility.LoadFromFile<List<Metadata>>(cachedFullDatabaseFilePath, Decode);

			foreach (Metadata metadata in allMetadata)
			{
				if (metadata is null)
				{
					continue;
				}

				if (MetadataCache.Items.ContainsKey(metadata.Id))
				{
					MetadataCache.Items.Remove(metadata.Id);
				}

				MetadataCache.Items.Add(metadata.Id, metadata);
			}

			IsReady = true;
		}

		public void AggregateFromFiles()
		{
			DirectoryInfo dirInfo = new DirectoryInfo(PathFormatter.GetMetadataDirectory());

			foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.json", PathFormatter.IsEnabled ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
			{
				int galleryId;

				if (int.TryParse(Path.GetFileNameWithoutExtension(fileInfo.Name), out galleryId))
				{
					string cachedMetadataFilePath = fileInfo.FullName;
					Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

					if (metadata is null)
					{
						continue;
					}

					if (MetadataCache.Items.ContainsKey(metadata.Id))
					{
						MetadataCache.Items.Remove(metadata.Id);
					}

					MetadataCache.Items.Add(metadata.Id, metadata);
				}
			}

			IsReady = true;
		}

		public void Delete()
		{
			string cachedFullDatabaseFilePath = CachedFullDatabaseFilePath;

			if (File.Exists(cachedFullDatabaseFilePath))
			{
				File.Delete(cachedFullDatabaseFilePath);
			}

			IsReady = false;
		}

		public void SaveToFile()
		{
			string cachedFullDatabaseFilePath = CachedFullDatabaseFilePath;

			JsonUtility.SaveToFile(SearchResultCache.MetadataCache.Items.Values.ToList(), cachedFullDatabaseFilePath, Encode);
		}

		private string Encode(string text)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (GZipStream gzipStream = new GZipStream(memoryStream, MetadataCaheSettings.CompressionLevel, leaveOpen: true))
				{
					gzipStream.Write(buffer, 0, buffer.Length);
				}

				byte[] compressedData = new byte[memoryStream.Length];

				memoryStream.Position = 0;
				memoryStream.Read(compressedData, 0, compressedData.Length);

				byte[] gzipBuffer = new byte[4 + compressedData.Length];

				Buffer.BlockCopy(compressedData, 0, gzipBuffer, 4, compressedData.Length);
				Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzipBuffer, 0, 4);

				return Convert.ToBase64String(gzipBuffer);
			}
		}

		private string Decode(string encodedText)
		{
			byte[] gzipBuffer = Convert.FromBase64String(encodedText);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				int dataLength = BitConverter.ToInt32(gzipBuffer, 0);
				byte[] buffer = new byte[dataLength];

				memoryStream.Write(gzipBuffer, 4, gzipBuffer.Length - 4);
				memoryStream.Position = 0;

				using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					gzipStream.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}
	}
}
