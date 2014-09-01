//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - L e x i c a l
///
///		Lexical analysis and file cache
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;

namespace Krkal.Compiler
{



	public class Lexical
	{

		private KrkalPath _krkalPath;
		public KrkalPath KrkalPath {
			get { return _krkalPath; }
		}
		public String File {
			get { return _krkalPath != null ? _krkalPath.Full : null; }
		}
		public string FileVersion {
			get { return _krkalPath.Version; }
		}

		private ErrorLog _errorLog = new ErrorLog();
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}

		private String _content;
		public String Content {
			get { return _content; }
		}
		// doesn't throw exception
		public char GetCharacter(int pos) {
			try {
				return _content[pos];
			}
			catch (IndexOutOfRangeException) {
				return '\0'; // all character tests to \0 will fail
			}
		}

		private LexicalToken _firstToken;
		public LexicalToken FirstToken {
			get { return _firstToken; }
		}

		private int _readingPosition;
		public int ReadingPosition {
			get { return _readingPosition; }
			set {
				_peekedToken = null;
				_prevToken = null;
				_readingPosition = value; 
			}
		}

		private LexicalToken _peekedToken;
		private LexicalToken _prevToken;

		private LexicalToken _currentToken;
		public LexicalToken CurrentToken {
			get { return _currentToken; }
		}

		public LexicalToken EndHeaderToken {
			get { return Header.EndHeaderToken; }
		}


		private Header _header;
		public Header Header {
			get { return _header; }
		}

		private Declarations _declarations;
		public Declarations Declarations {
			get { return _declarations; }
		}


		private Lines _lines;
		internal Lines Lines {
			get {
				if (_lines == null)
					_lines = new Lines(_content);
				return _lines; 
			}
		}


		// CONSTRUCTOR

		public Lexical(String file, String content) {
			KrkalCompiler compiler = KrkalCompiler.Compiler; // to init Constants
			if (file != null) {
				_krkalPath = new KrkalPath(file, compiler.CustomSyntax.FileExtensions);
			}
			_content = content;
		}




		public void SeekAfterToken(LexicalToken prevToken) {
			if (prevToken == null)
				throw new ArgumentNullException("prevToken");
			ReadingPosition = prevToken.Pos + prevToken.Size;
			_prevToken = prevToken;
			_peekedToken = prevToken.Next;
		}

		public void SeekAtToken(LexicalToken token) {
			if (token == null)
				throw new ArgumentNullException("token");
			ReadingPosition = token.Pos;
			_peekedToken = token;
		}


		public LexicalToken Read(LexicalToken prevToken) {
			SeekAfterToken(prevToken);
			return Read();
		}
		public LexicalToken Read(int pos) {
			ReadingPosition = pos;
			return Read();
		}
		public LexicalToken Read() {
			LexicalToken ret = Peek();
			SeekAfterToken(ret);
			return ret;
		}

		private void UpdateTokenChain(LexicalToken token) {
			if (_prevToken != null)
				_prevToken.Next = token;
			_peekedToken = token;
			if (ReadingPosition == 0)
				_firstToken = token;
		}

		private LexicalToken GetTokenFromCache() {
			if (_peekedToken != null)
				return _peekedToken;
			if (_firstToken != null && ReadingPosition == 0)
				return _firstToken;
			return null;
		}


		public LexicalToken Peek() {
			_currentToken = GetTokenFromCache();
			if (_currentToken == null) {
				_currentToken = new LexicalToken(this, ReadingPosition);
				UpdateTokenChain(_currentToken);
			}
			return _currentToken;
		}

		public LexicalToken Peek(int pos) {
			ReadingPosition = pos;
			return Peek();
		}

		public LexicalToken Peek(LexicalToken prevToken) {
			SeekAfterToken(prevToken);
			return Peek();
		}



