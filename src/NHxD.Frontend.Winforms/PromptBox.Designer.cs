namespace NHxD.Frontend.Winforms
{
	partial class PromptBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();

			this.inputLabel = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.backgroundPictureBox = new System.Windows.Forms.PictureBox();
			this.inputComboBox = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.backgroundPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// inputLabel
			// 
			this.inputLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inputLabel.BackColor = System.Drawing.SystemColors.Window;
			this.inputLabel.Location = new System.Drawing.Point(12, 9);
			this.inputLabel.Name = "inputLabel";
			this.inputLabel.Size = new System.Drawing.Size(361, 23);
			this.inputLabel.TabIndex = 0;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(298, 71);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(217, 71);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// backgroundPictureBox
			// 
			this.backgroundPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.backgroundPictureBox.BackColor = System.Drawing.SystemColors.Window;
			this.backgroundPictureBox.Location = new System.Drawing.Point(0, -4);
			this.backgroundPictureBox.Name = "backgroundPictureBox";
			this.backgroundPictureBox.Size = new System.Drawing.Size(385, 69);
			this.backgroundPictureBox.TabIndex = 4;
			this.backgroundPictureBox.TabStop = false;
			// 
			// inputComboBox
			// 
			this.inputComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.inputComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.inputComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.inputComboBox.FormattingEnabled = true;
			this.inputComboBox.Location = new System.Drawing.Point(15, 35);
			this.inputComboBox.Name = "inputComboBox";
			this.inputComboBox.Size = new System.Drawing.Size(358, 21);
			this.inputComboBox.TabIndex = 1;
			this.inputComboBox.SelectionChangeCommitted += new System.EventHandler(this.InputComboBox_SelectionChangeCommitted);
			this.inputComboBox.TextUpdate += new System.EventHandler(this.InputComboBox_TextUpdate);
			// 
			// PromptBox
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(385, 106);
			this.Controls.Add(this.inputComboBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.inputLabel);
			this.Controls.Add(this.backgroundPictureBox);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(401, 145);
			this.Name = "PromptBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			((System.ComponentModel.ISupportInitialize)(this.backgroundPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label inputLabel;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.PictureBox backgroundPictureBox;
		private System.Windows.Forms.ComboBox inputComboBox;
	}
}