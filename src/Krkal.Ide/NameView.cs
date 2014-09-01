using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;

namespace Krkal.Ide
{
	public partial class NameView : WeifenLuo.WinFormsUI.Docking.DockContent
	{

		private MainForm _myForm;

		private NameFilter _filterState;
		private NameTreeControl _lastSelectedControl;
		
		// CONSTRUCTOR
		public NameView(MainForm myForm) {
			InitializeComponent();
			_myForm = myForm;
			nameTree2.TreeViewNodeSorter = nameTree1.TreeViewNodeSorter = new NodeSorter();

			_filterState = new NameFilter(true, imageList2);
			nameTree1.Filter = _filterState.FilterFunction;
			nameTree2.Filter = _filterState.FilterFunction;
		}



		public void UpdateSyntax(Compilation compilation) {
			nameTree1.Compilation = compilation;
			nameTree2.Compilation = compilation;
		}

		public void ClearSyntax() {
			nameTree1.Compilation = null;
			nameTree2.Compilation = null;
		}


		private void NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			Identifier id = (Identifier)e.Node.Tag;
			KsidName ksid;
			if (nameTree1.Compilation != null && nameTree1.Compilation.KsidNames.TryGetName(id, out ksid) && ksid.DeclarationPlace != null) {
				KrkalCodeDocument doc = _myForm.OpenFile(ksid.DeclarationPlace.Lexical.File);
				if (doc != null && ksid.DeclarationPlace.PositionInLines != null) {
					doc.SelectText(ksid.DeclarationPlace.PositionInLines);
				}

			}
		}

		private void toolStripButton1_Click(object sender, EventArgs e) {
			if (_filterState.ShowDialog() == DialogResult.OK) {
				nameTree1.UpdateSyntax();
				nameTree2.UpdateSyntax();
			}
		}



		private void treeViewButton_CheckedChanged(object sender, EventArgs e) {
			nameTree1.IsTree = treeViewButton.Checked;
			nameTree2.IsTree = treeViewButton.Checked;
		}

		private void nameTree1_Enter(object sender, EventArgs e) {
			_lastSelectedControl = sender as NameTreeControl;
		}

		private void selectedNameBox_Click(object sender, EventArgs e) {
			if (_lastSelectedControl != null && _lastSelectedControl.SelectedNode != null) {
				Identifier id = (Identifier)_lastSelectedControl.SelectedNode.Tag;
				selectedNameBox.Text = id.ToString(true);
				nameTree1.StartIdentifier = id;
				nameTree2.StartIdentifier = id;
			}
		}

		private void clearSelectedNameButton_Click(object sender, EventArgs e) {
			selectedNameBox.Text = "";
			nameTree1.StartIdentifier = null;
			nameTree2.StartIdentifier = null;
		}

	}


	// Create a node sorter that implements the IComparer interface.
	public class NodeSorter : System.Collections.IComparer
	{


		private bool _byCategiries;
		public bool ByCategiries {
			get { return _byCategiries; }
			set { _byCategiries = value; }
		}

		public NodeSorter() : this(false) { }

		public NodeSorter(bool byCategories) {
			_byCategiries = byCategories;
		}

		// Compare the length of the strings, or the strings
		// themselves, if they are the same length.
		public int Compare(object x, object y) {
			TreeNode tx = x as TreeNode;
			TreeNode ty = y as TreeNode;

			if (_byCategiries && tx.ImageIndex != ty.ImageIndex) {
				return tx.ImageIndex - ty.ImageIndex;
			}

			// If they are the same length, call Compare.
			return string.Compare(tx.Text, ty.Text);
		}
	}

}

