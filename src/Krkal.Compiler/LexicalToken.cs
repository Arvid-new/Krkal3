//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - L e x i c a l T o k e n
///
///		Represents a lexical token
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Krkal.Compiler
{
	public enum LexicalTokenType
	{
		Unknown,
		Keyword,
		HeaderKeyword,
		Operator,
		Identifier,
		Char,
		Int,
		Double,
		String,
		VersionString,
		Error,
		Eof,
	}



	public class LexicalToken
	{
		private LexicalTokenType _type;
		public LexicalTokenType Type {
			get { return _type; }
		}

		private int _pos;
		public int Pos {
			get { return _pos; }
		}

		private int _size;
		public int Size {
			get { return _size; }
		}

		private Lexical _lexical;
		public Lexical Lexical {
			get { return _lexical; }
		}

		public bool PointsToFile {
			get { return _lexical != null && _lexical.KrkalPath != null; }
		}

		private LexicalToken _next;
		public LexicalToken Next {
			get { return _next; }
			set { _next = value; }
		}


		private PositionInLines _positionInLines;
		public PositionInLines PositionInLines {
			get {
				if (_positionInLines == null)
					_positionInLines = new PositionInLines(_pos, _size, _lexical.Lines);
				return _positionInLines; 
			}
		}



		private object _data;

		public Keyword Keyword {
			get {
				if (_type != LexicalTokenType.Keyword)
					throw new InternalCompilerException("IncompatibleType");
				return (Keyword)_data;
			}
		}

		public HeaderKeywordType HeaderKeyword {
			get {
				if (_type != LexicalTokenType.HeaderKeyword)
					throw new InternalCompilerException("IncompatibleType");
				return (HeaderKeywordType)_data;
			}
		}

		public Identifier Identifier {
			get {
				if (_type != LexicalTokenType.Identifier)
					throw new InternalCompilerException("IncompatibleType");
				return (Identifier)_data;
			}
		}

		public LanguageOperator Operator {
			get {
				if (_type != LexicalTokenType.Operator)
					throw new InternalCompilerException("IncompatibleType");
				return (LanguageOperator)_data;
			}
		}


		public String Text {
			get {
				String ret = _data as String;
				if (ret == null)
					throw new InternalCompilerException("IncompatibleType");
				return ret;
			}
		}

		public char Char {
			get {
				if (_type != LexicalTokenType.Char)
					throw new InternalCompilerException("IncompatibleType");
				return (Char)_data;
			}
		}

		public int Int {
			get {
				if (_type == LexicalTokenType.Char) {
					return (char)_data;
				} else if (_type == LexicalTokenType.Int) {
					return (int)_data;
				} else {
					throw new InternalCompilerException("IncompatibleType");
				}
			}
		}

		public double Double {
			get {
				if (_type == LexicalTokenType.Char) {
					return (char)_data;
				} else if (_type == LexicalTokenType.Int) {
					return (int)_data;
				} else if (_type == LexicalTokenType.Double) {
					return (double)_data;
				} else {
					throw new InternalCompilerException("IncompatibleType");
				}
			}
		}




		// CONSTRUCTORS


		public LexicalToken(Lexical lexical, int pos) {
			if (lexical == null)
				throw new ArgumentNullException("lexical");
			_lexical = lexical;

			_lexical.ReadWhiteSpace(ref pos);

			_pos = pos;
			if (_pos >= _lexical.Content.Length) {
				_size = 1;
				_type = LexicalTokenType.Eof;
				return;
			}

			try {
				String version = ParseVersion();
				if (version != null) {
					_type = LexicalTokenType.VersionString;
					_data = version;
					return;
				}

				char ch = _lexical.GetCharacter(_pos);
				switch (ch) {
					case '\'':
						ParseCharacter();
						break;
					case '"':
						ParseString();
						break;
					case '@':
						if (_lexical.GetCharacter(_pos + 1) == '"') {
							ParseVerbatimString();
						} else {
							ParseIdentifier(IdentifierRoot.Kernel);
						}
						break;
					case '.':
						ParseNumber(true);
						break;
					case '$':
						ParseIdentifier(IdentifierRoot.User);
						break;
					case '0':
						if (_lexical.GetCharacter(_pos + 1) == 'x' || _lexical.GetCharacter(_pos + 1) == 'X') {
							ParseHexadecimalNumber();
						} else {
							ParseNumber(false);
						}
						break;
					case '#':
						ParseHeaderKeyword();
						break;
					default:
						if (ch == '_') {
							if (_lexical.GetCharacter(_pos + 1) == 'K' && _lexical.GetCharacter(_pos + 2) == 'S' && _lexical.GetCharacter(_pos + 3) == 'I' && (_lexical.GetCharacter(_pos + 4) == 'D' || _lexical.GetCharacter(_pos + 4) == 'd') && _lexical.GetCharacter(_pos + 5) == '_') {
								ParseKsidIdentifier();
								break;
							}
						}
						if (CompilerConstants.IsDigit(ch)) {
							ParseNumber(false);
						} else if (CompilerConstants.IsFirstIdCharacter(ch)) {
							ParseIdentifier(IdentifierRoot.Localized);
						} else if (CompilerConstants.IsOperatorCharacter(ch)) {
							ParseOperator();
						} else {
							_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, 1, ErrorCode.EInvalidLexicalToken);
						}
						break;
				}
			}
			catch (ErrorException) {
				if (_size == 0)
					_size = 1;
				_data = null;
				_type = LexicalTokenType.Error;
			}

		}



		private bool IsNextCharacter {
			get {
				return (_pos + _size < _lexical.Content.Length);
			}
		}
		// doesn't throw exception. If the pos is out of range it returns \0
		private char NextCharacter {
			get {
				return _lexical.GetCharacter(_pos + _size);
			}
		}
		// doesn't throw exception. If the pos is out of range it returns \0
		private char NextNthCharacter(int n) {
			return _lexical.GetCharacter(_pos + _size + n);
		}



		private void ParseOperator() {
			LanguageOperator op = null;
			_type = LexicalTokenType.Operator;
			if (_pos + 2 < _lexical.Content.Length && CompilerConstants.TestOperator(_lexical.Content.Substring(_pos, 3), out op)) {
				_size = 3;
			} else if (_pos + 1 < _lexical.Content.Length && CompilerConstants.TestOperator(_lexical.Content.Substring(_pos, 2), out op)) {
				_size = 2;
			} else if (_pos < _lexical.Content.Length && CompilerConstants.TestOperator(_lexical.Content.Substring(_pos, 1), out op)) {
				_size = 1;
			} else {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, 1, ErrorCode.EInvalidLexicalToken);						
			}

			_data = op;
		}


		private void ParseHeaderKeyword() {
			_size = 1;
			_type = LexicalTokenType.HeaderKeyword;

			for ( ; CompilerConstants.IsNextIdCharacter(NextCharacter); _size++) ;

			HeaderKeywordType headerKeywordType;
			if (!CompilerConstants.TestHeaderKeyword(_lexical.Content.Substring(_pos, _size), out headerKeywordType))
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EUnknownHeaderKeyword);
			_data = headerKeywordType;
		}


		private void ParseNumber(bool hasDot) {
			bool hasExponent = false;
			_size = 1; // I don't need to check first character (its either . or digit)

			for (; ; _size++) {
				char ch = NextCharacter;
				if (!hasDot && !hasExponent && ch == '.') {
					hasDot = true;
				} else if (!hasExponent && (ch == 'e' || ch == 'E')) {
					hasExponent = true;
					char ch2 = NextNthCharacter(1);
					if (ch2 == '-' || ch2 == '+')
						_size++;
				} else if (!CompilerConstants.IsDigit(ch)) {
					break;
				}
			}

			try {
				if (hasDot || hasExponent) {
					_type = LexicalTokenType.Double;
					double d = Double.Parse(_lexical.Content.Substring(_pos, _size), NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);
					_data = d;
				} else {
					_type = LexicalTokenType.Int;
					int i = int.Parse(_lexical.Content.Substring(_pos, _size), NumberStyles.None, CultureInfo.InvariantCulture);
					_data = i;
				}
			}
			catch (FormatException) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EInvalidNumberFormat);
			}
			catch (OverflowException) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.ENumericLiteralOverflow);
			}
		}


		private void ParseHexadecimalNumber() {
			_size = 2;
			for (; CompilerConstants.IsHexadecimalDigit(NextCharacter); _size++) ;

			_type = LexicalTokenType.Int;
			try {
				int i = int.Parse(_lexical.Content.Substring(_pos + 2, _size - 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				_data = i;
			}
			catch (FormatException) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EInvalidNumberFormat);
			}
			catch (OverflowException) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.ENumericLiteralOverflow);
			}			
		}


		private void ParseKsidIdentifier() {
			_size = 0;
			for (; CompilerConstants.IsNextIdCharacter(NextCharacter); _size++) ;
			_type = LexicalTokenType.Identifier;
			try {
				Identifier id = Identifier.ParseKsid(_lexical.Content.Substring(_pos, _size));
				_data = id;
			}
			catch (FormatException) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EWronKsidIdentifierFormat);
			}
		}


		private void ParseIdentifier(IdentifierRoot identifierRoot) {
			List<Identifier.Part> parts = new List<Identifier.Part>();
			Identifier.Part part;
			Keyword keyword;

			if (identifierRoot == IdentifierRoot.Localized) {
				_size = 0;
			} else {
				_size = 1;
			}

			while(true) {
				part = ParseIdentifierPart();

				// keyword
				if (CompilerConstants.TestKeyword(part.Name, out keyword)) {
					if (part.Version == null && identifierRoot == IdentifierRoot.Localized && parts.Count == 0) {
						_type = LexicalTokenType.Keyword;
						_data = keyword;
						return;
					} else {
						_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EKeywordInsideIdentifier, part.Name);
					}
				}

				parts.Add(part);

				if (NextCharacter == '.') {
					_size++;
				} else {
					break;
				}

			}

			_type = LexicalTokenType.Identifier;
			Identifier id = new Identifier(identifierRoot, parts);
			_data = id;
		}


		private Identifier.Part ParseIdentifierPart() {
			int start = _pos + _size;
			int oldSize = _size;

			if (!CompilerConstants.IsFirstIdCharacter(NextCharacter)) {
				_lexical.ErrorLog.ThrowAndLogError(_lexical, start, 1, ErrorCode.EInvalidCharacterInId);
			}
			_size++;
			for (; CompilerConstants.IsNextIdCharacter(NextCharacter); _size++ ) ;

			String name = _lexical.Content.Substring(start, _size - oldSize);
			if (name.Contains(CompilerConstants.ForbiddenIdText))
				_lexical.ErrorLog.ThrowAndLogError(_lexical, start, _size - oldSize, ErrorCode.EForbiddenIdText);

			String version = null;

			if (NextCharacter == '$') {
				_size++;
				start = _pos + _size;
				version = ParseVersion();
				if (version == null)
					_lexical.ErrorLog.ThrowAndLogError(_lexical, start, _pos + _size - start + 1, ErrorCode.EInvalidVersionLiteral);
			}

			return new Identifier.Part(name, version);
		}


		private string ParseVersion() {
			int start = _pos + _size;
			int oldSize = _size;

			for (int part = 0; part < 4; part++) {
				if (part > 0) {
					if (NextCharacter != '_')
						return null;
					_size++;
				}
				for (int digit = 0; digit < 4; digit++) {
					if (!CompilerConstants.IsHexadecimalDigit(NextCharacter))
						return null;
					_size++;
				}
			}
			if (CompilerConstants.IsNextIdCharacter(NextCharacter))
				_lexical.ErrorLog.ThrowAndLogError(_lexical, start, _size - oldSize + 1, ErrorCode.EInvalidVersionLiteral);

			return _lexical.Content.Substring(start, _size - oldSize).ToUpper(CultureInfo.InvariantCulture);
		}


		private void ParseVerbatimString() {
			StringBuilder str = new StringBuilder();
			_size = 2;
			int start;
			int oldSize;
			while (true) {
				start = _pos + _size;
				oldSize = _size;
				for (; IsNextCharacter && NextCharacter != '"'; _size++) ;
				if (!IsNextCharacter)
					_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos+_size, 1, ErrorCode.EStringNotTermineted);

				str.Append(_lexical.Content, start, _size - oldSize);
				_size++;

				if (NextCharacter == '"') {
					_size++;
					str.Append('"');
				} else {
					break;
				}
			}

			_type = LexicalTokenType.String;
			_data = str.ToString();
		}


		private void ParseString() {
			_type = LexicalTokenType.String;
			StringBuilder str = new StringBuilder();
			_size = 1;
			int start = _pos + _size;
			int oldSize = _size;

			while (true) {
				if (!IsNextCharacter)
					_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos+_size, 1, ErrorCode.EStringNotTermineted);
				switch (NextCharacter) {
					case '"':
						str.Append(_lexical.Content, start, _size - oldSize);
						_size++;
						_data = str.ToString();
						return;
					case '\\':
						str.Append(_lexical.Content, start, _size - oldSize);
						str.Append(ReadEscapeSequence());
						start = _pos + _size;
						oldSize = _size;
						break;
					case '\n':
						_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos+_size, 1, ErrorCode.ENewLineNotAllowed);
						break;
					default:
						_size++;
						break;
				}
			}
		}

		private char ReadEscapeSequence() {
			_size++;
			char ch = NextCharacter;
			_size++;
			switch (ch) {
				case '\'':
					return '\'';
				case '"':
					return '"';
				case '\\':
					return '\\';
				case '0':
					return '\0';
				case 'a':
					return '\a';
				case 'b':
					return '\b';
				case 'f':
					return '\f';
				case 'n':
					return '\n';
				case 'r':
					return '\r';
				case 't':
					return '\t';
				case 'v':
					return '\v';
				case 'x':
					return ReadEscapeHexaNumber(false);
				case 'u':
					return ReadEscapeHexaNumber(true);
				default:
					_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos + _size - 2, 2, ErrorCode.EInvalidEscapeSequence);
					throw new InternalCompilerException("unreachable place");
			}
		}

		private char ReadEscapeHexaNumber(bool readAllForDigits) {
			int start = _pos + _size;
			int size2 = 0;
			while (size2 < 4 && CompilerConstants.IsHexadecimalDigit(NextCharacter)) {
				_size++;
				size2++;
			}

			if (readAllForDigits && size2 != 4)
				_lexical.ErrorLog.ThrowAndLogError(_lexical, start - 2, size2 + 2, ErrorCode.EInvalidEscapeSequence);

			return (char)ushort.Parse(_lexical.Content.Substring(start, size2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
		}


		private void ParseCharacter() {
			_type = LexicalTokenType.Char;
			_size = 1;

			char ch = NextCharacter;
			if (ch == '\n')
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos+_size, 1, ErrorCode.ENewLineNotAllowed);
			if (ch == '\'')
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EInvalidCharacterConstant);
			if (ch == '\\') {
				ch = ReadEscapeSequence();
			} else {
				_size++;
			}

			if (NextCharacter != '\'')
				_lexical.ErrorLog.ThrowAndLogError(_lexical, _pos, _size, ErrorCode.EInvalidCharacterConstant);
			_size++;

			_data = ch;
		}


		#region Operators

		public static bool operator == (LexicalToken token, KeywordType keywordType) {
			if (token == null || token.Type != LexicalTokenType.Keyword)
				return false;
			return (token.Keyword.Type == keywordType);
		}
		public static bool operator != (LexicalToken token, KeywordType keywordType) {
			return !(token == keywordType);
		}
		public static bool operator ==(KeywordType keywordType, LexicalToken token) {
			return (token == keywordType);
		}
		public static bool operator !=(KeywordType keywordType, LexicalToken token) {
			return !(token == keywordType);
		}

		public static bool operator ==(LexicalToken token, OperatorType operatorType) {
			if (token == null || token.Type != LexicalTokenType.Operator)
				return false;
			return (token.Operator.Type == operatorType);
		}
		public static bool operator !=(LexicalToken token, OperatorType operatorType) {
			return !(token == operatorType);
		}
		public static bool operator ==(OperatorType operatorType, LexicalToken token) {
			return (token == operatorType);
		}
		public static bool operator !=(OperatorType operatorType, LexicalToken token) {
			return !(token == operatorType);
		}

		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		#endregion


		public override string ToString() {
			if (_data != null) {
				return _type.ToString() + ": " + _data.ToString();
			} else {
				return _type.ToString();
			}
		}

		public String ToKsidString() {
			if (_type == LexicalTokenType.Identifier) {
				return Identifier.ToKsidString();
			} else {
				return Text;
			}
		}
	}
}

