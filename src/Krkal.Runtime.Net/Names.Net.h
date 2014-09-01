#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Krkal::Compiler;

#include "CStringToCharPtr.h"
#include "NamePtr.h"



namespace Krkal {
	namespace Runtime {


		value class KerObjectT;


		public enum class KerNameType{
			Variable,
			StaticVariable,
			SafeMethod,
			DirectMethod,
			StaticSafeMethod,
			StaticDirectMethod,

			Void,
			Class,
			Param,
			Group,
			Control,
			Enum,

			Game,
			Language,
			Campaign,
			Level,
			Solution,
			RunSource,
			CompileSource,
			Engine,

			ObjectVoid,
			UndefinedClass,

		};



		public value class KerName  {
		private:
			CKerName *_name;
		internal:

			KerName(CKerName *name) {
				_name = name;
			}

			operator CKerName*() { return _name;}

		public:

			property KerNameType Type {
				KerNameType get() {	return (KerNameType)_name->Type; }
			}

			IntPtr GetCKerName() { return IntPtr(_name); }



			property String ^ NameString {
				String ^ get() { return gcnew String(_name->GetNameString()) ; }
			}
			virtual String ^ ToString() override { 
				if (_name == 0)
					return "null";
				return gcnew String(_name->ToString()) ;  
			}
			property String ^ ShortString {
				String ^ get() { return gcnew String(_name->ToShortString()) ; }
			}
			//property String ^ UserName {
			//	String ^ get() { return gcnew String(_name->GetUserName()) ; }
			//}
			//property String ^ Comment {
			//	String ^ get() { return gcnew String(_name->Comment) ; }
			//}

			property KerObjectT ObjectType {
				KerObjectT get();
			}

			property LanguageType LangType {
				LanguageType get() { return LanguageType((BasicType)_name->LT.Type, Modifier::None, _name->LT.DimCount, nullptr); } 
			}



			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _name == 0;
				} else if (obj->GetType() == KerName::typeid) {
					return _name == ((KerName)obj)._name;
				} else {
					return false;
				}
			}


			static bool operator == (KerName name1, KerName name2) {
				return (name1._name == name2._name);
			}

			static bool operator != (KerName name1, KerName name2) {
				return (name1._name != name2._name);
			}

			property bool IsNull {
				bool get() { return (_name == 0);}
			}


			static bool operator < (KerName name1, KerName name2) {
				return (NamePtr(name1._name) < name2._name);
			}

			static bool operator > (KerName name1, KerName name2) {
				return (NamePtr(name1._name) > name2._name);
			}


			static bool operator <= (KerName name1, KerName name2) {
				return (NamePtr(name1._name) <= name2._name);
			}

			static bool operator >= (KerName name1, KerName name2) {
				return (NamePtr(name1._name) >= name2._name);
			}


		};



	}
}