using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Krkal.CodeGenerator;
using Krkal.Compiler;
using Krkal.FileSystem;
using Krkal.Runtime;

namespace Krkal.RunIde
{
	public partial class RunIdeForm : Krkal.Ide.MainForm
	{
		private Builder _builder;
		public Builder Builder {
			get { return _builder; }
		}


		private KrkalApplication _application;
		public KrkalApplication Application {
			get { return _application; }
		}

		private StartConfigurations _startConfigurations;
		internal StartConfigurations StartConfigurations {
			get {
				if (_startConfigurations == null)
					_startConfigurations = new StartConfigurations(this);
				return _startConfigurations; 
			}
		}
		
		public RunIdeForm(KrkalApplication application) : base(application.IdeHelper) {
			if (application == null)
				throw new ArgumentNullException("application");
			_application = application;
			application.IdeHelper.MyForm = this;
			InitializeComponent();

			_builder = new Builder(this);

			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RunIdeForm));
			ToolStripMenuItem projectMenu = mainMenu.Items.Find("menuItemProject", false)[0] as ToolStripMenuItem;

			ToolStripMenuItem runButton = new ToolStripMenuItem();
			runButton.Text = "&Run";
			runButton.Click += new EventHandler(runButton_Click);
			runButton.Image = Krkal.RunIde.Properties.Resources.FormRunHS;
			runButton.ShortcutKeys = System.Windows.Forms.Keys.F5;
			projectMenu.DropDown.Items.Add(runButton);


			runButton = new ToolStripMenuItem();
			runButton.Click += new EventHandler(runButton_Click);
			runButton.Image = Krkal.RunIde.Properties.Resources.FormRunHS;
			runButton.ToolTipText = "Run";
			toolBar.Items.Add(runButton);

			ToolStripMenuItem runConfigurationButton = new ToolStripMenuItem();
			runConfigurationButton.Text = "Con&figure Startup";
			runConfigurationButton.Click += new EventHandler(configureStartupButton_Click);
			runConfigurationButton.Image = Krkal.RunIde.Properties.Resources.DataContainer_NewRecordHS;
			runConfigurationButton.ShortcutKeys = System.Windows.Forms.Keys.F5 | System.Windows.Forms.Keys.Control;
			projectMenu.DropDown.Items.Add(runConfigurationButton);


			runConfigurationButton = new ToolStripMenuItem();
			runConfigurationButton.Click += new EventHandler(configureStartupButton_Click);
			runConfigurationButton.Image = Krkal.RunIde.Properties.Resources.DataContainer_NewRecordHS;
			runConfigurationButton.ToolTipText = "Configure Startup";
			toolBar.Items.Add(runConfigurationButton);


			ToolStripMenuItem buildSettings = new ToolStripMenuItem();
			buildSettings.Text = "&Settings and Output";
			buildSettings.Click += builderMenuItem_Click;
			buildSettings.Image = Krkal.RunIde.Properties.Resources.gear_2;
			buildSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			projectMenu.DropDown.Items.Add(buildSettings);


			buildSettings = new ToolStripMenuItem();
			buildSettings.Click += builderMenuItem_Click;
			buildSettings.Image = Krkal.RunIde.Properties.Resources.gear_2;
			buildSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			buildSettings.ToolTipText = "Build Settings and Output";
			toolBar.Items.Add(buildSettings);

			ToolStripMenuItem fileMenu = mainMenu.Items.Find("menuItemFile", false)[0] as ToolStripMenuItem;

			ToolStripMenuItem restartButton = new ToolStripMenuItem();
			restartButton.Text = "Exit To Start Menu";
			restartButton.Click += new EventHandler(restartButton_Click);
			fileMenu.DropDown.Items.Add(restartButton);

