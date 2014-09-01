using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Runtime;
using System.Windows.Forms;
using System.IO;
using Krkal.FileSystem;
using System.Collections.ObjectModel;
using Krkal.Compiler;
using Krkal.Ide;
using System.Globalization;




namespace Krkal.RunIde
{



	[Flags]
	public enum KrkalAppBehavior
	{
		None								= 0x000000,
		ShowStartupDialog					= 0x000001,
		ShowAlwaysStartupDialog				= 0x000002,

		LoadProfileFromConfigurations		= 0x000004,
		LoadLanguagesFromConfigurations		= 0x000008,
		LoadGameFromConfigurations			= 0x000010,
		LoadStartActionFromConfigurations	= 0x000020,
		UseProfileConfiguration				= 0x000040,
		UseGlobalConfuguration				= 0x000080,

		AllowIde							= 0x000100,
		AllowPluginsSetup					= 0x000200,
		AllowInternetLinks					= 0x000400,
		AllowHideOption						= 0x000800,

		UseDafaultResourceManager			= 0x001000,

		DeafultUseConfigurations = LoadProfileFromConfigurations | LoadLanguagesFromConfigurations | LoadGameFromConfigurations | LoadStartActionFromConfigurations | UseProfileConfiguration | UseGlobalConfuguration,
		DefaultStartDialogSettings = AllowIde | AllowPluginsSetup | AllowInternetLinks | AllowHideOption,
		DefaultStartUp = DeafultUseConfigurations | ShowStartupDialog | DefaultStartDialogSettings | UseDafaultResourceManager,

	}



	public enum StartAction
	{
		None,
		Exit,
		StartGame,
		StartIde,
		StartPluginsSetup,
	}


	public class KrkalApplication : IDisposable
	{
		bool _wantRestart;
		public bool WantRestart {
			get { return _wantRestart; }
			set {
				if (value)
					_behavior |= KrkalAppBehavior.ShowAlwaysStartupDialog;
				_wantRestart = value; 
			}
		}

		KrkalAppBehavior _behavior;
		public KrkalAppBehavior Behavior {
			get { return _behavior; }
			set { _behavior = value; }
		}


		String _caption = "Krkal";
		public String Caption {
			get { return _caption; }
			set { _caption = value; }
		}

		String _okText = "OK";
		public String OkText {
			get { return _okText; }
			set { _okText = value; }
		}

		String _cancelText = "Cancel";
		public String CancelText {
			get { return _cancelText; }
			set { _cancelText = value; }
		}



		KrkalApplicationContext _applicationContext = new KrkalApplicationContext();
		public KrkalApplicationContext ApplicationContext {
			get { return _applicationContext; }
		}

		IKrkalResourceManager _resourceManager;
		public IKrkalResourceManager ResourceManager {
			get { 
				if (_resourceManager == null) {
					SetLanguages(null, "EN");
				}
				return _resourceManager; 
			}
			set { _resourceManager = value;	}
		}

		public event EventHandler<ReloadResourceManagerEventArgs> ReloadResourceManager;
		public event EventHandler<EventArgs> ResourceManagerChanged;

		String _profile;
		public String Profile {
			get { return _profile; }
			set { if (value != _profile) ChangeProfile(value); }
		}

		public event EventHandler<ProfileChangedEnentArgs> ProfileChanged;

		int _profileLock;
		public void LockProfileChanging() { _profileLock++; }
		public void UnlockProfiechanging() { _profileLock--; }
		public bool IsProfileLocked {
			get { return _profileLock > 0; }
		}


		GameInfo _gameInfo;
		public GameInfo GameInfo {
			get { return _gameInfo; }
			set { _gameInfo = value; }
		}


		DataEnvironmentEx _dataEnvironment;
		public DataEnvironmentEx DataEnvironment {
			get {
				if (_dataEnvironment == null)
					_dataEnvironment = new DataEnvironmentEx();
				return _dataEnvironment; 
			}
		}


		EngineCollection _engines;
		public EngineCollection Engines {
			get { return _engines; }
		}


		IdeHelper _ideHelper;
		public IdeHelper IdeHelper {
			get {
				if (_ideHelper == null)
					_ideHelper = new IdeHelper(this);
				return _ideHelper; 
			}
		}






