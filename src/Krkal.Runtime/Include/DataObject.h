//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - D a t a O b j e c t
///
///		DataObject holds static data like attributes and data of data objects
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef DATAOBJECT_H
#define DATAOBJECT_H

#include "LanguageType.h"
#include "OPointer.h"

#include <hash_map>
#include <vector>

using namespace std;
using namespace stdext;

class CDataObjectLoader;


union UTempVar {
	wchar_t _char;
	int _int;
	double _double;
	void* _pointer;
};



class KRKALRUNTIME_API CDataObjectField {
public:
	CDataObjectField(CKerName *name, void *data);
	CKerName *Name;		// KSID jmeno
	void *_data;			

	template <typename T>
	const T &GetValue() { return *(T*)_data; } // Warning all OPointers are stored as CKerName* instead
	void Assign(UTempVar &var, CKerMain *KerMain);
	bool Compare(UTempVar &var);

	template<typename T>
	void Assign(T &input, CKerMain *KerMain) {
		if (Name->LT.DimCount) {
			if (*(CKerArrBase**)_data)
				KerMain->GarbageCollector->Release(*(CKerArrBase**)_data);
			if (*(CKerArrBase**)&input)
				KerMain->GarbageCollector->Hold(*(CKerArrBase**)&input);
		}
		GetValueNoConst<T>() = input;
	}

private:
	template <typename T>
	T &GetValueNoConst() { return *(T*)_data; } // Warning all OPointers are stored as CKerName* instead

};






class CDataObjectBackEnd {
public:
	typedef vector<CDataObjectField*> FieldsT;
	typedef hash_map<CKerName*, CDataObjectField*> FieldsMapT;
	~CDataObjectBackEnd() {
		for (FieldsT::iterator i = _fields.begin(); i != _fields.end(); ++i) {
			delete *i;
		}
	}

	FieldsT _fields;
	FieldsMapT _fMap;
};






class KRKALRUNTIME_API CDataObjectBase {
private:
	CDataObjectBackEnd *_backend;
	UC *_data;
	int _dataSize;
	int _maxSize;

public:
	CDataObjectBase() {
		_data = 0;
		_dataSize = 0; _maxSize = 0;
		_backend = new CDataObjectBackEnd();
	}
	~CDataObjectBase() {
		SAFE_DELETE_ARRAY(_data);
		SAFE_DELETE(_backend);
	}
	int GetFieldCount() { return _backend->_fields.size(); }
	CDataObjectField *GetField(int index) { return _backend->_fields[index]; }
	CDataObjectField *FindField(CKerName *name) {
		CDataObjectBackEnd::FieldsMapT::iterator i = _backend->_fMap.find(name);
		return i == _backend->_fMap.end() ? 0 : i->second;
	}
	CDataObjectField * AddField(CKerName *name);

	template <typename T>
	const T &GetValue(CKerName *name) { // Warning all OPointers are stored as CKerName* instead
		CDataObjectField *field = FindField(name);
		if (!field)
			throw CKernelError(eKRTEindexOutOfRange);
		return field->GetValue<T>();
	}

};



class KRKALRUNTIME_API CDataObject : public CDataObjectBase {
public:
	CKerObject* Instance;
	CKerName *Name;
	CDataObject(CKerName *name, CKerMain *KerMain);
	CDataObject(CKerName *name, CKerObject *obj, CKerMain *KerMain);
	~CDataObject();
	void AddClass(CKerName *value);
	CKerName *GetClass(CKerMain *KerMain);
	OPointer Construct(CDataObjectLoader *loader);
private:
	typedef vector<CKerName*> ClassesT;
	ClassesT *_classes;

	void LoadInstanceFields(CDataObjectLoader *loader);
	void* ConvertNameToObject(void* source, CLT &lt, CDataObjectLoader *loader);
};



class KRKALRUNTIME_API CAttributes : public CDataObjectBase {
};

#endif