namespace Krkal.RunIde
{
	partial class FileBrowserDialogEx
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
			this.nameTree = new Krkal.Ide.DoubleClickableTreeView();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(472, 336);
			this.okButton.TabIndex = 11;
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(573, 336);
			this.cancelButton.TabIndex = 12;
			// 
			// fileList
			// 
			this.fileList.Size = new System.Drawing.Size(457, 273);
			this.fileList.TabIndex = 1;
			this.fileList.SelectedIndexChanged += new System.EventHandler(this.fileList_SelectedIndexChanged);
			// 
			// selectOneButton
			// 
			this.selectOneButton.TabIndex = 3;
			// 
			// selectAllButton
			// 
			this.selectAllButton.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.TabIndex = 5;
			// 
			// selectionTextBox
			// 
			this.selectionTextBox.TabIndex = 6;
			// 
			// copyButton
			// 
			this.copyButton.TabIndex = 7;
			// 
			// moveButton
			// 
			this.moveButton.TabIndex = 8;
			// 
			// deleteButton
			// 
			this.deleteButton.TabIndex = 9;
			// 
			// packUnpackButton
			// 
			this.packUnpackButton.TabIndex = 10;
			// 
			// nameTree
			// 
			this.nameTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.nameTree.HideSelection = false;
			this.nameTree.Location = new System.Drawing.Point(470, 28);
			this.nameTree.Name = "nameTree";
			this.nameTree.ShowNodeToolTips = true;
			this.nameTree.Size = new System.Drawing.Size(198, 301);
			this.nameTree.TabIndex = 2;
			this.nameTree.DoubleClick += new System.EventHandler(this.nameTree_DoubleClick);
			this.nameTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.nameTree_AfterSelect);
			this.nameTree.Leave += new System.EventHandler(this.nameTree_Leave);
			this.nameTree.LabelDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.nameTree_LabelDoubleClick);
			// 
			// FileBrowserDialogEx
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(674, 366);
			this.Controls.Add(this.nameTree);
			this.MinimumSize = new System.Drawing.Size(682, 400);
			this.Name = "FileBrowserDialogEx";
			this.Text = "FileBrowserDialogEx";
			this.Controls.SetChildIndex(this.nameTree, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.fileList, 0);
			this.Controls.SetChildIndex(this.selectOneButton, 0);
			this.Controls.SetChildIndex(this.selectAllButton, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.selectionTextBox, 0);
			this.Controls.SetChildIndex(this.copyButton, 0);
			this.Controls.SetChildIndex(this.moveButton, 0);
			this.Controls.SetChildIndex(this.deleteButton, 0);
			this.Controls.SetChildIndex(this.packUnpackButton, 0);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Krkal.Ide.DoubleClickableTreeView nameTree;
	}
}