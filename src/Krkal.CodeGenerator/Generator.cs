//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - G e n e r a t o r
///
///		Main code genarator class
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

using Krkal.Compiler;
using Krkal.FileSystem;

namespace Krkal.CodeGenerator
{

	internal enum SystemMethodTag
	{
		None,
		AddCodeLineAndKerMain,
		Clone,
	}


	internal enum GeneratorKnownName
	{
		Enum,
		Enum_Class,
		Enum_Name,
		Enum_Type,
	}

	
	public class Generator : ICodeGenerator
	{

		FS _fs;
		public FS FS {
			get { return _fs; }
		}

		bool _disposed;
		public bool Disposed {
			get { return _disposed; }
		}

		Compilation _compilation;
		public Compilation Compilation {
			get { return _compilation; }
		}

		bool _isUpToDate;
		public bool IsOutputUpToDate(Compilation compilation) {
			return _isUpToDate;
		}

		bool _runCppCompiler;
		
		String _msBuildPath;
		public String MSBuildPath {
			get { return _msBuildPath; }
		}

		String _configuration;
		public String Configuration {
			get { return _configuration; }
		}

		String _projectName;
		public String ProjectName {
			get { return _projectName; }
		}

		FSRegisterFile _code;
		public FSRegisterFile Code {
			get { return _code; }
		}

		String _cppSourcePath;
		public String CppSourcePath {
			get { return _cppSourcePath; }
		}

		String _codePath;
		public String CodePath {
			get { return _codePath; }
		}

		String _compilationId;

		KSWriter _ksWriter;

		KsidName[] _knownNames;

		public Generator(Compilation compilation, bool runCppCompiler, String msBuildPath, String configuration, String ksSource) {
			if (String.IsNullOrEmpty(ksSource))
				throw new ArgumentException("Inalid KS Source parameter.");
			//compilation.OutputMessage("Initializing output.\n");
			_fs = FS.FileSystem;
			_fs.AddVersionFeature(FSVersionFeature.All, null, "Krkal", 1);
			_compilation = compilation;
			_runCppCompiler = runCppCompiler;
			_msBuildPath = msBuildPath;
			_configuration = configuration;
			InitProjectName();
			InitCppSourceDirectory(ksSource);
			if (!_isUpToDate) {
				_compilationId = KrkalPath.GenerateVersion();
				InitKnownNames();
				_ksWriter = new KSWriter(_cppSourcePath);
			}
		}




		~Generator() {
			Dispose(false);
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "disposing")]
		private void Dispose(bool disposing) {
			if (_code != null) {
				_code.Dispose();
				_code = null;
			}
			_disposed = true;
		}


		private void InitKnownNames() {
			_knownNames = new KsidName[_knownNamesDefs.Count];
			for (int f = 0; f < _knownNamesDefs.Count; f++ ) {
				_knownNames[f] = _compilation.KsidNames[_knownNamesDefs[f].Identifier];
			}
		}

		internal KsidName GetKnownName(GeneratorKnownName name) {
			return _knownNames[(int)name];
		}


		private void InitProjectName() {
			KrkalPath path = new KrkalPath(_compilation.RootFile, new String[] { ".kc", ".kcp" });
			_projectName = path.LongWithoutExtension;
		}



		public void GenerateMethod(Compilation compilation, MethodAnalysis methodAnalysis) {
			_ksWriter.GenerateMethod( methodAnalysis);
		}

		public void GenerationDone(Compilation compilation) {
			if (compilation == null)
				throw new ArgumentNullException("compilation");
			if (!_isUpToDate) {
				compilation.OutputMessage("Generating output.\n");
				GenerateCodeFile();
				_ksWriter.GenerateKSFiles(compilation);
			}
			CompileCppOutput();
		}





