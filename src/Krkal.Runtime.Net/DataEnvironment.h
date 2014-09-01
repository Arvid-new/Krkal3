
#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace Krkal::Compiler;

#include "CStringToCharPtr.h"
#include "CKerMain.Net.h"



namespace Krkal {
	namespace Runtime {





		[AttributeUsage(AttributeTargets::Property)]
		public ref class DataObjectMappingAttribute : public Attribute {
			String ^_name;
			BasicType _lType;
			int _dimension;


		public:
			property String ^Name {
				String ^get() { return _name; }
				void set(String ^value) { _name = value; }
			}

			property BasicType LType {
				BasicType get() { return _lType; }
				void set(BasicType value) { _lType = value; }
			}

			property int Dimension {
				int get() { return _dimension; }
				void set(int value) { _dimension = value; }
			}

			DataObjectMappingAttribute(String ^name, BasicType lType) {
				Init(name, lType, 0);
			}

			DataObjectMappingAttribute(String ^name, BasicType lType, int dimension) {
				Init(name, lType, dimension);
			}

		private:
			void Init(String ^name, BasicType lType, int dimension) {
				if (name == nullptr)
					throw gcnew ArgumentNullException("name");
				if (name->Length == 0)
					throw gcnew ArgumentException("name is empty");
				if (lType != BasicType::Char && lType != BasicType::Int && lType != BasicType::Double && lType != BasicType::Name)
					throw gcnew ArgumentException("unsupported language type");
				_name = name;
				_lType = lType;
				_dimension = dimension;
			}
		};





		[AttributeUsage(AttributeTargets::Class)]
		public ref class DataObjectClassAttribute : public Attribute {
			String ^_name;
		public:
			property String ^Name {
				String ^get() { return _name; }
				void set(String ^value) { _name = value; }
			}

			DataObjectClassAttribute(String ^name) {
				if (name == nullptr)
					throw gcnew ArgumentNullException("name");
				if (name->Length == 0)
					throw gcnew ArgumentException("name is empty");
				_name = name;
			}
		};










		public ref class DataEnvironment {
			KsidNames ^_names;

		public:
			property KsidNames^ Names {
				KsidNames ^get() { 
					if (_names == nullptr)
						_names = gcnew KsidNames();
					return _names; 
				}
			}
		};












		public ref class DataSource : IDisposable {
			KerMain ^_kerMain;
			KernelParameters ^_parameters;
			String ^_path;
			bool _deleteKerMain;
			DataEnvironment ^_env;

		public:
			property KerMain^ Kernel {
				KerMain ^ get() { return _kerMain; }
			}
			property DataEnvironment ^ Environment {
				DataEnvironment ^ get() { return _env; }
			}

			DataSource(KerMain ^kerMain, DataEnvironment^ environment) {
				if (kerMain == nullptr)
					throw gcnew ArgumentNullException("kerMain");
				if (environment == nullptr)
					throw gcnew ArgumentNullException("environment");
				_deleteKerMain = false;
				_kerMain = kerMain;
				_env = environment;
			}
			DataSource(String ^path, DataEnvironment^ environment);

			~DataSource() {
				this->!DataSource();
			}

			!DataSource() {
				if (_deleteKerMain) {
					delete _kerMain;
					delete _parameters;
				}
				_kerMain = nullptr;
				_parameters = nullptr;
			}


			int Save() {
				return Save(_path);
			}

			int Save(String ^file) {
				try {
					return _kerMain->GetKerMain()->Save(CStringToCharPtr(file), eKSFsaveConfiguration);
				} catch (CKernelPanic) {
					throw gcnew KernelPanicException();
				}
			}

			void SaveObject(Object ^obj, KsidName ^name);
			void LoadObject(Object ^obj, KsidName ^name);
			IList<KsidName^>^ GetNameLayerOrSet(KsidName ^parent, KerNameType type, bool isSet);
			KsidName ^LoadName(KerName name) {
				if (name.IsNull)
					return nullptr;
				return LoadName(name.operator CKerName *());
			}


		internal:
			CKerName *SaveName(DataObjectMappingAttribute ^varDescription);
			CKerName *SaveName(String ^name, KerNameType type);
			CKerName *SaveName(KsidName ^name) { return name == nullptr ? 0 : SaveName(name, CStringToCharPtr(name->Identifier->ToKsidString()));}
			KsidName ^LoadName(CKerName *name);

		private:
			property CKerMain* _kernel {
				CKerMain* get() { return _kerMain->GetKerMain(); }
			}

			CKerName *TestSaveName(String ^name, const char* nameStr);
			CKerName *SaveName(KsidName ^name, const char* nameStr);
			bool CheckTypeCompatibility(Type ^type, CLT &lt, Type ^%innerType);
			void SaveVariable(Object ^value, CKerName *varName, CKerName *dataObject);
			CKerArrBase* CreateArray(Object ^value, const CLT &lt);
			bool LoadVariable(Object ^%result, Type ^propertyType, Type ^innerType, CKerName *varName, CKerName *dataObject);
			String ^LoadString(CKerArrBase* arr, const CLT& lt);
			template<typename T>
			bool LoadVariable2(Object ^%result, Type ^type, CKerName *varName, CKerName *dataObject);
			Object ^LoadArray(CKerArrBase* arr, Type ^innerType, const CLT& lt);
			System::Collections::IList ^CreateList(Type ^innerType, int dimCount);
			Type ^CreateListType(Type ^innerType, int dimCount);
			template<typename T>
			void LoadArray2(System::Collections::IList ^list, Type ^type, CKerArrBase *arr);



		};



	}
}