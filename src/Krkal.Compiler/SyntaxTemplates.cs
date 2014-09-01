//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - S y n t a x T e m p l a t e s
///
///		Class that helps with syntax analysis
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	[Flags]
	internal enum DoBodyParameters {
		None = 0,
		RequiereLeftBracket = 1,
		RequiereSeparator = 2,
		AllowEmptyStatement = 4,
		Recover = 8,

		DoHeadBody = RequiereLeftBracket | AllowEmptyStatement | Recover,
		DoMethodParameters = RequiereSeparator,
		DoClass = RequiereLeftBracket | AllowEmptyStatement | Recover,
		DoNamesList = RequiereLeftBracket | RequiereSeparator,
		DoCodeBlock = RequiereLeftBracket | AllowEmptyStatement | Recover,
		DoMethodCall = RequiereLeftBracket | RequiereSeparator,
		DoArrayConstriction = RequiereLeftBracket | RequiereSeparator,
		DoEnum = RequiereLeftBracket | RequiereSeparator | Recover,
		DoAttribute = RequiereSeparator | AllowEmptyStatement | Recover,
	}


	[Flags]
	internal enum ExpressionContext
	{
		None = 0,
		Method = 1,
		ConstantRequiered = 2,
		StaticNotAllowed = 4,
		MemberNotAllowed = 8,

		StaticMethod = Method | MemberNotAllowed,
		StaticVariable = StaticNotAllowed | MemberNotAllowed,
		MemberVariable = MemberNotAllowed,
		Parameter = ConstantRequiered | StaticNotAllowed | MemberNotAllowed,
		Attribute = ConstantRequiered | StaticNotAllowed | MemberNotAllowed,
	}


	internal delegate bool DoSyntaxDelegate();

	internal class SyntaxTemplates
	{

		private ErrorLog _log;
		public ErrorLog ErrorLog {
			get { return _log; }
		}

		private Lexical _lexical;
		public Lexical Lexical {
			get { return _lexical; }
		}

		// CONSTRUCTOR

		internal SyntaxTemplates(ErrorLog log, Lexical lexical) {
			_log = log;
			_lexical = lexical;
		}



		public LexicalToken TryReadToken(LexicalTokenType type) {
			if (_lexical.Peek().Type == type) {
				return _lexical.Read();
			} else {
				return null;
			}
		}

		public bool TryReadToken(KeywordType type) {
			if (_lexical.Peek() == type) {
				_lexical.Read();
				return true;
			} else {
				return false;
			}
		}

		public bool TryReadToken(OperatorType type) {
			if (_lexical.Peek() == type) {
				_lexical.Read();
				return true;
			} else {
				return false;
			}
		}


		public void DoSemiColon() {
			if (_lexical.Peek() == OperatorType.SemiColon) {
				_lexical.Read();
			} else {
				_log.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.ESemiColonExpected);
			}
		}

		public void DoClosingBracket(OperatorType bracket) {
			if (_lexical.Peek() == bracket) {
				_lexical.Read();
			} else {
				_log.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EMissingDelimiter, CompilerConstants.GetLanguageOperator(bracket).Text);
			}
		}

		public void DoKeyword(KeywordType keyword) {
			if (_lexical.Peek() == keyword) {
				_lexical.Read();
			} else {
				_log.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, CompilerConstants.GetKeyword(keyword).Text);
			}
		}

		public LexicalToken RememberAttributes() {
			LexicalToken ret = null;
			if (_lexical.Peek() == OperatorType.LeftBracket) {
				ret = _lexical.Read();
				_lexical.SkipPart(']', false);
				DoClosingBracket(OperatorType.RightBracket);
			}
			return ret;
		}


		public void DoBody(DoBodyParameters parameters, DoSyntaxDelegate doInside, OperatorType delimiter, OperatorType rightBracket) {

			if ((parameters & DoBodyParameters.RequiereLeftBracket) != 0) {
				if (_lexical.Read() != CompilerConstants.GetLeftBracker(rightBracket))
					_log.ThrowAndLogError(_lexical.CurrentToken, ErrorCode.EMissingBody);
			}



			bool error = false;
			bool logError;
			LexicalToken token = _lexical.Peek();

			while (token != rightBracket) {
				if ((parameters & DoBodyParameters.AllowEmptyStatement) != 0 && token == delimiter) {
					_lexical.Read();
					token = _lexical.Peek();
					continue;
				}

				try {
					error = !doInside();
					logError = true;
				}
				catch (ErrorException) {
					error = true;
					logError = false;
				}

				token = _lexical.Peek();

				if (token == rightBracket) {
					break;
				}

				if ((parameters & DoBodyParameters.RequiereSeparator) != 0) {
					if (token == delimiter) {
						error = false;
						_lexical.Read();
					} else if (!error) {
						_log.LogError(token, ErrorCode.EMissingDelimiter, CompilerConstants.GetLanguageOperator(delimiter).Text);
					}
				}

				if (error) {
					if ((parameters & DoBodyParameters.Recover) != 0 && token.Type != LexicalTokenType.Eof) {
						if (logError)
							_log.LogError(token, ErrorCode.EUnexpectedToken);
						_lexical.SkipPart(CompilerConstants.GetLanguageOperator(delimiter).Text[0], CompilerConstants.GetLanguageOperator(rightBracket).Text[0], true);
					} else {
						_log.ThrowAndLogError(token, ErrorCode.EMissingDelimiter, CompilerConstants.GetLanguageOperator(rightBracket).Text);
					}
				}

				token = _lexical.Peek();

			}

			_lexical.Read();


		}


		public bool CheckCustomKeyword<T>(CustomKeywords<T> customKeywords, out T info, out WannaBeKsidName wannaBe) where T : CustomKeywordInfo {
			wannaBe = null;
			info = null;
			if (_lexical.PeekNth(1) == customKeywords.MainKeyword) {
				if (customKeywords.TestKeyword(_lexical.Peek(), out info, out wannaBe)) {
					return true;
				}
			}
			return false;
		}

		public bool CheckCustomKeyword<T>(CustomKeywords<T> customKeywords) where T : CustomKeywordInfo {
			WannaBeKsidName wannaBe;
			T info;
			if (_lexical.PeekNth(1) == customKeywords.MainKeyword) {
				if (customKeywords.TestKeyword(_lexical.Peek(), out info, out wannaBe)) {
					return true;
				}
			}
			return false;
		}

	}



	internal class SyntaxTemplatesEx : SyntaxTemplates
	{
		private Compilation _compilation;
		public Compilation Compilation {
			get { return _compilation; }
		}

		private SourceFile _sourceFile;
		public SourceFile SourceFile {
			get { return _sourceFile; }
		}

		private ClassName _thisClass;
		public ClassName ThisClass {
			get { return _thisClass; }
		}

		Identifier _objectContext;
		public Identifier ObjectContext {
			get { return _objectContext; }
		}

		private ExpressionContext _expressionContext;
		internal ExpressionContext ExpressionContext {
			get { return _expressionContext; }
		}

		private bool _constantMethod;
		public bool ConstantMethod {
			get { return _constantMethod; }
		}


		//private ClassName

		// CONSTRUCTOR
		internal SyntaxTemplatesEx(Compilation compilation, SourceFile sourceFile, Identifier objectContext, ClassName thisClass, ExpressionContext expressionContex, bool constantMethod) 
			: base(compilation.ErrorLog, sourceFile.Lexical)
		{
			_compilation = compilation;
			_sourceFile = sourceFile;
			_thisClass = thisClass;
			_objectContext = objectContext;
			_expressionContext = expressionContex;
			_constantMethod = constantMethod;
		}


		public KsidName IdToKsid(LexicalToken id, Identifier localizeTo, NameType expectedNameType) {
			WannaBeKsidName w = new WannaBeKsidName(id, localizeTo, Lexical.Header);
			return _compilation.KsidNames.Find(w, expectedNameType, _sourceFile);
		}

		public KsidName IdToKsid(LexicalToken id, Identifier localizeTo) {
			WannaBeKsidName w = new WannaBeKsidName(id, localizeTo, Lexical.Header);
			return _compilation.KsidNames.Find(w, _sourceFile);
		}

		public KsidName ReadKsid(Identifier localizeTo) {
			LexicalToken token = TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				ErrorLog.ThrowAndLogError(Lexical.CurrentToken, ErrorCode.EIdentifierExpected);
			return IdToKsid(token, localizeTo);
		}

		public KsidName ReadKsid(Identifier localizeTo, NameType expectedNameType) {
			LexicalToken token = TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				ErrorLog.ThrowAndLogError(Lexical.CurrentToken, ErrorCode.EIdentifierExpected);
			return IdToKsid(token, localizeTo, expectedNameType);
		}


		public void CheckIfFieldIsMember(KsidName field, ClassName objectType, LexicalToken errorToken) {
			if (objectType == null)
				ErrorLog.ThrowAndLogError(errorToken, ErrorCode.EFieldIsNotAMemberOf, field, "object");
			if (!objectType.UniqueNames.ContainsKey(field) && !objectType.NonUniqueNames.Contains(field))
				ErrorLog.ThrowAndLogError(errorToken, ErrorCode.EFieldIsNotAMemberOf, field, objectType);
		}

	}
}
