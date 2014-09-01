#include "stdafx.h"

#include "DataEnvironment.h"
#include "KerServices.h"

using namespace System::Reflection;
using namespace System::Text;


namespace Krkal {
namespace Runtime {



	DataSource::DataSource(String ^path, DataEnvironment^ environment) {
		if (environment == nullptr)
			throw gcnew ArgumentNullException("environment");
		_env = environment;
		_deleteKerMain = true;
		_path = path;

		_parameters = gcnew KernelParameters(path, KernelRunMode::DataOnly, KernelDebugMode::Debug);
		_kerMain = gcnew KerMain(_parameters);
		_kerMain->Load();
	}



	CKerName *DataSource::TestSaveName(String ^name, const char* nameStr) {
		CKerName *ret = _kerMain->GetKerMain()->KerNamesMain->GetNamePointer(nameStr);
		if (ret)
			return ret;

		Identifier ^id = Identifier::ParseKsid(name);
		KsidName ^ksid = nullptr;
		if (_env->Names->TryGetName(id, ksid))
			return SaveName(ksid, nameStr);

		return 0;
	}



	CKerName *DataSource::SaveName(DataObjectMappingAttribute ^varDescription) {
		if (varDescription == nullptr)
			throw gcnew ArgumentNullException("varDescription");

		CStringToCharPtr nameStr(varDescription->Name);

		CKerName *ret = TestSaveName(varDescription->Name, nameStr);
		if (ret)
			return ret;


		CLT lt((eKerTypes)varDescription->LType, 0, 0, varDescription->Dimension);
		return _kerMain->GetKerMain()->KerServices->CreateName(eKerNTvariable, nameStr, lt);

	}


	CKerName *DataSource::SaveName(String ^name, KerNameType type) {
		if (String::IsNullOrEmpty(name))
			return 0;

		CStringToCharPtr nameStr(name);

		CKerName *ret = TestSaveName(name, nameStr);
		if (ret)
			return ret;

		return _kerMain->GetKerMain()->KerServices->CreateName((eKerNameType)type, nameStr, CLT());
	}



	CKerName *DataSource::SaveName(KsidName ^name, const char* nameStr) {
		if (name == nullptr)
			return 0;

		CKerName *ret = _kerMain->GetKerMain()->KerNamesMain->GetNamePointer(nameStr);
		if (ret)
			return ret;

		TypedKsidName ^typedName = dynamic_cast<TypedKsidName ^>(name);

		CLT lt;
		if (typedName != nullptr) {
			lt.Type = (eKerTypes)typedName->LanguageType.BasicType;
			lt.DimCount = typedName->LanguageType.DimensionsCount;
			lt.Modifier = (eKerTypeModifier)typedName->LanguageType.Modifier;
			lt.ObjectType = SaveName(typedName->LanguageType.ObjectType);
		}

		return _kerMain->GetKerMain()->KerServices->CreateName((eKerNameType)name->NameType, nameStr, lt);
	}




	KsidName ^DataSource::LoadName(CKerName *name) {
		if (!name)
			return nullptr;

		Identifier ^id = Identifier::ParseKsid(gcnew String(name->name));
		KsidName ^ret = nullptr;
		if (!_env->Names->TryGetName(id, ret)) {
			ret = _env->Names->CreateName(id, (NameType)name->Type);
		} else if (ret->NameType != (NameType)name->Type) {
			ret->RemoveFromDag();
			ret = _env->Names->CreateName(id, (NameType)name->Type);
		}

		TypedKsidName ^typedRet = dynamic_cast<TypedKsidName ^>(ret);
		if (typedRet != nullptr) {
			typedRet->LanguageType = LanguageType((BasicType)name->LT.Type, (Modifier)name->LT.Modifier, name->LT.DimCount, dynamic_cast<ClassOrEnumName^>( LoadName(name->LT.ObjectType)));
		}

		return ret;
	}




	void DataSource::SaveObject(Object ^obj, KsidName ^name) {
		Type ^type = obj->GetType();
		array<Object^>^ attributes = type->GetCustomAttributes(DataObjectClassAttribute::typeid, true);
		if (attributes->Length == 0)
			return;

		CKerName *name2 = SaveName(name);
		if (!name2)
			return;

		for each(DataObjectClassAttribute ^ attr in attributes) {
			_kerMain->GetKerMain()->KerServices->CreateDataObject(SaveName(attr->Name, KerNameType::Class), name2);
		}

		for each(PropertyInfo ^propertyInfo in type->GetProperties()) {
			if (propertyInfo->CanRead) {
				attributes = propertyInfo->GetCustomAttributes(DataObjectMappingAttribute::typeid, true);
				if (attributes->Length == 1) {
					CKerName *varName = SaveName(safe_cast<DataObjectMappingAttribute^>(attributes[0]));
					Type ^innerType = nullptr;
					if (varName && CheckTypeCompatibility(propertyInfo->PropertyType, varName->LT, innerType)) {
						Object ^value = propertyInfo->GetGetMethod()->Invoke(obj, nullptr);
						SaveVariable(value, varName, name2);
					}
				}
			}
		}
	}



