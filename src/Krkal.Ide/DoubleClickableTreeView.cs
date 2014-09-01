using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krkal.Ide
{
	public partial class DoubleClickableTreeView : TreeView
	{
		public DoubleClickableTreeView() {
			InitializeComponent();
		}

		public event TreeNodeMouseClickEventHandler LabelDoubleClick;

		protected override void WndProc(ref Message m) {
			if (m.Msg == 0x203 /*WM_LBUTTONDBLCLK*/) {
				Point point = new Point(m.LParam.ToInt32());
				TreeViewHitTestInfo info = this.HitTest(point);
				if ((info.Location & (TreeViewHitTestLocations.Label | TreeViewHitTestLocations.Image)) != 0) {
					TreeNodeMouseClickEventHandler temp = LabelDoubleClick;
					if (temp != null) {
						TreeNodeMouseClickEventArgs e = new TreeNodeMouseClickEventArgs(info.Node, MouseButtons.Left, 2, point.X, point.Y);
						temp(this, e);
					}
				} else {
					base.WndProc(ref m);
				}
			} else {
				base.WndProc(ref m);
			}
		}

	}
}
