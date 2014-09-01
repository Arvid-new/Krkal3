using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using WeifenLuo.WinFormsUI.Docking;
using DockSample;
using System.Collections.Generic;
using Krkal.FileSystem;
using Krkal.Compiler;
using System.Globalization;
using System.Text;

namespace Krkal.Ide
{
    public partial class MainForm : Form
    {
		private IIdeHelper _ideHelper;
		public IIdeHelper IdeHelper {
			get { return _ideHelper; }
			set { _ideHelper = value; }
		}

		private String _caption = "Krkal IDE";
		public String Caption {
			get { return _caption; }
		}
		
		private String _caption2 = " - Krkal IDE";
		public String Caption2 {
			get { return _caption2; }
		}

		private String _currentDirectory = "$GAMES$";
		public String CurrentDirectory {
			get { return _currentDirectory; }
			set { _currentDirectory = value; }
		}

        protected bool _bSaveLayout = true;
		private DeserializeDockContent _deserializeDockContent;
		
		private SolutionExplorer _solutionExplorer;
		public SolutionExplorer SolutionExplorer {
			get { return _solutionExplorer; }
		}
		
		private ErrorList _errorListWindow;
		public ErrorList ErrorListWindow {
			get { return _errorListWindow; }
		}

		private ClassView _classView;
		public ClassView ClassView {
			get { return _classView; }
		}

		private NameView _nameView;
		public NameView NameView {
			get { return _nameView; }
		}


		private Project _project;
		public Project Project {
			get { return _project; }
		}

		private FS _fs;
		public FS FS {
			get { return _fs; }
		}


		private bool _doBackGroundCompilation = true;
		public bool DoBackGroundCompilation {
			get { return _doBackGroundCompilation; }
			set {
				_doBackGroundCompilation = value;
				backgroundCompilationTimer.Enabled = value;
			}
		}



		Dictionary<String, KrkalDocument> _openedFiles = new Dictionary<string,KrkalDocument>(StringComparer.CurrentCultureIgnoreCase);



		// CONSTRUCTOR

		public MainForm(IIdeHelper ideHelper) {
			_ideHelper = ideHelper;
			Init();
		}

		private void Init() {
			InitializeComponent();

			_fs = FS.FileSystem;
			_project = new Project(this);

			this.Text = Caption;
			_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);

			ToolStripMenuItem[] newFileMenuItems = CreateNewFileMenuItems(new EventHandler(newFileToolStripMenuItem_Click));
			toolBarButtonNew.DropDown.Items.AddRange(newFileMenuItems);
			newFileMenuItems = CreateNewFileMenuItems(new EventHandler(newFileToolStripMenuItem_Click));
			menuItemNew.DropDown.Items.AddRange(newFileMenuItems);
		}


        public MainForm()
        {
			_ideHelper = new DefaultIdeHelper();
			Init();
        }

		private void menuItemExit_Click(object sender, System.EventArgs e)
		{
			CloseAll();
		}

		protected virtual void CloseAll() {
			Close();
		}

		private void menuItemSolutionExplorer_Click(object sender, System.EventArgs e)
		{
			_solutionExplorer.Show(dockPanel);
		}


		private void menuItemErrorListWindow_Click(object sender, System.EventArgs e)
		{
			_errorListWindow.Show(dockPanel);
		}


		private void menuItemAbout_Click(object sender, System.EventArgs e)
		{
			AboutDialog aboutDialog = new AboutDialog();
			aboutDialog.ShowDialog(this);
		}