		// CONSTRUCTOR
		public KrkalApplication(KrkalAppBehavior behavior) {
			_behavior = behavior;
			if ((_behavior & KrkalAppBehavior.UseDafaultResourceManager) != 0)
				ReloadResourceManager += new EventHandler<ReloadResourceManagerEventArgs>(DefaultReloadResourceManager);
			RootNames.InitRootNames();
		}
	

		public void  Dispose()
		{
			RootNames.DoneRootNames();
		}



		protected virtual void ChangeProfile(string value) {
			if (String.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			if (IsProfileLocked)
				throw new InvalidOperationException("Prifile is locked");

			if (value.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
				throw new ArgumentException("Profile contains invalid characters.");

			var handler = ProfileChanged;
			if (handler != null) {
				ProfileChangedEnentArgs args = new ProfileChangedEnentArgs(value);
				handler(this, args);
				if (args.Cancel)
					return;
			}

			FS fs = FS.FileSystem;
			String profilePath = "$PROFILES$\\" + value;
			if (fs.CreateDir(profilePath) == 0)
				throw new FSFileNotFoundException("Unable to create profile");

			fs.AddFSDir("PROFILE", profilePath);
			CopyProfileFiles(fs);

			_profile = value;
		}



		private void CopyProfileFiles(FS fs) {
			using (FSSearchData search = fs.ReadDirectory("$ALLUSERS$\\Configuration")) {
				if (search != null) {
					for (int f = 0; f < search.Count; f++) {
						String dest = "$PROFILE$\\Configuration\\" + search.GetName(f);
						if (fs.FileExist(dest) == 0) {
							String source = "$ALLUSERS$\\Configuration\\" + search.GetName(f);
							fs.CopyTree(source, "$PROFILE$\\Configuration");
						}
					}
				}
			}
		}


		public virtual void ShowErrorMessage(String message) {
			MessageBox.Show(ResourceManager.GetText(message), Caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}



		void DefaultReloadResourceManager(object sender, ReloadResourceManagerEventArgs e) {
			if (e.NeedsToCreate)
				ResourceManager = new KrkalResourceManager();
			if (!((KrkalResourceManager)ResourceManager).Load(e.PrimaryLanguage, e.SecondaryLanguage)) {
				ShowErrorMessage("Error occurred during loading of language resources.");
			}
		}


		public virtual void SetLanguages(String primaryLanguage, String secondaryLanguage) {
			var handler = ReloadResourceManager;
			if (handler != null)
				handler(this, new ReloadResourceManagerEventArgs(primaryLanguage, secondaryLanguage, _resourceManager == null));

			if (_resourceManager == null)
				_resourceManager = new Krkal.Ide.EmptyResourceManager();

			var handler2 = ResourceManagerChanged;
			if (handler2 != null)
				handler2(this, EventArgs.Empty);

		}


		public virtual void InitializeCompiler() {
			KrkalCompiler.Compiler.Reset();
			StaticCustomSyntax cs = KrkalCompiler.Compiler.CustomSyntax;
			cs.InitializeCustomSyntax = InitializeCompiler2;
			cs.MixedNamespaces.Add("NameType");
			cs.MixedNamespaces.Add("GroupType");
			cs.MixedNamespaces.Add("ControlType");

			List<KnownName> names = new List<KnownName>();
			names.Add(new KnownName("_KSID_AllNameTypes", NameType.Void));
			cs.AddKnownNamesCollection(names);

			LoadCustomKeywords();
		}

		private void LoadCustomKeywords() {
			StaticCustomSyntax cs = KrkalCompiler.Compiler.CustomSyntax;
			using (RootNames rootNames = new RootNames(true)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), _dataEnvironment);

				IList<KsidName> nameTypes = dataSource.GetNameLayerOrSet(_dataEnvironment.RunIdeKnownName(RunIdeKnownNames.AllNameTypes), (KerNameType)(-1), false);
				foreach (KsidName nameType in nameTypes) {
					NameTypesInformation ntInfo = new NameTypesInformation(nameType, dataSource);
					if (ntInfo.Names != null && ntInfo.Values != null) {
						for (int f = 0; f < ntInfo.Names.Count && f < ntInfo.Values.Count; f++) {
							if (ntInfo.Names[f] != null)
								cs.CustomNameTypes.AddType(new CustomKeywordInfo(ntInfo.Names[f].Identifier.ToKsidString(), ntInfo.Values[f]));
						}
					}
				}
			}
		}



		public virtual void InitializeIde() {
			IdeHelper.LoadNewFileTemplates();
		}



		protected virtual void InitializeCompiler2(String engine, Compilation compilation, CustomSyntax customSyntax) {
			try {
				_engines[engine].InitialzeCompiler(compilation, customSyntax);
			}
			catch (EngineNotLoadedException ex) {
				ShowErrorMessage("Failed to load Game Engine.\n" + ex.Message);
			}
		}



		public void InitializeEngines() {
			_engines = new EngineCollection(this);
			_engines.Initialize();
		}


		public StartAction LoadConfiguration(string[] args) {
			_wantRestart = false;
			using (MainConfiguration mainConfiguration = new MainConfiguration(this)) {
				return mainConfiguration.Load(args);
			}
		}

		public void StartGame(GameInfo gameInfo) {
			if (gameInfo == null)
				throw new ArgumentNullException("gameInfo");
			if (gameInfo.StartLevel == null) {
				ShowErrorMessage("Start level not defined.");
				return;
			} 

			StartConfiguration startConf;
			using (RootNames rootNames = new RootNames(false)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), DataEnvironment);
				startConf = new StartConfiguration(gameInfo.StartLevel, dataSource);
			}

