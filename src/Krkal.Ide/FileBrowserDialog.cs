using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;
using System.IO;
using System.Collections;
using System.Globalization;

namespace Krkal.Ide
{


	public partial class FileBrowserDialog : Form
	{

		private FS _fs;
		private FileListComparer _fileListComparer = new FileListComparer();
		private String[] _rootDirs;
		private String[] _rootDirsFullPath;
		private List<String> _comboBoxDirs = new List<string>();
		private MainForm _myForm;


		private bool _selected;

		private String _directory;
		private String _rootedDirectory;
		public String Directory {
			get { return _directory; }
			set {
				if (_directory != value) {
					if (_fs.FileExist(value) > 1) {
						_fs.GetFullPath(value, ref _directory, FSFullPathType.WindowsOriginalCase);
						LoadDirectoryBox();
						if (!_selected)
							selectionTextBox.Text = "";
						LoadFiles();
					}
				}
			}
		}

		private bool _allowDirectoryChange = true;


		// CONSTRUCTOR
		public FileBrowserDialog() {
			// only for designer
			InitializeComponent();
		}


		public FileBrowserDialog(MainForm myForm) : this(null, myForm)
		{}


		public FileBrowserDialog(String directory, MainForm myForm) {
			InitializeComponent();

			_myForm = myForm;
			_fs = FS.FileSystem;
			fileList.ListViewItemSorter = _fileListComparer;

			using (FSSearchData sd = _fs.GetRoots()) {
				_rootDirs = new String[sd.Count];
				_rootDirsFullPath = new String[sd.Count];
				for (int f = 0; f < sd.Count; f++) {
					_rootDirs[f] = sd.GetName(f);
					_fs.GetFullPath(_rootDirs[f], ref _rootDirsFullPath[f]);
				}
			}

			if (directory == null) {
				directory = "$GAMES$"; // TODO implement to the FS some root query
			}

			Directory = directory;

			ToolStripMenuItem[] newFileMenuItems = _myForm.CreateNewFileMenuItems(new EventHandler(newFileToolStripMenuItem_Click));
			newFileButton.DropDown.Items.AddRange(newFileMenuItems);

		}



		private void LoadFiles() {

			fileList.BeginUpdate();
			fileList.Items.Clear();

			using (FSSearchData sData = _fs.ReadDirectory(_directory)) {

				if (sData != null) {
					for (int f = 0; f < sData.Count; f++) {
						ListViewItem item;
						switch (sData.GetAttr(f)) {
							case 1:
								item = new FileItem(sData.GetName(f));
								break;
							case 2:
								item = new DirectoryItem(sData.GetName(f), false);
								break;
							case 3:
								item = new DirectoryItem(sData.GetName(f), true);
								break;
							default:
								throw new InvalidOperationException("internal error");
						}

						fileList.Items.Add(item);
					}
				}

			}

			fileList.Sort();

			fileList.EndUpdate();
		}


		private void LoadDirectoryBox() {
			directoryBox.BeginUpdate();
			directoryBox.Items.Clear();
			_comboBoxDirs.Clear();
			for (int f = 0; f < _rootDirs.Length; f++) {
				directoryBox.Items.Add(_rootDirs[f]);
				_comboBoxDirs.Add(_rootDirs[f]);
				if (_directory.StartsWith(_rootDirsFullPath[f], true, CultureInfo.CurrentCulture )) {
					if (_directory.Length == _rootDirsFullPath[f].Length) {
						_rootedDirectory = _rootDirs[f];
					} else if (_directory[_rootDirsFullPath[f].Length] == '\\') {						
						int pos = _rootDirsFullPath[f].Length;
						_rootedDirectory = _rootDirs[f] + _directory.Substring(pos);
						string[] arr = _directory.Substring(pos).Split('\\');
						for (int ff = 1; ff < arr.Length; ff++) {
							pos = pos + 1 + arr[ff].Length;
							directoryBox.Items.Add(new String(' ', ff * 3) + arr[ff]);
							_comboBoxDirs.Add(_directory.Substring(0,pos));
						}
					}
				}
			}
			_allowDirectoryChange = false;
			directoryBox.Text = _rootedDirectory;
			directoryBox.SelectionStart = directoryBox.Text.Length;
			_allowDirectoryChange = true;
			directoryBox.EndUpdate();
		}



		private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e) {
			fileList.View = View.LargeIcon;
		}

