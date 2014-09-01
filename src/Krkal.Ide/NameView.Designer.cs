namespace Krkal.Ide
{
	partial class NameView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NameView));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.treeViewButton = new System.Windows.Forms.ToolStripButton();
			this.nameFilterButton = new System.Windows.Forms.ToolStripButton();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.nameTree1 = new Krkal.Ide.NameTreeControl();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.nameTree2 = new Krkal.Ide.NameTreeControl();
			this.toolStrip2 = new System.Windows.Forms.ToolStrip();
			this.selectedNameBox = new System.Windows.Forms.ToolStripTextBox();
			this.clearSelectedNameButton = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.toolStrip2.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.treeViewButton,
            this.nameFilterButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(215, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// treeViewButton
			// 
			this.treeViewButton.Checked = true;
			this.treeViewButton.CheckOnClick = true;
			this.treeViewButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.treeViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.treeViewButton.Image = global::Krkal.Ide.Properties.Resources.Control_TreeView;
			this.treeViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.treeViewButton.Name = "treeViewButton";
			this.treeViewButton.Size = new System.Drawing.Size(23, 22);
			this.treeViewButton.Text = "Tree View";
			this.treeViewButton.CheckedChanged += new System.EventHandler(this.treeViewButton_CheckedChanged);
			// 
			// nameFilterButton
			// 
			this.nameFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.nameFilterButton.Image = global::Krkal.Ide.Properties.Resources.Filter2HS;
			this.nameFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.nameFilterButton.Name = "nameFilterButton";
			this.nameFilterButton.Size = new System.Drawing.Size(23, 22);
			this.nameFilterButton.Text = "Name Filter";
			this.nameFilterButton.Click += new System.EventHandler(this.toolStripButton1_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 25);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.nameTree1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.nameTree2);
			this.splitContainer1.Panel2.Controls.Add(this.toolStrip2);
			this.splitContainer1.Size = new System.Drawing.Size(215, 417);
			this.splitContainer1.SplitterDistance = 213;
			this.splitContainer1.TabIndex = 1;
			// 
			// nameTree1
			// 
			this.nameTree1.Compilation = null;
			this.nameTree1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nameTree1.Filter = null;
			this.nameTree1.ImageIndex = 0;
			this.nameTree1.ImageList = this.imageList2;
			this.nameTree1.Location = new System.Drawing.Point(0, 0);
			this.nameTree1.Name = "nameTree1";
			this.nameTree1.SelectedImageIndex = 0;
			this.nameTree1.ShowNodeToolTips = true;
			this.nameTree1.Size = new System.Drawing.Size(215, 213);
			this.nameTree1.StartIdentifier = null;
			this.nameTree1.TabIndex = 1;
			this.nameTree1.Enter += new System.EventHandler(this.nameTree1_Enter);
			this.nameTree1.LabelDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.NodeMouseDoubleClick);
			// 
			// imageList2
			// 
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Fuchsia;
			this.imageList2.Images.SetKeyName(0, "VSObject_Field.bmp");
			this.imageList2.Images.SetKeyName(1, "VSObject_Field_Friend.bmp");
			this.imageList2.Images.SetKeyName(2, "VSObject_MethodOverload.bmp");
			this.imageList2.Images.SetKeyName(3, "VSObject_Method.bmp");
			this.imageList2.Images.SetKeyName(4, "VSObject_MethodOverload_Friend.bmp");
			this.imageList2.Images.SetKeyName(5, "VSObject_Method_Friend.bmp");
			this.imageList2.Images.SetKeyName(6, "VSObject_Constant.bmp");
			this.imageList2.Images.SetKeyName(7, "VSObject_Class.bmp");
			this.imageList2.Images.SetKeyName(8, "VSObject_Map.bmp");
			this.imageList2.Images.SetKeyName(9, "VSObject_Namespace.bmp");
			this.imageList2.Images.SetKeyName(10, "Control_Button.bmp");
			// 
			// nameTree2
			// 
			this.nameTree2.Compilation = null;
			this.nameTree2.Direction = Krkal.Compiler.Direction.Up;
			this.nameTree2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.nameTree2.Filter = null;
			this.nameTree2.ImageIndex = 0;
			this.nameTree2.ImageList = this.imageList2;
			this.nameTree2.Location = new System.Drawing.Point(0, 25);
			this.nameTree2.Name = "nameTree2";
			this.nameTree2.SelectedImageIndex = 0;
			this.nameTree2.ShowNodeToolTips = true;
			this.nameTree2.Size = new System.Drawing.Size(215, 175);
			this.nameTree2.StartIdentifier = null;
			this.nameTree2.TabIndex = 2;
			this.nameTree2.Enter += new System.EventHandler(this.nameTree1_Enter);
			this.nameTree2.LabelDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.NodeMouseDoubleClick);
			// 
			// toolStrip2
			// 
			this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedNameBox,
            this.clearSelectedNameButton});
			this.toolStrip2.Location = new System.Drawing.Point(0, 0);
			this.toolStrip2.Name = "toolStrip2";
			this.toolStrip2.Size = new System.Drawing.Size(215, 25);
			this.toolStrip2.TabIndex = 0;
			this.toolStrip2.Text = "toolStrip2";
			// 
			// selectedNameBox
			// 
			this.selectedNameBox.BackColor = System.Drawing.SystemColors.Window;
			this.selectedNameBox.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.selectedNameBox.Name = "selectedNameBox";
			this.selectedNameBox.ReadOnly = true;
			this.selectedNameBox.Size = new System.Drawing.Size(120, 25);
			this.selectedNameBox.ToolTipText = "To see children and parents of a specific name, select the name and click here.";
			this.selectedNameBox.Click += new System.EventHandler(this.selectedNameBox_Click);
			// 
			// clearSelectedNameButton
			// 
			this.clearSelectedNameButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.clearSelectedNameButton.Image = global::Krkal.Ide.Properties.Resources.DeleteHS;
			this.clearSelectedNameButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.clearSelectedNameButton.Name = "clearSelectedNameButton";
			this.clearSelectedNameButton.Size = new System.Drawing.Size(23, 22);
			this.clearSelectedNameButton.Text = "Back to noraml view";
			this.clearSelectedNameButton.Click += new System.EventHandler(this.clearSelectedNameButton_Click);
			// 
			// NameView
			// 
			this.ClientSize = new System.Drawing.Size(215, 442);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStrip1);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "NameView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
			this.TabText = "Name View";
			this.Text = "Name View";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.toolStrip2.ResumeLayout(false);
			this.toolStrip2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStrip toolStrip2;
		private System.Windows.Forms.ToolStripTextBox selectedNameBox;
		private System.Windows.Forms.ToolStripButton clearSelectedNameButton;
		private System.Windows.Forms.ToolStripButton treeViewButton;
		private NameTreeControl nameTree1;
		private NameTreeControl nameTree2;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.ToolStripButton nameFilterButton;
	}
}
