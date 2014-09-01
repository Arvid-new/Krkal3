//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - K S N a m e s
///
///		Holds identifiers of used entities
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using System.Globalization;
using System.IO;

namespace Krkal.CodeGenerator
{
	
	
	internal class KSNames
	{
		int _idCounter;

		Dictionary<KsidName, String> _ksidNames = new Dictionary<KsidName, string>();
		Dictionary<KsidName, String> _staticVariables = new Dictionary<KsidName, string>();
		Dictionary<TwoKsidsKey, String> _memberVariables = new Dictionary<TwoKsidsKey, string>();
		Dictionary<TwoKsidsKey, String> _casts = new Dictionary<TwoKsidsKey, string>();
		Dictionary<KsidName, String> _directMethods = new Dictionary<KsidName, string>();



		public String GetKsidName(KsidName name) {
			String ret;
			if (_ksidNames.TryGetValue(name, out ret))
				return ret;
			ret = GenerateSimpleName("_KSID_", name);
			_ksidNames.Add(name, ret);
			return ret;
		}


		public String GetStaticVariable(KsidName name) {
			String ret;
			if (_staticVariables.TryGetValue(name, out ret))
				return ret;
			GetKsidName(name);
			ret = GenerateSimpleName("_KSG_", name);
			_staticVariables.Add(name, ret);
			return ret;
		}


		public String GetMemberVariable(KsidName obj, KsidName variable) {
			String ret;
			TwoKsidsKey key = new TwoKsidsKey(obj, variable);
			if (_memberVariables.TryGetValue(key, out ret))
				return ret;
			GetKsidName(obj);
			GetKsidName(variable);
			ret = GenerateComposedName("_KSV_", obj, variable);
			_memberVariables.Add(key, ret);
			return ret;
		}


		public String GetCast(KsidName castFrom, KsidName castTo) {
			String ret;
			TwoKsidsKey key = new TwoKsidsKey(castFrom, castTo);
			if (_casts.TryGetValue(key, out ret))
				return ret;
			GetKsidName(castFrom);
			GetKsidName(castTo);
			ret = GenerateComposedName("_KSC_", castFrom, castTo);
			_casts.Add(key, ret);
			return ret;
		}


		public String GetDirectMethod(KsidName name) {
			String ret;
			if (_directMethods.TryGetValue(name, out ret))
				return ret;
			GetKsidName(name);
			ret = GenerateSimpleName("_KSDM_", name);
			_directMethods.Add(name, ret);
			return ret;
		}



		private string GenerateSimpleName(String prefix, KsidName name) {
			_idCounter++;
			return String.Format(CultureInfo.InvariantCulture, "{0}{1}_{2}", prefix, _idCounter, name.Identifier.LastPart.Name);
		}


		private string GenerateComposedName(string prefix, KsidName name1, KsidName name2) {
			_idCounter++;
			return String.Format(CultureInfo.InvariantCulture, "{0}{1}_{2}_{3}", prefix, _idCounter, name1.Identifier.LastPart.Name, name2.Identifier.LastPart.Name);
		}



		internal void WriteGetKsParams(TextWriter cppFile) {
			cppFile.WriteLine("void CScript::GetKsParams(CQueryKsParameters *query) {");

			cppFile.WriteLine();
			foreach (KeyValuePair<KsidName, String> pair in _ksidNames) {
				cppFile.Write('\t');
				cppFile.Write(pair.Value);
				cppFile.Write(" = query->GetKsidName(\"");
				cppFile.Write(pair.Key.Identifier.ToKsidString());
				cppFile.WriteLine("\");");
			}

			cppFile.WriteLine();
			foreach (KeyValuePair<KsidName, String> pair in _staticVariables) {
				cppFile.Write("\t*(void**)&");
				cppFile.Write(pair.Value);
				cppFile.Write(" = query->GetStaticVariablePtr(");
				cppFile.Write(_ksidNames[pair.Key]);
				cppFile.WriteLine(");");
			}

			cppFile.WriteLine();
			foreach (KeyValuePair<TwoKsidsKey, String> pair in _memberVariables) {
				cppFile.Write('\t');
				cppFile.Write(pair.Value);
				cppFile.Write(" = query->GetVariableOffset(");
				cppFile.Write(_ksidNames[pair.Key.Name1]);
				cppFile.Write(", ");
				cppFile.Write(_ksidNames[pair.Key.Name2]);
				cppFile.WriteLine(");");
			}

			cppFile.WriteLine();
			foreach (KeyValuePair<TwoKsidsKey, String> pair in _casts) {
				cppFile.Write('\t');
				cppFile.Write(pair.Value);
				cppFile.Write(" = query->GetCastOffset(");
				cppFile.Write(_ksidNames[pair.Key.Name1]);
				cppFile.Write(", ");
				cppFile.Write(_ksidNames[pair.Key.Name2]);
				cppFile.WriteLine(");");
			}

			cppFile.WriteLine();
			foreach (KeyValuePair<KsidName, String> pair in _directMethods) {
				cppFile.Write("\t*(void (**)())&");
				cppFile.Write(pair.Value);
				cppFile.Write(" = query->GetDirectMethodPtr(");
				cppFile.Write(_ksidNames[pair.Key]);
				cppFile.WriteLine(");");
			}

			cppFile.WriteLine('}');
		}





		internal void WriteDeclarations(TextWriter hFile) {
			hFile.WriteLine();
			foreach (String name in _ksidNames.Values) {
				hFile.Write("\tNamePtr ");
				hFile.Write(name);
				hFile.WriteLine(";");
			}

			hFile.WriteLine();
			foreach (KeyValuePair<KsidName, String> pair in _staticVariables) {
				hFile.Write('\t');
				hFile.Write(Utils.ParseType((pair.Key as TypedKsidName).LanguageType));
				hFile.Write(" * ");
				hFile.Write(pair.Value);
				hFile.WriteLine(";");
			}

			hFile.WriteLine();
			foreach (String name in _memberVariables.Values) {
				hFile.Write("\tint ");
				hFile.Write(name);
				hFile.WriteLine(";");
			}

			hFile.WriteLine();
			foreach (String name in _casts.Values) {
				hFile.Write("\tint ");
				hFile.Write(name);
				hFile.WriteLine(";");
			}

			hFile.WriteLine();
			foreach (KeyValuePair<KsidName, String> pair in _directMethods) {
				hFile.Write('\t');
				MethodName name = (MethodName)pair.Key;
				hFile.Write(Utils.ParseType(name.LanguageType));
				hFile.Write(" (*");
				hFile.Write(pair.Value);
				hFile.Write(')');
				hFile.Write(Utils.WriteDirectMethodParameterList(name.ParameterLists[0]));
				hFile.WriteLine(";");
			}
		
		}
	}






	internal class TwoKsidsKey : IEquatable<TwoKsidsKey>
	{
		KsidName _name1;
		public KsidName Name1 {
			get { return _name1; }
		}

		KsidName _name2;
		public KsidName Name2 {
			get { return _name2; }
		}

		// CONSTRUCTOR
		public TwoKsidsKey(KsidName name1, KsidName name2) {
			_name1 = name1;
			_name2 = name2;
		}

		#region equality

		public bool Equals(TwoKsidsKey other) {
			if (other == null)
				return false;
			return (_name1 == other._name1 && _name2 == other._name2);
		}

		public override int GetHashCode() {
			return _name1.GetHashCode() ^ _name2.GetHashCode();
		}

		public override bool Equals(object obj) {
			return Equals(obj as TwoKsidsKey);
		}

		#endregion
	}


}
