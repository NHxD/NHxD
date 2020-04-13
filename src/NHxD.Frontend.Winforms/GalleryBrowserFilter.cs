using System;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class GalleryBrowserFilter
	{
		private GallerySortType sortType;
		private SortOrder sortOrder;
		private string text;

		public GallerySortType SortType
		{
			get
			{
				return sortType;
			}
			set
			{
				sortType = value;
				OnSortTypeChanged();
			}
		}

		public SortOrder SortOrder
		{
			get
			{
				return sortOrder;
			}
			set
			{
				sortOrder = value;
				OnSortOrderChanged();
			}
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
				OnTextChanged();
			}
		}

		public event EventHandler SortTypeChanged = delegate { };
		public event EventHandler SortOrderChanged = delegate { };
		public event EventHandler TextChanged = delegate { };

		public Configuration.ConfigGalleryBrowserView GalleryBrowserSettings { get; }

		public GalleryBrowserFilter(Configuration.ConfigGalleryBrowserView galleryBrowserSettings)
		{
			GalleryBrowserSettings = galleryBrowserSettings;

			text = "";
			sortType = galleryBrowserSettings.SortType;
			sortOrder = galleryBrowserSettings.SortOrder;
		}

		protected virtual void OnSortTypeChanged()
		{
			SortTypeChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSortOrderChanged()
		{
			SortOrderChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTextChanged()
		{
			TextChanged.Invoke(this, EventArgs.Empty);
		}
	}
}
