namespace Krkal.Sample
{
	partial class GameMainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameMainForm));
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
			this.mapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.objectListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.mapGraphics = new System.Windows.Forms.ImageList(this.components);
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
			this.pauseButton.Image = global::Krkal.Sample.Properties.Resources.PauseHS;
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
			this.terminateButton.Image = global::Krkal.Sample.Properties.Resources.StopHS;
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
			this.garbageCollectorButton.Image = global::Krkal.Sample.Properties.Resources.ConflictHS;
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
			this.staticVariablesButton.Image = global::Krkal.Sample.Properties.Resources.Webcontrol_Objectdatasrc;
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
            this.objectBrowserToolStripMenuItem,
            this.mapToolStripMenuItem,
            this.objectListToolStripMenuItem});
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
			// mapToolStripMenuItem
			// 
			this.mapToolStripMenuItem.Name = "mapToolStripMenuItem";
			this.mapToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.mapToolStripMenuItem.Text = "&Map";
			this.mapToolStripMenuItem.Click += new System.EventHandler(this.mapToolStripMenuItem_Click);
			// 
			// objectListToolStripMenuItem
			// 
			this.objectListToolStripMenuItem.Name = "objectListToolStripMenuItem";
			this.objectListToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
			this.objectListToolStripMenuItem.Text = "Object &List";
			this.objectListToolStripMenuItem.Click += new System.EventHandler(this.objectListToolStripMenuItem_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 33;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// mapGraphics
			// 
			this.mapGraphics.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("mapGraphics.ImageStream")));
			this.mapGraphics.TransparentColor = System.Drawing.Color.Transparent;
			this.mapGraphics.Images.SetKeyName(0, "Default");
			this.mapGraphics.Images.SetKeyName(1, "60.png");
			this.mapGraphics.Images.SetKeyName(2, "aktivnimina.png");
			this.mapGraphics.Images.SetKeyName(3, "bomba01.png");
			this.mapGraphics.Images.SetKeyName(4, "bomba06.png");
			this.mapGraphics.Images.SetKeyName(5, "bomba.png");
			this.mapGraphics.Images.SetKeyName(6, "dira.png");
			this.mapGraphics.Images.SetKeyName(7, "fotobunka0.png");
			this.mapGraphics.Images.SetKeyName(8, "hajzly - dracek.png");
			this.mapGraphics.Images.SetKeyName(9, "hajzly - hemr.png");
			this.mapGraphics.Images.SetKeyName(10, "hajzly - pasovec.png");
			this.mapGraphics.Images.SetKeyName(11, "hemr2.png");
			this.mapGraphics.Images.SetKeyName(12, "hemr4.png");
			this.mapGraphics.Images.SetKeyName(13, "hemr6.png");
			this.mapGraphics.Images.SetKeyName(14, "hemr8.png");
			this.mapGraphics.Images.SetKeyName(15, "hlina.png");
			this.mapGraphics.Images.SetKeyName(16, "kamen0000.png");
			this.mapGraphics.Images.SetKeyName(17, "kamenyA.png");
			this.mapGraphics.Images.SetKeyName(18, "klic_cerveny.png");
			this.mapGraphics.Images.SetKeyName(19, "klic_modry.png");
			this.mapGraphics.Images.SetKeyName(20, "klic_zluty.png");
			this.mapGraphics.Images.SetKeyName(21, "konec60.png");
			this.mapGraphics.Images.SetKeyName(22, "koule.png");
			this.mapGraphics.Images.SetKeyName(23, "led.png");
			this.mapGraphics.Images.SetKeyName(24, "pasovec2.png");
			this.mapGraphics.Images.SetKeyName(25, "pasovec4.png");
			this.mapGraphics.Images.SetKeyName(26, "pasovec6.png");
			this.mapGraphics.Images.SetKeyName(27, "pasovec8.png");
			this.mapGraphics.Images.SetKeyName(28, "potvoraznackova2.png");
			this.mapGraphics.Images.SetKeyName(29, "potvoraznackova4.png");
			this.mapGraphics.Images.SetKeyName(30, "potvoraznackova6.png");
			this.mapGraphics.Images.SetKeyName(31, "potvoraznackova8.png");
			this.mapGraphics.Images.SetKeyName(32, "prepinac0.png");
			this.mapGraphics.Images.SetKeyName(33, "prepinac1.png");
			this.mapGraphics.Images.SetKeyName(34, "propadlo.png");
			this.mapGraphics.Images.SetKeyName(35, "stena0000.png");
			this.mapGraphics.Images.SetKeyName(36, "stenaroz.png");
			this.mapGraphics.Images.SetKeyName(37, "teleport1.png");
			this.mapGraphics.Images.SetKeyName(38, "zamekB.png");
			this.mapGraphics.Images.SetKeyName(39, "zamekR.png");
			this.mapGraphics.Images.SetKeyName(40, "zamekY.png");
			this.mapGraphics.Images.SetKeyName(41, "prikazanysmer8.png");
			this.mapGraphics.Images.SetKeyName(42, "prikazanysmer2.png");
			this.mapGraphics.Images.SetKeyName(43, "prikazanysmer4.png");
			this.mapGraphics.Images.SetKeyName(44, "prikazanysmer6.png");
			// 
			// GameMainForm
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
			this.Name = "GameMainForm";
			this.Text = "Krkal";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameMainForm_FormClosing);
			this.Load += new System.EventHandler(this.GameMainForm_Load);
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
		private System.Windows.Forms.ToolStripMenuItem mapToolStripMenuItem;
		private System.Windows.Forms.ImageList mapGraphics;
		private System.Windows.Forms.ToolStripMenuItem objectListToolStripMenuItem;
	}
}