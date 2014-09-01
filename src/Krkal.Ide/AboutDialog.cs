using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krkal.Ide
{
	public partial class AboutDialog : Form
	{
		public AboutDialog() {
			InitializeComponent();
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			try {
				System.Diagnostics.Process.Start(@"http://www.krkal.org");
			}
			catch (System.ComponentModel.Win32Exception) { }
		}
	}
}