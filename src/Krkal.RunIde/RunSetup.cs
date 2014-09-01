using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;
using Krkal.Compiler;
using System.Globalization;
using System.IO;

namespace Krkal.RunIde
{
	public partial class RunSetup : Form
	{
		RunIdeForm _myForm;
		List<int> _engineModeValues;

		bool _run;
		public bool Run {
			get { return _run; }
		}

		// CONSTRUCTOR
		public RunSetup(RunIdeForm myForm) {
			_myForm = myForm;
			InitializeComponent();
			IntitStartModes();
			if (_myForm.StartConfigurations.UseSolution) {
				saveWithSolution.Checked = true;
			} else {
				saveGlobally.Checked = true;
			}
			LoadConfiguration(_myForm.StartConfigurations.CurrentConfiguration);
		}


	
		private void LoadConfiguration(StartConfiguration startConfiguration) {
			if (startConfiguration == null)
				return;
			primarySourceTextBox.Text = startConfiguration.Source;
			
			for (int f = 0; f < _engineModeValues.Count; f++) {
				if (_engineModeValues[f] == startConfiguration.EngineRunMode) {
					engineModeCombo.SelectedIndex = f;
					break;
				}
			}

			int a = startConfiguration.KernelRunModeInt;
			if (a >= 0 && a < kernelModeCombo.Items.Count)
				kernelModeCombo.SelectedIndex = a;

			a = startConfiguration.KernelDebugModeInt;
			if (a >= 0 && a < debugModeCombo.Items.Count)
				debugModeCombo.SelectedIndex = a;
		}



		private void IntitStartModes() {
			_engineModeValues = new List<int>(_myForm.Application.Engines.StartModes.Count);
			foreach (var engineMode in _myForm.Application.Engines.StartModes) {
				engineModeCombo.Items.Add(engineMode.Key.ToString(true));
				_engineModeValues.Add(engineMode.Value);
			}
			if (_engineModeValues.Count > 0)
				engineModeCombo.SelectedIndex = 0;
			kernelModeCombo.SelectedIndex = 0;
			debugModeCombo.SelectedIndex = 0;
		}



		private void saveWithSolution_CheckedChanged(object sender, EventArgs e) {
			if (saveWithSolution.Checked)
				_myForm.StartConfigurations.UseSolution = true;
		}

		private void saveGlobally_CheckedChanged(object sender, EventArgs e) {
			if (saveGlobally.Checked)
				_myForm.StartConfigurations.UseSolution = false;
		}




		private void okButton_Click(object sender, EventArgs e) {
			if (saveWithSolution.Checked && !_myForm.Project.ValidSolution) {
				_myForm.Application.ShowErrorMessage("Cannot save the configuration with solution, because no valid solution is currently opened.");
				return;
			}
			if (String.IsNullOrEmpty(primarySourceTextBox.Text)) {
				_myForm.Application.ShowErrorMessage("Plaese fill the source text box.");
				return;
			}

			int engineMode = 0;
			if (_engineModeValues.Count > 0)
				engineMode = _engineModeValues[engineModeCombo.SelectedIndex];

			StartConfiguration ret = new StartConfiguration(primarySourceTextBox.Text, engineMode, (KernelRunMode)kernelModeCombo.SelectedIndex, (KernelDebugMode)debugModeCombo.SelectedIndex);
			_myForm.StartConfigurations.StoreConfiguration(ret);
			this.DialogResult = DialogResult.OK;
		}




		private void loadButton_Click(object sender, EventArgs e) {
			using (FileBrowserDialogEx fileBrowser = new FileBrowserDialogEx(_myForm.CurrentDirectory, _myForm)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Select Start Configuration Source";

				InitFBDialogNames(fileBrowser);

				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					if (fileBrowser.SelectedName != null) {
						using (RootNames rootNames = new RootNames(false)) {
							DataSource dataSource = new DataSource(rootNames.GetKernel(), _myForm.Application.DataEnvironment);
							StartConfiguration config = new StartConfiguration(fileBrowser.SelectedName, dataSource);
							LoadConfiguration(config);
						}
					} else {
						FillPrimarySource(fileBrowser.SelectedItemKeyName);
					}
				}
				_myForm.CurrentDirectory = fileBrowser.Directory;
			}

		}



		private void InitFBDialogNames(FileBrowserDialogEx fileBrowser) {
			using (RootNames rootNames = new RootNames(true)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), _myForm.Application.DataEnvironment);
				IList<KsidName> names = dataSource.GetNameLayerOrSet(null, KerNameType.Solution, true);
				List<String> userNames = new List<String>(names.Count);

				foreach (KsidName name in names) {
					userNames.Add(dataSource.Kernel.ReadUserName(name));
				}

				fileBrowser.AddNameList("Solutions", names, userNames);

				names = dataSource.GetNameLayerOrSet(null, KerNameType.Level, true);
				userNames.Clear();

				foreach (KsidName name in names) {
					userNames.Add(dataSource.Kernel.ReadUserName(name));
				}

				fileBrowser.AddNameList("Levels", names, userNames);
			}
		}



		private void primarySourceBrowse_Click(object sender, EventArgs e) {
			using (FileBrowserDialogEx fileBrowser = new FileBrowserDialogEx(_myForm.CurrentDirectory, _myForm)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Select Level / Solution / Code File / Saved Game";

				InitFBDialogNames(fileBrowser);

				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					if (fileBrowser.SelectedName != null) {
						primarySourceTextBox.Text = fileBrowser.SelectedName.Identifier.ToKsidString();
					} else {
						FillPrimarySource(fileBrowser.SelectedItemKeyName);
					}
				}
				_myForm.CurrentDirectory = fileBrowser.Directory;
			}

		}



		private void FillPrimarySource(string source) {
			if (source.EndsWith(".kcp", true, CultureInfo.CurrentCulture)) {
				source = Path.GetFileNameWithoutExtension(source) + ".code";
			}
			primarySourceTextBox.Text = source;
		}

		private void okRunButton_Click(object sender, EventArgs e) {
			okButton_Click(sender, e);
			if (DialogResult == DialogResult.OK)
				_run = true;
		}

	}
}
