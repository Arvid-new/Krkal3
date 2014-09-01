using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;

namespace Krkal.Compiler
{


	public class TextResource
	{
		Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}

		String _laguage;
		public String Laguage {
			get { return _laguage; }
		}

		String _userName;
		public String UserName {
			get { return _userName; }
		}

		String _comment;
		public String Comment {
			get { return _comment; }
		}

		DateTime _time;
		public DateTime Time {
			get { return _time; }
		}
		[CLSCompliant(false)]
		public uint UIntTime {
			get { return (uint)_time.DayOfYear | ((uint)_time.Year << 9); }
		}

		internal TextResource(SyntaxTemplates syntax) {
			LexicalToken token;
			if ((token = syntax.TryReadToken(LexicalTokenType.Identifier)) == null || token.Identifier.Root == IdentifierRoot.Localized)
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EIdentifierExpected);
			_identifier = token.Identifier;

			if ((token = syntax.TryReadToken(LexicalTokenType.Identifier)) == null || !token.Identifier.IsSimple)
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "language tag");
			_laguage = token.Identifier.Simple;

			if ((token = syntax.TryReadToken(LexicalTokenType.Int)) == null)
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "date in form YYYY-MM-DD");
			int year = token.Int;

			if (!syntax.TryReadToken(OperatorType.Minus))
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "date in form YYYY-MM-DD");

			if ((token = syntax.TryReadToken(LexicalTokenType.Int)) == null)
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "date in form YYYY-MM-DD");
			int month = token.Int;

			if (!syntax.TryReadToken(OperatorType.Minus))
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "date in form YYYY-MM-DD");

			if ((token = syntax.TryReadToken(LexicalTokenType.Int)) == null)
				syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "date in form YYYY-MM-DD");
			int day = token.Int;

			_time = new DateTime(year, month, day);

			if (!syntax.TryReadToken(KeywordType.Null)) {
				if ((token = syntax.TryReadToken(LexicalTokenType.String)) == null)
					syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "string");
				_userName = token.Text;
			}

			if (!syntax.TryReadToken(KeywordType.Null)) {
				if ((token = syntax.TryReadToken(LexicalTokenType.String)) == null)
					syntax.ErrorLog.ThrowAndLogError(syntax.Lexical.CurrentToken, ErrorCode.EExpectedTokenNotFound, "string");
				_comment = token.Text;
			}

			syntax.DoSemiColon();

		}
	}









	public class TextResourceReader
	{

		ErrorLog _errorLog;
		public ErrorLog ErrorLog {
			get { return _errorLog; }
		}

		List<TextResource> _textResources = new List<TextResource>();
		public IEnumerable<TextResource> TextResources {
			get { return _textResources; }
		}

		public static TextResourceReader ParseFile(String file) {
			return new TextResourceReader(file);
		}



		// CONSTRUCOR
		TextResourceReader(String file) {
			_errorLog = new ErrorLog();
			try {
				Lexical lexical = new Lexical(file, FS.FileSystem.OpenFileForReading(file));
				SyntaxTemplates syntax = new SyntaxTemplates(_errorLog, lexical);

				for (LexicalToken token = lexical.Peek(0); token.Type != LexicalTokenType.Eof; token = lexical.Peek()) {
					if (!syntax.TryReadToken(OperatorType.SemiColon)) {
						try {
							_textResources.Add(new TextResource(syntax));
						}
						catch (CompilerException) {
							lexical.SkipPart(';', '\0', true);
						}
					}
				}
			}
			catch (FSFileNotFoundException) {
				_errorLog.LogError(file, ErrorCode.FFileNotFound, file);
			} 
		}
	}
}











namespace Krkal
{

	public interface IKrkalResourceManager
	{
		String GetUserNameOrComment(String ksid, bool isComment);
		String GetText(String text);
		String GetText(String ksid, String defaultText);
		bool ReloadIfNeeded();
	};

}