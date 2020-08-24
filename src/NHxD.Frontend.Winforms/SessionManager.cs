using NHxD.Frontend.Winforms.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace NHxD.Frontend.Winforms
{
	public class SessionManager : ISessionManager
	{
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }
		public int RecentSearchLifeSpan { get; set; }
		public int QuerySearchLifeSpan { get; set; }
		public int TaggedSearchLifeSpan { get; set; }
		public ConfigNetwork NetworkSettings { get; set; }

		public SessionManager(IPathFormatter pathFormatter, ISearchResultCache searchResultCache, int recentSearchLifeSpan, int querySearchLifeSpan, int taggedSearchLifeSpan, ConfigNetwork networkSettings)
		{
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;
			RecentSearchLifeSpan = recentSearchLifeSpan;
			QuerySearchLifeSpan = querySearchLifeSpan;
			TaggedSearchLifeSpan = taggedSearchLifeSpan;
			NetworkSettings = networkSettings;
		}

		public string GetSessionQuery(int pageIndex)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}", "all", "page=", pageIndex);
		}

		public string GetSessionQuery(int tagId, int pageIndex)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}{3}{4}", "tagged", "tag_id=", tagId, "&page=", pageIndex);
		}

		public string GetSessionQuery(string query, int pageIndex)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}/{1}{2}{3}{4}", "search", "query=", query, "&page=", pageIndex);
		}


		public string GetSessionFileName(int pageIndex)
		{
			return PathFormatter.GetSession(GetSessionQuery(pageIndex));
		}

		public string GetSessionFileName(int tagId, int pageIndex)
		{
			return PathFormatter.GetSession(GetSessionQuery(tagId, pageIndex));
		}

		public string GetSessionFileName(string query, int pageIndex)
		{
			return PathFormatter.GetSession(GetSessionQuery(query, pageIndex));
		}


		public void ForgetSession(int tagId, int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string sessionId = GetSessionQuery(tagId, pageIndex);

			if (SearchResultCache.Items.ContainsKey(sessionId))
			{
				SearchResultCache.Items.Remove(sessionId);
			}
		}

		public void ForgetSession(string query, int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string sessionId = GetSessionQuery(query, pageIndex);

			if (SearchResultCache.Items.ContainsKey(sessionId))
			{
				SearchResultCache.Items.Remove(sessionId);
			}
		}

		public void ForgetSession(int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string sessionId = GetSessionQuery(pageIndex);

			if (SearchResultCache.Items.ContainsKey(sessionId))
			{
				SearchResultCache.Items.Remove(sessionId);
			}
		}


		public void DeleteSession(int tagId, int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string filePath = GetSessionFileName(tagId, pageIndex);

			if (!File.Exists(filePath))
			{
				return;
			}

			File.Delete(filePath);
		}

		public void DeleteSession(string query, int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string filePath = GetSessionFileName(query, pageIndex);

			if (!File.Exists(filePath))
			{
				return;
			}

			File.Delete(filePath);
		}

		public void DeleteSession(int pageIndex)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			string filePath = GetSessionFileName(pageIndex);

			if (!File.Exists(filePath))
			{
				return;
			}

			File.Delete(filePath);
		}

		public void DeleteExpiredSessions()
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			DeleteExpiredSessions(RecentSearchLifeSpan, PathFormatter.GetSessionDirectory("all"));
			DeleteExpiredSessions(QuerySearchLifeSpan, PathFormatter.GetSessionDirectory("search"));
			DeleteExpiredSessions(TaggedSearchLifeSpan, PathFormatter.GetSessionDirectory("tagged"));
		}

		public void DeleteExpiredSessions(int lifetime, string searchPath)
		{
			if (NetworkSettings.Offline)
			{
				return;
			}

			if (lifetime <= 0)
			{
				return;
			}

			if (!Directory.Exists(searchPath))
			{
				return;
			}

			DateTime now = DateTime.UtcNow;
			DirectoryInfo dirInfo = new DirectoryInfo(searchPath);
			List<string> expiredSessions = new List<string>();

			foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly))
			{
				if ((now - fileInfo.CreationTimeUtc).TotalMilliseconds > lifetime)
				{
					expiredSessions.Add(fileInfo.FullName);
				}
			}

			foreach (string path in expiredSessions)
			{
				File.Delete(path);
			}
		}
	}
}
