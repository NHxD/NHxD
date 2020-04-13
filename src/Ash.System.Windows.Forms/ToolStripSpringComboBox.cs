using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	// based on https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/stretch-a-toolstriptextbox-to-fill-the-remaining-width-of-a-toolstrip-wf

	public class ToolStripSpringComboBox : ToolStripComboBox
	{
		public bool Spring { get; set; } = true;

		public override Size GetPreferredSize(Size constrainingSize)
		{
			// Use the default size if the text box is on the overflow menu
			// or is on a vertical ToolStrip.
			if (!Spring
				|| IsOnOverflow
				|| Owner.Orientation == Orientation.Vertical)
			{
				return DefaultSize;
			}

			// Declare a variable to store the total available width as 
			// it is calculated, starting with the display width of the 
			// owning ToolStrip.
			Int32 width = Owner.DisplayRectangle.Width;

			// Subtract the width of the overflow button if it is displayed. 
			if (Owner.OverflowButton.Visible)
			{
				width = width - Owner.OverflowButton.Width - Owner.OverflowButton.Margin.Horizontal;
			}

			//if (!FocusFullWidth || !base.Focused)
			{
				// Declare a variable to maintain a count of ToolStripSpringTextBox 
				// items currently displayed in the owning ToolStrip. 
				Int32 springBoxCount = 0;

				foreach (ToolStripItem item in Owner.Items)
				{
					// Ignore items on the overflow menu.
					if (item.IsOnOverflow) continue;

					if (item is ToolStripSpringComboBox)
					{
						// For ToolStripSpringTextBox items, increment the count and 
						// subtract the margin width from the total available width.
						springBoxCount++;
						width -= item.Margin.Horizontal;
					}
					else
					{
						// For all other items, subtract the full width from the total
						// available width.
						width -= item.Width + item.Margin.Horizontal;
					}
				}

				// If there are multiple ToolStripSpringTextBox items in the owning
				// ToolStrip, divide the total available width between them. 
				if (springBoxCount > 1) width /= springBoxCount;
			}

			// If the available width is less than the default width, use the
			// default width, forcing one or more items onto the overflow menu.
			if (width < DefaultSize.Width) width = DefaultSize.Width;

			// Retrieve the preferred size from the base class, but change the
			// width to the calculated width. 
			Size size = base.GetPreferredSize(constrainingSize);

			size.Width = width;

			return size;
		}
	}


	public static class ToolStripComboBoxExtensionMethods
	{
		public static void EnableMiddleClickToClear(this ToolStripComboBox toolStripComboBox)
		{
			toolStripComboBox.MouseDown += ToolStripComboBox_MouseClick;
		}

		private static void ToolStripComboBox_MouseClick(object sender, MouseEventArgs e)
		{
			ToolStripComboBox comboBox = sender as ToolStripComboBox;

			if (e.Button == MouseButtons.Middle)
			{
				comboBox.SelectedIndex = -1;
				comboBox.SelectedItem = null;
				comboBox.ComboBox.ResetText();

				HandledMouseEventArgs handledE = e as HandledMouseEventArgs;

				if (handledE != null)
				{
					handledE.Handled = true;
				}
			}
		}

		public static void OverrideUpDownKeys(this ToolStripComboBox toolStripComboBox)
		{
			toolStripComboBox.KeyDown += ToolStripComboBox_KeyDown;
		}

		private static void ToolStripComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			ToolStripComboBox comboBox = sender as ToolStripComboBox;

			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				if (!comboBox.DroppedDown)
				{
					comboBox.Select();
					comboBox.Focus();
					comboBox.DroppedDown = true;

					e.Handled = true;
				}
			}
		}
	}

	public static class ComboBoxExtensionMethods
	{
		public static void EnableMiddleClickToClear(this ComboBox comboBox)
		{
			comboBox.MouseClick += ComboBox_MouseClick;
		}

		private static void ComboBox_MouseClick(object sender, MouseEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;

			if (e.Button == MouseButtons.Right)
			{
				comboBox.SelectedIndex = -1;
				comboBox.SelectedItem = null;
				comboBox.ResetText();

				HandledMouseEventArgs handledE = e as HandledMouseEventArgs;

				if (handledE != null)
				{
					handledE.Handled = true;
				}
			}
		}

		public static void OverrideMouseWheelBehaviour(this ComboBox comboBox)
		{
			comboBox.MouseWheel += ComboBox_MouseWheel;
		}

		private static void ComboBox_MouseWheel(object sender, MouseEventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;

			if (!comboBox.DroppedDown)
			{
				comboBox.Focus();
				comboBox.DroppedDown = true;

				HandledMouseEventArgs handledE = e as HandledMouseEventArgs;

				if (handledE != null)
				{
					handledE.Handled = true;
				}
			}
		}
	}
}
