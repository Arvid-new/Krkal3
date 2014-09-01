//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C o m m a n d
///
///		All Command Statements
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	public enum CommandType
	{
		None,
		If,
		While,
		Do,
		For,
		ForEach,
		Switch,
		Break,
		Continue,
		Return,
	}
	
	
	
	abstract public class Commanand : Statement
	{
		CommandType _type;
		public CommandType Type {
			get { return _type; }
		}

		
		// CONSTRUCTOR
		internal Commanand(Statement parentBlock)
			: base(parentBlock) 
		{
			Syntax.Lexical.Read(); // command keyword
		}


		internal static Commanand Create(CommandType type, Statement parentBlock) {
			Commanand command;
			switch (type) {
				case CommandType.If:
					command = new IfCommanand(parentBlock);
					break;
				case CommandType.While:
					command = new WhileCommanand(parentBlock);
					break;
				case CommandType.Do:
					command = new DoCommanand(parentBlock);
					break;
				case CommandType.For:
					command = new ForCommanand(parentBlock);
					break;
				case CommandType.ForEach:
					command = new ForEachCommanand(parentBlock);
					break;
				case CommandType.Switch:
					command = new SwitchCommanand(parentBlock);
					break;
				case CommandType.Break:
					command = new BreakCommanand(parentBlock);
					break;
				case CommandType.Continue:
					command = new ContinueCommanand(parentBlock);
					break;
				case CommandType.Return:
					command = new ReturnCommanand(parentBlock);
					break;
				default:
					throw new InternalCompilerException("Invalid command Type");
			}
			command._type = type;
			command.LastToken = command.Syntax.Lexical.CurrentToken;
			return command;
		}

		public override StatementType GetStatementType() {
			return StatementType.Command;
		}
	}






	public class IfCommanand : Commanand
	{
		Expression _condition;
		public Expression Condition {
			get { return _condition; }
		}

		Statement _ifPart;
		public Statement IfPart {
			get { return _ifPart; }
		}

		Statement _elsePart;
		public Statement ElsePart {
			get { return _elsePart; }
		}

		LexicalToken _elseToken;
		public LexicalToken ElseToken {
			get { return _elseToken; }
		}

		// CONSTRUCTOR
		internal IfCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			Syntax.DoClosingBracket(OperatorType.LeftParenthesis);
			_condition = new Expression(this, ExpressionType.Condition);
			Syntax.DoClosingBracket(OperatorType.RightParenthesis);

			_ifPart = Statement.Create(this, AllowedStatements.CommandBody);
			if (Syntax.TryReadToken(KeywordType.Else)) {
				_elseToken = Syntax.Lexical.CurrentToken;
				_elsePart = Statement.Create(this, AllowedStatements.CommandBody);
			}

		}
	}






	public class WhileCommanand : Commanand
	{
		Expression _condition;
		public Expression Condition {
			get { return _condition; }
		}

		Statement _body;
		public Statement Body {
			get { return _body; }
		}

		// CONSTRUCTOR
		internal WhileCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			Syntax.DoClosingBracket(OperatorType.LeftParenthesis);
			_condition = new Expression(this, ExpressionType.Condition);
			Syntax.DoClosingBracket(OperatorType.RightParenthesis);

			_body = Statement.Create(this, AllowedStatements.CommandBody);

		}
	}




	public class DoCommanand : Commanand
	{
		Expression _condition;
		public Expression Condition {
			get { return _condition; }
		}

		Statement _body;
		public Statement Body {
			get { return _body; }
		}

		// CONSTRUCTOR
		internal DoCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			_body = Statement.Create(this, AllowedStatements.CommandBody);

			Syntax.DoKeyword(KeywordType.While);
			
			Syntax.DoClosingBracket(OperatorType.LeftParenthesis);
			_condition = new Expression(this, ExpressionType.Condition);
			Syntax.DoClosingBracket(OperatorType.RightParenthesis);

			Syntax.DoSemiColon();
		}
	}

	
	


	
	public class ForCommanand : Commanand
	{

		Statement _initialization;
		public Statement Initialization {
			get { return _initialization; }
		}

		Expression _condition;
		public Expression Condition {
			get { return _condition; }
		}

		Expression _lastStep;
		public Expression LastStep {
			get { return _lastStep; }
		}

		Statement _body;
		public Statement Body {
			get { return _body; }
		}

		// CONSTRUCTOR
		internal ForCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			Syntax.DoClosingBracket(OperatorType.LeftParenthesis);

			_initialization = Statement.Create(this, AllowedStatements.ForInitialization); // reads also ;

			if (!Syntax.TryReadToken(OperatorType.SemiColon)) {
				_condition = new Expression(this, ExpressionType.Condition);
				Syntax.DoSemiColon();
			}

			if (!Syntax.TryReadToken(OperatorType.RightParenthesis)) {
				_lastStep = new Expression(this, ExpressionType.VoidNoSemicolon);
				Syntax.DoClosingBracket(OperatorType.RightParenthesis);
			}
			
			_body = Statement.Create(this, AllowedStatements.CommandBody);

		}
	}






	public class ForEachCommanand : Commanand
	{

		LocalDeclaration _declaration;
		public LocalDeclaration Declaration {
			get { return _declaration; }
		}

		Expression _array;
		public Expression Array {
			get { return _array; }
		}

		Expression _condition;
		public Expression Condition {
			get { return _condition; }
		}

		Statement _body;
		public Statement Body {
			get { return _body; }
		}

		// CONSTRUCTOR
		internal ForEachCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			Syntax.DoClosingBracket(OperatorType.LeftParenthesis);
			_declaration = LocalDeclaration.Create(this, false);

			Syntax.DoKeyword(KeywordType.In);
			_array = new Expression(this, ExpressionType.ForEachArray, _declaration.LanguageType);

			if (Syntax.TryReadToken(OperatorType.SemiColon)) {
				_condition = new Expression(this, ExpressionType.Condition);
			}

			Syntax.DoClosingBracket(OperatorType.RightParenthesis);

			_body = Statement.Create(this, AllowedStatements.CommandBody);

		}
	}








	public class SwitchCommanand : Commanand
	{
		// CONSTRUCTOR
		internal SwitchCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
		}
	}






	public class BreakCommanand : Commanand
	{
		Commanand _jumpTo;
		public Commanand JumpTo {
			get { return _jumpTo; }
		}

		// CONSTRUCTOR
		internal BreakCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			for(Statement statement = ParentBlock; statement != null; statement = statement.ParentBlock) {
				_jumpTo = statement as Commanand;
				if (_jumpTo != null) {
					if (_jumpTo is DoCommanand || _jumpTo is ForCommanand || _jumpTo is ForEachCommanand || _jumpTo is SwitchCommanand || _jumpTo is WhileCommanand)
						return;
				}
			}
			Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.ENoEnclosingLoop);
		}
	}







	public class ContinueCommanand : Commanand
	{
		Commanand _jumpTo;
		public Commanand JumpTo {
			get { return _jumpTo; }
		}

		// CONSTRUCTOR
		internal ContinueCommanand(Statement parentBlock)
			: base(parentBlock) 
		{
			for(Statement statement = ParentBlock; statement != null; statement = statement.ParentBlock) {
				_jumpTo = statement as Commanand;
				if (_jumpTo != null) {
					if (_jumpTo is DoCommanand || _jumpTo is ForCommanand || _jumpTo is ForEachCommanand || _jumpTo is WhileCommanand)
						return;
				}
			}
			Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.ENoEnclosingLoop);
		}
	}







	public class ReturnCommanand : Commanand
	{

		Expression _expression;
		public Expression Expression {
			get { return _expression; }
		}

		// CONSTRUCTOR
		internal ReturnCommanand(Statement parentBlock)
			: base(parentBlock) 
		{ 
			LanguageType type = Method.MethodField.Name.LanguageType;
			if (!type.IsVoid) {
				_expression = new Expression(this, ExpressionType.Initialization, type);
			}
			Syntax.DoSemiColon();
		}
	}



}
