//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - F i e l d
///
///		Any declared field. Only syntax analysis.
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	public enum FieldType
	{
		Name,
		Class,
		Group,
		Variable,
		Method,
		Control,
		ParameterSafe,
		ParameterDirect,
		File,
		NewDataObject,
		Enum,
		EnumField,
	}

	public enum EnumSubType
	{
		Int,
		Flags,
		Name,
	}


	public class Field
	{
		private FieldType _fieldType;
		public FieldType FieldType {
			get { return _fieldType; }
		}

		// used for names, groups, controls, new data objects (name type) and unums
		private int _fieldSubType;
		public int FieldSubType {
			get { return _fieldSubType; }
		}

		private WannaBeKsidName _customKeywordName;
		public WannaBeKsidName CustomKeywordName {
			get { return _customKeywordName; }
		}

		// type of variables or methods.
		private LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
			internal set { _languageType = value; }
		}

		// name that will be assigned to Language type. Or class name of Data Object or name assigned to enum
		private WannaBeKsidName _ltName;
		public WannaBeKsidName LTName {
			get { return _ltName; }
		}

		// Name of the field
		private WannaBeKsidName _name;
		public WannaBeKsidName Name {
			get { return _name; }
		}

		// Name of the field in case of direct method's parameter
		private LexicalToken _paramName;
		public LexicalToken ParamName {
			get { return _paramName; }
		}

		private LexicalToken _attributes;
		public LexicalToken Attributes {
			get { return _attributes; }
		}

		private List<Field> _children = new List<Field>();
		public IList<Field> Children {
			get { return _children; }
		}

		private Field _parent;
		public Field Parent {
			get { return _parent; }
		}

		private LexicalToken _assignmentOrBody;
		public LexicalToken AssignmentOrBody {
			get { return _assignmentOrBody; }
		}

		private SyntaxTemplates _syntax;
		internal SyntaxTemplates Syntax {
			get { return _syntax; }
		}

		private ConstantValue _cachedAssignment;
		public ConstantValue CachedAssignment {
			get { return _cachedAssignment; }
			internal set { _cachedAssignment = value; }
		}


		// CONSTRUCTOR

		// this construtor is used if there are multiple declarations of same type separated by commas.
		internal Field(Field sibling) {
			_fieldType = sibling._fieldType;
			_fieldSubType = sibling._fieldSubType;
			_languageType = sibling._languageType;
			_ltName = sibling._ltName;
			_parent = sibling._parent;
			_syntax = sibling._syntax;
			_customKeywordName = sibling._customKeywordName;
		}


		// for top level file field
		internal Field() {
			_fieldType = FieldType.File;
		}
		// for fake fields
		internal Field(FieldType fieldType) {
			_fieldType = fieldType;
		}


		// main constructor
		internal Field(SyntaxTemplates syntax, Field parent) {
			_parent = parent;
			_syntax = syntax;

			if (_parent != null && _parent.FieldType == FieldType.Enum) {
				// ennum fields don't have type specification
				_fieldType = FieldType.EnumField;
				_languageType.BasicType = BasicType.Int;
				_languageType.Modifier = Modifier.ConstV | Modifier.Public;
			} else {

				// read first part of declaration. First all modifiers then field type.
				do {
					if (ReadType()) {
						CheckIntegrity();
						return; // success
					}
				} while (ReadModifier());
				_syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EInvalidDeclaration);

			}
		}




		// Check if field belongs to correct scope.
		private void CheckIntegrity() {
			switch (_fieldType) {
				case FieldType.Name:
					if (_parent._fieldType != FieldType.File)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.ENameDeclarationIsntTop);
					break;
				case FieldType.Class:
					if (_parent._fieldType != FieldType.File)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EClassDeclarationIsntTop);
					break;
				case FieldType.Enum:
					if (_parent._fieldType != FieldType.File)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EEnumDeclarationIsntTop);
					break;
				case FieldType.NewDataObject:
					if (_parent._fieldType != FieldType.File)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EDOdeclarationIsntTop);
					break;
				case FieldType.Control:
				case FieldType.Group:
				case FieldType.Variable:
				case FieldType.Method:
					if (_parent._fieldType == FieldType.Method)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.ENotParamDeclaration);
					if (_parent._fieldType != FieldType.Class && _parent._fieldType != FieldType.Group)
						_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EDeclarationNotInClass, _fieldType);
					break;
			}
		}




		private bool ReadModifier() {
			LexicalToken token = _syntax.Lexical.Peek();
			if (token.Type == LexicalTokenType.Keyword && token.Keyword.Modifier != Modifier.None) {
				Modifier m = token.Keyword.Modifier;

				if ((_languageType.Modifier & m) != 0)
					_syntax.ErrorLog.LogError(token, ErrorCode.EModifierAlreadyUsed);
				if ((_languageType.Modifier & (Modifier)ModifierGroups.CallingGroup) != 0 && (m & (Modifier)ModifierGroups.CallingGroup) != 0)
					_syntax.ErrorLog.LogError(token, ErrorCode.ESafeDirectOverrideTogether);
				if ((_languageType.Modifier & (Modifier)ModifierGroups.AccessModifiers) != 0 && (m & (Modifier)ModifierGroups.AccessModifiers) != 0)
					_syntax.ErrorLog.LogError(token, ErrorCode.EOnlyOneAccessModCanBe);
				if ((_languageType.Modifier & (Modifier)ModifierGroups.RetGroup) != 0 && (m & (Modifier)ModifierGroups.RetGroup) != 0)
					_syntax.ErrorLog.LogError(token, ErrorCode.EModifierAlreadyUsed);
	
				_languageType.Modifier |= m;
				_syntax.Lexical.Read();
				return true;
			}
			return false;
		}






		private bool ReadType() {
			if (CheckCustomKeyword(FieldType.Name, KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes))
				return true;
			if (CheckCustomKeyword(FieldType.Group, KrkalCompiler.Compiler.CustomSyntax.CustomGroupTypes))
				return true;
			if (CheckCustomKeyword(FieldType.Control, KrkalCompiler.Compiler.CustomSyntax.CustomControlTypes))
				return true;
			if (CheckCustomKeyword(FieldType.Enum, KrkalCompiler.Compiler.CustomSyntax.EnumTypes))
				return true;

			LexicalToken token = _syntax.Lexical.Peek();
			if (token.Type == LexicalTokenType.Keyword) {
				switch (token.Keyword.Type) {
					case KeywordType.Class:
						_fieldType = FieldType.Class;
						break;
					case KeywordType.Group:
						_fieldType = FieldType.Group;
						_fieldSubType = 0;
						break;
					case KeywordType.Control:
						_fieldType = FieldType.Control;
						_fieldSubType = 0;
						break;
					case KeywordType.Enum:
						_fieldType = FieldType.Enum;
						_fieldSubType = 0;
						break;
					case KeywordType.Void:
						_languageType.BasicType = BasicType.Void;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.Char:
						_languageType.BasicType = BasicType.Char;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.Int:
						_languageType.BasicType = BasicType.Int;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.Double:
						_languageType.BasicType = BasicType.Double;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.Name:
						_languageType.BasicType = BasicType.Name;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.Object:
						_languageType.BasicType = BasicType.Object;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.String:
						_languageType.BasicType = BasicType.Char;
						_languageType.DimensionsCount = 1;
						_fieldType = FieldType.Variable;
						break;
					case KeywordType.New:
						return DoNewDataObject();						
					default:
						return false;
				}

				_syntax.Lexical.Read();

			} else if (token.Type == LexicalTokenType.Identifier) {
				_syntax.Lexical.Read();
				_fieldType = FieldType.Variable;
				_ltName = new WannaBeKsidName(token, null, _syntax.Lexical.Header);
			} else {
				return false;
			}


			if (_fieldType == FieldType.Variable) {

				_languageType.ReadArray(_syntax);

				if (_parent._fieldType == FieldType.File) {
					_fieldType = FieldType.Name;
				} else if (_parent._fieldType == FieldType.Method) {
					if ((_parent._languageType.Modifier & Modifier.Direct) != 0) {
						_fieldType = FieldType.ParameterDirect;
					} else {
						_fieldType = FieldType.ParameterSafe;
					}
				}
			}

			return true;
		}




		private bool DoNewDataObject() {
			_syntax.Lexical.Read();
			_fieldType = FieldType.NewDataObject;
			
			_fieldSubType = -1;
			CustomKeywordInfo info;
			if (_syntax.CheckCustomKeyword(KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes, out info, out _customKeywordName)) {
				if (info != null)
					_fieldSubType = info.Index;
				_syntax.Lexical.Read();
				_syntax.Lexical.Read();
			}
			// name type can be unspecified if there is no _customKeywordName. So the name cannot be declared here

			LexicalToken token = _syntax.TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EClassNameExpected);
			_ltName = new WannaBeKsidName(token, null, _syntax.Lexical.Header);

			return true;
		}




		private bool CheckCustomKeyword<T>(FieldType type, CustomKeywords<T> customKeywords) where T : CustomKeywordInfo {
			T info;
			if (_syntax.CheckCustomKeyword(customKeywords, out info, out _customKeywordName)) {
				_fieldType = type;
				if (info != null) {
					_fieldSubType = info.Index;
				} else {
					_fieldSubType = -1;
				}
				_syntax.Lexical.Read();
				_syntax.Lexical.Read();
				return true;
			} else {
				return false;
			}
		}





		// This fonction parses the second type of declaration. Started with field name
		// the parameter isFirst specifies if the name is the first of names in multiple declaration (its also set to true if only 1 name is declared)
		internal void DoFieldName(bool isFirst) {
			LexicalToken token = _syntax.TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EIdentifierExpected);

			if (_fieldType == FieldType.ParameterDirect) {
				if (!token.Identifier.IsSimple)
					_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EComposedParamId);
				_paramName = token;
			} else {
				_name = new WannaBeKsidName(token, _parent.NameSpace, _syntax.Lexical.Header);
			}

			if (_fieldType == FieldType.Name) {
				AdjustNameType();
			} else if (_fieldType == FieldType.Variable && isFirst) {
				CheckMethod();
			}

			CheckModifiersIntegrity(token);

			if (_fieldType != FieldType.ParameterDirect) {
				_attributes = _syntax.RememberAttributes();
			}


			switch (_fieldType) {
				case FieldType.Variable:
					DoAssignment(';');
					break;
				case FieldType.ParameterSafe:
					DoAssignment(')');
					break;
				case FieldType.EnumField:
					DoAssignment('}');
					break;
				case FieldType.Method:
				case FieldType.NewDataObject:
					RememberMethodBody();
					break;
				case FieldType.Class:
				case FieldType.Group:
					_syntax.DoBody(DoBodyParameters.DoClass, DoInnerField, OperatorType.SemiColon, OperatorType.RightCurlyBracket);
					break;
				case FieldType.Enum:
					DoEnumBody();
					break;

			}


			_parent.AddChild(this);

			if (_fieldType == FieldType.Variable || _fieldType == FieldType.Name) {
				DoNextSibling();
			}

			if (isFirst && (_fieldType == FieldType.Control || _fieldType == FieldType.Name || _fieldType == FieldType.Variable)) {
				_syntax.DoSemiColon();
			}

			AddImplicitModifiers();
		}




		private void DoEnumBody() {
			if ((EnumSubType)_fieldSubType == EnumSubType.Name) {
				if (_syntax.TryReadToken(OperatorType.Assign)) {
					LexicalToken token = _syntax.TryReadToken(LexicalTokenType.Identifier);
					if (token == null)
						_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EIdentifierExpected);
					_ltName = new WannaBeKsidName(token, null, _syntax.Lexical.Header);
				}
				_syntax.DoSemiColon();
			} else {
				_syntax.DoBody(DoBodyParameters.DoEnum, DoInnerField, OperatorType.Comma, OperatorType.RightCurlyBracket);
			}
		}



		private void CheckModifiersIntegrity(LexicalToken token) {
			Modifier m;
			switch (ShiftedType) {
				case FieldType.Class:
				case FieldType.Control:
				case FieldType.Group:
				case FieldType.Name:
				case FieldType.Enum:
				case FieldType.NewDataObject:
					if (_languageType.Modifier != Modifier.None)
						_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, _languageType.Modifier);
					break;
				case FieldType.Method:
					if ((_languageType.Modifier & Modifier.Direct) != 0) {
						m = _languageType.Modifier & ~((Modifier)ModifierGroups.AllowedForDirectMethod);
					} else {
						m = _languageType.Modifier & ~((Modifier)ModifierGroups.AllowedForSafeMethod);
					}
					if (m != Modifier.None)
						_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, m);
					break;
				case FieldType.ParameterDirect:
					m = _languageType.Modifier & ~((Modifier)ModifierGroups.AllowedForParameterDirect);
					if (m != Modifier.None)
						_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, m);
					break;
				case FieldType.ParameterSafe:
					m = _languageType.Modifier & ~((Modifier)ModifierGroups.AllowedForParameterSafe);
					if (m != Modifier.None)
						_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, m);
					break;
				case FieldType.Variable:
					m = _languageType.Modifier & ~((Modifier)ModifierGroups.AllowedForVariable);
					if (m != Modifier.None)
						_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, m);
					if ((_languageType.Modifier & Modifier.ConstV) != 0) {
						if ((_languageType.Modifier & Modifier.Static) != 0)
							_syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, Modifier.Static);
					}
					break;
			}


			if (_languageType.IsVoid && ShiftedType != FieldType.Method)
				_syntax.ErrorLog.LogError(token, ErrorCode.EVoidVariable);
			if ((_languageType.Modifier & (Modifier)ModifierGroups.RetCalcullationGroup) != 0) {
				if (_languageType.IsType(OperatorRequieredArguments.IntChar, false) == OperatorRequieredArguments.None)
					_syntax.ErrorLog.LogError(token, ErrorCode.ERetFceOnWrongType);
			}
		}


		void AddImplicitModifiers() {
			switch (ShiftedType) {
				case FieldType.Method:
					if ((_languageType.Modifier & Modifier.Direct) == 0) {
						_languageType.Modifier |= Modifier.Public;
					}
					break;
				case FieldType.Variable:
					if ((_languageType.Modifier & Modifier.ConstV) != 0) {
						_languageType.Modifier |= Modifier.Static;
					}
					break;
			}
		}



		private void DoNextSibling() {
			if (_syntax.Lexical.Peek() == OperatorType.Comma) {
				_syntax.Lexical.Read();
				Field f = new Field(this);
				f.DoFieldName(false);
			}
		}


		private void RememberMethodBody() {
			if (_syntax.Lexical.Read() != OperatorType.LeftCurlyBracket)
				_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EMissingBody);
			_assignmentOrBody = _syntax.Lexical.CurrentToken;
			_syntax.Lexical.SkipPart('}', false);
			_syntax.DoClosingBracket(OperatorType.RightCurlyBracket);			
		}


		private void DoAssignment(char delimiter) {
			if (_syntax.Lexical.Peek() == OperatorType.Assign) {
				_assignmentOrBody = _syntax.Lexical.Read();
				char[] closingDelimiters = new char[2];
				closingDelimiters[0] = ',';
				closingDelimiters[1] = delimiter;
				_syntax.Lexical.SkipDelimitedPart(closingDelimiters);
			}
		}


		private void CheckMethod() {
			if (_syntax.Lexical.Peek() == OperatorType.LeftParenthesis) {
				_syntax.Lexical.Read();
				_fieldType = FieldType.Method;

				_syntax.DoBody(DoBodyParameters.DoMethodParameters, DoInnerField, OperatorType.Comma, OperatorType.RightParenthesis);
			}
		}


		private bool DoInnerField() {
			Field f = new Field(_syntax, this);
			f.DoFieldName(true);
			return true;
		}




		private void AdjustNameType() {
			if (_fieldSubType >= 0 && _fieldSubType < (int)NameType.Void) {
				// typed name
				if (_syntax.Lexical.Peek() == OperatorType.LeftParenthesis) {
					_syntax.Lexical.Read();
					_syntax.DoClosingBracket(OperatorType.RightParenthesis);

					_fieldSubType = (int)GetMethodNameType();
				} else {
					_fieldSubType = (int)GetVariableNameType();
				}
			}
		}

		private NameType GetVariableNameType() {
			if ((_languageType.Modifier & Modifier.Static) != 0) {
				return NameType.StaticVariable;
			} else {
				return NameType.Variable;
			}
		}


		private NameType GetMethodNameType() {
			if ((_languageType.Modifier & Modifier.Direct) != 0) {
				if ((_languageType.Modifier & Modifier.Static) != 0) {
					return NameType.StaticDirectMethod;
				} else {
					return NameType.DirectMethod;
				}
			} else {
				if ((_languageType.Modifier & Modifier.Static) != 0) {
					return NameType.StaticSafeMethod;
				} else {
					return NameType.SafeMethod;
				}
			}
		}



		private void AddChild(Field child) {
			_children.Add(child);
		}


	
		public Identifier NameSpace {
			get {
				Field f = this;
				while (f._fieldType == FieldType.Group) {
					f = f._parent;
				}
				if (f._name == null) {
					return null;
				} else {
					return f._name.Identifier;
				}
			}
		}



		// Returns FieldType, except for Names of methods and Names of variables, in such case FieldType.Variable or FieldType.Method will be returned instead of FieldType.Name
		// also FieldType.Variable is returned instead FieldType.enumField
		public FieldType ShiftedType {
			get {
				if (_fieldType == FieldType.Name && _fieldSubType >= 0) {
					if (CompilerConstants.IsNameTypeVariable((NameType)_fieldSubType)) {
						return FieldType.Variable;
					} else if (CompilerConstants.IsNameTypeMethod((NameType)_fieldSubType)) {
						return FieldType.Method;
					}
				} else if (_fieldType == FieldType.EnumField) {
					return FieldType.Variable;
				} 
				return _fieldType;
			}
		}

		public bool HasLanguageType {
			get {
				FieldType type = ShiftedType;
				return (type == FieldType.Variable || type == FieldType.Method || type == FieldType.ParameterDirect || type == FieldType.ParameterSafe);
			}
		}


		public bool IsNameTypeValid {
			get { return (_fieldType != FieldType.File && _fieldType != FieldType.ParameterDirect && (_fieldType != FieldType.NewDataObject || _fieldSubType >= 0)); }
		}

		// returns the Name Type of declared name.
		public NameType GetNameType(Compilation compilation, SourceFile sf) {
			switch (_fieldType) {
				case FieldType.Class:
					return NameType.Class;
				case FieldType.Control:
					return NameType.Control;
				case FieldType.Group:
					return NameType.Group;
				case FieldType.Method:
					return GetMethodNameType();
				case FieldType.Name:
				case FieldType.NewDataObject:
					if (_customKeywordName != null) {
						CustomKeywordInfo info = ResolveCustomKeywordType(compilation.KsidNames, compilation.Compiler.CustomSyntax.CustomNameTypes, sf);
						return (NameType)info.Index;
					}
					if (_fieldSubType == -1)
						compilation.ErrorLog.ThrowAndLogError(_name.SourceToken, ErrorCode.EUnknownCustomSubType);
					return (NameType)_fieldSubType;
				case FieldType.ParameterSafe:
					return NameType.Param;
				case FieldType.Variable:
				case FieldType.EnumField:
					return GetVariableNameType();
				case FieldType.Enum:
					return NameType.Enum;
				default:
					throw new InternalCompilerException("not supported for this fieldtype");
			}
		}



		public T ResolveCustomKeywordType<T>(KsidNamesEx ksidNames, CustomKeywords<T> customKeywords, SourceFile sf) where T:CustomKeywordInfo {
			KsidName name = ksidNames.Find(_customKeywordName, NameType.StaticVariable, sf);
			T ret;
			if (!customKeywords.TestKeyword(name.Identifier, out ret, false))
				ksidNames.Compilation.ErrorLog.ThrowAndLogError(_customKeywordName.SourceToken, ErrorCode.EUnknownCustomSubType);
			return ret;
		}



		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append(_fieldType);
			sb.Append(' ');
			sb.Append(_fieldSubType);
			sb.Append(": ");

			if (HasLanguageType) {
				if (_languageType.Modifier != Modifier.None) {
					sb.Append(_languageType.Modifier);
					sb.Append(' ');
				}
				if (_ltName != null) {
					sb.Append(_ltName);
				} else {
					sb.Append(_languageType.BasicType);
				}

				for (int f = 0; f < _languageType.DimensionsCount; f++) {
					sb.Append("[]");
				}

				sb.Append(' ');
			}

			if (_paramName != null) {
				sb.Append(_paramName.Identifier);
			} else if (_name != null) {
				sb.Append(_name);
			}

			if (ShiftedType == FieldType.Method) {
				sb.Append("()");
			}

			return sb.ToString();
		}
	}


}

