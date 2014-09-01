using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;
using Krkal.Compiler;

namespace Krkal.RunIde
{
	public partial class ObjectBrowser : UserControl
	{
		private KerMain _kerMain;
		public KerMain KerMain {
			get { return _kerMain; }
			set { _kerMain = value; }
		}

		private KerObject[] _obj;
		public KerObject[] Obj {
			get { return _obj; }
			set {
				if (_kerMain != null) {
					if (_obj != null) {
						foreach(KerObject o in _obj)
							o.Release(_kerMain);
					}

					_obj = value;

					if (_obj != null) {
						foreach (KerObject o in _obj)
							o.Hold(_kerMain);
					}
					
					RefreshMe();
				}
			}
		}


		public ObjectBrowser() {
			InitializeComponent();
		}

		private void refreshButton_Click(object sender, EventArgs e) {
			RefreshMe();
		}



		private void RefreshMe() {
			treeView1.BeginUpdate();

			ReleaseNodes(treeView1.Nodes, true);
			if (_obj != null) {
				TreeNode node = null;
				foreach (KerObject o in _obj) {
					node = AddNode(treeView1.Nodes, "", new LanguageType(BasicType.Object), o);
					node.Expand();
				}
				if (node != null)
					treeView1.SelectedNode = node;
			}

			treeView1.EndUpdate();
		}


		private void ExpandObject(TreeNode node, KerObject obj) {
			ReleaseNodes(node.Nodes, true);
			if (obj.IsNull)
				return;
			foreach (KerOVar oVar in obj.Type.Variables) {
				AddNode(node.Nodes, oVar.Name.ShortString, oVar.LangType, obj.Read(oVar));
			}
		}

		private void ExpandArray(TreeNode node, KerArrBase arr) {
			ReleaseNodes(node.Nodes, true);
			if (arr == null)
				return;
			LanguageType lt = arr.LT;
			lt.DecreaseDimCount();
			for (int f = 0; f < arr.Count; f++) {
				AddNode(node.Nodes, f.ToString(), lt, arr.Read(f));
			}
		}



		private void ReleaseNodes(TreeNodeCollection treeNodeCollection, bool clear) {
			foreach (TreeNode node in treeNodeCollection) {
				if (node.Tag is KerArrBase) {
					KerArrBase arr = (KerArrBase)node.Tag;
					arr.Release(_kerMain);
					ReleaseNodes(node.Nodes, false);
				} else if (node.Tag is KerObject) {
					KerObject obj = (KerObject)node.Tag;
					obj.Release(_kerMain);
					ReleaseNodes(node.Nodes, false);
				} 
			}

			if (clear)
				treeNodeCollection.Clear();
		}

		private TreeNode AddNode(TreeNodeCollection nodes, string name, LanguageType languageType, Object variable) {
			TreeNode node = new TreeNode();
			String text = null;
			if (variable != null)
				text = variable.ToString();
			if (String.IsNullOrEmpty(text))
				text = "null";
			node.Text = String.Format("{0} = {1} : {2}", name, text, languageType);
			
			if (languageType.DimensionsCount > 0) {
				node.SelectedImageKey = node.ImageKey = "Array";
				KerArrBase arr = (KerArrBase)variable;
				if (arr != null && arr.Count > 0) {
					node.Nodes.Add("Expandable");
					arr.Hold(_kerMain);
					node.Tag = arr;
				}
			} else {
				node.SelectedImageKey = node.ImageKey = languageType.BasicType.ToString();
				if (languageType.BasicType == BasicType.Object) {
					KerObject obj = (KerObject)variable;
					if (!obj.IsNull && obj.Type.Variables.Count > 0) {
						node.Nodes.Add("Expandable");
						obj.Hold(_kerMain);
						node.Tag = obj;
					}
				}
			}
			nodes.Add(node);
			return node;
		}

		private void treeView1_AfterExpand(object sender, TreeViewEventArgs e) {
			if (e.Node.Tag is KerArrBase) {
				KerArrBase arr = (KerArrBase)e.Node.Tag;
				ExpandArray(e.Node, arr);
			} else if (e.Node.Tag is KerObject) {
				KerObject obj = (KerObject)e.Node.Tag;
				ExpandObject(e.Node, obj);
			} 
		}

		private void staticVariablesButton_Click(object sender, EventArgs e) {
			Obj = new KerObject[] { _kerMain.StaticData };
		}

		private void treeView1_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Delete) {
				DeleteSelectedNode();
			}
		}

		private void DeleteSelectedNode() {
			TreeNode node = treeView1.SelectedNode;
			if (node != null && node.Tag is KerObject) {
				KerObject obj = (KerObject)node.Tag;
				if (obj != null) {
					obj.Kill(_kerMain);
					if (node.Level == 0) {
						treeView1.SelectedNode = node.PrevNode;
					}
				}
			}
		}

		private void killButton_Click(object sender, EventArgs e) {
			DeleteSelectedNode();
		}


	}
}

