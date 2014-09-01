namespace Krkal.Ide
{
	partial class ClassView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClassView));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.treeViewButton = new System.Windows.Forms.ToolStripButton();
			this.changeDirectionButton = new System.Windows.Forms.ToolStripButton();
			this.nameFilterButton = new System.Windows.Forms.ToolStripButton();
			this.showInheritedItemsButton = new System.Windows.Forms.ToolStripButton();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.classTree = new Krkal.Ide.NameTreeControl();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.fieldTree = new Krkal.Ide.DoubleClickableTreeView();
			this.toolStrip1.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.treeViewButton,
            this.changeDirectionButton,
            this.nameFilterButton,
            this.showInheritedItemsButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(219, 25);
			this.toolStrip1.TabIndex = 1;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// treeViewButton
			// 
			this.treeViewButton.CheckOnClick = true;
			this.treeViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.treeViewButton.Image = global::Krkal.Ide.Properties.Resources.Control_TreeView;
			this.treeViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.treeViewButton.Name = "treeViewButton";
			this.treeViewButton.Size = new System.Drawing.Size(23, 22);
			this.treeViewButton.Text = "Tree View";
			this.treeViewButton.CheckedChanged += new System.EventHandler(this.treeViewButton_CheckedChanged);
			// 
			// changeDirectionButton
			// 
			this.changeDirectionButton.CheckOnClick = true;
			this.changeDirectionButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.changeDirectionButton.Image = global::Krkal.Ide.Properties.Resources.BuilderDialog_RemoveAll;
			this.changeDirectionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.changeDirectionButton.Name = "changeDirectionButton";
			this.changeDirectionButton.Size = new System.Drawing.Size(23, 22);
			this.changeDirectionButton.Text = "Change Direction";
			this.changeDirectionButton.CheckedChanged += new System.EventHandler(this.changeDirectionButton_CheckedChanged);
			// 
			// nameFilterButton
			// 
			this.nameFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.nameFilterButton.Image = global::Krkal.Ide.Properties.Resources.Filter2HS;
			this.nameFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.nameFilterButton.Name = "nameFilterButton";
			this.nameFilterButton.Size = new System.Drawing.Size(23, 22);
			this.nameFilterButton.Text = "Name Filter";
			this.nameFilterButton.Click += new System.EventHandler(this.nameFilterButton_Click);
			// 
			// showInheritedItemsButton
			// 
			this.showInheritedItemsButton.Checked = true;
			this.showInheritedItemsButton.CheckOnClick = true;
			this.showInheritedItemsButton.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showInheritedItemsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.showInheritedItemsButton.Image = global::Krkal.Ide.Properties.Resources.VSObject_Module;
			this.showInheritedItemsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.showInheritedItemsButton.Name = "showInheritedItemsButton";
			this.showInheritedItemsButton.Size = new System.Drawing.Size(23, 22);
			this.showInheritedItemsButton.Text = "Show Inherited Items";
			this.showInheritedItemsButton.CheckedChanged += new System.EventHandler(this.showInheritedItemsButton_CheckedChanged);
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
			this.splitContainer1.Panel1.Controls.Add(this.classTree);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.fieldTree);
			this.splitContainer1.Size = new System.Drawing.Size(219, 465);
			this.splitContainer1.SplitterDistance = 242;
			this.splitContainer1.TabIndex = 2;
			// 
			// classTree
			// 
			this.classTree.Compilation = null;
			this.classTree.Direction = Krkal.Compiler.Direction.Up;
			this.classTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.classTree.Filter = null;
			this.classTree.ImageIndex = 0;
			this.classTree.ImageList = this.imageList2;
			this.classTree.IsTree = false;
			this.classTree.Location = new System.Drawing.Point(0, 0);
			this.classTree.Name = "classTree";
			this.classTree.SelectedImageIndex = 0;
			this.classTree.ShowNodeToolTips = true;
			this.classTree.Size = new System.Drawing.Size(219, 242);
			this.classTree.StartIdentifier = null;
			this.classTree.TabIndex = 0;
			this.classTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.classTree_AfterSelect);
			this.classTree.LabelDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.classTree_NodeMouseDoubleClick);
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
			// fieldTree
			// 
			this.fieldTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fieldTree.ImageIndex = 0;
			this.fieldTree.ImageList = this.imageList2;
			this.fieldTree.Location = new System.Drawing.Point(0, 0);
			this.fieldTree.Name = "fieldTree";
			this.fieldTree.SelectedImageIndex = 0;
			this.fieldTree.ShowNodeToolTips = true;
			this.fieldTree.Size = new System.Drawing.Size(219, 219);
			this.fieldTree.TabIndex = 0;
			this.fieldTree.LabelDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.fieldTree_NodeMouseDoubleClick);
			// 
			// ClassView
			// 
			this.ClientSize = new System.Drawing.Size(219, 490);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.toolStrip1);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "ClassView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft;
			this.TabText = "Class View";
			this.Text = "Class View";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private DoubleClickableTreeView fieldTree;
		private System.Windows.Forms.ToolStripButton treeViewButton;
		private NameTreeControl classTree;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.ToolStripButton nameFilterButton;
		private System.Windows.Forms.ToolStripButton showInheritedItemsButton;
		private System.Windows.Forms.ToolStripButton changeDirectionButton;
	}
}
