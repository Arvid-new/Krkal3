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
	public partial class ClassView : WeifenLuo.WinFormsUI.Docking.DockContent
	{

		private MainForm _myForm;

		private NameFilter _filterState;
		private bool _showInheritedItems = true;

		// CONSTRUCTOR
		public ClassView(MainForm myForm) {
			InitializeComponent();
			_myForm = myForm;

			classTree.TreeViewNodeSorter = new NodeSorter();
			fieldTree.TreeViewNodeSorter = new NodeSorter(true);


			_filterState = new NameFilter(false, imageList2);
			_filterState.Change(NameType.Class, true);

			classTree.Filter = _filterState.FilterFunction;
		}




		public void UpdateSyntax(Compilation compilation) {
			classTree.Compilation = compilation;
			UpdateFieldTree();
		}

		public void ClearSyntax() {
			classTree.Compilation = null;
			UpdateFieldTree();
		}

		private void classTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			Identifier id = (Identifier)e.Node.Tag;
			KsidName ksid;
			if (classTree.Compilation != null && classTree.Compilation.KsidNames.TryGetName(id, out ksid) && ksid.DeclarationPlace != null) {
				KrkalCodeDocument doc = _myForm.OpenFile(ksid.DeclarationPlace.Lexical.File);
				if (doc != null && ksid.DeclarationPlace.PositionInLines != null) {
					doc.SelectText(ksid.DeclarationPlace.PositionInLines);
				}

			}
		}

		private void nameFilterButton_Click(object sender, EventArgs e) {
			if (_filterState.ShowDialog() == DialogResult.OK) {
				classTree.UpdateSyntax();
				UpdateFieldTree();
			}
		}

		private void treeViewButton_CheckedChanged(object sender, EventArgs e) {
			classTree.IsTree = treeViewButton.Checked;
			UpdateFieldTree();
		}


		private void classTree_AfterSelect(object sender, TreeViewEventArgs e) {
			UpdateFieldTree();
		}
		

		private void UpdateFieldTree() {
			fieldTree.BeginUpdate();
			fieldTree.Nodes.Clear();
			if (classTree.SelectedNode != null && classTree.Compilation != null) {
				Identifier id = (Identifier)classTree.SelectedNode.Tag;
				KsidName ksid;
				classTree.Compilation.KsidNames.TryGetName(id, out ksid);
				ClassName className = ksid as ClassName;
				if (className != null) {
					StringBuilder sb = new StringBuilder();
					UpdateFieldTree2(className.Methods, className, sb);
					UpdateFieldTree2(className.UniqueNames.Values, className, sb);
					UpdateFieldTree2(className.StaticFields.Values, className, sb);
				}
			}
			fieldTree.EndUpdate();
		}

		private void UpdateFieldTree2(System.Collections.IEnumerable fields, ClassName className, StringBuilder sb) {
			foreach (ClassField field in fields) {
				if (_showInheritedItems || field.InheritedFrom == className) {
					MethodField method = field as MethodField;
					StaticField staticField = field as StaticField;
					if (staticField != null || method != null || !CompilerConstants.IsNameTypeMethod(field.Name.NameType)) {
						sb.Length = 0;

						TypedKsidName typedKsid = field.Name as TypedKsidName;
						if (typedKsid != null) {
							sb.Append(typedKsid.LanguageType.ToString());
							sb.Append(' ');
						}
						int typeLength = sb.Length;

						sb.Append(field.Name.Identifier.ToString(true));

						if (method != null)
							sb.Append(method.ParameterList.ToString());

						TreeNode node = new TreeNode(sb.ToString(typeLength, sb.Length-typeLength));
						node.SelectedImageIndex = node.ImageIndex = (int)field.Name.NameType;
						if (field.Field.Name != null)
							node.Tag = field.Field.Name.SourceToken;
						node.ToolTipText = sb.ToString();
						fieldTree.Nodes.Add(node);

					}
				}
			}
		}

		

		private void showInheritedItemsButton_CheckedChanged(object sender, EventArgs e) {
			_showInheritedItems = showInheritedItemsButton.Checked;
			UpdateFieldTree();
		}

		private void changeDirectionButton_CheckedChanged(object sender, EventArgs e) {
			if (changeDirectionButton.Checked) {
				classTree.Direction = Direction.Down;
			} else {
				classTree.Direction = Direction.Up;
			}
			UpdateFieldTree();
		}

		private void fieldTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
			LexicalToken token = e.Node.Tag as LexicalToken;
			if (token != null) {
				KrkalCodeDocument doc = _myForm.OpenFile(token.Lexical.File);
				if (doc != null && token.PositionInLines != null) {
					doc.SelectText(token.PositionInLines);
				}

			}
		}


	}
}