	void DataSource::SaveVariable(Object ^value, CKerName *varName, CKerName *dataObject) {
		if (varName->LT.DimCount) {
			CKerArrBase* arr = CreateArray(value, varName->LT);
			_kernel->DataObjectTryForcedWrite(dataObject, varName, arr);
		} else {
			switch (varName->LT.Type) {
				case eKTchar: 
					{
						wchar_t ch = Convert::ToChar(value);
						_kernel->DataObjectTryForcedWrite(dataObject, varName, ch);
						break;
					}
				case eKTint: 
					{
						int i = Convert::ToInt32(value);
						_kernel->DataObjectTryForcedWrite(dataObject, varName, i);
						break;
					}
				case eKTdouble: 
					{
						double d = Convert::ToDouble(value);
						_kernel->DataObjectTryForcedWrite(dataObject, varName, d);
						break;
					}
				case eKTname: 
					{
						CKerName *name = SaveName(safe_cast<KsidName^>(value));
						_kernel->DataObjectTryForcedWrite(dataObject, varName, name);
						break;
					}					
			}
		}
	}



	CKerArrBase* DataSource::CreateArray(Object ^value, const CLT &lt) {
		if (value == nullptr)
			return 0;
		System::Collections::IEnumerable ^collection = safe_cast<System::Collections::IEnumerable^>(value);

		if (lt.DimCount > 1) {
			CKerArr<CKerArrBase*>* arr = new CKerArr<CKerArrBase*>(_kernel->GarbageCollector);
			CLT mLt = lt;
			mLt.DimCount--;
			for each(Object ^member in collection) {
				arr->AddLast(CreateArray(member, mLt));
			}
			return arr;
		} else {
			switch (lt.Type) {
				case eKTchar: 
					{
						CKerArr<wchar_t>* arr = new CKerArr<wchar_t>(_kernel->GarbageCollector);
						for each(Object ^member in collection) {
							arr->AddLast(Convert::ToChar(member));
						}
						return arr;
					}
				case eKTint: 
					{
						CKerArr<int>* arr = new CKerArr<int>(_kernel->GarbageCollector);
						for each(Object ^member in collection) {
							arr->AddLast(Convert::ToInt32(member));
						}
						return arr;
					}
				case eKTdouble: 
					{
						CKerArr<double>* arr = new CKerArr<double>(_kernel->GarbageCollector);
						for each(Object ^member in collection) {
							arr->AddLast(Convert::ToDouble(member));
						}
						return arr;
					}
				case eKTname: 
					{
						CKerArr<CKerName*>* arr = new CKerArr<CKerName*>(_kernel->GarbageCollector);
						for each(Object ^member in collection) {
							arr->AddLast(SaveName(safe_cast<KsidName^>(member)));
						}
						return arr;
					}					
			}
		}
		return 0;
	}



	bool DataSource::CheckTypeCompatibility(Type ^type, CLT &lt, Type ^%innerType) {
		if (lt.DimCount) {
			CLT mLT = lt;
			mLT.DimCount--;
			if (type->IsInterface && type->IsGenericType) {
				if (type->GetGenericTypeDefinition() == IEnumerable::typeid) {
					return CheckTypeCompatibility(type->GetGenericArguments()[0], mLT, innerType);
				}
			}
			for each (Type^ itype in type->GetInterfaces()) {
				if (itype->IsGenericType) {
					if (itype->GetGenericTypeDefinition() == IEnumerable::typeid) {
						return CheckTypeCompatibility(itype->GetGenericArguments()[0], mLT, innerType);
					}
				}
			}
			return false;
		} else {
			innerType = type;
			switch (lt.Type) {
				case eKTint:
				case eKTchar:
				case eKTdouble:
					return (type == Int32::typeid || type == Double::typeid || type == Char::typeid || type == Boolean::typeid);
				case eKTname:
					return KsidName::typeid->IsAssignableFrom(type);
				default:
					return false;
			}
		}
	}


	void DataSource::LoadObject(Object ^obj, KsidName ^name) {
		Type ^type = obj->GetType();

		CKerName *name2 = _kernel->KerNamesMain->GetNamePointer(CStringToCharPtr(name->Identifier->ToKsidString()));
		if (!name2 || !name2->DataObject)
			return;

		for each(PropertyInfo ^propertyInfo in type->GetProperties()) {
			if (propertyInfo->CanWrite) {
				array<Object^>^ attributes = propertyInfo->GetCustomAttributes(DataObjectMappingAttribute::typeid, true);
				if (attributes->Length == 1) {
					CKerName *varName = _kernel->KerNamesMain->GetNamePointer(CStringToCharPtr(safe_cast<DataObjectMappingAttribute^>(attributes[0])->Name));
					Type ^innerType = nullptr;
					if (varName && CheckTypeCompatibility(propertyInfo->PropertyType, varName->LT, innerType)) {
						Object ^value;
						if (LoadVariable(value, propertyInfo->PropertyType, innerType, varName, name2)) {
							array<Object^>^ prms = gcnew array<Object^>(1);
							prms[0] = value;
							propertyInfo->GetSetMethod()->Invoke(obj, prms);
						}
					}
				}
			}
		}
	}



