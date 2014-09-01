using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;

namespace Krkal.RunIde
{
	public partial class ObjectBrowserForm : WeifenLuo.WinFormsUI.Docking.DockContent
	{

		public KerMain KerMain {
			get { return objectBrowser1.KerMain; }
			set { objectBrowser1.KerMain = value; }
		}

		public KerObject[] Obj {
			get { return objectBrowser1.Obj; }
			set { objectBrowser1.Obj = value; }
		}



		public ObjectBrowserForm() {
			InitializeComponent();
		}
	}
}
