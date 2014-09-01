//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - D e c l a r a t i o n s
///
///		Clss Declarations holds declarations of names, dependencies and classes for a file
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	public class Declarations
	{
		private Lexical _lexical;
		public Lexical Lexical {
			get { return _lexical; }
		}

		private SyntaxTemplates _syntax;


		List<Field> _names = new List<Field>();
		public IList<Field> Names {
			get { return _names; }
		}

		List<Dependency> _dependencies = new List<Dependency>();
		public IList<Dependency> Dependencies {
			get { return _dependencies; }
		}

		List<Field> _classes = new List<Field>();
		public IList<Field> Classes {
			get { return _classes; }
		}

		List<Field> _enums = new List<Field>();
		public List<Field> Enums {
			get { return _enums; }
		}

		List<Field> _dataObjects = new List<Field>();
		public List<Field> DataObjects {
			get { return _dataObjects; }
		}


		Field _fileField;
		public Field FileField {
			get { return _fileField; }
		}



		// CONSTRUCTOR

		internal Declarations(Lexical lexical) {
			_lexical = lexical;
			_syntax = new SyntaxTemplates(lexical.ErrorLog, lexical);

			if (_lexical.Header.EndHeaderToken == null)
				return;

			_lexical.SeekAtToken(_lexical.Header.EndHeaderToken);

			LexicalToken token;
			_fileField = new Field();

			while ((token = _lexical.Peek()).Type != LexicalTokenType.Eof) {

				try {
					if (token == KeywordType.Depend && !_syntax.CheckCustomKeyword(KrkalCompiler.Compiler.CustomSyntax.CustomNameTypes)) {
						DoDependency();
					} else {
						Field field = new Field(_syntax, _fileField);
						field.DoFieldName(true);
					}
				}
				catch (ErrorException) {
					_lexical.SkipOutsidePart();
				}

			}

			foreach (Field field in _fileField.Children) {
				switch (field.FieldType) {
					case FieldType.Class:
						_classes.Add(field); break;
					case FieldType.Enum:
						_enums.Add(field); break;
					case FieldType.NewDataObject:
						_dataObjects.Add(field); break;
					case FieldType.Name:
						_names.Add(field); break;
				}
			}
		}



		private void DoDependency() {
			_lexical.Read();
			Dependency dependency = new Dependency(_syntax);
			dependency.DoRightPart();
			Dependencies.Add(dependency);
			while ((dependency = dependency.DoSibling()) != null) {
				Dependencies.Add(dependency);
			}
			_syntax.DoSemiColon();
		}
	}









	public class Dependency
	{
		private LexicalToken _depentdencyOperator;
		public LexicalToken DepentdencyOperator {
			get { return _depentdencyOperator; }
		}

		private List<WannaBeKsidName> _left;
		public ICollection<WannaBeKsidName> Left {
			get { return _left; }
		}

		private List<WannaBeKsidName> _right;
		public ICollection<WannaBeKsidName> Right {
			get { return _right; }
		}

		private SyntaxTemplates _syntax;

		// CONSTRUCTOR

		internal Dependency(Dependency sibling) {
			_left = sibling._right;
			_syntax = sibling._syntax;
		}

		internal Dependency(SyntaxTemplates syntax) {
			_syntax = syntax;
			DoPart();
		}



		private void DoPart() {
			if (_left == null) {
				_left = new List<WannaBeKsidName>();
			} else {
				_right = new List<WannaBeKsidName>();
			}
			if (_syntax.Lexical.Peek() == OperatorType.LeftCurlyBracket) {
				_syntax.DoBody(DoBodyParameters.DoNamesList, DoName, OperatorType.Comma, OperatorType.RightCurlyBracket);
			} else {
				DoName();
			}
		}


		private bool DoName() {
			LexicalToken token = _syntax.TryReadToken(LexicalTokenType.Identifier);
			if (token == null)
				_syntax.ErrorLog.ThrowAndLogError(_syntax.Lexical.CurrentToken, ErrorCode.EIdentifierExpected);
			WannaBeKsidName name = new WannaBeKsidName(token, null, _syntax.Lexical.Header);
			if (_right != null) {
				_right.Add(name);
			} else {
				_left.Add(name);
			}
			return true;
		}


		internal void DoRightPart() {
			LexicalToken token = _syntax.Lexical.Peek();
			if (token != OperatorType.RightShift && token != OperatorType.LeftShift)
				_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EShiftOperatorExpected);
			_depentdencyOperator = _syntax.Lexical.Read();
			DoPart();
		}



		internal Dependency DoSibling() {
			LexicalToken token = _syntax.Lexical.Peek();
			if (token != OperatorType.RightShift && token != OperatorType.LeftShift)
				return null;
			Dependency depentency = new Dependency(this);
			depentency.DoRightPart();
			return depentency;
		}
	
	}



}



