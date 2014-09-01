using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;
using System.IO;
using System.Globalization;

namespace Krkal.Ide
{
	public partial class NewFileDialog : Form
	{

		private NewFileType _newFileType;
		private FS _fs;
		private MainForm _myForm;
		private INewDocumentInformation _information;

		// CONSTRUCTOR

		public NewFileDialog(NewFileType newFileType, String directory, INewDocumentInformation information, MainForm myForm) {
			InitializeComponent();

			_myForm = myForm;
			_information = information;
			_newFileType = newFileType;
			_fs = FS.FileSystem;
			if (_information == null) {
				Text = Captions[(int)_newFileType];
			} else {
				Text = _information.Name;
			}
			locationTextBox.Text = directory;
			nameTextBox.Select();
		}

		public String FileName {
			get { return nameTextBox.Text; }
			set { nameTextBox.Text = value; }
		}

		String _keyFileName; // Filled after the file was created. formated as InvariantKeyOriginalCase
		public String KeyFileName {
			get { return _keyFileName; }
		}

		public String FileLocation {
			get { return locationTextBox.Text; }
			set { locationTextBox.Text = value; }
		}

		private static String[] Captions = {
			"New Directory",
			"New Package",
			"New File",
			"Save As",
		};

		private String _extension;
		public String Extension {
			get { return _extension; }
			set { _extension = value; }
		}

		private String _saveAsContent;
		public String SaveAsContent {
			get { return _saveAsContent; }
			set { _saveAsContent = value; }
		}




		private void okButton_Click(object sender, EventArgs e) {
			if (FileName.Length == 0)
				return;
			if (_extension == null) {
				if (_information != null)
					_extension = _information.Extension;
				if (_extension == null)
					_extension = String.Empty;
			}
			FileName = KrkalPath.CompleteName(FileName, _extension);
			String fullName = Path.Combine(FileLocation, FileName);

			try {
				switch (_newFileType) {
					case NewFileType.Directory:
						_fs.CreateDir(fullName);
						break;
					case NewFileType.Package:
						_fs.CreateArchive(fullName);
						break;
					case NewFileType.File: 
						{
							String content = null;
							if (_information != null) {
								try {
									content = String.Format(_information.Content, KrkalPath.GenerateVersion());
								}
								catch (FormatException) { }
							}
							if (!CreateFile(fullName, content))
								return;
							break;
						}
					case NewFileType.SaveAs:
						if (!CreateFile(fullName, _saveAsContent))
							return;
						break;
				}

				if (_fs.GetFullPath(fullName, ref _keyFileName, FSFullPathType.InvariantKeyOriginalCase) == 0)
					throw new FSFileNotFoundException("IO Error");

				DialogResult = DialogResult.OK;

			}
			catch (FSFileNotFoundException ex) {
				MessageBox.Show(ex.Message, _myForm.Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private bool CreateFile(string fullName, string content) {
			if (_fs.FileExist(fullName) > 0) {
				if (MessageBox.Show("A file of this name already exists. Do you want to overwrite it?\n" + fullName, _myForm.Caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
					return false;
			}
			if (content == null)
				content = String.Empty;
			using (TextWriter writer = _fs.OpenFileForWriting(fullName)) {
				writer.Write(content);
			}
			return true;
		}





		private void browseButton_Click(object sender, EventArgs e) {
			using (FileBrowserDialog fileBrowser = new FileBrowserDialog(FileLocation, _myForm)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.DirectorySelect = true;
				fileBrowser.Text = "Select Folder";
				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					if (!String.IsNullOrEmpty(fileBrowser.Directory))
						FileLocation = fileBrowser.Directory;
				}
			}
		}

	}



	public enum NewFileType
	{
		Directory,
		Package,
		File,
		SaveAs,
	}


}