		private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e) {
			fileList.View = View.SmallIcon;
		}

		private void listToolStripMenuItem_Click(object sender, EventArgs e) {
			fileList.View = View.List;
		}

		private void toolStripButton1_Click(object sender, EventArgs e) {
			String newPath = Path.GetDirectoryName(Directory);
			if (newPath != null && newPath.Length > 0) {
				Directory = newPath;
			}
		}

		private void fileList_ItemActivate(object sender, EventArgs e) {
			if (fileList.SelectedItems.Count == 1) {
				DirectoryItem dir = fileList.SelectedItems[0] as DirectoryItem;
				if (dir != null) {
					Directory = Path.Combine(_directory, dir.Name);
				}
			}
		}



		public ListView.SelectedListViewItemCollection SelectedItems {
			get { return fileList.SelectedItems; }
		}

		// formated as InvariantKeyOriginalCase
		public String SelectedItemKeyName {
			get {
				if (String.IsNullOrEmpty(_directory) || fileList.SelectedItems == null || fileList.SelectedItems.Count == 0 || String.IsNullOrEmpty(fileList.SelectedItems[0].Name))
					return null;
				String ret = Path.Combine(_directory, fileList.SelectedItems[0].Name);
				if (_fs.GetFullPath(ret, ref ret, FSFullPathType.InvariantKeyOriginalCase) == 0)
					return null;
				return ret;
			}
		}

		public bool MultiSelect {
			get { return fileList.MultiSelect; }
			set { fileList.MultiSelect = value; }
		}

		bool _directorySelect;
		public bool DirectorySelect {
			get { return _directorySelect; }
			set { _directorySelect = value; }
		}

		protected virtual void okButton_Click(object sender, EventArgs e) {
			if (SelectedItems.Count > 0 || (_directorySelect && !String.IsNullOrEmpty(Directory))) {
				DialogResult = DialogResult.OK;
			} else {
				DialogResult = DialogResult.None;
			}
		}

		private void fileList_DoubleClick(object sender, EventArgs e) {
			bool ds = _directorySelect;
			_directorySelect = false;
			okButton_Click(sender, e);
			_directorySelect = ds;
		}

		private void newFileToolStripMenuItem_Click(object sender, EventArgs e) {
			using (NewFileDialog newFD = new NewFileDialog(NewFileType.File, _directory, (INewDocumentInformation)((ToolStripMenuItem)sender).Tag, _myForm)) {
				UpdateAndSelect(newFD);
			}
		}


		private void newDirectoryToolStripMenuItem_Click(object sender, EventArgs e) {
			using (NewFileDialog newFD = new NewFileDialog(NewFileType.Directory, _directory, null, _myForm)) {
				UpdateAndSelect(newFD);
			}
		}

		private void newPackageToolStripMenuItem_Click(object sender, EventArgs e) {
			using (NewFileDialog newFD = new NewFileDialog(NewFileType.Package, _directory, null, _myForm)) {
				UpdateAndSelect(newFD);
			}
		}



		private void UpdateAndSelect(NewFileDialog newFD) {
			if (newFD.ShowDialog() == DialogResult.OK) {
				LoadFiles();
				if (fileList.Items.ContainsKey(newFD.FileName)) {
					fileList.Items[newFD.FileName].Selected = true;
					fileList.Select();
				}
			}
		}


		public static readonly String[] KrkalExtensions = {
			".kc",
			".kcp",
		};

		private delegate void MyDel();
		private void directoryBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (_allowDirectoryChange) {
				this.BeginInvoke(new MyDel(delegate() { // I need to resend the event because the combobox doesnt allow to change its Text
					Directory = _comboBoxDirs[directoryBox.SelectedIndex];
					directoryBox.Text = _rootedDirectory;
					directoryBox.SelectionStart = directoryBox.Text.Length;
				}));
			}
		}


		private void fileList_SelectedIndexChanged(object sender, EventArgs e) {
			if (!_selected && fileList.SelectedItems.Count == 1) {
				FileOrDirectoryItem item = fileList.SelectedItems[0] as FileOrDirectoryItem;
				if (item != null) {
					selectionTextBox.Text = Path.Combine(_rootedDirectory, item.Name);
					selectionTextBox.ForeColor = SystemColors.GrayText;
					selectionTextBox.SelectionStart = selectionTextBox.Text.Length;
				}
			}
		}

		private void selectOneButton_Click(object sender, EventArgs e) {
			if (fileList.SelectedItems.Count == 1) {
				FileOrDirectoryItem item = fileList.SelectedItems[0] as FileOrDirectoryItem;
				if (item != null) {
					selectionTextBox.Text = Path.Combine(_rootedDirectory, item.Name);
					selectionTextBox.ForeColor = SystemColors.ControlText;
					selectionTextBox.SelectionStart = selectionTextBox.Text.Length;
					_selected = true;
				}
			}
		}

		private void selectAllButton_Click(object sender, EventArgs e) {
			selectionTextBox.Text = Path.Combine(_rootedDirectory, "*");
			selectionTextBox.ForeColor = SystemColors.ControlText;
			selectionTextBox.SelectionStart = selectionTextBox.Text.Length;
			_selected = true;
		}

		private void deleteButton_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(selectionTextBox.Text)) {
				String text = "Are you sure, you want to delete the selected file or folder? \n" + selectionTextBox.Text;
				if (MessageBox.Show(text, "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK) {
					UpdateAfterOperation(_fs.Delete(selectionTextBox.Text));
				}
			}
		}

		private void ShowIOResult(int p) {
			if (p==1)
				return;
			MessageBox.Show(p == 0 ? "IO operation failed." : "During IO operation some errors occured.", "Error message", MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}

		private void UpdateAfterOperation(int res) {
			ShowIOResult(res);
			_selected = false;
			selectionTextBox.Text = "";
			LoadFiles();
		}

		private void copyButton_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(selectionTextBox.Text) && !String.IsNullOrEmpty(_rootedDirectory)) {
				using (CopyFilesDialog cfd = new CopyFilesDialog(selectionTextBox.Text, _rootedDirectory)) {
					if (cfd.ShowDialog() == DialogResult.OK) {
						UpdateAfterOperation(cfd.Result);
					}
				}
			}
		}

		private void packUnpackButton_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(selectionTextBox.Text)) {
				String text = "Are you sure, you want to change the selected folder to archive Or change the selected archive to folder? \n" + selectionTextBox.Text;
				if (MessageBox.Show(text, "Pack or Unpack", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK) {
					UpdateAfterOperation(_fs.PackOrUnpackArchive(selectionTextBox.Text));
				}
			}
		}

		private void moveButton_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(selectionTextBox.Text) && !String.IsNullOrEmpty(_rootedDirectory)) {
				using (MoveFileDialog mfd = new MoveFileDialog(selectionTextBox.Text, _rootedDirectory)) {
					if (mfd.ShowDialog() == DialogResult.OK) {
						UpdateAfterOperation(mfd.Result);
					}
				}
			}
		}

	
	}




	public abstract class FileOrDirectoryItem : ListViewItem
	{

		private KrkalPath _krkalPath;
		public KrkalPath KrkalPath {
			get { return _krkalPath; }
		}

		public FileOrDirectoryItem(String name) {
			_krkalPath = new KrkalPath(name, FileBrowserDialog.KrkalExtensions);
			this.ToolTipText = name;
			this.Name = name;
			this.Text = _krkalPath.ShortWithExtension;
			if (_krkalPath.KnownExtension >= 0)
				this.ImageIndex = _krkalPath.KnownExtension;
		}
	}



	public class DirectoryItem : FileOrDirectoryItem
	{
		bool _isPackage;
		public bool IsPackage {
			get { return _isPackage; }
		}

		public DirectoryItem(String name, bool isPackage)
			: base(name) 
		{
			_isPackage = isPackage;
			if (this.KrkalPath.KnownExtension == -1)
				this.ImageKey = isPackage ? "package.ico" : "Folder.ico";
		}
	}



	public class FileItem : FileOrDirectoryItem
	{
		public FileItem(String name)
			: base(name) 
		{
			if (this.KrkalPath.KnownExtension == -1)
				this.ImageKey = "document.ico";
		}
	}


	internal class FileListComparer : IComparer
	{

		public int Compare(object x, object y) {
			if (x== null)
				throw new ArgumentNullException("x");
			if (y==null)
				throw new ArgumentNullException("y");
			if (x is DirectoryItem) {
				if (!(y is DirectoryItem))
					return -1;
			} else {
				if (y is DirectoryItem)
					return 1;
			}
			return StringComparer.CurrentCultureIgnoreCase.Compare(((ListViewItem)x).Text, ((ListViewItem)y).Text);
		}

	}

}