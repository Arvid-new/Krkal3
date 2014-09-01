using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Ide;
using Krkal.Runtime;
using Krkal.Compiler;
using System.Globalization;
using Krkal.FileSystem;

namespace Krkal.RunIde
{
	public class IdeHelper : IIdeHelper
	{
		KrkalApplication _application;

		RunIdeForm _myForm;
		public RunIdeForm MyForm {
			get { return _myForm; }
			set { _myForm = value; }
		}

		public string Caption {
			get { return _application.Caption; }
		}

		public string OkText {
			get { return _application.OkText; }
		}

		public string CancelText {
			get { return _application.CancelText; }
		}

		public IKrkalResourceManager ResourceManager {
			get { return _application.ResourceManager; }
		}


		IList<INewDocumentInformation> _newDocumentInformation;
		public IList<INewDocumentInformation> NewDocumentInformation {
			get { return _newDocumentInformation; }
			set { _newDocumentInformation = value; }
		}


		// CONSTRUCTOR
		internal IdeHelper(KrkalApplication application) {
			_application = application;
		}



		public IExpandedSolution ExpandSolution(string solution) {
			String solutionUserName = null;
			IList<String> ret;
			if (solution != null && solution.StartsWith("_KSI", StringComparison.Ordinal)) {
				using (RootNames rootNames = new RootNames(true)) {
					KerMain kerMain = rootNames.GetKernel();
					KerName name = kerMain.FindName(solution);
					if (!name.IsNull)
						solutionUserName = kerMain.ReadUserName(name);
				}
				ret = RootNames.GetFiles(solution, KerNameType.CompileSource, false);
			} else {
				ret = new List<string>(1);
				if (!String.IsNullOrEmpty(solution))
					ret.Add(solution);
			}
			return new ExpandedSolution(ret, solutionUserName, _myForm != null ? _myForm.StartConfigurations.GetSelectedProject(solution) : null);
		}


		IList<INewDocumentInformation> IIdeHelper.GetNewDocumentInformation() {
			return NewDocumentInformation;
		}


		internal void LoadNewFileTemplates() {
			using (RootNames rootNames = new RootNames(true)) {
				DataSource dataSource = new DataSource(rootNames.GetKernel(), _application.DataEnvironment);

				IList<KsidName> newFiles = dataSource.GetNameLayerOrSet(_application.DataEnvironment.RunIdeKnownName(RunIdeKnownNames.AllNewFileTemplates), (KerNameType)(-1), false);
				bool kcDetected = false;
				bool kcpDetected = false;
				_newDocumentInformation = new List<INewDocumentInformation>();

				foreach (KsidName name in newFiles) {
					NewFileTemplate template = new NewFileTemplate(name, dataSource);
					_newDocumentInformation.Add(template);
					if (String.Compare(template.Extension, ".kc", true, CultureInfo.CurrentCulture) == 0)
						kcDetected = true;
					if (String.Compare(template.Extension, ".kcp", true, CultureInfo.CurrentCulture) == 0)
						kcpDetected = true;
				}

				if (!kcpDetected)
					_newDocumentInformation.Add(DefaultIdeHelper.CreateNewProjectFileInfo());
				if (!kcDetected)
					_newDocumentInformation.Add(DefaultIdeHelper.CreateNewSourceFileInfo());
			}
		}



		public void GenerateData(Compilation compilation) {
			if (compilation == null || compilation.ErrorLog.ErrorCount > 0)
				return;

			KrkalPath path = new KrkalPath(compilation.RootFile, compilation.Compiler.CustomSyntax.FileExtensions);
			if (RootNames.IsRegisterUpToDate(path.LongWithoutExtension + ".data")) {
				compilation.OutputMessage("Data File is up to date. Skipping data file generation.\n");
			} else {
				compilation.OutputMessage("Generating data file:\n");
				try {
					using (RuntimeErrorConvertor convertor = new RuntimeErrorConvertor()) {
						using (KernelParameters parameters = new KernelParameters(path.LongWithoutExtension + ".code", KernelRunMode.CreateData, KernelDebugMode.Debug)) {
							parameters.OnError += new KerLoggingCallBackDelegate(convertor.WriteLine);
							convertor.ErrorMessage += delegate(object sender, RuntimeErrorEventArgs e) {
								compilation.OutputMessage(e.WholeMessage);
							};
							using (KerMain kerMain = new KerMain(parameters)) {
								kerMain.Load();
								kerMain.RunTurn(1, false);
							}
						}
					}
				}
				catch (KernelPanicException) {
				}
			}
		}


		public void ShowErrorMessage(string message) {
			_application.ShowErrorMessage(message);
		}


	}








	[DataObjectClass("_KSID_NewFileTemplate")]
	public class NewFileTemplate : INewDocumentInformation
	{
		String _name;
		public String Name {
			get { return _name; }
		}

		String _menuName;
		public String MenuName {
			get { return _menuName; }
		}

		String _extension;
		[DataObjectMapping("_KSID_NewFileTemplate__M_Extension", BasicType.Char, 1)]
		public String Extension {
			get { return _extension; }
			set { _extension = value; }
		}

		String _content;
		[DataObjectMapping("_KSID_NewFileTemplate__M_Content", BasicType.Char, 1)]
		public String Content {
			get { return _content; }
			set { _content = value; }
		}


		KsidName _ksidname;
		public KsidName KsidName {
			get { return _ksidname; }
		}

		// CONSTRUCTOR
		public NewFileTemplate(KsidName name, DataSource dataSource) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (dataSource == null)
				throw new ArgumentNullException("dataSource");

			_ksidname = name;
			dataSource.LoadObject(this, name);
			if (_extension == null)
				_extension = String.Empty;

			_name = dataSource.Kernel.ReadUserName(name);
			_menuName = dataSource.Kernel.ReadComment(name);
			if (String.IsNullOrEmpty(_menuName))
				_menuName = name.Identifier.LastPart.Name;
		}




	}

}
