using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Ide;
using Krkal.Compiler;

namespace Krkal.RunIde
{
	public partial class FileBrowserDialogEx : FileBrowserDialog
	{

		KsidName _selectedName;
		public KsidName SelectedName {
			get { return _selectedName; }
		}

		String _selectedUserName;
		public String SelectedUserName {
			get { return _selectedUserName; }
		}


		// CONSTRUCTOR
		public FileBrowserDialogEx(String directory, MainForm myForm) : base(directory, myForm) {
			InitializeComponent();
			nameTree.TreeViewNodeSorter = new NodeSorter();
		}

		public FileBrowserDialogEx(MainForm myForm)	: base(myForm) {
			InitializeComponent();
			nameTree.TreeViewNodeSorter = new NodeSorter();
		}



		public void AddNameList(String label, IEnumerable<KsidName> names, IEnumerable<String> userNames) {
			TreeNode root = nameTree.Nodes.Add(label);
			IEnumerator<String> enumerator = userNames == null ? null : userNames.GetEnumerator();
			foreach (KsidName name in names) {
				if (enumerator != null)
					enumerator.MoveNext();
				TreeNode node = new TreeNode(enumerator != null ? enumerator.Current : name.Identifier.ToString(true));
				node.Tag = name;
				node.ToolTipText = name.ToString();
				root.Nodes.Add(node);
			}
			root.Expand();
		}


		private void nameTree_DoubleClick(object sender, EventArgs e) {
			okButton_Click(sender, e);
		}


		protected override void okButton_Click(object sender, EventArgs e) {
			if (SelectedItems.Count > 0 || SelectedName != null) {
				DialogResult = DialogResult.OK;
			} else {
				DialogResult = DialogResult.None;
			}
		}

		private void nameTree_AfterSelect(object sender, TreeViewEventArgs e) {
			KsidName name = e.Node.Tag as KsidName;
			if (name != null) {
				_selectedName = name;
				_selectedUserName = e.Node.Text;
				fileList.SelectedItems.Clear();
			} else {
				_selectedName = null;
				_selectedUserName = null;
			}
		}

		private void fileList_SelectedIndexChanged(object sender, EventArgs e) {
			if (fileList.SelectedItems.Count > 0) {
				_selectedName = null;
				_selectedUserName = null;
				nameTree.SelectedNode = null;
			}
		}

		private void nameTree_Leave(object sender, EventArgs e) {
			if (_selectedName == null)
				nameTree.SelectedNode = null;
		}

		private void nameTree_LabelDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			okButton_Click(sender, e);
		}
	}
}
