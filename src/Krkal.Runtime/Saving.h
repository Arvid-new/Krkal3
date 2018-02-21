//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - S a v i n g
///
///		Saving of games, levels and data files
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __SAVING_H__
#define __SAVING_H__

#include "CKerMain.h"
#include "fs.api.h"


class CDataObjectBase;

class CKerSaver {
public:
	typedef pair<CFSRegister*, int> RegOrdT;
	typedef pair<CFSRegKey*, int> KeyOrdT;
	typedef unordered_map<CKerName*, RegOrdT> NamesMapT;
	typedef unordered_map<CKerObject*, RegOrdT> ObjectsMapT;
	typedef unordered_map<CKerArrBase*, KeyOrdT> ArraysMapT;


	CKerSaver(CKerMain *kerMain, const char *file, eKerSavingFlags flags, ArrPtr<NamePtr> names, ArrPtr<NamePtr> dependencies, ArrPtr<NamePtr> dataObjects);
	~CKerSaver();
	int Save();
private:
	CKerMain *KerMain;
	char * _file;
	CFSRegister *_code;
	eKerSavingFlags _flags;
	ArrPtr<NamePtr> _names;
	ArrPtr<NamePtr> _dependencies;
	ArrPtr<NamePtr> _dataObjects;
	int _nameStart;
	int _arrayStart;
	int _objectStart;

	NamesMapT _namesMap;
	ObjectsMapT _objectsMap;
	ArraysMapT _arraysMap; // for normal arrays
	ArraysMapT _arraysMap2; // for arrays that were converted (OPointer to Name)

	static char *RegisterNamesText[];
	CFSRegister* _rootRegisters[3];
	CFSRegKey* _arrayLTs;


	enum eRegisterName {
		eRNnames,
		eRNobjects,
		eRNarrays,
	};
	enum eOPointerMode {
		eOPMalreadyIsName,
		eOPMconvertToName,
		eOPMopointer,
	};


	void SetAndSaveOrdinalStarts();
	void SaveNames();
	void SaveDependencies();
	void SaveDataObjects();
	void SaveObjects();
	void SaveMessages();
	void WriteSourceFiles();

	RegOrdT GetOrSaveName(CKerName *name);
	int GetOrSaveArray(CKerArrBase* arr, CLT &lt, eOPointerMode mode);
	RegOrdT GetObject(CKerObject *obj);

	void SaveAttributes(CFSRegister *reg, CDataObjectBase *data, const char* keyName = "Attributes");
	template<typename T>
	void SaveArray(CKerArr<T>* arr, CFSRegKey *key, CLT &fieldLT, eOPointerMode mode);
	void SaveDataObject(CDataObject *obj);
	void SaveObject(CFSRegister *reg, CKerObject* obj, eOPointerMode mode);

	CFSRegister* GetRegister(eRegisterName name);
	EFSRTypes GetValueType(CLT &lt);
	CFSRegKey *CreateValueKey(CFSRegister *reg, CKerName *varName);
	void WriteValue(CFSRegKey *key, CLT &lt, void *value, eOPointerMode mode);
	void WriteLT(CFSRegKey *key, CLT &lt);

};

#endif
