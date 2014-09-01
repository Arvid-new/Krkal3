namespace Krkal.Ide
{
	partial class ErrorList
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorList));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.errorLabel = new System.Windows.Forms.ToolStripLabel();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.warningLabel = new System.Windows.Forms.ToolStripLabel();
			this.listView1 = new System.Windows.Forms.ListView();
			this.iconColumn = new System.Windows.Forms.ColumnHeader();
			this.numberColumn = new System.Windows.Forms.ColumnHeader();
			this.descriptionColumn = new System.Windows.Forms.ColumnHeader();
			this.fileColumn = new System.Windows.Forms.ColumnHeader();
			this.lineColumn = new System.Windows.Forms.ColumnHeader();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.errorLabel,
            this.toolStripSeparator1,
            this.warningLabel});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(878, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// errorLabel
			// 
			this.errorLabel.Image = global::Krkal.Ide.Properties.Resources.CriticalError;
			this.errorLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.errorLabel.Margin = new System.Windows.Forms.Padding(3, 1, 0, 2);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = new System.Drawing.Size(61, 22);
			this.errorLabel.Text = "0 Errors";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// warningLabel
			// 
			this.warningLabel.Image = global::Krkal.Ide.Properties.Resources.warning;
			this.warningLabel.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.warningLabel.Margin = new System.Windows.Forms.Padding(3, 1, 0, 2);
			this.warningLabel.Name = "warningLabel";
			this.warningLabel.Size = new System.Drawing.Size(77, 22);
			this.warningLabel.Text = "0 Warnings";
			// 
			// listView1
			// 
			this.listView1.AllowColumnReorder = true;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.iconColumn,
            this.numberColumn,
            this.descriptionColumn,
            this.fileColumn,
            this.lineColumn});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.Location = new System.Drawing.Point(0, 25);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(878, 245);
			this.listView1.SmallImageList = this.imageList1;
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			// 
			// iconColumn
			// 
			this.iconColumn.Text = "";
			this.iconColumn.Width = 32;
			// 
			// numberColumn
			// 
			this.numberColumn.Text = "Num";
			this.numberColumn.Width = 46;
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Text = "Description";
			this.descriptionColumn.Width = 625;
			// 
			// fileColumn
			// 
			this.fileColumn.Text = "File";
			this.fileColumn.Width = 104;
			// 
			// lineColumn
			// 
			this.lineColumn.Text = "Line";
			this.lineColumn.Width = 45;
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			this.imageList1.Images.SetKeyName(0, "document.bmp");
			this.imageList1.Images.SetKeyName(1, "warning.bmp");
			this.imageList1.Images.SetKeyName(2, "Error.bmp");
			this.imageList1.Images.SetKeyName(3, "CriticalError.bmp");
			this.imageList1.Images.SetKeyName(4, "CriticalError.bmp");
			// 
			// ErrorList
			// 
			this.ClientSize = new System.Drawing.Size(878, 270);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.toolStrip1);
			this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop)
						| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
			this.HideOnClose = true;
			this.Name = "ErrorList";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom;
			this.TabText = "Error List";
			this.Text = "Error List";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripLabel errorLabel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel warningLabel;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader iconColumn;
		private System.Windows.Forms.ColumnHeader numberColumn;
		private System.Windows.Forms.ColumnHeader descriptionColumn;
		private System.Windows.Forms.ColumnHeader fileColumn;
		private System.Windows.Forms.ColumnHeader lineColumn;
		private System.Windows.Forms.ImageList imageList1;
	}
}
