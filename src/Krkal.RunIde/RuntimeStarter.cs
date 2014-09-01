using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Runtime;
using Krkal.Compiler;
using System.Globalization;
using System.IO;

namespace Krkal.RunIde
{


	public interface IRuntimeStarter : IDisposable
	{
		// after calling this method the console is yours, add it to your form or dispose it.
		KerConsole TekeOverKerKonsole();

		KerConsole Console {
			get;
		}
		KerMain KerMain {
			get;
		}
		KernelParameters KernelParameters {
			get;
		}
		KrkalApplication Application {
			get;
		}
		int EngineRunMode {
			get;
			set;
		}
	}




	public class RuntimeStarter : IRuntimeStarter
	{
		String _source;
		
		int _engineRunMode;
		public int EngineRunMode {
			get { return _engineRunMode; }
			set { _engineRunMode = value; }
		}

		KerConsole _console;
		public KerConsole Console {
			get { return _console; }
		}

		KerMain _kerMain;
		public KerMain KerMain {
			get { return _kerMain; }
		}

		KernelParameters _kernelParameters;
		public KernelParameters KernelParameters {
			get { return _kernelParameters; }
		}

		KrkalApplication _application;
		public KrkalApplication Application {
			get { return _application; }
		}

		EngineForm _engineForm;


		// CONSTRUCTOR
		public RuntimeStarter(String source, KrkalApplication application, int engineRunMode, KernelRunMode kernelRunMode, KernelDebugMode kernelDebugMode) {
			_source = source;
			_application = application;
			_engineRunMode = engineRunMode;
			_kernelParameters = new KernelParameters(_source, kernelRunMode, kernelDebugMode);
		}

		internal RuntimeStarter(StartConfiguration startConfiguration, KrkalApplication application) {
			if (startConfiguration == null)
				throw new ArgumentNullException("startConfiguration");
			_source = startConfiguration.Source;
			_application = application;
			_engineRunMode = startConfiguration.EngineRunMode;
			_kernelParameters = new KernelParameters(_source, startConfiguration.KernelRunMode, startConfiguration.KernelDebugMode);
		}


		public void Run() {
			_console = new KerConsole();
			_console.LoadErrorTexts();
			try {
				_kernelParameters.OnError += new KerLoggingCallBackDelegate(_console.WriteLine);
				_kernelParameters.CreateEngineAndServices = new CreateEngineAndServicesDelegate(CreateEngineAndServices);
				_kerMain = new KerMain(_kernelParameters);
				_kerMain.Load();
				if (_engineForm != null)
					_engineForm.StartRunningTurns();
			}
			catch (KernelPanicException) {
				if (_console != null) {
					_engineRunMode = (int)DefaultEngineRunModes.None;
					_application.Engines[null].CreateGame(this);
				}
				_application.ShowErrorMessage("Error occured during start of Krkal Runtime. See log for further details.");
			}
		}

		public void Dispose() {
			if (_kerMain != null)
				_kerMain.Dispose();
			if (_kernelParameters != null)
				_kernelParameters.Dispose();
			if (_console != null)
				_console.Dispose();
			_kerMain = null;
			_kernelParameters = null;
			_console = null;
		}




		private bool CreateEngineAndServices(KerMain kerMain, String engine) {
			try {
				if (_engineRunMode == (int)DefaultEngineRunModes.AutoDetect) {
					if (_kernelParameters.RunMode == KernelRunMode.CreateData) {
						_engineRunMode = (int)DefaultEngineRunModes.CreateData;
					} else {
						_engineRunMode = (int)DefaultEngineRunModes.Run;
					}
				}

				if (_engineRunMode == (int)DefaultEngineRunModes.CreateData || _engineRunMode == (int)DefaultEngineRunModes.None) {
					_engineForm = _application.Engines[null].CreateGame(this);
				} else {
					_engineForm = _application.Engines[engine].CreateGame(this);
				}
			}
			catch (EngineNotLoadedException ex) {
				_application.ShowErrorMessage("Failed to load Game Engine.\n" + ex.Message);
				return false;
			}
			return true;
		}


