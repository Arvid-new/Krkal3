using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.RunIde
{


	public enum StepsToDo
	{
		CreateDataFile,
		CompileOutput,
		GenerateOutput,
		CompileMethods,
		TypesOnly,
	}

	public partial class Builder : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		private RunIdeForm _myForm;

		public StepsToDo StepsToDo {
			get { return (StepsToDo)stepsToDo.SelectedIndex; }
			set { stepsToDo.SelectedIndex = (int)value; }
		}

		public String Configuration {
			get { return configuration.Text; }
			set {
				if (String.Compare(value, "Debug", true, CultureInfo.CurrentCulture) == 0) {
					configuration.SelectedIndex = 0;
				} else {
					configuration.SelectedIndex = 1;
				}
			}
		}

		public String BinPath {
			get { return binPath.Text; }
			set { binPath.Text = value; }
		}

		public Builder(RunIdeForm myForm) {
			InitializeComponent();
			_myForm = myForm;
			stepsToDo.SelectedIndex = (int)StepsToDo.CreateDataFile;
#if DEBUG 
			configuration.SelectedIndex = 0;
#else
			configuration.SelectedIndex = 1;
#endif

			_myForm.Project.CompilationStarted += new Krkal.Ide.CompilationStartedHandler(Project_CompilationStarted);
			_myForm.Project.CompilationEnded += new Krkal.Ide.CompilationEndedHandler(Project_CompilationEnded);
			_myForm.Project.CompilerAssyncMessage += new EventHandler<CompilerMessageEventArgs>(Project_CompilerAssyncMessage);
		}


		void Project_CompilationEnded(ErrorLog errorLog) {
			if (errorLog == null) {
                Project_CompilerAssyncMessage2("Canceled.\n");
			} else {
				Project_CompilerAssyncMessage2(String.Format(CultureInfo.CurrentCulture, "==== Done. {0} errors. {1} warnings. ====\n", errorLog.ErrorCount, errorLog.WarningCount));
			}
		}

		void Project_CompilationStarted(CompilationType compilationType, bool rebuildAll) {
			output.Clear();
			if (_myForm.Project.ValidSolution) {
				if (rebuildAll) {
                    Project_CompilerAssyncMessage2("==== Rebuilding '" + _myForm.Project.Solution + "' ====\n");
				} else {
                    Project_CompilerAssyncMessage2("==== Compiling '" + _myForm.Project.Solution + "' ====\n");
				}
			}
		}

		delegate void CompilerAssyncMessage2Delegate(String message);
		void Project_CompilerAssyncMessage2(String message) {
			output.AppendText(message);
            output.ScrollToCaret();
		}

		void Project_CompilerAssyncMessage(object sender, CompilerMessageEventArgs e) {
			if (InvokeRequired) {
				BeginInvoke(new CompilerAssyncMessage2Delegate(Project_CompilerAssyncMessage2), e.Message);
			} else {
				Project_CompilerAssyncMessage2(e.Message);
			}
		}



		private void stepsToDo_SelectedIndexChanged(object sender, EventArgs e) {
			if (stepsToDo.SelectedIndex == (int)StepsToDo.TypesOnly) {
				_myForm.Project.CompilationType = Krkal.Compiler.CompilationType.Semantical;
			} else if (stepsToDo.SelectedIndex == (int)StepsToDo.CompileMethods) {
				_myForm.Project.CompilationType = Krkal.Compiler.CompilationType.Methods;
			} else {
				_myForm.Project.CompilationType = Krkal.Compiler.CompilationType.Output;
			}

			_myForm.Project.CreateDataFile = (stepsToDo.SelectedIndex <= (int)StepsToDo.CreateDataFile);
		}


		private void compileButton_Click(object sender, EventArgs e) {
			_myForm.CompileProject();
		}

		private void runButton_Click(object sender, EventArgs e) {
			_myForm.RunGame();
		}



		protected override string GetPersistString() {
			return String.Format("{0},{1},{2},{3}", GetType(), StepsToDo, Configuration, BinPath);
		}

	
	}
}