		private void GenerateCodeFile() {
			TypeInfoWriter writer = new TypeInfoWriter(_code.Reg, _compilation, this);
			_code.WriteVersionFeatures();
			_code.Reg.AddKey("Unique Compilation Id", FSRegKeyType.String).StringWrite(_compilationId);
			writer.WriteProjectFiles();
			writer.WriteOrdinalStarts();
			writer.WriteKsidNames();
			writer.WriteDependencies();
			writer.WriteStaticConstants();
			writer.WriteGlobalAttributes();
			try {
				_code.WriteFile();
				_compilation.OutputMessage(" -> '" + _codePath + "'.\n");
			}
			catch (FSFileNotFoundException) {
				_compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _codePath);
			}
		}




		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Krkal.CodeGenerator.CppCompiler")]
		private void CompileCppOutput() {
			if (_runCppCompiler) {
				new CppCompiler(this);
				CopyOutput();
			}
		}





		private void InitCppSourceDirectory(String ksSource) {
			_cppSourcePath = @"$CPPSOURCE$\" + _projectName;
			if (_fs.FileExist(_cppSourcePath) == 0) {
				_compilation.OutputMessage("Creting workspace in CppSource.\n");
				if (_fs.CreateDir(_cppSourcePath) == 0)
					_compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _cppSourcePath);
				if (_fs.CopyTree("$CPPSOURCE$\\" + ksSource + "\\*", _cppSourcePath, 0, null, 0) == 0)
					_compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _cppSourcePath);
			}

			_codePath = _cppSourcePath + '\\' + _projectName + ".code";
			CheckIfItIsUpToDate();

			if (!_isUpToDate) {
				_code = new FSRegisterFile(_codePath, "KRKAL3 SCRIPT CODE", true);
				if (_code.OpenError != FSRegOpenError.OK)
					_compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, _codePath);
			}
		}



		private void CheckIfItIsUpToDate() {
			_isUpToDate = false;
			if (_compilation.RebuildAll)
				return;

			if (_fs.FileExist(_cppSourcePath + @"\Script.cpp") != 1)
				return;
			if (_fs.FileExist(_cppSourcePath + @"\Script.h") != 1)
				return;

			using (FSRegisterFile code = new FSRegisterFile(_codePath, "KRKAL3 SCRIPT CODE")) {
				if (code.OpenError != FSRegOpenError.OK)
					return;

				FSRegKey fileName = code.Reg.FindKey("Source Files");
				FSRegKey fileDate = code.Reg.FindKey("Source Files Dates");
				if (fileName.IsNull || fileDate.IsNull)
					return;
				if (_compilation.SourceFiles.Count != fileDate.Top / 2)
					return;

				// if the compilation is unchanged the order of source files should be the same.
				foreach (SourceFile sf in _compilation.SourceFiles) {
					System.Runtime.InteropServices.ComTypes.FILETIME fileTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
					if (FS.FileSystem.GetfileTime(sf.File, ref fileTime) == 0)
						return;
					String full = null;
					if (FS.FileSystem.GetFullPath(sf.File, ref full, FSFullPathType.InvariantKey) == 0)
						return;
					
					if (String.CompareOrdinal(full, fileName.StringRead()) != 0)
						return;
					if (fileTime.dwLowDateTime != fileDate.ReadI())
						return;
					if (fileTime.dwHighDateTime != fileDate.ReadI())
						return;
				}

			}

			_compilation.OutputMessage("Output files are up to date. Skipping compilation.\n");
			_isUpToDate = true;
		}



		private void CopyOutput() {
			_compilation.OutputMessage("\n");

			String dest = @"$SCRIPTS$\" + _projectName + ".code";
			CopyFile(dest, _codePath, 1);

			dest = @"$KSBIN$\" + _projectName + ".dll";
			String source = _cppSourcePath + '\\' + _configuration + @"\Krkal.KS.dll";
			CopyFile(dest, source, 0);

		}

		private void CopyFile(string dest, string source, int compressMode) {
			try {
				int fileSize = _fs.GetFileSize(source);
				if (fileSize <= 0)
					throw new FSFileNotFoundException();
				byte[] buffer = new byte[fileSize];
				if (1 != _fs.ReadFile(source, buffer))
					throw new FSFileNotFoundException();
				if (1 != _fs.WriteFile(dest, buffer, fileSize, compressMode))
					throw new FSFileNotFoundException();
				System.Runtime.InteropServices.ComTypes.FILETIME ft = new System.Runtime.InteropServices.ComTypes.FILETIME();
				if (1 != _fs.GetfileTime(source, ref ft))
					throw new FSFileNotFoundException();
				if (1 != _fs.SetFileTime(dest, ft))
					throw new FSFileNotFoundException();
			}
			catch (FSFileNotFoundException) {
				_compilation.ErrorLog.ThrowAndLogFatalError("", ErrorCode.FUnableToCreateOutputFile, dest);
			}

			_compilation.OutputMessage(" -> '" + dest + "'.\n");
		}




		public static void AddSystemMethods(CustomSyntax cs) {
			
			SystemMethod method;

			// ARRAY

			method = new SystemMethod("AddFirst", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("item", SystemMethodTemplateType.ArrayMember);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("AddLast", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("item", SystemMethodTemplateType.ArrayMember);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("AddRangeFirst", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("array", SystemMethodTemplateType.SameConstO);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("AddRangeLast", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("array", SystemMethodTemplateType.SameConstO);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Insert", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("index", BasicType.Int);
			method.AddArgument("item", SystemMethodTemplateType.ArrayMember);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("InsertRange", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("index", BasicType.Int);
			method.AddArgument("array", SystemMethodTemplateType.SameConstO);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("RemoveFirst", OperatorRequieredArguments.Array, SystemMethodTemplateType.ArrayMember, false);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("RemoveLast", OperatorRequieredArguments.Array, SystemMethodTemplateType.ArrayMember, false);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("SetCount", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("value", BasicType.Int);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Reserve", OperatorRequieredArguments.Array, BasicType.Void, false);
			method.AddArgument("capacity", BasicType.Int);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Clear", OperatorRequieredArguments.Array, BasicType.Void, false);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("GetCapacity", OperatorRequieredArguments.Array, BasicType.Int, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("GetCount", OperatorRequieredArguments.Array, BasicType.Int, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("IsLocked", OperatorRequieredArguments.Array, BasicType.Int, true);
			cs.SystemMethods.Add(method);


			////

			method = new SystemMethod("ToString", OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Name, SystemMethodTemplateType.None, new LanguageType(BasicType.Char, Modifier.None, 1, null), null, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("ToShortString", OperatorRequieredArguments.Name, SystemMethodTemplateType.None, new LanguageType(BasicType.Char, Modifier.None, 1, null), null, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Type", OperatorRequieredArguments.Object, BasicType.Name, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Lives", OperatorRequieredArguments.Object, BasicType.Int, true);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Clone", OperatorRequieredArguments.Object | OperatorRequieredArguments.Array, SystemMethodTemplateType.SameRemoveConstO, true);
			method.Tag = SystemMethodTag.Clone;
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Compare", OperatorRequieredArguments.Object | OperatorRequieredArguments.Array, BasicType.Int, true);
			method.AddArgument("other", SystemMethodTemplateType.SameConstAll);
			cs.SystemMethods.Add(method);

			method = new SystemMethod("Kill", OperatorRequieredArguments.Object, BasicType.Void, false);
			method.Tag = SystemMethodTag.AddCodeLineAndKerMain;
			cs.SystemMethods.Add(method);
		}



		static List<KnownName> _knownNamesDefs;

		public static void AddKnownNames(CustomSyntax customSyntax) {
			if (_knownNamesDefs == null) {
				List<KnownName> names = new List<KnownName>();
				names.Add(new KnownName("_KSID_Enum", NameType.Class));
				names.Add(new KnownName("_KSID_Enum__M_Class", NameType.Variable, new LanguageType(BasicType.Name, Modifier.Public), null));
				names.Add(new KnownName("_KSID_Enum__M_Name", NameType.Variable, new LanguageType(BasicType.Name, Modifier.Public), null));
				names.Add(new KnownName("_KSID_Enum__M_Type", NameType.Variable, new LanguageType(BasicType.Int, Modifier.Public), null));
				_knownNamesDefs = names;
			}
			customSyntax.AddKnownNamesCollection(_knownNamesDefs);
		}
	}

}
