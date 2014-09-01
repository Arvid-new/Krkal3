namespace Krkal.RunIde
{
	partial class ObjectBrowser
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectBrowser));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.refreshButton = new System.Windows.Forms.ToolStripButton();
			this.staticVariablesButton = new System.Windows.Forms.ToolStripButton();
			this.treeView1 = new System.Windows.Forms.TreeView();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.killButton = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshButton,
            this.staticVariablesButton,
            this.killButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(195, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// refreshButton
			// 
			this.refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.refreshButton.Image = global::Krkal.RunIde.Properties.Resources.RefreshDocViewHS;
			this.refreshButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(23, 22);
			this.refreshButton.Text = "Refresh";
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
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
			// treeView1
			// 
			this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView1.ImageIndex = 0;
			this.treeView1.ImageList = this.imageList2;
			this.treeView1.Location = new System.Drawing.Point(0, 25);
			this.treeView1.Name = "treeView1";
			this.treeView1.SelectedImageIndex = 0;
			this.treeView1.ShowRootLines = false;
			this.treeView1.Size = new System.Drawing.Size(195, 383);
			this.treeView1.TabIndex = 1;
			this.treeView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView1_KeyDown);
			this.treeView1.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterExpand);
			// 
			// imageList2
			// 
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList2.Images.SetKeyName(0, "Int");
			this.imageList2.Images.SetKeyName(1, "Object");
			this.imageList2.Images.SetKeyName(2, "Char");
			this.imageList2.Images.SetKeyName(3, "Array");
			this.imageList2.Images.SetKeyName(4, "Double");
			this.imageList2.Images.SetKeyName(5, "Name");
			// 
			// killButton
			// 
			this.killButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.killButton.Image = ((System.Drawing.Image)(resources.GetObject("killButton.Image")));
			this.killButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.killButton.Name = "killButton";
			this.killButton.Size = new System.Drawing.Size(23, 22);
			this.killButton.Text = "Kill Selected Object";
			this.killButton.Click += new System.EventHandler(this.killButton_Click);
			// 
			// ObjectBrowser
			// 
			this.ClientSize = new System.Drawing.Size(195, 408);
			this.Controls.Add(this.treeView1);
			this.Controls.Add(this.toolStrip1);
			this.Name = "ObjectBrowser";
			this.Text = "Object Browser";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.TreeView treeView1;
		private System.Windows.Forms.ToolStripButton refreshButton;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.ToolStripButton staticVariablesButton;
		private System.Windows.Forms.ToolStripButton killButton;

	}
}