		public LexicalToken PeekNth(int index) {
			LexicalToken ret = Peek();
			for (; index > 0; index--) {
				if (ret.Next == null)
					ret.Next = new LexicalToken(this, ret.Pos + ret.Size);
				ret = ret.Next;
			}
			_currentToken = ret;
			return ret;
		}





		/// <summary>
		///	ruturns true and optinoaly increments position, if there is newline
		/// otherwise returns false
		/// </summary>
		/// <param name="pos">position in the source code text</param>
		/// <param name="increasePosition">if true and newline found, the pos will be incremented. Otherwise pos will be kept</param>
		/// <returns>if there is a newline</returns>
		internal bool ReadNewLine(ref int pos, bool increasePosition) {
			int pos2 = pos;
			if (GetCharacter(pos2) == '\n') {
				pos2++;
				if (increasePosition) {
					pos = pos2;
				}
				return true;
			} else {
				return false;
			}
		}


		internal void ReadWhiteSpace(ref int pos) {
			while (true) {
				if (ReadNewLine(ref pos, true)) {
					// continue
				} else if (CompilerConstants.IsWhiteSpace(GetCharacter(pos))) {
					pos++;
					// continue
				} else if (GetCharacter(pos) == '/') {
					if (GetCharacter(pos + 1) == '/') {
						pos += 2;
						ReadSingleLineComment(ref pos);
						// continue
					} else if (GetCharacter(pos + 1) == '*') {
						pos += 2;
						ReadDelimitedComment(ref pos);
						// continue
					} else {
						return;
					}
				} else {
					return;
				}
			} // end loop
		}



		internal void SkipOutsidePart() {
			int pos = ReadingPosition;
			SkipPart(ref pos, '\0', '}', null, true, true);
			ReadingPosition = pos;
		}

		internal void SkipDelimitedPart(ICollection<char> closingDelimiters) {
			int pos = ReadingPosition;
			SkipPart(ref pos, '\0', '\0', closingDelimiters, false, false);
			ReadingPosition = pos;
		}



		public String ReadBody() {
			int start = ReadingPosition;
			LexicalToken token = Read();
			if (token != OperatorType.LeftCurlyBracket)
				throw new InternalCompilerException("Expected { at the beginning of Body Block");
			int pos = ReadingPosition;
			SkipPart(ref pos, '\0', '}', null, false, true);
			ReadingPosition = pos;
			return _content.Substring(start, pos - start);
		}



		internal void SkipPart(char bracket, bool recoverAfterError) {
			int pos = ReadingPosition;
			SkipPart(ref pos, '\0', bracket, null, recoverAfterError, false);
			ReadingPosition = pos;
		}

