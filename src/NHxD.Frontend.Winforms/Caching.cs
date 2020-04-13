using Nhentai;
using System.Collections.Generic;
using System.Linq;

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
			foreach (var kvp in items)
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
}
