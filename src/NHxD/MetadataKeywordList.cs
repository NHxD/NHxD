using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHxD
{
	public class MetadataKeywordList
	{
		[JsonProperty("title")]
		public MetadataKeywordListTitles Title { get; } = new MetadataKeywordListTitles();

		[JsonProperty("tags")]
		public MetadataKeywordListTags Tags { get; } = new MetadataKeywordListTags();

		[JsonProperty("scanlator")]
		public List<string> Scanlator { get; } = new List<string>();

		[JsonProperty("upload_date")]
		public List<string> UploadDate { get; } = new List<string>();

		[JsonProperty("num_favorites")]
		public List<string> NumFavorites { get; } = new List<string>();

		[JsonProperty("num_pages")]
		public List<string> NumPages { get; } = new List<string>();

		[JsonProperty("id")]
		public List<string> Id { get; } = new List<string>();

		[JsonProperty("media_id")]
		public List<string> MediaId { get; } = new List<string>();

		public event MetadataKeywordListItemAddedEventHandler ItemAdded = delegate { };
		public event MetadataKeywordListItemRemovedEventHandler ItemRemoved = delegate { };

		protected virtual void OnItemAdded(TagEventArgs e)
		{
			ItemAdded.Invoke(this, e);
		}

		protected virtual void OnItemRemoved(TagEventArgs e)
		{
			ItemRemoved.Invoke(this, e);
		}

		public List<string> this[string fieldName]
		{
			get
			{
				if (!string.IsNullOrEmpty(fieldName))
				{
					if (fieldName.Equals("Title.English", StringComparison.OrdinalIgnoreCase))
					{
						return Title.English;
					}
					else if (fieldName.Equals("Title.Japanese", StringComparison.OrdinalIgnoreCase))
					{
						return Title.Japanese;
					}
					else if (fieldName.Equals("Title.Pretty", StringComparison.OrdinalIgnoreCase))
					{
						return Title.Pretty;
					}
					else if (fieldName.Equals("Scanlator", StringComparison.OrdinalIgnoreCase))
					{
						return Scanlator;
					}
					else if (fieldName.Equals("UploadDate", StringComparison.OrdinalIgnoreCase))
					{
						return UploadDate;
					}
					else if (fieldName.Equals("NumFavorites", StringComparison.OrdinalIgnoreCase))
					{
						return NumFavorites;
					}
					else if (fieldName.Equals("NumPages", StringComparison.OrdinalIgnoreCase))
					{
						return NumPages;
					}
					else
					{
						TagType tagType;

						if (Enum.TryParse(fieldName, true, out tagType))
						{
							return this[tagType];
						}
					}
				}

				return new List<string>();
			}
		}

		public List<string> this[TagType tagType]
		{
			get
			{
				switch (tagType)
				{
					case TagType.Tag:
						return Tags.Tag;

					case TagType.Artist:
						return Tags.Artist;

					case TagType.Group:
						return Tags.Group;

					case TagType.Parody:
						return Tags.Parody;

					case TagType.Character:
						return Tags.Character;

					case TagType.Category:
						return Tags.Category;

					case TagType.Language:
						return Tags.Language;

					default:
						return new List<string>();
				}
			}
		}

		public bool Any(TagInfo tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			return this[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
		}

		public void Remove(TagInfo tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			List<string> results = this[tag.Type].Where(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)).ToList();

			if (results.Count > 0)
			{
				TagInfo copy = new TagInfo() { Type = tag.Type, Name = tag.Name, Id = tag.Id, Url = tag.Url };

				this[tag.Type].Remove(results[0]);

				OnItemRemoved(new TagEventArgs(copy));
			}
		}

		public void Add(TagInfo tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			this[tag.Type].Add(tag.Name);

			OnItemAdded(new TagEventArgs(tag));
		}
	}

	public delegate void MetadataKeywordListItemAddedEventHandler(object sender, TagEventArgs e);
	public delegate void MetadataKeywordListItemRemovedEventHandler(object sender, TagEventArgs e);


	public class MetadataKeywordListTitles
	{
		[JsonProperty("english")]
		public List<string> English { get; } = new List<string>();

		[JsonProperty("japanese")]
		public List<string> Japanese { get; } = new List<string>();

		[JsonProperty("pretty")]
		public List<string> Pretty { get; } = new List<string>();
	}

	public class MetadataKeywordListTags
	{
		[JsonProperty("tag")]
		public List<string> Tag { get; } = new List<string>();

		[JsonProperty("artist")]
		public List<string> Artist { get; } = new List<string>();

		[JsonProperty("group")]
		public List<string> Group { get; } = new List<string>();

		[JsonProperty("category")]
		public List<string> Category { get; } = new List<string>();

		[JsonProperty("character")]
		public List<string> Character { get; } = new List<string>();

		[JsonProperty("parody")]
		public List<string> Parody { get; } = new List<string>();

		[JsonProperty("language")]
		public List<string> Language { get; } = new List<string>();
	}
}
