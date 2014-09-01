using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;
using Krkal.FileSystem;
using System.IO;
using System.Globalization;

namespace Krkal.Ide
{

	public delegate void CompilationStartedHandler (CompilationType compilationType, bool rebuildAll);
	public delegate void CompilationEndedHandler (ErrorLog errorLog);


	internal enum RunningCompilation
	{
		None,
		Background,
		Requested,
		RequestedAll,
		Manual,
	}


	public class Project
	{
		private MainForm _myForm;
		public MainForm MyForm {
			get { return _myForm; }
		}

		private Compilation[] _workCompilations;

		private Compilation[] _compilations;
		public IList<Compilation> Compilations {
			get { return _compilations; }
		}

		public Compilation Compilation {
			get {
				if (_compilations != null && _selectedProject >= 0 && _selectedProject < _compilations.Length) {
					return _compilations[_selectedProject];
				} else {
					return null;
				}
			}
		}


		private CompilationType _compilationType = CompilationType.Output;
		public CompilationType CompilationType {
			get { return _compilationType; }
			set { _compilationType = value; }
		}


		bool _createDataFile;
		public bool CreateDataFile {
			get { return _createDataFile; }
			set { _createDataFile = value; }
		}


		private String _solution;
		public String Solution {
			get { return _solution; }
			set {
				if (_solution != value) {
					_solution = value;
					ClearSolution();
					ExpandSolution();
					UpdateCaption();
				}
			}
		}

		public bool ValidSolution {
			get { return !String.IsNullOrEmpty(_solution);  }
		}

		private String _lastProject;

		private String _solutionUserName;
		public String SolutionUserName {
			get { return String.IsNullOrEmpty(_solutionUserName) ? _solution : _solutionUserName; }
			set {
				if (_solutionUserName != value) {
					_solutionUserName = value;
					UpdateCaption();
				}
			}
		}

		private void UpdateCaption() {
			if (ValidSolution) {
				_myForm.Text = new KrkalPath(SolutionUserName, KrkalCompiler.Compiler.CustomSyntax.FileExtensions).ShortWithExtension + _myForm.Caption2;
			} else {
				_myForm.Text = _myForm.Caption;
			}
		}


		public event EventHandler<EventArgs> SelectedProjectChanged;

		private int _workSelectedProject;

		private int _selectedProject;
		public int SelectedProject {
			get { return _selectedProject; }
			set {
				if (_selectedProject != value) {
					_selectedProject = value;
					var handler = SelectedProjectChanged;
					if (handler != null)
						handler(this, EventArgs.Empty);
				}
			}
		}

		private IList<String> _workProjects;
		private IList<String> _projects;
		public IList<String> Projects {
			get { return _projects; }
		}

		private RunningCompilation _compilationRequested;
		internal RunningCompilation CompilationRequested {
			get { return _compilationRequested; }
			set { _compilationRequested = value; }
		}

		Puzzle.SourceCode.Language _puzzleLanguage;
		public Puzzle.SourceCode.Language PuzzleLanguage {
			get { return _puzzleLanguage; }
		}



		private ErrorLog _workErrorLog;
		private ErrorLog _errorLog;
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}


