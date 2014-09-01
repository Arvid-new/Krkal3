using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;
using System.IO;
using System.Windows.Forms;

namespace Krkal.Ide
{
	public class KrkalDocument : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		protected FS _fs;
		protected String _file;
		protected MainForm _myForm;
		

		// CONSTRUCTOR


		public KrkalDocument() { // Do not use. Only for the EVIL designer
		}

		public KrkalDocument(MainForm myForm) {
			_myForm = myForm;
			_fs = FS.FileSystem;
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KrkalDocument_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KrkalDocument_FormClosing);
		}



		public virtual void OpenFile(String file) {
			this.TabText = new KrkalPath(file, FileBrowserDialog.KrkalExtensions).ShortWithExtension;
			this.ToolTipText = file;
			_file = file;
		}

		public virtual void Save() {
		}


		public virtual void SaveAs() {
		}


		public virtual bool Modified() {
			return false;
		}


		private bool _dontSaveOnClose;

		private void KrkalDocument_FormClosing(object sender, FormClosingEventArgs e) {
			if (!_dontSaveOnClose && Modified() && _file != null) {
				DialogResult res = MessageBox.Show("Save changes in " + _file + "?", _myForm.Caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (res == DialogResult.Cancel) {
					e.Cancel = true;
				} else if (res == DialogResult.Yes) {
					Save();
				}
			}
		}


		private void KrkalDocument_FormClosed(object sender, FormClosedEventArgs e) {
			if (_file != null) {
				_myForm.FileClosed(_file);
			}
		}


		public void CloseWithoutSaving() {
			_dontSaveOnClose = true;
			Close();
		}


		protected override string GetPersistString() {
			String filename = _file == null ? "" : _file;
			return GetType().ToString() + "," + filename;
		}



	}


}