		internal void SkipPart(char delimiter, char bracket, bool recoverAfterError) {
			int pos = ReadingPosition;
			SkipPart(ref pos, delimiter, bracket, null, recoverAfterError, false);
			ReadingPosition = pos;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		private void SkipPart(ref int pos, char delimiter, char bracket, ICollection<char> closingDelimiters, bool recoverAfterError, bool eatEndingBracket) {
			List<char> openedBrackets = new List<char>();

			bool doLoop = true;

			while (doLoop) {

				if (pos >= _content.Length) {
					break;
				}
				char ch = GetCharacter(pos);
				pos++;
				switch (ch) {
					case '(':
						openedBrackets.Add(')');
						break;
					case '{':
						openedBrackets.Add('}');
						break;
					case '[':
						openedBrackets.Add(']');
						break;
					case ')':
					case '}':
					case ']':
						int ff = openedBrackets.LastIndexOf(ch);
						if (ff >= 0) {
							if (!recoverAfterError && openedBrackets.Count > ff+1) {
								_errorLog.LogError(this, pos - 1, 1, ErrorCode.EClosingBracketMissing);
							}
							openedBrackets.RemoveRange(ff, openedBrackets.Count-ff);
							if (recoverAfterError && openedBrackets.Count == 0 && ch == bracket)
								doLoop = false;
						} else {
							if (ch == bracket) {
								if (!eatEndingBracket)
									pos--; // do not eat the bracket
								doLoop = false;
							} else if (closingDelimiters != null && closingDelimiters.Contains(ch)) {
								pos--;
								doLoop = false;
							} else if (!recoverAfterError) {
								_errorLog.LogError(this, pos - 1, 1, ErrorCode.EOpeningBracketMissing);
							}
						}
						break;
					case '/':
						if (GetCharacter(pos) == '/') {
							pos ++;
							ReadSingleLineComment(ref pos);
						} else if (GetCharacter(pos) == '*') {
							pos ++;
							try {
								ReadDelimitedComment(ref pos);
							}
							catch (CompilerException) { }
						}
						break;
					case '"':
						SkipCharacterOrString(ref pos, '"');
						break;
					case  '\'':
						SkipCharacterOrString(ref pos, '\'');
						break;
					case '@':
						if (GetCharacter(pos) == '"') {
							pos++;
							SkipVerbatimString(ref pos);
						}
						break;
					default:
						if (openedBrackets.Count == 0) {
							if (delimiter != '\0' && ch == delimiter) {
								doLoop = false;
							} else if (closingDelimiters != null && closingDelimiters.Contains(ch)) {
								pos--;
								doLoop = false;
							}
						}
						break;
				}

				
			} // end loop

			if (!recoverAfterError && openedBrackets.Count > 0) {
				_errorLog.LogError(this, pos - 1, 1, ErrorCode.EClosingBracketMissing);
			}

		}



		private void SkipVerbatimString(ref int pos) {
			for (; pos < _content.Length; pos++) {
				if (GetCharacter(pos) == '"') {
					if (GetCharacter(pos + 1) == '"') {
						pos++;
					} else {
						break;
					}
				}
			}
			pos++;
		}

		private void SkipCharacterOrString(ref int pos, char quote) {
			bool isExcapeSequence = false;
			for (; pos < _content.Length && (GetCharacter(pos) != quote || isExcapeSequence); pos++) {
				if (isExcapeSequence) {
					isExcapeSequence = false;
				} else if (GetCharacter(pos) == '\\') {
					isExcapeSequence = true;
				}
			}
			pos++;
		}



		internal void ReadSingleLineComment(ref int pos) {
			while (pos < _content.Length && !ReadNewLine(ref pos, false)) {
				pos++;
			}
		}



		internal void ReadDelimitedComment(ref int pos) {
			try {
				while (true) {
					if (_content[pos] == '*' && _content[pos + 1] == '/') {
						pos += 2;
						return;
					} else {
						pos++;
					}
				}
			}
			catch (IndexOutOfRangeException) {
				_errorLog.ThrowAndLogError(this, pos, 1, ErrorCode.ECommentNotTerminated);
			}
		}



		public void DoHeader() {
			if (_header == null)
				_header = new Header(this);
		}

		public void DoDeclarations() {
			if (_declarations == null)
				_declarations = new Declarations(this);
		}


	}





	public class LexicalCache
	{
		private INonChangedFileCollection _nonChangedFiles = KrkalCompiler.Compiler.FS.RegisterNonChangedFiles();
		private Dictionary<String, Lexical> _cache = new Dictionary<string, Lexical>(StringComparer.CurrentCultureIgnoreCase);


		public Lexical Get(String file, bool dontUseCache) {
			lock (_cache) {
				if (!dontUseCache && _nonChangedFiles.Contains(file)) {
					return _cache[file];
				} else {
					_cache.Remove(file);
					Lexical ret = new Lexical(file, KrkalCompiler.Compiler.FS.OpenFileForReading(file, _nonChangedFiles));
					_cache.Add(file, ret);
					return ret;
				}
			}
		}


		internal void Clear() {
			lock (_cache) {
				_nonChangedFiles = KrkalCompiler.Compiler.FS.RegisterNonChangedFiles();
				_cache.Clear();
			}
		}
	}

}
