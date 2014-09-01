//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - E x p r
///
///		Node of an expression
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	[Flags]
	public enum ExprCallType
	{
		None = 0,
		Static = 1,
		Direct = 2,
		New = 4,
	}




	public enum OperatorResult
	{
		BiggestArgument,
		FirstArgument,
		Int,
	}

	[Flags]
	public enum OperatorRequieredArguments
	{
		None = 0,
		IntCharDouble = 1,
		IntChar = 2,
		Array = 4, // of identical type
		NameObject = 8,
		Pointer = 16, // any combination of types
		Name = 32,
		Object = 64,
	}


	[Flags]
	public enum OperatorConstBehavior
	{
		None = 0, // no const chacks. Result will have no const flags
		LValueRequiered = 1, // also means that type1 has to have set more constO or constMO than type2
		ResultKeep2ndLevel = 2, // O is requiered only if the array is multidimensional
		ResultIsFirst = 4,
		AppendCheck = 8, // do special checking for +=
	}


	[Flags]
	public enum OperatorArgumentCasts
	{
		None = 0,
		CastArithmeticsToBiggest = 1,
		CastArithmeticsToResult = 2,
		ArrayOperation = 4,
		CastObjectsToName = 8,
		MoveObjectsPointersToStartPosition = 16,
		CastObjectToResult = 32,
		CastObjectToDerived = 64,
		CastObjectToBase = 128,
	}



	public enum ExprNodeType
	{
		Binary,
		ArrayAccess,
		Unary,
		Constant,
		KsidName,
		LocalVariable,
		ObjectAccess,
		StaticAccess,
		DirectCall,
		SafeCall,
		Array,
		SystemMethod,
		Assigned,
	}









	public abstract class ExprNode
	{
		private LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
			set { _languageType = value; }
		}

		private LexicalToken _token;
		public LexicalToken Token {
			get { return _token; }
			set { _token = value; }
		}

		private Expression _expression;
		public Expression Expression {
			get { return _expression; }
		}

		bool _wasInParenthesis;
		public bool WasInParenthesis {
			get { return _wasInParenthesis; }
			set { _wasInParenthesis = value; }
		}


		ConstantValue _constantValue;
		public ConstantValue ConstantValue {
			get { return _constantValue; }
		}



		// CONSTRUCTOR
		protected ExprNode(Expression expression, LexicalToken token) {
			_expression = expression;
			_token = token;
		}


		internal void WalkForConstants() {
			bool calculate = true;
			bool checkType = false;
			foreach (ExprNode node in Children) {
				if (node != null) {
					node.WalkForConstants();
					if (node._constantValue == null) {
						calculate = false;
					} else {
						checkType = true;
					}
				} else {
					calculate = false; // calls can have unspecified nodes.
				}
			}
			if (checkType)
				CheckConstantType();
			_constantValue = calculate ? CalculateConstant() : null;
			if (_constantValue != null) {
				foreach (ExprNode node in Children) {
					node._constantValue = null;
				}
			}
		}

		protected virtual void CheckConstantType() {
			// by default no check needed
		}

		protected virtual ConstantValue CalculateConstant() {
			return null;
		}

		internal virtual void SpecifyUnknownType(LanguageType type) {
			throw new InternalCompilerException("this shouldn't have unknown type");
		}

		public virtual bool IsLValue() {
			return false;
		}

		protected void CheckLValue(OperatorConstBehavior requieredArguments) {
			if ((requieredArguments & OperatorConstBehavior.LValueRequiered) != 0) {
				if (!this[0].IsLValue()) {
					Expression.Syntax.ErrorLog.ThrowAndLogError(_token, ErrorCode.ELValueRequiered);
				}
				if (this[0].LanguageType.IsUnasigned) {
					Expression.Syntax.ErrorLog.ThrowAndLogError(_token, ErrorCode.EUnassignedTypeInValue);
				}
				if ((this[0].LanguageType.Modifier & Modifier.ConstV) != 0)
					Expression.Syntax.ErrorLog.ThrowAndLogError(_token, ErrorCode.ECannotWriteToConstant);
			}
		}


		internal static ClassName CheckIfIsObject(ExprNode node, SyntaxTemplatesEx syntax, LexicalToken errorToken, out bool isConst) {
			if (node == null) {
				if (syntax.ExpressionContext == ExpressionContext.StaticMethod)
					syntax.ErrorLog.ThrowAndLogError(errorToken, ErrorCode.EStaticAccessingMember);
				if ((syntax.ExpressionContext & ExpressionContext.MemberNotAllowed) != 0)
					syntax.ErrorLog.ThrowAndLogError(errorToken, ErrorCode.EAccessMembersNotAllowed);
				isConst = syntax.ConstantMethod;
				return syntax.ThisClass;
			} else if (node.LanguageType.IsUnasigned) {
				node.SpecifyUnknownType(new LanguageType(BasicType.Object, Modifier.ConstV));
				isConst = (node.LanguageType.Modifier & Modifier.ConstO) != 0;
				return null;
			} else if (node.LanguageType.IsType(OperatorRequieredArguments.Object, false) != 0) {
				isConst = (node.LanguageType.Modifier & Modifier.ConstO) != 0;
				return (ClassName)node.LanguageType.ObjectType;
			} else {
				syntax.ErrorLog.ThrowAndLogError(errorToken, ErrorCode.EObjectTypeRequiered);
				isConst = (node.LanguageType.Modifier & Modifier.ConstO) != 0;
				return null;
			}
		}


		internal KsidName CheckIfIsName(NameType expectedType) {
			if (LanguageType.IsUnasigned)
				SpecifyUnknownType(new LanguageType(BasicType.Name, Modifier.ConstV));
			if (LanguageType.IsType(OperatorRequieredArguments.Name, false) == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ENameTypeRequiered);
			ExprKsidName exprKsid = this as ExprKsidName;
			if (exprKsid != null) {
				if (exprKsid.Name.NameType != expectedType)
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EDifferentNameType, expectedType, exprKsid.Name.NameType);
				return exprKsid.Name;
			} else {
				return null;
			}
		}


		internal OperatorArgumentCasts CheckType(LanguageType type) {
			LanguageType type2 = type.ClearModifier(ModifierGroups.VariableConstGroup);

			if (LanguageType.IsUnasigned) {
				if ((type.Modifier & Modifier.Ret) != 0) {
					SpecifyUnknownType(type2);
				} else {
					SpecifyUnknownType(new LanguageType(type2.BasicType, type2.Modifier | Modifier.ConstV, type2.DimensionsCount, type2.ObjectType));
				}
			}

			if ((type.Modifier & Modifier.Ret) != 0) {
				// I will need reference, so exact type match is requiered
				if (!IsLValue())
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ELValueRequiered);
				if (LanguageType != type2)
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnableToMakeReference, type2);
				return OperatorArgumentCasts.None;
			}

			OperatorRequieredArguments req = OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array | OperatorRequieredArguments.Name | OperatorRequieredArguments.Object;
			OperatorRequieredArguments arg = CheckTypesMatch(type2, LanguageType, req);
			if (arg == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnableToConvertTypeTo, type2);

			CheckConstBehavior(type2, LanguageType, OperatorConstBehavior.LValueRequiered);

			OperatorArgumentCasts casts = OperatorArgumentCasts.CastArithmeticsToResult | OperatorArgumentCasts.CastObjectToResult;
			return SolveCasts(type2, LanguageType, casts, arg);
		}


		internal OperatorArgumentCasts CheckTypeInForEachArray(LanguageType type) {

			if (LanguageType.IsUnasigned) {
				LanguageType type2 = type;
				type2.IncreaseDimCount(true);
				SpecifyUnknownType(type2);
			}

			LanguageType type1 = LanguageType;
			if (type1.DimensionsCount <= type.DimensionsCount)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EArrayDoesntContainType, type);

			while (type1.DimensionsCount != type.DimensionsCount) {
				type1.DecreaseDimCount();
			}

			OperatorRequieredArguments req = OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array | OperatorRequieredArguments.Name | OperatorRequieredArguments.Object;
			OperatorRequieredArguments arg = CheckTypesMatch(type, type1, req);
			if (arg == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EArrayDoesntContainType, type);

			CheckConstBehavior(type, type1, OperatorConstBehavior.LValueRequiered);

			OperatorArgumentCasts casts = OperatorArgumentCasts.CastArithmeticsToResult | OperatorArgumentCasts.CastObjectToResult;
			return SolveCasts(type, type1, casts, arg);
		}


		protected static OperatorRequieredArguments CheckTypesMatch(LanguageType type1, LanguageType type2, OperatorRequieredArguments req) {
			OperatorRequieredArguments arg;
			arg = type1.IsType(req, true) & type2.IsType(req, true);
			if ((arg & OperatorRequieredArguments.Array) != 0 && !type1.IsNull && !type2.IsNull && type1.ClearModifier() != type2.ClearModifier()) {
				arg &= ~OperatorRequieredArguments.Array;
			}
			return arg;
		}


		protected void CheckConstBehavior(LanguageType type1, LanguageType type2, OperatorConstBehavior req) {
			if ((req & OperatorConstBehavior.AppendCheck) != 0) {
				if (type1.IsPointerToConst)
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ECannotChangeConstantObject);
				type2.IsPointerToConst = false;
			} else if ((req & OperatorConstBehavior.LValueRequiered) == 0) {
				return;
			}

			Modifier mod = type2.Modifier & (Modifier)ModifierGroups.PointerConstGroup;
			if ((mod & type1.Modifier) != mod)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ECannotCastOffConst);
		}




		protected static OperatorArgumentCasts SolveCasts(LanguageType type1, LanguageType type2, OperatorArgumentCasts possibleCasts, OperatorRequieredArguments arg) {
			OperatorArgumentCasts argumentCasts = OperatorArgumentCasts.None;
			if ((arg & (OperatorRequieredArguments.IntChar | OperatorRequieredArguments.IntCharDouble)) != 0) {
				argumentCasts |= (possibleCasts & (OperatorArgumentCasts.CastArithmeticsToBiggest | OperatorArgumentCasts.CastArithmeticsToResult));
			}
			if ((arg & OperatorRequieredArguments.Array) != 0) {
				argumentCasts |= (possibleCasts & OperatorArgumentCasts.ArrayOperation);
			}
			if ((arg & OperatorRequieredArguments.NameObject) != 0) {
				argumentCasts |= (possibleCasts & OperatorArgumentCasts.CastObjectsToName);
			}
			if ((arg & OperatorRequieredArguments.Object) != 0) {
				argumentCasts |= (possibleCasts & OperatorArgumentCasts.MoveObjectsPointersToStartPosition);
				if ((possibleCasts & OperatorArgumentCasts.CastObjectToResult) != 0) {
					if (type1.ObjectType != type2.ObjectType) {
						if (type1.ObjectType == null) {
							argumentCasts |= OperatorArgumentCasts.CastObjectToBase; // cast to ancestor
						} else if (type2.ObjectType == null) {
							argumentCasts |= OperatorArgumentCasts.CastObjectToDerived;
						} else if (type1.ObjectType.IsDescendant(type2.ObjectType)) {
							argumentCasts |= OperatorArgumentCasts.CastObjectToBase;
						} else {
							argumentCasts |= OperatorArgumentCasts.CastObjectToDerived;
						}
					}
				}
			}
			return argumentCasts;
		}




		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public int ChildrenCount {
			get { return GetChildrenCount(); }
		}

		public ExprNode this[int index] {
			get { return GetChild(index); }
		}

		protected abstract ExprNode GetChild(int index);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		protected abstract int GetChildrenCount();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		public abstract ExprNodeType GetNodeType();



		public IEnumerable<ExprNode> Children {
			get {
				for (int f = 0; f < GetChildrenCount(); f++) {
					yield return GetChild(f);
				}
			}
		}
	}








	public class ExprBinary : ExprNode
	{
		private ExprNode _left;
		public ExprNode Left {
			get { return _left; }
			set { _left = value; }
		}

		private ExprNode _right;
		public ExprNode Right {
			get { return _right; }
			set { _right = value; }
		}

		private OperatorArgumentCasts _argumentCasts;
		public OperatorArgumentCasts ArgumentCasts {
			get { return _argumentCasts; }
			set { _argumentCasts = value; }
		}

		// CONSTRUCTOR
		internal ExprBinary(Expression expression, LexicalToken token) : base(expression, token) { }


		internal void CheckBinaryNode() {
			OperatorRequieredArguments arg;
			OperatorDescription opDescription = Token.Operator.BinaryOperatotDescription;

			CheckLValue(opDescription.ConstBehavior);

			SolveUnassignedType();

			// types check
			arg = CheckTypesMatch(_left.LanguageType, _right.LanguageType, opDescription.RequieredArguments);
			if (arg == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ETypeMismatchBinary, _left.LanguageType, _right.LanguageType, Token.Operator);

			CheckConstBehavior(_left.LanguageType, _right.LanguageType, opDescription.ConstBehavior);
			AssignResult(arg);

			_argumentCasts = SolveCasts(_left.LanguageType, _right.LanguageType, opDescription.ArgumentCasts, arg);

		}




		private void AssignResult(OperatorRequieredArguments arg) {
			LanguageType result;

			switch (Token.Operator.BinaryOperatotDescription.Result) {
				case OperatorResult.Int:
					result = new LanguageType(BasicType.Int);
					break;
				case OperatorResult.FirstArgument:
					result = _left.LanguageType;
					break;
				case OperatorResult.BiggestArgument:
					result = LanguageType.Biggest(_left.LanguageType, _right.LanguageType);
					break;
				default:
					throw new InternalCompilerException();
			}

			if ((arg & OperatorRequieredArguments.Array) != 0 && (Token.Operator.BinaryOperatotDescription.ConstBehavior & OperatorConstBehavior.ResultKeep2ndLevel) != 0) {
				Modifier mod = (_left.LanguageType.Modifier | _right.LanguageType.Modifier) & (Modifier)ModifierGroups.PointerConstGroup;
				result.Modifier = ((result.Modifier) & ~(Modifier)ModifierGroups.PointerConstGroup) | mod;
				result.SetArrayConst(result.DimensionsCount - 1, false);
			}

			if (result.IsNull)
				result = new LanguageType(BasicType.Int);

			LanguageType = result;
		}


		private void SolveUnassignedType() {
			if (!_left.LanguageType.IsUnasigned && !_right.LanguageType.IsUnasigned)
				return;

			BinaryOperatorType type = Token.Operator.BinaryOperatorType;
			LanguageType lType = _left.LanguageType;

			if (type == BinaryOperatorType.LogicalOperator || type == BinaryOperatorType.Shift || type == BinaryOperatorType.ShiftAssign) {
				lType = new LanguageType(BasicType.Int);
			} else {
				if (lType.IsUnasigned)
					lType = _right.LanguageType;
				if (lType.IsUnasigned || lType.BasicType == BasicType.Null || lType.BasicType == BasicType.Void)
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnassignedType);
			}

			lType.Modifier |= Modifier.ConstV;

			if (_left.LanguageType.IsUnasigned)
				_left.SpecifyUnknownType(lType);
			if (_right.LanguageType.IsUnasigned)
				_right.SpecifyUnknownType(lType);
		}



		protected override ExprNode GetChild(int index) {
			switch (index) {
				case 0:
					return _left;
				case 1:
					return _right;
				default:
					throw new ArgumentOutOfRangeException("index");
			}
		}

		protected override int GetChildrenCount() {
			return 2;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.Binary;
		}

		protected override ConstantValue CalculateConstant() {
			return ConstantValue.DoBinary(_left.ConstantValue, _right.ConstantValue, this);
		}

		protected override void CheckConstantType() {
			if (_right.ConstantValue != null && (Token.Operator.BinaryOperatotDescription.ConstBehavior & OperatorConstBehavior.LValueRequiered) != 0) {
				ConstantValue.CheckType(_right, _left.LanguageType);
			}
		}
	}










	public class ExprArrayAccess : ExprNode
	{
		private ExprNode _array;
		public ExprNode Array {
			get { return _array; }
			set { _array = value; }
		}

		private ExprNode _index;
		public ExprNode Index {
			get { return _index; }
			set { _index = value; }
		}

		OperatorArgumentCasts _cast;
		public OperatorArgumentCasts Cast {
			get { return _cast; }
		}


		// CONSTRUCTOR
		internal ExprArrayAccess(Expression expression, LexicalToken token, ExprNode array, ExprNode index) 
			: base(expression, token) 
		{
			_array = array;
			_index = index;

			if (!_array.LanguageType.IsUnasigned) {

				if (_array.LanguageType.IsType(OperatorRequieredArguments.Array, false) == 0)
					Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EArrayRequiered);
				
				LanguageType type = _array.LanguageType;
				type.DecreaseDimCount();
				LanguageType = type;
			}

			_cast = _index.CheckType(new LanguageType(BasicType.Int, Modifier.ConstV));

		}

		internal override void SpecifyUnknownType(LanguageType type) {
			LanguageType = type;
			type.IncreaseDimCount(true);
			_array.SpecifyUnknownType(type);
		}

		public override bool IsLValue() {
			return true;
		}

		protected override ExprNode GetChild(int index) {
			switch (index) {
				case 0:
					return _array;
				case 1:
					return _index;
				default:
					throw new ArgumentOutOfRangeException("index");
			}
		}

		protected override int GetChildrenCount() {
			return 2;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.ArrayAccess;
		}

		protected override ConstantValue CalculateConstant() {
			return _array.ConstantValue.ArrayAccess(_index.ConstantValue, this);
		}
	}











	public abstract class ExprUnaryBase : ExprNode
	{
		private ExprNode _child;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public ExprNode Child {
			get { return _child; }
			set { _child = value; }
		}


		// CONSTRUCTOR
		internal ExprUnaryBase(Expression expression, LexicalToken token, ExprNode child)
			: base(expression, token) 
		{
			_child = child;
		}


		protected override ExprNode GetChild(int index) {
			if (index == 0) {
				return _child;
			} else {
				throw new ArgumentOutOfRangeException("index");
			}
		}

		protected override int GetChildrenCount() {
			return 1;
		}

	}










	public sealed class ExprUnary : ExprUnaryBase
	{

		private bool _postFix;
		public bool PostFix {
			get { return _postFix; }
			set { _postFix = value; }
		}

		// CONSTRUCTOR
		internal ExprUnary(Expression expression, LexicalToken token, ExprNode child) : this(expression, token, child, false) { }
		internal ExprUnary(Expression expression, LexicalToken token, ExprNode child, bool postFix)
			: base(expression, token, child) 
		{
			_postFix = postFix;

			CheckArgumentType();
			CheckLValue(token.Operator.UnaryOperatorDescription.ConstBehavior);

			if (token.Operator.UnaryOperatorDescription.Result == OperatorResult.Int || child.LanguageType.IsNull) {
				LanguageType = new LanguageType(BasicType.Int);
			} else {
				LanguageType = child.LanguageType;
			}
		}

		internal override void SpecifyUnknownType(LanguageType type) {
			LanguageType = type;
			CheckArgumentType();
			Child.SpecifyUnknownType(type);
		}

		private void CheckArgumentType() {
			if (Child.LanguageType.IsUnasigned)
				return;
			if (Child.LanguageType.IsType(Token.Operator.UnaryOperatorDescription.RequieredArguments, true) == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ETypeMismatchUnary, Child.LanguageType, Token.Operator);
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.Unary;
		}


		protected override ConstantValue CalculateConstant() {
			return Child.ConstantValue.DoUnary(this);
		}
	}












	public abstract class ExprLeaf : ExprNode
	{

		// CONSTRUCTOR
		internal ExprLeaf(Expression expression, LexicalToken token) : base(expression, token) { }

		
		protected override ExprNode GetChild(int index) {
			throw new ArgumentOutOfRangeException("index");
		}
		protected override int GetChildrenCount() {
			return 0;
		}
	}











	public class ExprConstant : ExprLeaf
	{
		// CONSTRUCTOR
		internal ExprConstant(Expression expression, LexicalToken token) 
			: base(expression, token) 
		{
			switch (token.Type) {
				case LexicalTokenType.Double:
					this.LanguageType = new LanguageType(BasicType.Double);
					break;
				case LexicalTokenType.Char:
					this.LanguageType = new LanguageType(BasicType.Char);
					break;
				case LexicalTokenType.Int:
					this.LanguageType = new LanguageType(BasicType.Int);
					break;
				case LexicalTokenType.String:
					this.LanguageType = new LanguageType(BasicType.Char, Modifier.None, 1, null);
					break;
				case LexicalTokenType.Keyword:
					switch (token.Keyword.ConstantKeyword) {
						case ConstantKeyword.Everything:
						case ConstantKeyword.Nothing:
							this.LanguageType = new LanguageType(BasicType.Name);
							break;
						case ConstantKeyword.False:
						case ConstantKeyword.True:
							this.LanguageType = new LanguageType(BasicType.Int);
							break;
						case ConstantKeyword.Null:
							this.LanguageType = new LanguageType(BasicType.Null);
							break;
						case ConstantKeyword.This:
							if (Expression.Syntax.ExpressionContext == ExpressionContext.StaticMethod) {
								Expression.Syntax.ErrorLog.LogError(token, ErrorCode.WThisInStaticIsNull);
								this.LanguageType = new LanguageType(BasicType.Object, Expression.Syntax.ConstantMethod ? Modifier.ConstO : Modifier.None);
							} else if ((Expression.Syntax.ExpressionContext & ExpressionContext.MemberNotAllowed) != 0) {
								Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAccessMembersNotAllowed);
								throw new InternalCompilerException();
							} else {
								this.LanguageType = new LanguageType(BasicType.Object, Expression.Syntax.ConstantMethod ? Modifier.ConstO : Modifier.None, 0, Expression.Syntax.ThisClass);
							}
							break;
						case ConstantKeyword.Sender:
							if ((Expression.Syntax.ExpressionContext & ExpressionContext.Method) == 0)
								Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAccessMembersNotAllowed);
							this.LanguageType = new LanguageType(BasicType.Object, Expression.Syntax.ConstantMethod ? Modifier.ConstO : Modifier.None);
							break;
						default:
							throw new InternalCompilerException();
					}
					break;
				default:
					throw new InternalCompilerException();
					
			}
		}


		public override ExprNodeType GetNodeType() {
			return ExprNodeType.Constant;
		}


		protected override ConstantValue CalculateConstant() {
			switch (Token.Type) {
				case LexicalTokenType.Double:
					return new DoubleConstantValue(Token.Double);
				case LexicalTokenType.Char:
					return new CharConstantValue(Token.Char);
				case LexicalTokenType.Int:
					return new IntConstantValue(Token.Int);
				case LexicalTokenType.String:
					return new ArrayConstantValue(Token.Text);
				case LexicalTokenType.Keyword:
					switch (Token.Keyword.ConstantKeyword) {
						case ConstantKeyword.Everything:
							return new NameConstantValue(Expression.Syntax.Compilation.CompilerKnownName(CompilerKnownName.Everything));
						case ConstantKeyword.Nothing:
							return new NameConstantValue(Expression.Syntax.Compilation.CompilerKnownName(CompilerKnownName.Nothing));
						case ConstantKeyword.False:
							return new IntConstantValue(0);
						case ConstantKeyword.True:
							return new IntConstantValue(1);
						case ConstantKeyword.Null:
							return new NullConstantValue();
					}
					break;
			}
			return null;
		}
	}












	public class ExprLocalVariable : ExprLeaf
	{
		LocalVariable _variable;
		public LocalVariable Variable {
			get { return _variable; }
			set { _variable = value; }
		}

		// CONSTRUCTOR
		internal ExprLocalVariable(Expression expression, LexicalToken token, LocalVariable variable)
			: base(expression, token) 
		{ 
			_variable = variable;
			LanguageType = variable.LanguageType;
		}

		public override bool IsLValue() {
			return true;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.LocalVariable;
		}


		protected override ConstantValue CalculateConstant() {
			if (_variable.LanguageType.IsAllConst && _variable.Declaration != null && _variable.Declaration.Initialization != null && _variable.Declaration.Initialization.RootNode != null && _variable.Declaration.Initialization.RootNode.ConstantValue != null) {
				return _variable.Declaration.Initialization.RootNode.ConstantValue.MakeCopy(LanguageType, false);
			}
			return null;
		}
	}












	public class ExprAssigned : ExprLeaf
	{
		LocalVariable _parameter;
		public LocalVariable Parameter {
			get { return _parameter; }
		}

		// CONSTRUCTOR
		internal ExprAssigned(Expression expression, LexicalToken token)
			: base(expression, token) 
		{
			if (Expression.Method == null || !CompilerConstants.IsNameTypeSafeMethod(Expression.Method.MethodField.Name.NameType))
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAssignedCannotBeUsed);

			Expression.Syntax.DoClosingBracket(OperatorType.LeftParenthesis);

			LexicalToken prmToken = Expression.Syntax.TryReadToken(LexicalTokenType.Identifier);
			if (prmToken == null || !prmToken.Identifier.IsSimple)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Expression.Syntax.Lexical.CurrentToken, ErrorCode.EParameterNameRequiered);
			LocalVariable parameter;
			Expression.Variables.TryGetValue(prmToken.Identifier.Simple, out parameter);
			if (parameter == null || parameter.Parameter == null)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Expression.Syntax.Lexical.CurrentToken, ErrorCode.EParameterNameRequiered);

			Expression.Syntax.DoClosingBracket(OperatorType.RightParenthesis);

			_parameter = parameter;
			LanguageType = new LanguageType(BasicType.Int);
		}


		public override ExprNodeType GetNodeType() {
			return ExprNodeType.Assigned;
		}
	}









	public class ExprKsidName : ExprLeaf
	{
		KsidName _name;
		public KsidName Name {
			get { return _name; }
			set { _name = value; }
		}

		// CONSTRUCTOR
		internal ExprKsidName(Expression expression, LexicalToken token, KsidName name)
			: base(expression, token) 
		{
			_name = name;
			LanguageType = new LanguageType(BasicType.Name);
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.KsidName;
		}

		protected override ConstantValue CalculateConstant() {
			return new NameConstantValue(_name);
		}
	}














	public class ExprObjectAccess : ExprUnaryBase
	{
		TypedKsidName _variableName;
		public TypedKsidName VariableName {
			get { return _variableName; }
			set { _variableName = value; }
		}

		ClassName _objectType;
		public ClassName ObjectType {
			get { return _objectType; }
		}

		// CONSTRUCTOR
		internal ExprObjectAccess(Expression expression, LexicalToken token, ExprNode child, TypedKsidName variableName)
			: base(expression, token, child) 
		{
			bool isConstant;
			_objectType = CheckIfIsObject(child, Expression.Syntax, Token, out isConstant);

			Expression.Syntax.CheckIfFieldIsMember(variableName, _objectType, token);

			_variableName = variableName;
			LanguageType lt = _variableName.LanguageType.ClearModifier(ModifierGroups.VariableConstGroup);
			if (isConstant)
				lt.Modifier |= Modifier.ConstV;
			LanguageType = lt;

		}

		public override bool IsLValue() {
			return true;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.ObjectAccess;
		}
	}











	public class ExprStaticAccess : ExprLeaf
	{
		StaticVariableName _variableName;
		public StaticVariableName VariableName {
			get { return _variableName; }
			set { _variableName = value; }
		}

		// CONSTRUCTOR
		internal ExprStaticAccess(Expression expression, LexicalToken token, StaticVariableName variableName)
			: base(expression, token) 
		{
			if (!variableName.IsConstant && (Expression.Syntax.ExpressionContext & ExpressionContext.StaticNotAllowed) != 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAccessStaticNotAllowed);
			_variableName = variableName;
			LanguageType = variableName.LanguageType.ClearModifier(ModifierGroups.VariableConstGroup);
		}

		public override bool IsLValue() {
			return true;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.StaticAccess;
		}


		protected override ConstantValue CalculateConstant() {
			if (_variableName.IsConstant) {
				ConstantValue constant = _variableName.GetConstantValue();
				if (constant == null)
					Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnableToRetrieveConstant, _variableName);
				bool isIndependent = constant.IsProjectIndependent && _variableName.VariableField.Field.Syntax.Lexical == Expression.Syntax.Lexical;
				return constant.MakeCopy(LanguageType, isIndependent);
			} else {
				return null;
			}
		}
	}














	public abstract class ExprCall : ExprNode
	{
		ExprCallType _callType;
		public ExprCallType CallType {
			get { return _callType; }
			set { _callType = value; }
		}


		ExprNode _obj;
		public ExprNode Obj {
			get { return _obj; }
			set { _obj = value; }
		}

		ClassName _objectType;
		public ClassName ObjectType {
			get { return _objectType; }
			set { _objectType = value; }
		}


		ExprNode _method;
		public ExprNode Method {
			get { return _method; }
			set { _method = value; }
		}

		MethodName _methodName;
		public MethodName MethodName {
			get { return _methodName; }
			set { _methodName = value; }
		}


		List<ExprNode> _parameters = new List<ExprNode>();
		public IList<ExprNode> Parameters {
			get { return _parameters; }
		}


		// CONSTRUCTOR
		internal ExprCall(Expression expression, LexicalToken token) : base(expression, token) 
		{
			if ((Expression.Syntax.ExpressionContext & ExpressionContext.ConstantRequiered) != 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EConstantRequiered);
		}


		protected override int GetChildrenCount() {
			return _parameters.Count + 2;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "index-2")]
		protected override ExprNode GetChild(int index) {
			switch (index) {
				case 0:
					return _obj;
				case 1:
					return _method;
				default:
					return _parameters[index - 2];
			}
		}
	}












	public class ExprDirectCall : ExprCall
	{


		List<OperatorArgumentCasts> _parameterCasts = new List<OperatorArgumentCasts>();
		public IList<OperatorArgumentCasts> ParametersCasts {
			get { return _parameterCasts; }
		}


		// CONSTRUCTOR
		internal ExprDirectCall(Expression expression, LexicalToken token, ExprNode obj, ExprKsidName method) 
			: base(expression, token) 
		{
			Method = method;
			MethodName = (MethodName)method.Name;

			if (MethodName.DMInheritedFrom == null)
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EDMNotImplemented);

			if (method.Name.NameType == NameType.DirectMethod) {
				Obj = obj;
				bool isConst;
				ObjectType = CheckIfIsObject(obj, Expression.Syntax, Token, out isConst);
				Expression.Syntax.CheckIfFieldIsMember(MethodName, ObjectType, Token);
				if (isConst && !MethodName.IsConstant)
					Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.ECannotCallNonConstMethod);
				CallType = ExprCallType.Direct;
			} else {
				if ((Expression.Syntax.ExpressionContext & ExpressionContext.StaticNotAllowed) != 0)
					Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAccessStaticNotAllowed);
				CallType = ExprCallType.Direct | ExprCallType.Static;
			}

			Expression.Syntax.DoBody(DoBodyParameters.DoMethodCall, DoParameter, OperatorType.Comma, OperatorType.RightParenthesis);

			if (Parameters.Count != MethodName.ParameterLists[0].Count)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EWrongNumberOfArguments);
			// todo optional arguments

			LanguageType = MethodName.LanguageType.ClearModifier(ModifierGroups.VariableConstGroup);
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.DirectCall;
		}

		private bool DoParameter() {
			int pos = Parameters.Count;
			ParameterList parameters = MethodName.ParameterLists[0];

			if (pos >= parameters.Count)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EWrongNumberOfArguments);

			ExprNode node = Expression.DoBinaryOperator();
			OperatorArgumentCasts cast = node.CheckType(parameters[pos].Type);

			Parameters.Add(node);
			_parameterCasts.Add(cast);
			
			return true;
		}


		protected override void CheckConstantType() {
			ParameterList parameters = MethodName.ParameterLists[0];
			for (int f = 0; f < Parameters.Count; f++ ) {
				ConstantValue.CheckType(Parameters[f], parameters[f].Type);
			}
		}
	}












	public class ExprSafeCall : ExprCall
	{
		List<ExprNode> _paramNames = new List<ExprNode>();
		public IList<ExprNode> ParamNames {
			get { return _paramNames; }
		}

		MessageType _messageType;
		public MessageType MessageType {
			get { return _messageType; }
		}


		ExprNode _meassageArgument;
		public ExprNode MeassageArgument {
			get { return _meassageArgument; }
		}

		OperatorArgumentCasts _meassageArgumentCast;
		public OperatorArgumentCasts MeassageArgumentCast {
			get { return _meassageArgumentCast; }
		}

		// CONSTRUCTOR
		internal ExprSafeCall(Expression expression, LexicalToken token, ExprCallType callType, ExprNode obj, ExprNode method) 
			: base(expression, token) 
		{
			Obj = obj;
			Method = method;
			CallType = callType;
			bool isConstant = false;

			if (callType == ExprCallType.New) {
				ObjectType = obj.CheckIfIsName(NameType.Class) as ClassName;
				MethodName = (MethodName)Expression.Syntax.Compilation.CompilerKnownName(CompilerKnownName.Constructor);
			} else if (callType == ExprCallType.Static) {
				if ((Expression.Syntax.ExpressionContext & ExpressionContext.StaticNotAllowed) != 0)
					Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EAccessStaticNotAllowed);
				MethodName = method.CheckIfIsName(NameType.StaticSafeMethod) as MethodName;
			} else {
				ObjectType = CheckIfIsObject(obj, Expression.Syntax, Token, out isConstant);
				MethodName = method.CheckIfIsName(NameType.SafeMethod) as MethodName;
			}

			if (isConstant && MethodName != null && !MethodName.IsConstant)
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.ECannotCallNonConstMethod);

			Expression.Syntax.DoBody(DoBodyParameters.DoMethodCall, DoParameter, OperatorType.Comma, OperatorType.RightParenthesis);

			LexicalToken msgToken = Expression.Syntax.Lexical.Peek();
			if (msgToken.Type == LexicalTokenType.Keyword && msgToken.Keyword.MessageType != MessageType.None) {
				DoMessage();
				LanguageType = new LanguageType(BasicType.Void);
			} else {
				if (callType == ExprCallType.New) {
					LanguageType = new LanguageType(BasicType.Object, Modifier.None, 0, ObjectType);
				} else {
					if (MethodName != null) {
						LanguageType = MethodName.LanguageType.ClearModifier(ModifierGroups.VariableConstGroup);
					} 
					// else LanguageType = Unasigned
				}
			}
		}



		private void DoMessage() {
			LexicalToken token = Expression.Syntax.Lexical.Read();
			_messageType = token.Keyword.MessageType;

			if (MessageType == MessageType.Timed) {
				_meassageArgument = Expression.DoBinaryOperator();
				_meassageArgumentCast = _meassageArgument.CheckType(new LanguageType(BasicType.Int, Modifier.ConstV));
			} else if (MessageType == MessageType.CallEnd) {
				_meassageArgument = Expression.DoBinaryOperator();
				_meassageArgumentCast = _meassageArgument.CheckType(new LanguageType(BasicType.Object, Modifier.ConstV | Modifier.ConstO));
			}
		}



		private bool DoParameter() {
			SyntaxTemplatesEx syntax = Expression.Syntax;
			bool wasRet = false;

			if (syntax.TryReadToken(KeywordType.Ret))
				wasRet = true;

			ExprNode paramName = Expression.DoKsidName(MethodName != null ? MethodName.Identifier : syntax.ObjectContext);
			paramName.CheckIfIsName(NameType.Param);
			_paramNames.Add(paramName);

			if (!syntax.TryReadToken(OperatorType.Assign))
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, OperatorType.Assign);

			ExprNode parameter = Expression.DoBinaryOperator();
			if (parameter.LanguageType.IsUnasigned) 
				syntax.ErrorLog.ThrowAndLogError(parameter.Token, ErrorCode.EUnassignedType);
			if (parameter.LanguageType.IsVoid)
				syntax.ErrorLog.ThrowAndLogError(parameter.Token, ErrorCode.ETypeVoidNotAllowed);
			if (wasRet) {
				if (!parameter.IsLValue())
					Expression.Syntax.ErrorLog.ThrowAndLogError(parameter.Token, ErrorCode.ELValueRequiered);
				LanguageType lType = parameter.LanguageType;
				lType.Modifier |= Modifier.Ret;
				parameter.LanguageType = lType;
			}
			Parameters.Add(parameter);

			return true;
		}


		internal override void SpecifyUnknownType(LanguageType type) {
			LanguageType = type;
		}

		protected override int GetChildrenCount() {
			return Parameters.Count * 2 + 3;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "index-3")]
		protected override ExprNode GetChild(int index) {
			switch (index) {
				case 0:
					return Obj;
				case 1:
					return Method;
				case 2:
					return _meassageArgument;
				default:
					index -= 3;
					if ((index & 1) != 0) {
						return Parameters[index >> 1];
					} else {
						return _paramNames[index >> 1];
					}
			}
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.SafeCall;
		}
	}











	public class ExprSystemMethod : ExprNode
	{
		SystemMethod _systemMethod;
		public SystemMethod SystemMethod {
			get { return _systemMethod; }
		}

		ExprNode _obj;
		public ExprNode Obj {
			get { return _obj; }
		}

		LanguageType _templateType;
		public LanguageType TemplateType {
			get { return _templateType; }
		}

		LanguageType _templateMember;
		public LanguageType TemplateMember {
			get { return _templateMember; }
		}
		
		List<ExprNode> _parameters = new List<ExprNode>();
		public IList<ExprNode> Parameters {
			get { return _parameters; }
		}

		List<OperatorArgumentCasts> _parameterCasts = new List<OperatorArgumentCasts>();
		public IList<OperatorArgumentCasts> ParametersCasts {
			get { return _parameterCasts; }
		}

		List<LanguageType> _parameterTypes = new List<LanguageType>();
		public IList<LanguageType> ParameterTypes {
			get { return _parameterTypes; }
		}


		// CONSTRUCTOR
		internal ExprSystemMethod(Expression expression, LexicalToken token, SystemMethod systemMethod, ExprNode obj) 
			: base(expression, token)
		{
			if ((Expression.Syntax.ExpressionContext & ExpressionContext.ConstantRequiered) != 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EConstantRequiered);

			_systemMethod = systemMethod;
			_obj = obj;
			_templateType = _obj.LanguageType;
			if (_templateType.DimensionsCount > 0) {
				_templateMember = _templateType;
				_templateMember.DecreaseDimCount();
			}

			if (_obj.LanguageType.IsPointerToConst && !systemMethod.IsConstant)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.ECannotCallNonConstMethod);

			Expression.Syntax.DoBody(DoBodyParameters.DoMethodCall, DoParameter, OperatorType.Comma, OperatorType.RightParenthesis);

			if (Parameters.Count != _systemMethod.Arguments.Count)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EWrongNumberOfArguments);
			// todo optional arguments

			LanguageType = CalculateType(_systemMethod.RuturnType);
		}


		private LanguageType CalculateType(SystemMethodArgument argument) {
			LanguageType ret;
			switch (argument.TemplateType) {
				case SystemMethodTemplateType.Same:
					ret = _templateType;
					ret.Modifier |= argument.LanguageType.Modifier;
					return ret;
				case SystemMethodTemplateType.SameConstO:
					ret = _templateType;
					ret.Modifier |= argument.LanguageType.Modifier;
					ret.IsPointerToConst = true;
					return ret;
				case SystemMethodTemplateType.SameRemoveConstO:
					ret = _templateType;
					ret.Modifier |= argument.LanguageType.Modifier;
					ret.IsPointerToConst = false;
					return ret;
				case SystemMethodTemplateType.SameConstAll:
					ret = _templateType;
					ret.Modifier |= argument.LanguageType.Modifier;
					if (ret.BasicType == BasicType.Object)
						ret.Modifier |= Modifier.ConstO;
					for (int f = 0; f < ret.DimensionsCount; f++) {
						ret.SetArrayConst(f, true);
					}
					return ret;
				case SystemMethodTemplateType.ArrayMember:
					if (_templateMember.IsUnasigned)
						throw new InternalCompilerException("Wrong use of System Method. Template Member Type is unasigned!");
					ret = _templateMember;
					ret.Modifier |= argument.LanguageType.Modifier;
					return _templateMember;
				default:
					return argument.LanguageType;
			}
		}


		protected override int GetChildrenCount() {
			return _parameters.Count + 1;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "index-2")]
		protected override ExprNode GetChild(int index) {
			if (index == 0) {
				return _obj;
			} else {
				return _parameters[index - 1];
			}
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.SystemMethod;
		}


		private bool DoParameter() {
			int pos = Parameters.Count;

			if (pos >= _systemMethod.Arguments.Count)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EWrongNumberOfArguments);

			ExprNode node = Expression.DoBinaryOperator();
			LanguageType type = CalculateType(_systemMethod.Arguments[pos]);
			OperatorArgumentCasts cast = node.CheckType(type);

			Parameters.Add(node);
			_parameterCasts.Add(cast);
			_parameterTypes.Add(type);

			return true;
		}


		protected override void CheckConstantType() {
			for (int f = 0; f < Parameters.Count; f++) {
				ConstantValue.CheckType(Parameters[f], _parameterTypes[f]);
			}
		}

	}











	public class ExprArray : ExprNode
	{
		List<ExprNode> _fields = new List<ExprNode>();
		public IList<ExprNode> Fields {
			get { return _fields; }
		}

		List<OperatorArgumentCasts> _casts = new List<OperatorArgumentCasts>();
		public IList<OperatorArgumentCasts> Casts {
			get { return _casts; }
		}


		// CONSTRUCTOR
		internal ExprArray(Expression expression, LexicalToken token)
			: base(expression, token) 
		{
			Expression.Syntax.DoBody(DoBodyParameters.DoArrayConstriction, DoField, OperatorType.Comma, OperatorType.RightCurlyBracket);
		}


		private bool DoField() {
			ExprNode field = Expression.DoBinaryOperator();
			_fields.Add(field);
			return true;
		}


		internal override void SpecifyUnknownType(LanguageType type) {
			if (type.BasicType == BasicType.Void)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnassignedType);
			if (type.DimensionsCount == 0)
				Expression.Syntax.ErrorLog.ThrowAndLogError(Token, ErrorCode.EUnableToConvertTypeTo, type);

			LanguageType = type;
			
			type.DecreaseDimCount();
			foreach (ExprNode field in _fields) {
				OperatorArgumentCasts cast = field.CheckType(type);
				_casts.Add(cast);
			}
		}




		protected override ExprNode GetChild(int index) {
			return _fields[index];
		}

		protected override int GetChildrenCount() {
			return _fields.Count;
		}

		public override ExprNodeType GetNodeType() {
			return ExprNodeType.Array;
		}



		protected override ConstantValue CalculateConstant() {
			return new ArrayConstantValue(_fields);
		}

		protected override void CheckConstantType() {
			if (LanguageType.DimensionsCount == 1) {
				LanguageType lt = LanguageType;
				lt.DecreaseDimCount();
				foreach (ExprNode node in _fields) {
					ConstantValue.CheckType(node, lt);
				}
			}
		}
	}


}



