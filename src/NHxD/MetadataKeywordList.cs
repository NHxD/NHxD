using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHxD
{
	public class MetadataKeywordList
	{
		private readonly MetadataKeywordListTitles title;
		private readonly MetadataKeywordListTags tags;
		private readonly List<string> scanlator;
		private readonly List<string> uploadDate;
		private readonly List<string> numFavorites;
		private readonly List<string> numPages;
		private readonly List<string> id;
		private readonly List<string> mediaId;

		[JsonProperty("title")]
		public MetadataKeywordListTitles Title => title;

		[JsonProperty("tags")]
		public MetadataKeywordListTags Tags => tags;

		[JsonProperty("scanlator")]
		public List<string> Scanlator => scanlator;

		[JsonProperty("upload_date")]
		public List<string> UploadDate => uploadDate;

		[JsonProperty("num_favorites")]
		public List<string> NumFavorites => numFavorites;

		[JsonProperty("num_pages")]
		public List<string> NumPages => numPages;

		[JsonProperty("id")]
		public List<string> Id => id;

		[JsonProperty("media_id")]
		public List<string> MediaId => mediaId;

		public event MetadataKeywordListItemAddedEventHandler ItemAdded = delegate { };
		public event MetadataKeywordListItemRemovedEventHandler ItemRemoved = delegate { };

		public MetadataKeywordList()
		{
			title = new MetadataKeywordListTitles();
			tags = new MetadataKeywordListTags();
			scanlator = new List<string>();
			uploadDate = new List<string>();
			numFavorites = new List<string>();
			numPages = new List<string>();
			id = new List<string>();
			mediaId = new List<string>();
		}

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
						// TODO: it'd be better if this was prefixed with "Tags." to avoid naming collision.

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
				TagInfo copy = new TagInfo() { Type = tag.Type, Name = tag.Name, Id = tag.Id, Url = tag.Url, Count = tag.Count, CreationTime = tag.CreationTime, LastAccessTime = tag.LastAccessTime, LastWriteTime = tag.LastWriteTime, LastVisitTime = tag.LastVisitTime };

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

		public bool IsInMetadata(Metadata metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			return DoesListContain(Tags.Tag, metadata.Tags.Where(x => x.Type == TagType.Tag).Select(x => x.Name))
				|| DoesListContain(Tags.Artist, metadata.Tags.Where(x => x.Type == TagType.Artist).Select(x => x.Name))
				|| DoesListContain(Tags.Group, metadata.Tags.Where(x => x.Type == TagType.Group).Select(x => x.Name))
				|| DoesListContain(Tags.Category, metadata.Tags.Where(x => x.Type == TagType.Category).Select(x => x.Name))
				|| DoesListContain(Tags.Character, metadata.Tags.Where(x => x.Type == TagType.Character).Select(x => x.Name))
				|| DoesListContain(Tags.Parody, metadata.Tags.Where(x => x.Type == TagType.Parody).Select(x => x.Name))
				|| DoesListContain(Tags.Language, metadata.Tags.Where(x => x.Type == TagType.Language).Select(x => x.Name))
				|| DoesListContain(Title.English, metadata.Title.English, StringComparison.InvariantCultureIgnoreCase)
				|| DoesListContain(Title.Japanese, metadata.Title.Japanese, StringComparison.InvariantCultureIgnoreCase)
				|| DoesListContain(Title.Pretty, metadata.Title.Pretty, StringComparison.InvariantCultureIgnoreCase)
				|| DoesListContain(Scanlator, metadata.Scanlator, StringComparison.InvariantCultureIgnoreCase)
				|| DoesListContain(UploadDate, metadata.UploadDate)
				|| DoesListContain(NumFavorites, metadata.NumFavorites)
				|| DoesListContain(NumPages, metadata.NumPages);
		}

		private bool DoesListContain(IEnumerable<string> list, IEnumerable<string> value)
		{
			foreach (string tagValue in value)
			{
				if (DoesListContain(list, tagValue, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		private bool DoesListContain(IEnumerable<string> list, int value)
		{
			return DoesListContain(list, value.ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase);
		}

		private bool DoesListContain(IEnumerable<string> list, long value)
		{
			string dateTimeShortString = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value).ToShortDateString();

			return DoesListContain(list, dateTimeShortString, StringComparison.OrdinalIgnoreCase);
		}

		private bool DoesListContain(IEnumerable<string> list, string value, StringComparison stringComparison)
		{
			foreach (string token in list)
			{
				if (list.Any(x => x.Equals(value, stringComparison)))
				{
					return true;
				}
			}

			return false;
		}
	}

	public delegate void MetadataKeywordListItemAddedEventHandler(object sender, TagEventArgs e);
	public delegate void MetadataKeywordListItemRemovedEventHandler(object sender, TagEventArgs e);

	public class MetadataKeywordListTitles
	{
		private readonly List<string> english;
		private readonly List<string> japanese;
		private readonly List<string> pretty;

		[JsonProperty("english")]
		public List<string> English => english;

		[JsonProperty("japanese")]
		public List<string> Japanese => japanese;

		[JsonProperty("pretty")]
		public List<string> Pretty => pretty;

		public MetadataKeywordListTitles()
		{
			english = new List<string>();
			japanese = new List<string>();
			pretty = new List<string>();
		}
	}

	public class MetadataKeywordListTags
	{
		private readonly List<string> tag;
		private readonly List<string> artist;
		private readonly List<string> group;
		private readonly List<string> category;
		private readonly List<string> character;
		private readonly List<string> parody;
		private readonly List<string> language;

		[JsonProperty("tag")]
		public List<string> Tag => tag;

		[JsonProperty("artist")]
		public List<string> Artist => artist;

		[JsonProperty("group")]
		public List<string> Group => group;

		[JsonProperty("category")]
		public List<string> Category => category;

		[JsonProperty("character")]
		public List<string> Character => character;

		[JsonProperty("parody")]
		public List<string> Parody => parody;

		[JsonProperty("language")]
		public List<string> Language => language;

		public MetadataKeywordListTags()
		{
			tag = new List<string>();
			artist = new List<string>();
			group = new List<string>();
			category = new List<string>();
			character = new List<string>();
			parody = new List<string>();
			language = new List<string>();
		}
	}
}
