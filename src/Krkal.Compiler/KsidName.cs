//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - K s i d N a m e
///
///		KSID name
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Krkal.Compiler
{
	public class KsidName : DagNode<KsidName>
	{

		private Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}

		NameType _nameType;
		public NameType NameType {
			get { return _nameType; }
		}

		LexicalToken _declarationPlace;
		public LexicalToken DeclarationPlace {
			get { return _declarationPlace; }
		}

		int _ordinal;
		public int Ordinal {
			get { return _ordinal; }
			set { _ordinal = value; }
		}


		DataObject _dataObject;
		public DataObject DataObject {
			get { return _dataObject; }
			set { _dataObject = value; }
		}


		Dictionary<KsidName, AttributeField> _attributes;
		public ICollection<AttributeField> Attributes {
			get { return _attributes == null ? null : _attributes.Values; }
		}

		internal IDictionary<KsidName, AttributeField> GetOrCreateAttributesDictionary() {
			if (_attributes == null)
				_attributes = new Dictionary<KsidName, AttributeField>();
			return _attributes;
		}


		// CONSTRUCTOR

		//internal KsidName(Dag<KsidName> dag, Identifier identifier, NameType nameType) 
		//    : this(dag, identifier, nameType, null)
		//{ }

		internal KsidName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace) {
			_identifier = identifier;
			_nameType = nameType;
			_declarationPlace = declarationPlace;
			if (dag == null)
				throw new ArgumentNullException("dag");
			AddToDag(dag);
		}


		public Compilation Compilation {
			get { return ((KsidNamesEx)Dag).Compilation; }
		}


		public override string ToString() {
			return _identifier.ToString();
		}


		public static Predicate<KsidName> MatchType(NameType type) {
			return delegate(KsidName name) { return (name.NameType == type); };
		}


		internal virtual void Reset(CompilationStep step) {			
		}
	}




	public class TypedKsidName : KsidName
	{
		// CONSTRUCTOR
		//internal TypedKsidName(Dag<KsidName> dag, Identifier identifier, NameType nameType) 
		//    : base(dag, identifier, nameType)
		//{}
		internal TypedKsidName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace)
			: base(dag, identifier, nameType, declarationPlace) 
		{ }

		private LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
			set {
				_languageType = value;
				_languageType.Modifier &= (Modifier)ModifierGroups.RelevantForTypedName;
			}
		}

		internal void AssignType(Field field, SourceFile sf) {
			KsidNamesEx ksidNames = (KsidNamesEx)Dag;
			LanguageType type = field.LanguageType;
			if (field.LTName != null) {
				if (!type.AssignObjectType(ksidNames.Find(field.LTName, sf), true))
					ksidNames.Compilation.ErrorLog.ThrowAndLogError(field.LTName.SourceToken, ErrorCode.EClassOrEnumNameExpected);
			}
			type.CheckConstIntegrity(ksidNames.Compilation.ErrorLog, field.Name.SourceToken);
			if (NameType == NameType.StaticVariable && (type.Modifier & Modifier.ConstV) != 0 && !type.IsAllConst)
				ksidNames.Compilation.ErrorLog.LogError(DeclarationPlace, ErrorCode.EConstModsOnAllLevelsNeeded);
			AssignType(type, field.Name.SourceToken);
		}

		internal void AssignType(LanguageType type, LexicalToken errorToken) {
			type.Modifier &= (Modifier)ModifierGroups.RelevantForTypedName;
			if (_languageType.IsUnasigned) {
				_languageType = type;
			} else {
				if (_languageType != type)
					Compilation.ErrorLog.ThrowAndLogError(errorToken, ErrorCode.ERedeclaringType, _languageType, type);
			}
		}
	
	}




	public class StaticVariableName : TypedKsidName
	{

		ClassName _parentClass;
		public ClassName ParentClass { // in which class it was defined
			get { return _parentClass; }
			internal set { _parentClass = value; }
		}

		UniqueField _variableField;
		public UniqueField VariableField {
			get { return _variableField; }
			internal set { _variableField = value; }
		}

		Expression _assignment;
		public Expression GetAssignment() {
			InitializeIfNeeded();
			return _assignment; 
		}

		ConstantValue _constantValue;
		// the initialization will be processed if necessary
		public ConstantValue GetConstantValue() {
			InitializeIfNeeded();
			return _constantValue;
		}


		bool _processing;
		internal bool Processing {
			get {
				TryAssignConstant();
				return _processing; 
			}
		}


		public bool IsConstant {
			get { return (LanguageType.Modifier & Modifier.ConstV) != 0; }
		}



		// CONSTRUCTOR
		internal StaticVariableName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace)
			: base(dag, identifier, nameType, declarationPlace) {
		}



		private void CheckConstant() {
			if (IsConstant) {
				if (_constantValue == null) {
					_variableField.Compilation.ErrorLog.LogError(_variableField.Field.Name.SourceToken, ErrorCode.EConstantRequiered);
				} else {
					if (_constantValue.IsProjectIndependent)
						_variableField.Field.CachedAssignment = _constantValue;
				}
				_assignment = null; // dont public constant initialization code
			}
		}


		private void TryAssignConstant() {
			if (!_processing && _variableField != null && _variableField.Field.CachedAssignment != null) {
				_processing = true;
				_constantValue = _variableField.Field.CachedAssignment;
			}
		}


		private void InitializeIfNeeded() {
			if (!Processing) {
				_processing = true;

				if (_variableField.Field.AssignmentOrBody == null) {
					if (IsConstant && _variableField.Field.FieldType == FieldType.EnumField) {
						StaticVariableName prev = GetPreviosVariable();
						if (prev != null) {
							ConstantValue prevValue = prev.GetConstantValue();
							if (prevValue == null) {
								_variableField.Compilation.ErrorLog.LogError(_variableField.Field.Name.SourceToken, ErrorCode.EUnableToRetrieveConstant, prev);
								_constantValue = new IntConstantValue(0);
							} else {
								_constantValue = new IntConstantValue(((NumericConstantValue)prevValue).GetInt() + 1, prevValue.IsProjectIndependent);
							}
						} else {
							_constantValue = new IntConstantValue(0);
						}
					}
				} else {
					_assignment = new Expression(_variableField);
					_constantValue = _assignment.RootNode.ConstantValue;
				}

				CheckConstant();
			}

		}

		private StaticVariableName GetPreviosVariable() {
			Field prev = null;
			foreach (Field field in _variableField.Field.Parent.Children) {
				if (field == _variableField.Field)
					break;
				prev = field;
			}
			if (prev == null)
				return null;

			return _variableField.Compilation.KsidNames.Find(prev.Name, _variableField.SourceFile) as StaticVariableName;
		}

	}











	public class MethodName : TypedKsidName
	{

		Set<ParameterList> _parameterLists;
		ReadOnlyCollection<ParameterList> _roParameterLists;
		public IList<ParameterList> ParameterLists {
			get { return _roParameterLists; }
		}

		private ClassName _dmInheritedFrom;
		public ClassName DMInheritedFrom {
			get { return _dmInheritedFrom; }
		}

		public bool IsConstant {
			get { return (LanguageType.Modifier & Modifier.ConstM) != 0; }
		}


		// CONSTRUCTOR
		//internal MethodName(Dag<KsidName> dag, Identifier identifier, NameType nameType) 
		//    : this(dag, identifier, nameType, null)
		//{	}
		internal MethodName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace)
			: base(dag, identifier, nameType, declarationPlace) 
		{
			_parameterLists = new Set<ParameterList>();
			_roParameterLists = new ReadOnlyCollection<ParameterList>(_parameterLists);
		}


		internal ParameterList AddParameterList(Field field, Compilation compilation, SourceFile sf) {
			ParameterList ret = new ParameterList(field, compilation, sf);
			_parameterLists.Add(ret);
			return ret;
		}


		internal override void Reset(CompilationStep step) {
			base.Reset(step);
			if (step < CompilationStep.DoClasses) {
				_parameterLists.Clear();
				_dmInheritedFrom = null;
			}
		}

		internal bool AllowOnlyOneDefinition(ClassName inheritedFrom) {
			bool ret = (_dmInheritedFrom == null);
			_dmInheritedFrom = inheritedFrom;
			return ret;
		}
	}




	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class ParameterList : IEnumerable<ParameterList.Param>, IEquatable<ParameterList>
	{

		private List<Param> _params;

		public Param this[int index] {
			get { return _params[index]; }
		}

		public int Count {
			get { return _params.Count; }
		}


		// CONSTRUCTOR
		internal ParameterList(Field field, Compilation compilation, SourceFile sf) {
			_params = new List<Param>(field.Children.Count);
			Set<String> lastNames = new Set<string>();

			foreach (Field param in field.Children) {
				Param p = new Param(param, compilation, sf);
				if (lastNames.Contains(p.Identifier.LastPart.Name)) {
					LexicalToken token = param.ParamName != null ? param.ParamName : param.Name.SourceToken;
					compilation.ErrorLog.LogError(token, ErrorCode.EMultipleDeclaration);
				}
				lastNames.Add(p.Identifier.LastPart.Name);
				_params.Add(p);
			}

		}


		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append('(');
			for (int f = 0; f < Count; f++) {
				if (f > 0)
					sb.Append(", ");
				sb.Append(_params[f]);
			}
			sb.Append(')');
			return sb.ToString();
		}



		#region IEnumerable<Part> Members

		public IEnumerator<Param> GetEnumerator() {
			return _params.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _params.GetEnumerator();
		}

		#endregion

		#region IEquatable<ParameterList> Members

		public bool Equals(ParameterList other) {
			if (other == null || Count != other.Count)
				return false;
			for (int f = 0; f < Count; f++) {
				if (_params[f].Identifier != other._params[f].Identifier)
					return false;
				if (_params[f].Type != other._params[f].Type)
					return false;
			}
			return true;
		}

		public override int GetHashCode() {
			int ret = Count;
			if (ret > 0) {
				ret ^= _params[0].Identifier.GetHashCode();
				ret ^= _params[ret - 1].Identifier.GetHashCode();
			}
			return ret;
		}

		#endregion


		#region class Param
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class Param
		{
			LanguageType _type;
			public LanguageType Type {
				get { return _type; }
			}

			Identifier _identifier;
			public Identifier Identifier {
				get { return _identifier; }
			}

			KsidName _paramName;
			public KsidName ParamName {
				get { return _paramName; }
			}

			Field _field;
			public Field Field {
				get { return _field; }
			}

			ConstantValue _defaultValue;
			public ConstantValue DefaultValue {
				get { return _defaultValue; }
			}


			// CONSTRUCTOR
			internal Param(Field field, Compilation compilation, SourceFile sf) {
				_field = field;
				_type = field.LanguageType;
				if (field.LTName != null)
					if (!_type.AssignObjectType(compilation.KsidNames.Find(field.LTName, sf)))
						compilation.ErrorLog.ThrowAndLogError(field.LTName.SourceToken, ErrorCode.EClassOrEnumNameExpected);
				if (field.ParamName != null) {
					_identifier = field.ParamName.Identifier;
				} else {
					_paramName = compilation.KsidNames.FindOrAdd(field.Name, NameType.Param, sf);
					_identifier = _paramName.Identifier;
				}
			}



			internal void ReadDefaultValue(SyntaxTemplatesEx syntax) {
				try {
					if (_field.CachedAssignment != null) {
						_defaultValue = _field.CachedAssignment;
					} else if (_field.AssignmentOrBody != null) {
						syntax.Lexical.SeekAfterToken(_field.AssignmentOrBody);
						Expression expression = new Expression(_type, syntax);
						_defaultValue = expression.RootNode.ConstantValue;
						if (_defaultValue.IsProjectIndependent)
							_field.CachedAssignment = _defaultValue;
					}
				}
				catch (ErrorException) { }
			}


			public override string ToString() {
				StringBuilder sb = new StringBuilder();
				sb.Append(_type);
				sb.Append(" ");
				sb.Append(_identifier.LastPart.Name);
				return sb.ToString();
			}
		}
		#endregion





	}





	public abstract class ClassOrEnumName : KsidName
	{
		internal ClassOrEnumName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace)
			: base(dag, identifier, nameType, declarationPlace) { }
	}



	public class EnumName : ClassOrEnumName
	{
		EnumSubType _type;
		public EnumSubType Type {
			get { return _type; }
		}

		ClassName _myClass;
		public ClassName MyClass {
			get { return _myClass; }
		}

		KsidName _myName;
		public KsidName MyName {
			get { return _myName; }
		}

		bool _initialized;

		internal EnumName(Dag<KsidName> dag, Identifier identifier, LexicalToken declarationPlace)
			: base(dag, identifier, NameType.Enum, declarationPlace) 
		{}


		internal void Initialize(Field field, SourceFile sf, KsidNamesEx ksidNames) {
			if (_initialized && _type != (EnumSubType)field.FieldSubType)
				ksidNames.Compilation.ErrorLog.ThrowAndLogError(field.Name.SourceToken, ErrorCode.EEnumRedefinition);
			_type = (EnumSubType)field.FieldSubType;

			if (_type == EnumSubType.Name) {

				KsidName name = this;
				if (field.LTName != null)
					name = ksidNames.Find(field.LTName, sf);
				if (_initialized && name != _myName)
					ksidNames.Compilation.ErrorLog.ThrowAndLogError(field.Name.SourceToken, ErrorCode.EEnumRedefinition);
				_myName = name;

			} else {

				if (_myClass == null) {
					_myClass = (ClassName)ksidNames.FindOrAddHidden(this, "HiddenClass", NameType.Class, field.Name.SourceToken);
				}

			}

			_initialized = true;
		}
	}






	public class DataObject
	{
		List<ClassName> _classes = new List<ClassName>();
		public ClassName Class {
			get { return _classes.Count > 0 ? _classes[0] : null; }
		}

		MethodName _initializationMethod;
		public MethodName InitializationMethod {
			get { return _initializationMethod; }
		}

		KsidName _name;
		public KsidName Name {
			get { return _name; }
		}

		internal DataObject(KsidName name, MethodName initializationMethod) {
			_name = name;
			_initializationMethod = initializationMethod;
		}

		internal void AddClass(ClassName className) {
			bool found = false;
			for (int f = 0; f < _classes.Count; ) {
				if (className == _classes[f] || className.IsDescendant(_classes[f])) {
					found = true;
				} else if (_classes[f].IsDescendant(className)) {
					if (found) {
						_classes.RemoveAt(f);
						continue;
					}
					_classes[f] = className;
					found = true;
				}
				f++;
			}
			if (!found)
				_classes.Add(className);
		}

		internal void CheckClasses(ErrorLog log) {
			if (_classes.Count != 1)
				log.LogError((LexicalToken)null, ErrorCode.EIncompatibleDataObjectClass, _name);
		}
	}



}

