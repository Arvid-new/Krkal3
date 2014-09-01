//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - S a v i n g
///
///		Saving of games, levels and data files
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "Saving.h"
#include "GarbageCollector.h"
#include "DataObject.h"
#include "KerServicesBackend.h"
#include "KerServices.h"



char *CKerSaver::RegisterNamesText[] = {"Names", "Objects", "Arrays"};


CKerSaver::CKerSaver(CKerMain *kerMain, const char *file, eKerSavingFlags flags, ArrPtr<NamePtr> names, ArrPtr<NamePtr> dependencies, ArrPtr<NamePtr> dataObjects) {
	KerMain = kerMain;
	_file = newstrdup(file);
	_code=0;
	_flags = flags;
	_names = names;
	_dependencies = dependencies;
	_dataObjects = dataObjects;
	_rootRegisters[0] = 0; _rootRegisters[1] = 0; _rootRegisters[2] = 0;
	_arrayLTs = 0;
	if (_names)
		KerMain->GarbageCollector->Hold(_names);
	if (_dependencies)
		KerMain->GarbageCollector->Hold(_dependencies);
	if (_dataObjects)
		KerMain->GarbageCollector->Hold(_dataObjects);

}




CKerSaver::~CKerSaver() {
	SAFE_DELETE_ARRAY(_file);
	SAFE_DELETE(_code);
	if (_names)
		KerMain->GarbageCollector->Release(_names);
	if (_dependencies)
		KerMain->GarbageCollector->Release(_dependencies);
	if (_dataObjects)
		KerMain->GarbageCollector->Release(_dataObjects);
}





int CKerSaver::Save() {
	int ret = 0;
	try {
		if (_flags & eKSFsaveDataFile || KerMain->GetRunMode() == eKRMDataEdit) {
			KerMain->Errors->LogError(eKRTEsavingDataFile, 0, _file);
			_code = new CFSRegister(_file, "KRKAL3 DATA", 1);
		} else if (KerMain->GetRunMode() == eKRMEditor) {
			KerMain->Errors->LogError(eKRTEsavingLevel, 0, _file);
			_code = new CFSRegister(_file, "KRKAL3 LEVEL", 1);
		} else if (KerMain->GetRunMode() == eKRMNormal) {
			KerMain->Errors->LogError(eKRTEsavingGame, 0, _file);
			_code = new CFSRegister(_file, "KRKAL3 GAME", 1);
		} else {
			throw CKernelError(eKRTEgameNotSaved);
		}

		char buff[21];
		KerMain->FS->GenerateVersionNumber(buff);
		_code->AddKey("Unique Compilation Id", FSRTstring)->stringwrite(buff);
		_code->WriteVersionFeatures();

		if (_flags & eKSFsaveDataFile) {
			_code->AddKey("Kernel Mode", FSRTint)->writei((int)eKRMDataOnly);
		} else {
			_code->AddKey("Kernel Mode", FSRTint)->writei((int)KerMain->GetRunMode());	
		}

		WriteSourceFiles();

		SetAndSaveOrdinalStarts();
		if (_flags & eKSFsaveGlobalAttributes)
			SaveAttributes(_code, KerMain->Attributes);
		SaveNames();
		SaveDependencies();
		SaveDataObjects();
		if (!(_flags & eKSFsaveDataFile)) {
			SaveObjects();
			if (KerMain->IsGameMode())
				SaveMessages();
		}


		if (!_code->WriteFile())
			throw CKernelError(eKRTEgameNotSaved);
	
	} 
	catch (CKernelError err) {
		ret = err.ErrorNum;
	}
	catch (CKernelPanic) {
		ret = eKRTEgameNotSaved;
	}

	if (ret) {
		KerMain->Errors->LogError(ret);
		if (ret != eKRTEgameNotSaved)
			KerMain->Errors->LogError(eKRTEgameNotSaved);
	} else {
		KerMain->Errors->LogError(eKRTEsaveLOK);
	}
	
	return ret;
}




void CKerSaver::SetAndSaveOrdinalStarts() {
	_nameStart = 1;
	_objectStart = _nameStart + KerMain->KerNamesMain->GetNamesCount();
	_arrayStart = _objectStart + KerMain->GarbageCollector->managedObjects.size();
	CFSRegKey *key = _code->AddKey("Ordinal Starts", FSRTint);
	key->writei(_nameStart);
	key->writei(_objectStart);
	key->writei(_arrayStart);
}



