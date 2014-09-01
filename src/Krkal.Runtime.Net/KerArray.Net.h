#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Krkal::Compiler;

#include "CStringToCharPtr.h"
#include "Names.Net.h"
#include "KernelArray.h"
#include "CKerMain.Net.h"
#include "Objects.Net.h"



namespace Krkal {
	namespace Runtime {


		ref class KerMain;


		public ref class KerArrBase abstract {


		protected:
			CKerArrBase *_arr;
			LanguageType _lt;

			KerArrBase(CKerArrBase *arr, LanguageType lt) {
				_arr = arr;
				_lt = lt;
			}

		internal:

			CKerArrBase* GetCKerArrBase() { return _arr;}

			static KerArrBase^ Create(CKerArrBase *arr, LanguageType lt);

		public:


			property LanguageType LT {
				LanguageType get() { return _lt;}
			}


			property int Count {
				int get() { return _arr->GetCount(); }
			}


			virtual Object^ Read(int index) abstract;


			void Hold(KerMain ^KerMain);
			void Release(KerMain ^KerMain);


			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _arr == 0;
				} else if (obj->GetType() == KerArrBase::typeid) {
					return _arr == ((KerArrBase^)obj)->_arr;
				} else {
					return false;
				}
			}


			static bool operator == (KerArrBase ^arr1, KerArrBase ^arr2) {
				if ((Object^)arr1 == nullptr || (Object^)arr2 == nullptr)
					return ((Object^)arr1 == nullptr && (Object^)arr2 == nullptr);
				return (arr1->_arr == arr2->_arr);
			}

			static bool operator != (KerArrBase ^arr1, KerArrBase ^arr2) {
				return !(arr1 == arr2);
			}

			property bool IsNull {
				bool get() { return (_arr == 0);}
			}

			virtual String ^ ToString() override {   
				if (_arr == 0) 
					return "null";
				return "Count = " + Count.ToString();
			}

		};



		template<typename T, typename T2>
		public ref class KerArr : public KerArrBase {

		internal:

			KerArr(CKerArrBase *arr, LanguageType lt) : KerArrBase(arr, lt) {}

		public:

			// default indexer
			property T2 default[int] {
				T2 get(int index) {
					return T2(((CKerArr<T>*)_arr)->At(index));
				}
				void set(int index, T2 value) {
					((CKerArr<T>*)_arr)->At(index) = value;
				}
			}

			virtual Object^ Read(int index) override {
				return T2(((CKerArr<T>*)_arr)->At(index));
			}


		};







		ref class KerArrInt : public KerArr<int, int> {
		internal:
			KerArrInt(CKerArrBase *arr, LanguageType lt) : KerArr(arr, lt) {}
		};

		ref class KerArrChar : public KerArr<wchar_t, wchar_t> {
		internal:
			KerArrChar(CKerArrBase *arr, LanguageType lt) : KerArr(arr, lt) {}
		};

		ref class KerArrName : public KerArr<NamePtr, KerName> {
		internal:
			KerArrName(CKerArrBase *arr, LanguageType lt) : KerArr(arr, lt) {}
		};

		ref class KerArrObject : public KerArr<OPointer, KerObject> {
		internal:
			KerArrObject(CKerArrBase *arr, LanguageType lt) : KerArr(arr, lt) {}
		};

		ref class KerArrDouble : public KerArr<double, double> {
		internal:
			KerArrDouble(CKerArrBase *arr, LanguageType lt) : KerArr(arr, lt) {}
		};






		public ref class KerArrArray : public KerArrBase {

		internal:

			KerArrArray(CKerArrBase *arr, LanguageType lt) : KerArrBase(arr, lt) {}

		public:

			// default indexer
			property KerArrBase^ default[int] {
				KerArrBase^ get(int index) {
					if (((CKerArr<CKerArrBase*>*)_arr)->At(index) == 0)
						return nullptr;

					LanguageType lt = _lt;
					lt.DimensionsCount--;

					return KerArrBase::Create(((CKerArr<CKerArrBase*>*)_arr)->At(index), lt);
				}
				void set(int index, KerArrBase^ value) {
					if (value != nullptr) {
						LanguageType lt = _lt;
						lt.DimensionsCount--;
						if (value->LT != lt)
							throw gcnew ArgumentException("Invalid type");
						((CKerArr<CKerArrBase*>*)_arr)->At(index) = value->GetCKerArrBase();
					} else {
						((CKerArr<CKerArrBase*>*)_arr)->At(index) = 0;
					}
				}
			}


			virtual Object^ Read(int index) override {
				return this[index];
			}


		};







	}
}