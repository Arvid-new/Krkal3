//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - S t a t e m e n t
///
///		Method Statement and Block
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	[Flags]
	internal enum AllowedStatements
	{
		None = 0,
		Declaration = 1,
		CommandAndBlock = 2,
		Empty = 4,
		All = Declaration | CommandAndBlock,
		CommandBody = CommandAndBlock | Empty,
		ForInitialization = Declaration | Empty,
	}



	public enum StatementType
	{
		Block,
		Command,
		Expression,
		Declaration,
	}


	public abstract class Statement
	{
		private Statement _parentBlock;
		public Statement ParentBlock {
			get { return _parentBlock; }
		}

		private MethodAnalysis _method;
		public MethodAnalysis Method {
			get { return _method; }
		}

		private SyntaxTemplatesEx _syntax;
		internal SyntaxTemplatesEx Syntax {
			get { return _syntax; }
		}

		private Dictionary<String, LocalVariable> _variables;
		public IDictionary<String, LocalVariable> Variables {
			get { return _variables; }
		}


		private LexicalToken _firstToken;
		public LexicalToken FirstToken {
			get { return _firstToken; }
		}

		private LexicalToken _lastToken;
		public LexicalToken LastToken {
			get { return _lastToken; }
			protected set { _lastToken = value; }
		}

		public int Pos {
			get { return _firstToken.Pos; }
		}

		public int Size {
			get { return _lastToken.Pos - _firstToken.Pos + _lastToken.Size; }
		}

		PositionInLines _positionInLines;
		public PositionInLines PositionInLines {
			get {
				if (_positionInLines == null)
					_positionInLines = new PositionInLines(Pos, Size, _firstToken.Lexical.Lines);
				return _positionInLines; 
			}
		}



		// CONSTRUCTOR
		protected Statement(MethodAnalysis method) {
			if (method == null)
				throw new ArgumentNullException("method");
			_method = method;
			_syntax = new SyntaxTemplatesEx(method.MethodField.Compilation, method.MethodField.SourceFile, _method.MethodField.ObjectContext, _method.MethodField.InheritedFrom, CompilerConstants.IsNameTypeStatic(_method.MethodField.Name.NameType) ? ExpressionContext.StaticMethod : ExpressionContext.Method, _method.MethodField.Name.IsConstant);
			_lastToken = _firstToken = _syntax.Lexical.Peek();
			AddParametersToVariables();
		}

		protected Statement(Statement parentBlock) {
			if (parentBlock == null)
				throw new ArgumentNullException("parentBlock");
			_parentBlock = parentBlock;
			_method = parentBlock._method;
			_syntax = parentBlock._syntax;
			_lastToken = _firstToken = _syntax.Lexical.Peek();
			_variables = parentBlock._variables;
		}

		protected Statement(UniqueField variableField) {
			if (variableField == null)
				throw new ArgumentNullException("variableField");
			_syntax = new SyntaxTemplatesEx(variableField.Compilation, variableField.SourceFile, variableField.ObjectContext, variableField.InheritedFrom, CompilerConstants.IsNameTypeStatic(variableField.Name.NameType) ? ExpressionContext.StaticVariable : ExpressionContext.MemberVariable, false);
			_syntax.Lexical.SeekAfterToken(variableField.Field.AssignmentOrBody);
			_lastToken = _firstToken = _syntax.Lexical.Peek();
			_variables = new Dictionary<string, LocalVariable>();
		}

		internal Statement(SyntaxTemplatesEx syntax) {
			if (syntax == null)
				throw new ArgumentNullException("syntax");
			_syntax = syntax;
			_lastToken = _firstToken = _syntax.Lexical.Peek();
			_variables = new Dictionary<string, LocalVariable>();
		}



		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public abstract StatementType GetStatementType();

		private void AddParametersToVariables() {
			_variables = new Dictionary<string, LocalVariable>();
			int f = 0;
			foreach (ParameterList.Param param in _method.MethodField.ParameterList) {
				LocalVariable v = new LocalVariable(param, f);
				_variables.Add(v.Name, v);
				f++;
			}
		}



		internal void DeclareVariable(LocalDeclaration variableDeclaration) {
			if (_variables == null || (_parentBlock != null && _parentBlock._variables == _variables)) {
				if (_parentBlock != null && _parentBlock._variables != null) {
					_variables = new Dictionary<string, LocalVariable>(_parentBlock._variables);
				} else {
					_variables = new Dictionary<string, LocalVariable>();
				}
			}

			LocalVariable variable = _method.DeclatreVariable(variableDeclaration);

			if (_variables.ContainsKey(variable.Name))
				_syntax.ErrorLog.ThrowAndLogError(variable.Declaration.NameToken, ErrorCode.ELocalVariableDeclared);
			_variables.Add(variable.Name, variable);			
		}


		internal static Statement Create(Statement parentBlock, AllowedStatements allowedStatements) {
			return Create(parentBlock, (allowedStatements & AllowedStatements.Declaration) != 0, (allowedStatements & AllowedStatements.CommandAndBlock) != 0, (allowedStatements & AllowedStatements.Empty) != 0);
		}

		private static Statement Create(Statement parentBlock, bool allowDeclaration, bool allowCommandsAndBlocks, bool allowEmpty) {
			SyntaxTemplates syntax = parentBlock.Syntax;
			LexicalToken token = syntax.Lexical.Peek();
			Statement statement;

			if (allowEmpty && syntax.TryReadToken(OperatorType.SemiColon))
				return null;

			if (token.Type == LexicalTokenType.Keyword) {
				if (allowCommandsAndBlocks && token.Keyword.CommandType != CommandType.None) {
					statement = Commanand.Create(token.Keyword.CommandType, parentBlock);
				} else if (allowDeclaration && (token.Keyword.BasicType != BasicType.Unasigned || token == KeywordType.String || (token.Keyword.Modifier & (Modifier)ModifierGroups.AllowedForLocalVariable) != 0)) {
					statement = LocalDeclaration.Create(parentBlock, true);
				} else {
					statement = new Expression(parentBlock, ExpressionType.Void);
				}
			} else if (token.Type == LexicalTokenType.Identifier) {
				if (allowDeclaration && (syntax.Lexical.PeekNth(1).Type == LexicalTokenType.Identifier || (syntax.Lexical.PeekNth(1) == OperatorType.LeftBracket && syntax.Lexical.PeekNth(2) == OperatorType.RightBracket))) {
					statement = LocalDeclaration.Create(parentBlock, true);
				} else {
					statement = new Expression(parentBlock, ExpressionType.Void);
				}
			} else if (allowCommandsAndBlocks && token == OperatorType.LeftCurlyBracket) {
				statement = new CodeBlock(parentBlock);
			} else {
				statement = new Expression(parentBlock, ExpressionType.Void);
			}
			return statement;
		}
	}














	public class CodeBlock : Statement
	{

		private List<Statement> _statements = new List<Statement>();
		private IList<Statement> _roStatements;
		public IList<Statement> Statements {
			get { 
				if (_roStatements == null)
					_roStatements = _statements.AsReadOnly();
				return _roStatements;
			}
		}

		// CONSTRUCTOR

		internal CodeBlock(MethodAnalysis method) 
			: base(method)
		{
			DoCodeBlock();
		}

		internal CodeBlock(Statement parentBlock)
			: base(parentBlock) 
		{
			DoCodeBlock();
		}


		internal void AddStatement(Statement statement) {
			_statements.Add(statement);
		}


		private void DoCodeBlock() {
			Syntax.DoBody(DoBodyParameters.DoCodeBlock, DoStatement, OperatorType.SemiColon, OperatorType.RightCurlyBracket);
			LastToken = Syntax.Lexical.CurrentToken;
		}


		private bool DoStatement() {
			Statement statement = Statement.Create(this, AllowedStatements.All);
			_statements.Add(statement);
			return true;
		}

		public override StatementType GetStatementType() {
			return StatementType.Block;
		}
	}
}
