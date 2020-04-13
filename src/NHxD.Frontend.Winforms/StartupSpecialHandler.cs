using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHxD.Frontend.Winforms
{
	public class StartupSpecialHandler
	{
		private static readonly Dictionary<string, StartupSpecialItem> SpecialStartupItems = new Dictionary<string, StartupSpecialItem>()
		{
			{ "1-1", new StartupSpecialItem(StartupSpecialDateFilters.NewYearsDay, new StartupSpecialItemValueCollection { { 17773, TagsFilters.Blacklist } }) },		// kimono
			{ "1-25", new StartupSpecialItem(StartupSpecialDateFilters.LunarNewYear, new StartupSpecialItemValueCollection { { 24450, TagsFilters.Blacklist } }) },		// chinese dress
			{ "4-13", new StartupSpecialItem(StartupSpecialDateFilters.Easter, new StartupSpecialItemValueCollection { { 23132, TagsFilters.Blacklist } }) },        // bunny girl
			{ "5-10", new StartupSpecialItem(StartupSpecialDateFilters.MothersDay, new StartupSpecialItemValueCollection  { { 15853, TagsFilters.Whitelist } }) },		// mother
			{ "5-12", new StartupSpecialItem(StartupSpecialDateFilters.InternationalNursesDay, new StartupSpecialItemValueCollection  { { 6525, TagsFilters.Blacklist } }) },		// nurse
			{ "7-24", new StartupSpecialItem(StartupSpecialDateFilters.SportsDay, new StartupSpecialItemValueCollection { { 17349, TagsFilters.Blacklist } }) },		// tracksuit
			{ "10-31", new StartupSpecialItem(StartupSpecialDateFilters.Halloween, new StartupSpecialItemValueCollection { { 7546, TagsFilters.Blacklist } }) },	// witch
			{ "11-19", new StartupSpecialItem(StartupSpecialDateFilters.WorldToiletDay, new StartupSpecialItemValueCollection { { 32282, TagsFilters.Whitelist }, { 10476, TagsFilters.Whitelist }, { 2820, TagsFilters.Whitelist }, { 8391, TagsFilters.Whitelist } }) },	// piss drinking, urination, scat, public use
			{ "12-8", new StartupSpecialItem(StartupSpecialDateFilters.FeastOfTheImmaculateConception, new StartupSpecialItemValueCollection { { 2515, TagsFilters.Whitelist }, { 29224, TagsFilters.Whitelist }, { 6343, TagsFilters.Whitelist } }) },	// virginity, impregnation, pregnant
			{ "12-25", new StartupSpecialItem(StartupSpecialDateFilters.Christmas, new StartupSpecialItemValueCollection { { 30811, TagsFilters.Blacklist } }) },	// chritsmas
		};

		public Configuration.ConfigGallery GallerySettings { get; }
		public TagsModel TagsModel { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public SearchHandler SearchHandler { get; }
		

		public StartupSpecialHandler(Configuration.ConfigGallery gallerySettings, TagsModel tagsModel, MetadataKeywordLists metadataKeywordLists, SearchHandler searchHandler)
		{
			GallerySettings = gallerySettings;
			TagsModel = tagsModel;
			MetadataKeywordLists = metadataKeywordLists;
			SearchHandler = searchHandler;
		}

		public bool Execute()
		{
			if (GallerySettings.StartupSpecialDateFilters == StartupSpecialDateFilters.None)
			{
				return false;
			}

			DateTime today = DateTime.Now;
			string todayString = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", today.Month, today.Day);
			StartupSpecialItem specialItem;

			if (GallerySettings.PreventMultipleStartupSpecial
				&& GallerySettings.LatestActivatedStartupSpecialDate != null
				&& today.Date.Equals(GallerySettings.LatestActivatedStartupSpecialDate.Date))
			{
				return false;
			}

			if (!SpecialStartupItems.TryGetValue(todayString, out specialItem))
			{
				return false;
			}

			if (!GallerySettings.StartupSpecialDateFilters.HasFlag(specialItem.DateFilters))
			{
				return false;
			}

			List<int> allowedtagIds = specialItem.Values.Select(x => x.Item1).ToList();

			for (int i = 0; i < specialItem.Values.Count; ++i)
			{
				var value = specialItem.Values[i];

				if (TagsModel.AllTags.Any(x => x.Id == value.Item1))
				{
					TagInfo tag = TagsModel.AllTags.First(x => x.Id == value.Item1);

					if (value.Item2 != TagsFilters.None)
					{
						// require tag to be whitelisted.
						if (value.Item2.HasFlag(TagsFilters.Whitelist)
							&& !MetadataKeywordLists.Whitelist["tag"].Contains(tag.Name))
						{
							continue;
						}
						// require tag to be not blacklisted.
						else if (value.Item2.HasFlag(TagsFilters.Blacklist)
							&& MetadataKeywordLists.Blacklist["tag"].Contains(tag.Name))
						{
							continue;
						}

						// require tag to be not ignorelisted.
						if (value.Item2.HasFlag(TagsFilters.Ignorelist)
							&& MetadataKeywordLists.Ignorelist["tag"].Contains(tag.Name))
						{
							continue;
						}
					}

					allowedtagIds.Add(i);
				}
			}

			if (allowedtagIds.Count == 0)
			{
				return false;
			}

			int randomTagId = allowedtagIds[(new Random()).Next(allowedtagIds.Count)];

			SearchHandler.ParseAndExecuteSearchText("tagged:" + randomTagId.ToString(CultureInfo.InvariantCulture));

			GallerySettings.LatestActivatedStartupSpecialDate = today;

			return true;
		}
	}

	public class StartupSpecialItem
	{
		public StartupSpecialDateFilters DateFilters { get; }
		public StartupSpecialItemValueCollection Values { get; }

		public StartupSpecialItem(StartupSpecialDateFilters filters, StartupSpecialItemValueCollection values)
		{
			DateFilters = filters;
			Values = values;
		}
	}

	public class StartupSpecialItemValueCollection : List<Tuple<int, TagsFilters>>
	{
		public void Add(int item, TagsFilters item2)
		{
			Add(new Tuple<int, TagsFilters>(item, item2));
		}
	}

	public class StartupSpecialItemValue
	{
		public int TagId { get; }
		public TagsFilters TagsFilters { get; }

		public StartupSpecialItemValue(int tagId, TagsFilters tagsFilters)
		{
			TagId = tagId;
			TagsFilters = tagsFilters;
		}
	}

	[Flags]
	public enum StartupSpecialDateFilters
	{
		None = 0,
		NewYearsDay = 1,
		LunarNewYear = 2,
		Easter = 4,
		MothersDay = 8,
		InternationalNursesDay = 16,
		SportsDay = 32,
		Halloween = 64,
		WorldToiletDay = 128,
		FeastOfTheImmaculateConception = 256,
		Christmas = 512,
		All = NewYearsDay | LunarNewYear | Easter | MothersDay | InternationalNursesDay | SportsDay | Halloween | WorldToiletDay | FeastOfTheImmaculateConception | Christmas
	}
}
