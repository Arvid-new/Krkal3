//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - D a t a O b j e c t
///
///		DataObject holds static data like attributes and data of data objects
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "DataObject.h"
#include "names.h"
#include "CKerMain.h"
#include "KerServices.h"
#include "loading.h"



CDataObjectField::CDataObjectField(CKerName *name, void *data) {
	Name = name; _data = data;
}



void CDataObjectField::Assign(UTempVar &var, CKerMain *KerMain) {
	if (Name->LT.DimCount > 0) {
		if (GetValue<CKerArrBase*>())
			KerMain->GarbageCollector->Release(GetValue<CKerArrBase*>());
		if (var._pointer)
			KerMain->GarbageCollector->Hold((CKerArrBase*)var._pointer);
		GetValueNoConst<void*>() = var._pointer;
	} else {
		switch (Name->LT.Type) {
			case eKTchar: GetValueNoConst<wchar_t>() = var._char; return;
			case eKTdouble: GetValueNoConst<double>() = var._double; return;
			case eKTint: GetValueNoConst<int>() = var._int; return;
			default: GetValueNoConst<void*>() = var._pointer; return;
		}
	}
}
bool CDataObjectField::Compare(UTempVar &var) {
	if (Name->LT.DimCount > 0) {
		return CKerServices::RecursiveArrayCompare(Name->LT, GetValue<CKerArrBase*>(), (CKerArrBase*)var._pointer);
	} else {
		switch (Name->LT.Type) {
			case eKTchar: return GetValue<wchar_t>() == var._char;
			case eKTdouble: return GetValue<double>() == var._double;
			case eKTint: return GetValue<int>() == var._int;
			default: return GetValue<void*>() == var._pointer;
		}
	}
}



CDataObjectField * CDataObjectBase::AddField(CKerName *name) {
	if (FindField(name))
		return 0;
	int size = (int)name->LT.SizeOf();
	if (!size)
		return 0;

	if (_dataSize + size > _maxSize) {
		int newsize = _maxSize * 2 > 16 ? _maxSize * 2 : 16;
		UC *newdata = new UC[newsize];
		memset(newdata, 0, newsize);
		if (_maxSize) {
			memcpy(newdata, _data, _maxSize);
			for (CDataObjectBackEnd::FieldsT::iterator i = _backend->_fields.begin(); i != _backend->_fields.end(); ++i) {
				(**i)._data = (UC*)((**i)._data) - _data + newdata;
			}
			SAFE_DELETE_ARRAY(_data);
		}
		_data = newdata;
		_maxSize = newsize;
	}

	CDataObjectField *field = new CDataObjectField(name, _data + _dataSize);
	_dataSize += size;
	_backend->_fields.push_back(field);
	_backend->_fMap[name] = field;

	return field;
}




CDataObject::CDataObject(CKerName *name, CKerMain *KerMain) { 
	Name = name; _classes=0; Instance=0;
	Name->DataObject = this;
	KerMain->Objs->AddDataObject(this);
}


CDataObject::CDataObject(CKerName *name, CKerObject *obj, CKerMain *KerMain) {
	Name = name; _classes=0; Instance=0;
	Name->DataObject = this;
	KerMain->Objs->AddDataObject(this);
	AddClass(obj->Type->Name);
	obj->DataObjectName = name;
	Instance = obj;
	KerMain->GarbageCollector->Hold(obj);
}


OPointer CDataObject::Construct(CDataObjectLoader *loader) {
	if (Instance)
		return Instance->thisO;

	OPointer obj = loader->KerMain->Construct(GetClass(loader->KerMain));
	if (obj) {
		CKerObject *o = obj.KerObject();
		o->DataObjectName = Name;
		Instance = o;
		loader->KerMain->GarbageCollector->Hold(o);
		LoadInstanceFields(loader);
	}

	return obj;
}


void CDataObject::LoadInstanceFields(CDataObjectLoader *loader) {
	CKerOVar *var = Instance->Type->OVars;
	for (int f=0; f < Instance->Type->NumVars; f++, var++) {
		int varFlag = loader->KerMain->ReadAttribute<int>(var, eKKNattrVarFlag);
		if (varFlag & eKVFdontSaveToDataObject)
			continue;

		CDataObjectField *field = FindField(var->Name);
		if (!field) {
			loader->KerMain->Errors->LogError(eKRTEVarLoad, 0, var->Name->name);
		} else {
			if (var->LT.Type == eKTobject) {
				var->GetValue<void*>(Instance) = ConvertNameToObject(field->GetValue<void*>(), var->LT, loader);
			} else {
				if (var->LT.DimCount > 0) {
					var->GetValue<void*>(Instance) = field->GetValue<void*>();
				} else {
					switch (Name->LT.Type) {
						case eKTchar: var->GetValue<wchar_t>(Instance) = field->GetValue<wchar_t>(); return;
						case eKTdouble: var->GetValue<double>(Instance) = field->GetValue<double>(); return;
						case eKTint: var->GetValue<int>(Instance) = field->GetValue<int>(); return;
						default: var->GetValue<void*>(Instance) = field->GetValue<void*>(); return;
					}
				}
			}
		}
	}

}



void* CDataObject::ConvertNameToObject(void* source, CLT &lt, CDataObjectLoader *loader) {
	if (!source)
		return 0;

	if (lt.DimCount > 0) {
		CKerArr<void*>* arr = (CKerArr<void*>*)source;
		CKerArr<void*>* ret = new CKerArr<void*>(loader->KerMain->GarbageCollector, arr->GetCount());
		CLT lt2(lt.Type, lt.Modifier, lt.ObjectType, lt.DimCount-1);
		for (int f=0; f < arr->GetCount(); f++) {
			ret->AddLast(ConvertNameToObject(arr->At(f), lt2, loader));
		}
		return ret;
	} else {
		CKerName *name = (CKerName*)source;
		OPointer obj = loader->Load(name).Cast(lt.ObjectType);
		return *(void**)&obj;
	}
}



CDataObject::~CDataObject() {
	SAFE_DELETE(_classes);
}



void CDataObject::AddClass(CKerName *value) {
	if (!value)
		return;
	bool found = false;
	if (!_classes)
		_classes = new ClassesT();
	
	for (ClassesT::iterator i = _classes->begin(); i != _classes->end(); ) {
		if (value == *i) {
			found = true;
		} else {
			int res = value->Compare(*i);
			if (res == 1) {
				found = true;
			} else if (res == 2) {
				if (found) {
					i = _classes->erase(i);
					continue;
				} else {
					*i = value;
					found = true;
				}
			}
		}
		++i;
	}

	if (!found)
		_classes->push_back(value);
}




CKerName *CDataObject::GetClass(CKerMain *KerMain) {
	if (!_classes) {
		KerMain->Errors->LogError(eKRTEInvalidObjType, 0, Name->name);
		return KerMain->Object;
	} else {
		if (_classes->size() > 1)
			KerMain->Errors->LogError(eKRTEInvalidObjType, 0, Name->name);
		return *_classes->begin();
	}
}
