//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - Utils
///
///		Utils
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.CodeGenerator
{
	internal static class Utils
	{
		public static String ParseType(LanguageType type) {
			if (type.DimensionsCount > 0) {
				int dimCount = type.DimensionsCount;
				type.DimensionsCount = 0;
				if (dimCount == 1) {
					return String.Format(CultureInfo.InvariantCulture, "ArrPtr<{0}>", ParseType(type));
				} else {
					return String.Format(CultureInfo.InvariantCulture, "ArrPtr<{0},{1}>", ParseType(type), dimCount);
				}
			} else {
				switch (type.BasicType) {
					case BasicType.Double:
						return "double";
					case BasicType.Char:
						return "wchar_t";
					case BasicType.Int:
						return "int";
					case BasicType.Name:
						return "NamePtr";
					case BasicType.Object:
						return "OPointer";
					case BasicType.Void:
						return "void";
					default:
						throw new InternalCompilerException();
				}
			}
		}



		public static String WriteDirectMethodParameterList(ParameterList parameters) {
			StringBuilder sb = new StringBuilder();
			sb.Append("(CKerMain *KerMain, OPointer thisO");

			foreach (ParameterList.Param prm in parameters) {
				sb.Append(", ");
				sb.Append(ParseType(prm.Type));
				sb.Append(' ');
				if ((prm.Type.Modifier & Modifier.Ret) != 0)
					sb.Append('&');
				sb.Append("_KSL_");
				sb.Append(prm.Identifier.LastPart.Name);
			}

			sb.Append(')');
			return sb.ToString();
		}
	}
}