		// CONSTRUCTOR
		public Project(MainForm myForm) {
			_myForm = myForm;
			string synFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "KrkalC.syn");
			_puzzleLanguage = new Puzzle.SourceCode.SyntaxLoader().Load(synFile);
		}




		private void ClearSolution() {
			_myForm.SolutionExplorer.ClearSyntax();
			_myForm.ErrorListWindow.ClearSyntax();
			_myForm.NameView.ClearSyntax();
			_myForm.ClassView.ClearSyntax();
			_myForm.ClearSyntax();
			_compilations = null;
			_lastProject = null;
		}



		internal void UpdateSolution() {
			_compilations = _workCompilations;
			_errorLog = _workErrorLog;
			Compilation compilation = _workCompilations[_workSelectedProject];
			if (_lastProject != compilation.RootFile || compilation.Changed || _compilationRequested == RunningCompilation.Manual) {

				_lastProject = compilation.RootFile;
				_myForm.SolutionExplorer.UpdateSyntax(_compilations);
				_myForm.ErrorListWindow.UpdateSyntax(_errorLog);
				_myForm.NameView.UpdateSyntax(compilation);
				_myForm.ClassView.UpdateSyntax(compilation);
				_myForm.UpdateSyntax();

				if (_compilationRequested == RunningCompilation.Manual) {
					if (_workErrorLog.ErrorCount == 0) {
						_myForm.StatusText = "Compilation succeeded.";
					} else {
						_myForm.StatusText = "Compilation failed.";
					}

					CompilationEndedHandler h2 = CompilationEnded;
					if (h2 != null)
						h2(_errorLog);
				}

			}

			_compilationRequested = RunningCompilation.None;
		}


		public void ExpandSolution() {
			if (ValidSolution) {
				IExpandedSolution expSolution = _myForm.IdeHelper.ExpandSolution(_solution);
				_projects = expSolution.Projects;
				SolutionUserName = expSolution.SolutionUserName;
				_selectedProject = expSolution.SelectedProject;
			} else {
				_projects = null;
			}
		}



		public event CompilationStartedHandler CompilationStarted;
		public event CompilationEndedHandler CompilationEnded;
		public event EventHandler<CompilerMessageEventArgs> CompilerAssyncMessage;

		public void Compile(bool rebuildAll) {
			if (_myForm.backgroundCompilation.IsBusy) {
				if (_compilationRequested != RunningCompilation.Manual) {
					if (rebuildAll) {
						_compilationRequested = RunningCompilation.RequestedAll;
					} else {
						_compilationRequested = RunningCompilation.Requested;
					}
				}
				return;
			}

			_myForm.SaveAll();

			if (ValidSolution) {
				ExpandSolution();
				if (_projects != null && _projects.Count > 0) {
					_compilationRequested = RunningCompilation.Manual;
					_myForm.StatusText = "Compiling ...";

					CompilationStartedHandler h1 = CompilationStarted;
					if (h1 != null)
						h1(_compilationType, rebuildAll);

					RunWorker(true, rebuildAll);
				} else {
					ClearSolution();
				}
			} else {
				ClearSolution();
			}

		}



		public void CompileInBackGround() {
			if (_myForm.backgroundCompilation.IsBusy)
				return;

			if (ValidSolution && _projects != null && _projects.Count > 0) {
				_compilationRequested = RunningCompilation.Background;

				RunWorker(false, false);				

			}
		
		}


		private void RunWorker(bool isManual, bool rebuildAll) {
			_workProjects = _projects;
			if (_myForm.DoBackGroundCompilation) {
				_myForm.backgroundCompilation.RunWorkerAsync(new bool[] {isManual, rebuildAll});
			} else {
				InnerCompile(isManual, rebuildAll);
				UpdateSolution();
			}
		}



		internal void InnerCompile(bool isManual, bool rebuildAll) {
			_workCompilations = new Compilation[_workProjects.Count];
			_workSelectedProject = _selectedProject;
			if (_workSelectedProject < 0 || _workSelectedProject >= _workCompilations.Length) {
				_selectedProject = _workSelectedProject = 0;
			}

			CompilationType workCompilationtype = _compilationType;
			bool workCreateData = _createDataFile;

			for (int f=0; f< _workCompilations.Length; f++) {
				if (isManual) {
					_workCompilations[f] = new Compilation(_workProjects[f], workCompilationtype, CompilerMessage);
				} else if (f == _workSelectedProject) {
					_workCompilations[f] = new Compilation(_workProjects[f], CompilationType.Semantical);
				} else {
					_workCompilations[f] = new Compilation(_workProjects[f], CompilationType.FileListOnly);
				}
				_workCompilations[f].RebuildAll = rebuildAll;
			}

			foreach (Compilation compilation in _workCompilations) {
				compilation.Compile();
				if (isManual && workCreateData && compilation.ErrorLog.ErrorCount == 0 && compilation.SourceFiles.RootFile.Lexical.Header.GeneratesData) {
					_myForm.IdeHelper.GenerateData(compilation);
				}
				compilation.OutputMessage(String.Format(CultureInfo.CurrentCulture, "---- {0} errors, {1} warnings ----\n", compilation.ErrorLog.ErrorCount, compilation.ErrorLog.WarningCount));
			}

			if (_workCompilations.Length == 1) {
				_workErrorLog = _workCompilations[0].ErrorLog;
			} else {
				_workErrorLog = new ErrorLog();
				foreach (Compilation compilation in _workCompilations) {
					_workErrorLog.AddErrors(compilation.ErrorLog);
				}
			}
		}



		private void CompilerMessage(object sender, CompilerMessageEventArgs e) {
			EventHandler<CompilerMessageEventArgs> h = CompilerAssyncMessage;
			if (h != null)
				h(sender, e);
		}
	}
}