		private void menuItemOpen_Click(object sender, System.EventArgs e)
		{

			using (FileBrowserDialog fileBrowser = new FileBrowserDialog(_currentDirectory, this)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Open Text File";
				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					OpenFile(fileBrowser.SelectedItemKeyName);
				}
				_currentDirectory = fileBrowser.Directory;
			}

			
		}


		public KrkalCodeDocument OpenFile(String file) {
			if (String.IsNullOrEmpty(file))
				return null;

			KrkalCodeDocument doc;
			KrkalDocument doc2;
			if (_openedFiles.TryGetValue(file, out doc2)) {
				doc = doc2 as KrkalCodeDocument;
				if (doc != null)
					doc.Activate();
				return doc;
			}

			doc = new KrkalCodeDocument(this);
			try {
				doc.OpenFile(file);
				_openedFiles.Add(file, doc);
				doc.Show(dockPanel);
			}
			catch (FSFileNotFoundException ex) {
				doc.Close();
				MessageBox.Show(ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				doc = null;
			}

			return doc;
		}



		private void toolStripButtonOpenRegister_Click(object sender, EventArgs e) {
			using (FileBrowserDialog fileBrowser = new FileBrowserDialog(_currentDirectory, this)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Open Register";
				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					OpenRegister(fileBrowser.SelectedItemKeyName);
				}
				_currentDirectory = fileBrowser.Directory;
			}

		}


		private RegisterDocument OpenRegister(string file) {
			if (String.IsNullOrEmpty(file))
				return null;

			RegisterDocument doc;
			KrkalDocument doc2;
			if (_openedFiles.TryGetValue(file, out doc2)) {
				doc = doc2 as RegisterDocument;
				if (doc != null)
					doc.Activate();
				return doc;
			}

			doc = new RegisterDocument(this);
			try {
				doc.OpenFile(file);
				_openedFiles.Add(file, doc);
				doc.Show(dockPanel);
			}
			catch (FSFileNotFoundException ex) {
				doc.Close();
				MessageBox.Show(ex.Message, Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
				doc = null;
			}

			return doc;
		}



		internal void ChangeDocFileName(KrkalDocument doc, string originalName, string newName) {
			KrkalDocument doc2;
			if (_openedFiles.TryGetValue(newName, out doc2)) {
				doc2.CloseWithoutSaving();
			}
			_openedFiles.Remove(originalName);
			_openedFiles.Add(newName, doc);
		}



		internal void FileClosed(String file) {
			_openedFiles.Remove(file);
		}


		public virtual void SaveAll() {
			foreach (KrkalDocument doc in _openedFiles.Values) {
				if (doc.Modified())
					doc.Save();
			}
		}


		private void menuItemFile_Popup(object sender, System.EventArgs e)
		{
			if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				menuItemClose.Enabled = menuItemCloseAll.Enabled = (ActiveMdiChild != null);
			}
			else
			{
				menuItemClose.Enabled = (dockPanel.ActiveDocument != null);
				menuItemCloseAll.Enabled = (dockPanel.DocumentsCount > 0);
			}
			menuItemSave.Enabled = menuItemClose.Enabled;
		}

		private void menuItemClose_Click(object sender, System.EventArgs e)
		{
			if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
				ActiveMdiChild.Close();
			else if (dockPanel.ActiveDocument != null)
				dockPanel.ActiveDocument.DockHandler.Close();
		}

		private void menuItemCloseAll_Click(object sender, System.EventArgs e)
		{
			CloseAllDocuments();
		}

		private void CloseAllDocuments()
		{
			if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				foreach (Form form in MdiChildren)
					form.Close();
			}
			else
			{
				List<IDockContent> tempList = new List<IDockContent>(dockPanel.Documents);
				foreach (IDockContent content in tempList) {
					if (content.DockHandler.HideOnClose) {
						content.DockHandler.Hide();
					} else {
						content.DockHandler.Close();
					}
				}
			}
		}

		protected virtual IDockContent GetContentFromPersistString(string persistString)
		{
			if (persistString == typeof(ErrorList).ToString())
				return _errorListWindow;
			else if (persistString == typeof(ClassView).ToString())
				return _classView;
			else if (persistString == typeof(NameView).ToString())
				return _nameView;
			else
			{
				string[] parsedStrings = persistString.Split(new char[] { ',' });
				if (parsedStrings.Length < 1)
					return null;

				KrkalDocument doc = null;

				if (parsedStrings[0] == typeof(KrkalCodeDocument).ToString()) {
					doc = new KrkalCodeDocument(this);
				} else if (parsedStrings[0] == typeof(RegisterDocument).ToString()) {
					doc = new RegisterDocument(this);
				} else if (parsedStrings[0] == typeof(SolutionExplorer).ToString()) {
					if (parsedStrings.Length >= 2 && parsedStrings[1] != string.Empty) {
						_project.Solution = parsedStrings[1];
					}
					if (parsedStrings.Length >= 3 && parsedStrings[2] != string.Empty) {
						compileInBackgroundToolStripMenuItem.Checked = Boolean.Parse(parsedStrings[2]);
					}
					return _solutionExplorer;
				}

				if (doc != null) {
					if (parsedStrings.Length >= 2 && parsedStrings[1] != string.Empty) {
						try {
							doc.OpenFile(parsedStrings[1]);
							_openedFiles.Add(parsedStrings[1], doc);
						}
						catch (FSFileNotFoundException) {
							doc.Close();
							return null;
						}
					}
					return doc;
				}
					
				return null;

			}
		}


		const string ConfigFile = "$PROFILE$\\Configuration\\IdeMainForm.config";


		protected virtual void MainForm_Load(object sender, System.EventArgs e)
		{
			Program.DisplayComponentWarning();

			_solutionExplorer = new SolutionExplorer(this);
			_errorListWindow = new ErrorList(this);
			_classView = new ClassView(this);
			_nameView = new NameView(this);

			try {
				using (Stream stream = FS.FileSystem.SreamReadFile(ConfigFile)) {
					dockPanel.LoadFromXml(stream, _deserializeDockContent);
				}
			}
			catch (FSFileNotFoundException) {
				CreateDefaultLayout();
			}

		}

		protected virtual void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (_bSaveLayout) {
				try {
					using (Stream stream = FS.FileSystem.StreamWriteFile(ConfigFile, 0)) {
						dockPanel.SaveAsXml(stream, Encoding.UTF8);
					}
				}
				catch (FSFileNotFoundException) {
					_ideHelper.ShowErrorMessage("Error while saving configuration.");
				}
			} else {
				FS.FileSystem.Delete(ConfigFile);
			}
		}

		private void menuItemToolBar_Click(object sender, System.EventArgs e)
		{
			toolBar.Visible = menuItemToolBar.Checked = !menuItemToolBar.Checked;
		}

		private void menuItemStatusBar_Click(object sender, System.EventArgs e)
		{
			statusBar.Visible = menuItemStatusBar.Checked = !menuItemStatusBar.Checked;
		}


		private void CreateDefaultLayout() {
			dockPanel.SuspendLayout(true);

			_errorListWindow.Show(dockPanel);
			_solutionExplorer.Show(dockPanel);

			dockPanel.ResumeLayout(true, true);
		}


		private void menuItemCloseAllButThisOne_Click(object sender, System.EventArgs e)
		{
			if (dockPanel.DocumentStyle == DocumentStyle.SystemMdi)
			{
				Form activeMdi = ActiveMdiChild;
				foreach (Form form in MdiChildren)
				{
					if (form != activeMdi)
						form.Close();
				}
			}
			else
			{
				List<IDockContent> tempList = new List<IDockContent>(dockPanel.Documents);
				foreach (IDockContent document in tempList)
				{
					if (!document.DockHandler.IsActivated)
						document.DockHandler.Close();
				}
			}
		}


        private void exitWithoutSavingLayout_Click(object sender, EventArgs e)
        {
            _bSaveLayout = false;
            CloseAll();
            _bSaveLayout = true;
		}



		private void chooseProjectToolStripMenuItem_Click(object sender, EventArgs e) {
			OpenProject();
		}

		private void compileToolStripMenuItem_Click(object sender, EventArgs e) {
			CompileProject();
		}

		public void CompileProject() {
			CompileProject(false);
		}

		public void CompileProject(bool rebuildAll) {
			if (!_project.ValidSolution)
				OpenProject();
			_project.Compile(rebuildAll);
		}

		public virtual void OpenProject() {
			using (FileBrowserDialog fileBrowser = new FileBrowserDialog(_currentDirectory, this)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Open Project";
				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					_project.Solution = fileBrowser.SelectedItemKeyName;
				}
				_currentDirectory = fileBrowser.Directory;
			}
		}

		private void toolStripMenuItem1_Click(object sender, EventArgs e) {
			TestForm test = new TestForm();
			test.ShowDialog();
		}

		private void menuItemSave_Click(object sender, EventArgs e) {
			KrkalDocument doc = dockPanel.ActiveDocument as KrkalDocument;
			if (doc != null)
				doc.Save();
		}

		private void menuItemSaveAs_Click(object sender, EventArgs e) {
			KrkalDocument doc = dockPanel.ActiveDocument as KrkalDocument;
			if (doc != null) {
				doc.SaveAs();
			}
		}


		private void menuItemSaveAll_Click(object sender, EventArgs e) {
			SaveAll();
		}

		private void classViewToolStripMenuItem_Click(object sender, EventArgs e) {
			_classView.Show(dockPanel);
		}


		public void UpdateSyntax() {
			foreach (KrkalDocument doc2 in _openedFiles.Values) {
				KrkalCodeDocument doc = doc2 as KrkalCodeDocument;
				if (doc != null) {
					doc.UpdateSyntax();
				}
			}
		}

		public void ClearSyntax() {
			foreach (KrkalDocument doc2 in _openedFiles.Values) {
				KrkalCodeDocument doc = doc2 as KrkalCodeDocument;
				if (doc != null) {
					doc.ClearSyntex();
				}
			}
		}

		private void nameViewToolStripMenuItem_Click(object sender, EventArgs e) {
			_nameView.Show(dockPanel);
		}

		private void newFileToolStripMenuItem_Click(object sender, EventArgs e) {
			using (NewFileDialog newFD = new NewFileDialog(NewFileType.File, _currentDirectory, (INewDocumentInformation)((ToolStripMenuItem)sender).Tag, this)) {
				if (newFD.ShowDialog() == DialogResult.OK) {
					OpenFile(newFD.KeyFileName);
				}
			}
		}



		private void backgroundCompilation_DoWork(object sender, DoWorkEventArgs e) {
			bool[] args = (bool[])e.Argument;
			Project.InnerCompile(args[0], args[1]);
		}

		private void backgroundCompilation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Error != null) {
				DoBackGroundCompilation = false;
				MessageBox.Show(e.Error.Message, "Exception occcured", MessageBoxButtons.OK, MessageBoxIcon.Error);
				MessageBox.Show("For safety reasons background compilation has been turned off.\nYou can change it in Configuration.", Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			} else if (Project.CompilationRequested == RunningCompilation.Requested) {
				Project.Compile(false);
			} else if (Project.CompilationRequested == RunningCompilation.RequestedAll) {
				Project.Compile(true);
			} else if (!e.Cancelled) {
				Project.UpdateSolution();
			}
		}

		private void backgroundCompilationTimer_Tick(object sender, EventArgs e) {
			Project.CompileInBackGround();
		}

		private void compileInBackgroundToolStripMenuItem_CheckStateChanged(object sender, EventArgs e) {
			DoBackGroundCompilation = compileInBackgroundToolStripMenuItem.Checked;
		}

		private void rebuildAllToolStripMenuItem_Click(object sender, EventArgs e) {
			CompileProject(true);
		}


		public string StatusText {
			get { return statusText.Text; }
			set { statusText.Text = value; }
		}




		public ToolStripMenuItem[] CreateNewFileMenuItems(EventHandler handler) {
			IList<INewDocumentInformation> info = _ideHelper.GetNewDocumentInformation();
			ToolStripMenuItem[] ret = new ToolStripMenuItem[info.Count];
			for (int f = 0; f < info.Count; f++) {
				ret[f] = new ToolStripMenuItem();
				ret[f].Name = "newSourceFileToolStripMenuItem" + f.ToString(CultureInfo.InvariantCulture);
				ret[f].Tag = info[f];
				//this.newSourceFileToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
				ret[f].Text = info[f].MenuName;
				ret[f].Click += handler;
			}
			return ret;
		}


	}
}