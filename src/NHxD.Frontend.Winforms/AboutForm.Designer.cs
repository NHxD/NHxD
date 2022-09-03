namespace NHxD.Frontend.Winforms
{
	partial class AboutForm
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

			this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.webBrowser = new Ash.System.Windows.Forms.WebBrowserEx();
			this.okButton = new System.Windows.Forms.Button();
			this.flowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// webBrowser
			// 
			this.webBrowser.AllowNavigation = false;
			this.webBrowser.AllowWebBrowserDrop = false;
			this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowser.IsWebBrowserContextMenuEnabled = false;
			this.webBrowser.Location = new System.Drawing.Point(0, 0);
			this.webBrowser.Margin = new System.Windows.Forms.Padding(0);
			this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser.Name = "webBrowser";
			this.webBrowser.ScriptErrorsSuppressed = true;
			this.webBrowser.ScrollBarsEnabled = false;
			this.webBrowser.Size = new System.Drawing.Size(588, 183);
			this.webBrowser.TabIndex = 1;
			this.webBrowser.WebBrowserShortcutsEnabled = false;
			this.webBrowser.ZoomFactor = 1F;
			// 
			// okButton
			// 
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Location = new System.Drawing.Point(502, 197);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "&OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// flowLayoutPanel
			// 
			this.flowLayoutPanel.Controls.Add(this.okButton);
			this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flowLayoutPanel.Location = new System.Drawing.Point(12, 12);
			this.flowLayoutPanel.Name = "flowLayoutPanel";
			this.flowLayoutPanel.Padding = new System.Windows.Forms.Padding(12);
			this.flowLayoutPanel.Size = new System.Drawing.Size(852, 48);
			this.flowLayoutPanel.TabIndex = 2;
			// 
			// AboutForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.okButton;
			this.ClientSize = new System.Drawing.Size(584, 231);
			this.Controls.Add(this.webBrowser);
			this.Controls.Add(this.flowLayoutPanel);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(600, 270);
			this.Name = "AboutForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			this.Load += new System.EventHandler(this.AboutForm_Load);
			this.flowLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Ash.System.Windows.Forms.WebBrowserEx webBrowser;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
		private System.Windows.Forms.Button okButton;
	}
}