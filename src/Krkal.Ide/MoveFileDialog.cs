using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Krkal.FileSystem;

namespace Krkal.Ide
{
	public partial class MoveFileDialog : Form
	{
		// CONSTRUCTOR
		public MoveFileDialog(String source, String destination) {
			InitializeComponent();
			sourceBox.Text = source;
			destinationBox.Text = destination;
			if (source.EndsWith("*")) {
				newNameBox.Enabled = false;
			} else {
				newNameBox.Text = Path.GetFileName(source);
			}
		}

		int _result;
		public int Result {
			get { return _result; }
		}

		private void okButton_Click(object sender, EventArgs e) {
			try {
				_result = FS.FileSystem.Move(sourceBox.Text, Path.Combine(destinationBox.Text, newNameBox.Text));
				DialogResult = DialogResult.OK;
			}
			catch (ArgumentException) { }
		}
	}
}
