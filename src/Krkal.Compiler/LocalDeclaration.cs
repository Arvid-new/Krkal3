//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - L o c a l D e c l a r a t i o n
///
///		Local declaration in a method body
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	public class LocalDeclaration : Statement
	{

		LanguageType _languageType;
		public LanguageType LanguageType {
			get { return _languageType; }
		}

		String _name;
		public String Name {
			get { return _name; }
		}

		LexicalToken _nameToken;
		public LexicalToken NameToken {
			get { return _nameToken; }
		}

		Expression _initialization;
		public Expression Initialization {
			get { return _initialization; }
		}

		// CONSTRUCTOR
		private LocalDeclaration(Statement parentBlock)
			: base(parentBlock) 
		{
			LexicalToken token = Syntax.Lexical.Read();

			while (token.Type == LexicalTokenType.Keyword && token.Keyword.Modifier != Modifier.None) {
				if ((token.Keyword.Modifier & (Modifier)ModifierGroups.AllowedForLocalVariable) != 0) {
					if ((_languageType.Modifier & token.Keyword.Modifier) != 0)
						Syntax.ErrorLog.LogError(token, ErrorCode.EModifierAlreadyUsed);
					_languageType.Modifier |= token.Keyword.Modifier;
				} else {
					Syntax.ErrorLog.LogError(token, ErrorCode.EModifierNotAllowed, token.Keyword.Modifier);
				}
				token = Syntax.Lexical.Read();
			}

			if (token.Type == LexicalTokenType.Keyword) {
				if (token == KeywordType.String) {
					_languageType.BasicType = BasicType.Char;
					_languageType.DimensionsCount = 1;
				} else {
					if (token.Keyword.BasicType == BasicType.Unasigned)
						Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EExpectedTypeInDeclaration);
					if (token.Keyword.BasicType == BasicType.Void)
						Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EVoidVariable);
					_languageType.BasicType = token.Keyword.BasicType;
				}
			} else {
				if (token.Type != LexicalTokenType.Identifier)
					Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EExpectedTypeInDeclaration);
				if (!_languageType.AssignObjectType(Syntax.IdToKsid(token, null)))
					Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EClassOrEnumNameExpected);
			}

			_languageType.ReadArray(Syntax);

			_languageType.CheckConstIntegrity(Syntax.ErrorLog, Syntax.Lexical.CurrentToken);
		}



		public override StatementType GetStatementType() {
			return StatementType.Declaration;
		}


		internal static LocalDeclaration Create(Statement parentBlock, bool allowSiblings) {
			LocalDeclaration decl = new LocalDeclaration(parentBlock);
			decl.LastToken = decl.Syntax.Lexical.CurrentToken;
			decl.DoName(allowSiblings);
			while (allowSiblings && decl.Syntax.TryReadToken(OperatorType.Comma)) {
				CodeBlock codeBlock = parentBlock as CodeBlock;
				if (codeBlock != null) {
					codeBlock.AddStatement(decl);
				}
				decl = (LocalDeclaration)decl.MemberwiseClone();
				decl.DoName(allowSiblings);
			}
			if (allowSiblings)
				decl.Syntax.DoSemiColon();
			return decl;
		}




		private void DoName(bool allowInitialization) {
			LexicalToken token = Syntax.TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				Syntax.ErrorLog.ThrowAndLogError(Syntax.Lexical.CurrentToken, ErrorCode.EIdentifierExpected);

			if (!token.Identifier.IsSimple)
				Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EComposedLocalId);
			_nameToken = token;
			_name = token.Identifier.Simple;

			ParentBlock.DeclareVariable(this);

			if (Syntax.TryReadToken(OperatorType.Assign)) {
				if (!allowInitialization)
					Syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EInitializationNotAllowed);
				_initialization = new Expression(this, ExpressionType.Initialization, _languageType);
			} else {
				_initialization = null;
			}
		}



	}

}