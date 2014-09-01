//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - L a n g u a g e T y p e
///
///		Represents the language type
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Krkal.Compiler
{

	public enum BasicType
	{
		Unasigned,
		Void,
		Char,
		Int,
		Double,
		Name,
		Object,
		Null,
	}

	[Flags]
	public enum Modifier
	{		
		None =		0,
		Ret =		1,
		RetOr =		2,
		RetAnd =	4,
		RetAdd =	6,
		Static =	8,
		Public =	16,
		Override =	32,
		Direct =	64,
		Protected = 128,
		ConstV =	0x000100,
		ConstO =	0x000200,
		ConstM =	0x000400,
		//			0x000800,
		//			0x001000,
		Const3 =	0x002000,
		Const2 =	0x004000,
		Const1 =	0x008000,

		Safe =		0x010000, // duplicite flags not needed for kernel, so they are larger than 16 bits, they will be truncated
		Private =	0x020000,
		Extern =	0x040000,
	}

	public enum ModifierGroups
	{
		None = 0,
		CallingGroup = Modifier.Override | Modifier.Direct | Modifier.Safe,
		RetCalcullationGroup = Modifier.RetOr | Modifier.RetAnd | Modifier.RetAdd,
		RetGroup = Modifier.Ret | RetCalcullationGroup,
		PointerConstGroup = Modifier.ConstO | Modifier.Const1 | Modifier.Const2 | Modifier.Const3,
		VariableConstGroup = Modifier.ConstV | PointerConstGroup,
		ConstGroup = VariableConstGroup | Modifier.ConstM,
		AccessModifiers = Modifier.Private | Modifier.Protected | Modifier.Public,
		AllowedForSafeMethod = RetCalcullationGroup | Modifier.Static | CallingGroup | Modifier.Extern | Modifier.ConstM | PointerConstGroup,
		AllowedForDirectMethod = Modifier.Static | Modifier.Direct | Modifier.Extern | Modifier.ConstM | PointerConstGroup | AccessModifiers,
		AllowedForParameterDirect = Modifier.Ret | PointerConstGroup,
		AllowedForParameterSafe = RetGroup | PointerConstGroup,
		AllowedForVariable = Modifier.Static | VariableConstGroup | AccessModifiers,
		AllowedForLocalVariable = VariableConstGroup,
		RelevantForTypedName = RetCalcullationGroup | Modifier.Static | Modifier.Direct | ConstGroup | Modifier.Public | Modifier.Protected,
	}


	public struct LanguageType
	{
		public const int MaxConstArrayDim = 3;
		public const int MaxDimCount = 15;

		private BasicType _basicType;
		public BasicType BasicType {
			get { return _basicType; }
			set { _basicType = value; }
		}

		private Modifier _modifier;
		public Modifier Modifier {
			get { return _modifier; }
			set { _modifier = value; }
		}

		private byte _dimensionsCount;
		public byte DimensionsCount {
			get { return _dimensionsCount; }
			set { _dimensionsCount = value; }
		}

		private ClassOrEnumName _objectType;
		public ClassOrEnumName ObjectType {
			get { return _objectType; }
			set { _objectType = value; }
		}



		// CONSTRUCTOR

		public LanguageType(BasicType basicType, Modifier modifier, byte dimensionsCount, ClassOrEnumName objectType) {
			_basicType = basicType;
			_modifier = modifier;
			_dimensionsCount = dimensionsCount;
			_objectType = objectType;
		}

		public LanguageType(BasicType basicType)
			: this(basicType, Modifier.None, 0, null) 
		{ }

	
		public LanguageType(BasicType basicType, Modifier modifier)
			: this(basicType, modifier, 0, null) 
		{ }



		internal void ReadArray(SyntaxTemplates syntax) {
			while (syntax.Lexical.Peek() == OperatorType.LeftBracket) {
				if (_dimensionsCount+1 > MaxDimCount)
					syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EMaxArrayDimensionsExceeded);
				syntax.Lexical.Read();
				syntax.DoClosingBracket(OperatorType.RightBracket);
				_dimensionsCount++;
			}
			if (_basicType == BasicType.Void && _dimensionsCount > 0)
				syntax.ErrorLog.LogError(syntax.Lexical.CurrentToken, ErrorCode.EArrayCannotBeVoid);
		}


		// if function fails throw EClassOrEnumNameExpected
		internal bool AssignObjectType(KsidName objectType) {
			return AssignObjectType(objectType, false);
		}

		// if function fails throw EClassOrEnumNameExpected
		internal bool AssignObjectType(KsidName objectType, bool addRetOrToFlagsEnums) {
			_objectType = objectType as ClassOrEnumName;
			if (_objectType == null)
				return false;
			EnumName enumName = _objectType as EnumName;
			if (enumName != null) {
				if (enumName.Type == EnumSubType.Name) {
					_basicType = BasicType.Name;
				} else {
					_basicType = BasicType.Int;
					if (addRetOrToFlagsEnums && enumName.Type == EnumSubType.Flags && (_modifier & (Modifier)ModifierGroups.RetGroup) == 0)
						_modifier |= Modifier.RetOr;
				}
			} else {
				_basicType = BasicType.Object;
			}
			return true;
		}



		public bool IsPointerToConst {
			get { return _dimensionsCount == 0 ? ((_modifier & Modifier.ConstO) != 0) : GetArrayConst(_dimensionsCount - 1); }
			set {
				if (_dimensionsCount == 0) {
					if (value) {
						_modifier |= Modifier.ConstO;
					} else {
						_modifier &= ~Modifier.ConstO;
					}
				} else {
					SetArrayConst(_dimensionsCount - 1, value);
				}
			}
		}


		public bool IsAllConst {
			get {
				if (_basicType == BasicType.Object && (_modifier & Modifier.ConstO) == 0)
					return false;
				if ((_modifier & Modifier.ConstV) == 0)
					return false;
				for (int f = 0; f < _dimensionsCount; f++) {
					if (!GetArrayConst(f))
						return false;
				}
				return true;
			}
		}

		public bool GetArrayConst(int dim) { // dimensions are counted from 0 and from most inner array (int[][][][] a; a[3][2][1][0] = 5)
			if (dim < MaxConstArrayDim) {
				return (_modifier & (Modifier)((int)Modifier.Const1 >> dim)) != 0;
			} else {
				return false;
			}
		}

		public void SetArrayConst(int dim, bool isConst) { // dimensions are counted from 0 and from most inner array (int[][][][] a; a[3][2][1][0] = 5)
			if (dim < MaxConstArrayDim) {
				if (isConst) {
					_modifier |= (Modifier)((int)Modifier.Const1 >> dim);
				} else {
					_modifier &= ~(Modifier)((int)Modifier.Const1 >> dim);
				}
			}
		}


		internal void CheckConstIntegrity(ErrorLog errorLog, LexicalToken errorToken) {
			if ((_modifier & Modifier.ConstO) != 0) {
				if (_basicType != BasicType.Object)
					errorLog.LogError(errorToken, ErrorCode.EModifierNotAllowed, Modifier.ConstO);
			}
			for (int f = 0; f < MaxConstArrayDim; f++) {
				if (f + 1 > _dimensionsCount && GetArrayConst(f)) {
					errorLog.LogError(errorToken, ErrorCode.EModifierNotAllowed, (Modifier)((int)Modifier.Const1 >> f));
				}
			}
		}


		public void IncreaseDimCount(bool isNewVariableConstV) {
			_dimensionsCount++;
			if ((_modifier & Modifier.ConstV) != 0)
				SetArrayConst(_dimensionsCount - 1, true);
			if (isNewVariableConstV)
				_modifier |= Modifier.ConstV;
		}


		public void DecreaseDimCount() {
			_dimensionsCount--;
			if (GetArrayConst(_dimensionsCount)) {
				_modifier |= Modifier.ConstV;
				SetArrayConst(_dimensionsCount, false);
			}
		}


		public OperatorRequieredArguments IsType(OperatorRequieredArguments typesGroup, bool acceptNull) {
			OperatorRequieredArguments ret = OperatorRequieredArguments.None;
			if (_dimensionsCount > 0) {
				ret |= (typesGroup & (OperatorRequieredArguments.Array | OperatorRequieredArguments.Pointer));
			} else  {
				switch (_basicType) {
					case BasicType.Int:
					case BasicType.Char:
						ret |= (typesGroup & (OperatorRequieredArguments.IntChar | OperatorRequieredArguments.IntCharDouble));
						break;
					case BasicType.Double:
						ret |= (typesGroup & (OperatorRequieredArguments.IntCharDouble));
						break;
					case BasicType.Name:
						ret |= (typesGroup & (OperatorRequieredArguments.Name | OperatorRequieredArguments.NameObject | OperatorRequieredArguments.Pointer));
						break;
					case BasicType.Object:
						ret |= (typesGroup & (OperatorRequieredArguments.Object | OperatorRequieredArguments.NameObject | OperatorRequieredArguments.Pointer));
						break;
				}
				if (acceptNull && _basicType == BasicType.Null) {
					ret |= typesGroup;
				}
			}
			return ret;
		}


		public bool IsUnasigned {
			get { return (_basicType == BasicType.Unasigned); }
		}

		public bool IsNull {
			get { return (_basicType == BasicType.Null && _dimensionsCount == 0); }
		}

		public bool IsVoid {
			get { return (_basicType == BasicType.Void); }
		}


		public static LanguageType Biggest(LanguageType a, LanguageType b) {
			if (a._dimensionsCount == 0 && b._dimensionsCount == 0) {
				if (b._basicType == BasicType.Double)
					return b;
				if (b._basicType == BasicType.Int && a._basicType != BasicType.Double)
					return b;
			}
			return a;
		}

		public LanguageType ClearModifier() {
			return new LanguageType(_basicType, Modifier.None, _dimensionsCount, _objectType);
		}

		public LanguageType ClearModifier(ModifierGroups keepGroups) {
			return new LanguageType(_basicType, _modifier & (Modifier)keepGroups, _dimensionsCount, _objectType);
		}


		#region Operators

		public override bool Equals(object obj) {
			if (obj is LanguageType) {
				LanguageType lt = (LanguageType)obj;
				return (_basicType == lt._basicType && _modifier == lt._modifier && _dimensionsCount == lt._dimensionsCount && _objectType == lt._objectType);
			} else {
				return false;
			}
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}


		public static bool operator == (LanguageType lt1, LanguageType lt2) {
			return (lt1._basicType == lt2._basicType && lt1._modifier == lt2._modifier && lt1._dimensionsCount == lt2._dimensionsCount && lt1._objectType == lt2._objectType);
		}

		public static bool operator != (LanguageType lt1, LanguageType lt2) {
			return !(lt1 == lt2);
		}

		#endregion


		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			if (_modifier != Modifier.None) {
				sb.Append(_modifier.ToString().ToLower(CultureInfo.InvariantCulture));
				sb.Append(' ');
			}
			if (_objectType != null) {
				sb.Append(_objectType.Identifier.ToString(true));
			} else {
				sb.Append(_basicType.ToString().ToLower(CultureInfo.InvariantCulture));
			}

			for (int f = 0; f < _dimensionsCount; f++) {
				sb.Append("[]");
			}

			return sb.ToString();
		}


	}
}
