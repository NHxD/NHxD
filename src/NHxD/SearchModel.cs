using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace NHxD.Frontend.Winforms
{
	public class GalleryModel
	{
		private readonly BindingList<string> searches;
		private readonly BindingList<string> filters;

		private ISearchArg searchArg;
		private ISearchProgressArg searchProgressArg;

		public BindingList<string> Searches => searches;
		public BindingList<string> Filters => filters;

		public TagsModel TagsModel { get; }

		public event EventHandler SearchesChanged = delegate { };
		public event EventHandler SearchArgChanged = delegate { };
		public event EventHandler SearchProgressArgChanged = delegate { };
		public event EventHandler FiltersChanged = delegate { };

		public GalleryModel(TagsModel tagsModel)
		{
			TagsModel = tagsModel;

			searches = new BindingList<string>();
			filters = new BindingList<string>();
		}

		public ISearchArg SearchArg
		{
			get
			{
				return searchArg;
			}
			set
			{
				searchArg = value;
				OnSearchArgChanged();
			}
		}

		public ISearchProgressArg SearchProgressArg
		{
			get
			{
				return searchProgressArg;
			}
			set
			{
				searchProgressArg = value;
				OnSearchProgressArgChanged();
			}
		}

		/*public void AddClearFilter()
		{
			filters.Insert(0, "");

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}*/

		public void AddFilter(string filter)
		{
			OnFilterAdded(filter);
		}

		protected virtual void OnFilterAdded(string filter)
		{
			int itemIndex = filters.IndexOf(filter);

			if (itemIndex != -1)
			{
				filters.RemoveAt(itemIndex);
			}

			filters.Insert(0, filter);

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchArgChanged()
		{
			SearchArgChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchProgressArgChanged()
		{
			SearchProgressArgChanged.Invoke(this, EventArgs.Empty);
		}

		public void AddSearch(int tagId, int pageIndex)
		{
			OnSearchItemAdded(tagId, pageIndex);
		}

		public void AddSearch(string query, int pageIndex)
		{
			OnSearchItemAdded(query, pageIndex);
		}

		public void AddSearch(int pageIndex)
		{
			OnSearchItemAdded(pageIndex);
		}




		protected virtual void OnSearchItemAdded(int tagId, int pageIndex)
		{
			string queryString1 = null;

			// match alternate syntaxes and remove them as necessary.
			// (e.g., "tag:" instead of "tagged:", "tag:name" instead of "tag:type:name", "tag:name:1" instead of "tag:name", etc.)

			if (TagsModel.AllTags.Any(x => x.Id == tagId))
			{
				TagInfo tagInfo = TagsModel.AllTags.First(x => x.Id == tagId);

				queryString1 = string.Format(CultureInfo.InvariantCulture, "tagged:{0}:{1}:{2}", tagInfo.Type.ToString().ToLowerInvariant(), QueryParser.SanitizeTagName(tagInfo.Name), pageIndex);

				int itemIndex1 = Searches.IndexOf(queryString1);

				if (itemIndex1 != -1)
				{
					Searches.RemoveAt(itemIndex1);
				}
			}

			string queryString2 = string.Format(CultureInfo.InvariantCulture, "tagged:{0}:{1}", tagId, pageIndex);

			int itemIndex2 = Searches.IndexOf(queryString2);

			if (itemIndex2 != -1)
			{
				Searches.RemoveAt(itemIndex2);
			}

			// prefer keeping the named syntax
			if (!string.IsNullOrEmpty(queryString1))
			{
				Searches.Insert(0, queryString1);
				//Searches.Add(queryString1);
			}
			else
			{
				Searches.Insert(0, queryString2);
				//Searches.Add(queryString2);
			}

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchItemAdded(string query, int pageIndex)
		{
			string queryString = string.Format(CultureInfo.InvariantCulture, "search:{0}:{1}", query, pageIndex);
			int itemIndex = Searches.IndexOf(queryString);

			if (itemIndex != -1)
			{
				Searches.RemoveAt(itemIndex);
			}

			Searches.Insert(0, queryString);
			//Searches.Add(queryString);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchItemAdded(int pageIndex)
		{
			string queryString = string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex);
			int itemIndex = Searches.IndexOf(queryString);

			if (itemIndex != -1)
			{
				Searches.RemoveAt(itemIndex);
			}

			Searches.Insert(0, queryString);
			//Searches.Add(queryString);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveSearch(int pageIndex)
		{
			string queryString = string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex);
			int index = Searches.IndexOf(queryString);

			if (index == -1)
			{
				return;
			}

			Searches.RemoveAt(index);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveSearch(string query, int pageIndex)
		{
			string queryString = string.Format(CultureInfo.InvariantCulture, "search:{0}:{1}", query, pageIndex);
			int index = Searches.IndexOf(queryString);

			if (index == -1)
			{
				return;
			}

			Searches.RemoveAt(index);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveSearch(int tagId, int pageIndex)
		{
			int index = -1;

			if (TagsModel.AllTags.Any(x => x.Id == tagId))
			{
				TagInfo tagInfo = TagsModel.AllTags.First(x => x.Id == tagId);
				string queryString1 = string.Format(CultureInfo.InvariantCulture, "tagged:{0}:{1}:{2}", tagInfo.Type.ToString().ToLowerInvariant(), tagInfo.Name, pageIndex);

				index = Searches.IndexOf(queryString1);
			}

			if (index == -1)
			{
				string queryString2 = string.Format(CultureInfo.InvariantCulture, "tagged:{0}:{1}", tagId, pageIndex);

				index = Searches.IndexOf(queryString2);
			}

			if (index == -1)
			{
				return;
			}

			Searches.RemoveAt(index);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveSearch(string queryString)
		{
			int index = Searches.IndexOf(queryString);

			if (index == -1)
			{
				return;
			}

			Searches.RemoveAt(index);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}

		public void RemoveFilter(string filter)
		{
			int index = Filters.IndexOf(filter);

			if (index == -1)
			{
				return;
			}

			Filters.RemoveAt(index);

			FiltersChanged.Invoke(this, EventArgs.Empty);
		}
	}
}
