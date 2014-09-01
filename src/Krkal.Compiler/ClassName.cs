//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C l a s s N a m e
///
///		Represents a class KSID name
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Krkal.Compiler
{
	public class ClassName : ClassOrEnumName
	{
		// CONSTRUCTOR
		//internal ClassName(Dag<KsidName> dag, Identifier identifier, NameType nameType) 
		//    : this(dag, identifier, nameType, null)
		//{	}
		internal ClassName(Dag<KsidName> dag, Identifier identifier, NameType nameType, LexicalToken declarationPlace)
			: base(dag, identifier, nameType, declarationPlace) 
		{
		}


		Dictionary<Field, MethodField> _methods = new Dictionary<Field, MethodField>();
		public ICollection<MethodField> Methods {
			get { return _methods.Values; }
		}

		Set<KsidName> _nonUniqueNames = new Set<KsidName>();
		public Set<KsidName> NonUniqueNames {
			get { return _nonUniqueNames; }
		}

		Dictionary<KsidName, UniqueField> _uniqueNames = new Dictionary<KsidName, UniqueField>();
		public IDictionary<KsidName, UniqueField> UniqueNames {
			get { return _uniqueNames; }
		}

		Dictionary<Field, StaticField> _staticFields = new Dictionary<Field, StaticField>();
		public IDictionary<Field, StaticField> StaticFields {
			get { return _staticFields; }
		}



		internal void ReadClassFields(Field parent, SourceFile sf, Identifier objectContext) {
			foreach (Field field in parent.Children) {
				try {
					KsidName name = Compilation.KsidNames.FindOrAdd(field.Name, field.GetNameType(Compilation, sf), sf);
					TypedKsidName typedName = name as TypedKsidName;
					if (typedName != null) {
						typedName.AssignType(field, sf);

						if ((typedName.LanguageType.Modifier & Modifier.Static) != 0) {
							if (!_staticFields.ContainsKey(field))
								_staticFields.Add(field, new StaticField(typedName, field, this, objectContext, sf));
							((ClassName)Compilation.CompilerKnownName(CompilerKnownName.Static)).AppendField(field, name, sf, objectContext);

							StaticVariableName staticVar = typedName as StaticVariableName;
							if (staticVar != null)
								staticVar.ParentClass = this;
							continue;
						}
					}

					AppendField(field, name, sf, objectContext);
				}
				catch (ErrorException) { }
			}
		}


		internal void AppendField(Field field, KsidName name, SourceFile sf, Identifier objectContext) {

			if (_uniqueNames.ContainsKey(name))
				Compilation.ErrorLog.ThrowAndLogError(field.Name.SourceToken, ErrorCode.EMultipleDeclaration);

			MethodName methodName = name as MethodName;
			if (methodName != null) {
				MethodField methodField = new MethodField(methodName, field, this, objectContext, Compilation, sf);
				_methods.Add(field, methodField);
				if ((field.LanguageType.Modifier & Modifier.Override) != 0) {
					if (_nonUniqueNames.Contains(name)) {
						_nonUniqueNames.Remove(name);
						Compilation.ErrorLog.LogError(field.Name.SourceToken, ErrorCode.EMultipleDeclaration);
					}
					_uniqueNames.Add(name, new UniqueField(name, field, this, objectContext, sf));
				} else if ((field.LanguageType.Modifier & Modifier.Direct) != 0) {
					_uniqueNames.Add(name, new UniqueField(name, field, this, objectContext, sf));
					if (!methodName.AllowOnlyOneDefinition(this))
						Compilation.ErrorLog.LogError(field.Name.SourceToken, ErrorCode.EOneDefinitionAllowed);
				} else {
					_nonUniqueNames.Add(name);
				}
			} else {
				if (name.NameType == NameType.Group) {
					_nonUniqueNames.Add(name);
				} else {
					_uniqueNames.Add(name, new UniqueField(name, field, this, objectContext, sf));
				}
			}


			if (name.NameType == NameType.Group)
				ReadClassFields(field, sf, objectContext);

		}






		internal void InheritNames() {

			foreach (ClassName directAncestor in this[Direction.Up].Layer(MatchType(NameType.Class))) {

				InheritMethods(directAncestor._methods.Values);
				InheritUniqueSet(directAncestor._uniqueNames.Values);
				InheritNonUniqueSet(directAncestor._nonUniqueNames);
				InheritStatics(directAncestor._staticFields.Values);

			}
		}



		private void InheritStatics(ICollection<StaticField> from) {
			foreach (StaticField stat in from) {
				if (!_staticFields.ContainsKey(stat.Field))
					_staticFields.Add(stat.Field, stat);
			}
		}



		private void InheritMethods(ICollection<MethodField> from) {
			foreach (MethodField method in from) {
				if (!_uniqueNames.ContainsKey(method.Name) && !_methods.ContainsKey(method.Field))
					_methods.Add(method.Field, method);
			}
		}


		private void InheritNonUniqueSet(Set<KsidName> from) {
			foreach (KsidName name in from) {
				UniqueField myUField;
				if (_uniqueNames.TryGetValue(name, out myUField)) {
					if (myUField.InheritedFrom != this)
						Compilation.ErrorLog.LogError(myUField.Field.Name.SourceToken, ErrorCode.EAmbiguousInheritance, Identifier, name.Identifier);
				} else {
					_nonUniqueNames.Add(name);
				}
			}
		}

		
		private void InheritUniqueSet(ICollection<UniqueField> from) 
		{
			foreach (UniqueField uField in from) {
				if (_nonUniqueNames.Contains(uField.Name)) {
					_nonUniqueNames.Remove(uField.Name);
					Compilation.ErrorLog.LogError(uField.Field.Name.SourceToken, ErrorCode.EAmbiguousInheritance, Identifier, uField.Name.Identifier);
				}
				UniqueField myUField;
				if (_uniqueNames.TryGetValue(uField.Name, out myUField)) {
					if (myUField.InheritedFrom != this && uField.InheritedFrom != myUField.InheritedFrom)
						Compilation.ErrorLog.LogError(uField.Field.Name.SourceToken, ErrorCode.EAmbiguousInheritance, Identifier, uField.Name.Identifier);
				} else {
					_uniqueNames.Add(uField.Name, uField);
				}
			}			
		}



		internal override void Reset(CompilationStep step) {
			base.Reset(step);
			if (step < CompilationStep.DoClasses) {
				_methods.Clear();
				_nonUniqueNames.Clear();
				_uniqueNames.Clear();
			}
			// Imho it's not necassary to clear inheritance step
		}



		//internal int GetObjectOrdinal(ClassField field) {
		//    ClassField field2;
		//    int ret = 1;
		//    if (_usedNames.TryGetValue(field.Name, out field2)) {
		//        ret = field2.ObjectOrdinal + 1;
		//    } 
		//    _usedNames[field.Name] = field;
		//    return ret;
		//}

	}





	public abstract class ClassField
	{
		private Field _field;
		public Field Field {
			get { return _field; }
		}

		private ClassName _inheritedFrom;
		public ClassName InheritedFrom
		{
			get { return _inheritedFrom; }
		}

		private Identifier _objectContext;
		public Identifier ObjectContext {
			get { return _objectContext; }
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public KsidName Name {
			get { return GetName(); }
		}

		private int _uniqueOrdinal;
		public int UniqueOrdinal {
			get { return _uniqueOrdinal; }
		}

		//private int _objectOrdinal;
		//public int ObjectOrdinal {
		//    get { return _objectOrdinal; }
		//}


		private SourceFile _sourceFile;
		public SourceFile SourceFile {
			get { return _sourceFile; }
		}

		public Compilation Compilation {
			get { return _inheritedFrom.Compilation; }
		}


		// CONSTRUCTOR

		internal ClassField(Field field, ClassName inheritedFrom, Identifier objectContext, SourceFile sf) {
			_field = field;
			_inheritedFrom = inheritedFrom;
			_uniqueOrdinal = inheritedFrom.Compilation.GetClassFieldCounter();
			_sourceFile = sf;
			_objectContext = objectContext;
		//	_objectOrdinal = inheritedFrom.GetObjectOrdinal(this);
		}

		public override string ToString() {
			return GetName().ToString();
		}

		public String ToKsfString() {
			return String.Format(CultureInfo.InvariantCulture, "_KSF_{0}_{1}", _uniqueOrdinal, GetName().Identifier.LastPart.Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		protected abstract KsidName GetName();
	}



	public class MethodField : ClassField
	{

		private MethodName _name;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public new MethodName Name {
			get { return _name; }
		}

		private ParameterList _parameterList;
		public ParameterList ParameterList {
			get { return _parameterList; }
		}


		// CONSTRUCTOR

		internal MethodField(MethodName name, Field field, ClassName inheritedFrom, Identifier objectContext, Compilation compilation, SourceFile sf) 
			: base(field, inheritedFrom, objectContext, sf)
		{
			_name = name;
			_parameterList = _name.AddParameterList(field, compilation, sf);
		}

		protected override KsidName GetName()	{
			return _name;
		}

	}



	public class UniqueField : ClassField
	{

		private KsidName _name;

		Expression _assignment;
		public Expression Assignment {
			get { return _assignment; }
		}


		// CONSTRUCTOR

		internal UniqueField(KsidName name, Field field, ClassName inheretedFrom, Identifier objectContext, SourceFile sf) 
			: base(field, inheretedFrom, objectContext, sf)
		{
			_name = name;

			StaticVariableName staticVar = _name as StaticVariableName;
			if (staticVar != null)
				staticVar.VariableField = this;
		}

		protected override KsidName GetName() {
			return _name;
		}

		internal void DoAssignment() {
			if (_assignment == null && CompilerConstants.IsNameTypeVariable(_name.NameType)) {
				StaticVariableName staticVar = _name as StaticVariableName;
				if (staticVar != null) {
					_assignment = staticVar.GetAssignment();
				} else {
					if (Field.AssignmentOrBody != null)
						_assignment = new Expression(this);
				}
			}
		}
	}



	public class StaticField : ClassField
	{
		private TypedKsidName _name;
		public new TypedKsidName Name {
			get { return _name; }
		}

		// CONSTRUCTOR

		internal StaticField(TypedKsidName name, Field field, ClassName inheretedFrom, Identifier objectContext, SourceFile sf) 
			: base(field, inheretedFrom, objectContext, sf)
		{
			_name = name;
		}

	
		protected override KsidName GetName() {
			return _name;
		}
	}


}
