//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - I d e n t i f i e r
///
///		Represents sructured identifier with versions
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;


namespace Krkal.Compiler
{


	public enum IdentifierRoot
	{
		Localized,
		Kernel,
		User,
	}



	public class Identifier : IEquatable<Identifier>
	{
		private IList<Part> _parts;

		public Part this[int index] {
			get { return _parts[index]; }
		}

		public Part LastPart {
			get { return _parts[_parts.Count - 1]; }
		}

		public int PartsCount {
			get { return _parts.Count; }
		}

		private IdentifierRoot _root;
		public IdentifierRoot Root {
			get { return _root; }
		}

		public bool IsSimple {
			get { return (_root == IdentifierRoot.Localized && PartsCount == 1 && _parts[0].Version == null); }
		}

		public bool IsFull {
			get {
				if (_root == IdentifierRoot.Localized)
					return false;
				foreach (Part part in _parts) {
					if (part.Version == null)
						return false;
				}
				return true;
			}
		}

		public String Simple {
			get { return _parts[0].Name; }
		}




		// CONSTRUCTOR

		internal Identifier(IdentifierRoot root, IList<Part> parts) {
			_root = root;
			_parts = parts;
		}


		// creates key for #name table
		internal Identifier(NameTableRecord part1, Identifier part2, int length) {
			if (part2.Root == IdentifierRoot.Localized)
				throw new InternalCompilerException("only full names can be added to neme table");
			_root = part2.Root;
			_parts = new List<Part>(length);

			int f = 0;
			if (part1 != null) {
				if (part2.Root != part1.Identifier.Root)
					throw new InternalCompilerException("name roots differ");
				for (; f < part1.Identifier.PartsCount; f++) {
					_parts.Add(part1.Identifier[f]);
				}
			}
			for (; f < length; f++) {
				_parts.Add(part2[f]);
			}
		}

		// joins names
		internal Identifier(Identifier part1, Identifier part2, IdentifierRoot defaultRoot) {
			if (part2.Root != IdentifierRoot.Localized)
				throw new InternalCompilerException("only not full names can be localized");
			if (part1 == null) {
				_root = defaultRoot;
				_parts = part2._parts;
			} else {
				_root = part1._root;
				List<Part> parts = new List<Part>(part1.PartsCount + part2.PartsCount);
				parts.AddRange(part1._parts);
				parts.AddRange(part2._parts);
				_parts = parts;
			}
		}

		// adds a version
		internal Identifier(Identifier identifier, String version) {
			_root = identifier.Root;
			_parts = new List<Part>(identifier._parts);
			_parts[_parts.Count - 1] = new Part(LastPart.Name, version);
		}

		// adds one level
		internal Identifier(Identifier identifier, Part part) {
			_root = identifier.Root;
			_parts = new List<Part>(identifier._parts);
			_parts.Add(part);
		}


		// parses id from string
		public static Identifier Parse(String identifier) {
			if (identifier == null)
				throw new ArgumentNullException("identifier");
			Lexical lexical = new Lexical(null, identifier);
			LexicalToken token = lexical.Read();
			if (token.Type != LexicalTokenType.Identifier)
				throw new FormatException("invalid identifier format");
			return token.Identifier;
		}


		// parses id from ksid string
		public static Identifier ParseKsid(String identifier) {
			if (identifier == null)
				throw new ArgumentNullException("identifier");

			IdentifierRoot root = IdentifierRoot.Localized;
			if (identifier.StartsWith("_KSID_", StringComparison.Ordinal)) {
				root = IdentifierRoot.Kernel;
				identifier = identifier.Substring(6);
			} else if (identifier.StartsWith("_KSId_", StringComparison.Ordinal)) {
				root = IdentifierRoot.User;
				identifier = identifier.Substring(6);
			}

			string[] parts = identifier.Split(new String[] { "__M_" }, StringSplitOptions.None);
			if (parts == null || parts.Length == 0)
				throw new FormatException("invalid identifier format");

			List<Part> idParts = new List<Part>(parts.Length);

			foreach (string s in parts) {
				if (String.IsNullOrEmpty(s))
					throw new FormatException("invalid identifier format");

				if (CompilerConstants.EndsWithVersion(s)) {
					idParts.Add(new Part(s.Substring(0, s.Length - 20), s.Substring(s.Length - 19)));
				} else {
					idParts.Add(new Part(s, null));
				}
			}

			return new Identifier(root, idParts);
		}



		public override string ToString() {
			return ToString(false);
		}


		public string ToString(bool withoutVersions) {
			StringBuilder sb = new StringBuilder();

			if (_root == IdentifierRoot.User) {
				sb.Append('$');
			} else if (_root == IdentifierRoot.Kernel) {
				sb.Append('@');
			}

			bool first = true;
			foreach (Part part in _parts) {
				if (!first) {
					sb.Append('.');
				} else {
					first = false;
				}
				sb.Append(part.Name);
				if (!withoutVersions && part.Version != null) {
					sb.Append('$');
					sb.Append(part.Version);
				}
			}

			return sb.ToString();
		}



		public string ToKsidString() {
			StringBuilder sb = new StringBuilder();

			if (_root == IdentifierRoot.Kernel) {
				sb.Append("_KSID_");
			} else if (_root == IdentifierRoot.User) {
				sb.Append("_KSId_");
			}

			bool first = true;
			foreach (Part part in _parts) {
				if (!first) {
					sb.Append("__M_");
				} else {
					first = false;
				}
				sb.Append(part.Name);
				if (part.Version != null) {
					sb.Append('_');
					sb.Append(part.Version);
				}
			}

			return sb.ToString();
		}




		#region equality operators

		public override int GetHashCode() {
			// I will not calculate hash from versions
			if (_parts.Count > 1) {
				return _parts[_parts.Count - 1].Name.GetHashCode() ^ _parts[_parts.Count - 2].Name.GetHashCode();
			} else {
				return _parts[_parts.Count - 1].Name.GetHashCode();
			}
		}


		public override bool Equals(object obj) {
			return Equals(obj as Identifier);
		}

		public bool Equals(Identifier other) {
			if (other == null)
				return false;
			if (PartsCount != other.PartsCount)
				return false;
			if (_root != other._root)
				return false;

			for (int f = 0; f < PartsCount; f++) {
				if (_parts[f].Name != other._parts[f].Name)
					return false;
				if (_parts[f].Version != other._parts[f].Version)
					return false;
			}
			return true;
		}


		public static bool operator == (Identifier id1, Identifier id2) {
			if ((object)id1 != null) {
				return id1.Equals(id2);
			} else {
				return ((object)id2 == null);
			}
		}

		public static bool operator != (Identifier id1, Identifier id2) {
			return !(id1 == id2);
		}


		#endregion





		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
		public class Part
		{
			private string _name;
			public string Name {
				get { return _name; }
			}

			private string _version;
			public string Version {
				get { return _version; }
			}

			internal Part(String name, String version) {
				_name = name;
				_version = version;
			}
		}


	}
}