			RuntimeStarter starter = new RuntimeStarter(startConf, this);
			starter.Run();
			if (_applicationContext.Forms.Count > 0)
				_applicationContext.RunApplication();
		}

		public void StartIde() {
			RunIdeForm form = _applicationContext.FindRunIdeForm();
			if (form != null) {
				form.Activate();
			} else {
				_applicationContext.RegisterAndShowForm(new RunIdeForm(this));
				_applicationContext.RunApplication();
			}
		}

		public void StartPluginsSetup() {
			if (_applicationContext.Running)
				throw new InvalidOperationException("You cannot start setup while other windows are running.");
			_applicationContext.RegisterAndShowForm(new Setup(this));
			_applicationContext.RunApplication();
		}

		public virtual bool ExistsProfile(string profile) {
			return (FS.FileSystem.FileExist("$PROFILES$\\" + profile) != 0);
		}


	}











	public class KrkalApplicationContext : ApplicationContext
	{
		List<Form> _forms = new List<Form>();
		ReadOnlyCollection<Form> _roForms;
		public IList<Form> Forms {
			get { return _roForms; }
		}


		bool _running;
		public bool Running {
			get { return _running; }
		}


		// CONSTRUCTOR
		public KrkalApplicationContext() {
			_roForms = new ReadOnlyCollection<Form>(_forms);
		}


		public void RegisterAndShowForm(Form form) {
			if (form == null)
				throw new ArgumentNullException("form");
			_forms.Add(form);
			form.FormClosed += new FormClosedEventHandler(form_FormClosed);
			form.Show();
		}



		public void RunApplication() {
			if (_running)
				return;
			if (_forms.Count == 0)
				throw new InvalidOperationException("You cannot run application without registering a form.");
			_running = true;
			Application.Run(this);
		}


		void form_FormClosed(object sender, FormClosedEventArgs e) {
			_forms.Remove((Form)sender);
			if (_forms.Count == 0) {
				_running = false;
				ExitThread();
			}
		}




		public RunIdeForm FindRunIdeForm() {
			foreach (Form form in _forms) {
				RunIdeForm ret = form as RunIdeForm;
				if (ret != null)
					return ret;
			}
			return null;
		}


		public void CloseAll() {
			while (_forms.Count > 0) {
				_forms[0].Close();
			}
		}
	}











	public class ProfileChangedEnentArgs : EventArgs {
		public ProfileChangedEnentArgs(String newProfile) {
			_newProfile = newProfile;
		}

		String _newProfile;
		public String NewProfile
		{
		  get { return _newProfile; }
		}

		bool _cancel;
		public bool Cancel {
			get { return _cancel; }
			set { _cancel = value; }
		}
	}



	public class ReloadResourceManagerEventArgs : EventArgs
	{
		public ReloadResourceManagerEventArgs(String primaryLanguage, String secondaryLanguage, bool needsToCreate) {
			_primaryLanguage = primaryLanguage; _secondaryLanguage = secondaryLanguage; _needsToCreate = needsToCreate;
		}

		String _primaryLanguage;
		public String PrimaryLanguage {
			get { return _primaryLanguage; }
		}

		String _secondaryLanguage;
		public String SecondaryLanguage {
			get { return _secondaryLanguage; }
		}

		bool _needsToCreate;
		public bool NeedsToCreate {
			get { return _needsToCreate; }
		}
	}
}
