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
using Krkal.Services;
using Krkal.RunIde;


namespace Krkal.Sample
{
	public partial class GameMainForm : EngineForm
	{

		private bool _bSaveLayout = true;
		private DeserializeDockContent _deserializeDockContent;

		IRuntimeStarter _runtimeStarter;

		KerConsoleForm _console;
		ObjectBrowserForm _objectBrowser;
		public ObjectBrowserForm ObjectBrowser {
			get { return _objectBrowser; }
		}
		Map _map;
		ObjectList _objectList;
		public ObjectList ObjectList {
			get { return _objectList; }
		}

		public KerMain KerMain {
			get { return _runtimeStarter.KerMain; }
		}

		public KrkalApplication KApplication {
			get { return _runtimeStarter.Application; }
		}


		SampleServices _services;
		public SampleServices Services {
			get { return _services; }
		}


		bool _terminated;
		public bool Terminated {
			get { return _terminated; }
		}

		public ImageList MapGraphics {
			get { return mapGraphics; }
		}


		public GameMainForm(IRuntimeStarter runtimeStarter) {
			if (runtimeStarter == null)
				throw new ArgumentNullException("runtimeStarter");
			_runtimeStarter = runtimeStarter;
			InitializeComponent();
			_console = new KerConsoleForm(_runtimeStarter.TekeOverKerKonsole());
			_objectBrowser = new ObjectBrowserForm();
			_objectBrowser.KerMain = KerMain;
			_map = new Map(this);
			_objectList = new ObjectList(this);
			_deserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
			_services = new SampleServices(KerMain);
		}


		private IDockContent GetContentFromPersistString(string persistString) {

			if (persistString == typeof(KerConsoleForm).ToString()) {
				return _console;
			} else if (persistString == typeof(ObjectBrowserForm).ToString()) {
				return _objectBrowser;
			} else if (persistString == typeof(Map).ToString()) {
				return _map;
			} else if (persistString == typeof(ObjectList).ToString()) {
				return _objectList;
			} else {
				return null;
			}
		}



		const string ConfigFile = "$PROFILE$\\Configuration\\SampleGame.config";


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

			if (_services != null)
				_services.Dispose();

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

			_map.Show(dockPanel);
			_objectList.Show(dockPanel);
			_objectBrowser.Show(dockPanel);
			_console.Show(dockPanel);

			dockPanel.ResumeLayout(true, true);
		}




		private void InvalidateMap() {
			int x = 0, y = 0, dx=0, dy=0;
			if (_services.MapSizeChanged) {
				_services.GetSize(ref x, ref y);
				_map.SetSize(x, y);
			}
			_services.GetInvalidArea(ref x, ref y, ref dx, ref dy);
			_map.InvalidateArea(x, y, dx, dy);
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
				InvalidateMap();
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
			_map.Enabled = false;
			_objectList.Enabled = false;
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

		private void mapToolStripMenuItem_Click(object sender, EventArgs e) {
			_map.Show(dockPanel);
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

		private void objectListToolStripMenuItem_Click(object sender, EventArgs e) {
			_objectList.Show(dockPanel);
		}





		public override void StartRunningTurns() {
			try {
				timer1.Enabled = true;
				InvalidateMap();
				_objectList.InitializeNames();
			}
			catch (KernelPanicException) {
				HardTerminate();
				KApplication.ShowErrorMessage("Error occured during start of Krkal Runtime. See log for further details.");
			}
		}

	}
}