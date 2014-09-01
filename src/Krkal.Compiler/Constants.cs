//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C o n s t a n t s
///
///		Compiler constants.
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	public enum KeywordType
	{
		Name,
		Class,
		Depend,
		Char,
		Int,
		Double,
		Object,
		Control,
		Group,
		Void,
		String,
		Enum,

		Null,
		True,
		False,
		Everything,
		Nothing,
		This,
		Sender,

		Ret,
		RetOr,
		RetAnd,
		RetAdd,
		Static,
		Safe,
		Override,
		Direct,
		Extern,
		ConstV,
		ConstO,
		ConstM,
		Const1,
		Const2,
		Const3,
		Private,
		Protected,
		Public,

		Message,
		End,
		NextTurn,
		NextEnd,
		Timed,
		CallEnd,

		Assigned,
		New,

		If,
		Else,
		While,
		Do,
		For,
		ForEach,
		In,
		Switch,
		Case,
		Default,
		Break,
		Continue,
		Return,
	}


	public enum HeaderKeywordType {
		Head,
		Attributes,
		Names,
	}


	public enum HeaderFieldType
	{
		Version,
		Include,
		System,
		Component,
		AttributesDefinition,
		Engine,
		GeneratesData,
	}



	public enum OperatorType
	{
		PlusPlus,		//	++
		MinusMinus,		//	--
		Access,			//	->
		Plus,			//	+
		Minus,			//	-
		Exclamation,	//	!
		Tilda,			//	~
		Multiply,		//	*
		Division,		//	/
		Modulo,			//	%
		LeftShift,		//	<<
		RightShift,		//	>>
		Less,			//	<
		Greater,		//	>
		LessEqual,		//	<=
		GreaterEqual,	//	>=
		Equal,			//	==
		NotEqual,		//	!=
		And,			//	&
		Or,				//	|
		Xor,			//	^
		DoubleAnd,		//	&&
		DoubleOr,		//	||
		Assign,			//	=
		MultiplyAssign,	//	*=
		DivisionAssign,	//	/=
		ModuloAssign,	//	%=
		PlusAssign,		//	+=
		MinusAssign,	//	-=
		LeftShiftAssign,//	<<=
		RightShiftAssign,//	>>=
		AndAssign,		//	&=
		OrAssign,		//	|=
		XorAssign,		//	^=

		Comma,			//	,
		QuestionMark,	//	?
		Colon,			//	:
		SemiColon,		//	;
		LeftParenthesis,//	(
		RightParenthesis,//	)
		LeftBracket,	//	[
		RightBracket,	//	]
		LeftCurlyBracket,//	{
		RightCurlyBracket,//}
	}



	public class Keyword
	{
		private KeywordType _type;
		public KeywordType Type {
			get { return _type; }
		}

		private String _text;
		public String Text {
			get { return _text; }
		}

		private Modifier _modifier;
		public Modifier Modifier {
			get { return _modifier; }
		}

		private BasicType _basicType;
		public BasicType BasicType {
			get { return _basicType; }
		}

		private CommandType _commandType;
		public CommandType CommandType {
			get { return _commandType; }
		}

		private ConstantKeyword _constantKeyword;
		public ConstantKeyword ConstantKeyword {
			get { return _constantKeyword; }
		}

		private MessageType _messageType;
		public MessageType MessageType {
			get { return _messageType; }
		}

		// CONSTRUCTOR

		public Keyword(KeywordType type) {
			_type = type;
			_text = type.ToString().ToLowerInvariant();

			_modifier = CompilerConstants.GetModifier(type.ToString());
			_basicType = CompilerConstants.GetBasicType(type.ToString());
			if (_basicType == BasicType.Null)
				_basicType = BasicType.Unasigned;
			_commandType = CompilerConstants.GetCommandType(type.ToString());
			_constantKeyword = CompilerConstants.GetConstantKeyword(type.ToString());
			_messageType = CompilerConstants.GetMessageType(type.ToString());
		
		}


		public override string ToString() {
			return _text;
		}
	}




	public class OperatorDescription
	{
		private int _type;
		internal int Type {
			get { return _type; }
		}

		private OperatorRequieredArguments _requieredArguments;
		public OperatorRequieredArguments RequieredArguments {
			get { return _requieredArguments; }
		}

		private OperatorResult _result;
		public OperatorResult Result {
			get { return _result; }
		}

		private OperatorArgumentCasts _argumentCasts;
		public OperatorArgumentCasts ArgumentCasts {
			get { return _argumentCasts; }
		}

		private OperatorConstBehavior _constBehavior;
		public OperatorConstBehavior ConstBehavior {
			get { return _constBehavior; }
		}

		// CONSTRUCTOR
		internal OperatorDescription(int type, int[] description) {
			_type = type;
			if (description != null) {
				_requieredArguments = (OperatorRequieredArguments)(description[0]);
				_result = (OperatorResult)(description[1]);
				_argumentCasts = (OperatorArgumentCasts)(description[2]);
				_constBehavior = (OperatorConstBehavior)(description[3]);
			}
		}

	}



	public class LanguageOperator
	{
		private OperatorType _type;
		public OperatorType Type {
			get { return _type; }
		}

		private String _text;
		public String Text {
			get { return _text; }
		}

		private int _priority;
		public int Priority {
			get { return _priority; }
		}

		internal UnaryOperatorType UnaryOpType {
			get { return (UnaryOperatorType)_unaryOperatorDescription.Type; }
		}

		public BinaryOperatorType BinaryOperatorType {
			get { return (BinaryOperatorType)_binaryOperatorDescription.Type; }
		}

		private OperatorDescription _unaryOperatorDescription;
		public OperatorDescription UnaryOperatorDescription {
			get { return _unaryOperatorDescription; }
		}

		private OperatorDescription _binaryOperatorDescription;
		public OperatorDescription BinaryOperatotDescription {
			get { return _binaryOperatorDescription; }
		}

		public LanguageOperator(OperatorType type, String text, int priority, OperatorDescription unaryOpDescription, OperatorDescription binaryOperatorDescription) {
			_type = type;
			_text = text;
			_priority = priority;
			_unaryOperatorDescription = unaryOpDescription;
			_binaryOperatorDescription = binaryOperatorDescription;
		}


		public override string ToString() {
			return _text;
		}
	}



	public static class CompilerConstants
	{
		private const int NameTypesCount = 11;

		private static Dictionary<String, Keyword> _keywords;
		private static Keyword[] _keywords2;
		private static Dictionary<String, HeaderKeywordType> _headerKeywords;
		private static Dictionary<String, HeaderFieldType> _headerFields;
		private static Dictionary<String, Modifier> _modifiers;
		private static Dictionary<String, BasicType> _basicTypes;
		private static Dictionary<String, CommandType> _commandTypes;
		private static Dictionary<String, ConstantKeyword> _constantKeywords;
		private static Dictionary<String, MessageType> _messageTypes;
		private static Dictionary<String, LanguageOperator> _operators;
		private static LanguageOperator[] _operators2;
		private static bool[] _operatorCharactersMap = new bool[128];
		private static bool[] _hexadecimalDigitsMap = new bool[128];
		private static bool[] _nextIdCharactersMap = new bool[128];
		private static bool[] _nameTypeMethod = new bool[NameTypesCount];
		private static bool[] _nameTypeVariable = new bool[NameTypesCount];
		private static bool[] _nameTypeSafeMethod = new bool[NameTypesCount];
		private static bool[] _nameTypeDirectMethod = new bool[NameTypesCount];
		private static bool[] _nameTypeStatic = new bool[NameTypesCount];
		private static bool[] _nameTypeField = new bool[NameTypesCount];

		#region static String[] _operatorTexts
		private static String[] _operatorTexts = {
			"++", /* PlusPlus, */
			"--", /* MinusMinus, */
			"->", /* Access, */
			"+", /* Plus, */
			"-", /* Minus, */
			"!", /* Exclamation, */
			"~", /* Tilda, */
			"*", /* Multiply, */
			"/", /* Division, */
			"%", /* Modulo, */
			"<<", /* LeftShift, */
			">>", /* RightShift, */
			"<", /* Less, */
			">", /* Greater, */
			"<=", /* LessEqual, */
			">=", /* GreaterEqual, */
			"==", /* Equal, */
			"!=", /* NotEqual, */
			"&", /* And, */
			"|", /* Or, */
			"^", /* Xor, */
			"&&", /* DoubleAnd, */
			"||", /* DoubleOr, */
			"=", /* Assign, */
			"*=", /* MultiplyAssign, */
			"/=", /* DivisionAssign, */
			"%=", /* ModuloAssign, */
			"+=", /* PlusAssign, */
			"-=", /* MinusAssign, */
			"<<=", /* LeftShiftAssign, */
			">>=", /* RightShiftAssign, */
			"&=", /* AndAssign, */
			"|=", /* OrAssign, */
			"^=", /* XorAssign, */

			",", /* Comma, */
			"?", /* QuestionMark, */
			":", /* Colon, */
			";", /* SemiColon, */
			"(", /* LeftParenthesis, */
			")", /* RightParenthesis, */
			"[", /* LeftBracket, */
			"]", /* RightBracket, */
			"{", /* LeftCurlyBracket, */
			"}", /*	RightCurlyBracket, */
		};
		#endregion static String[] _operatorTexts

		#region static int[,] _operatorPriorities
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
		private static int[,] _operatorPriorities = {
			/* PlusPlus, */ {
				0, 
				(int)UnaryOperatorType.PlusPlus, 
				0,
			}, 
			/* MinusMinus, */ {
				0, 
				(int)UnaryOperatorType.PlusPlus,
				0,
			}, 
			/* Access, */ {
				0,
				0,
				0,
			}, 
			/* Plus, */ {
				13, 
				(int)UnaryOperatorType.Plus,
				(int)BinaryOperatorType.Plus,
			}, 
			/* Minus, */ {
				13, 
				(int)UnaryOperatorType.Plus,
				(int)BinaryOperatorType.Minus,
			}, 
			/* Exclamation, */ {
				0, 
				(int)UnaryOperatorType.Exclamation,
				0,
			}, 
			/* Tilda, */ {
				0, 
				(int)UnaryOperatorType.Tilda,
				0,
			}, 
			/* Multiply, */ {
				14, 
				0,
				(int)BinaryOperatorType.Multiply,
			}, 
			/* Division, */ {
				14, 
				0,
				(int)BinaryOperatorType.Division,
			}, 
			/* Modulo, */ {
				14, 
				0,
				(int)BinaryOperatorType.Modulo,
			}, 
			/* LeftShift, */ {
				12, 
				0,
				(int)BinaryOperatorType.Shift,
			}, 
			/* RightShift, */ {
				12, 
				0,
				(int)BinaryOperatorType.Shift,
			}, 
			/* Less, */ {
				11, 
				0,
				(int)BinaryOperatorType.Compare,
			}, 
			/* Greater, */ {
				11, 
				0,
				(int)BinaryOperatorType.Compare,
			}, 
			/* LessEqual, */ {
				11, 
				0,
				(int)BinaryOperatorType.Compare,
			}, 
			/* GreaterEqual, */ {
				11, 
				0,
				(int)BinaryOperatorType.Compare,
			}, 
			/* Equal, */ {
				10, 
				0,
				(int)BinaryOperatorType.Equality,
			}, 
			/* NotEqual, */ {
				10, 
				0,
				(int)BinaryOperatorType.Equality,
			}, 
			/* And, */ {
				9, 
				(int)UnaryOperatorType.And,
				(int)BinaryOperatorType.BinaryOperator,
			}, 
			/* Or, */ {
				7, 
				0,
				(int)BinaryOperatorType.BinaryOperator,
			}, 
			/* Xor, */ {
				8, 
				0,
				(int)BinaryOperatorType.BinaryOperator,
			}, 
			/* DoubleAnd, */ {
				6, 
				0,
				(int)BinaryOperatorType.LogicalOperator,
			}, 
			/* DoubleOr, */ {
				5, 
				0,
				(int)BinaryOperatorType.LogicalOperator,
			}, 
			/* Assign, */ {
				3, 
				0,
				(int)BinaryOperatorType.Assign,
			}, 
			/* MultiplyAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.MultiplyAssign,
			}, 
			/* DivisionAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.DivisionAssign,
			}, 
			/* ModuloAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.ModuloAssign,
			}, 
			/* PlusAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.PlusAssign,
			}, 
			/* MinusAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.MinusAssign,
			}, 
			/* LeftShiftAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.ShiftAssign,
			}, 
			/* RightShiftAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.ShiftAssign,
			}, 
			/* AndAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.BinaryOperatorAssign,
			}, 
			/* OrAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.BinaryOperatorAssign,
			}, 
			/* XorAssign, */ {
				3, 
				0,
				(int)BinaryOperatorType.BinaryOperatorAssign,
			}, 

			/* Comma, */ {
				0, 
				0,
				0,
			}, 
			/* QuestionMark, */ {
				0, 
				0,
				0,
			}, 
			/* Colon, */ {
				0, 
				0,
				0,
			}, 
			/* SemiColon, */ {
				0, 
				0,
				0,
			}, 
			/* LeftParenthesis, */ {
				0, 
				(int)UnaryOperatorType.Parenthesis,
				0,
			}, 
			/* RightParenthesis, */ {
				0, 
				0,
				0,
			}, 
			/* LeftBracket, */ {
				0, 
				0,
				0,
			}, 
			/* RightBracket, */ {
				0, 
				0,
				0,
			}, 
			/* LeftCurlyBracket, */ {
				0, 
				(int)UnaryOperatorType.NewArray,
				0,
			}, 
			/* RightCurlyBracket, */ {
				0, 
				0,
				0,
			}, 
		};
		#endregion

		#region Operator Descriptions

		private static int[][] _unaryOperatorDescriptions = {
			/* None = 0, */
			null,
			/* PlusPlus = 1, */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.FirstArgument,
				(int)OperatorArgumentCasts.None,
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* Plus = 2, */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.FirstArgument,
				(int)OperatorArgumentCasts.None,
				(int)OperatorConstBehavior.None,
			},
			/* Exclamation = 3, */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Pointer),
				(int)OperatorResult.Int,
				(int)OperatorArgumentCasts.None,
				(int)OperatorConstBehavior.None,
			},
			/* Tilda = 4, */ new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.FirstArgument,
				(int)OperatorArgumentCasts.None,
				(int)OperatorConstBehavior.None,
			},
			/* And = 5, */
			null,
			/* Parenthesis = 6, */
			null,
			/* NewArray = 7, */
			null,
		};

		private static int[][] _binaryOperatorDescriptions = {
			/* None, */
			null,
			/* Plus,		+ */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest | OperatorArgumentCasts.ArrayOperation),
				(int)OperatorConstBehavior.ResultKeep2ndLevel,
			},
			/* Minus,		- */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest),
				(int)OperatorConstBehavior.None,
			},
			/* Multiply,	* */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest),
				(int)OperatorConstBehavior.None,
			},
			/* Division,	/ */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest),
				(int)OperatorConstBehavior.None,
			},
			/* Modulo,		% */ new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest),
				(int)OperatorConstBehavior.None,
			},
			/* Shift,		<< >> */ new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.None),
				(int)OperatorConstBehavior.None,
			},
			/* Compare,		< > <= >= */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array | OperatorRequieredArguments.NameObject),
				(int)OperatorResult.Int,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest | OperatorArgumentCasts.ArrayOperation | OperatorArgumentCasts.CastObjectsToName),
				(int)OperatorConstBehavior.None,
			},
			/* Equality,	== != */ new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array | OperatorRequieredArguments.Name | OperatorRequieredArguments.Object),
				(int)OperatorResult.Int,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest | OperatorArgumentCasts.ArrayOperation | OperatorArgumentCasts.MoveObjectsPointersToStartPosition),
				(int)OperatorConstBehavior.None,
			},
			/* BonaryOp,	& | ^ */  new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.BiggestArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToBiggest),
				(int)OperatorConstBehavior.None,
			},
			/* LogicalOp,	&& || */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Pointer),
				(int)OperatorResult.Int,
				(int)(OperatorArgumentCasts.None),
				(int)OperatorConstBehavior.None,
			},
			/* Assign,		= */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array | OperatorRequieredArguments.Name | OperatorRequieredArguments.Object),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult | OperatorArgumentCasts.CastObjectToResult),
				(int)(OperatorConstBehavior.LValueRequiered | OperatorConstBehavior.ResultIsFirst),
			},
			/* MultiplyAssign,	*= */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult),
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* DivisionAssign,	/= */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult),
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* ModuloAssign,	%= */  new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult),
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* PlusAssign,		+= */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble | OperatorRequieredArguments.Array ),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult | OperatorArgumentCasts.ArrayOperation),
				(int)(OperatorConstBehavior.LValueRequiered | OperatorConstBehavior.AppendCheck | OperatorConstBehavior.ResultIsFirst),
			},
			/* MinusAssign,		-= */  new int[] {
				(int)(OperatorRequieredArguments.IntCharDouble),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult),
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* ShiftAssign,		<<= >>= */  new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.None),
				(int)OperatorConstBehavior.LValueRequiered,
			},
			/* BinaryOpAssign,	&= |= ^= */  new int[] {
				(int)(OperatorRequieredArguments.IntChar),
				(int)OperatorResult.FirstArgument,
				(int)(OperatorArgumentCasts.CastArithmeticsToResult),
				(int)OperatorConstBehavior.LValueRequiered,
			},
		};

		#endregion

		public const String ForbiddenIdText = "__M_";


		private static Dictionary<String, T> GetEnumDictionary<T>()  {
			Array members = Enum.GetValues(typeof(T));
			Dictionary<string, T> ret = new Dictionary<string, T>(members.Length);
			foreach (T member in members) {
				ret.Add(member.ToString(), member);
			}
			return ret;
		}

		internal static void InitializeCompilerConstants() {

			_modifiers = GetEnumDictionary<Modifier>();
			_basicTypes = GetEnumDictionary<BasicType>();
			_commandTypes = GetEnumDictionary<CommandType>();
			_messageTypes = GetEnumDictionary<MessageType>();
			_constantKeywords = GetEnumDictionary<ConstantKeyword>();

			Array keywords = Enum.GetValues(typeof(KeywordType));
			_keywords = new Dictionary<string, Keyword>(keywords.Length);
			_keywords2 = new Keyword[keywords.Length];
			for (int f=0; f<keywords.Length; f++) {
				Keyword kw = new Keyword((KeywordType)keywords.GetValue(f));
				_keywords.Add(kw.Text, kw);
				_keywords2[f] = kw;
			}

			keywords = Enum.GetValues(typeof(HeaderKeywordType));
			_headerKeywords = new Dictionary<string, HeaderKeywordType>(keywords.Length);
			foreach (HeaderKeywordType keyword in keywords) {
				_headerKeywords.Add('#' + keyword.ToString().ToLowerInvariant(), keyword);
			}


			keywords = Enum.GetValues(typeof(HeaderFieldType));
			_headerFields = new Dictionary<string, HeaderFieldType>(keywords.Length);
			foreach (HeaderFieldType keyword in keywords) {
				_headerFields.Add(keyword.ToString().ToLowerInvariant(), keyword);
			}


			_operators = new Dictionary<string,LanguageOperator>(_operatorTexts.Length);
			_operators2 = new LanguageOperator[_operatorTexts.Length];
			for (int f = 0; f < _operatorTexts.Length; f++) {
				OperatorDescription unary = new OperatorDescription(_operatorPriorities[f, 1], _unaryOperatorDescriptions[_operatorPriorities[f, 1]]);
				OperatorDescription binary = new OperatorDescription(_operatorPriorities[f, 2], _binaryOperatorDescriptions[_operatorPriorities[f, 2]]);
				LanguageOperator op = new LanguageOperator((OperatorType)f, _operatorTexts[f], _operatorPriorities[f, 0], unary, binary);
				_operators.Add(_operatorTexts[f], op);
				_operators2[f] = op;
			}

			foreach (String operatorText in _operatorTexts) {
				foreach (char ch in operatorText) {
					_operatorCharactersMap[ch] = true;
				}
			}

			for (int f = '0'; f <= '9'; f++) {
				_nextIdCharactersMap[f] = true;
			}
			for (int f = 'A'; f <= 'Z'; f++) {
				_nextIdCharactersMap[f] = true;
			}
			for (int f = 'a'; f <= 'z'; f++) {
				_nextIdCharactersMap[f] = true;
			}
			_nextIdCharactersMap['_'] = true;


			for (int f = '0'; f <= '9'; f++) {
				_hexadecimalDigitsMap[f] = true;
			}
			for (int f = 'A'; f <= 'F'; f++) {
				_hexadecimalDigitsMap[f] = true;
			}
			for (int f = 'a'; f <= 'f'; f++) {
				_hexadecimalDigitsMap[f] = true;
			}


			_nameTypeDirectMethod[(int)NameType.DirectMethod] = true;
			_nameTypeDirectMethod[(int)NameType.StaticDirectMethod] = true;

			_nameTypeField[(int)NameType.Control] = true;
			_nameTypeField[(int)NameType.DirectMethod] = true;
			_nameTypeField[(int)NameType.Group] = true;
			_nameTypeField[(int)NameType.SafeMethod] = true;
			_nameTypeField[(int)NameType.StaticDirectMethod] = true;
			_nameTypeField[(int)NameType.StaticSafeMethod] = true;
			_nameTypeField[(int)NameType.StaticVariable] = true;
			_nameTypeField[(int)NameType.Variable] = true;

			_nameTypeMethod[(int)NameType.DirectMethod] = true;
			_nameTypeMethod[(int)NameType.SafeMethod] = true;
			_nameTypeMethod[(int)NameType.StaticDirectMethod] = true;
			_nameTypeMethod[(int)NameType.StaticSafeMethod] = true;

			_nameTypeSafeMethod[(int)NameType.StaticSafeMethod] = true;
			_nameTypeSafeMethod[(int)NameType.SafeMethod] = true;

			_nameTypeStatic[(int)NameType.StaticDirectMethod] = true;
			_nameTypeStatic[(int)NameType.StaticSafeMethod] = true;
			_nameTypeStatic[(int)NameType.StaticVariable] = true;

			_nameTypeVariable[(int)NameType.StaticVariable] = true;
			_nameTypeVariable[(int)NameType.Variable] = true;
		}




		public static bool TestKeyword(String text, out Keyword keyword) {
			return _keywords.TryGetValue(text, out keyword);
		}

		public static bool TestOperator(String text, out LanguageOperator languageOperator) {
			return _operators.TryGetValue(text, out languageOperator);
		}

		public static bool TestHeaderKeyword(String text, out HeaderKeywordType headerKeywordType) {
			return _headerKeywords.TryGetValue(text, out headerKeywordType);
		}

		public static bool TestHeaderField(String text, out HeaderFieldType headerFieldType) {
			return _headerFields.TryGetValue(text, out headerFieldType);
		}

		public static bool IsOperatorCharacter(char ch) {
			uint index = ch;
			if (index > 127)
				return false;
			return _operatorCharactersMap[index];
		}

		public static bool IsDigit(char ch) {
			return (ch >= '0' && ch <= '9');
		}

		public static bool IsHexadecimalDigit(char ch) {
			uint index = ch;
			if (index > 127)
				return false;
			return _hexadecimalDigitsMap[index];
		}


		public static bool IsFirstIdCharacter(char ch) {
			return ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch == '_'));
		}

		public static bool IsNextIdCharacter(char ch) {
			uint index = ch;
			if (index > 127)
				return false;
			return _nextIdCharactersMap[index];
		}

		public static bool IsWhiteSpace(char ch) {
			return Char.IsWhiteSpace(ch);
		}


		public static OperatorType GetLeftBracker(OperatorType rightBracket) {
			switch (rightBracket) {
				case OperatorType.RightBracket :
					return OperatorType.LeftBracket;
				case OperatorType.RightCurlyBracket:
					return OperatorType.LeftCurlyBracket;
				case OperatorType.RightParenthesis:
					return OperatorType.LeftParenthesis;
				default:
					throw new ArgumentException("this is not a right bracket", "rightBracket");
			}
		}


		public static Keyword GetKeyword(KeywordType type) {
			return _keywords2[(int)type];
		}

		public static LanguageOperator GetLanguageOperator(OperatorType type) {
			return _operators2[(int)type];
		}


		public static int HeaderSectionCount {
			get { return _headerKeywords.Count; }
		}



		public static bool IsNameTypeMethod(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeMethod[index];
		}
		public static bool IsNameTypeVariable(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeVariable[index];
		}
		public static bool IsNameTypeDirectMethod(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeDirectMethod[index];
		}
		public static bool IsNameTypeSafeMethod(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeSafeMethod[index];
		}
		public static bool IsNameTypeStatic(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeStatic[index];
		}
		public static bool IsNameTypeField(NameType nameType) {
			int index = (int)nameType;
			if (index >= NameTypesCount)
				return false;
			return _nameTypeField[index];
		}




		public static Modifier GetModifier(String text) {
			Modifier ret;
			if (!_modifiers.TryGetValue(text, out ret))
				ret = Modifier.None;
			return ret;
		}
		public static BasicType GetBasicType(String text) {
			BasicType ret;
			if (!_basicTypes.TryGetValue(text, out ret))
				ret = BasicType.Unasigned;
			return ret;
		}
		public static CommandType GetCommandType(String text) {
			CommandType ret;
			if (!_commandTypes.TryGetValue(text, out ret))
				ret = CommandType.None;
			return ret;
		}
		public static MessageType GetMessageType(String text) {
			MessageType ret;
			if (!_messageTypes.TryGetValue(text, out ret))
				ret = MessageType.None;
			return ret;
		}
		public static ConstantKeyword GetConstantKeyword(String text) {
			ConstantKeyword ret;
			if (!_constantKeywords.TryGetValue(text, out ret))
				ret = ConstantKeyword.None;
			return ret;
		}



		public static bool EndsWithVersion(String text) {
			if (text == null || text.Length < 20)
				return false;

			for (int f = 0; f < 20; f++) {
				char ch = text[text.Length - 20 + f];
				if (f % 5 == 0) {
					if (ch != '_') 
						return false;
				} else {
					if (!IsHexadecimalDigit(ch))
						return false;
				}
			}

			return true;
		}

	}
}
