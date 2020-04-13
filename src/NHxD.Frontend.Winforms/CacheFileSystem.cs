using Ash.System.Windows.Forms;
using Newtonsoft.Json;
using Nhentai;
using NHxD.Plugin.ArchiveWriter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class CacheFileSystem : ICacheFileSystem
	{
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }
		public List<IArchiveWriter> ArchiveWriters { get; }

		public CacheFileSystem(IPathFormatter pathFormatter, ISearchResultCache searchResultCache, List<IArchiveWriter> archiveWriters)
		{
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;
			ArchiveWriters = archiveWriters;
		}

		public void OpenFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}

			using (new CursorScope(Cursors.WaitCursor))
			{
				Process.Start(new ProcessStartInfo()
				{
					UseShellExecute = true,
					Verb = "open",
					FileName = fileName
				});
			}
		}

		public void OpenFolder(string fileName)
		{
			OpenFile(fileName);
		}

		public void SelectFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}

			using (new CursorScope(Cursors.WaitCursor))
			{
				Process.Start(new ProcessStartInfo()
				{
					FileName = "explorer",
					Arguments = string.Format(CultureInfo.InvariantCulture, "/e, /select, \"{0}\"", fileName.Replace('/', '\\'))
				});
			}
		}

		public void SelectFolder(string fileName)
		{
			SelectFile(fileName);
		}

		public void OpenMetadata(int galleryId)
		{
			WithMetadata(galleryId, OpenFile);
		}

		public void SelectMetadata(int galleryId)
		{
			WithMetadata(galleryId, SelectFile);
		}

		public bool WithMetadata(int galleryId, Action<string> action)
		{
			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			if (!File.Exists(cachedMetadataFilePath))
			{
				return false;
			}

			if (action != null)
			{
				action.Invoke(cachedMetadataFilePath);
			}

			return true;
		}


		public void OpenCover(string metadataJson)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			WithCover(metadata, OpenFile);
		}

		public void SelectCover(string metadataJson)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			WithCover(metadata, SelectFile);
		}

		public bool WithCover(Metadata metadata, Action<string> action)
		{
			string cachedCoverFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedCoverFilePath = PathFormatter.GetCover(metadata);
			}
			else
			{
				cachedCoverFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), metadata.Id, metadata.Images.Cover.GetFileExtension());
			}

			if (!File.Exists(cachedCoverFilePath))
			{
				return false;
			}

			if (action != null)
			{
				action.Invoke(cachedCoverFilePath);
			}

			return true;
		}


		public void OpenPage(string metadataJson, int pageIndex)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			WithPage(metadata, pageIndex, OpenFile);
		}

		public void SelectPage(string metadataJson, int pageIndex)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			WithPage(metadata, pageIndex, SelectFile);
		}

		public bool WithPage(Metadata metadata, int pageIndex, Action<string> action)
		{
			if (metadata == null
				|| pageIndex < 0
				|| pageIndex > metadata.Images.Pages.Count - 1)
			{
				return false;
			}

			string pageCachedFilePath;

			if (PathFormatter.IsEnabled)
			{
				pageCachedFilePath = PathFormatter.GetPage(metadata, pageIndex);
			}
			else
			{
				string paddedIndex = (pageIndex + 1).ToString(CultureInfo.InvariantCulture).PadLeft(GetBaseCount(metadata.Images.Pages.Count), '0');

				pageCachedFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/{2}{3}", PathFormatter.GetCacheDirectory(), metadata.Id, paddedIndex, metadata.Images.Pages[pageIndex].GetFileExtension());
			}

			if (!File.Exists(pageCachedFilePath))
			{
				return false;
			}

			if (action != null)
			{
				action.Invoke(pageCachedFilePath);
			}

			return true;
		}


		public void OpenPagesFolder(int galleryId)
		{
			WithPagesFolder(galleryId, OpenFolder);
		}

		public void SelectPagesFolder(int galleryId)
		{
			WithPagesFolder(galleryId, SelectFolder);
		}

		public bool WithPagesFolder(int galleryId, Action<string> action)
		{
			string cachedPagesPath;

			if (PathFormatter.IsEnabled)
			{
				string cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);

				Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

				if (metadata == null)
				{
					return false;
				}

				cachedPagesPath = PathFormatter.GetPages(metadata);
			}
			else
			{
				cachedPagesPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/", PathFormatter.GetCacheDirectory(), galleryId);
			}

			if (!Directory.Exists(cachedPagesPath))
			{
				return false;
			}

			if (action != null)
			{
				action.Invoke(cachedPagesPath);
			}

			return true;
		}


		public void OpenArchive(int galleryId)
		{
			WithArchive(galleryId, OpenFile);
		}

		public void SelectArchive(int galleryId)
		{
			WithArchive(galleryId, SelectFile);
		}

		public bool WithArchive(int galleryId, Action<string> action)
		{
			List<string> cachedArcihveFilePaths = new List<string>();

			if (PathFormatter.IsEnabled)
			{
				string cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);

				Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

				if (metadata == null)
				{
					return false;
				}

				foreach (IArchiveWriter archiveWriter in ArchiveWriters)
				{
					string archivePath = PathFormatter.GetArchive(metadata, archiveWriter);

					cachedArcihveFilePaths.Add(archivePath);
				}
			}
			else
			{
				foreach (IArchiveWriter archiveWriter in ArchiveWriters)
				{
					string archivePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, archiveWriter.FileExtension);

					cachedArcihveFilePaths.Add(archivePath);
				}
			}

			foreach (string archivePath in cachedArcihveFilePaths)
			{
				if (!File.Exists(archivePath))
				{
					continue;
				}

				if (action != null)
				{
					action.Invoke(archivePath);
				}

				return true;
			}

			return false;
		}


		public void OpenFirstCachedPage(int galleryId)
		{
			WithFirstCachedPage(galleryId, OpenFile);
		}

		public void SelectFirstCachedPage(int galleryId)
		{
			WithFirstCachedPage(galleryId, SelectFile);
		}

		public bool WithFirstCachedPage(int galleryId, Action<string> action)
		{
			return WithCachedPage(galleryId, (num, first, last) => (num >= first && num <= last), action);
		}

		public bool WithCachedPage(int galleryId, Func<int, int, int, bool> predicate, Action<string> action)
		{
			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			if (metadata == null)
			{
				return false;
			}

			string cachedPagesPath;

			if (PathFormatter.IsEnabled)
			{
				cachedPagesPath = PathFormatter.GetPages(metadata);
			}
			else
			{
				cachedPagesPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/", PathFormatter.GetCacheDirectory(), galleryId);
			}

			if (!Directory.Exists(cachedPagesPath))
			{
				return false;
			}

			DirectoryInfo dirInfo = new DirectoryInfo(cachedPagesPath);
			string firstPageFileName = null;
			int numPages = metadata.Images.Pages.Count;

			foreach (FileInfo fileInfo in dirInfo.EnumerateFiles())
			{
				string fileTitle = Path.GetFileNameWithoutExtension(fileInfo.Name).TrimStart(new char[] { '0' });
				int num;

				if (int.TryParse(fileTitle, out num))
				{
					if (predicate.Invoke(num, 1, numPages))
					//if (num >= 1 && num <= metadata.Images.Pages.Count)
					{
						firstPageFileName = fileInfo.FullName;
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(firstPageFileName))
			{
				return false;
			}

			if (action != null)
			{
				action.Invoke(firstPageFileName);
			}

			return true;
		}


		public bool DoesMetadataExists(int galleryId)
		{
			return WithMetadata(galleryId, null);
		}

		public bool DoesCoverExists(int galleryId)
		{
			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			if (metadata == null)
			{
				return false;
			}

			return WithCover(metadata, null);
		}

		public bool AnyPageExists(int galleryId)
		{
			return WithFirstCachedPage(galleryId, null);
		}

		public bool DoesPageExists(int galleryId, int pageIndex)
		{
			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			if (metadata == null
				|| pageIndex < 0
				|| pageIndex > metadata.Images.Pages.Count - 1)
			{
				return false;
			}

			return WithPage(metadata, pageIndex, null);
		}

		public int[] GetCachedPageIndices(int galleryId)
		{
			List<int> indices = new List<int>();

			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			if (metadata == null)
			{
				return indices.ToArray();
			}

			string cachedPagesPath;

			if (PathFormatter.IsEnabled)
			{
				cachedPagesPath = PathFormatter.GetPages(metadata);
			}
			else
			{
				cachedPagesPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/", PathFormatter.GetCacheDirectory(), galleryId);
			}

			if (!Directory.Exists(cachedPagesPath))
			{
				return indices.ToArray();
			}

			DirectoryInfo dirInfo = new DirectoryInfo(cachedPagesPath);

			foreach (FileInfo fileInfo in dirInfo.EnumerateFiles())
			{
				string fileTitle = Path.GetFileNameWithoutExtension(fileInfo.Name).TrimStart(new char[] { '0' });
				int num;

				if (int.TryParse(fileTitle, out num))
				{
					if (num >= 1 && num <= metadata.Images.Pages.Count)
					{
						indices.Add(num);
					}
				}
			}

			return indices.ToArray();
		}

		public bool DoesPagesFolderExists(int galleryId)
		{
			return WithPagesFolder(galleryId, null);
		}

		public bool DoesComicBookArchiveExists(int galleryId)
		{
			return WithArchive(galleryId, null);
		}

		private static int GetBaseCount(int count)
		{
			return (count < 10) ? 1
				: (count < 100) ? 2
				: (count < 1000) ? 3 : 4;
		}
	}
}
