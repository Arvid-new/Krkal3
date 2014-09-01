//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - H e a d e r
///
///		represents a file header
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	public class IncludeDescription
	{
		private LexicalToken _fileName;
		public LexicalToken FileName {
			get { return _fileName; }
		}

		private LexicalToken _version;
		public LexicalToken Version {
			get { return _version; }
		}

		internal IncludeDescription(LexicalToken fileName, LexicalToken version) {
			_fileName = fileName;			
			_version = version;
		}
	}


	public class Header 
	{
		private Lexical _lexical;
		public Lexical Lexical {
			get { return _lexical; }
		}

		private SyntaxTemplates _syntax;

		bool _isSystem;
		public bool IsSystem {
			get { return _isSystem; }
		}

		bool _isComponent;
		public bool IsComponent {
			get { return _isComponent; }
		}

		bool _generatesData;
		public bool GeneratesData {
			get { return _generatesData; }
		}

		LexicalToken _engine;
		public LexicalToken Engine {
			get { return _engine; }
		}

		LexicalToken _version;
		public LexicalToken Version {
			get {
				if (_version == null)
					_lexical.ErrorLog.ThrowAndLogFatalError(_lexical.File, ErrorCode.FCoreVersionNotSpecified);
				return _version; 
			}
		}

		LexicalToken _attributesDefinition;
		public LexicalToken AttributesDefinition {
			get { return _attributesDefinition; }
		}

		private LexicalToken _endHeaderToken;
		public LexicalToken EndHeaderToken {
			get { return _endHeaderToken; }
		}

		private LexicalToken _attributesToken;
		public LexicalToken AttributesToken {
			get { return _attributesToken; }
		}


		private List<IncludeDescription> _includes = new List<IncludeDescription>();
		public IEnumerable<IncludeDescription> Includes {
			get {
				foreach (IncludeDescription includeDescription in _includes) {
					yield return includeDescription;
				}
			}
		}

		private NameTable _nameTable = new NameTable();
		internal NameTable NameTable {
			get { return _nameTable; }
		}



		// CONSTRUCTOR

		internal Header(Lexical lexical) {
			_lexical = lexical;
			_syntax = new SyntaxTemplates(lexical.ErrorLog, lexical);
			LexicalToken token = null;
			_lexical.ReadingPosition = 0;

			bool[] sectionsEntered = new bool[CompilerConstants.HeaderSectionCount];


			while ((token = _lexical.Read()).Type == LexicalTokenType.HeaderKeyword) {

				try {

					CheckSectionDuplicity(token.HeaderKeyword, sectionsEntered);

					switch (token.HeaderKeyword) {
						case HeaderKeywordType.Head:
							_syntax.DoBody(DoBodyParameters.DoHeadBody, DoHead, OperatorType.SemiColon, OperatorType.RightCurlyBracket);
							break;
						case HeaderKeywordType.Attributes:
							_attributesToken = _syntax.RememberAttributes();
							break;
						case HeaderKeywordType.Names:
							_syntax.DoBody(DoBodyParameters.DoHeadBody, DoName, OperatorType.SemiColon, OperatorType.RightCurlyBracket);
							break;
						default:
							throw new InternalCompilerException();
					}

				}
				catch (ErrorException) {
					_lexical.SkipOutsidePart();
				}

			}

			_endHeaderToken = token;

		}

		private void CheckSectionDuplicity(HeaderKeywordType headerKeywordType, bool[] sectionsEntered) {
			if (!sectionsEntered[(int)headerKeywordType]) {
				sectionsEntered[(int)headerKeywordType] = true;
			} else {
				_lexical.ErrorLog.LogError(_lexical.CurrentToken, ErrorCode.EDuplicatedHeaderSection);
			}
		}




		private bool DoName() {
			LexicalToken tokenName = _lexical.Peek();
			if (tokenName.Type == LexicalTokenType.Identifier) {
				if (!tokenName.Identifier.IsFull)
					_lexical.ErrorLog.LogError(tokenName, ErrorCode.EWrongHeadName);
				_lexical.Read();

				LexicalToken tokenOld = _lexical.Peek();
				if (tokenOld.Type == LexicalTokenType.Identifier && tokenOld.Identifier.IsSimple && tokenOld.Identifier.Simple == "old") {
					_lexical.Read();
					_nameTable.Add(tokenName, true);
				} else {
					_nameTable.Add(tokenName, false);
				}

				_syntax.DoSemiColon();
				return true;
			} else {
				return false;
			}
		}


		private bool DoHead() {
			LexicalToken token = _lexical.Peek();
			if (token.Type == LexicalTokenType.Identifier && token.Identifier.IsSimple) {
				_lexical.Read();

				HeaderFieldType fieldType;
				if (!CompilerConstants.TestHeaderField(token.Identifier.Simple, out fieldType))
					_lexical.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnknownHeaderKeyword);

				switch (fieldType) {
					case HeaderFieldType.Version:
						if (_version != null)
							_lexical.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnexpectedToken);
						_version = _syntax.TryReadToken(LexicalTokenType.VersionString);
						if (_version == null)
							_lexical.ErrorLog.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EVersionExpected);
						break;
					case HeaderFieldType.Include:
						{
							LexicalToken file = _syntax.TryReadToken(LexicalTokenType.String);
							LexicalToken version = _syntax.TryReadToken(LexicalTokenType.VersionString);
							if (file == null || version == null)
								_lexical.ErrorLog.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EIncludeError);
							_includes.Add(new IncludeDescription(file, version));
							break;
						}
					case HeaderFieldType.System:
						_isSystem = true;
						break;
					case HeaderFieldType.Component:
						_isComponent = true;
						break;
					case HeaderFieldType.AttributesDefinition:
						if (_attributesDefinition != null)
							_lexical.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnexpectedToken);
						if (_syntax.Lexical.Peek().Type != LexicalTokenType.Identifier && _syntax.Lexical.Peek().Type != LexicalTokenType.String)
							_lexical.ErrorLog.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "string or identifier");
						_attributesDefinition = _syntax.Lexical.Read();
						break;
					case HeaderFieldType.GeneratesData:
						_generatesData = true;
						break;
					case HeaderFieldType.Engine:
						if (_engine != null)
							_lexical.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnexpectedToken);
						if (_syntax.Lexical.Peek().Type != LexicalTokenType.Identifier && _syntax.Lexical.Peek().Type != LexicalTokenType.String)
							_lexical.ErrorLog.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "string or identifier");
						_engine = _syntax.Lexical.Read();
						break;
					
					default:
						throw new InternalCompilerException();

				}

				_syntax.DoSemiColon();

				return true;
			} else {
				return false;
			}
		}

	}
}
