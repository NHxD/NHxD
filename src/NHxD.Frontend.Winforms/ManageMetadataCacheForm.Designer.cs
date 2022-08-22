
namespace NHxD.Frontend.Winforms
{
	partial class ManageMetadataCacheForm
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
			this.components = new System.ComponentModel.Container();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.unloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.onProgramStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buildAtStartupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadAtStartupToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.onProgramExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteAtExitCacheToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compressionLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fastestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optimalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.cancelOperationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusStrip1
			// 
			this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
			this.statusStrip1.Location = new System.Drawing.Point(0, 240);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(610, 26);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 1;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 18);
			// 
			// toolStripStatusLabel1
			// 
			this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.Size = new System.Drawing.Size(493, 20);
			this.toolStripStatusLabel1.Spring = true;
			this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
			this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// menuStrip1
			// 
			this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.onProgramStartToolStripMenuItem,
            this.onProgramExitToolStripMenuItem,
            this.compressionLevelToolStripMenuItem,
            this.cancelOperationToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(610, 28);
			this.menuStrip1.TabIndex = 8;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildToolStripMenuItem,
            this.loadToolStripMenuItem,
            this.unloadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.deleteToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
			this.fileToolStripMenuItem.Text = "&File";
			this.fileToolStripMenuItem.DropDownOpening += new System.EventHandler(this.fileToolStripMenuItem_DropDownOpening);
			// 
			// buildToolStripMenuItem
			// 
			this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
			this.buildToolStripMenuItem.Size = new System.Drawing.Size(140, 26);
			this.buildToolStripMenuItem.Text = "&Build";
			this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
			// 
			// loadToolStripMenuItem
			// 
			this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
			this.loadToolStripMenuItem.Size = new System.Drawing.Size(140, 26);
			this.loadToolStripMenuItem.Text = "&Load";
			this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
			// 
			// unloadToolStripMenuItem
			// 
			this.unloadToolStripMenuItem.Name = "unloadToolStripMenuItem";
			this.unloadToolStripMenuItem.Size = new System.Drawing.Size(140, 26);
			this.unloadToolStripMenuItem.Text = "&Unload";
			this.unloadToolStripMenuItem.Click += new System.EventHandler(this.unloadToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(140, 26);
			this.saveToolStripMenuItem.Text = "&Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(140, 26);
			this.deleteToolStripMenuItem.Text = "&Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// onProgramStartToolStripMenuItem
			// 
			this.onProgramStartToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildAtStartupToolStripMenuItem,
            this.loadAtStartupToolStripMenuItem1});
			this.onProgramStartToolStripMenuItem.Name = "onProgramStartToolStripMenuItem";
			this.onProgramStartToolStripMenuItem.Size = new System.Drawing.Size(138, 24);
			this.onProgramStartToolStripMenuItem.Text = "On Program &Start";
			this.onProgramStartToolStripMenuItem.DropDownOpening += new System.EventHandler(this.onProgramStartToolStripMenuItem_DropDownOpening);
			// 
			// buildAtStartupToolStripMenuItem
			// 
			this.buildAtStartupToolStripMenuItem.Name = "buildAtStartupToolStripMenuItem";
			this.buildAtStartupToolStripMenuItem.Size = new System.Drawing.Size(126, 26);
			this.buildAtStartupToolStripMenuItem.Text = "&Build";
			this.buildAtStartupToolStripMenuItem.Click += new System.EventHandler(this.buildAtStartupToolStripMenuItem_Click);
			// 
			// loadAtStartupToolStripMenuItem1
			// 
			this.loadAtStartupToolStripMenuItem1.Name = "loadAtStartupToolStripMenuItem1";
			this.loadAtStartupToolStripMenuItem1.Size = new System.Drawing.Size(126, 26);
			this.loadAtStartupToolStripMenuItem1.Text = "&Load";
			this.loadAtStartupToolStripMenuItem1.Click += new System.EventHandler(this.loadAtStartupToolStripMenuItem1_Click);
			// 
			// onProgramExitToolStripMenuItem
			// 
			this.onProgramExitToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteAtExitCacheToolStripMenuItem});
			this.onProgramExitToolStripMenuItem.Name = "onProgramExitToolStripMenuItem";
			this.onProgramExitToolStripMenuItem.Size = new System.Drawing.Size(131, 24);
			this.onProgramExitToolStripMenuItem.Text = "On Program Exit";
			this.onProgramExitToolStripMenuItem.DropDownOpening += new System.EventHandler(this.onProgramExitToolStripMenuItem_DropDownOpening);
			// 
			// deleteAtExitCacheToolStripMenuItem
			// 
			this.deleteAtExitCacheToolStripMenuItem.Name = "deleteAtExitCacheToolStripMenuItem";
			this.deleteAtExitCacheToolStripMenuItem.Size = new System.Drawing.Size(136, 26);
			this.deleteAtExitCacheToolStripMenuItem.Text = "&Delete";
			this.deleteAtExitCacheToolStripMenuItem.Click += new System.EventHandler(this.deleteAtExitCacheToolStripMenuItem_Click);
			// 
			// compressionLevelToolStripMenuItem
			// 
			this.compressionLevelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem,
            this.fastestToolStripMenuItem,
            this.optimalToolStripMenuItem});
			this.compressionLevelToolStripMenuItem.Name = "compressionLevelToolStripMenuItem";
			this.compressionLevelToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
			this.compressionLevelToolStripMenuItem.Text = "&Compression Level";
			this.compressionLevelToolStripMenuItem.DropDownOpening += new System.EventHandler(this.compressionLevelToolStripMenuItem_DropDownOpening);
			// 
			// noneToolStripMenuItem
			// 
			this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
			this.noneToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
			this.noneToolStripMenuItem.Text = "&None";
			this.noneToolStripMenuItem.Click += new System.EventHandler(this.noneToolStripMenuItem_Click);
			// 
			// fastestToolStripMenuItem
			// 
			this.fastestToolStripMenuItem.Name = "fastestToolStripMenuItem";
			this.fastestToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
			this.fastestToolStripMenuItem.Text = "&Fastest";
			this.fastestToolStripMenuItem.Click += new System.EventHandler(this.fastestToolStripMenuItem_Click);
			// 
			// optimalToolStripMenuItem
			// 
			this.optimalToolStripMenuItem.Name = "optimalToolStripMenuItem";
			this.optimalToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
			this.optimalToolStripMenuItem.Text = "&Optimal";
			this.optimalToolStripMenuItem.Click += new System.EventHandler(this.optimalToolStripMenuItem_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 43);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(187, 17);
			this.label1.TabIndex = 9;
			this.label1.Text = "&Metadata loaded in memory:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(46, 17);
			this.label2.TabIndex = 10;
			this.label2.Text = "label2";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 137);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 17);
			this.label3.TabIndex = 12;
			this.label3.Text = "label3";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 109);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(149, 17);
			this.label4.TabIndex = 11;
			this.label4.Text = "&Metadata files on disk:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(12, 203);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(46, 17);
			this.label5.TabIndex = 14;
			this.label5.Text = "label5";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(12, 175);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(162, 17);
			this.label6.TabIndex = 13;
			this.label6.Text = "&Metadata cache on disk:";
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.WorkerReportsProgress = true;
			this.backgroundWorker1.WorkerSupportsCancellation = true;
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
			this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// backgroundWorker2
			// 
			this.backgroundWorker2.WorkerReportsProgress = true;
			this.backgroundWorker2.WorkerSupportsCancellation = true;
			this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
			this.backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker2_ProgressChanged);
			this.backgroundWorker2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// cancelOperationToolStripMenuItem
			// 
			this.cancelOperationToolStripMenuItem.Name = "cancelOperationToolStripMenuItem";
			this.cancelOperationToolStripMenuItem.Size = new System.Drawing.Size(67, 24);
			this.cancelOperationToolStripMenuItem.Text = "&Cancel";
			this.cancelOperationToolStripMenuItem.Visible = false;
			this.cancelOperationToolStripMenuItem.Click += new System.EventHandler(this.cancelOperationToolStripMenuItem_Click);
			// 
			// ManageMetadataCacheForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(610, 266);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ManageMetadataCacheForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Manage Metadata Cache";
			this.Load += new System.EventHandler(this.ManageMetadataCacheForm_Load);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
		private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem onProgramStartToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem buildAtStartupToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadAtStartupToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem onProgramExitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteAtExitCacheToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem compressionLevelToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fastestToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optimalToolStripMenuItem;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private System.ComponentModel.BackgroundWorker backgroundWorker2;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ToolStripMenuItem unloadToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cancelOperationToolStripMenuItem;
	}
}