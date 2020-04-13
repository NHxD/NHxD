using Nhentai;
using System;
using System.ComponentModel;

namespace NHxD
{
	public class DetailsModel
	{
		private readonly BindingList<int> searches;

		private DetailsTarget target;
		private Metadata metadata;

		public BindingList<int> Searches => searches;

		public Metadata Metadata
		{
			get
			{
				return metadata;
			}
			set
			{
				metadata = value;
				OnMetadataChanged();
			}
		}

		public DetailsTarget Target
		{
			get
			{
				return target;
			}
			set
			{
				target = value;
				OnTargetChanged();
			}
		}

		public event EventHandler SearchesChanged = delegate { };
		public event EventHandler MetadataChanged = delegate { };
		public event EventHandler TargetChanged = delegate { };

		public DetailsModel()
		{
			//Metadata = metadata;
			//Target = target;

			searches = new BindingList<int>();
		}

		protected virtual void OnTargetChanged()
		{
			TargetChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnMetadataChanged()
		{
			MetadataChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSearchItemAdded(int galleryId)
		{
			int itemIndex = Searches.IndexOf(galleryId);

			if (itemIndex != -1)
			{
				Searches.RemoveAt(itemIndex);
			}

			Searches.Insert(0, galleryId);

			SearchesChanged.Invoke(this, EventArgs.Empty);
		}
		public void AddSearch(int galleryId)
		{
			OnSearchItemAdded(galleryId);
		}
	}

	// HACK... because details & download views currently share the same browser for effiency (but makes the code ugly).
	public enum DetailsTarget
	{
		Details,
		Download
	}
}