	template<typename T>
	bool DataSource::LoadVariable2(Object ^%result, Type ^type, CKerName *varName, CKerName *dataObject) {
		T res;
		if (_kernel->DataObjectTryRead(dataObject, varName, res)) {
			result = Convert::ChangeType(res, type);
			return true;
		}
		return false;
	}






	bool DataSource::LoadVariable(Object ^%result, Type ^propertyType, Type ^innerType, CKerName *varName, CKerName *dataObject) {
		if (varName->LT.DimCount) {
			CKerArrBase *arr = 0;
			if (_kernel->DataObjectTryRead(dataObject, varName, arr)) {
				if (!arr) {
					result = nullptr;
				} else if (propertyType == String::typeid) {
					result = LoadString(arr, varName->LT);
				} else {
					result = LoadArray(arr, innerType, varName->LT);
				}
				return true;
			}
		} else {
			switch (varName->LT.Type) {
				case eKTint:
					return LoadVariable2<int>(result, propertyType, varName, dataObject);
				case eKTchar:
					return LoadVariable2<wchar_t>(result, propertyType, varName, dataObject);
				case eKTdouble:
					return LoadVariable2<double>(result, propertyType, varName, dataObject);
				case eKTname:
					{
						CKerName *name = 0;
						if (_kernel->DataObjectTryRead(dataObject, varName, name)) {
							result = LoadName(name);
							return true;
						}
					}
			}
		}
		return false;
	}



	Type ^DataSource::CreateListType(Type ^innerType, int dimCount) {
		Type ^res = innerType;
		for (int f=0; f < dimCount; f++) {
			array<Type^>^ arr = gcnew array<Type^>(1);
			arr[0] = res;
			if (f == dimCount-1) {
				res = List::typeid->MakeGenericType(arr);
			} else {
				res = IEnumerable::typeid->MakeGenericType(arr);
			}
		}
		return res;
	}


	System::Collections::IList ^DataSource::CreateList(Type ^innerType, int dimCount) {
		Type ^type = CreateListType(innerType, dimCount);
		return safe_cast<System::Collections::IList ^>(type->GetConstructor(Type::EmptyTypes)->Invoke(nullptr));
	}



	template<typename T>
	void DataSource::LoadArray2(System::Collections::IList ^list, Type ^type, CKerArrBase *arr) {
		CKerArr<T>* arr2 = (CKerArr<T>*)arr;
		for (int f=0; f<arr2->GetCount(); f++) {
			T m = arr2->At(f);
			list->Add(Convert::ChangeType(m, type));
		}
	}



	Object ^DataSource::LoadArray(CKerArrBase* arr, Type ^innerType, const CLT& lt) {
		if (!arr)
			return nullptr;
		System::Collections::IList ^list = CreateList(innerType, lt.DimCount);

		if (lt.DimCount > 1) {
			CKerArr<CKerArrBase*>* arr2 = (CKerArr<CKerArrBase*>*)arr;
			CLT mLT = lt;
			mLT.DimCount--;
			for (int f=0; f<arr2->GetCount(); f++) {
				list->Add(LoadArray(arr2->At(f), innerType, mLT));
			}
		} else {
			switch (lt.Type) {
				case eKTchar:
					LoadArray2<wchar_t>(list, innerType, arr);
					break;
				case eKTint:
					LoadArray2<int>(list, innerType, arr);
					break;
				case eKTdouble:
					LoadArray2<double>(list, innerType, arr);
					break;
				case eKTname:
					{
						CKerArr<CKerName*>* arr2 = (CKerArr<CKerName*>*)arr;
						for (int f=0; f<arr2->GetCount(); f++) {
							CKerName* name = arr2->At(f);
							list->Add(LoadName(name));
						}
						break;
					}
			}
		}

		return list;
	}


	String ^DataSource::LoadString(CKerArrBase* arr, const CLT &lt) {
		StringBuilder ^sb = gcnew StringBuilder();
		IEnumerable<wchar_t>^ collection = safe_cast<IEnumerable<wchar_t>^>(LoadArray(arr, wchar_t::typeid, lt));
		for each (wchar_t ch in collection) {
			sb->Append(ch);
		}
		return sb->ToString();
	}








	IList<KsidName^>^ DataSource::GetNameLayerOrSet(KsidName ^parent, KerNameType type, bool isSet) {
		IList<KsidName^>^ ret = gcnew List<KsidName^>();
		CKerName *name = 0;
		if (parent != nullptr) {
			name = _kernel->KerNamesMain->GetNamePointer(CStringToCharPtr(parent->Identifier->ToKsidString()));
			if (!name)
				return ret;
		}
		
		CKerNameList *list = 0;
		if (isSet) {
			list = _kernel->KerNamesMain->FindSet(name, 0, (int)type);
		} else {
			list = _kernel->KerNamesMain->FindLayer(name, 0, (int)type);
		}

		for (CKerNameList *list2 = list ; list2; list2 = list2->next) {
			ret->Add(LoadName(list2->name));
		}

		if(list) list->DeleteAll();
		return ret;
	}



}
}
