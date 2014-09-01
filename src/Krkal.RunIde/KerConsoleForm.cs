using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krkal.RunIde
{
	public partial class KerConsoleForm : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		private KerConsole kerConsole1;


		public KerConsoleForm() {
			InitializeComponent();
			this.kerConsole1 = new Krkal.RunIde.KerConsole();
			InitKerConsole();
		}
		public KerConsoleForm(KerConsole console) {
			if (console == null)
				throw new ArgumentNullException("console");
			InitializeComponent();
			this.kerConsole1 = console;
			InitKerConsole();
		}


		private void InitKerConsole() {
			this.kerConsole1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kerConsole1.Name = "kerConsole1";
			this.Controls.Add(this.kerConsole1);
			kerConsole1.LoadErrorTexts();
		}

		public void WriteLine(int time, int errorNum, int errorParam, string message) {
			kerConsole1.WriteLine(time, errorNum, errorParam, message);
		}

	}
}
