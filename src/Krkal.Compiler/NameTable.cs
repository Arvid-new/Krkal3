//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - N a m e T a b l e
///
///		Table of KSID names, functionality to search for a version
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{


	class NameTable
	{
		private Dictionary<NameTableKey, NameTableRecord> _names = new Dictionary<NameTableKey,NameTableRecord>();

		private Dictionary<Identifier, NameTableRecord> _identifierCache;
		private Dictionary<Identifier, NameTableRecord> IdentifierCache {
			get {
				if (_identifierCache == null)
					_identifierCache = new Dictionary<Identifier, NameTableRecord>();
				return _identifierCache; 
			}
		}

		private NameTableRecord Add(LexicalToken source, bool old, bool used, NameTableKey key, NameTableRecord unspecifiedParent) {
			NameTableRecord record = new NameTableRecord(key.Identifier, source, old, used, unspecifiedParent);
			_names.Add(key, record);
			return record;
		}

		// used when found new id in source, not in #names table
		private NameTableRecord Add(NameTableKey key, NameTableRecord unspecifiedParent) {
			return Add(null, false, true, key, unspecifiedParent);
		}

		// used when adding a version
		public NameTableRecord Add(NameTableKey key, String version, bool old) {
			Identifier identifier = new Identifier(key.Identifier, version);
			NameTableRecord record = new NameTableRecord(identifier, null, old, true, null);
			_names.Add(key, record);
			return record;
		}

		// used when constructing #names table
		public NameTableRecord Add(LexicalToken identifier, bool old) {
			NameTableKey key = new NameTableKey(identifier.Identifier);
			return Add(identifier, old, false, key, null);
		}


		// used when creating name table foe known names
		public void AddKnownName(KsidName knownName) {
			for (int f = 0; f < knownName.Identifier.PartsCount; f++) {
				if (knownName.Identifier[f].Version != null) {
					Identifier keyId = new Identifier(null, knownName.Identifier, f + 1);
					NameTableKey key = new NameTableKey(keyId);

					Add(null, false, true, key, null);
				}
			}
		}


		public NameTableRecord SearchOrAdd(Identifier identifier) {

			int pos = 0;
			NameTableRecord ret = null;
			NameTableRecord ret2;

			if (identifier.Root == IdentifierRoot.Kernel) {
				if (KrkalCompiler.Compiler.CustomSyntax.MixedNamespaces.Contains(identifier[0].Name)) {
					pos++; // mixed namespace
				} else {
					return null; // normal kernel name -> not in the name table
				}
			}


			while (true) {

				for (; pos < identifier.PartsCount && identifier[pos].Version != null; pos++) ;

				if (pos >= identifier.PartsCount)
					break;

				Identifier keyId = new Identifier(ret, identifier, pos + 1);
				NameTableKey key = new NameTableKey(keyId);

				ret2 = ret;
				if (!_names.TryGetValue(key, out ret)) {
					ret = Add(key, ret2);
				}
				ret.Used = true;

				pos++;
			}

			return ret;
		}



		internal bool TryGetName(NameTableKey key, out NameTableRecord ntRecord) {
			return _names.TryGetValue(key, out ntRecord);
		}

		internal bool TryGetCachedIdentifier(Identifier identifier, out NameTableRecord fullIdentifier) {
			return IdentifierCache.TryGetValue(identifier, out fullIdentifier);
		}

		internal void AddToIdentifierCache(Identifier identifier, NameTableRecord fullIdentifier) {
			IdentifierCache.Add(identifier, fullIdentifier);
		}

	}









	public class NameTableRecord
	{
		Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}

		LexicalToken _source;
		public LexicalToken Source {
			get { return _source; }
		}

		NameTableRecord _unspecifiedParent;
		public NameTableRecord UnspecifiedParent {
			get { return _unspecifiedParent; }
		}



		bool _old;
		public bool Old {
			get { return _old; }
		}

		bool _used;
		public bool Used {
			get { return _used; }
			set { _used = value; }
		}


		// CONSTRUCTOR

		internal NameTableRecord(Identifier identifier, LexicalToken source, bool old, bool used, NameTableRecord unspecifiedParent) {
			_identifier = identifier;
			_source = source;
			_old = old;
			_used = used;
			if (unspecifiedParent != null && unspecifiedParent._identifier.LastPart.Version == null) {
				_unspecifiedParent = unspecifiedParent;
			}
		}


		public override string ToString() {
			return _identifier.ToString();
		}
	}









	internal class NameTableKey : IEquatable<NameTableKey>
	{

		private Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}


		// CONSTRUCTOR

		public NameTableKey(Identifier identifier) {
			_identifier = identifier;
		}


		public override int GetHashCode() {
			return _identifier.GetHashCode();
		}

		public bool Equals(NameTableKey other) {
			if (other == null)
				return false;
			if (_identifier.PartsCount != other._identifier.PartsCount)
				return false;
			if (_identifier.Root != other._identifier.Root)
				return false;

			int f = 0;
			for (; f < _identifier.PartsCount - 1; f++) {
				if (_identifier[f].Name != other._identifier[f].Name)
					return false;
				if (_identifier[f].Version != other._identifier[f].Version)
					return false;
			}
			if (_identifier[f].Name != other._identifier[f].Name)
				return false;
			return true;
		}


		public override string ToString() {
			return _identifier.ToString();
		}
	}









	public class WannaBeKsidName
	{
		private NameTableRecord _ntRecord;
		public NameTableRecord NTRecord {
			get { return _ntRecord; }
		}

		private LexicalToken _sourceToken;
		public LexicalToken SourceToken {
			get { return _sourceToken; }
		}

		private Identifier _identifier;
		public Identifier Identifier {
			get { return _identifier; }
		}

		public bool IsFull {
			get { 
				return (_ntRecord == null || _ntRecord.Identifier.LastPart.Version != null); 
				// not full names have _ntRecord with not specified version of the last part
			}
		}

		// CONSTRUCTOR

		internal WannaBeKsidName(LexicalToken identifier, Identifier localizeTo, Header header) : this(identifier, identifier.Identifier, localizeTo, header) { }

		internal WannaBeKsidName(LexicalToken sourceToken,  Identifier identifier, Identifier localizeTo, Header header) {
			_sourceToken = sourceToken;

			if (identifier.Root == IdentifierRoot.Localized) {
				_identifier = new Identifier(localizeTo, identifier, (header == null || header.IsSystem) ? IdentifierRoot.Kernel : IdentifierRoot.User);
			} else {
				_identifier = identifier;
			}

			if (header != null) {
				_ntRecord = header.NameTable.SearchOrAdd(_identifier);
				if (_ntRecord != null) {
					if (_ntRecord.Identifier.PartsCount < _identifier.PartsCount) {
						_identifier = new Identifier(_ntRecord, _identifier, _identifier.PartsCount);
					} else {
						_identifier = _ntRecord.Identifier;
					}
				}
			}
		}



		public override string ToString() {
			return _identifier.ToString();
		}

	}



}
