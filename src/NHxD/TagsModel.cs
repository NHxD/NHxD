using Nhentai;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHxD
{
	public class TagsModel
	{
		private readonly List<TagInfo> tags;

		private int newTagsCount;
		
		public int NewTagsCount => newTagsCount;
		// TODO: use a dictionary Dictionary<int, TagInfo> instead of a list.
		// TODO: use the CreationTime property to order by added date.
		public List<TagInfo> AllTags => tags;
		public bool IsDirty => NewTagsCount > 0;

		public event TagEventHandler ItemAdded = delegate { };

		public TagsModel()
		{
			tags = new List<TagInfo>();
			//allTags.AddingNew += AllTags_AddingNew;
		}

		public void AddTag(TagInfo tagInfo)
		{
			AllTags.Add(tagInfo);

			OnItemAdded(tagInfo);
		}

		protected virtual void OnItemAdded(TagInfo tagInfo)
		{
			++newTagsCount;

			ItemAdded.Invoke(this, new TagEventArgs(tagInfo));
		}

		public void MarkAsDirty()
		{
			if (newTagsCount == 0)
			{
				++newTagsCount;
			}
		}

		public void MarkAsCleaned()
		{
			newTagsCount = 0;
		}

		public int CollectTags(SearchResult searchResult)
		{
			int count = 0;

			for (int i = 0; i < searchResult.Result.Count; ++i)
			{
				Metadata metadata = searchResult.Result[i];

				count += CollectTags(metadata);
			}

			return count;
		}

		public int CollectTags(Metadata metadata)
		{
			int addCount = 0;

			foreach (Tag tag in metadata.Tags)
			{
				if (AllTags.Any(x => x.Id == tag.Id))
				{
					TagInfo tagInfo = AllTags.First(x => x.Id == tag.Id);

					if (tagInfo.Type != tag.Type)
					{
						tagInfo.Type = tag.Type;
						MarkAsDirty();
					}

					if (string.IsNullOrEmpty(tagInfo.Name)
						|| !tagInfo.Name.Equals(tag.Name))
					{
						tagInfo.Name = tag.Name;
						tagInfo.Count = tag.Count;
						MarkAsDirty();
					}

					if (tagInfo.Count != tag.Count)
					{
						tagInfo.Count = tag.Count;
						MarkAsDirty();
					}

					if (string.IsNullOrEmpty(tagInfo.Url)
						|| !tagInfo.Url.Equals(tag.Url))
					{
						tagInfo.Url = tag.Url;
						MarkAsDirty();
					}

					continue;
				}

				AddTag(new TagInfo()
				{
					Id = tag.Id,
					Name = tag.Name,
					Type = tag.Type,
					Count = tag.Count,
					Url = tag.Url
					//CreationTime = DateTime.Now
				});

				++addCount;
			}

			return addCount;
		}
	}

	[Flags]
	public enum TagsFilters
	{
		None = 0,
		Whitelist = 1,
		Blacklist = 2,
		Ignorelist = 4,
		Hidelist = 8,
		Other = 16,
		All = Whitelist | Blacklist | Ignorelist | Hidelist | Other,
	}

	public class TagInfo : Tag
	{
		// TODO?
		//[JsonProperty("creationTime")]
		//public DateTime CreationTime { get; set; }
	}

	public delegate void TagEventHandler(object sender, TagEventArgs e);

	public class TagEventArgs : EventArgs
	{
		public Tag Tag { get; }

		public TagEventArgs(Tag tag)
		{
			Tag = tag;
		}
	}
}