		public KerConsole TekeOverKerKonsole() {
			KerConsole ret = _console;
			_console = null;
			return ret;
		}

	}






	[DataObjectClass("_KSID_StartConfiguration")]
	class StartConfiguration
	{
		String _source;
		[DataObjectMapping("_KSID_StartConfiguration__M_Source", BasicType.Char, 1)]
		public String Source {
			get { return _source; }
			set { _source = value; }
		}

		int _engineRunMode;
		[DataObjectMapping("_KSID_StartConfiguration__M_EngineRunMode", BasicType.Int)]
		public int EngineRunMode {
			get { return _engineRunMode; }
			set { _engineRunMode = value; }
		}

		KernelRunMode _kernelRunMode;
		public KernelRunMode KernelRunMode {
			get { return _kernelRunMode; }
			set { _kernelRunMode = value; }
		}
		[DataObjectMapping("_KSID_StartConfiguration__M_KernelRunMode", BasicType.Int)]
		public int KernelRunModeInt {
			get { return (int)_kernelRunMode; }
			set { _kernelRunMode = (KernelRunMode)value; }
		}

		KernelDebugMode _kernelDebugMode;
		public KernelDebugMode KernelDebugMode {
			get { return _kernelDebugMode; }
			set { _kernelDebugMode = value; }
		}
		[DataObjectMapping("_KSID_StartConfiguration__M_DebugMode", BasicType.Int)]
		public int KernelDebugModeInt {
			get { return (int)_kernelDebugMode; }
			set { _kernelDebugMode = (KernelDebugMode)value; }
		}


		// CONSTRUCTOR
		public StartConfiguration(StartConfiguration other) {
			_source = other._source;
			_engineRunMode = other._engineRunMode;
			_kernelRunMode = other._kernelRunMode;
			_kernelDebugMode = other._kernelDebugMode;
		}


		public StartConfiguration(String source, int engineRunMode, KernelRunMode kernelRunMode, KernelDebugMode kernelDebugMode) {
			_source = source;
			_engineRunMode = engineRunMode;
			_kernelRunMode = kernelRunMode;
			_kernelDebugMode = kernelDebugMode;
		}

		public StartConfiguration(String source, DataSource dataSource) {
			if (String.IsNullOrEmpty(source))
				throw new ArgumentException("Source argument is null or empty.");
			InitToDefaults(source);
			
			if (dataSource != null) {
				KsidName name = dataSource.LoadName(dataSource.Kernel.FindName(source));
				if (name != null) {
					KsidName name2 = dataSource.LoadName(dataSource.Kernel.ReadNameAttribute(name, ((DataEnvironmentEx)dataSource.Environment).RunIdeKnownName(RunIdeKnownNames.Attribute_StartConfiguration)));
					if (name2 != null)
						name = name2;
					dataSource.LoadObject(this, name);
				}
			}
		}


		public StartConfiguration(KsidName source, DataSource dataSource) {
			if (source == null)
				throw new ArgumentNullException("source");
			InitToDefaults(source.Identifier.ToKsidString());

			if (dataSource != null) {
				KsidName name2 = dataSource.LoadName(dataSource.Kernel.ReadNameAttribute(source, ((DataEnvironmentEx)dataSource.Environment).RunIdeKnownName(RunIdeKnownNames.Attribute_StartConfiguration)));
				if (name2 != null)
					source = name2;
				dataSource.LoadObject(this, source);
			}
		}


		private void InitToDefaults(string source) {
			_source = source;
			if (source != null && source.EndsWith(".kcp", true, CultureInfo.CurrentCulture)) {
				_source = Path.GetFileNameWithoutExtension(source) + ".code";
			}
			_engineRunMode = (int)DefaultEngineRunModes.AutoDetect;
			_kernelDebugMode = KernelDebugMode.Debug;
			_kernelRunMode = KernelRunMode.Normal;
		}


	}