void CKerSaver::SaveNames() {
	if (_flags & eKSFsaveAllNames) {
		for (CKerNameList *i = KerMain->KerNamesMain->Names; i; i=i->next) {
			SaveAttributes(GetOrSaveName(i->name).first, i->name->Attributes);
		}
	} else if (_flags & eKSFsaveNewNames) {
		for (CKerServicesBackend::NamesT::iterator i = KerMain->KerServices->_backend->NewNames.begin(); i != KerMain->KerServices->_backend->NewNames.end(); ++i) {
			SaveAttributes(GetOrSaveName(*i).first, (**i).Attributes);
		}
	} else if (_names) {
		for (int f = 0; f < _names->GetCount(); f++) {
			if (_names[f])
				SaveAttributes(GetOrSaveName(_names[f]).first, _names[f]->Attributes);
		}
	}
}



void CKerSaver::SaveDependencies() {
	CFSRegKey *key = _code->AddKey("Dependencies", FSRTint);
	if (_flags & eKSFsaveAllDependencies) {
		for (CKerNameList *i = KerMain->KerNamesMain->Names; i; i=i->next) {
			for (CKerNameList *j = i->name->childs; j; j=j->next) {
				key->writei(GetOrSaveName(i->name).second);
				key->writei(GetOrSaveName(j->name).second);
			}
		}
	} else if (_flags & eKSFsaveNewDependencies) {
		for (CKerServicesBackend::NamesT::iterator i = KerMain->KerServices->_backend->NewDependencies.begin(); i != KerMain->KerServices->_backend->NewDependencies.end(); ) {
			key->writei(GetOrSaveName(*i).second); ++i;
			key->writei(GetOrSaveName(*i).second); ++i;
		}
	} else if (_dependencies) {
		for (int f = 0; f < _dependencies->GetCount(); f+=2) {
			if (_dependencies[f] && _dependencies[f+1]) {
				key->writei(GetOrSaveName(_dependencies[f]).second);
				key->writei(GetOrSaveName(_dependencies[f+1]).second);
			}
		}
	}
}



void CKerSaver::SaveDataObjects() {
	if (_flags & eKSFsaveAllDataObjects) {
		for (int f=0; f<KerMain->Objs->DataObjectCount; f++) {
			SaveDataObject(KerMain->Objs->DataObjects[f]);
		}
	} else if (_flags & eKSFsaveNewDataObjects) {
		for (CKerServicesBackend::NamesT::iterator i = KerMain->KerServices->_backend->NewDataObjects.begin(); i != KerMain->KerServices->_backend->NewDataObjects.end(); ++i) {
			SaveDataObject((**i).DataObject);
		}
	} else if (_dataObjects) {
		for (int f = 0; f < _dataObjects->GetCount(); f++) {
			if (_dataObjects[f])
				SaveDataObject(_dataObjects[f]->DataObject);
		}
	}
}



void CKerSaver::SaveDataObject(CDataObject *obj) {
	if (!obj)
		return;
	CFSRegister *nameReg = GetOrSaveName(obj->Name).first;
	nameReg->AddKey("Class", FSRTint)->writei(GetOrSaveName(obj->GetClass(KerMain)).second);
	if (obj->Instance) {
		SaveObject(nameReg, obj->Instance, eOPMconvertToName);
	} else {
		SaveAttributes(nameReg, obj, "Data");
	}
}




void CKerSaver::SaveObjects() {
	for (CGarbageCollector::listT::iterator i = KerMain->GarbageCollector->managedObjects.begin(); i != KerMain->GarbageCollector->managedObjects.end(); ++i) {
		CKerObject* obj = dynamic_cast<CKerObject*>(*i);
		if (obj && !obj->DataObjectName) {
			SaveObject(GetObject(obj).first, obj, eOPMopointer);
		}
	}
}




