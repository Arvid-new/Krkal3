using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Runtime;
using Krkal.Compiler;
using Krkal.FileSystem;
using System.Globalization;
using System.Windows.Forms;
using System.Collections;
using Krkal.Ide;

namespace Krkal.RunIde
{
	[DataObjectClass("_KSID_GameInformation")]
	public class GameInfo
	{
		String _webPage;
		[DataObjectMapping("_KSID_GameInformation__M_WebPage", BasicType.Char, 1)]
		public String WebPage {
			get { return _webPage; }
			set { _webPage = value; }
		}
		
		String _author;
		[DataObjectMapping("_KSID_GameInformation__M_Author", BasicType.Char, 1)]
		public String Author {
			get { return _author; }
			set { _author = value; }
		}

		KsidName _icon;
		[DataObjectMapping("_KSID_GameInformation__M_Icon", BasicType.Name)]
		public KsidName Icon {
			get { return _icon; }
			set { _icon = value; }
		}

		KsidName _startLevel;
		[DataObjectMapping("_KSID_GameInformation__M_StartLevel", BasicType.Name)]
		public KsidName StartLevel {
			get { return _startLevel; }
			set { _startLevel = value; }
		}

		KsidName _name;
		public KsidName Name {
			get { return _name; }
		}

		// CONSTRUCTOR
		public GameInfo(KsidName name, DataEnvironment env) {
			if (env == null)
				throw new ArgumentNullException("env");

			using (RootNames rootNames = new RootNames(false)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), env);
				Init(name, dataSource);
			}
		}


		// CONSTRUCTOR
		public GameInfo(KsidName name, DataSource dataSource) {
			Init(name, dataSource);
		}


		private void Init(KsidName name, DataSource dataSource) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (dataSource == null)
				throw new ArgumentNullException("dataSource");

			_name = name;
			dataSource.LoadObject(this, name);
		}


	}







	[DataObjectClass("_KSID_MainConfiguration")]
	public class MainConfiguration : IDisposable
	{
		String _primaryLanguage;
		[DataObjectMapping("_KSID_MainConfiguration__M_PrimaryLanguage", BasicType.Char, 1)]
		public String PrimaryLanguage {
			get { return _primaryLanguage; }
			set { _primaryLanguage = value; }
		}

		String _secondaryLanguage = "EN";
		[DataObjectMapping("_KSID_MainConfiguration__M_SecondaryLanguage", BasicType.Char, 1)]
		public String SecondaryLanguage {
			get { return _secondaryLanguage; }
			set { _secondaryLanguage = value; }
		}
		
		KsidName _game;
		[DataObjectMapping("_KSID_MainConfiguration__M_Game", BasicType.Name)]
		public KsidName Game {
			get { return _game; }
			set { _game = value; }
		}
		
		String _profile;
		[DataObjectMapping("_KSID_MainConfiguration__M_Profile", BasicType.Char, 1)]
		public String Profile {
			get { return _profile; }
			set { _profile = value; }
		}

		bool _dontShowStartDialog2; // this one is not saved
		bool _dontShowStartDialog;
		[DataObjectMapping("_KSID_MainConfiguration__M_DontShowStartDialog", BasicType.Int)]
		public bool DontShowStartDialog {
			get { return _dontShowStartDialog; }
			set { _dontShowStartDialog = value; }
		}

		int _intStartAction;
		[DataObjectMapping("_KSID_MainConfiguration__M_StartAction", BasicType.Int)]
		public int IntStartAction {
			get { return _intStartAction; }
			set { _intStartAction = value; _startAction = (StartAction)value; }
		}
		StartAction _startAction;
		public StartAction StartAction {
			get { return _startAction; }
			set { 
				_startAction = value;
				if (value == StartAction.StartGame || value == StartAction.StartIde)
					_intStartAction = (int)value;
			}
		}


		KrkalApplication _application;
		public KrkalApplication KrkalApplication {
			get { return _application; }
		}

		DataSource _allUsersConfiguration;
		DataSource _profileConfiguration;


		// CONSTRUCTOR
		public MainConfiguration(KrkalApplication application) {
			_application = application;
		}



		public StartAction Load(string[] args) {
			if ((_application.Behavior & KrkalAppBehavior.UseGlobalConfuguration) != 0) {
				_allUsersConfiguration = LoadConfiguration("$ALLUSERS$\\Configuration\\MainConfiguration.data");
			}
			ApplyCommandLineProfile(args);
			ChangeProfile(_profile);
			ApplyCommandLineArgs(args);
			ApplyLanguages();

			if (StartAction == StartAction.None || (!_dontShowStartDialog && !_dontShowStartDialog2) || (_application.Behavior & KrkalAppBehavior.ShowAlwaysStartupDialog) != 0) {
				if ((_application.Behavior & KrkalAppBehavior.ShowStartupDialog) != 0) {
					using (StartDialog startDialog = new StartDialog(this)) {
						Application.Run(startDialog);
					}
				}
			}

			ApplyGame();
			Save(_allUsersConfiguration);
			Save(_profileConfiguration);

			return StartAction;
		}



		const string ProfileArg = "profile:";
		const string GameArg = "game:";
		const string LanguageArg = "lang:";
		const string RunIde = "ide";
		const string StartDialog = "config";



		private void ApplyCommandLineProfile(string[] args) {
			if (args != null && (_application.Behavior & KrkalAppBehavior.LoadProfileFromConfigurations) != 0) {
				foreach (String arg in args) {
					if (arg.StartsWith(ProfileArg, true, CultureInfo.CurrentCulture)) {
						_profile = arg.Substring(ProfileArg.Length);
						return;
					}
				}
			}
		}




		private void ApplyCommandLineArgs(string[] args) {
			if (args != null) {
				foreach (String arg in args) {
					if (arg.StartsWith(GameArg, true, CultureInfo.CurrentCulture)) {
						try {
							Identifier id = Identifier.Parse(arg.Substring(GameArg.Length));
							if (id.Root != IdentifierRoot.Localized) {
								KsidName name;
								if (!_application.DataEnvironment.Names.TryGetName(id, out name))
									name = _application.DataEnvironment.Names.CreateName(id, (NameType)KerNameType.Game);
								_game = name;
								StartAction = StartAction.StartGame;
								_dontShowStartDialog2 = true;
							}
						}
						catch (FormatException) { }

					} else if (arg.StartsWith(LanguageArg, true, CultureInfo.CurrentCulture)) {
						_primaryLanguage = arg.Substring(LanguageArg.Length);
					} else if (String.Compare(arg, RunIde, true, CultureInfo.CurrentCulture) == 0) {
						StartAction = StartAction.StartIde;
						_dontShowStartDialog2 = true;
					} else if (String.Compare(arg, StartDialog, true, CultureInfo.CurrentCulture) == 0) {
						StartAction = StartAction.None;
					}
				}
			}
		}



		public void ChangeProfile(String profile) {
			if ((_application.Behavior & KrkalAppBehavior.LoadProfileFromConfigurations) != 0) {
				if (profile != null) {
					_application.Profile = profile;
					_profile = profile;
					if ((_application.Behavior & KrkalAppBehavior.UseProfileConfiguration) != 0) {
						if (_profileConfiguration != null)
							_profileConfiguration.Dispose();
						_profileConfiguration = null;
						_profileConfiguration = LoadConfiguration("$PROFILE$\\Configuration\\MainConfiguration.data");
					}
				}
			}
		}



		private DataSource LoadConfiguration(string path) {
			DataSource dataSource = new DataSource(path, _application.DataEnvironment);
			dataSource.LoadObject(this, _application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.MainConfigurationData));
			return dataSource;
		}



		private void Save(DataSource dataSource) {
			if (dataSource != null) {
				dataSource.SaveObject(this, _application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.MainConfigurationData));
				dataSource.Save();
			}
		}

		private void ApplyGame() {
			if (_game != null && (_application.Behavior & KrkalAppBehavior.LoadGameFromConfigurations) != 0) {
				_application.GameInfo = new GameInfo(_game, _application.DataEnvironment);
			}
		}

		public void ApplyLanguages() {
			if ((_application.Behavior & KrkalAppBehavior.LoadLanguagesFromConfigurations) != 0) {
				_application.SetLanguages(_primaryLanguage, _secondaryLanguage);
			}
		}



		public void Dispose() {
			if (_allUsersConfiguration != null)
				_allUsersConfiguration.Dispose();
			if (_profileConfiguration != null)
				_profileConfiguration.Dispose();
			_allUsersConfiguration = null;
			_profileConfiguration = null;
		}


		internal void CreateProfile(string profile) {
			_profile = profile;
			if ((_application.Behavior & KrkalAppBehavior.UseProfileConfiguration) != 0) {
				if (_profileConfiguration != null)
					_profileConfiguration.Dispose();
				_profileConfiguration = null;
				_profileConfiguration = new DataSource("$PROFILE$\\Configuration\\MainConfiguration.data", _application.DataEnvironment);
				Save(_profileConfiguration);
			}
		}
	}










	public class StartInformation
	{
		IList<KsidName> _languages;
		public IList<KsidName> Languages {
			get { return _languages; }
		}

		List<GameInfo> _games;
		public List<GameInfo> Games {
			get { return _games; }
		}

		List<String> _profiles;
		public List<String> Profiles {
			get { return _profiles; }
		}


		// CONSTRUCTOR
		public StartInformation(DataSource dataSource) {
			DataEnvironmentEx env = (DataEnvironmentEx)dataSource.Environment;
			_languages = dataSource.GetNameLayerOrSet(env.RunIdeKnownName(RunIdeKnownNames.AllLanguages), KerNameType.Language, false);
			
			IList<KsidName> games = dataSource.GetNameLayerOrSet(env.RunIdeKnownName(RunIdeKnownNames.AllGames), KerNameType.Game, false);
			_games = new List<GameInfo>(games.Count);
			foreach (KsidName name in games) {
				_games.Add(new GameInfo(name, dataSource));
			}

			_profiles = new List<string>();
			using (FSSearchData searchData = FS.FileSystem.ReadDirectory("$PROFILES$")) {
				if (searchData != null) {
					for (int f = 0; f < searchData.Count; f++) {
						if (searchData.GetAttr(f) == 2 || searchData.GetAttr(f) == 3) {
							if (searchData.GetName(f).EndsWith(".user", true, CultureInfo.CurrentCulture)) {
								_profiles.Add(searchData.GetName(f));
							}
						}
					}
				}
			}
		}

		internal void AddProfile(string profile) {
			_profiles.Add(profile);
		}
	}





	public enum RunIdeKnownNames
	{
		AllLanguages,
		AllGames,
		MainConfigurationData,
		AllEngines,
		AllNameTypes,
		AllNewFileTemplates,
		Attribute_StartConfiguration,
		IdeFormConfiguratuionData,
	}



	public class DataEnvironmentEx : DataEnvironment
	{
		KsidName[] _knownNames;
		public KsidName RunIdeKnownName(RunIdeKnownNames nameIndex) {
			return _knownNames[(int)nameIndex];
		}

		static List<KnownName> _knownNamesSource;

		public DataEnvironmentEx() {
			InitKnownNames(KnownNamesSource);
		}


		private void InitKnownNames(ICollection<KnownName> names) {
			_knownNames = new KsidName[names.Count];
			int g = 0;
			foreach (KnownName name in names) {
				_knownNames[g] = Names.DeclareKnownName(name);
				g++;
			}
		}


		public static IList<KnownName> KnownNamesSource {
			get {
				if (_knownNamesSource == null) {
					_knownNamesSource = new List<KnownName>();
					_knownNamesSource.Add(new KnownName("_KSID_AllLanguages", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_AllGames", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_MainConfigurationData", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_AllEngines", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_AllNameTypes", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_AllNewFileTemplates", NameType.Void));
					_knownNamesSource.Add(new KnownName("_KSID_Attribute__M_StartConfiguration", NameType.Variable, new LanguageType(BasicType.Name, Modifier.Public), null));
					_knownNamesSource.Add(new KnownName("_KSID_IdeFormConfiguratuionData", NameType.Void));
				}
				return DataEnvironmentEx._knownNamesSource; 
			}
		}
	
	}






	[DataObjectClass("_KSID_NameTypesInformation")]
	public class NameTypesInformation
	{
		KsidName _name;
		public KsidName Name {
			get { return _name; }
		}


		IList<KsidName> _names;
		[DataObjectMapping("_KSID_NameTypesInformation__M_Names", BasicType.Name, 1)]
		public IList<KsidName> Names {
			get { return _names; }
			set { _names = value; }
		}

		IList<int> _values;
		[DataObjectMapping("_KSID_NameTypesInformation__M_Values", BasicType.Int, 1)]
		public IList<int> Values {
			get { return _values; }
			set { _values = value; }
		}

		public NameTypesInformation(KsidName name, DataSource dataSource) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (dataSource == null)
				throw new ArgumentNullException("dataSource");
			_name = name;
			dataSource.LoadObject(this, _name);
		}
	}






}