			FS.AddVersionFeature(FSVersionFeature.All, null, "Krkal", 1);
		}




		protected override IDockContent GetContentFromPersistString(string persistString) {
			string[] parsedStrings = persistString.Split(',');
			
			if (parsedStrings.Length > 0 && parsedStrings[0] == typeof(Builder).ToString()) {
				if (parsedStrings.Length >= 4) {
					_builder.StepsToDo = (StepsToDo)Enum.Parse(typeof(StepsToDo), parsedStrings[1]);
					_builder.Configuration = parsedStrings[2];
					_builder.BinPath = parsedStrings[3];
				}
				return _builder;
			} else {
				return base.GetContentFromPersistString(persistString);
			}
		}


		protected override void MainForm_Load(object sender, EventArgs e) {
			base.MainForm_Load(sender, e);
			IdeConfigurationSaver saver = new IdeConfigurationSaver(this);
			saver.Load();
			Project.ExpandSolution();
		}


		protected override void MainForm_Closing(object sender, CancelEventArgs e) {
			if (_bSaveLayout) {
				IdeConfigurationSaver saver = new IdeConfigurationSaver(this);
				saver.Save();
			} else {
				IdeConfigurationSaver.DeleteConfigurationFile();
			}
			base.MainForm_Closing(sender, e);
		}


		void runButton_Click(object sender, EventArgs e) {
			RunGame();
		}

	
		void configureStartupButton_Click(object sender, EventArgs e) {
			ConfigureStartup();
		}


		private void builderMenuItem_Click(object sender, EventArgs e) {
			_builder.Show(dockPanel);
		}


		protected override void CloseAll() {
			_application.ApplicationContext.CloseAll();
		}


		void restartButton_Click(object sender, EventArgs e) {
			_application.ApplicationContext.CloseAll();
			_application.WantRestart = true;
		}







		private delegate BuilderInfo GetBuilderInfoDelegate();

		private BuilderInfo GetBuilderInfo2() {
			return new BuilderInfo(_builder.StepsToDo, _builder.BinPath, _builder.Configuration);
		}
		public BuilderInfo GetBuilderInfo() {
			if (InvokeRequired) {
				return (BuilderInfo)Invoke(new GetBuilderInfoDelegate(GetBuilderInfo2));
			} else {
				return GetBuilderInfo2();
			}
		}






		public void RunGame() {
			if (StartConfigurations.CurrentConfiguration != null) {
				RuntimeStarter starter = new RuntimeStarter(StartConfigurations.CurrentConfiguration, _application);
				starter.Run();
			}
		}


		public void ConfigureStartup() {
			using (RunSetup runsetup = new RunSetup(this)) {
				runsetup.ShowDialog();
				if (runsetup.Run)
					RunGame();
			}
		}



		public override void OpenProject() {
			using (FileBrowserDialogEx fileBrowser = new FileBrowserDialogEx(CurrentDirectory, this)) {
				fileBrowser.MultiSelect = false;
				fileBrowser.Text = "Open Project";

				using (RootNames rootNames = new RootNames(true)) {
					DataSource dataSource = new DataSource(rootNames.GetKernel(), _application.DataEnvironment);
					IList<KsidName> names = dataSource.GetNameLayerOrSet(null, KerNameType.Solution, true);
					List<String> userNames = new List<String>(names.Count);

					foreach (KsidName name in names) {
						userNames.Add(dataSource.Kernel.ReadUserName(name));
					}

					fileBrowser.AddNameList("Solutions", names, userNames);
				}

				if (fileBrowser.ShowDialog() == DialogResult.OK) {
					if (fileBrowser.SelectedName != null) {
						Project.Solution = fileBrowser.SelectedName.Identifier.ToKsidString();
						Project.SolutionUserName = fileBrowser.SelectedUserName;
					} else {
						Project.Solution = fileBrowser.SelectedItemKeyName;
						Project.SolutionUserName = null;
					}
				}
				CurrentDirectory = fileBrowser.Directory;
			}
		}

	}





	public class BuilderInfo
	{
		StepsToDo _stepsToDo;
		public StepsToDo StepsToDo {
			get { return _stepsToDo; }
		}

		String _binPath;
		public String BinPath {
			get { return _binPath; }
		}

		String _configuration;
		public String Configuration {
			get { return _configuration; }
		}

		public BuilderInfo(StepsToDo stepsToDo, String binPath, String configuration) {
			_stepsToDo = stepsToDo;
			_binPath = binPath;
			_configuration = configuration;
		}
	}



}

