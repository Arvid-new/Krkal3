//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - C p p C o m p i l e r
///
///		Communicates with MSBuild, starts and logs C++ compilation
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

//using Microsoft.Build.BuildEngine;
//using Microsoft.Build.Framework;
using Krkal.Compiler;
//using Microsoft.Build.Utilities;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Utilities;
using Microsoft.Build.Logging;


namespace Krkal.CodeGenerator
{
	class CppCompiler
	{
		Generator _generator;


		internal CppCompiler(Generator generator) {
			_generator = generator;
			if (!Build2()) {
				_generator.Compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FCodeGenerationFailed);
			}
		}

		private bool Build2() {
			_generator.Compilation.OutputMessage("Running Cpp compiler ...\n\n");

			Process process = new Process();
			process.StartInfo.FileName = Path.Combine(_generator.MSBuildPath, @"MSBuild.exe");
			process.StartInfo.WorkingDirectory = _generator.MSBuildPath;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;
			
			String path = null;
			_generator.FS.GetFullPath(_generator.CppSourcePath, ref path);
			string target = _generator.Compilation.RebuildAll ? "Rebuild" : "Build";


			process.StartInfo.Arguments = String.Format(@"""{0}\Krkal.KS.sln"" /t:{1} /p:Configuration={2}", path, target, _generator.Configuration);
			_generator.Compilation.OutputMessage(process.StartInfo.Arguments + '\n');

			process.ErrorDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
			process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);

			try {
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();




				process.WaitForExit();
				return (process.ExitCode == 0);
			}
			catch (Exception e) {
				_generator.Compilation.OutputMessage(String.Format("Exception Occured: {0}\n", e.Message));
				return false;
			}

		}

		void process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
			if (e.Data != null)
				_generator.Compilation.OutputMessage(e.Data + "\n");
		}



		private bool Build() {
			_generator.Compilation.OutputMessage("Running Cpp compiler ...\n\n");

            ProjectCollection collection = new ProjectCollection(ToolsetDefinitionLocations.Registry);

            collection.SetGlobalProperty("Configuration", _generator.Configuration);

			String path = null;
			_generator.FS.GetFullPath(_generator.CppSourcePath, ref path);

			FileLogger logger = new FileLogger();
			logger.Parameters = @"logfile=" + path + @"\build.log";
			collection.RegisterLogger(logger);
			MyLogger myLogger = new MyLogger(_generator.Compilation);
			collection.RegisterLogger(myLogger);

            Project project = collection.LoadProject(path + @"\Krkal.KS.vcxproj");

            bool success = project.Build(_generator.Compilation.RebuildAll ? "Rebuild" : "Build");

			collection.UnregisterAllLoggers();

			return success;
		}
	}







	// This logger will derive from the Microsoft.Build.Utilities.Logger class,
	// which provides it with getters and setters for Verbosity and Parameters,
	// and a default empty Shutdown() implementation.
	internal class MyLogger : Logger
	{
		private Compilation _compilation;

		public MyLogger(Compilation compilation) {
			_compilation = compilation;
		}


		/// <summary>
		/// Initialize is guaranteed to be called by MSBuild at the start of the build
		/// before any events are raised.
		/// </summary>
		public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource) {
			if (eventSource == null)
				throw new ArgumentNullException("eventSource");

			// For brevity, we'll only register for certain event types. Loggers can also
			// register to handle TargetStarted/Finished and other events.
			//eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
			//eventSource.TaskStarted += new TaskStartedEventHandler(eventSource_TaskStarted);
			//eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
			//eventSource.BuildStarted += new BuildStartedEventHandler(eventSource_BuildStarted);
			//eventSource.BuildFinished += new BuildFinishedEventHandler(eventSource_BuildFinished);
			//eventSource.StatusEventRaised += new BuildStatusEventHandler(eventSource_StatusEventRaised);
			
			//eventSource.AnyEventRaised += new AnyEventHandler(eventSource_AnyEventRaised);

			eventSource.MessageRaised += new Microsoft.Build.Framework.BuildMessageEventHandler(eventSource_AnyEventRaised);
			eventSource.WarningRaised += new Microsoft.Build.Framework.BuildWarningEventHandler(eventSource_WarningRaised);
			eventSource.ErrorRaised += new Microsoft.Build.Framework.BuildErrorEventHandler(eventSource_ErrorRaised);
		
		}

		void eventSource_AnyEventRaised(object sender, Microsoft.Build.Framework.BuildEventArgs e) {
			if (String.Compare(e.SenderName, "MSBuild", true, CultureInfo.CurrentCulture) == 0)
				return;
			WriteLine(e.Message);
		}


		void eventSource_ErrorRaised(object sender, Microsoft.Build.Framework.BuildErrorEventArgs e) {
			if (String.Compare(e.SenderName, "MSBuild", true, CultureInfo.CurrentCulture) == 0)
				return;
			WriteLine(FormatErrorEvent(e));
		}

		void eventSource_WarningRaised(object sender, Microsoft.Build.Framework.BuildWarningEventArgs e) {
			if (String.Compare(e.SenderName, "MSBuild", true, CultureInfo.CurrentCulture) == 0)
				return;
			WriteLine(FormatWarningEvent(e));
		}

		private void WriteLine(string str) {
			_compilation.OutputMessage(str + "\n");
		}


		/// <summary>
		/// Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all 
		/// events have been raised.
		/// </summary>
		public override void Shutdown() {
		}


    }




}