void CKerSaver::SaveObject(CFSRegister *reg, CKerObject* obj, eOPointerMode mode) {
	if (!obj)
		return;
	CFSRegister *dataReg = reg->AddKey("Data", FSRTregister)->GetSubRegister();
	for (int f=0; f < obj->Type->NumVars; f++) {
		CKerOVar *field = obj->Type->OVars + f;
		if (!field->LT.IsStaticConstant()) {
			int varFlag = KerMain->ReadAttribute<int>(field, eKKNattrVarFlag);
			if (mode == eOPMconvertToName) { // saving dataobject
				if (varFlag & eKVFdontSaveToDataObject)
					continue;
			} else if (KerMain->GetRunMode() == eKRMNormal) { // saving object to save game
				if (varFlag & eKVFdontSaveToGame)
					continue;
			} else { // saving object to level
				if (varFlag & eKVFdontSaveToLevel)
					continue;
			}

			CFSRegKey *key = CreateValueKey(dataReg, field->Name);
			WriteValue(key, field->LT, (BYTE*)obj->thisO + field->DataOffset, mode);
		}
	}
}



void CKerSaver::SaveMessages() {
	// TODO
}



void CKerSaver::WriteSourceFiles() {
	CFSRegKey *rootNames = _code->AddKey("Source Project", FSRTstring);
	for (int f=0; f < KerMain->KernelParameters->GetRootNamesCount(); f++) {
		rootNames->stringwrite(KerMain->KernelParameters->GetRootName(f));
	}

	if (KerMain->GetRunMode() == eKRMCreateData || (_flags & eKSFsaveSourceFiles)) {
		CFSRegKey *fileName = _code->AddKey("Source Files", FSRTstring);
		CFSRegKey *fileDate = _code->AddKey("Source Files Dates", FSRTint);
		for (CKernelParameters::UsedFilesT::iterator i = KerMain->KernelParameters->UsedFilesBegin(); i != KerMain->KernelParameters->UsedFilesEnd(); ++i) {
			fileName->stringwrite(i->first.c_str());
			fileDate->writei(i->second.FileTime.dwLowDateTime);
			fileDate->writei(i->second.FileTime.dwHighDateTime);
		}
	}
}




void CKerSaver::SaveAttributes(CFSRegister *reg, CDataObjectBase *data, const char* keyName) {
	if (data && data->GetFieldCount()) {
		CFSRegister *attr = reg->AddKey(keyName, FSRTregister)->GetSubRegister();
		for (int f = 0; f < data->GetFieldCount(); f++) {
			CDataObjectField *field = data->GetField(f);
			CFSRegKey *key = CreateValueKey(attr, field->Name);
			WriteValue(key, field->Name->LT, field->_data, eOPMalreadyIsName);
		}
	}
}





CKerSaver::RegOrdT CKerSaver::GetOrSaveName(CKerName *name) {
	if (!name)
		return RegOrdT((CFSRegister*)0,0);
	NamesMapT::iterator i = _namesMap.find(name);
	if (i != _namesMap.end())
		return i->second;

	CFSRegister *nsReg = GetRegister(eRNnames);
	int ordinal = nsReg->GetNumberOfKeys() + _nameStart;
	CFSRegister *nReg = nsReg->AddKey(name->name, FSRTregister)->GetSubRegister();
	_namesMap[name] = RegOrdT(nReg, ordinal);
	
	nReg->AddKey("Type", FSRTint)->writei(name->Type == eKerNTundefinedClass ? eKerNTclass : name->Type);
	if (!name->LT.IsUnasigned())
		WriteLT(nReg->AddKey("LT", FSRTint), name->LT);

	return RegOrdT(nReg, ordinal);
}



void CKerSaver::WriteLT(CFSRegKey *key, CLT &lt) {
	UI a = ((UI)lt.DimCount << 24) | ((UI)lt.Type << 16) | (UI)lt.Modifier;
	int b = lt.ObjectType ? GetOrSaveName(lt.ObjectType).second : 0;

	key->writei(a); key->writei(b);
}



EFSRTypes CKerSaver::GetValueType(CLT &lt) {
	if (lt.DimCount>0)
		return FSRTint;
	switch (lt.Type) {
		case eKTchar: return FSRTwChar;
		case eKTdouble: return FSRTdouble;
		case eKTint:
		case eKTname:
		case eKTobject: return FSRTint;
		default: throw CExc(eKernel, 0, "Unexpected error");
	}
}



CFSRegKey *CKerSaver::CreateValueKey(CFSRegister *reg, CKerName *varName) {
	int ordinal = GetOrSaveName(varName).second;
	char buff[16];
	sprintf_s(buff, "%i", ordinal);
	return reg->AddKey(buff, GetValueType(varName->LT));
}



