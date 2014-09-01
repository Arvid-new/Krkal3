using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Krkal.RunIde
{
	public partial class Setup : Form
	{
		public Setup(KrkalApplication application) {
			InitializeComponent();
			Text = application.Caption;
			okButton.Text = application.OkText;
			notImplementedLabel.Text = application.ResourceManager.GetText("_KSID_RunIde__M_Setup__M_NotImplementedLabel", notImplementedLabel.Text);
		}

		private void okButton_Click(object sender, EventArgs e) {
			Close();
		}
	}
}
