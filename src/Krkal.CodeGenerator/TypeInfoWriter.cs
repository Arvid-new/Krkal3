//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - T y p e I n f o W r i t e r
///
///		Outputs type information to a .code file
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.FileSystem;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.CodeGenerator
{


	class TypeInfoWriter
	{
		FSRegister _code;

		Compilation _compilation;
		Generator _generator;

		int _arrayCounter;

		FSRegKey _arrayLTs;
		public FSRegKey ArrayLTs {
			get {
				if (_arrayLTs.IsNull)
					_arrayLTs = _code.AddKey("Arrays LTs", FSRegKeyType.Int);
				return _arrayLTs; 
			}
		}

		FSRegister _arrayRegister;
		public FSRegister ArrayRegister {
			get {
				if (_arrayRegister.IsNull)
					_arrayRegister = _code.AddKey("Arrays", FSRegKeyType.Register).Subregister;
				return _arrayRegister; 
			}
		}


		Set<String> _usedFieldCache = new Set<string>();


		// CONSTRUCTOR
		internal TypeInfoWriter(FSRegister code, Compilation compilation, Generator generator) {
			_code = code;
			_compilation = compilation;
			_generator = generator;
		}



		public void WriteKsidNames() {
			FSRegister names = _code.AddKey("Names", FSRegKeyType.Register).Subregister;

			_compilation.KsidNames.AssignOrdinals();

			foreach (KsidName ksid in _compilation.KsidNames) {
				FSRegister name = names.AddKey(ksid.Identifier.ToKsidString(), FSRegKeyType.Register).Subregister;

				name.AddKey("Type", FSRegKeyType.Int).WriteI((int)ksid.NameType);

				WriteTypedName(name, ksid as TypedKsidName);
				WriteClassName(name, ksid as ClassName);
				WriteEnum(name, ksid as EnumName);
				WriteAttributes(name, ksid.Attributes);
			}
		}


		private void WriteEnum(FSRegister reg, EnumName enumName) {
			if (enumName == null)
				return;

			WriteKsid(reg, _generator.GetKnownName(GeneratorKnownName.Enum), "Class");
			FSRegister data = reg.AddKey("Data", FSRegKeyType.Register).Subregister;

			String key = _generator.GetKnownName(GeneratorKnownName.Enum_Class).Ordinal.ToString(CultureInfo.InvariantCulture);
			WriteKsid(data, enumName.MyClass, key);
			key = _generator.GetKnownName(GeneratorKnownName.Enum_Name).Ordinal.ToString(CultureInfo.InvariantCulture);
			WriteKsid(data, enumName.MyName, key);
			key = _generator.GetKnownName(GeneratorKnownName.Enum_Type).Ordinal.ToString(CultureInfo.InvariantCulture);
			data.AddKey(key, FSRegKeyType.Int).WriteI((int)enumName.Type);
		}


		private void WriteVariable(FSRegister data, TypedKsidName var, ConstantValue value) {
			String keyName = var.Ordinal.ToString(CultureInfo.InvariantCulture);
			FSRegKey key = CreateVariableKey(data, keyName, var.LanguageType);
			WriteVariable2(key, var.LanguageType, value);
		}

		private static FSRegKey CreateVariableKey(FSRegister data, string keyName, LanguageType languageType) {
			if (languageType.DimensionsCount > 0) {
				return data.AddKey(keyName, FSRegKeyType.Int);
			} else {
				switch (languageType.BasicType) {
					case BasicType.Double:
						return data.AddKey(keyName, FSRegKeyType.Double);
					case BasicType.Char:
						return data.AddKey(keyName, FSRegKeyType.WChar);
					default:
						return data.AddKey(keyName, FSRegKeyType.Int);
				}
			}
		}



		private void WriteVariable2(FSRegKey key, LanguageType lt, ConstantValue value) {
			if (lt.DimensionsCount > 0) {
				ArrayConstantValue av = value as ArrayConstantValue;
				if (av != null) {
					key.WriteI(_arrayCounter);
					_arrayCounter++;
					WriteArray(lt, av);
				} else {
					key.WriteI(0);
				}
			} else {
				switch (lt.BasicType) {
					case BasicType.Double:
						key.WriteD(((NumericConstantValue)value).GetDouble());
						break;
					case BasicType.Char:
						key.WriteW(((NumericConstantValue)value).GetChar());
						break;
					case BasicType.Int:
						key.WriteI(((NumericConstantValue)value).GetInt());
						break;
					case BasicType.Name: 
						{
							NameConstantValue nc = value as NameConstantValue;
							if (nc != null && nc.Name != null) {
								key.WriteI(nc.Name.Ordinal);
							} else {
								key.WriteI(0);
							}
						}
						break;
					case BasicType.Object:
						key.WriteI(0);
						break;
					default:
						throw new InternalCompilerException();
				}
			}			
		}



		private void WriteArray(LanguageType lt, ArrayConstantValue value) {
			WriteLanguageType(ArrayLTs, lt);
			lt.DecreaseDimCount();
			FSRegKey key = CreateVariableKey(ArrayRegister, "A", lt);
			foreach (ConstantValue v in value.Array) {
				WriteVariable2(key, lt, v);
			}
		}



		private static void WriteTypedName(FSRegister name, TypedKsidName typedKsidName) {
			if (typedKsidName == null)
				return;
			WriteLanguageType(name, typedKsidName.LanguageType);
		}

		private static void WriteLanguageType(FSRegister reg, LanguageType languageType) {
			FSRegKey key = reg.AddKey("LT", FSRegKeyType.Int);
			WriteLanguageType(key, languageType);
		}

		private static void WriteLanguageType(FSRegKey key, LanguageType languageType) {
			uint a = ((uint)languageType.DimensionsCount << 24) | ((uint)languageType.BasicType << 16) | (((uint)languageType.Modifier) & 0xFFFF);
			key.WriteI((int)a);
			if (languageType.ObjectType != null) {
				key.WriteI(languageType.ObjectType.Ordinal);
			} else {
				key.WriteI(0);
			}
		}




		private void WriteClassName(FSRegister name, ClassName className) {
			if (className == null)
				return;

			WriteVariables(new LazyAddSubRegister(name, "Variables"), className.UniqueNames);
			WriteMethods(new LazyAddSubRegister(name, "Methods"), className.Methods);
			WriteControls(new LazyAddSubRegister(name, "Controls"), className.UniqueNames);
			WriteStatics(new LazyAddSubRegister(name, "Statics"), className.StaticFields.Values);
		}



		private void WriteStatics(LazyAddSubRegister statics, ICollection<StaticField> staticFields) {
			foreach (StaticField field in staticFields) {
				WriteClassField(statics.GetR(), field);
			}
		}

		private void WriteMethods(LazyAddSubRegister methods, ICollection<MethodField> methodFields) {
			foreach (MethodField field in methodFields) {
				FSRegister method = WriteClassField(methods.GetR(), field);
				if (!method.IsNull) {
					if ((field.Name.LanguageType.Modifier & Modifier.Direct) == 0) {
						method.AddKey("Safe", FSRegKeyType.Char).WriteC(1);
					} else {
						method.AddKey("Safe", FSRegKeyType.Char).WriteC(0);
					}
					WriteParams(new LazyAddSubRegister(method, "Params"), field.ParameterList);
				}
			}			
		}


		private FSRegister WriteClassField(FSRegister reg, ClassField field) {
			String ksf = field.ToKsfString();
			FSRegister reg2 = reg.AddKey(ksf, FSRegKeyType.Register).Subregister;
			if (_usedFieldCache.Contains(ksf)) // I saved this field previously so I wont save it again
				return new FSRegister();
			_usedFieldCache.Add(ksf);
			WriteKsid(reg2, field.Name);
			WriteInheritedFrom(reg2, field.InheritedFrom);
			WriteAttributes(reg2, _compilation.TryGetFieldAttributes(field.Field));
			return reg2;
		}


		private void WriteParams(LazyAddSubRegister reg, ParameterList parameterList) {
			foreach (ParameterList.Param prm in parameterList) {
				FSRegister prmReg = reg.GetR().AddKey(prm.Identifier.LastPart.Name, FSRegKeyType.Register).Subregister;
				WriteLanguageType(prmReg, prm.Type);
				if (prm.ParamName != null)
					WriteKsid(prmReg, prm.ParamName);
				WriteAttributes(prmReg, _compilation.TryGetFieldAttributes(prm.Field));
				if (prm.DefaultValue != null) {
					FSRegKey def = CreateVariableKey(prmReg, "Default", prm.Type);
					WriteVariable2(def, prm.Type, prm.DefaultValue);
				}
			}
		}

		private static void WriteKsid(FSRegister reg, KsidName ksid) {
			WriteKsid(reg, ksid, "Ksid");
		}

		private static void WriteKsid(FSRegister reg, KsidName ksid, string keyName) {
			FSRegKey key = reg.AddKey(keyName, FSRegKeyType.Int);
			if (ksid != null) {
				key.WriteI(ksid.Ordinal);
			} else {
				key.WriteI(0);
			}
		}

		private static void WriteInheritedFrom(FSRegister reg, ClassName className) {
			reg.AddKey("From", FSRegKeyType.Int).WriteI(className.Ordinal);
		}



		private void WriteVariables(LazyAddSubRegister variables, IDictionary<KsidName, UniqueField> uniqueNames) {
			foreach (UniqueField field in uniqueNames.Values) {
				if (CompilerConstants.IsNameTypeVariable(field.Name.NameType)) {
					WriteClassField(variables.GetR(), field);
				}
			}
		}


		private void WriteControls(LazyAddSubRegister controls, IDictionary<KsidName, UniqueField> uniqueNames) {
			foreach (UniqueField field in uniqueNames.Values) {
				if (field.Name.NameType == NameType.Control) {
					WriteClassField(controls.GetR(), field);
				}
			}
		}




		public void WriteDependencies() {
			FSRegKey key = _code.AddKey("Dependencies", FSRegKeyType.Int);
			foreach (KsidName name in _compilation.KsidNames) {
				foreach (KsidName child in name.Children) {
					key.WriteI(name.Ordinal);
					key.WriteI(child.Ordinal);
				}
			}
		}



		public void WriteOrdinalStarts() {
			FSRegKey key = _code.AddKey("Ordinal Starts", FSRegKeyType.Int);
			key.WriteI(1);
			_arrayCounter = _compilation.KsidNames.Count + 1;
			key.WriteI(_arrayCounter);
			key.WriteI(_arrayCounter);
		}

		
		
		public void WriteStaticConstants() {
			FSRegister data = _code.AddKey("Static Constants", FSRegKeyType.Register).Subregister;

			foreach (UniqueField field in ((ClassName)_compilation.CompilerKnownName(CompilerKnownName.Static)).UniqueNames.Values) {
				StaticVariableName staticVar = field.Name as StaticVariableName;
				if (staticVar != null && staticVar.IsConstant && staticVar.GetConstantValue() != null) {
					WriteVariable(data, staticVar, staticVar.GetConstantValue());
				}
			}
		}



		private void WriteAttributes(FSRegister reg, ICollection<AttributeField> attributes) {
			if (attributes == null || attributes.Count == 0)
				return;

			FSRegister data = reg.AddKey("Attributes", FSRegKeyType.Register).Subregister;

			foreach (AttributeField attr in attributes) {
				WriteVariable(data, attr.Name, attr.Value);
			}
		}


		public void WriteGlobalAttributes() {
			WriteAttributes(_code, _compilation.GlobalAttributes);
		}



		public void WriteProjectFiles() {
			_code.AddKey("Source Project", FSRegKeyType.String).StringWrite(_compilation.SourceFiles.RootFile.File);
			FSRegKey fileName = _code.AddKey("Source Files", FSRegKeyType.String);
			FSRegKey fileDate = _code.AddKey("Source Files Dates", FSRegKeyType.Int);
			foreach (SourceFile sf in _compilation.SourceFiles) {
				System.Runtime.InteropServices.ComTypes.FILETIME fileTime = new System.Runtime.InteropServices.ComTypes.FILETIME();
				if (FS.FileSystem.GetfileTime(sf.File, ref fileTime) == 0)
					_compilation.ErrorLog.LogError("", ErrorCode.FFileNotFound, sf.File);
				String full = null;
				if (FS.FileSystem.GetFullPath(sf.File, ref full, FSFullPathType.InvariantKey) == 0) {
					_compilation.ErrorLog.LogError("", ErrorCode.FFileNotFound, sf.File);
				} else {
					fileName.StringWrite(full);
					fileDate.WriteI(fileTime.dwLowDateTime);
					fileDate.WriteI(fileTime.dwHighDateTime);
				}
			}
		}
	}




	class LazyAddSubRegister
	{
		public LazyAddSubRegister(FSRegister parent, String name) {
			_parent = parent;
			_name = name;
		}

		public FSRegister GetR() {
			if (_register.IsNull) {
				_register = _parent.AddKey(_name, FSRegKeyType.Register).Subregister;
			}
			return _register;
		}

		FSRegister _parent;
		String _name;
		FSRegister _register;
	}

}
