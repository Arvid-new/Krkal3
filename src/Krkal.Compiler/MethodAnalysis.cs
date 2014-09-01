//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - M e t h o d A n a l y s i s
///
///		Represents compiled method
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	public class MethodAnalysis
	{
		private MethodField _methodField;
		public MethodField MethodField {
			get { return _methodField; }
		}

		private CodeBlock _codeBlock;
		public CodeBlock CodeBlock {
			get { return _codeBlock; }
		}

		private String _externBlock;
		public String ExternBlock {
			get { return _externBlock; }
		}

		private List<ExternMethodSubstitution> _substitutedNames;
		public IList<ExternMethodSubstitution> SubstitutedNames {
			get { return _substitutedNames; }
		}


		// only local variables
		private List<LocalVariable> _variables = new List<LocalVariable>();
		private IList<LocalVariable> _roVariables;
		public IList<LocalVariable> Variables {
			get {
				if (_roVariables == null)
					_roVariables = _variables.AsReadOnly();
				return _roVariables;
			}
		}

		private Dictionary<String, List<LocalVariable>> _groupedVariables = new Dictionary<string, List<LocalVariable>>();

		// CONSTRUCTOR

		public MethodAnalysis(MethodField methodField) {
			if (methodField == null)
				throw new ArgumentNullException("methodField");
			_methodField = methodField;

			DoDefaultParameters();

			methodField.Field.Syntax.Lexical.SeekAtToken(_methodField.Field.AssignmentOrBody);

			if ((methodField.Field.LanguageType.Modifier & Modifier.Extern) != 0) {
				int pos = methodField.Field.Syntax.Lexical.ReadingPosition;
				_externBlock = methodField.Field.Syntax.Lexical.ReadBody();
				ResoveKsidNamesForExternMethod(pos);
			} else {
				_codeBlock = new CodeBlock(this);
			}
		}




		private void DoDefaultParameters() {
			SyntaxTemplatesEx syntax = new SyntaxTemplatesEx(_methodField.Compilation, _methodField.SourceFile, _methodField.ObjectContext, null, ExpressionContext.Parameter, false);
			foreach (ParameterList.Param param in _methodField.ParameterList) {
				param.ReadDefaultValue(syntax);
			}
		}




		private void ResoveKsidNamesForExternMethod(int pos) {
			_substitutedNames = new List<ExternMethodSubstitution>();
			SyntaxTemplatesEx syntax = new SyntaxTemplatesEx(_methodField.Compilation, _methodField.SourceFile, _methodField.ObjectContext, _methodField.InheritedFrom, CompilerConstants.IsNameTypeStatic(_methodField.Name.NameType) ? ExpressionContext.StaticMethod : ExpressionContext.Method, _methodField.Name.IsConstant);

			for (int index = 0; (index = _externBlock.IndexOf("_KS", index, StringComparison.Ordinal)) != -1; index++) {
				ExternMethodSubstitution substitution = new ExternMethodSubstitution(index);
				if (substitution.Parse(pos, syntax))
					_substitutedNames.Add(substitution);
			}
		}



		internal LocalVariable DeclatreVariable(LocalDeclaration variableDeclaration) {
			LocalVariable variable = new LocalVariable(variableDeclaration, _variables.Count);
			_variables.Add(variable);

			List<LocalVariable> list;
			if (_groupedVariables.TryGetValue(variable.Name, out list)) {
				foreach (LocalVariable v in list) {
					Statement block = v.Declaration.ParentBlock;
					while (block != null) {
						if (block == variable.Declaration.ParentBlock)
							variableDeclaration.Syntax.ErrorLog.ThrowAndLogError(variableDeclaration.NameToken, ErrorCode.ELocalVariableDeclared);
						block = block.ParentBlock;
					}
				}
			} else {
				list = new List<LocalVariable>();
				_groupedVariables.Add(variable.Name, list);
			}
			list.Add(variable);

			return variable;
		}
	}








	public class LocalVariable
	{
		private String _name;
		public String Name {
			get { return _name; }
		}

		private LocalDeclaration _declaration;
		public LocalDeclaration Declaration {
			get { return _declaration; }
		}

		private ParameterList.Param _parameter;
		public ParameterList.Param Parameter {
			get { return _parameter; }
		}

		public LanguageType LanguageType {
			get { return _declaration != null ? _declaration.LanguageType : _parameter.Type; }
		}

		int _index;
		/// <summary>
		/// Index to parameter list or to local variable list 
		/// </summary>
		public int Index {
			get { return _index; }
		}

		// CONSTRUCTOR
		internal LocalVariable(LocalDeclaration declaration, int index) {
			_declaration = declaration;
			_index = index;
			_name = declaration.Name;
		}


		// CONSTRUCTOR
		internal LocalVariable(ParameterList.Param parameter, int index) {
			_parameter = parameter;
			_index = index;
			_name = parameter.Identifier.LastPart.Name;
		}
	}



	public enum ExternMethodSubstitutionType
	{
		Ksid,
		StaticVariable,
		MemberVariable,
		Cast,
		DirectMethod,
	}

	public class ExternMethodSubstitution
	{
		int _startIndex;
		public int StartIndex {
			get { return _startIndex; }
		}

		int _endIndex;
		public int EndIndex {
			get { return _endIndex; }
		}

		ExternMethodSubstitutionType _type;
		public ExternMethodSubstitutionType Type {
			get { return _type; }
		}

		KsidName _name1;
		public KsidName Name1 {
			get { return _name1; }
		}

		KsidName _name2;
		public KsidName Name2 {
			get { return _name2; }
		}

		// CONSTRUCTOR

		internal ExternMethodSubstitution(int startIndex) {
			_startIndex = startIndex;
		}

		internal bool Parse(int pos, SyntaxTemplatesEx syntax) {

			LexicalToken token = syntax.Lexical.Read(pos + _startIndex);
			if (token.Type != LexicalTokenType.Identifier || !token.Identifier.IsSimple || token.Identifier.Simple.Length < 5)
				return false;

			switch (token.Identifier.Simple[3]) {
				case 'I':
					if (token.Identifier.Simple != "_KSID_")
						return false;
					_type = ExternMethodSubstitutionType.Ksid;
					break;
				case 'G':
					if (token.Identifier.Simple != "_KSG_")
						return false;
					_type = ExternMethodSubstitutionType.StaticVariable;
					break;
				case 'V':
					if (token.Identifier.Simple != "_KSV_")
						return false;
					_type = ExternMethodSubstitutionType.MemberVariable;
					break;
				case 'C':
					if (token.Identifier.Simple != "_KSC_")
						return false;
					_type = ExternMethodSubstitutionType.Cast;
					break;
				case 'D':
					if (token.Identifier.Simple != "_KSDM_")
						return false;
					_type = ExternMethodSubstitutionType.DirectMethod;
					break;
				default:
					return false;
			}

			syntax.DoClosingBracket(OperatorType.LeftParenthesis);

			if (_type == ExternMethodSubstitutionType.MemberVariable || _type == ExternMethodSubstitutionType.Cast) {
				_name1 = syntax.ReadKsid(null, NameType.Class);
				syntax.DoClosingBracket(OperatorType.Comma);

				if (_type == ExternMethodSubstitutionType.MemberVariable) {
					_name2 = syntax.ReadKsid(_name1.Identifier);
				} else {
					_name2 = syntax.ReadKsid(null, NameType.Class);
				}
			} else {
				_name1 = syntax.ReadKsid(syntax.ObjectContext);
			}

			syntax.DoClosingBracket(OperatorType.RightParenthesis);

			_endIndex = syntax.Lexical.ReadingPosition - pos;

			return true;
		}

	}

}
