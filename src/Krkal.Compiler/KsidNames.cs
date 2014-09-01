//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - K s i d N a m e s
///
///		dag of KSID names
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{


	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public class KsidNames : Dag<KsidName>
	{

		private Dictionary<Identifier, KsidName> _dictionary = new Dictionary<Identifier, KsidName>();

		public KsidName this[Identifier identifier] {
			get { return _dictionary[identifier]; }
		}


		public KsidName DeclareKnownName(KnownName name) {
			KsidName output;
			bool testLT = false;

			if (TryGetName(name.Identifier, out output)) {
				if (name.NameType != output.NameType)
					throw new ArgumentException("Declaring known name twice with different type.");
				testLT = true;
			} else {
				output = CreateName(name.Identifier, name.NameType);
			}

			TypedKsidName typedOutput = output as TypedKsidName;
			if (typedOutput != null) {
				LanguageType lt = name.LanguageType;
				if (name.LTName != null) {
					lt.ObjectType = (ClassOrEnumName)this[name.LTName.Identifier];
				}
				if (testLT) {
					lt.Modifier &= (Modifier)ModifierGroups.RelevantForTypedName;
					if (typedOutput.LanguageType != lt)
						throw new ArgumentException("Declaring known name twice with different language type.");
				} else {
					typedOutput.LanguageType = lt;
				}
			}

			if (name.Parent != null) {
				output.Parents.Add(this[name.Parent.Identifier]);
			}

			return output;
		}


		public KsidName CreateName(Identifier identifier, NameType nameType) {
			return CreateName(identifier, nameType, null);
		}

		public KsidName CreateName(Identifier identifier, NameType nameType, LexicalToken declarationPlace) {
			KsidName ret;
			switch (nameType) {
				case NameType.DirectMethod:
				case NameType.SafeMethod:
				case NameType.StaticDirectMethod:
				case NameType.StaticSafeMethod:
					ret = new MethodName(this, identifier, nameType, declarationPlace);
					break;
				case NameType.StaticVariable:
					ret = new StaticVariableName(this, identifier, nameType, declarationPlace);
					break;
				case NameType.Variable:
					ret = new TypedKsidName(this, identifier, nameType, declarationPlace);
					break;
				case NameType.Class:
					ret = new ClassName(this, identifier, nameType, declarationPlace);
					break;
				case NameType.Enum:
					ret = new EnumName(this, identifier, declarationPlace);
					break;
				default:
					ret = new KsidName(this, identifier, nameType, declarationPlace);
					break;
			}
			_dictionary.Add(ret.Identifier, ret);
			return ret;
		}


		public bool TryGetName(Identifier identifier, out KsidName name) {
			return _dictionary.TryGetValue(identifier, out name);
		}


		public void AssignOrdinals() {
			for (int f = 0; f < Count; f++) {
				this[f].Ordinal = f + 1;
			}
		}

	}



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase")]
	public class KsidNamesEx : KsidNames
	{

		private Compilation _compilation;
		public Compilation Compilation {
			get { return _compilation; }
			internal set { _compilation = value; }
		}

		// CONSTRUCTOR
		internal KsidNamesEx(Compilation compilation) {
			_compilation = compilation;
		}



		public KsidName FindOrAdd(WannaBeKsidName name, NameType nameType, SourceFile sourceFile) {
			return FindOrAdd(name, nameType, sourceFile, true, true);
		}
		public KsidName Find(WannaBeKsidName name, NameType nameType, SourceFile sourceFile) {
			return FindOrAdd(name, nameType, sourceFile, false, true);
		}
		public KsidName Find(WannaBeKsidName name, SourceFile sourceFile) {
			return FindOrAdd(name, NameType.Void, sourceFile, false, false);
		}

		public KsidName FindOrAddHidden(KsidName localizeTo, String name, NameType nameType, LexicalToken sourceToken) {
			return FindOrAddHidden(localizeTo, name, nameType, sourceToken, true, true);
		}



		private KsidName FindOrAdd(WannaBeKsidName name, NameType nameType, SourceFile sourceFile, bool declaration, bool checkType) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (sourceFile == null)
				throw new ArgumentNullException("sourceFile");

			Identifier id = CompleteName(name, sourceFile, declaration);

			return FindOrAdd2(id, name.SourceToken, nameType, declaration, checkType);
		}


		private KsidName FindOrAddHidden(KsidName localizeTo, string name, NameType nameType, LexicalToken sourceToken, bool declaration, bool checkType) {
			if (localizeTo == null)
				throw new ArgumentNullException("localizeTo");
			if (name == null)
				throw new ArgumentNullException("name");

			Identifier id = new Identifier(localizeTo.Identifier, new Identifier.Part(name, null));
			
			return FindOrAdd2(id, sourceToken, nameType, declaration, checkType);
		}


		private KsidName FindOrAdd2(Identifier id, LexicalToken sourceToken, NameType nameType, bool declaration, bool checkType) {

			KsidName ret;
			if (!TryGetName(id, out ret)) {
				if (!declaration) {
					_compilation.ErrorLog.ThrowAndLogError(sourceToken, ErrorCode.ENameNotDeclared, id);
				}
				ret = CreateName(id, nameType, sourceToken);
			}

			if (checkType && nameType != ret.NameType)
				_compilation.ErrorLog.ThrowAndLogError(sourceToken, ErrorCode.EDifferentNameType, nameType, ret.NameType);

			return ret;
		}



		public Identifier CompleteName(WannaBeKsidName name, SourceFile sourceFile, bool declaration) {
			if (name == null)
				throw new ArgumentNullException("name");
			if (sourceFile == null)
				throw new ArgumentNullException("sourceFile");

			if (name.IsFull)
				return name.Identifier;

			NameTableRecord ntRecord = CompleteName2(name.NTRecord, sourceFile, declaration, name.SourceToken);
			if (ntRecord.Identifier.PartsCount < name.Identifier.PartsCount) {
				return new Identifier(ntRecord, name.Identifier, name.Identifier.PartsCount);
			} else {
				return ntRecord.Identifier;
			}

		}

		private NameTableRecord CompleteName2(NameTableRecord ntRecord, SourceFile sourceFile, bool declaration, LexicalToken sourceToken) {

			NameTableRecord newNTRecord;

			// try complete from cache
			if (sourceFile.NewNames.TryGetCachedIdentifier(ntRecord.Identifier, out newNTRecord))
				return newNTRecord;

			// left part is ntRecord prefix. Complete prefix of the left part
			Identifier leftPart = ntRecord.Identifier;
			if (ntRecord.UnspecifiedParent != null) {
				leftPart = new Identifier(CompleteName2(ntRecord.UnspecifiedParent, sourceFile, declaration, sourceToken), leftPart, leftPart.PartsCount);
			}

			// add last version to the left part
			NameTableKey key = new NameTableKey(leftPart);

			if (!sourceFile.NewNames.TryGetName(key, out newNTRecord)) {
				List<NameTableRecord> records = SearchForVersion(key);
				bool old = false;

				if (records.Count > 1)
					_compilation.ErrorLog.ThrowAndLogError(sourceToken, ErrorCode.ENameVersionsConflict, BuildConflictList(records));
				if (!declaration && (records.Count == 0 || records[0].Used == false))
					_compilation.ErrorLog.ThrowAndLogError(sourceToken, ErrorCode.ENameNotDeclared, leftPart);
				if (records.Count == 1 && records[0].Used == false && records[0].Identifier.LastPart.Version != sourceFile.FileVersion) {
					_compilation.ErrorLog.LogError(sourceToken, ErrorCode.WNameVersionsPossibleConflict, BuildConflictList(records));
					old = true;
				}

				String version = records.Count == 1 ? records[0].Identifier.LastPart.Version : sourceFile.FileVersion;
				newNTRecord = sourceFile.NewNames.Add(key, version, old);
			}


			sourceFile.NewNames.AddToIdentifierCache(ntRecord.Identifier, newNTRecord);	// add to the cache

			return newNTRecord;
		}




		private static string BuildConflictList(List<NameTableRecord> records) {
			StringBuilder sb = new StringBuilder();
			for (int f = 0; f < records.Count; f++) {
				if (f > 0)
					sb.Append(", ");
				sb.Append(records[f].Identifier);
				if (records[f].Source != null && records[f].Source.Lexical.File != null) {
					sb.Append(" in ");
					sb.Append(records[f].Source.Lexical.File);
				}
			}
			return sb.ToString();
		}


		private List<NameTableRecord> SearchForVersion(NameTableKey key) {
			NameTableRecord ntRecord;
			String version;
			Set<String> foundVersions = new Set<string>();
			List<NameTableRecord> output = new List<NameTableRecord>();

			foreach (SourceFile file in _compilation.SourceFiles) {
				if (!file.NewNames.TryGetName(key, out ntRecord))
					file.Lexical.Header.NameTable.TryGetName(key, out ntRecord);
				if (ntRecord != null) {
					version = ntRecord.Identifier.LastPart.Version;
					if (version != null && !foundVersions.Contains(version)) {
						foundVersions.Add(version);
						output.Add(ntRecord);
					}
				}
			}

			if (_compilation.KnownNamesNameTable.TryGetName(key, out ntRecord)) {
				version = ntRecord.Identifier.LastPart.Version;
				if (version != null && !foundVersions.Contains(version)) {
					foundVersions.Add(version);
					output.Add(ntRecord);
				}
			}


			return output;
		}

	}
}
