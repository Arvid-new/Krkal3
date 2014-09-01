using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;

namespace Krkal.Ide
{
	public partial class CopyFilesDialog : Form
	{
		//CONSTRUCTOR
		public CopyFilesDialog(String source, String destination) {
			InitializeComponent();
			sourceBox.Text = source;
			destinationBox.Text = destination;
			extensionBox.Enabled = false;
			radioButton1.Checked = true;
		}

		int _result;
		public int Result {
			get { return _result; }
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e) {
			if (radioButton3.Checked || radioButton4.Checked || radioButton5.Checked) {
				extensionBox.Enabled = true;
			} else {
				extensionBox.Enabled = false;
			}
		}

		private void okButton_Click(object sender, EventArgs e) {
			if (extensionBox.Enabled && String.IsNullOrEmpty(extensionBox.Text))
				return;
			FS fs = FS.FileSystem;
			FSCopyTreeChangeMode mode = FSCopyTreeChangeMode.None;
			if (radioButton2.Checked) {
				mode = FSCopyTreeChangeMode.ChangeVersion;
			} else if (radioButton3.Checked) {
				mode = FSCopyTreeChangeMode.AddExtension;
			} else if (radioButton4.Checked) {
				mode = FSCopyTreeChangeMode.RemoveExtension;
			} else if (radioButton5.Checked) {
				mode = FSCopyTreeChangeMode.ChangeExtension;
			} 

			_result = fs.CopyTree(sourceBox.Text, destinationBox.Text, mode, extensionBox.Text, -1);

			DialogResult = DialogResult.OK;
		}


	}
}
