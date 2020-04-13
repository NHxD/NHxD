using System;
using System.Collections.Generic;

namespace NHxD
{
	public class BrowsingModel
	{
		private readonly Dictionary<string, BrowsingItem> searchHistory;

		public Dictionary<string, BrowsingItem> SearchHistory => searchHistory;

		public event BrowsingItemChangeEventHandler ItemChanged = delegate { };
		public event BrowsingItemChangeEventHandler ItemAdded = delegate { };
		public event BrowsingItemChangeEventHandler ItemRemoved = delegate { };

		public BrowsingModel()
		{
			searchHistory = new Dictionary<string, BrowsingItem>();
		}

		protected virtual void OnItemChanged(BrowsingItemChangeEventArgs e)
		{
			ItemChanged.Invoke(this, e);
		}

		protected virtual void OnItemAdded(BrowsingItemChangeEventArgs e)
		{
			ItemChanged.Invoke(this, e);
		}

		protected virtual void OnItemRemoved(BrowsingItemChangeEventArgs e)
		{
			ItemRemoved.Invoke(this, e);
		}

		public void RemoveSearchHistory(string uri)
		{
			if (!SearchHistory.ContainsKey(uri))
			{
				return;
			}

			DateTime today = DateTime.Now;
			DateTime creationTime = SearchHistory[uri].CreationTime;

			SearchHistory.Remove(uri);

			OnItemRemoved(new BrowsingItemChangeEventArgs(uri, new BrowsingItem(creationTime, today, today)));
		}

		public void AddSearchHistory(string uri)
		{
			DateTime today = DateTime.Now;

			if (SearchHistory.ContainsKey(uri))
			{
				BrowsingItem searchItem = SearchHistory[uri];

				searchItem.LastAccessTime = today;

				OnItemChanged(new BrowsingItemChangeEventArgs(uri, searchItem));
			}
			else
			{
				BrowsingItem searchItem = new BrowsingItem(today, today, today);

				SearchHistory.Add(uri, searchItem);

				OnItemAdded(new BrowsingItemChangeEventArgs(uri, searchItem));
			}
		}
	}

	public delegate void BrowsingItemChangeEventHandler(object sender, BrowsingItemChangeEventArgs e);

	public class BrowsingItemChangeEventArgs : EventArgs
	{
		public string Key { get; }
		public BrowsingItem Value { get; }

		public BrowsingItemChangeEventArgs(string key, BrowsingItem value)
		{
			Key = key;
			Value = value;
		}
	}

	public class BrowsingItem
	{
		public DateTime CreationTime { get; set; }
		public DateTime LastAccessTime { get; set; }
		public DateTime LastWriteTime { get; set; }

		public BrowsingItem()
		{

		}

		public BrowsingItem(DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime)
		{
			CreationTime = creationTime;
			LastAccessTime = lastAccessTime;
			LastWriteTime = lastWriteTime;
		}
	}

	public class AddBrowsingItemTask
	{
		public string Uri { get; }
		public BrowsingItem Item { get; }

		public AddBrowsingItemTask(string uri, BrowsingItem item)
		{
			Uri = uri;
			Item = item;
		}
	}
}