	class StartConfigurations
	{
		#region class SolutionInfo
		internal class SolutionInfo
		{
			StartConfiguration _startConfiguration;
			public StartConfiguration StartConfiguration {
				get { return _startConfiguration; }
				set { _startConfiguration = value; }
			}
			String _selectedProject;
			public String SelectedProject {
				get { return _selectedProject; }
				set { _selectedProject = value; }
			}
			String _solutionName;
			public String SolutionName {
				get { return _solutionName; }
			}

			// CONSTRUCTOR
			public SolutionInfo(String solutionName) {
				_solutionName = solutionName;
			}
			public SolutionInfo(String solutionName, String selectedProject, StartConfiguration startConfiguration) {
				_solutionName = solutionName;
				_selectedProject = selectedProject;
				_startConfiguration = startConfiguration;
			}
		}
		#endregion

		RunIdeForm _myForm;

		bool _useSolution = true;
		public bool UseSolution {
			get { return _useSolution; }
			set { _useSolution = value; }
		}

		Dictionary<String, SolutionInfo> _solutions = new Dictionary<string, SolutionInfo>();
		internal ICollection<SolutionInfo> SolutionInfos {
			get { return _solutions.Values; }
			set {
				_solutions = new Dictionary<string, SolutionInfo>();
				if (value != null) {
					foreach(SolutionInfo info in value) {
						if (info != null && !String.IsNullOrEmpty(info.SolutionName))
							_solutions.Add(info.SolutionName, info);
					}
				}
			}
		}
		StartConfiguration _globalConfig;
		internal StartConfiguration GlobalConfig {
			get { return _globalConfig; }
			set { _globalConfig = value; }
		}

		internal StartConfiguration CurrentConfiguration {
			get {
				if (_useSolution) {
					if (!_myForm.Project.ValidSolution)
						return null;
					SolutionInfo ret;
					if (_solutions.TryGetValue(_myForm.Project.Solution, out ret) && ret.StartConfiguration != null)
						return ret.StartConfiguration;
					return LoadSolutionDefaults(_myForm.Project.Solution);
				} else {
					return _globalConfig;
				}
			}
		}




		// CONSTRUCTOR
		public StartConfigurations(RunIdeForm myForm) {
			_myForm = myForm;
			_myForm.Project.SelectedProjectChanged += new EventHandler<EventArgs>(Project_SelectedProjectChanged);
		}




		private StartConfiguration LoadSolutionDefaults(string solution) {
			using (RootNames rootNames = new RootNames(true)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), _myForm.Application.DataEnvironment);
				StartConfiguration ret = new StartConfiguration(solution, dataSource);
				SolutionInfo info = AddSolutionInfo(solution);
				info.StartConfiguration = ret;
				return ret;
			}
		}



		private SolutionInfo AddSolutionInfo(string solution) {
			SolutionInfo info;
			if (!_solutions.TryGetValue(solution, out info)) {
				info = new SolutionInfo(solution);
				_solutions.Add(solution, info);
			}
			return info;
		}



		public void StoreConfiguration(StartConfiguration configuration) {
			if (_useSolution) {
				if (_myForm.Project.ValidSolution) {
					SolutionInfo info = AddSolutionInfo(_myForm.Project.Solution);
					info.StartConfiguration = configuration;
				}
			} else {
				_globalConfig = configuration;
			}
		}



		internal String GetSelectedProject(string solution) {
			SolutionInfo info;
			if (_solutions.TryGetValue(solution, out info)) {
				return info.SelectedProject;
			} else {
				return null;
			}
		}


		void Project_SelectedProjectChanged(object sender, EventArgs e) {
			if (_myForm.Project.ValidSolution && _myForm.Project.Projects != null && _myForm.Project.SelectedProject >= 0 && _myForm.Project.SelectedProject < _myForm.Project.Projects.Count) {
				SolutionInfo info = AddSolutionInfo(_myForm.Project.Solution);
				info.SelectedProject = _myForm.Project.Projects[_myForm.Project.SelectedProject];
			}
		}

	}
}
