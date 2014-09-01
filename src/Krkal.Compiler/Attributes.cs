using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{

	[Flags]
	public enum AttributeLocation
	{
		OnNames		= 0,
		OnFields	= 1,
		Global		= 2,
		Inherit		= 16,
		PositionMask= 15,
	}









	public class AttributeField
	{
		TypedKsidName _name;
		public TypedKsidName Name {
			get { return _name; }
		}

		ConstantValue _value;
		public ConstantValue Value {
			get { return _value; }
		}

		IAttribute _attributeDefinition;
		internal IAttribute AttributeDefinition {
			get { return _attributeDefinition; }
		}

		LexicalToken _token;
		public LexicalToken Token {
			get { return _token; }
		}

		// CONSTRUCTOR
		internal AttributeField(TypedKsidName name, IAttribute definition, LexicalToken token, SyntaxTemplatesEx syntax) {
			_name = name;
			_attributeDefinition = definition;
			_token = token;

			syntax.DoClosingBracket(OperatorType.Assign);

			Expression expression = new Expression(_name.LanguageType, syntax);
			_value = expression.RootNode.ConstantValue;
		}

		internal AttributeField(TypedKsidName name, ConstantValue value) {
			_name = name;
			_value = value;
		}


		internal bool Merge(AttributeField attr) {
			if (Name.LanguageType.DimensionsCount == 0 && Name.LanguageType.BasicType == BasicType.Int && (Name.LanguageType.Modifier & (Modifier)ModifierGroups.RetCalcullationGroup) == Modifier.RetOr) {
				_value = new IntConstantValue(((NumericConstantValue)_value).GetInt() | ((NumericConstantValue)attr._value).GetInt(), false);
				return true;
			} else {
				return ConstantValue.MemberwiseCompare(Name.LanguageType, _value, attr._value);
			}
		}
	}



















	class AttributeDefinitions
	{
		IAttributesCollection _attributesCollection;
		public IAttributesCollection AttributesCollection {
			get { return _attributesCollection; }
		}

		static Identifier _attributeNamespace = new Identifier(IdentifierRoot.Kernel, new Identifier.Part[] { new Identifier.Part("Attribute", null) });
		public static Identifier AttributeNamespace {
			get { return _attributeNamespace; }
		}

		Dictionary<TypedKsidName, IAttribute> _definitions = new Dictionary<TypedKsidName, IAttribute>();


		// CONSTRUCTOR
		public AttributeDefinitions(Compilation compilation) {
			LexicalToken token = compilation.SourceFiles.RootFile.Lexical.Header.AttributesDefinition;
			string ksid = null;
			if (token != null)
				ksid = token.ToKsidString();

			if (string.IsNullOrEmpty(ksid) || compilation.CustomSyntax.AttributeProvider == null) {
				_attributesCollection = EmptyAttributeCollection.EmptyCollection;
			} else {
				_attributesCollection = compilation.CustomSyntax.AttributeProvider.LoadAttributes(ksid, compilation.RebuildAll);
				if (_attributesCollection == null) {
					_attributesCollection = EmptyAttributeCollection.EmptyCollection;
					compilation.ErrorLog.LogError(token, ErrorCode.FUnableToLoadAttributeDefs);
				}
			}

			foreach (IAttribute attr in _attributesCollection) {
				KsidName name;
				compilation.KsidNames.TryGetName(Identifier.ParseKsid(attr.KsidName), out name);
				if (TestNameValidity(name)) {
					_definitions.Add((TypedKsidName)name, attr);
				} else {
					compilation.ErrorLog.LogError((LexicalToken)null, ErrorCode.EWrongAttributeName, name);
				}
			}
		}



		private bool TestNameValidity(KsidName name) {
			if (name == null || name.NameType != NameType.Variable)
				return false;
			TypedKsidName name2 = name as TypedKsidName;
			if (name2.LanguageType.BasicType == BasicType.Object)
				return false;
			if ((name2.LanguageType.Modifier & Modifier.Public) == 0)
				return false;
			for (int f = 0; f < name2.LanguageType.DimensionsCount; f++) {
				if (!name2.LanguageType.GetArrayConst(f))
					return false;
			}
			return true;
		}


		public IAttribute TryGetDefinition(TypedKsidName name) {
			IAttribute res;
			_definitions.TryGetValue(name, out res);
			return res;
		}

	}


















	class AttributeAnalysis
	{
		SyntaxTemplatesEx _syntax;
		AttributeDefinitions _definitions;
		Dictionary<KsidName, AttributeField> _toInherit = new Dictionary<KsidName,AttributeField>();
		Set<KsidName> _seenNames = new Set<KsidName>();
		List<AttributeField> _fieldAttributes;
		Field _currentField;


		// CONSTRUCTOR
		public AttributeAnalysis(Compilation compilation, Field field, LexicalToken attributeToken, SourceFile sf) {
			_syntax = new SyntaxTemplatesEx(compilation, sf, null, null, ExpressionContext.Attribute, false);
			_definitions = compilation.AttributeDefinitions;

			DoAttributes(field, attributeToken);
		}

		private void DoAttributes(Field field, LexicalToken attributeToken) {
			_currentField = field;

			try {
				if (attributeToken != null) {
					_syntax.Lexical.SeekAfterToken(attributeToken);
					_seenNames.Clear();
					_syntax.DoBody(DoBodyParameters.DoAttribute, DoAttribute, OperatorType.SemiColon, OperatorType.RightBracket);
				}

				foreach (AttributeField attr in _toInherit.Values) {
					AppendAttribute(attr);
				}
			}
			catch (ErrorException) { }

			if (field != null) {
				if (_fieldAttributes != null) {
					_syntax.Compilation.StoreFieldAttributes(field, _fieldAttributes);
					_fieldAttributes = null;
				}

				Dictionary<KsidName, AttributeField> toInherit2 = _toInherit;
				foreach (Field child in field.Children) {
					_toInherit = new Dictionary<KsidName, AttributeField>(toInherit2); // return to original _toInherit
					DoAttributes(child, child.Attributes);
				}
			}
		}



		private bool DoAttribute() {
			LexicalToken token = _syntax.Lexical.Peek();
			TypedKsidName name = _syntax.ReadKsid(AttributeDefinitions.AttributeNamespace) as TypedKsidName;
			if (name == null)
				_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnknownAttributeName);
			IAttribute def = _definitions.TryGetDefinition(name);
			if (def == null)
				_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EUnknownAttributeName);

			if (_seenNames.Contains(name)) {
				_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EMultipleDeclaration);
			} else {
				_seenNames.Add(name);
			}

			if (_currentField == null) {
				if ((def.Location & AttributeLocation.PositionMask) != AttributeLocation.Global)
					_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EInvalidAttributeUse);
			} else {
				if ((def.Location & AttributeLocation.PositionMask) == AttributeLocation.Global)
					_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EInvalidAttributeUse);
				if ((def.Location & AttributeLocation.Inherit) == 0 || _currentField.Children.Count == 0) {
					if (!TestFilter(def.Filter) || !TestLocation(def.Location))
						_syntax.ErrorLog.ThrowAndLogError(token, ErrorCode.EInvalidAttributeUse);
				}
			}

			AttributeField attr = new AttributeField(name, def, token, _syntax);

			if (_currentField == null) {
				MergeToDictionary(attr, _syntax.Compilation.GetOrCreateGlobalAttributesDictionary(), _syntax.ErrorLog);
			} else {
				if ((def.Location & AttributeLocation.Inherit) == 0) {
					AppendAttribute(attr);
				} else {
					// merge to _toInherit
					AttributeField attr2;
					_toInherit.TryGetValue(name, out attr2);
					if (attr2 != null) {
						attr.Merge(attr2); // merge in new value to not influence attribute inheritance in defferent fields
						_toInherit.Remove(name);
						_toInherit.Add(name, attr);
					} else {
						_toInherit.Add(name, attr);
					}
				}
			}

			return true;
		}




		private void AppendAttribute(AttributeField attr) {
			if (TestFilter(attr.AttributeDefinition.Filter)) {
				if ((attr.AttributeDefinition.Location & AttributeLocation.PositionMask) == AttributeLocation.OnFields) {
					if (TestLocation(attr.AttributeDefinition.Location)) {
						if (_fieldAttributes == null)
							_fieldAttributes = new List<AttributeField>();
						_fieldAttributes.Add(attr);
					}
				} else if (_currentField.Name != null) {
					KsidName name = _syntax.Compilation.KsidNames.Find(_currentField.Name, _syntax.SourceFile);
					MergeToDictionary(attr, name.GetOrCreateAttributesDictionary(), _syntax.ErrorLog);
				}
			}
		}



		internal static void MergeToDictionary(AttributeField attr, IDictionary<KsidName, AttributeField> attributes, ErrorLog log) {
			AttributeField attr2;
			attributes.TryGetValue(attr.Name, out attr2);
			if (attr2 != null) {
				if (!attr2.Merge(attr))
					log.LogError(attr.Token, ErrorCode.EAttributeValueConflict);
			} else {
				attributes.Add(attr.Name, attr);
			}
		}



		private bool TestFilter(IEnumerable<NameType> filter) {
			if (filter == null)
				return true;
			bool res = true;
			foreach (NameType nameType in filter) {
				res = false; break; // test if the enumeration is empty
			}

			if (res)
				return true;


			if (!_currentField.IsNameTypeValid)
				return false;
			NameType fieldNT = _currentField.GetNameType(_syntax.Compilation, _syntax.SourceFile);

			foreach (NameType nameType in filter) {
				if (fieldNT == nameType)
					return true;
			}

			return false;
		}


		private bool TestLocation(AttributeLocation attributeLocation) {
			if ((attributeLocation & AttributeLocation.PositionMask) == AttributeLocation.OnFields) {
				if (_currentField == null)
					return false;
				switch (_currentField.FieldType) {
					case FieldType.Control:
					case FieldType.Group:
					case FieldType.Method:
					case FieldType.ParameterDirect:
					case FieldType.ParameterSafe:
					case FieldType.Variable:
						return true;
					default:
						return false;						
				}
			} else {
				return true;
			}
		}
	}
















	class EmptyAttributeCollection : IAttributesCollection
	{
		public string Root {
			get { return String.Empty; }
		}

		public bool IsReloadNeeded() {
			return false;
		}


		List<IAttribute> _empty = new List<IAttribute>();
		public IEnumerator<IAttribute> GetEnumerator() {
			return _empty.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return _empty.GetEnumerator();
		}


		// CONSTRUCTOR
		private EmptyAttributeCollection() { }

		static EmptyAttributeCollection _emptyAttrCollection;
		internal static EmptyAttributeCollection EmptyCollection {
			get {
				if (_emptyAttrCollection == null)
					_emptyAttrCollection = new EmptyAttributeCollection();
				return _emptyAttrCollection; 
			}
		}

	}

}
