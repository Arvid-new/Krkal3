//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C u s t o m S y n t a x
///
///		Class CustomSyntax is used to configure the compiler
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Krkal.Compiler
{
	

	public enum NameType {
		Variable,
		StaticVariable,
		SafeMethod,
		DirectMethod,
		StaticSafeMethod,
		StaticDirectMethod,

		Void,
		Class,
		Param,
		Group,
		Control,
		Enum,
	}



	public enum CompilerKnownName
	{
		Static,
		Constructor,
		Destructor,
		Main,
		Object,
		CreateData,
		Everything,
		Nothing,
		Attribute_Engine,
	}


	public enum SystemMethodTemplateType
	{
		None,
		Same,
		ArrayMember,
		SameConstO,
		SameRemoveConstO,
		SameConstAll,
	}



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public delegate ICodeGenerator CreateCodeGeneratorDelegate(Compilation compilation);
	public delegate void InitializeCustomSyntax(String engine, Compilation compilation, CustomSyntax customSyntax);


	public class StaticCustomSyntax
	{

		public const String DefaultType = "Void";

		private CustomNameTypes _customNameTypes = new CustomNameTypes();
		public CustomNameTypes CustomNameTypes {
			get { return _customNameTypes; }
		}


		private CustomGroupTypes _customGroupTypes = new CustomGroupTypes();
		public CustomGroupTypes CustomGroupTypes { // warning this is common for all complilation. Index in the type info may not be valid. Search it again in compilation's custom syntax
			get { return _customGroupTypes; }
		}

		private CustomControlTypes _customControlTypes = new CustomControlTypes();
		public CustomControlTypes CustomControlTypes { // warning this is common for all complilation. Index in the type info may not be valid. Search it again in compilation's custom syntax
			get { return _customControlTypes; }
		}


		private Set<String> _mixedNamespaces = new Set<string>();
		public Set<String> MixedNamespaces {
			get { return _mixedNamespaces; }
		}


		private EnumTypes _enumTypes = new EnumTypes();
		internal EnumTypes EnumTypes {
			get { return _enumTypes; }
		}


		private List<IList<KnownName>> _knownNames;
		private ReadOnlyCollection<IList<KnownName>> _roKnownNames;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<IList<KnownName>> KnownNames {
			get { return _roKnownNames; }
		}


		private IList<String> _fileExtensions;
		public IList<String> FileExtensions {
			get { return _fileExtensions; }
			set { _fileExtensions = value; }
		}




		private InitializeCustomSyntax _initializeCustomSyntax;
		public InitializeCustomSyntax InitializeCustomSyntax {
			get { return _initializeCustomSyntax; }
			set { _initializeCustomSyntax = value; }
		}



		// CONSTRUCTOR

		internal StaticCustomSyntax() {
			InitKnownNames();
			InitFileExtensions();
			InitMixedNamespaces();
		}



		private void InitFileExtensions() {
			_fileExtensions = new List<String>();
			_fileExtensions.Add(".kc");
			_fileExtensions.Add(".kcp");
		}



		private void InitMixedNamespaces() {
			_mixedNamespaces.Add("Constructor");
			_mixedNamespaces.Add("Destructor");
		}



		private void InitKnownNames() {
			_knownNames = new List<IList<KnownName>>();
			_roKnownNames = new ReadOnlyCollection<IList<KnownName>>(_knownNames);

			List<KnownName> names = new List<KnownName>();

			names.Add(new KnownName("_KSID_Static", NameType.Class));
			names.Add(new KnownName("_KSID_Constructor", NameType.SafeMethod, BasicType.Void));
			names.Add(new KnownName("_KSID_Destructor", NameType.SafeMethod, BasicType.Void));
			names.Add(new KnownName("_KSID_Main", NameType.StaticSafeMethod, new LanguageType(BasicType.Void, Modifier.Static, 0, null), null));
			names.Add(new KnownName("_KSID_Object", NameType.Class));
			names.Add(new KnownName("_KSID_CreateData", NameType.StaticSafeMethod, new LanguageType(BasicType.Void, Modifier.Static, 0, null), null));
			names.Add(new KnownName("_KSID_Everything", NameType.Void));
			names.Add(new KnownName("_KSID_Nothing", NameType.Void));
			names.Add(new KnownName("_KSID_Attribute__M_Engine", NameType.Variable, new LanguageType(BasicType.Char, Modifier.Public | Modifier.Const1, 1, null), null));

			for (int f = 1; Enum.IsDefined(typeof(EnumSubType), (EnumSubType)f); f++) {
				String name = "_KSID_EnumType__M_" + ((EnumSubType)f).ToString();
				names.Add(new KnownName(name, NameType.StaticVariable, new LanguageType(BasicType.Int, Modifier.Public | Modifier.ConstV | Modifier.Static), null));
			}

			_knownNames.Add(new ReadOnlyCollection<KnownName>(names));
		}


		public void AddKnownNamesCollection(IList<KnownName> collection) {
			_knownNames.Add(collection);
		}

	}








	public class CustomSyntax
	{


		private CustomGroupTypes _customGroupTypes = new CustomGroupTypes();
		public CustomGroupTypes CustomGroupTypes {
			get { return _customGroupTypes; }
		}

		private CustomControlTypes _customControlTypes = new CustomControlTypes();
		public CustomControlTypes CustomControlTypes {
			get { return _customControlTypes; }
		}


		private List<IList<KnownName>> _knownNames;
		private ReadOnlyCollection<IList<KnownName>> _roKnownNames;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IList<IList<KnownName>> KnownNames {
			get { return _roKnownNames; }
		}

		private List<SystemMethod> _systemMethods = new List<SystemMethod>();
		public IList<SystemMethod> SystemMethods {
			get { return _systemMethods; }
		}



		private CreateCodeGeneratorDelegate _createCodeGeneratorDelegate;
		public CreateCodeGeneratorDelegate CreateCodeGeneratorDelegate {
			get { return _createCodeGeneratorDelegate; }
			set { _createCodeGeneratorDelegate = value; }
		}


		private IAttributesProvider _attributeProvider;
		public IAttributesProvider AttributeProvider {
			get { return _attributeProvider; }
			set { _attributeProvider = value; }
		}


		// CONSTRUCTOR

		internal CustomSyntax(Compilation compilation) {
			InitKnownNames();
			InitializeCustomSyntax initialize = compilation.Compiler.CustomSyntax.InitializeCustomSyntax;
			if (initialize != null) {
				string ksid = null;
				if (compilation.SourceFiles.RootFile.Lexical.Header.Engine != null)
					ksid = compilation.SourceFiles.RootFile.Lexical.Header.Engine.ToKsidString();
				initialize(ksid, compilation, this);
			}

			List<KnownName> ckKnownName = new List<KnownName>();

			foreach (CustomKeywordInfo info in _customControlTypes.ROTypeInfos) {
				compilation.Compiler.CustomSyntax.CustomControlTypes.AddType(info);
				if (!info.Hidden)
					ckKnownName.Add(new KnownName(info.FullName.ToKsidString(), NameType.StaticVariable, new LanguageType(BasicType.Int, Modifier.Public | Modifier.ConstV | Modifier.Static), null));
			}
			foreach (CustomKeywordInfo info in _customGroupTypes.ROTypeInfos) {
				compilation.Compiler.CustomSyntax.CustomGroupTypes.AddType(info);
				if (!info.Hidden)
					ckKnownName.Add(new KnownName(info.FullName.ToKsidString(), NameType.StaticVariable, new LanguageType(BasicType.Int, Modifier.Public | Modifier.ConstV | Modifier.Static), null));
			}
			foreach (CustomKeywordInfo info in compilation.Compiler.CustomSyntax.CustomNameTypes.ROTypeInfos) {
				if (!info.Hidden)
					ckKnownName.Add(new KnownName(info.FullName.ToKsidString(), NameType.StaticVariable, new LanguageType(BasicType.Int, Modifier.Public | Modifier.ConstV | Modifier.Static), null));
			}

			AddKnownNamesCollection(ckKnownName);
		}




		private void InitKnownNames() {
			_knownNames = new List<IList<KnownName>>();
			_roKnownNames = new ReadOnlyCollection<IList<KnownName>>(_knownNames);
		}


		public void AddKnownNamesCollection(IList<KnownName> collection) {
			_knownNames.Add(collection);
		}

		internal ICodeGenerator CreateCodeGenerator(Compilation compilation) {
			CreateCodeGeneratorDelegate del = _createCodeGeneratorDelegate;
			if (del != null) {
				return del(compilation);
			} else {
				return null;
			}
		}
	}





	public class CustomKeywordInfo
	{
		public String Name {
			get { return _fullName.LastPart.Name; }
		}

		int _index;
		public int Index {
			get { return _index; }
		}

		bool _hidden;
		public bool Hidden {
			get { return _hidden; }
		}

		Identifier _fullName;
		public Identifier FullName {
			get { return _fullName; }
		}


		// CONSTRUCTOR
		public CustomKeywordInfo(String fullName, int index, bool hidden) {
			if (fullName == null)
				throw new ArgumentNullException("name");
			if (fullName.Length == 0)
				throw new ArgumentException("Name cannot be empty");
			_fullName = Identifier.ParseKsid(fullName);
			Init(index, hidden);
		}

		public CustomKeywordInfo(Identifier fullName, int index, bool hidden) {
			if (fullName == null)
				throw new ArgumentNullException("fullName");
			_fullName = fullName;
			Init(index, hidden);
		}

		public CustomKeywordInfo(String fullName, int index) : this(fullName, index, false) { }


		private void Init(int index, bool hidden) {
			if (!hidden && _fullName.Root == IdentifierRoot.Localized)
				throw new ArgumentException("Full name cannot be localized.");
			if (Char.ToUpperInvariant(Char.ToLowerInvariant(Name[0])) != Name[0])
				throw new ArgumentException("First character of the custom name has to be upper case.");
			_index = index; _hidden = hidden;
		}

	}





	public class CustomNameTypes : CustomKeywords<CustomKeywordInfo>
	{

		// CONSTRUCTOR

		internal CustomNameTypes() : base("_KSID_NameType") {
			for (int f = 0; Enum.IsDefined(typeof(NameType), (NameType)f); f++) {
				String name = ((NameType)f).ToString();
				if (f >= (int)NameType.Void) {
					break;
				} else {
					AddType(new CustomKeywordInfo(name, f, true));
				}
			}
			MainKeyword = KeywordType.Name;
		}
	}



	public class CustomGroupTypes : CustomKeywords<CustomKeywordInfo>
	{

		// CONSTRUCTOR
		internal CustomGroupTypes() : base("_KSID_GroupType") {
			AddType(new CustomKeywordInfo(StaticCustomSyntax.DefaultType, 0, true));
			MainKeyword = KeywordType.Group;
		}
	
	}




	public class CustomControlTypes : CustomKeywords<CustomKeywordInfo>
	{

		// CONSTRUCTOR
		internal CustomControlTypes() : base("_KSID_ControlType") {
			AddType(new CustomKeywordInfo(StaticCustomSyntax.DefaultType, 0, true));
			MainKeyword = KeywordType.Control;
		}

	}



	internal class EnumTypes : CustomKeywords<CustomKeywordInfo>
	{

		// CONSTRUCTOR
		internal EnumTypes() : base("_KSID_EnumType") {
			AddType(new CustomKeywordInfo(StaticCustomSyntax.DefaultType, 0, true));
			for (int f = 1; Enum.IsDefined(typeof(EnumSubType), (EnumSubType)f); f++) {
				String name = "_KSID_EnumType__M_" + ((EnumSubType)f).ToString();
				AddType(new CustomKeywordInfo(name, f));
			}
			MainKeyword = KeywordType.Enum;
		}

	}




	public class CustomKeywords<TKeywordInfo> where TKeywordInfo : CustomKeywordInfo 
	{
		private List<TKeywordInfo> _typeInfos = new List<TKeywordInfo>();

		private ReadOnlyCollection<TKeywordInfo> _roTypeInfos;
		public ReadOnlyCollection<TKeywordInfo> ROTypeInfos {
			get { return _roTypeInfos; }
		}


		private Dictionary<String, TKeywordInfo> _typeIndexes = new Dictionary<string, TKeywordInfo>();
		private Dictionary<Identifier, TKeywordInfo> _typeIndexes2 = new Dictionary<Identifier, TKeywordInfo>();

		KeywordType _mainKeyword;

		public KeywordType MainKeyword {
			get { return _mainKeyword; }
			protected set { _mainKeyword = value; }
		}


		Identifier _namespace;
		public Identifier Namespace {
			get { return _namespace; }
		}



		// CONSTRUCTOR

		internal CustomKeywords(String namespaceStr) {
			_namespace = Identifier.ParseKsid(namespaceStr);
			_roTypeInfos = new ReadOnlyCollection<TKeywordInfo>(_typeInfos);
		}



		public void AddType(TKeywordInfo keywordInfo) {
			if (keywordInfo == null)
				throw new ArgumentNullException("keywordInfo");

			TKeywordInfo info2;
			if (_typeIndexes2.TryGetValue(keywordInfo.FullName, out info2)) {
				if (info2.Index != keywordInfo.Index)
					throw new ArgumentException("Custom Keyword already defined with different index");
				return;
			}

			_typeIndexes2.Add(keywordInfo.FullName, keywordInfo);
			_typeInfos.Add(keywordInfo);


			String key = Char.ToLowerInvariant(keywordInfo.Name[0]) + keywordInfo.Name.Substring(1);
			if (_typeIndexes.TryGetValue(key, out info2)) {
				if (info2.Index != keywordInfo.Index) {
					_typeIndexes[key] = null;
				}
			} else {
				_typeIndexes.Add(key, keywordInfo);
			}
		}



		public bool TestKeyword(LexicalToken token, out TKeywordInfo info, out WannaBeKsidName WannaBe) {
			String text;
			WannaBe = null;
			if (token.Type == LexicalTokenType.Identifier && token.Identifier.IsSimple) {
				text = token.Identifier.Simple;
			} else if (token.Type == LexicalTokenType.Keyword) {
				text = token.Keyword.Text;
			} else {
				info = null;
				return false;
			}

			if (!_typeIndexes.TryGetValue(text, out info) || info.Hidden) {
				info = null;
				return false;
			}

			string text2 = Char.ToUpperInvariant(text[0]) + text.Substring(1);
			Identifier id = new Identifier(IdentifierRoot.Localized, new Identifier.Part[] {new Identifier.Part(text2, null)});
			WannaBe = new WannaBeKsidName(token, id, _namespace, token.Lexical.Header);

			return true;
		}



		public bool TestKeyword(Identifier fullName, out TKeywordInfo keywordInfo, bool returnAlsoHidden) {
			if (_typeIndexes2.TryGetValue(fullName, out keywordInfo)) {
				if (returnAlsoHidden || !keywordInfo.Hidden) {
					return true;
				}
			}
			keywordInfo = null;
			return false;
		}

	}




	public class KnownName
	{
		private String _name;
		public String Name {
			get { return _name; }
		}

		private Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}

		private NameType _nameType;
		public NameType NameType {
			get { return _nameType; }
		}

		private LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
		}

		private KnownName _ltName;
		public KnownName LTName {
			get { return _ltName; }
		}

		private KnownName _parent;
		public KnownName Parent {
			get { return _parent; }
		}

		// CONSTRUCTOR
		public KnownName(String name, NameType type) 
			: this(name, type, new LanguageType(), null, null) {}

		public KnownName(String name, NameType type, BasicType languageType)
			: this(name, type, new LanguageType(languageType), null, null) { }

		public KnownName(String name, NameType type, LanguageType languageType, KnownName ltName)
			: this(name, type, languageType, ltName, null) { }

		public KnownName(String name, NameType type, KnownName parent)
			: this(name, type, new LanguageType(), null, parent) { }

		public KnownName(String name, NameType type, LanguageType languageType, KnownName ltName, KnownName parent) {
			if (name == null)
				throw new ArgumentNullException("name");
			_name = name;
			_nameType = type;
			_languageType = languageType;
			_ltName = ltName;
			_parent = parent;
			_identifier = Identifier.ParseKsid(name);
			if (type == NameType.SafeMethod || type == NameType.StaticSafeMethod)
				_languageType.Modifier |= Modifier.Public;
			if (_identifier.Root != IdentifierRoot.Kernel)
				throw new InternalCompilerException("Known name has to be of kernel type");
		}
	}





	public class SystemMethodArgument
	{
		String _name;
		public String Name {
			get { return _name; }
		}

		SystemMethodTemplateType _templateType;
		public SystemMethodTemplateType TemplateType {
			get { return _templateType; }
		}

		LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
		}

		KnownName _ltName;
		public KnownName LTName {
			get { return _ltName; }
		}

		// CONSTRUCTOR
		internal SystemMethodArgument(String name, SystemMethodTemplateType templateType, LanguageType languageType, KnownName ltName) {
			_name = name;
			_templateType = templateType;
			_languageType = languageType;
			_ltName = ltName;
		}

		internal SystemMethodArgument(SystemMethodArgument source, Compilation compilation) {
			_name = source._name;
			_templateType = source._templateType;
			_languageType = source._languageType;
			if (source._ltName != null) {
				_languageType.ObjectType = (ClassOrEnumName)compilation.KsidNames[source._ltName.Identifier];
			}
		}

	}




	public class SystemMethod
	{
		String _name;
		public String Name {
			get { return _name; }
		}

		OperatorRequieredArguments _applyOnType;
		public OperatorRequieredArguments ApplyOnType {
			get { return _applyOnType; }
		}

		List<SystemMethodArgument> _arguments = new List<SystemMethodArgument>();
		public IList<SystemMethodArgument> Arguments {
			get { return _arguments.AsReadOnly(); }
		}

		SystemMethodArgument _ruturnType;
		public SystemMethodArgument RuturnType {
			get { return _ruturnType; }
		}

		bool _isConstant;
		public bool IsConstant {
			get { return _isConstant; }
		}

		object _tag;
		public object Tag {
			get { return _tag; }
			set { _tag = value; }
		}

		// CONSTRUCTOR

		public SystemMethod(String name, OperatorRequieredArguments applyOnType, BasicType returnType, bool isConstant)
			: this(name, applyOnType, SystemMethodTemplateType.None, new LanguageType(returnType), null, isConstant)
		{ }
		public SystemMethod(String name, OperatorRequieredArguments applyOnType, SystemMethodTemplateType returnType, bool isConstant)
			: this(name, applyOnType, returnType, new LanguageType(BasicType.Void), null, isConstant) 
		{ }
		public SystemMethod(String name, OperatorRequieredArguments applyOnType, SystemMethodTemplateType templateType, LanguageType languageType, KnownName ltName, bool isConstant) {
			_name = name;
			_applyOnType = applyOnType;
			_isConstant = isConstant;
			_ruturnType = new SystemMethodArgument(null, templateType, languageType, ltName);
		}

		// copy constructor
		internal SystemMethod(SystemMethod source, Compilation compilation) {
			_name = source._name;
			_applyOnType = source._applyOnType;
			_ruturnType = new SystemMethodArgument(source._ruturnType, compilation);
			_tag = source._tag;
			_isConstant = source._isConstant;
			foreach (SystemMethodArgument argument in source._arguments) {
				_arguments.Add(new SystemMethodArgument(argument, compilation));
			}
		}


		public void AddArgument(String name, BasicType basicType) {
			_arguments.Add(new SystemMethodArgument(name, SystemMethodTemplateType.None, new LanguageType(basicType), null));
		}
		public void AddArgument(String name, SystemMethodTemplateType templateType) {
			_arguments.Add(new SystemMethodArgument(name, templateType, new LanguageType(), null));
		}
		public void AddArgument(String name, SystemMethodTemplateType templateType, LanguageType languageType, KnownName ltName) {
			_arguments.Add(new SystemMethodArgument(name, templateType, languageType, ltName));
		}
	}







	public interface IAttribute
	{
		String KsidName {
			get;
		}
		AttributeLocation Location {
			get;
		}
		IEnumerable<NameType> Filter {
			get;
		}
	}

	public interface IAttributesCollection : IEnumerable<IAttribute>
	{
		String Root {
			get;
		}
		bool IsReloadNeeded(); // when this returns true, you need to stop using this collection and ask IAttributesProvider for new one
	}


	public interface IAttributesProvider
	{
		// collections should be loaded from cache if the are unchanged
		// returns null if loading fails
		IAttributesCollection LoadAttributes(String root, bool dontUseCache); 
	}
}
