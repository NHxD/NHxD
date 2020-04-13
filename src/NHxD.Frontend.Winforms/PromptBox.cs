using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class PromptBox : Form
	{
		private string inputText;
		private string inputLabelText;
		private List<string> inputValues = new List<string>();

		public string InputText
		{
			get
			{
				return inputText;
			}
			set
			{
				inputText = value;
				OnInputTextChanged();
			}
		}

		public string InputLabelText
		{
			get
			{
				return inputLabelText;
			}
			set
			{
				inputLabelText = value;
				OnInputLabelTextChanged();
			}
		}

		public List<string> InputValues
		{
			get
			{
				return inputValues;
			}
			set
			{
				inputValues = value;
				OnInputValuesChanged();
			}
		}

		public event EventHandler InputTextChanged = delegate { };
		public event EventHandler InputLabelTextChanged = delegate { };
		public event EventHandler InputValuesChanged = delegate { };

		public PromptBox()
		{
			InitializeComponent();
		}

		public static string Show(string header, string input, string title)
		{
			return Show(header, input, title, null);
		}

		public static string Show(string header, string input, string title, string[] defaultValues)
		{
			using (PromptBox dialog = new PromptBox())
			{
				dialog.Text = title;
				dialog.InputLabelText = header;
				dialog.InputText = input;
				dialog.InputValues = defaultValues != null ? new List<string>(defaultValues) : null;

				if (dialog.ShowDialog() == DialogResult.OK)
				{
					return dialog.InputText;
				}
			}

			return null;
		}

		protected virtual void OnInputTextChanged()
		{
			if (inputComboBox != null)
			{
				inputComboBox.Text = inputText;
			}

			InputTextChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnInputLabelTextChanged()
		{
			if (inputLabel != null)
			{
				inputLabel.Text = inputLabelText;
			}

			InputLabelTextChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnInputValuesChanged()
		{
			if (inputComboBox != null)
			{
				inputComboBox.Items.AddRange(inputValues.Cast<object>().ToArray());
			}

			InputValuesChanged.Invoke(this, EventArgs.Empty);
		}

		private void InputComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;

			if (comboBox == null)
			{
				return;
			}

			InputText = (comboBox.SelectedItem as string) ?? "";
		}

		private void InputComboBox_TextUpdate(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;

			if (comboBox == null)
			{
				return;
			}

			InputText = comboBox.Text;
		}
	}
}
