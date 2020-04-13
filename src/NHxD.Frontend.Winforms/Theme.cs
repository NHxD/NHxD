using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	//
	// NOTE: Now obsolete. Might be refactored later.
	//

	public class Theme
	{
		[JsonProperty("lists")]
		public ThemeLists Lists { get; set; } = new ThemeLists();
	}

	public class ThemeLists
	{
		[JsonProperty("tags")]
		public ThemeList Tags { get; set; } = new ThemeList();

		[JsonProperty("bookmarks")]
		public ThemeList Bookmarks { get; set; } = new ThemeList();

		[JsonProperty("library")]
		public ThemeList Library { get; set; } = new ThemeList();

		[JsonProperty("browsing")]
		public ThemeList Browsing { get; set; } = new ThemeList();
	}

	public class ThemeList
	{
		[JsonProperty("treeView")]
		public ThemeTreeView TreeView { get; set; } = new ThemeTreeView();

		[JsonProperty("toolStrip")]
		public ThemeToolStrip ToolStrip { get; set; } = new ThemeToolStrip();
	}

	public class ThemeToolStrip : ThemeControl
	{
		public static void Apply(ToolStrip toolStrip, ThemeToolStrip themer)
		{
			/*
			foreach (ToolStripItem item in toolStrip.Items)
			{
				// TODO: apply theme to item.
			}
			*/
		}
	}

	public class ThemeComboBox : ThemeControl
	{
	}

	public class ThemeCheckBox : ThemeButtonBase
	{
	}

	public class ThemeButtonBase : ThemeControl
	{
		[JsonProperty("image")]
		public ThemeImage Image { get; set; } = new ThemeImage();

		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("textImageRelation")]
		public TextImageRelation TextImageRelation { get; set; } = TextImageRelation.Overlay;

		[JsonConverter(typeof(StringEnumConverter))]
		[JsonProperty("textImageFlags")]
		public TextImageFlags TextImageFlags { get; set; } = TextImageFlags.TextAndImage;

		public static void Apply(ButtonBase button, ThemeButtonBase themer)
		{
			ThemeControl.Apply(button, themer);

			if (themer.TextImageFlags.HasFlag(TextImageFlags.Image))
			{
				if (string.IsNullOrEmpty(themer.Image.Path))
				{
					return;
				}

				if (!File.Exists(themer.Image.Path))
				{
					return;
				}

				Bitmap bitmap = new Bitmap(themer.Image.Path);

				if (!themer.Image.Size.IsEmpty)
				{
					Bitmap oldBitmap = bitmap;

					bitmap = new Bitmap(oldBitmap, themer.Image.Size);

					oldBitmap.Dispose();
				}

				if (bitmap == null)
				{
					return;
				}

				button.TextImageRelation = themer.TextImageRelation;

				if (!themer.TextImageFlags.HasFlag(TextImageFlags.Text))
				{
					button.Text = "";
				}

				button.MinimumSize = bitmap.Size;
				button.Image = (Bitmap)bitmap.Clone();
				bitmap.Dispose();
			}
		}
	}

	public class ThemeTreeView : ThemeControl
	{
	}

	public class ThemeControl
	{
		[JsonProperty("backColor")]
		public Color BackColor { get; set; } = Color.FromKnownColor(KnownColor.Control);

		[JsonProperty("foreColor")]
		public Color ForeColor { get; set; } = Color.FromKnownColor(KnownColor.ControlText);

		[JsonProperty("padding")]
		public Padding Padding { get; set; } = Padding.Empty;

		[JsonProperty("margin")]
		public Padding Margin { get; set; } = new Padding(3);

		[JsonProperty("size")]
		public Size Size { get; set; } = Size.Empty;

		// TODO: font, etc....

		public static void Apply(Control control, ThemeButtonBase themer)
		{
			// FIXME: doesn't seem to work on toolstrip items...
			control.Padding = themer.Padding;
			control.Margin = themer.Margin;

			if (!themer.Size.IsEmpty)
			{
				control.Size = themer.Size;
			}
		}
	}

	public class ThemeImage
	{
		[JsonProperty("path")]
		public string Path { get; set; } = "";

		[JsonProperty("size")]
		public Size Size { get; set; } = Size.Empty;

		// TODO: font, etc....
	}

	// OBSOLETE: there's probably already an enum like this.
	[Flags]
	public enum TextImageFlags
	{
		None = 0,
		Text = 1,
		Image = 2,
		TextAndImage = 1 | 2,
	}
}
