//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - E r r o r H a n d l i n g
///
///		Error Handling
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Krkal.Compiler
{

	public enum ErrorType
	{
		None,				// no error occured
		Warning,		
		Error,
		FatalError,			// error that makes further compilation impossible
		InternalError,		// enexpected internal error (caused by bug in Krkal.Compiler, not by bug in compiled code)
	}

	public enum ErrorCode
	{
		INone						= 0,
		IUnspecifiedError			= 1,
		EEndOfFileReached			= 2,
		FCoreVersionMisMatch		= 3,
		FConflictInModifications	= 4,
		FIncludesAreInCycle			= 5,
		EClosingBracketMissing		= 6,
		EOpeningBracketMissing		= 7,
		ECommentNotTerminated		= 8,
		EInvalidLexicalToken		= 9,
		EUnknownHeaderKeyword		= 10,
		EInvalidNumberFormat		= 11,
		ENumericLiteralOverflow		= 12,
		EKeywordInsideIdentifier	= 13,
		EInvalidCharacterInId		= 14,
		EForbiddenIdText			= 15,
		EInvalidVersionLiteral		= 16,
		EStringNotTermineted		= 17,
		ENewLineNotAllowed			= 18,
		EInvalidEscapeSequence		= 19,
		EInvalidCharacterConstant	= 20,
		EExpectedTokenNotFound		= 21,
		FCoreVersionNotSpecified	= 22,
		EMissingBody				= 23,
		EUnexpectedToken			= 24,
		ESemiColonExpected			= 25,
		EMissingStatement			= 26,
		EMissingDelimiter			= 27,
		EWrongHeadName				= 28,
		EVersionExpected			= 29,
		EIncludeError				= 30,
		FFileNotFound				= 31,
		EDuplicatedHeaderSection	= 32,
		EInvalidDeclaration			= 33,
		ENameDeclarationIsntTop		= 34,
		EClassDeclarationIsntTop	= 35,
		EDeclarationNotInClass		= 36,
		ENotParamDeclaration		= 37,
		EModifierNotAllowed			= 38,
		EIdentifierExpected			= 39,
		EComposedParamId			= 40,
		ESafeDirectOverrideTogether = 41,
		EInvlaidModifier			= 42,
		EModifierAlreadyUsed		= 43,
		EShiftOperatorExpected		= 44,
		EDifferentNameType			= 45,
		ENameNotDeclared			= 46,
		ENameVersionsConflict		= 47,
		WNameVersionsPossibleConflict = 48,
		ERedeclaringType			= 49,
		ECycleInNames				= 50,
		EMultipleDeclaration		= 51,
		EAmbiguousInheritance		= 52,
		FFileWithoutVersion			= 53,
		EVoidVariable				= 54,
		EComposedLocalId			= 55,
		ELocalVariableDeclared		= 56,
		EObjectTypeRequiered		= 57,
		EFieldIsNotAMemberOf		= 58,
		EArrayRequiered				= 59,
		ENumericTypeRequiered		= 60,
		ETypeMismatchUnary			= 61,
		ELValueRequiered			= 62,
		EUnassignedTypeInValue		= 63,
		EUnassignedType				= 64,
		ETypeMismatchBinary			= 65,
		EWrongNumberOfArguments		= 66,
		EUnableToConvertTypeTo		= 67,
		ETypeVoidNotAllowed			= 68,
		EStaticFieldExpected		= 69,
		ENameTypeRequiered			= 70,
		EExpectedTypeInDeclaration	= 71,
		EInitializationNotAllowed	= 72,
		ENoEnclosingLoop			= 73,
		EArrayDoesntContainType		= 74,
		EOneDefinitionAllowed		= 75,
		FUnableToCreateOutputFile	= 76,
		FCodeGenerationFailed		= 77,
		EStaticAccessingMember		= 78,
		WThisInStaticIsNull			= 79,
		EUnableToMakeReference		= 80,
		EAccessMembersNotAllowed	= 81,
		EAccessStaticNotAllowed		= 82,
		EConstantRequiered			= 83,
		EMethodNameExpected			= 84,
		EArrayCannotBeVoid			= 85,
		ERetFceOnWrongType			= 86,
		EDMNotImplemented			= 87,
		EAssignedCannotBeUsed		= 88,
		EParameterNameRequiered		= 89,
		EEnumDeclarationIsntTop		= 90,
		EDOdeclarationIsntTop		= 91,
		ENameTypeExpected			= 92,
		EClassNameExpected			= 93,
		EClassOrEnumNameExpected	= 94,
		EEnumRedefinition			= 95,
		EIncompatibleDataObjectClass= 96,
		ECannotWriteToConstant		= 97,
		ECannotChangeConstantObject = 98,
		ECannotCastOffConst			= 99,
		ECannotCallNonConstMethod	= 100,
		EMaxArrayDimensionsExceeded = 101,
		EOnlyOneAccessModCanBe		= 102,
		EArrayIndexOutOfRange		= 103,
		EAccessingNullArray			= 104,
		EConstModsOnAllLevelsNeeded = 105,
		EUnableToRetrieveConstant	= 106,
		EValueDoesntBelongToEnum	= 107,
		EDivisionByZero				= 108,
		FUnableToLoadAttributeDefs	= 109,
		EUnknownAttributeName		= 110,
		EInvalidAttributeUse		= 111,
		EAttributeValueConflict		= 112,
		EWrongAttributeName			= 113,
		EWronKsidIdentifierFormat	= 114,
		EUnknownCustomSubType		= 115,

	}


	static internal class ErrorTexts
	{
		internal readonly static String[] Texts = {
			"No error.", // 0
			"Unspecified Error occured.", // 1
			"End of file reached." , // 2
			"Core versions doesn't match: In include statement used {0}, but file contains {1}.", // 3
			"Conflict in Modifications. (\"{1}\" x \"{2}\") Include the correct file higher in the include hierarchy.", // 4
			"Includes are in cycle.", // 5
			"Closing bracket ')', '}' or ']' is missing.", // 6
			"Opening bracket '(', '{' or '[' is missing.", // 7
			"Delimited comment not terminated by */.", // 8
			"Invalid lexical token.", // 9
			"Unknown header keyword.", // 10
			"Number has invalid format.", // 11
			"Number is too large or too small.", // 12
			"Keyword \"{0}\" cannot be a part of an identifier.", // 13
			"Invalid character in identifier.", // 14
			"Text " + CompilerConstants.ForbiddenIdText + " cannot be a part of an identifier.", // 15
			"Invalid version literal.", // 16
			"String should be terminated by \".", // 17
			"New line not allowed inside a string or character constant.", //18
			"Invalid escape sequence.", // 19
			"Invalid character constant.", // 20
			"Expected token \"{0}\" not found.", // 21
			"Core version not specified. Use version statement in #head section.", // 22
			"Missing body.", // 23
			"Token not expected.", // 24
			"; expected.", // 25
			"A statement is missing.", //26
			"{0} expected.", //27
			"KSID names in #names have too be fully specified, with versions.", //28
			"Version literal expected.", // 29
			"Include incorrectly specified. Use: include \"file\" xxxx_xxxx_xxxx_xxxx;", //30
			"File \"{0}\" not fount or is not accessible.", // 31
			"Duplicated header section.", // 32
			"Invalid declaration.", // 33
			"Names can be declared only in global scope.", // 34
			"Classes cannot be nested. Classes can be declared only in global scope.", // 35
			"The declaration ({0}) can occure only inside a class.", // 36
			"Expected declaration of a method parameter.", // 37
			"Modifier {0} not allowed for this field.", // 38
			"Identifier expected.", // 39
			"Identifier of Direct paramater has to be simple (not structured).",  // 40
			"Only one of modifiers 'safe', 'override' or 'direct' can be used at a time.", // 41
			"Modifier '{0}' not valid at this context.", // 42
			"Modifier of such type already used.", // 43
			"Shift operator ( << or >> ) expected.", // 44
			"Expected KSID type \"{0}\". That's different from declaration type \"{1}\".", // 45
			"KSID name \"{0}\" not declared.", // 46
			"Name versions are in conflict. To resolve add manually a record to the name table. Possible versions: {0}.", // 47
			"For this name compiler used a version from an unused name ({0}). Check if it is correct.", // 48
			"Cannot redeclare language type from {0} to {1}.", // 49
			"Ksid name dependencies go in cycle. Name in cycle: {0}.", // 50
			"Multiple declarations are not allowed.", // 51
			"Ambiguous inheritance in class \"{0}\". Same field \"{1}\" is inherited from two or more independent base classes. Move the field to only one common base class or redeclare it in \"{0}\".", // 52
			"File name \"{0}\" doesn't contaion version.", // 53
			"Variable cannot be void.", // 54
			"Identifier of Local Variable has to be simple (not structured).",  // 55
			"Local variable already declared. Variable of same name cannot be declared in identical, parent or children code block.", // 56
			"Expression if type 'object' requiered.", // 57
			"Field \"{0}\" is not a member of \"{1}\".", // 58
			"Expression of type 'array' requiered.", // 59
			"Expression of numeric type requiered.", // 60
			"Type mismatch. Type '{0}' is not compatible with unary operator '{1}'.", // 61
			"LValue (local variable, parameter, object or array field or static variable) requiered.", // 62
			"Unable to determine type of a LValue subexpression. Use temporal variable and split the expression to solve it.", // 63
			"Unable to determine type of a subexpression. Use temporal variable and split the expression to solve it.", // 64
			"Type mismatch. Binary operator '{2}' doesn't accept types '{0}' and '{1}'.", // 65
			"Wrong number of arguments.", // 66
			"Unable to convert type to '{0}'.", // 67
			"Type 'void' not allowed.", // 68
			"Static field expected.", // 69
			"Expression of type 'name' requiered.", // 70
			"Expected type of declared variable.", // 71
			"Initialization not allowed.", // 72
			"No enclosing loop out of which to break or continue.", // 73
			"Expected Array containing type '{0}'.", // 74
			"Only one definition of Direct Method is allowed.", // 75
			"Unable to create output file: '{0}'.", // 76
			"Code generation failed.", // 77
			"Static methods cannot acces member fields without an object reference.", // 78
			"'this' in static methods is always null.", // 79
			"Unable to make reference to '{0}'. Types have to match exactly.", // 80
			"It's not allowed to use object members.", // 81
			"It's not allowed to use static members.", // 82
			"Constant expression requiered.", // 83
			"Method name expected.", // 84
			"Array cannot be void.", // 85
			"retor, retand and retadd can be applied only on types int and char.", // 86
			"Direct method was not implemented.", // 87
			"assigned keyword can be used only in safe methods.", // 88
			"Mothod's parameter name requiered.", // 89
			"Enums can be declared only in global scope.", // 90
			"Data Objects can be initialized only in global scope.", // 91
			"Type of neme in form \"xxxx name\" expected.", // 92
			"Name of a class expected.", // 93
			"KSID name of type 'class' or 'enum' expected. This name has wrong type.", // 94
			"Enum definition is different in another place.", // 95
			"Data Object '{0}' declared with incompatoble classes.", // 96
			"Changing a constant variable is not allowed.", // 97
			"Changing a constant object is not allowed.", // 98
			"You cannot assign or cast reference to a constant object to a reference to non constant object.", // 99
			"It's illegal to call non constant method on a constant object.", // 100
			"Maximum count of array dimensions exceeded.", // 101
			"You can use only one from the access modifiers (private, protected and public).", // 102
			"Index to array is out of range.", // 103
			"Accessing null array", // 104
			"Constant modifiers are requiered on all levels.", // 105
			"Compiler is unable to retrieve constant value '{0}'. There may be cycles in constant declarations.", // 106
			"The value doesn't belong to enum '{0}'.", // 107
			"Division by zero.", // 108
			"Unable to load definitions of attributes.", // 109
			"Unknown attribute name.", // 110
			"Using the attribute on this field is illegal.", // 111
			"The attribute was used previosly with different value.", // 112
			"Attribute definition attached to wrong KSID name. Verify the attribute definition. Attribute name has to be public variable name, each array level has to be const, objects are not allowed.", // 113
			"Invalid identifier in KSID format.", // 114
			"Unknown custom subtype of name group or control.", // 115
		};
	}




	public class ErrorDescription
	{

		private ErrorType _errorType;
		public ErrorType ErrorType {
			get { return _errorType; }
		}

		private ErrorCode _errorCode;
		public ErrorCode ErrorCode {
			get { return _errorCode; }
		}

		private String _message;
		public String Message {
			get { return _message; }
		}

		private String _file;
		public String File {
			get { return _file; }
		}

		private int _pos;
		public int Pos {
			get { return _pos; }
		}

		private int _size;
		public int Size {
			get { return _size; }
		}

		private PositionInLines _positionInLines;
		// can be null
		public PositionInLines PositionInLines {
			get { return _positionInLines; }
		}


		// CONSTRUCTOR

		internal ErrorDescription()
			: this(ErrorType.InternalError, ErrorCode.IUnspecifiedError, null, "", 0, 0, null) 
		{ }



		internal ErrorDescription(ErrorType type, ErrorCode code, String message, String file, int pos, int size, PositionInLines positionInLines) {
			_errorType = type;
			_errorCode = code;
			_message = message;
			if (_message == null || message.Length == 0)
				_message = ErrorTexts.Texts[(int)_errorCode];
			_file = file;
			_pos = pos;
			_size = size;
			_positionInLines = positionInLines;
		}

		public override string ToString() {
			return _message;
		}
	}




	internal class ErrorDescriptionComparer : IComparer<ErrorDescription>
	{
		public int Compare(ErrorDescription x, ErrorDescription y) {
			if (x == null)
				throw new ArgumentNullException("x");
			if (y == null)
				throw new ArgumentNullException("y");

			if (x.ErrorType == y.ErrorType) {
				int ret = String.Compare(x.File, y.File, true, CultureInfo.CurrentCulture);
				if (ret == 0) {
					if (x.Pos == y.Pos) {
						return (int)x.ErrorCode - (int)y.ErrorCode;
					} else {
						return x.Pos - y.Pos;
					}
				} else {
					return ret;
				}
			} else {
				return -((int)x.ErrorType - (int)y.ErrorType);
			}
		}
	}

	
	public class ErrorLog
	{
		private SortedDictionary<ErrorDescription, ErrorDescription> _errors = new SortedDictionary<ErrorDescription, ErrorDescription>(new ErrorDescriptionComparer());
		public ICollection<ErrorDescription> Errors {
			get { return _errors.Values; }
			
		}

		private int _errorCount;
		public int ErrorCount {
			get { return _errorCount; }
		}

		private int _warningCount;
		public int WarningCount {
			get { return _warningCount; }
		}

		private List<ErrorLog> _subordinateErrorLogs = new List<ErrorLog>();

		// Constructor 

		public ErrorLog() {
		}

		private ErrorDescription LogError(String file, int pos, int size, PositionInLines positionInLines, ErrorCode errorCode, params object[] args) {

			String message;
			if (args != null && args.Length > 0) {
				message = String.Format(CultureInfo.CurrentCulture, ErrorTexts.Texts[(int)errorCode], args);
			} else {
				message = ErrorTexts.Texts[(int)errorCode];
			}

			ErrorType type = ErrorType.None;
			switch (errorCode.ToString()[0]) {
				case 'W':
					type = ErrorType.Warning;
					break;
				case 'E':
					type = ErrorType.Error;
					break;
				case 'I':
					type = ErrorType.InternalError;
					break;
				case 'F':
					type = ErrorType.FatalError;
					break;
			}

			if (file == null)
				file = "";

			ErrorDescription ret = new ErrorDescription(type, errorCode, message, file, pos, size, positionInLines);
			ret = InsertErrorDescription(ret);

			return ret;
		}


		private ErrorDescription InsertErrorDescription(ErrorDescription description) {
			ErrorDescription ret;

			// test if the error is already logged
			if (!_errors.TryGetValue(description, out ret)) {
				// insert it in the log
				ret = description;
				if (ret.ErrorType == ErrorType.Warning) {
					_warningCount++;
				} else {
					_errorCount++;
				}
				_errors.Add(ret, ret);
			}

			return ret;
		}



		public ErrorDescription LogError(Lexical lexical, int pos, int size, ErrorCode errorCode, params object[] args) {
			if (lexical == null) {
				return LogError("", pos, size, null, errorCode, args);
			} else {
				return LogError(lexical.File, pos, size, new PositionInLines(pos, size, lexical.Lines), errorCode, args);
			}
		}

		public ErrorDescription LogError(String file, ErrorCode errorCode, params object[] args) {
			return LogError(file, 0, 0, null, errorCode, args);
		}
		public ErrorDescription LogError(LexicalToken token, ErrorCode errorCode, params object[] args) {
			if (token == null || !token.PointsToFile) {
				return LogError("", 0, 0, null, errorCode, args);
			} else {
				return LogError(token.Lexical.File, token.Pos, token.Size, token.PositionInLines, errorCode, args);
			}
		}


		public void ThrowAndLogError(String file, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(file, errorCode, args);
			throw new ErrorException(d.Message, d);
		}
		public void ThrowAndLogError(Lexical lexical, int pos, int size, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(lexical, pos, size, errorCode, args);
			throw new ErrorException(d.Message, d);
		}
		public void ThrowAndLogError(LexicalToken token, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(token, errorCode, args);
			throw new ErrorException(d.Message, d);
		}
		public void ThrowAndLogFatalError(String file, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(file, errorCode, args);
			throw new FatalErrorException(d.Message, d);
		}
		public void ThrowAndLogFatalError(Lexical lexical, int pos, int size, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(lexical, pos, size, errorCode, args);
			throw new FatalErrorException(d.Message, d);
		}
		public void ThrowAndLogFatalError(LexicalToken token, ErrorCode errorCode, params object[] args) {
			ErrorDescription d = LogError(token, errorCode, args);
			throw new FatalErrorException(d.Message, d);
		}



		public void AddErrors(ErrorLog log) {
			foreach (ErrorDescription description in log.Errors) {
				InsertErrorDescription(description);
			}
		}


		public void RegisterSubordinateLog(ErrorLog log) {
			if (!_subordinateErrorLogs.Contains(log))
				_subordinateErrorLogs.Add(log);
		}


		public void JoinErrors() {
			foreach (ErrorLog log in _subordinateErrorLogs) {
				AddErrors(log);
			}
		}

		public void Clear() {
			_errors.Clear();
			_errorCount = 0;
			_warningCount = 0;
		}

	}




	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class CompilerException : Exception
	{
		private ErrorDescription _errorDescription;
		public ErrorDescription ErrorDescription {
			get { return _errorDescription; }
		}

		public CompilerException(String message, ErrorDescription errorDescription) 
			: base(message)
		{
			_errorDescription = errorDescription;
			if (_errorDescription == null)
				_errorDescription = new ErrorDescription();
		}
	}


	// this exception will not be caught -> crash
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
	public class InternalCompilerException : Exception
	{
		public InternalCompilerException()
			: base("Internal error in KRKAL Compiler") { }
		public InternalCompilerException(String message)
			: base(message) { }
		public InternalCompilerException(String message, Exception innerException)
			: base(message, innerException) { }
	}


	public class ErrorException : CompilerException
	{
		public ErrorException(String message, ErrorDescription errorDescription)
			: base(message, errorDescription) { }
	}

	public class FatalErrorException : CompilerException
	{
		public FatalErrorException(String message, ErrorDescription errorDescription)
			: base(message, errorDescription) { }
	}

	//public class EndOfFileException : CompilerException
	//{
	//    public EndOfFileException(String message, ErrorDescription errorDescription)
	//        : base(message, errorDescription) { }
	//    public EndOfFileException(String message, String file)
	//        : base(message, new ErrorDescription(ErrorType.Error, ErrorCode.EEndOfFileReached, message, file, 0, 0)) { }
	//}



}