//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C o m p i l a t i o n
///
///		Main class that is responsible for the whole compilation
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Krkal.Compiler
{

	public enum CompilationType
	{
		FileListOnly,
		Semantical,
		Methods,
		Output,
	}



	public interface ICodeGenerator : IDisposable
	{
		bool IsOutputUpToDate(Compilation compilation);
		void GenerateMethod(Compilation compilation, MethodAnalysis methodAnalysis);
		void GenerationDone(Compilation compilation);
	}

	public class CompilerMessageEventArgs : EventArgs
	{
		String _message;
		public String Message {
			get { return _message; }
		}
		
		public CompilerMessageEventArgs(String message) {
			_message = message;
		}
	}




	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public class Compilation
	{
		private SourceFilesCollection _sourceFiles;
		public SourceFilesCollection SourceFiles {
			get { return _sourceFiles; }
		}

		private AttributeDefinitions _attributeDefinitions;
		internal AttributeDefinitions AttributeDefinitions {
			get { return _attributeDefinitions; }
		}

		private Dictionary<KsidName,AttributeField> _globalAttributes;
		public ICollection<AttributeField> GlobalAttributes {
			get { return _globalAttributes == null ? null : _globalAttributes.Values; }
		}

		private Dictionary<Field, ICollection<AttributeField>> _fieldAttributes = new Dictionary<Field, ICollection<AttributeField>>();

		private String _rootFile;
		public String RootFile {
			get { return _rootFile; }
		}

		private ErrorLog _errorLog = new ErrorLog();
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}

		private int _errorCountBeforeMethodCompilation = -1;

		private int _errorCountAfterTypeAnalysis = -1;
		public int ErrorCountAfterTypeAnalysis {
			get { return _errorCountAfterTypeAnalysis; }
		}

		bool _changed;
		public bool Changed {
			get { return _changed; }
		}

		private CompilationType _compilationType;
		public CompilationType CompilationType {
			get { return _compilationType; }
		}

		private bool _rebuildAll;
		public bool RebuildAll {
			get { return _rebuildAll; }
			set { _rebuildAll = value; }
		}

		private KrkalCompiler _compiler = KrkalCompiler.Compiler; // initilaizes the Compiler singleton
		public KrkalCompiler Compiler {
			get { return _compiler; }
		}


		private KsidNamesEx _ksidNames;
		public KsidNamesEx KsidNames {
			get {
				if (_ksidNames == null) {
					_ksidNames = new KsidNamesEx(this);
				}
				return _ksidNames; 
			}
		}

		private CompilationStep _step;

		private int _classFieldCounter;

		private KsidName[][] _knownNames;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<IList<KsidName>> KnownNames {
			get { return _knownNames; }
		}

		public KsidName CompilerKnownName(CompilerKnownName nameIndex) {
			return _knownNames[0][(int)nameIndex];
		}

		private Dictionary<String, SystemMethod> _systemMethods = new Dictionary<string,SystemMethod>();

		public event EventHandler<CompilerMessageEventArgs> CompilerAssyncMessage;


		CustomSyntax _customSyntax;
		public CustomSyntax CustomSyntax {
			get { return _customSyntax; }
		}


		NameTable _knownNamesNameTable;
		internal NameTable KnownNamesNameTable {
			get {
				if (_knownNamesNameTable == null)
					_knownNamesNameTable = new NameTable();
				return _knownNamesNameTable; 
			}
		}

		// CONSTRUCTOR

		public Compilation(String file, CompilationType type) 
			: this(file, type, null)
		{}

		public Compilation(String file, CompilationType type, EventHandler<CompilerMessageEventArgs> compilerAssyncMessageHandler) {
			_rootFile = file;
			_compilationType = type;
			CompilerAssyncMessage = compilerAssyncMessageHandler;
		}


		public bool Compile() {
			try {
				Compile2();
			}
			catch (CompilerException) {
			}

			_errorLog.JoinErrors();

			return (_errorLog.ErrorCount == 0); 
		}

		private void Compile2() {
			OutputMessage(String.Format("\n---- Compiling {0} ----\n", _rootFile));
			OutputMessage("Building include tree.\n");
			_sourceFiles = new SourceFilesCollection(_errorLog, _rootFile, _rebuildAll);
			if (_compilationType == CompilationType.FileListOnly) {
				_changed = true;
				return;
			}

			Compilation cachedCompilation;
			if (!_rebuildAll && (cachedCompilation = KrkalCompiler.Compiler.CompilationCacheGet(_rootFile)) != null && cachedCompilation.SourceFiles.IsUnchanged(_sourceFiles) && (_compilationType == CompilationType.Semantical || cachedCompilation.AreAttributeDefinitionsUnchanged())) {
				CopyMembers(cachedCompilation);
			} else {
				_changed = true;
				_customSyntax = new CustomSyntax(this);
				DoDeclarations();
			}
			KrkalCompiler.Compiler.CompilationCacheAdd(_rootFile, this);

			DeclareKnownNames();
			DeclareKsidNames();
			DoEnums();
			AddTypesToKsids();
			DoDependencies();
			ResetNames();
			DoDataObjects();
			DoClasses();
			InheritNames();
			AddToWorkingCompilationCache();

			if (CompilationType == CompilationType.Semantical || _errorCountAfterTypeAnalysis != 0)
				return;

			CompileInitializations();
			CompileAttributes();
			GenerateCode();
		}














		private void DoDeclarations() {
			if (_step < CompilationStep.DoDeclarations) {
				OutputMessage("Parsing declarations.\n");
				foreach (SourceFile sf in _sourceFiles) {
					sf.Lexical.DoDeclarations();
				}

				_step = CompilationStep.DoDeclarations;
			}
		}



		private void DeclareKnownNames() {
			if (_step < CompilationStep.DeclareKnownNames) {
				OutputMessage("Declaring known names.\n");

				_knownNames = new KsidName[KrkalCompiler.Compiler.CustomSyntax.KnownNames.Count + CustomSyntax.KnownNames.Count][];

				int f = 0;
				DeclareKnownNames2(KrkalCompiler.Compiler.CustomSyntax.KnownNames, ref f);
				DeclareKnownNames2(CustomSyntax.KnownNames, ref f);

				// declare System Methods
				foreach (SystemMethod method in CustomSyntax.SystemMethods) {
					_systemMethods.Add(method.Name, new SystemMethod(method, this));
				}

				_step = CompilationStep.DeclareKnownNames;
			}
		}



		private void DeclareKnownNames2(IList<IList<KnownName>> iList, ref int f) {
			foreach (IList<KnownName> names in iList) {
				_knownNames[f] = new KsidName[names.Count];

				int g = 0;
				foreach (KnownName name in names) {
					_knownNames[f][g] = KsidNames.DeclareKnownName(name);
					KnownNamesNameTable.AddKnownName(_knownNames[f][g]);
					g++;
				}

				f++;
			}
		}




		private void DeclareKsidNames() {
			if (_step < CompilationStep.DeclareKsidNames) {
				OutputMessage("Declaring names.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Field field in sf.Lexical.Declarations.FileField.Children) {
						try {
							if (field.IsNameTypeValid)
								KsidNames.FindOrAdd(field.Name, field.GetNameType(this, sf), sf);
						}
						catch (ErrorException) { }
					}
				}

				_step = CompilationStep.DeclareKsidNames;
			}
		}



		private void DoEnums() {
			if (_step < CompilationStep.DoEnums) {
				OutputMessage("Declaring enums.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Field field in sf.Lexical.Declarations.Enums) {
						try {
							EnumName enumName = (EnumName)KsidNames.Find(field.Name, NameType.Enum, sf);
							enumName.Initialize(field, sf, _ksidNames);
						}
						catch (ErrorException) { }
					}
				}

				_step = CompilationStep.DoEnums;
			}
		}



		private void AddTypesToKsids() {
			if (_step < CompilationStep.AddTypesToKsids) {
				OutputMessage("Adding types to names.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Field field in sf.Lexical.Declarations.Names) {
						AddTypeToKsid(field, sf);
					}
				}

				_step = CompilationStep.AddTypesToKsids;
			}
		}


		private void AddTypeToKsid(Field field, SourceFile sf) {
			try {
				if (field.HasLanguageType) {
					TypedKsidName name =  (TypedKsidName)KsidNames.Find(field.Name, field.GetNameType(this, sf), sf);
					name.AssignType(field, sf);
				}
			}
			catch (ErrorException) { }
		}




		private void DoDependencies() {
			if (_step < CompilationStep.DoDependencies) {
				OutputMessage("Creating names dependencies.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Dependency dependency in sf.Lexical.Declarations.Dependencies) {
						DoDependency(dependency, sf);
					}
				}

				// add dependencies to Object
				KsidName Obj = CompilerKnownName(Krkal.Compiler.CompilerKnownName.Object);
				foreach (KsidName name in KsidNames) {
					if (name.NameType == NameType.Class && Obj != name)
						Obj.Children.Add(name);
				}

				KsidName nameInCycle = KsidNames.Sort();
				if (nameInCycle != null)
					_errorLog.LogError("", ErrorCode.ECycleInNames, nameInCycle);

				_step = CompilationStep.DoDependencies;
			}
		}



		private void DoDependency(Dependency dependency, SourceFile sf) {
			try {
				KsidName[] left = CompleteArrayOfNames(dependency.Left, sf);
				KsidName[] right = CompleteArrayOfNames(dependency.Right, sf);

				Direction direction = dependency.DepentdencyOperator == OperatorType.LeftShift ? Direction.Down : Direction.Up;

				foreach (KsidName lName in left) {
					foreach (KsidName rName in right) {
						lName[direction].Edges.Add(rName);
					}
				}
			}
			catch (ErrorException) { }
		}


		private KsidName[] CompleteArrayOfNames(ICollection<WannaBeKsidName> names, SourceFile sf) {
			KsidName[] output = new KsidName[names.Count];
			int f = 0;
			foreach (WannaBeKsidName name in names) {
				output[f] = KsidNames.Find(name, sf);
				f++;
			}
			return output;
		}



		private void ResetNames() {
			if (_step < CompilationStep.DoDataObjects || _step > CompilationStep.InheritNames)
				return;
			foreach (KsidName name in KsidNames) {
				name.Reset(_step);
			}
			_classFieldCounter = 0;
		}




		private void DoDataObjects() {
			if (_step < CompilationStep.DoDataObjects) {
				OutputMessage("Declaring Data Objects.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Field field in sf.Lexical.Declarations.DataObjects) {
						try {
							DoDataObject(field, sf);
						}
						catch (ErrorException) { }
					}
				}

				foreach (KsidName name in KsidNames) {
					if (name.DataObject != null) {
						try {
							DoDataObject2(name.DataObject);
						}
						catch (ErrorException) { }
					}
				}

				_step = CompilationStep.DoDataObjects;
			}
		}


		private void DoDataObject2(DataObject dataObject) {
			dataObject.CheckClasses(_errorLog);
			
			StringBuilder sb = new StringBuilder();
			sb.Append("static void @CreateData() { \n\tobject o = new ");
			sb.Append(dataObject.Class.ToString());
			sb.Append("();\n");
			sb.Append("\t@DataObjects.AttachName(o, &");
			sb.Append(dataObject.Name.ToString());
			sb.Append(");\n");
			sb.Append("\to->");
			sb.Append(dataObject.InitializationMethod.ToString());
			sb.Append("() message; \n}");

			SyntaxTemplates syntax = new SyntaxTemplates(_errorLog, new Lexical(null, sb.ToString()));
			Field field = new Field(syntax, new Field(FieldType.Class));
			SourceFile sf = new SourceFile(syntax.Lexical);
			field.DoFieldName(true);

			((ClassName)CompilerKnownName(Krkal.Compiler.CompilerKnownName.Static)).AppendField(field, CompilerKnownName(Krkal.Compiler.CompilerKnownName.CreateData), sf, null);
		}



		private void DoDataObject(Field field, SourceFile sf) {
			ClassName className = (ClassName)KsidNames.Find(field.LTName, NameType.Class, sf);
			KsidName name = KsidNames.Find(field.Name, sf);

			if (name.DataObject == null) {
				MethodName initialization = (MethodName)KsidNames.FindOrAddHidden(name, "DOInitialization", NameType.SafeMethod, name.DeclarationPlace);
				initialization.AssignType(new LanguageType(BasicType.Void), field.Name.SourceToken);
				name.DataObject = new DataObject(name, initialization);
			}
			name.DataObject.AddClass(className);
			className.AppendField(field, name.DataObject.InitializationMethod, sf, className.Identifier);			
		}




		private void DoClasses() {
			if (_step < CompilationStep.DoClasses) {
				OutputMessage("Declaring classes.\n");

				foreach (SourceFile sf in _sourceFiles) {
					foreach (Field field in sf.Lexical.Declarations.Classes) {
						try {
							ClassName className = (ClassName)KsidNames.Find(field.Name, NameType.Class, sf);
							className.ReadClassFields(field, sf, className.Identifier);
						}
						catch (ErrorException) { }
					}
					foreach (Field field in sf.Lexical.Declarations.Enums) {
						try {
							EnumName enumName = (EnumName)KsidNames.Find(field.Name, NameType.Enum, sf);
							if (enumName.MyClass != null)
								enumName.MyClass.ReadClassFields(field, sf, enumName.Identifier);
						}
						catch (ErrorException) { }
					}
				}

				_step = CompilationStep.DoClasses;
			}
		}



		private void InheritNames() {
			if (_step < CompilationStep.InheritNames) {
				OutputMessage("Building inheritance.\n");

				foreach (KsidName name in KsidNames) {
					ClassName className = name as ClassName;
					if (className != null) {
						try {
							className.InheritNames();
						}
						catch (ErrorException) { }
					}
				}

				_step = CompilationStep.InheritNames;
			}
		}


		private void AddToWorkingCompilationCache() {
			if (_step < CompilationStep.AddToWorkingCompilationCache) {
				_changed = true;
				_errorLog.JoinErrors();
				_errorCountAfterTypeAnalysis = _errorLog.ErrorCount;
				if (_errorCountAfterTypeAnalysis == 0) {
					KrkalCompiler.Compiler.WorkingCompilationCacheAdd(_rootFile, this);
				}
				OutputMessage("Type compilation completed. " + _errorCountAfterTypeAnalysis.ToString(CultureInfo.CurrentCulture) + " errors.\n");
				_step = CompilationStep.AddToWorkingCompilationCache;
			}
		}


		private void CompileInitializations() {
			if (_step < CompilationStep.CompileInitializations) {
				OutputMessage("Compiling initializations.\n");
				foreach (KsidName name in KsidNames) {
					ClassName className = name as ClassName;
					if (className != null) {
						foreach (UniqueField field in className.UniqueNames.Values) {
							if (field.InheritedFrom == className) {
								try {
									field.DoAssignment();
								}
								catch (ErrorException) { }
							}
						}
					}
				}
				_step = CompilationStep.CompileInitializations;
			}
		}


		private void CompileAttributes() {
			if (_step < CompilationStep.CompileAttributes) {
				OutputMessage("Compiling attributes.\n");
				if (_attributeDefinitions == null)
					_attributeDefinitions = new AttributeDefinitions(this);

				foreach (SourceFile sf in _sourceFiles) {
					new AttributeAnalysis(this, null, sf.Lexical.Header.AttributesToken, sf);
					new AttributeAnalysis(this, sf.Lexical.Declarations.FileField, sf.Lexical.Declarations.FileField.Attributes, sf);
				}

				AddEngineAttribute();
	
				_step = CompilationStep.CompileAttributes;
				_errorCountBeforeMethodCompilation = _errorLog.ErrorCount;
			}
		}



		private void AddEngineAttribute() {
			if (_sourceFiles.RootFile.Lexical.Header.Engine != null) {
				ConstantValue value = new ArrayConstantValue(_sourceFiles.RootFile.Lexical.Header.Engine.ToKsidString());
				AttributeField attr = new AttributeField((TypedKsidName)CompilerKnownName(Krkal.Compiler.CompilerKnownName.Attribute_Engine), value);
				AttributeAnalysis.MergeToDictionary(attr, GetOrCreateGlobalAttributesDictionary(), _errorLog);
			}
		}


		private void GenerateCode() {
			if (_step < CompilationStep.CompileMethods || _compilationType == CompilationType.Output) {
				_changed = true;

				if (_errorCountBeforeMethodCompilation == 0 && _errorLog.ErrorCount > 0)
					_errorLog.Clear(); // clear errors from previous compilation.

				using (ICodeGenerator codeGenerator = _compilationType == CompilationType.Output ? CustomSyntax.CreateCodeGenerator(this) : null) {
					if (codeGenerator == null || !codeGenerator.IsOutputUpToDate(this)) {
						OutputMessage("Compiling methods.\n");
						DoMethods(codeGenerator);
					}
					if (codeGenerator != null && _errorLog.ErrorCount == 0 && _compilationType == CompilationType.Output)
						codeGenerator.GenerationDone(this);
				}

				if (_compilationType == CompilationType.Output) {
					_step = CompilationStep.GenerateCode;
				} else {
					_step = CompilationStep.CompileMethods;
				}
			}
		}

		private void DoMethods(ICodeGenerator codeGenerator) {
			foreach (KsidName name in KsidNames) {
				ClassName className = name as ClassName;
				if (className != null) {
					foreach (MethodField mField in className.Methods) {
						if (mField.InheritedFrom == className) {
							try {
								MethodAnalysis analysis;
								analysis = new MethodAnalysis(mField);
								if (codeGenerator != null && _errorLog.ErrorCount == 0 && _compilationType == CompilationType.Output)
									codeGenerator.GenerateMethod(this, analysis);
							}
							catch (ErrorException) { }
						}
					}
				}
			}
		}




		internal bool AreAttributeDefinitionsUnchanged() {
			return _attributeDefinitions == null || !_attributeDefinitions.AttributesCollection.IsReloadNeeded();
		}


		private void CopyMembers(Compilation cachedCompilation) {
			_errorLog = cachedCompilation._errorLog;
			_sourceFiles = cachedCompilation._sourceFiles;
			_ksidNames = cachedCompilation._ksidNames;
			if (_ksidNames != null)
				_ksidNames.Compilation = this;
			_step = cachedCompilation._step;
			_knownNames = cachedCompilation._knownNames;
			_systemMethods = cachedCompilation._systemMethods;
			_errorCountAfterTypeAnalysis = cachedCompilation._errorCountAfterTypeAnalysis;
			_errorCountBeforeMethodCompilation = cachedCompilation._errorCountBeforeMethodCompilation;
			_classFieldCounter = cachedCompilation._classFieldCounter;
			_attributeDefinitions = cachedCompilation._attributeDefinitions;
			_globalAttributes = cachedCompilation._globalAttributes;
			_fieldAttributes = cachedCompilation._fieldAttributes;
			_customSyntax = cachedCompilation._customSyntax;
			_knownNamesNameTable = cachedCompilation._knownNamesNameTable;
		}

		

		internal int GetClassFieldCounter() {
			return ++_classFieldCounter;
		}



		public void OutputMessage(String message) {
			EventHandler<CompilerMessageEventArgs> h = CompilerAssyncMessage;
			if (h != null)
				h(this, new CompilerMessageEventArgs(message));
		}


		public bool TryGetSystemMethod(String name, out SystemMethod method) {
			return _systemMethods.TryGetValue(name, out method);
		}

		internal void StoreFieldAttributes(Field field, ICollection<AttributeField> fieldAttributes) {
			_fieldAttributes.Remove(field);
			_fieldAttributes.Add(field, fieldAttributes);
		}

		public ICollection<AttributeField> TryGetFieldAttributes(Field field) {
			ICollection<AttributeField> output;
			_fieldAttributes.TryGetValue(field, out output);
			return output;
		}

		internal IDictionary<KsidName, AttributeField> GetOrCreateGlobalAttributesDictionary() {
			if (_globalAttributes == null)
				_globalAttributes = new Dictionary<KsidName, AttributeField>();
			return _globalAttributes;
		}
	}




	internal enum CompilationStep
	{
		Nothing,
		DoDeclarations,
		DeclareKnownNames,
		DeclareKsidNames,
		DoEnums,
		AddTypesToKsids,
		DoDependencies,
		DoDataObjects,
		DoClasses,
		InheritNames,
		AddToWorkingCompilationCache,
		CompileInitializations,
		CompileAttributes,
		CompileMethods,
		GenerateCode,
	}









}
