using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.IO;
using Krkal.Runtime;
using Krkal.FileSystem;
using Krkal.RunIde;


namespace Krkal.RunIde
{
	public partial class GameRunForm : EngineForm
	{

		private bool _bSaveLayout = true;
		private DeserializeDockContent _deserializeDockContent;

		IRuntimeStarter _runtimeStarter;

		KerConsoleForm _console;
		ObjectBrowserForm _objectBrowser;
		public ObjectBrowserForm ObjectBrowser {
			get { return _objectBrowser; }
		}

		public KerMain KerMain {
			get { return _runtimeStarter.KerMain; }
		}


		bool _terminated;
		public bool Terminated {
			get { return _terminated; }
		}



		public GameRunForm(IRuntimeStarter runtimeStarter) {
			if (runtimeStarter == null)
				throw new ArgumentNullException("runtimeStarter");
			_runtimeStarter = runtimeStarter;
			InitializeComponent();
			_console = new KerConsoleForm(_runtimeStarter.TekeOverKerKonsole());
			_objectBrowser = new ObjectBrowserForm();
			_objectBrowser.KerMain = _runtimeStarter.KerMain;
			_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
		}


		private IDockContent GetContentFromPersistString(string persistString) {

			if (persistString == typeof(KerConsoleForm).ToString()) {
				return _console;
			} else if (persistString == typeof(ObjectBrowserForm).ToString()) {
				return _objectBrowser;
			} else {
				return null;
			}
		}



		const string ConfigFile = "$PROFILE$\\Configuration\\DefaultGame.config";

		private void GameMainForm_Load(object sender, EventArgs e) {

			try {
				using (Stream stream = FS.FileSystem.SreamReadFile(ConfigFile)) {
					dockPanel.LoadFromXml(stream, _deserializeDockContent);
				}
			}
			catch (FSFileNotFoundException) {
				CreateDefaultLayout();
			}

		}


		private void GameMainForm_FormClosing(object sender, FormClosingEventArgs e) {
			timer1.Enabled = false;
			_runtimeStarter.Dispose();

			if (_bSaveLayout) {
				try {
					using (Stream stream = FS.FileSystem.StreamWriteFile(ConfigFile, 0)) {
						dockPanel.SaveAsXml(stream, Encoding.UTF8);
					}
				}
				catch (FSFileNotFoundException) {
					_runtimeStarter.Application.ShowErrorMessage("Error while saving configuration.");
				}
			} else {
				FS.FileSystem.Delete(ConfigFile);
			}

		}



		private void CreateDefaultLayout() {
			dockPanel.SuspendLayout(true);

			_objectBrowser.Show(dockPanel);
			_console.Show(dockPanel);

			dockPanel.ResumeLayout(true, true);
		}






		private void consoleToolStripMenuItem_Click(object sender, EventArgs e) {
			_console.Show(dockPanel);
		}

		private void pouseButton_Click(object sender, EventArgs e) {
			if (!_terminated)
				timer1.Enabled = !pauseButton.Checked;
		}

		private void terminateButton_Click(object sender, EventArgs e) {
			KerMain.TerminateKernel();
		}

		private void timer1_Tick(object sender, EventArgs e) {
			try {
				KerMain.RunTurn(timer1.Interval, false);
			}
			catch (KernelPanicException) {
				HardTerminate();
			}
		}

		private void HardTerminate() {
			_terminated = true;
			timer1.Enabled = false;
			pauseButton.Enabled = false;
			terminateButton.Enabled = false;
			garbageCollectorButton.Enabled = false;
			staticVariablesButton.Enabled = false;
			_objectBrowser.Enabled = false;
		}

		private void garbageCollectorButton_Click(object sender, EventArgs e) {
			KerMain.RunGarbageCollector = true;
		}

		private void objectBrowserToolStripMenuItem_Click(object sender, EventArgs e) {
			_objectBrowser.Show(dockPanel);
		}

		private void staticVariablesButton_Click(object sender, EventArgs e) {
			SelectObject(KerMain.StaticData);
		}



		public void SelectObjects(KerObject[] arr) {
			if (arr != null && arr.Length > 0) {
				_objectBrowser.Obj = arr;
				_objectBrowser.Show(dockPanel);
			} else {
				_objectBrowser.Obj = null;
			}
		}

		public void SelectObject(KerObject obj) {
			_objectBrowser.Obj = new KerObject[] { obj };
			_objectBrowser.Show(dockPanel);
		}






		public override void StartRunningTurns() {
			timer1.Enabled = true;
		}

	}
}