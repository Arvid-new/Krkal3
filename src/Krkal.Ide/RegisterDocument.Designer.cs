namespace Krkal.Ide
{
	partial class RegisterDocument
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterDocument));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.treeView = new Aga.Controls.Tree.TreeViewAdv();
			this.keyColumn = new Aga.Controls.Tree.TreeColumn();
			this.valueColumn = new Aga.Controls.Tree.TreeColumn();
			this.hexaColumn = new Aga.Controls.Tree.TreeColumn();
			this.ordinalColumn = new Aga.Controls.Tree.TreeColumn();
			this.typeColumn = new Aga.Controls.Tree.TreeColumn();
			this.nodeIcon = new Aga.Controls.Tree.NodeControls.NodeIcon();
			this.nodeKeyName = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeValueText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeHexaText = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeOrdinal = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.nodeType = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this._reloadButton = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._reloadButton});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(604, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// treeView
			// 
			this.treeView.AllowColumnReorder = true;
			this.treeView.BackColor = System.Drawing.SystemColors.Window;
			this.treeView.Columns.Add(this.keyColumn);
			this.treeView.Columns.Add(this.valueColumn);
			this.treeView.Columns.Add(this.hexaColumn);
			this.treeView.Columns.Add(this.ordinalColumn);
			this.treeView.Columns.Add(this.typeColumn);
			this.treeView.DefaultToolTipProvider = null;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView.DragDropMarkColor = System.Drawing.Color.Black;
			this.treeView.FullRowSelect = true;
			this.treeView.GridLineStyle = ((Aga.Controls.Tree.GridLineStyle)((Aga.Controls.Tree.GridLineStyle.Horizontal | Aga.Controls.Tree.GridLineStyle.Vertical)));
			this.treeView.LineColor = System.Drawing.SystemColors.ControlDark;
			this.treeView.LoadOnDemand = true;
			this.treeView.Location = new System.Drawing.Point(0, 25);
			this.treeView.Model = null;
			this.treeView.Name = "treeView";
			this.treeView.NodeControls.Add(this.nodeIcon);
			this.treeView.NodeControls.Add(this.nodeKeyName);
			this.treeView.NodeControls.Add(this.nodeValueText);
			this.treeView.NodeControls.Add(this.nodeHexaText);
			this.treeView.NodeControls.Add(this.nodeOrdinal);
			this.treeView.NodeControls.Add(this.nodeType);
			this.treeView.SelectedNode = null;
			this.treeView.ShowNodeToolTips = true;
			this.treeView.Size = new System.Drawing.Size(604, 463);
			this.treeView.TabIndex = 1;
			this.treeView.Text = "treeViewAdv1";
			this.treeView.UseColumns = true;
			// 
			// keyColumn
			// 
			this.keyColumn.Header = "Key";
			this.keyColumn.MinColumnWidth = 20;
			this.keyColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.keyColumn.TooltipText = null;
			this.keyColumn.Width = 250;
			// 
			// valueColumn
			// 
			this.valueColumn.Header = "Value";
			this.valueColumn.MinColumnWidth = 20;
			this.valueColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.valueColumn.TooltipText = null;
			this.valueColumn.Width = 200;
			// 
			// hexaColumn
			// 
			this.hexaColumn.Header = "Hexadecimal Value";
			this.hexaColumn.MinColumnWidth = 20;
			this.hexaColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.hexaColumn.TooltipText = null;
			this.hexaColumn.Width = 200;
			// 
			// ordinalColumn
			// 
			this.ordinalColumn.Header = "Ordinal";
			this.ordinalColumn.MinColumnWidth = 20;
			this.ordinalColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.ordinalColumn.TooltipText = null;
			// 
			// typeColumn
			// 
			this.typeColumn.Header = "Type, Count";
			this.typeColumn.MinColumnWidth = 20;
			this.typeColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.typeColumn.TooltipText = null;
			this.typeColumn.Width = 90;
			// 
			// nodeIcon
			// 
			this.nodeIcon.DataPropertyName = "Icon";
			this.nodeIcon.LeftMargin = 1;
			this.nodeIcon.ParentColumn = this.keyColumn;
			this.nodeIcon.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
			// 
			// nodeKeyName
			// 
			this.nodeKeyName.DataPropertyName = "KeyName";
			this.nodeKeyName.IncrementalSearchEnabled = true;
			this.nodeKeyName.LeftMargin = 3;
			this.nodeKeyName.ParentColumn = this.keyColumn;
			this.nodeKeyName.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// nodeValueText
			// 
			this.nodeValueText.DataPropertyName = "KeyValue";
			this.nodeValueText.IncrementalSearchEnabled = true;
			this.nodeValueText.LeftMargin = 3;
			this.nodeValueText.ParentColumn = this.valueColumn;
			this.nodeValueText.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// nodeHexaText
			// 
			this.nodeHexaText.DataPropertyName = "HexaValue";
			this.nodeHexaText.IncrementalSearchEnabled = true;
			this.nodeHexaText.LeftMargin = 3;
			this.nodeHexaText.ParentColumn = this.hexaColumn;
			this.nodeHexaText.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// nodeOrdinal
			// 
			this.nodeOrdinal.DataPropertyName = "Ordinal";
			this.nodeOrdinal.IncrementalSearchEnabled = true;
			this.nodeOrdinal.LeftMargin = 3;
			this.nodeOrdinal.ParentColumn = this.ordinalColumn;
			// 
			// nodeType
			// 
			this.nodeType.DataPropertyName = "TypeCount";
			this.nodeType.IncrementalSearchEnabled = true;
			this.nodeType.LeftMargin = 3;
			this.nodeType.ParentColumn = this.typeColumn;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList1.Images.SetKeyName(0, "XPfolder_closed.bmp");
			this.imageList1.Images.SetKeyName(1, "VSObject_Enum.bmp");
			this.imageList1.Images.SetKeyName(2, "VSObject_Field.bmp");
			// 
			// _reloadButton
			// 
			this._reloadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this._reloadButton.Image = global::Krkal.Ide.Properties.Resources.RefreshDocViewHS;
			this._reloadButton.ImageTransparentColor = System.Drawing.Color.Black;
			this._reloadButton.Name = "_reloadButton";
			this._reloadButton.Size = new System.Drawing.Size(23, 22);
			this._reloadButton.Text = "Reload";
			this._reloadButton.Click += new System.EventHandler(this._reloadButton_Click);
			// 
			// RegisterDocument
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.ClientSize = new System.Drawing.Size(604, 488);
			this.Controls.Add(this.treeView);
			this.Controls.Add(this.toolStrip1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "RegisterDocument";
			this.TabText = "RegisterDocument";
			this.Text = "RegisterDocument";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RegisterDocument_FormClosed);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private Aga.Controls.Tree.TreeViewAdv treeView;
		private Aga.Controls.Tree.TreeColumn keyColumn;
		private Aga.Controls.Tree.TreeColumn valueColumn;
		private Aga.Controls.Tree.TreeColumn hexaColumn;
		private Aga.Controls.Tree.NodeControls.NodeIcon nodeIcon;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeKeyName;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeValueText;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeHexaText;
		private Aga.Controls.Tree.TreeColumn ordinalColumn;
		private Aga.Controls.Tree.TreeColumn typeColumn;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeOrdinal;
		private Aga.Controls.Tree.NodeControls.NodeTextBox nodeType;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolStripButton _reloadButton;
	}
}