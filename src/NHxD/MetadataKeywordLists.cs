using System;

namespace NHxD
{
	public class MetadataKeywordLists
	{
		private readonly MetadataKeywordList whitelist;
		private readonly MetadataKeywordList blacklist;
		private readonly MetadataKeywordList ignorelist;
		private readonly MetadataKeywordList hidelist;

		public MetadataKeywordList Whitelist => whitelist;
		public MetadataKeywordList Blacklist => blacklist;
		public MetadataKeywordList Ignorelist => ignorelist;
		public MetadataKeywordList Hidelist => hidelist;

		public event MetadataKeywordListChangedEventHandler WhitelistChanged = delegate { };
		public event MetadataKeywordListChangedEventHandler BlacklistChanged = delegate { };
		public event MetadataKeywordListChangedEventHandler IgnorelistChanged = delegate { };
		public event MetadataKeywordListChangedEventHandler HidelistChanged = delegate { };

		public MetadataKeywordLists()
		{
			whitelist = new MetadataKeywordList();
			blacklist = new MetadataKeywordList();
			ignorelist = new MetadataKeywordList();
			hidelist = new MetadataKeywordList();

			whitelist.ItemRemoved += Whitelist_ItemRemoved;
			blacklist.ItemRemoved += Blacklist_ItemRemoved;
			ignorelist.ItemRemoved += Ignorelist_ItemRemoved;
			hidelist.ItemRemoved += Hidelist_ItemRemoved;
			
			whitelist.ItemAdded += Whitelist_ItemAdded;
			blacklist.ItemAdded += Blacklist_ItemAdded;
			ignorelist.ItemAdded += Ignorelist_ItemAdded;
			hidelist.ItemAdded += Hidelist_ItemAdded;
		}

		private void Whitelist_ItemRemoved(object sender, TagEventArgs e)
		{
			WhitelistChanged.Invoke(whitelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Remove, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Blacklist_ItemRemoved(object sender, TagEventArgs e)
		{
			BlacklistChanged.Invoke(blacklist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Remove, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Ignorelist_ItemRemoved(object sender, TagEventArgs e)
		{
			IgnorelistChanged.Invoke(ignorelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Remove, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Hidelist_ItemRemoved(object sender, TagEventArgs e)
		{
			HidelistChanged.Invoke(hidelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Remove, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}


		private void Whitelist_ItemAdded(object sender, TagEventArgs e)
		{
			WhitelistChanged.Invoke(whitelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Add, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Blacklist_ItemAdded(object sender, TagEventArgs e)
		{
			BlacklistChanged.Invoke(blacklist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Add, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Ignorelist_ItemAdded(object sender, TagEventArgs e)
		{
			IgnorelistChanged.Invoke(ignorelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Add, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}

		private void Hidelist_ItemAdded(object sender, TagEventArgs e)
		{
			HidelistChanged.Invoke(hidelist, new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Add, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id));
		}
	}

	public enum MetadataKeywordsListsEventType
	{
		None,
		Add,
		Remove
	}

	public delegate void MetadataKeywordListChangedEventHandler(object sender, MetadataKeywordListChangedEventArgs e);

	public class MetadataKeywordListChangedEventArgs : EventArgs
	{
		public MetadataKeywordsListsEventType EventType { get; }
		public string TagType { get; }
		public string TagName { get; }
		public int TagId { get; }

		public MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType eventType, string tagType, string tagName, int tagId)
		{
			EventType = eventType;
			TagType = tagType;
			TagName = tagName;
			TagId = tagId;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
				EventType.ToString().ToLowerInvariant(),
				TagType?.ToLowerInvariant() ?? "",
				TagName
			};
		}
	}
}