void CKerSaver::WriteValue(CFSRegKey *key, CLT &lt, void *value, eOPointerMode mode) {
	if (lt.DimCount>0) {
		CKerArrBase * arr = *(CKerArrBase**)value;
		key->writei(GetOrSaveArray(arr, lt, mode));
	} else {
		switch (lt.Type) {
			case eKTchar: 
				key->writew(*(wchar_t*)value);
				break;
			case eKTdouble: 
				key->writed(*(double*)value);
				break;
			case eKTint:
				key->writei(*(int*)value);
				break;
			case eKTname:
				{
					CKerName * name = *(CKerName**)value;
					key->writei(GetOrSaveName(name).second);
					break;
				}
			case eKTobject:
				if (mode == eOPMalreadyIsName) {
					CKerName * name = *(CKerName**)value;
					key->writei(GetOrSaveName(name).second);
				} else {
					OPointer obj = *(OPointer*)value;
					if (!obj) {
						key->writei(0);
					} else if (obj.KerObject()->DataObjectName) {
						key->writei(GetOrSaveName(obj.KerObject()->DataObjectName).second);
					} else if (mode == eOPMopointer) {
						key->writei(GetObject(obj.KerObject()).second);
					} else {
						key->writei(0);
						KerMain->Errors->LogError(eKRTEsavingOPtrNotAllowed);
					}
				}
				break;
			default: throw CExc(eKernel, 0, "Unexpected error");
		}
	}
}


int CKerSaver::GetOrSaveArray(CKerArrBase* arr, CLT &lt, eOPointerMode mode) {
	if (!arr)
		return 0;
	ArraysMapT &aMap = (lt.Type == eKTobject && mode == eOPMconvertToName) ? _arraysMap2 : _arraysMap;
	ArraysMapT::iterator i = aMap.find(arr);
	if (i != aMap.end())
		return i->second.second;

	CFSRegister *asReg = GetRegister(eRNarrays);
	int ordinal = asReg->GetNumberOfKeys() + _arrayStart;
	CLT fieldLT(lt.Type, lt.Modifier, lt.ObjectType, lt.DimCount-1);
	CFSRegKey *aKey = asReg->AddKey("A", GetValueType(fieldLT));
	aMap[arr] = KeyOrdT(aKey, ordinal);

	if (!_arrayLTs)
		_arrayLTs = _code->AddKey("Arrays LTs", FSRTint);
	WriteLT(_arrayLTs, lt);

	if (fieldLT.DimCount>0) {
		SaveArray((CKerArr<void*>*)arr, aKey, fieldLT, mode);
	} else {
		switch (fieldLT.Type) {
			case eKTchar: SaveArray((CKerArr<wchar_t>*)arr, aKey, fieldLT, mode); break;
			case eKTdouble: SaveArray((CKerArr<double>*)arr, aKey, fieldLT, mode); break;
			case eKTint: SaveArray((CKerArr<int>*)arr, aKey, fieldLT, mode); break;
			default: SaveArray((CKerArr<void*>*)arr, aKey, fieldLT, mode); break;
		}
	}

	return ordinal;

}


template<typename T>
void CKerSaver::SaveArray(CKerArr<T>* arr, CFSRegKey *key, CLT &fieldLT, eOPointerMode mode) {
	for (int f=0; f < arr->GetCount(); f++) {
		WriteValue(key, fieldLT, &arr->At(f), mode);
	}
}



CKerSaver::RegOrdT CKerSaver::GetObject(CKerObject *obj) {
	if (!obj)
		return RegOrdT((CFSRegister*)0,0);
	ObjectsMapT::iterator i = _objectsMap.find(obj);
	if (i != _objectsMap.end())
		return i->second;

	CFSRegister *osReg = GetRegister(eRNobjects);
	int ordinal = osReg->GetNumberOfKeys() + _objectStart;
	char buff[16];
	sprintf_s(buff, "%i", GetOrSaveName(obj->Type->Name).second);
	CFSRegister *oReg = osReg->AddKey(buff, FSRTregister)->GetSubRegister();
	_objectsMap[obj] = RegOrdT(oReg, ordinal);
	
	return RegOrdT(oReg, ordinal);
}



CFSRegister* CKerSaver::GetRegister(eRegisterName name) {
	if (!_rootRegisters[name]) {
		_rootRegisters[name] = _code->AddKey(RegisterNamesText[name], FSRTregister)->GetSubRegister();
	}
	return _rootRegisters[name];
}
