using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	public enum ConstantType {
		Unasigned,
		Char,
		Int,
		Double,
		Name,
		Array,
		Null,
	}

	public abstract class ConstantValue
	{
		ConstantType _type;
		public ConstantType Type {
			get { return _type; }
		}


		bool _isProjectIndependent;
		public bool IsProjectIndependent {
			get { return _isProjectIndependent; }
			set { _isProjectIndependent = value; }
		}


		// CONSTRUCTOR
		protected ConstantValue(ConstantType type, bool isProjectIndependent) {
			_type = type;
			_isProjectIndependent = isProjectIndependent;
		}


		public abstract bool GetBool();


		public static ConstantValue DoBinary(ConstantValue v1, ConstantValue v2, ExprBinary expr) {
			try {
				ConstantValue res = DoBinary2(v1, v2, expr);
				if (res != null)
					res.IsProjectIndependent = v1.IsProjectIndependent && v2.IsProjectIndependent;
				return res;
			}
			catch (DivideByZeroException) {
				expr.Expression.Syntax.ErrorLog.LogError(expr.Token, ErrorCode.EDivisionByZero);
				return null;
			}
		}

		public static ConstantValue DoBinary2(ConstantValue v1, ConstantValue v2, ExprBinary expr) {
			if (v1 == null || v2 == null)
				return null;

			switch (expr.Token.Operator.Type) {
				case OperatorType.DoubleAnd:
					return new IntConstantValue(v1.GetBool() && v2.GetBool() ? 1 : 0);
				case OperatorType.DoubleOr:
					return new IntConstantValue(v1.GetBool() || v2.GetBool() ? 1 : 0);
			}

			if (expr.LanguageType.DimensionsCount > 0)
				return DoArrayBinary(v1, v2, expr);

			NumericConstantValue v1n = v1 as NumericConstantValue;
			NumericConstantValue v2n = v2 as NumericConstantValue;
			if (v1n == null || v2n == null)
				return null;

			if ((v1n.Type == ConstantType.Double || v1n.Type == ConstantType.Double) && expr.LanguageType.BasicType == BasicType.Int)
				return (DoDoubleToIntBinary(v1n, v2n, expr));


			switch (expr.LanguageType.BasicType) {
				case BasicType.Int:
				case BasicType.Char:
					return DoIntBinary(v1n, v2n, expr);
				case BasicType.Double:
					return DoDoubleBinary(v1n, v2n, expr);
				case BasicType.Null:
					return new NullConstantValue();
				default:
					return null;
			}
		}




		private static ConstantValue DoArrayBinary(ConstantValue v1, ConstantValue v2, ExprBinary expr) {
			if (expr.Token.Operator.Type == OperatorType.Plus) {
				return new ArrayConstantValue(v1 as ArrayConstantValue, v2 as ArrayConstantValue);
			} else {
				return null;
			}
		}



		private static ConstantValue DoIntBinary(NumericConstantValue v1, NumericConstantValue v2, ExprBinary expr) {
			switch (expr.Token.Operator.Type) {
				case OperatorType.And:
					return MakeIntCharResult(v1.GetInt() & v2.GetInt(), expr);
				case OperatorType.Division:
					return MakeIntCharResult(v1.GetInt() / v2.GetInt(), expr);
				case OperatorType.Equal:
					return MakeIntCharResult(v1.GetInt() == v2.GetInt() ? 1 : 0, expr);
				case OperatorType.Greater:
					return MakeIntCharResult(v1.GetInt() > v2.GetInt() ? 1 : 0, expr);
				case OperatorType.GreaterEqual:
					return MakeIntCharResult(v1.GetInt() >= v2.GetInt() ? 1 : 0, expr);
				case OperatorType.LeftShift:
					return MakeIntCharResult(v1.GetInt() << v2.GetInt(), expr);
				case OperatorType.Less:
					return MakeIntCharResult(v1.GetInt() < v2.GetInt() ? 1 : 0, expr);
				case OperatorType.LessEqual:
					return MakeIntCharResult(v1.GetInt() <= v2.GetInt() ? 1 : 0, expr);
				case OperatorType.Minus:
					return MakeIntCharResult(v1.GetInt() - v2.GetInt(), expr);
				case OperatorType.Modulo:
					return MakeIntCharResult(v1.GetInt() % v2.GetInt(), expr);
				case OperatorType.Multiply:
					return MakeIntCharResult(v1.GetInt() * v2.GetInt(), expr);
				case OperatorType.NotEqual:
					return MakeIntCharResult(v1.GetInt() != v2.GetInt() ? 1 : 0, expr);
				case OperatorType.Or:
					return MakeIntCharResult(v1.GetInt() | v2.GetInt(), expr);
				case OperatorType.Plus:
					return MakeIntCharResult(v1.GetInt() + v2.GetInt(), expr);
				case OperatorType.RightShift:
					return MakeIntCharResult(v1.GetInt() >> v2.GetInt(), expr);
				case OperatorType.Xor:
					return MakeIntCharResult(v1.GetInt() ^ v2.GetInt(), expr);
				default: 
					return null;
			}
		}

		
		private static ConstantValue DoDoubleBinary(NumericConstantValue v1, NumericConstantValue v2, ExprBinary expr) {
			switch (expr.Token.Operator.Type) {
				case OperatorType.Division:
					return new DoubleConstantValue(v1.GetDouble() / v2.GetDouble());
				case OperatorType.Minus:
					return new DoubleConstantValue(v1.GetDouble() - v2.GetDouble());
				case OperatorType.Multiply:
					return new DoubleConstantValue(v1.GetDouble() * v2.GetDouble());
				case OperatorType.Plus:
					return new DoubleConstantValue(v1.GetDouble() + v2.GetDouble());
				default:
					return null;
			}
		}
		
		
		private static ConstantValue DoDoubleToIntBinary(NumericConstantValue v1, NumericConstantValue v2, ExprBinary expr) {
			switch (expr.Token.Operator.Type) {
				case OperatorType.Equal:
					return new IntConstantValue(v1.GetDouble() == v2.GetDouble() ? 1 : 0);
				case OperatorType.Greater:
					return new IntConstantValue(v1.GetDouble() > v2.GetDouble() ? 1 : 0);
				case OperatorType.GreaterEqual:
					return new IntConstantValue(v1.GetDouble() >= v2.GetDouble() ? 1 : 0);
				case OperatorType.Less:
					return new IntConstantValue(v1.GetDouble() < v2.GetDouble() ? 1 : 0);
				case OperatorType.LessEqual:
					return new IntConstantValue(v1.GetDouble() <= v2.GetDouble() ? 1 : 0);
				case OperatorType.NotEqual:
					return new IntConstantValue(v1.GetDouble() != v2.GetDouble() ? 1 : 0);
				default:
					return null;
			}
		}


		private static ConstantValue MakeIntCharResult(int p, ExprBinary expr) {
			if (expr.LanguageType.BasicType == BasicType.Char) {
				return new CharConstantValue((char)p);
			} else {
				return new IntConstantValue(p);
			}
		}


		public virtual ConstantValue ArrayAccess(ConstantValue index, ExprNode expr) {
			throw new InternalCompilerException("this is not array!");
		}


		public abstract ConstantValue MakeCopy(LanguageType lt, bool isProjectIndependent);


		public static bool MemberwiseCompare(LanguageType lt, ConstantValue v1, ConstantValue v2) {
			if (v1.GetBool() == v2.GetBool()) {
				if (v1.GetBool()) {
					if (lt.DimensionsCount > 0) {
						return CompareArrayMembers(lt, (ArrayConstantValue)v1, (ArrayConstantValue)v2);
					} else {
						switch (lt.BasicType) {
							case BasicType.Double:
								return ((NumericConstantValue)v1).GetDouble() == ((NumericConstantValue)v2).GetDouble();
							case BasicType.Int:
							case BasicType.Char:
								return ((NumericConstantValue)v1).GetInt() == ((NumericConstantValue)v2).GetInt();
							case BasicType.Name:
								return ((NameConstantValue)v1).Name == ((NameConstantValue)v2).Name;
							default:
								return false;
						}
					}
				} else {
					return true;  // null == null
				}
			} else {
				return false; // null == !null
			}
		}



		private static bool CompareArrayMembers(LanguageType lt, ArrayConstantValue v1, ArrayConstantValue v2) {
			lt.DecreaseDimCount();
			if (v1.Array.Count != v2.Array.Count) {
				return false;
			} else {
				for (int f = 0; f < v1.Array.Count; f++) {
					if (!MemberwiseCompare(lt, v1.Array[f], v2.Array[f]))
						return false;
				}
			}
			return true;
		}


		public ConstantValue DoUnary(ExprUnary expr) {
			if (expr.Token.Operator.UnaryOpType == UnaryOperatorType.PlusPlus)
				return null;
			if (expr.Token.Operator.Type == OperatorType.Exclamation)
				return new IntConstantValue(GetBool() ? 0 : 1, IsProjectIndependent);

			if (_type == ConstantType.Array || _type == ConstantType.Name)
				throw new InternalCompilerException();

			ConstantValue res;
			if (_type == ConstantType.Null) {
				res = new IntConstantValue(0);
				res = res.MakeCopy(expr.LanguageType, true);
			} else {
				res = MakeCopy(expr.LanguageType, IsProjectIndependent);
			}
			res.ApplyUnaryOperator(expr.Token.Operator);
			return res;
		}


		protected virtual void ApplyUnaryOperator(LanguageOperator languageOperator) {
			throw new InternalCompilerException();
		}

		internal static void CheckType(ExprNode node, LanguageType expectedType) {
			if (node.ConstantValue != null && expectedType.DimensionsCount == 0) {
				EnumName en = expectedType.ObjectType as EnumName;
				if (en != null) {
					switch (en.Type) {
						case EnumSubType.Name:
							node.ConstantValue.CheckEnumName(en, node);
							break;
						case EnumSubType.Int:
							node.ConstantValue.CheckEnumInt(en, node);
							break;
						case EnumSubType.Flags:
							node.ConstantValue.CheckEnumFlags(en, node);
							break;
					}
				}
			}
		}

		protected virtual void CheckEnumInt(EnumName enumName, ExprNode node) {
			throw new InternalCompilerException();
		}

		protected virtual void CheckEnumFlags(EnumName enumName, ExprNode node) {
			throw new InternalCompilerException();
		}

		protected virtual void CheckEnumName(EnumName enumName, ExprNode node) {
			throw new InternalCompilerException();
		}
	}








	public abstract class NumericConstantValue : ConstantValue
	{

		// CONSTRUCTOR
		protected NumericConstantValue(ConstantType type, bool isProjectIndependent) : base(type, isProjectIndependent) { }

		public abstract char GetChar();
		public abstract int GetInt();
		public abstract double GetDouble();

		public override ConstantValue MakeCopy(LanguageType lt, bool isProjectIndependent) {
			if (lt.DimensionsCount > 0) {
				throw new InternalCompilerException();
			}

			switch (lt.BasicType) {
				case BasicType.Double:
					return new DoubleConstantValue(GetDouble(), isProjectIndependent);
				case BasicType.Char:
					return new CharConstantValue(GetChar(), isProjectIndependent);
				case BasicType.Int:
					return new IntConstantValue(GetInt(), isProjectIndependent);
				case BasicType.Null:
					return new NullConstantValue(); // will not go here
				default:
					throw new InternalCompilerException();

			}

		}


		protected override void CheckEnumInt(EnumName enumName, ExprNode node) {
			if (node.ConstantValue != null) {
				int value = GetInt();
				foreach (StaticField field in enumName.MyClass.StaticFields.Values) {
					StaticVariableName staticVar = field.Name as StaticVariableName;
					if (staticVar != null) {
						NumericConstantValue cv = staticVar.GetConstantValue() as NumericConstantValue;
						if (cv != null && cv.GetInt() == value)
							return;
					}
				}

				node.Expression.Syntax.ErrorLog.LogError(node.Token, ErrorCode.EValueDoesntBelongToEnum, enumName);
			}
		}


		protected override void CheckEnumFlags(EnumName enumName, ExprNode node) {
			if (node.ConstantValue != null) {
				int value = GetInt();
				foreach (StaticField field in enumName.MyClass.StaticFields.Values) {
					if (value == 0)
						return;
					StaticVariableName staticVar = field.Name as StaticVariableName;
					if (staticVar != null) {
						NumericConstantValue cv = staticVar.GetConstantValue() as NumericConstantValue;
						if (cv != null)
							value &= ~cv.GetInt();
					}
				}

				if (value != 0)
					node.Expression.Syntax.ErrorLog.LogError(node.Token, ErrorCode.EValueDoesntBelongToEnum, enumName);
			}
		}
	}


	public class IntConstantValue : NumericConstantValue
	{
		int _value;
		public int Value {
			get { return _value; }
		}


		// CONSTRUCTOR
		public IntConstantValue(int value, bool isProjectIndependent) : base(ConstantType.Int, isProjectIndependent) { _value = value; }
		public IntConstantValue(int value) : base(ConstantType.Int, true) { _value = value; }

		public override char GetChar() {
			return (char)_value;
		}

		public override int GetInt() {
			return _value;
		}

		public override double GetDouble() {
			return _value;
		}

		public override bool GetBool() {
			return _value != 0;
		}

		protected override void ApplyUnaryOperator(LanguageOperator languageOperator) {
			switch (languageOperator.Type) {
				case OperatorType.Minus:
					_value = -_value;
					break;
				case OperatorType.Tilda:
					_value = ~_value;
					break;
			}
		}
	}


	public class CharConstantValue : NumericConstantValue
	{
		char _value;
		public char Value {
			get { return _value; }
		}


		// CONSTRUCTOR
		public CharConstantValue(char value, bool isProjectIndependent) : base(ConstantType.Char, isProjectIndependent) { _value = value; }
		public CharConstantValue(char value) : base(ConstantType.Char, true) { _value = value; }

		public override char GetChar() {
			return _value;
		}

		public override int GetInt() {
			return _value;
		}

		public override double GetDouble() {
			return _value;
		}

		public override bool GetBool() {
			return _value != 0;
		}


		protected override void ApplyUnaryOperator(LanguageOperator languageOperator) {
			switch (languageOperator.Type) {
				case OperatorType.Minus:
					_value = unchecked((char)-(int)_value);
					break;
				case OperatorType.Tilda:
					_value = unchecked((char)~(int)_value);
					break;
			}
		}

	}


	public class DoubleConstantValue : NumericConstantValue
	{
		double _value;
		public double Value {
			get { return _value; }
		}


		// CONSTRUCTOR
		public DoubleConstantValue(double value, bool isProjectIndependent) : base(ConstantType.Double, isProjectIndependent) { _value = value; }
		public DoubleConstantValue(double value) : base(ConstantType.Double, true) { _value = value; }

		public override char GetChar() {
			return (char)Math.Floor(_value+0.5);
		}

		public override int GetInt() {
			return (int)Math.Floor(_value + 0.5);
		}

		public override double GetDouble() {
			return _value;
		}

		public override bool GetBool() {
			return _value != 0;
		}

		protected override void ApplyUnaryOperator(LanguageOperator languageOperator) {
			switch (languageOperator.Type) {
				case OperatorType.Minus:
					_value = -_value;
					break;
				case OperatorType.Tilda:
					throw new InternalCompilerException();
			}
		}

	}


	public class NullConstantValue : NumericConstantValue
	{

		// CONSTRUCTOR
		public NullConstantValue() : base(ConstantType.Null, true) { }

		public override char GetChar() {
			return (char)0;
		}

		public override int GetInt() {
			return 0;
		}

		public override double GetDouble() {
			return 0;
		}

		public override bool GetBool() {
			return false;
		}

		public override ConstantValue ArrayAccess(ConstantValue index, ExprNode expr) {
			expr.Expression.Syntax.ErrorLog.LogError(expr.Token, ErrorCode.EAccessingNullArray);
			return null;
		}


		public override ConstantValue MakeCopy(LanguageType lt, bool isProjectIndependent) {
			return new NullConstantValue();
		}

		protected override void CheckEnumName(EnumName enumName, ExprNode node) {
			// ok
		}
	}


	public class NameConstantValue : ConstantValue 
	{
		KsidName _name;
		public KsidName Name {
			get { return _name; }
		}


		// CONSTRUCTOR
		public NameConstantValue(KsidName name) : base(ConstantType.Name, false) { _name = name; }

		public override bool GetBool() {
			return _name != null;
		}

		public override ConstantValue MakeCopy(LanguageType lt, bool isProjectIndependent) {
			if (lt.DimensionsCount > 0 || lt.BasicType != BasicType.Name)
				throw new InternalCompilerException();
			return new NameConstantValue(_name);
		}

		protected override void CheckEnumName(EnumName enumName, ExprNode node) {
			if (enumName.MyName != null && _name != null) {
				if (!enumName.MyName.IsDescendant(_name))
					node.Expression.Syntax.ErrorLog.LogError(node.Token, ErrorCode.EValueDoesntBelongToEnum, enumName);
			}
		}
	}


	public class ArrayConstantValue : ConstantValue
	{
		List<ConstantValue> _array;
		public IList<ConstantValue> Array {
			get { return _array; }
		}


		// CONSTRUCTOR
		public ArrayConstantValue(IEnumerable<ExprNode> members) : base(ConstantType.Array, true) {
			_array = new List<ConstantValue>();
			foreach (ExprNode member in members) {
				if (!member.ConstantValue.IsProjectIndependent)
					IsProjectIndependent = false;
				_array.Add(member.ConstantValue);
			}
		}

		public ArrayConstantValue(IEnumerable<ConstantValue> members, bool isProjectIndependent)
			: base(ConstantType.Array, isProjectIndependent) 
		{
			_array = new List<ConstantValue>(members);
		}

		public ArrayConstantValue(String str) : base(ConstantType.Array, true) {
			_array = new List<ConstantValue>(str.Length);
			foreach (char ch in str) {
				_array.Add(new CharConstantValue(ch, true));
			}
		}

		public ArrayConstantValue(ArrayConstantValue arr1, ArrayConstantValue arr2)	: base(ConstantType.Array, true) {			
			int capacity = 0;
			if (arr1 != null)
				capacity += arr1._array.Count;
			if (arr2 != null)
				capacity += arr2._array.Count;
			_array = new List<ConstantValue>(capacity);
			if (arr1 != null) {
				_array.AddRange(arr1._array);
				if (!arr1.IsProjectIndependent)
					IsProjectIndependent = false;
			}
			if (arr2 != null) {
				_array.AddRange(arr2._array);
				if (!arr2.IsProjectIndependent)
					IsProjectIndependent = false;
			}
		}

		public override bool GetBool() {
			return true;
		}


		public override ConstantValue ArrayAccess(ConstantValue index, ExprNode expr) {
			int i = ((NumericConstantValue)index).GetInt();
			if (i < 0 || i >= _array.Count) {
				expr.Expression.Syntax.ErrorLog.LogError(expr.Token, ErrorCode.EArrayIndexOutOfRange);
				return null;
			}
			return _array[i].MakeCopy(expr.LanguageType, IsProjectIndependent && index.IsProjectIndependent);
		}


		public override ConstantValue MakeCopy(LanguageType lt, bool isProjectIndependent) {
			if (lt.DimensionsCount == 0)
				throw new InternalCompilerException();
			return new ArrayConstantValue(_array, isProjectIndependent);
		}
	}
}
