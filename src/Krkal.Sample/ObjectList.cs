using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;

namespace Krkal.Sample
{
	public partial class ObjectList : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		GameMainForm _myForm;

		public ObjectList(GameMainForm myForm) {
			_myForm = myForm;
			InitializeComponent();
		}

		public void InitializeNames() {
			IList<KerName> arr = _myForm.KerMain.GetNames(_myForm.KerMain.FindName("_KSID_Placeable"));
			treeView1.BeginUpdate();
			foreach (KerName name in arr) {
				TreeNode node = new TreeNode(name.ShortString);
				node.Tag = name;
				treeView1.Nodes.Add(node);
			}
			treeView1.Sort();
			treeView1.EndUpdate();
		}

		public KerName SelectedName {
			get {
				if (treeView1.SelectedNode != null) {
					return (KerName)treeView1.SelectedNode.Tag;
				} else {
					return new KerName();
				}
			}
		}
	}
}

