//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - E x p r e s s i o n
///
///		Represents an expression
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{



	
	public class Expression : Statement
	{
		private const int RightToLeftAssociativity = 3;

		ExpressionType _type;
		public ExpressionType Type {
			get { return _type; }
		}

		ExprNode _rootNode;
		public ExprNode RootNode {
			get { return _rootNode; }
		}

		OperatorArgumentCasts _cast;
		public OperatorArgumentCasts Cast {
			get { return _cast; }
		}

		// CONSTRUCTOR
		internal Expression(Statement parentBlock, ExpressionType type)
			: this(parentBlock, type, new LanguageType()) { }

		internal Expression(Statement parentBlock, ExpressionType type, LanguageType expectedType)
			: base(parentBlock) 
		{
			DoExpression(type, expectedType);
		}

		internal Expression(UniqueField variableField)
			: base(variableField) 
		{
			DoExpression(ExpressionType.Initialization, ((TypedKsidName)variableField.Name).LanguageType);
		}

		internal Expression(LanguageType requestedType, SyntaxTemplatesEx syntax)
			: base(syntax) 
		{
			DoExpression(ExpressionType.Initialization, requestedType);
		}



		private void DoExpression(ExpressionType type, LanguageType expectedType) {
			_type = type;
			expectedType.Modifier = (expectedType.Modifier & (Modifier)ModifierGroups.VariableConstGroup) | Modifier.ConstV;

			_rootNode = DoBinaryOperator();

			switch (_type) {
				case ExpressionType.Condition:
					if (_rootNode.LanguageType.IsUnasigned)
						_rootNode.SpecifyUnknownType(new LanguageType(BasicType.Int, Modifier.ConstV));
					if (_rootNode.LanguageType.IsVoid)
						Syntax.ErrorLog.ThrowAndLogError(_rootNode.Token, ErrorCode.ETypeVoidNotAllowed);
					break;
				case ExpressionType.Initialization:
					_cast = _rootNode.CheckType(expectedType);
					break;
				case ExpressionType.Void:
					if (_rootNode.LanguageType.IsUnasigned)
						_rootNode.SpecifyUnknownType(new LanguageType(BasicType.Void));
					Syntax.DoSemiColon();
					break;
				case ExpressionType.VoidNoSemicolon:
					if (_rootNode.LanguageType.IsUnasigned)
						_rootNode.SpecifyUnknownType(new LanguageType(BasicType.Void));
					_type = ExpressionType.Void;
					break;
				case ExpressionType.ForEachArray:
					_cast = _rootNode.CheckTypeInForEachArray(expectedType);
					break;
			}

			LastToken = Syntax.Lexical.CurrentToken;

			_rootNode.WalkForConstants();
			if (_type == ExpressionType.Initialization)
				ConstantValue.CheckType(_rootNode, expectedType);

			if (_rootNode.ConstantValue == null && (Syntax.ExpressionContext & ExpressionContext.ConstantRequiered) != 0)
				Syntax.ErrorLog.ThrowAndLogError(_rootNode.Token, ErrorCode.EConstantRequiered);

		}



		public override StatementType GetStatementType() {
			return StatementType.Expression;
		}



		internal ExprNode DoBinaryOperator() {
			ExprNode node1 = DoUnaryOperator();
			return DoBinaryOperator(1, node1);
		}


		private ExprNode DoBinaryOperator(int lowestPriority, ExprNode node1) {
			while (true) {

				LexicalToken token = Syntax.Lexical.Peek();
				if (token.Type != LexicalTokenType.Operator || token.Operator.Priority < lowestPriority) {
					// I cannot handle this
					return node1;
				}


				Syntax.Lexical.Read();
				ExprBinary node = new ExprBinary(this, token);
				node.Left = node1;
				int currentPriority = token.Operator.Priority;

				ExprNode node2 = DoUnaryOperator();

				token = Syntax.Lexical.Peek();
				if (token.Type == LexicalTokenType.Operator && (token.Operator.Priority > currentPriority || (token.Operator.Priority == currentPriority && currentPriority == RightToLeftAssociativity))) {
					// I need to do highest priority operator first
					node2 = DoBinaryOperator(currentPriority + 1, node2);
				}

				node.Right = node2;
				node.CheckBinaryNode();

				node1 = node;
			}
		}






		private ExprNode DoUnaryOperator() {
			
			ExprNode node = DoUnaryOperator2();

			while (true) {
				LexicalToken token = Syntax.Lexical.Peek();

				if (token.Type != LexicalTokenType.Operator)
					return node;

				switch (token.Operator.Type) {
					case OperatorType.PlusPlus:
					case OperatorType.MinusMinus:
						node = new ExprUnary(this, Syntax.Lexical.Read(), node, true);
						break;
					case OperatorType.Access:
						node = DoAccess(node);
						break;
					case OperatorType.LeftBracket:
						node = DoArrayIndexing(node);
						break;
					case OperatorType.LeftParenthesis:
						node = DoCall(node);
						break;
					default:
						return node;
				}

			}
		}




		private ExprNode DoUnaryOperator2() {

			LexicalToken token = Syntax.Lexical.Peek();

			switch (token.Type) {
				case LexicalTokenType.Operator:
					
					switch (token.Operator.UnaryOpType) {
						
						case UnaryOperatorType.Parenthesis:
							return DoParenthesis();
						case UnaryOperatorType.And:
							return DoUnaryAnd();
						case UnaryOperatorType.Exclamation:
						case UnaryOperatorType.Plus:
						case UnaryOperatorType.PlusPlus:
						case UnaryOperatorType.Tilda:
							return DoArithmeticUnaryOperator();
						case UnaryOperatorType.NewArray:
							return new ExprArray(this, token);

					}
					break;

				case LexicalTokenType.Keyword:

					if (token.Keyword.ConstantKeyword != ConstantKeyword.None) {
						return new ExprConstant(this, Syntax.Lexical.Read());
					} else if (token.Keyword.Type == KeywordType.New) {
						return DoNewObject();
					} else if (token.Keyword.Type == KeywordType.Static) {
						return DoStaticAccess();
					} else if (token.Keyword.Type == KeywordType.Assigned) {
						return new ExprAssigned(this, Syntax.Lexical.Read());
					}
					break;

				case LexicalTokenType.Double:
				case LexicalTokenType.Char:
				case LexicalTokenType.Int:
				case LexicalTokenType.String:
					return new ExprConstant(this, Syntax.Lexical.Read());

				case LexicalTokenType.Identifier:
					return DoIdentifier();

			}

			Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.EUnexpectedToken);
			return null;
		}





		private ExprNode DoParenthesis() {
			Syntax.Lexical.Read(); // (
			ExprNode node = DoBinaryOperator();
			node.WasInParenthesis = true;
			Syntax.DoClosingBracket(OperatorType.RightParenthesis);
			return node;
		}


		private ExprNode DoUnaryAnd() {
			Syntax.Lexical.Read(); // &
			KsidName name = Syntax.ReadKsid(Syntax.ObjectContext);
			return new ExprKsidName(this, Syntax.Lexical.CurrentToken, name);
		}


		private ExprNode DoArithmeticUnaryOperator() {
			LexicalToken token = Syntax.Lexical.Read();
			return new ExprUnary(this, token, DoUnaryOperator());
		}





		private ExprNode DoNewObject() {
			LexicalToken token = Syntax.Lexical.Read(); // new
			ExprNode objectName = DoKsidName(null);
			return new ExprSafeCall(this, token, ExprCallType.New, objectName, null);
		}




		private ExprNode DoStaticAccess() {
			LexicalToken token = Syntax.Lexical.Read(); // static
			if (Syntax.Lexical.Peek() != OperatorType.Access) 
				Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, OperatorType.Access);
			token = Syntax.Lexical.Read(); // ->

			ExprNode field = DoKsidName(Syntax.ObjectContext);
			ExprKsidName ksidField = field as ExprKsidName;

			if (ksidField != null) {
				KsidName fieldName = ksidField.Name;

				switch (fieldName.NameType) {
					case NameType.StaticDirectMethod:
						return new ExprDirectCall(this, token, null, ksidField);
					case NameType.StaticSafeMethod:
						return new ExprSafeCall(this, token, ExprCallType.Static, null, field);
					case NameType.StaticVariable:
						return new ExprStaticAccess(this, token, (StaticVariableName)fieldName);
					default:
						Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EStaticFieldExpected);
						throw new InternalCompilerException();
				}
			
			} else {
				return new ExprSafeCall(this, token, ExprCallType.Static, null, field);
			}
		}




		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		private ExprNode DoIdentifier() {
			LexicalToken token = Syntax.Lexical.Read();
			Identifier id = token.Identifier;
			
			if (id.IsSimple && this.Variables.ContainsKey(id.Simple)) {
				return new ExprLocalVariable(this, token, this.Variables[id.Simple]);
			}

			KsidName name = Syntax.IdToKsid(token, Syntax.ObjectContext);

			switch (name.NameType) {
				case NameType.Variable:
					return new ExprObjectAccess(this, token, null, (TypedKsidName)name);
				case NameType.StaticVariable:
					return new ExprStaticAccess(this, token, (StaticVariableName)name);
				default:
					return new ExprKsidName(this, token, name);
			}
		}



		private ExprNode DoCall(ExprNode node) {
			ExprKsidName ksid = node as ExprKsidName;
			if (ksid != null) {
				switch (ksid.Name.NameType) {
					case NameType.DirectMethod:
					case NameType.StaticDirectMethod:
						return new ExprDirectCall(this, Syntax.Lexical.CurrentToken, null, ksid);
					case NameType.SafeMethod:
						return new ExprSafeCall(this, Syntax.Lexical.CurrentToken, ExprCallType.None, null, ksid);
					case NameType.StaticSafeMethod:
						return new ExprSafeCall(this, Syntax.Lexical.CurrentToken, ExprCallType.Static, null, ksid);
					default:
						Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.EMethodNameExpected);
						throw new InternalCompilerException();
				}
			}

			Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.EUnexpectedToken);
			throw new InternalCompilerException();
		}



		private ExprNode DoAccess(ExprNode obj) {
			LexicalToken token = Syntax.Lexical.Read(); // ->

			// system methods
			ExprNode ret = TrySystemMethod(obj);
			if (ret != null)
				return ret;


			bool isConstant;
			ClassName objectType = ExprNode.CheckIfIsObject(obj, Syntax, token, out isConstant);

			ExprNode field = DoKsidName(objectType == null ? null : objectType.Identifier);
			ExprKsidName ksidField = field as ExprKsidName;

			if (ksidField != null) {
				KsidName fieldName = ksidField.Name;

				switch (fieldName.NameType) {
					case NameType.DirectMethod:
						return new ExprDirectCall(this, token, obj, ksidField);
					case NameType.SafeMethod:
						return new ExprSafeCall(this, token, ExprCallType.None, obj, field);
					case NameType.Variable:
						return new ExprObjectAccess(this, token, obj, (TypedKsidName)fieldName);
					default:
						Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EFieldIsNotAMemberOf, fieldName, obj.LanguageType);
						throw new InternalCompilerException();
				}

			} else {
				return new ExprSafeCall(this, token, ExprCallType.None, obj, field);
			}
		}



		private ExprNode TrySystemMethod(ExprNode obj) {
			if (obj.LanguageType.IsUnasigned)
				return null;
			
			LexicalToken token = Syntax.Lexical.Peek();
			SystemMethod systemMethod;
			if (token.Type == LexicalTokenType.Identifier && token.Identifier.IsSimple && Syntax.Compilation.TryGetSystemMethod(token.Identifier.Simple, out systemMethod)) {

				if (obj.LanguageType.IsType(systemMethod.ApplyOnType, false) != 0) {
					token = Syntax.Lexical.Read();
					return new ExprSystemMethod(this, token, systemMethod, obj);
				}

			} 

			return null;
		}




		private ExprNode DoArrayIndexing(ExprNode node) {
			LexicalToken token = Syntax.Lexical.Read(); // [
			ExprNode index = DoBinaryOperator();
			Syntax.DoClosingBracket(OperatorType.RightBracket);

			return new ExprArrayAccess(this, token, node, index);
		}


		internal ExprNode DoKsidName(Identifier localizeTo) {
			ExprNode name;
			if (Syntax.TryReadToken(OperatorType.LeftParenthesis)) {
				name = DoBinaryOperator();
				Syntax.DoClosingBracket(OperatorType.RightParenthesis);
			} else {
				KsidName name2 = Syntax.ReadKsid(localizeTo);
				name = new ExprKsidName(this, Syntax.Lexical.CurrentToken, name2);
			}
			return name;
		}



	}












	public enum MessageType
	{
		None,
		Message,
		End,
		NextTurn,
		NextEnd,
		Timed,
		CallEnd,
	}



	public enum UnaryOperatorType
	{
		None = 0,
		PlusPlus = 1,	// and MinusMinis
		Plus = 2,		// and Minus
		Exclamation = 3,
		Tilda = 4,
		And = 5,
		Parenthesis = 6,
		NewArray = 7,
	}

	public enum BinaryOperatorType
	{
		None,
		Plus,				//	+
		Minus,				//	-
		Multiply,			//	*
		Division,			//	/
		Modulo,				//	%
		Shift,				//	<< >>
		Compare,			//	< > <= >=
		Equality,			//	== !=
		BinaryOperator,		//	& | ^
		LogicalOperator,	//	&& ||
		Assign,				//	=
		MultiplyAssign,		//	*=
		DivisionAssign,		//	/=
		ModuloAssign,		//	%=
		PlusAssign,			//	+=
		MinusAssign,		//	-=
		ShiftAssign,		//	<<= >>=
		BinaryOperatorAssign,//	&= |= ^=
	}

	public enum ExpressionType
	{
		Void,
		Condition,
		Initialization,
		VoidNoSemicolon,
		ForEachArray,
	}



	public enum ConstantKeyword
	{
		None,
		Null,
		True,
		False,
		Everything,
		Nothing,
		This,
		Sender,
	}
}

