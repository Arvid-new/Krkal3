#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Krkal::Compiler;

#include "CStringToCharPtr.h"
#include "CKerMain.h"
#include "Names.Net.h"
#include "KernelArray.h"



namespace Krkal {
	namespace Runtime {


		ref class KerArrBase;
		ref class KerMain;



		public value class KerOVar  {
		private:
			CKerOVar *_var;
		internal:

			KerOVar(CKerOVar *var) {
				_var = var;
			}

			property int Offset {
				int get() { return _var->Offset; }
			}

		public:


			property LanguageType LangType {
				LanguageType get() { return LanguageType((BasicType)_var->LT.Type, Modifier::None, _var->LT.DimCount, nullptr); } 
			}

			property KerName Name {
				KerName get() { return KerName(_var->Name); }
			}




			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _var == 0;
				} else if (obj->GetType() == KerOVar::typeid) {
					return _var == ((KerOVar)obj)._var;
				} else {
					return false;
				}
			}


			static bool operator == (KerOVar var1, KerOVar var2) {
				return (var1._var == var2._var);
			}

			static bool operator != (KerOVar var1, KerOVar var2) {
				return (var1._var != var2._var);
			}

			property bool IsNull {
				bool get() { return (_var == 0);}
			}

		};








		public value class KerObjectT  {
		private:
			CKerObjectT *_obj;
			List<KerOVar> ^_variables;
		internal:

			KerObjectT(CKerObjectT *obj) {
				_obj = obj;
			}

		public:


			property KerName Name {
				KerName get() { return KerName(_obj->Name); }
			}


			property IList<KerOVar> ^Variables {
				IList<KerOVar> ^get() {
					if (_variables == nullptr) {
						_variables = gcnew List<KerOVar>(_obj->NumVars);
						for (int f=0; f < _obj->NumVars; f++) {
							_variables->Add(KerOVar(_obj->OVars + f));
						}
					}
					return _variables;
				}
			}


			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _obj == 0;
				} else if (obj->GetType() == KerObjectT::typeid) {
					return _obj == ((KerObjectT)obj)._obj;
				} else {
					return false;
				}
			}


			static bool operator == (KerObjectT obj1, KerObjectT obj2) {
				return (obj1._obj == obj2._obj);
			}

			static bool operator != (KerObjectT obj1, KerObjectT obj2) {
				return (obj1._obj != obj2._obj);
			}

			property bool IsNull {
				bool get() { return (_obj == 0);}
			}

		};






		public value class KerObject  {
		private:
			CKerObject *_obj;
			void **_entryPoint;
		internal:

			KerObject(OPointer obj) {
				_entryPoint = obj;
				if (obj == 0) {
					_obj = 0;
				} else {
					_obj = obj.KerObject();
				}
			}

			operator OPointer() { return _entryPoint;}


		public:

			KerObject(IntPtr opointer) {
				OPointer obj((void**)opointer.ToPointer()) ;
				_entryPoint = obj;
				if (obj == 0) {
					_obj = 0;
				} else {
					_obj = obj.KerObject();
				}
			}

			bool Cast(KerName name) {
				if (name.IsNull || _obj == 0) {
					_entryPoint = 0;
					return false;
				}
				_entryPoint = OPointer(_obj->thisO).Cast(name);
				return (_entryPoint != 0);
			}


			property KerObjectT Type {
				KerObjectT get() { return KerObjectT(_obj->Type); }
			}

			property KerName Name {
				KerName get() { return KerName(_obj->Type->Name); }
			}



			wchar_t ReadChar(KerOVar var) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Char)
					throw gcnew ArgumentException("Invalid type.");
				return OPointer(_obj->thisO).get<wchar_t>(var.Offset);
			}

			int ReadInt(KerOVar var) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Int)
					throw gcnew ArgumentException("Invalid type.");
				return OPointer(_obj->thisO).get<int>(var.Offset);
			}

			double ReadDouble(KerOVar var) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Double)
					throw gcnew ArgumentException("Invalid type.");
				return OPointer(_obj->thisO).get<double>(var.Offset);
			}

			KerName ReadName(KerOVar var) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Name)
					throw gcnew ArgumentException("Invalid type.");
				return KerName(OPointer(_obj->thisO).get<CKerName*>(var.Offset));
			}

			KerObject ReadObject(KerOVar var) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Object)
					throw gcnew ArgumentException("Invalid type.");
				return KerObject(OPointer(_obj->thisO).get<OPointer>(var.Offset));
			}

			KerArrBase ^ReadArray(KerOVar var);
			Object ^Read(KerOVar var);



			void WriteChar(KerOVar var, wchar_t value) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Char)
					throw gcnew ArgumentException("Invalid type.");
				OPointer(_obj->thisO).get<wchar_t>(var.Offset) = value;
			}

			void WriteInt(KerOVar var, int value) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Int)
					throw gcnew ArgumentException("Invalid type.");
				OPointer(_obj->thisO).get<int>(var.Offset) = value;
			}

			void WriteDouble(KerOVar var, double value) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Double)
					throw gcnew ArgumentException("Invalid type.");
				OPointer(_obj->thisO).get<double>(var.Offset) = value;
			}

			void WriteName(KerOVar var, KerName value) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Name)
					throw gcnew ArgumentException("Invalid type.");
				OPointer(_obj->thisO).get<CKerName*>(var.Offset) = value;
			}

			void WriteObject(KerOVar var, KerObject value) {
				if (var.LangType.DimensionsCount != 0 || var.LangType.BasicType != BasicType::Object)
					throw gcnew ArgumentException("Invalid type.");
				OPointer(_obj->thisO).get<OPointer>(var.Offset) = value;
			}

			void WriteArray(KerOVar var, KerArrBase ^value);



			void Hold(KerMain ^KerMain);
			void Release(KerMain ^KerMain);
			
			void Kill(KerMain ^KerMain);



			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _obj == 0;
				} else if (obj->GetType() == KerObject::typeid) {
					return _obj == ((KerObject)obj)._obj;
				} else {
					return false;
				}
			}


			static bool operator == (KerObject obj1, KerObject obj2) {
				return (obj1._obj == obj2._obj);
			}

			static bool operator != (KerObject obj1, KerObject obj2) {
				return (obj1._obj != obj2._obj);
			}

			property bool IsNull {
				bool get() { return (_entryPoint == 0);}
			}



			virtual String ^ ToString() override {   
				if (_obj == 0) 
					return "null";
				return Name.ShortString;
			}

		};




	}
}