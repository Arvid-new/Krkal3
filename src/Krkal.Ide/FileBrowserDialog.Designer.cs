namespace Krkal.Ide
{
	partial class FileBrowserDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileBrowserDialog));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.directoryBox = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripDropDownButton();
			this.largeIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.smallIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.newFileButton = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripDropDownButton();
			this.newDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.fileList = new System.Windows.Forms.ListView();
			this.fileIconsLarge = new System.Windows.Forms.ImageList(this.components);
			this.fileIconsSmall = new System.Windows.Forms.ImageList(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.selectionTextBox = new System.Windows.Forms.TextBox();
			this.packUnpackButton = new System.Windows.Forms.Button();
			this.deleteButton = new System.Windows.Forms.Button();
			this.moveButton = new System.Windows.Forms.Button();
			this.copyButton = new System.Windows.Forms.Button();
			this.selectAllButton = new System.Windows.Forms.Button();
			this.selectOneButton = new System.Windows.Forms.Button();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.directoryBox,
            this.toolStripButton1,
            this.toolStripSplitButton1,
            this.toolStripSeparator1,
            this.newFileButton,
            this.toolStripButton2});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(592, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.AutoSize = false;
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(95, 22);
			this.toolStripLabel1.Text = "Look in:";
			// 
			// directoryBox
			// 
			this.directoryBox.AutoSize = false;
			this.directoryBox.AutoToolTip = true;
			this.directoryBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.directoryBox.MaxDropDownItems = 16;
			this.directoryBox.Name = "directoryBox";
			this.directoryBox.Size = new System.Drawing.Size(250, 21);
			this.directoryBox.SelectedIndexChanged += new System.EventHandler(this.directoryBox_SelectedIndexChanged);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = global::Krkal.Ide.Properties.Resources.GoToParentFolderHS;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Up One Level";
			this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// toolStripSplitButton1
			// 
			this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.largeIconsToolStripMenuItem,
            this.smallIconsToolStripMenuItem,
            this.listToolStripMenuItem});
			this.toolStripSplitButton1.Image = global::Krkal.Ide.Properties.Resources.LegendHS;
			this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripSplitButton1.Name = "toolStripSplitButton1";
			this.toolStripSplitButton1.Size = new System.Drawing.Size(29, 22);
			this.toolStripSplitButton1.Text = "Views";
			// 
			// largeIconsToolStripMenuItem
			// 
			this.largeIconsToolStripMenuItem.Name = "largeIconsToolStripMenuItem";
			this.largeIconsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.largeIconsToolStripMenuItem.Text = "Large Icons";
			this.largeIconsToolStripMenuItem.Click += new System.EventHandler(this.largeIconsToolStripMenuItem_Click);
			// 
			// smallIconsToolStripMenuItem
			// 
			this.smallIconsToolStripMenuItem.Name = "smallIconsToolStripMenuItem";
			this.smallIconsToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.smallIconsToolStripMenuItem.Text = "Small Icons";
			this.smallIconsToolStripMenuItem.Click += new System.EventHandler(this.smallIconsToolStripMenuItem_Click);
			// 
			// listToolStripMenuItem
			// 
			this.listToolStripMenuItem.Name = "listToolStripMenuItem";
			this.listToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
			this.listToolStripMenuItem.Text = "List";
			this.listToolStripMenuItem.Click += new System.EventHandler(this.listToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// newFileButton
			// 
			this.newFileButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newFileButton.Image = global::Krkal.Ide.Properties.Resources.NewDocumentHS;
			this.newFileButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newFileButton.Name = "newFileButton";
			this.newFileButton.Size = new System.Drawing.Size(29, 22);
			this.newFileButton.Text = "New ...";
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newDirectoryToolStripMenuItem,
            this.newPackageToolStripMenuItem});
			this.toolStripButton2.Image = global::Krkal.Ide.Properties.Resources.NewFolderHS;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(29, 22);
			this.toolStripButton2.Text = "New Directory ...";
			// 
			// newDirectoryToolStripMenuItem
			// 
			this.newDirectoryToolStripMenuItem.Name = "newDirectoryToolStripMenuItem";
			this.newDirectoryToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
			this.newDirectoryToolStripMenuItem.Text = "New &Directory";
			this.newDirectoryToolStripMenuItem.Click += new System.EventHandler(this.newDirectoryToolStripMenuItem_Click);
			// 
			// newPackageToolStripMenuItem
			// 
			this.newPackageToolStripMenuItem.Name = "newPackageToolStripMenuItem";
			this.newPackageToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
			this.newPackageToolStripMenuItem.Text = "New &Package";
			this.newPackageToolStripMenuItem.Click += new System.EventHandler(this.newPackageToolStripMenuItem_Click);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(491, 307);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(95, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(491, 336);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(95, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// fileList
			// 
			this.fileList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.fileList.HideSelection = false;
			this.fileList.LargeImageList = this.fileIconsLarge;
			this.fileList.Location = new System.Drawing.Point(7, 28);
			this.fileList.Name = "fileList";
			this.fileList.ShowItemToolTips = true;
			this.fileList.Size = new System.Drawing.Size(579, 273);
			this.fileList.SmallImageList = this.fileIconsSmall;
			this.fileList.TabIndex = 4;
			this.fileList.UseCompatibleStateImageBehavior = false;
			this.fileList.View = System.Windows.Forms.View.List;
			this.fileList.ItemActivate += new System.EventHandler(this.fileList_ItemActivate);
			this.fileList.SelectedIndexChanged += new System.EventHandler(this.fileList_SelectedIndexChanged);
			this.fileList.DoubleClick += new System.EventHandler(this.fileList_DoubleClick);
			// 
			// fileIconsLarge
			// 
			this.fileIconsLarge.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileIconsLarge.ImageStream")));
			this.fileIconsLarge.TransparentColor = System.Drawing.Color.Transparent;
			this.fileIconsLarge.Images.SetKeyName(0, "Code_CodeFileCS.ico");
			this.fileIconsLarge.Images.SetKeyName(1, "VSProject_genericproject.ico");
			this.fileIconsLarge.Images.SetKeyName(2, "Folder.ico");
			this.fileIconsLarge.Images.SetKeyName(3, "document.ico");
			this.fileIconsLarge.Images.SetKeyName(4, "GenericMusicDoc.ico");
			this.fileIconsLarge.Images.SetKeyName(5, "sound.ico");
			this.fileIconsLarge.Images.SetKeyName(6, "idr_dll.ico");
			this.fileIconsLarge.Images.SetKeyName(7, "textdoc.ico");
			this.fileIconsLarge.Images.SetKeyName(8, "Resource_Bitmap.ico");
			this.fileIconsLarge.Images.SetKeyName(9, "user.ico");
			this.fileIconsLarge.Images.SetKeyName(10, "package.ico");
			// 
			// fileIconsSmall
			// 
			this.fileIconsSmall.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("fileIconsSmall.ImageStream")));
			this.fileIconsSmall.TransparentColor = System.Drawing.Color.Transparent;
			this.fileIconsSmall.Images.SetKeyName(0, "Code_CodeFileCS.ico");
			this.fileIconsSmall.Images.SetKeyName(1, "VSProject_genericproject.ico");
			this.fileIconsSmall.Images.SetKeyName(2, "Folder.ico");
			this.fileIconsSmall.Images.SetKeyName(3, "document.ico");
			this.fileIconsSmall.Images.SetKeyName(4, "GenericMusicDoc.ico");
			this.fileIconsSmall.Images.SetKeyName(5, "sound.ico");
			this.fileIconsSmall.Images.SetKeyName(6, "idr_dll.ico");
			this.fileIconsSmall.Images.SetKeyName(7, "textdoc.ico");
			this.fileIconsSmall.Images.SetKeyName(8, "Resource_Bitmap.ico");
			this.fileIconsSmall.Images.SetKeyName(9, "user.ico");
			this.fileIconsSmall.Images.SetKeyName(10, "package.ico");
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(86, 312);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(54, 13);
			this.label1.TabIndex = 7;
			this.label1.Text = "Selection:";
			// 
			// selectionTextBox
			// 
			this.selectionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectionTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.selectionTextBox.Location = new System.Drawing.Point(146, 309);
			this.selectionTextBox.Name = "selectionTextBox";
			this.selectionTextBox.ReadOnly = true;
			this.selectionTextBox.Size = new System.Drawing.Size(318, 20);
			this.selectionTextBox.TabIndex = 8;
			this.selectionTextBox.TabStop = false;
			// 
			// packUnpackButton
			// 
			this.packUnpackButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.packUnpackButton.Location = new System.Drawing.Point(389, 336);
			this.packUnpackButton.Name = "packUnpackButton";
			this.packUnpackButton.Size = new System.Drawing.Size(75, 23);
			this.packUnpackButton.TabIndex = 12;
			this.packUnpackButton.Text = "(Un)Pack";
			this.packUnpackButton.UseVisualStyleBackColor = true;
			this.packUnpackButton.Click += new System.EventHandler(this.packUnpackButton_Click);
			// 
			// deleteButton
			// 
			this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deleteButton.Image = global::Krkal.Ide.Properties.Resources.DeleteHS;
			this.deleteButton.Location = new System.Drawing.Point(308, 336);
			this.deleteButton.Name = "deleteButton";
			this.deleteButton.Size = new System.Drawing.Size(75, 23);
			this.deleteButton.TabIndex = 11;
			this.deleteButton.Text = "Delete";
			this.deleteButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.deleteButton.UseVisualStyleBackColor = true;
			this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
			// 
			// moveButton
			// 
			this.moveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.moveButton.Image = global::Krkal.Ide.Properties.Resources.MoveToFolderHS;
			this.moveButton.Location = new System.Drawing.Point(227, 336);
			this.moveButton.Name = "moveButton";
			this.moveButton.Size = new System.Drawing.Size(75, 23);
			this.moveButton.TabIndex = 10;
			this.moveButton.Text = "Move";
			this.moveButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.moveButton.UseVisualStyleBackColor = true;
			this.moveButton.Click += new System.EventHandler(this.moveButton_Click);
			// 
			// copyButton
			// 
			this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.copyButton.Image = global::Krkal.Ide.Properties.Resources.CopyHS;
			this.copyButton.Location = new System.Drawing.Point(146, 336);
			this.copyButton.Name = "copyButton";
			this.copyButton.Size = new System.Drawing.Size(75, 23);
			this.copyButton.TabIndex = 9;
			this.copyButton.Text = "Copy";
			this.copyButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.copyButton.UseVisualStyleBackColor = true;
			this.copyButton.Click += new System.EventHandler(this.copyButton_Click);
			// 
			// selectAllButton
			// 
			this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectAllButton.Image = global::Krkal.Ide.Properties.Resources.RadialChartHS;
			this.selectAllButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.selectAllButton.Location = new System.Drawing.Point(7, 336);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = new System.Drawing.Size(73, 23);
			this.selectAllButton.TabIndex = 6;
			this.selectAllButton.Text = "Select All";
			this.selectAllButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// selectOneButton
			// 
			this.selectOneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectOneButton.Image = global::Krkal.Ide.Properties.Resources.PushpinHS1;
			this.selectOneButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.selectOneButton.Location = new System.Drawing.Point(7, 307);
			this.selectOneButton.Name = "selectOneButton";
			this.selectOneButton.Size = new System.Drawing.Size(73, 23);
			this.selectOneButton.TabIndex = 5;
			this.selectOneButton.Text = "Select One";
			this.selectOneButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.selectOneButton.UseVisualStyleBackColor = true;
			this.selectOneButton.Click += new System.EventHandler(this.selectOneButton_Click);
			// 
			// FileBrowserDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(592, 366);
			this.Controls.Add(this.packUnpackButton);
			this.Controls.Add(this.deleteButton);
			this.Controls.Add(this.moveButton);
			this.Controls.Add(this.copyButton);
			this.Controls.Add(this.selectionTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.selectAllButton);
			this.Controls.Add(this.selectOneButton);
			this.Controls.Add(this.fileList);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.toolStrip1);
			this.HelpButton = true;
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "FileBrowserDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "FileBrowserDialog";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected System.Windows.Forms.ToolStrip toolStrip1;
		protected System.Windows.Forms.Button okButton;
		protected System.Windows.Forms.Button cancelButton;
		protected System.Windows.Forms.ListView fileList;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripComboBox directoryBox;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ImageList fileIconsLarge;
		private System.Windows.Forms.ImageList fileIconsSmall;
		private System.Windows.Forms.ToolStripDropDownButton newFileButton;
		private System.Windows.Forms.ToolStripDropDownButton toolStripButton2;
		private System.Windows.Forms.ToolStripMenuItem newDirectoryToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newPackageToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton toolStripSplitButton1;
		private System.Windows.Forms.ToolStripMenuItem largeIconsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem smallIconsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
		protected System.Windows.Forms.Button selectOneButton;
		protected System.Windows.Forms.Button selectAllButton;
		protected System.Windows.Forms.Label label1;
		protected System.Windows.Forms.TextBox selectionTextBox;
		protected System.Windows.Forms.Button copyButton;
		protected System.Windows.Forms.Button moveButton;
		protected System.Windows.Forms.Button deleteButton;
		protected System.Windows.Forms.Button packUnpackButton;
	}
}