namespace Krkal.RunIde
{
	partial class GameRunForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameRunForm));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.pauseButton = new System.Windows.Forms.ToolStripButton();
			this.terminateButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.garbageCollectorButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.staticVariablesButton = new System.Windows.Forms.ToolStripButton();
			this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.objectBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.toolStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pauseButton,
            this.terminateButton,
            this.toolStripSeparator1,
            this.garbageCollectorButton,
            this.toolStripSeparator2,
            this.staticVariablesButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 24);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(640, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// pauseButton
			// 
			this.pauseButton.CheckOnClick = true;
			this.pauseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.pauseButton.Image = global::Krkal.RunIde.Properties.Resources.PauseHS;
			this.pauseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pauseButton.Name = "pauseButton";
			this.pauseButton.Size = new System.Drawing.Size(23, 22);
			this.pauseButton.Text = "Pause";
			this.pauseButton.ToolTipText = "Pause";
			this.pauseButton.Click += new System.EventHandler(this.pouseButton_Click);
			// 
			// terminateButton
			// 
			this.terminateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.terminateButton.Image = global::Krkal.RunIde.Properties.Resources.StopHS;
			this.terminateButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.terminateButton.Name = "terminateButton";
			this.terminateButton.Size = new System.Drawing.Size(23, 22);
			this.terminateButton.Text = "Terminate Kernel";
			this.terminateButton.Click += new System.EventHandler(this.terminateButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// garbageCollectorButton
			// 
			this.garbageCollectorButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.garbageCollectorButton.Image = global::Krkal.RunIde.Properties.Resources.ConflictHS;
			this.garbageCollectorButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.garbageCollectorButton.Name = "garbageCollectorButton";
			this.garbageCollectorButton.Size = new System.Drawing.Size(23, 22);
			this.garbageCollectorButton.Text = "Run Garbage Collector";
			this.garbageCollectorButton.Click += new System.EventHandler(this.garbageCollectorButton_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// staticVariablesButton
			// 
			this.staticVariablesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.staticVariablesButton.Image = global::Krkal.RunIde.Properties.Resources.Webcontrol_Objectdatasrc;
			this.staticVariablesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.staticVariablesButton.Name = "staticVariablesButton";
			this.staticVariablesButton.Size = new System.Drawing.Size(23, 22);
			this.staticVariablesButton.Text = "Static Variables";
			this.staticVariablesButton.Click += new System.EventHandler(this.staticVariablesButton_Click);
			// 
			// dockPanel
			// 
			this.dockPanel.ActiveAutoHideContent = null;
			this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dockPanel.DockBottomPortion = 150;
			this.dockPanel.DockLeftPortion = 200;
			this.dockPanel.DockRightPortion = 200;
			this.dockPanel.DockTopPortion = 150;
			this.dockPanel.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
			this.dockPanel.Location = new System.Drawing.Point(0, 0);
			this.dockPanel.Name = "dockPanel";
			this.dockPanel.Size = new System.Drawing.Size(640, 384);
			this.dockPanel.TabIndex = 1;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Location = new System.Drawing.Point(0, 362);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(640, 22);
			this.statusStrip1.TabIndex = 4;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(640, 24);
			this.menuStrip1.TabIndex = 5;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.consoleToolStripMenuItem,
            this.objectBrowserToolStripMenuItem});
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
			this.viewToolStripMenuItem.Text = "&View";
			// 
			// consoleToolStripMenuItem
			// 
			this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
			this.consoleToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.consoleToolStripMenuItem.Text = "&Console";
			this.consoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
			// 
			// objectBrowserToolStripMenuItem
			// 
			this.objectBrowserToolStripMenuItem.Name = "objectBrowserToolStripMenuItem";
			this.objectBrowserToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.objectBrowserToolStripMenuItem.Text = "&Object Browser";
			this.objectBrowserToolStripMenuItem.Click += new System.EventHandler(this.objectBrowserToolStripMenuItem_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 33;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// GameRunForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 384);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.dockPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "GameRunForm";
			this.Text = "Krkal";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.GameMainForm_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameMainForm_FormClosing);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton pauseButton;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ToolStripButton terminateButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton garbageCollectorButton;
		private System.Windows.Forms.ToolStripMenuItem objectBrowserToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton staticVariablesButton;
	}
}