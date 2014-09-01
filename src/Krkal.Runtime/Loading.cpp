//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - L o a d i n g
///
///		Loading of scripts, levels, games and data files
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "Loading.h"
#include "CKerMain.h"
#include "fs.api.h"
#include "KerServices.h"
#include "DataObject.h"
#include "RootNames.h"


CScriptLoader::~CScriptLoader() {
	if (!_dontDeleteCode)
		SAFE_DELETE(_code);
	SAFE_DELETE(_dataObjectLoader);
}


void CScriptLoader::Load() {
	if (!_code) {
		if (_mode == eLMscript) {
			_code = new CFSRegister(_codePath,"KRKAL3 SCRIPT CODE");
		} else if (_mode == eLMdata) {
			_code = new CFSRegister(_codePath,"KRKAL3 DATA");
		} else if (_mode == eLMlevel) {
			_code = new CFSRegister(_codePath,"KRKAL3 LEVEL");
		} else{
			KerMain->Errors->LogError(eKRTEPELoadingScripts);
		}
	}
	if (_code->GetOpenError()!=FSREGOK)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);

	if (!_code->CheckVersionFeatures())
		KerMain->Errors->LogError(eKRTEinvalidFileVersion);

	LoadOrdinalStarts();
	CFSRegister *names = TryGetSubRegister(_code, "Names");

	LoadNames(names);
	if (_mode == eLMscript) {
		KerMain->FindOrAddKnownNames();
		KerMain->SetConstructors();
	}
	LoadDependencies();
	AssignLTs(names);
	PrepareArrays();
	LoadAttributes(TryGetSubRegister(_code, "Attributes"), KerMain->Attributes);
	LoadAttributesOfNames(names);
	
	if (_mode == eLMscript)
		LoadClasses(names);

	LoadDataObjects(names);
}




void CScriptLoader::LoadOrdinalStarts() {
	CFSRegKey *starts = _code->FindKey("Ordinal Starts");
	if (!starts || starts->CFSGetKeyType() != FSRTint || starts->top < 3)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	_nameStart = starts->readi();
	_objectStart = starts->readi();
	_arrayStart = starts->readi();
}





void CScriptLoader::LoadNames(CFSRegister *names) {
	if (names) {
		for(CFSRegKey *key = names->GetFirstKey(); key; key = key->GetNextKey()) {
			CKerName *name = KerMain->KerNamesMain->GetNamePointer(key->GetName());
			if (!name) {
				name = new CKerName(key->GetName(), (eKerNameType)ReadInt(GetSubRegister(key), "Type"), KerMain->KerNamesMain);
				if (name->Type == eKerNTclass) {
					if (_mode == eLMscript) {
						KerMain->Objs->NumObjectT++;
					} else {
						name->Type = eKerNTundefinedClass;
					}
				}
			} else {
				if (name->Type != (eKerNameType)ReadInt(GetSubRegister(key), "Type") && name->Type != eKerNTundefinedClass)
					KerMain->Errors->LogError(eKRTEksidTypeConflict, 0, name->name);
			}
			ksidNames.push_back(name);
		}
	}
}





void CScriptLoader::AssignLTs(CFSRegister *names) {
	if (names) {
		int f = 0;
		for(CFSRegKey *key = names->GetFirstKey(); key; key = key->GetNextKey(), f++) {
			CLT lt;
			CKerName *name = ksidNames[f];
			CFSRegister *nameReg = GetSubRegister(key);
			if (nameReg->FindKey("LT")) {
				lt = ReadLT(nameReg);
				if (name->LT.IsUnasigned()) {
					name->LT = lt;
				} else if (name->LT != lt || name->LT.Modifier != lt.Modifier) {
					KerMain->Errors->LogError(eKRTEksidLTConflict, 0, name->name);
				}
			}
			_originalKsidLts.push_back(lt);
		}
	}
}





void CScriptLoader::LoadAttributesOfNames(CFSRegister *names) {
	if (names) {
		int f = 0;
		for(CFSRegKey *key = names->GetFirstKey(); key; key = key->GetNextKey(), f++) {
			CKerName *name = ksidNames[f];
			CFSRegister *nameReg = GetSubRegister(key);
			LoadAttributes(TryGetSubRegister(nameReg, "Attributes"), name->Attributes);
		}
	}
}





void CScriptLoader::LoadClasses(CFSRegister *names) {
	if (names && KerMain->Objs->NumObjectT) {
		KerMain->Objs->ObjectTypes = new CKerObjectT[KerMain->Objs->NumObjectT];
		CKerObjectT* currentObjT = KerMain->Objs->ObjectTypes;
		int f = 0;
		for(CFSRegKey *key = names->GetFirstKey(); key; key = key->GetNextKey(), f++) {
			CKerName *name = ksidNames[f];
			CFSRegister *nameReg = GetSubRegister(key);
			if (name->Type == eKerNTclass) {
				currentObjT->Name = name;
				name->ObjectType = currentObjT;
				LoadMethods(TryGetSubRegister(nameReg, "Methods"), currentObjT);
				LoadVariables(TryGetSubRegister(nameReg, "Variables"), currentObjT);
				LoadStaticVariables(TryGetSubRegister(nameReg, "Statics"), currentObjT);

				currentObjT->CalculateVariableOffsets(*KerMain);

				currentObjT++;
			}
		}
	}
}




void CScriptLoader::PrepareArrays() {
	CFSRegister *arrReg = TryGetSubRegister(_code, "Arrays");
	if (arrReg) {
		for (CFSRegKey *key = arrReg->GetFirstKey(); key; key = key->GetNextKey()) {
			_arrays.push_back(pair<CFSRegKey*, void*>(key, 0));
		}
		CFSRegKey *types = _code->FindKey("Arrays LTs");
		if (!types || types->CFSGetKeyType() != FSRTint || types->top != arrReg->GetNumberOfKeys()*2)
			KerMain->Errors->LogError(eKRTEPELoadingScripts);
	}
}




void CScriptLoader::CreateEmptyObjects() {
	CFSRegister *objReg = TryGetSubRegister(_code, "Objects");
	if (objReg) {
		for (CFSRegKey *key = objReg->GetFirstKey(); key; key = key->GetNextKey()) {
			CKerName *className = GetName(atoi(key->GetName()));
			if (!className || className->Type != eKerNTclass) {
				KerMain->Errors->LogError(eKRTEobjectLoad);
				_objects.push_back(RegObjT(key->GetSubRegister(), 0));
			} else {
			
				OPointer obj;
				if (className == KerMain->Static) {
					obj = KerMain->StaticData;
				} else {
					obj = KerMain->Construct(className);
				}

				_objects.push_back(RegObjT(key->GetSubRegister(), obj.KerObject()));
			}
		}
	}
}





void CScriptLoader::LoadDataObjects(CFSRegister *names) {
	if (names) {
		int f = 0;
		for(CFSRegKey *key = names->GetFirstKey(); key; key = key->GetNextKey(), f++) {
			CKerName *name = ksidNames[f];
			CFSRegister *nameReg = GetSubRegister(key);
			CFSRegister *dataReg = TryGetSubRegister(nameReg, "Data");
			CFSRegKey *classKey = nameReg->FindKey("Class");

			if (classKey) {
				CKerName *type = ReadKsid(nameReg, "Class");
				if (!name->DataObject) {
					new CDataObject(name, KerMain);
				}
				name->DataObject->AddClass(type);
				if (dataReg)
					ReadDataValues(dataReg, name->DataObject);
			}

		}
	}
}






void CScriptLoader::LoadDependencies() {
	CFSRegKey *key = _code->FindKey("Dependencies");
	if (key) {
		if (key->CFSGetKeyType() != FSRTint)
			KerMain->Errors->LogError(eKRTEPELoadingScripts);
		while (!key->eof()) {
			GetName(key->readi())->AddParent(GetName(key->readi()));
		}
	}
}



void CScriptLoader::LoadVariables(CFSRegister *reg, CKerObjectT *ot) {
	if (reg && reg->GetNumberOfKeys()) {
		ot->NumVars = reg->GetNumberOfKeys();
		ot->OVars = new CKerOVar[ot->NumVars];
		CKerOVar *currentVar = ot->OVars;
		CFSRegKey *key = reg->GetFirstKey();

		for ( ; key; key = key->GetNextKey(), currentVar++) {
			if (!TestFieldCache(key, currentVar))
				LoadField(key, currentVar);
		}
	}
}


void CScriptLoader::LoadStaticVariables(CFSRegister *reg, CKerObjectT *ot) {
	if (reg && reg->GetNumberOfKeys()) {
		ot->NumStaticVars = reg->GetNumberOfKeys();
		ot->StaticVars = new CKerField[ot->NumStaticVars];
		CKerField *currentVar = ot->StaticVars;
		CFSRegKey *key = reg->GetFirstKey();

		for ( ; key; key = key->GetNextKey(), currentVar++) {
			if (!TestFieldCache(key, currentVar))
				LoadField(key, currentVar);
		}
	}
}


void CScriptLoader::LoadMethods(CFSRegister *reg, CKerObjectT *ot) {
	if (reg && reg->GetNumberOfKeys()) {
		ot->NumM = reg->GetNumberOfKeys();
		ot->Methods = new CKerMethod[ot->NumM];
		CKerMethod *currentMethod = ot->Methods;
		CFSRegKey *key = reg->GetFirstKey();

		for ( ; key; key = key->GetNextKey(), currentMethod++) {
			if (!TestFieldCache(key, currentMethod)) {
				CFSRegister *reg2 = LoadField(key, currentMethod);
				currentMethod->Safe = ReadChar(reg2, "Safe");
				LoadParams(TryGetSubRegister(reg2, "Params"), currentMethod);
				currentMethod->Function = KerMain->KS->GetMethodPointer(currentMethod->FieldName);
				if (currentMethod->Function == 0)
					KerMain->Errors->LogError(eKRTEfunctionDoesntExist, 0, currentMethod->FieldName);
				if (!currentMethod->Safe)
					currentMethod->Name->DirectMethodPtr = currentMethod->Function;
			}
		}
	}
}


template <typename T>
T *CScriptLoader::TestFieldCache(CFSRegKey *key, T *currentField) {
	if (key->CFSGetKeyType() != FSRTregister)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	
	if (key->GetSubRegister()->GetNumberOfKeys() == 0) {
		FieldsCacheT::iterator i = _fieldCache.find(string(key->GetName()));
		if (i == _fieldCache.end())
			KerMain->Errors->LogError(eKRTEPELoadingScripts);
		T *ret = (T*)i->second;
		currentField->Assign(*ret);
		return ret;
	} else {
		_fieldCache[string(key->GetName())] = currentField;
		return 0;
	}
}



void CScriptLoader::LoadParams(CFSRegister *reg, CKerMethod *m) {
	if (reg && reg->GetNumberOfKeys()) {
		m->NumP = reg->GetNumberOfKeys();
		m->Params = new CKerParam[m->NumP];
		CKerParam *currentParam = m->Params;
		CFSRegKey *key = reg->GetFirstKey();

		int Offset = m->LT.SizeOf();
		for (; key; key = key->GetNextKey(), currentParam++) {			
			CFSRegister *prmReg = LoadField(key, currentParam);
			
			CFSRegKey *defKey = prmReg->FindKey("Default");
			if (defKey) {
				currentParam->DefaultValue = new UTempVar();
				ReadObjVar(currentParam->DefaultValue, currentParam->LT, defKey, currentParam->LT);
				if (currentParam->LT.DimCount && currentParam->DefaultValue->_pointer)
					KerMain->GarbageCollector->Hold((CKerArrBase*)currentParam->DefaultValue->_pointer);
			}
			
			currentParam->Offset = Offset;
			Offset += currentParam->LT.SizeOf();
		}
		m->ParamSize = Offset;
	} else {
		m->ParamSize = m->LT.SizeOf();
	}
}




CFSRegister * CScriptLoader::LoadField(CFSRegKey *key, CKerField *field) {
	field->FieldName = newstrdup(key->GetName());
	CFSRegister *reg = GetSubRegister(key);
	if (reg->FindKey("Ksid")) {
		field->Name = ReadKsid(reg);
	}
	if (reg->FindKey("LT")) {
		field->LT = ReadLT(reg);
	} else if (field->Name) {
		field->LT = field->Name->LT;
	}
	if (reg->FindKey("From")) {
		field->ParentObj = ReadKsid(reg, "From");
	}
	LoadAttributes(TryGetSubRegister(reg, "Attributes"), field->Attributes);
	return reg;
}



void CScriptLoader::LoadAttributes(CFSRegister *reg, CAttributes *&attributes) {
	if (reg && reg->GetNumberOfKeys()) {
		if (!attributes)
			attributes = new CAttributes();
		ReadDataValues(reg, attributes);
	}
}


void CScriptLoader::ReadDataValues(CFSRegister *reg, CDataObjectBase *dataObj) {
	for (CFSRegKey *key = reg->GetFirstKey() ; key; key = key->GetNextKey()) {
		int ordinal = atoi(key->GetName());
		CKerName *varName = GetName(ordinal);
		UTempVar var;
		ReadVar(&var, varName->LT, key, _originalKsidLts[ordinal-_nameStart]);
		
		CDataObjectField *field;
		if (field = dataObj->FindField(varName)) {
			if (varName->LT.DimCount == 0 && varName->LT.Type == eKTint && (varName->LT.Modifier & eKTMretOr)) {
				// combine ORable attribute flags
				int a = field->GetValue<int>() | var._int;
				field->Assign(a, KerMain);
			} else if (!field->Compare(var))
				KerMain->Errors->LogError(eKRTEconstantValueConflict, 0, field->Name->name);
		} else if (field = dataObj->AddField(varName)) {
			field->Assign(var, KerMain);
		} else {
			KerMain->Errors->LogError(eKRTEPELoadingScripts);
		}
	}
}



void CScriptLoader::ReadVar(UTempVar *dest, CLT &dType, CFSRegKey *source, CLT &sType) {
	KerMain->ClearParam(dest, dType);
	if (sType.DimCount > 0) {
		if (sType == dType || (sType.DimCount == dType.DimCount && (sType.Type == eKTname || sType.Type == eKTobject) && (dType.Type == eKTname || dType.Type == eKTobject))) {
			dest->_pointer = LoadArray(source->readi(), sType, true);
		} else {
			KerMain->Errors->LogError(eKRTEarrayconv);
		}
	} else if ((sType.Type == eKTname || sType == eKTobject) && dType.DimCount == 0) {
		CKerName *name = GetName(source->readi());
		if (dType.Type == eKTname || dType.Type == eKTobject) {
			dest->_pointer = name;
		} else {
			KerMain->Errors->LogError(eKRTEptrtonum);
		}
	} else {
		KerMain->ConvertParam(source->GetDirectAccessFromPos(), sType, dest, dType);
	}
}


void CScriptLoader::ReadObjVar(UTempVar *dest, CLT &dType, CFSRegKey *source, CLT &sType) {
	KerMain->ClearParam(dest, dType);
	if (sType.DimCount > 0) {
		if (sType == dType) {
			dest->_pointer = LoadArray(source->readi(), sType, false);
		} else {
			KerMain->Errors->LogError(eKRTEarrayconv);
		}
	} else {
		if (dType.DimCount != 0) {
			KerMain->Errors->LogError(eKRTEarrayconv);
		} else {
			switch (sType.Type) {
				case eKTname:
					if (dType.Type != eKTname) {
						KerMain->Errors->LogError(eKRTEptrconv);
					} else {
						dest->_pointer = GetName(source->readi());
					}
				case eKTobject:
					if (dType.Type != eKTobject) {
						KerMain->Errors->LogError(eKRTEptrconv);
					} else {
						int ordinal = source->readi();
						if (!ordinal) {
							dest->_pointer = 0;
						} else if (ordinal < _objectStart) {
							dest->_pointer = LoadDataObject(GetName(ordinal)).Cast(dType.ObjectType);
						} else if (ordinal < _arrayStart) {
							dest->_pointer = GetObject(ordinal).Cast(dType.ObjectType);
						} else {			
							KerMain->Errors->LogError(eKRTEptrconv);
						}
					}
				default:
					KerMain->ConvertParam(source->GetDirectAccessFromPos(), sType, dest, dType);
					source->pos++;
			}
		}
	}
}



void *CScriptLoader::LoadArray(int ordinal, CLT lt, bool isDataObject) {
	if (!ordinal)
		return 0;

	ordinal -= _arrayStart;
	if (ordinal < 0 || ordinal >= (int)_arrays.size())
		KerMain->Errors->LogError(eKRTEPELoadingScripts);

	CFSRegKey *typeKey = _code->FindKey("Arrays LTs");
	typeKey->pos = ordinal*2;
	if (lt != ReadLT(typeKey))
		KerMain->Errors->LogError(eKRTEPELoadingScripts);

	if (_arrays[ordinal].second)
		return _arrays[ordinal].second;

	void *ret = 0;
	if (lt.DimCount == 1) {
		switch (lt.Type) {
			case eKTchar:
				ret = CreateArray<wchar_t>(_arrays[ordinal].first);
				break;
			case eKTdouble:
				ret = CreateArray<double>(_arrays[ordinal].first);
				break;
			case eKTint:
				ret = CreateArray<int>(_arrays[ordinal].first);
				break;
			case eKTname:
			case eKTobject:
				ret = CreatePtrArray(_arrays[ordinal].first, lt, isDataObject);
				break;
			default:
				KerMain->Errors->LogError(eKRTEPELoadingScripts);				
		}
	} else {
		ret = CreatePtrArray(_arrays[ordinal].first, lt, isDataObject);
	}
	
	_arrays[ordinal].second = ret;
	return ret;
}



void *CScriptLoader::CreatePtrArray(CFSRegKey *key, CLT lt, bool isDataObject) {
	CKerArr<void*> *ret = new CKerArr<void*>(KerMain->GarbageCollector);	
	for (key->pos = 0; !key->eof(); ) {
		int ordinal = key->readi();
		void* val=0;
		
		if (ordinal == 0) {
			val = 0;
		} else if (ordinal < _objectStart) {
			if (lt.DimCount > 1 || (lt.Type != eKTname && lt.Type != eKTobject)) {
				KerMain->Errors->LogError(eKRTEptrconv);
			} else {
				if (isDataObject || lt.Type == eKTname) {
					val = GetName(ordinal);
				} else {
					val = LoadDataObject(GetName(ordinal)).Cast(lt.ObjectType);
				}
			}
		} else if (ordinal < _arrayStart) {
			if (lt.DimCount > 1 || lt.Type != eKTobject) {
				KerMain->Errors->LogError(eKRTEptrconv);
			} else {
				val = GetObject(ordinal).Cast(lt.ObjectType);
			}
		} else {
			if (lt.DimCount <= 1) {
				KerMain->Errors->LogError(eKRTEarrayconv);
			} else {
				val = LoadArray(ordinal, CLT(lt.Type, lt.Modifier, lt.ObjectType, lt.DimCount-1), isDataObject);
			}
		}
		ret->AddLast(val);
	}
	return ret;
}



template <typename T>
void *CScriptLoader::CreateArray(CFSRegKey *key) {
	CKerArr<T> *ret = new CKerArr<T>(KerMain->GarbageCollector);	
	for (key->pos = 0; !key->eof(); key->pos++) {
		ret->AddLast(*(T*)key->GetDirectAccessFromPos());
	}
	return ret;
}



void CScriptLoader::LoadConstants() {
	LoadObject(KerMain->StaticData.KerObject(), TryGetSubRegister(_code, "Static Constants"), eKOLTconstants);
}



OPointer CScriptLoader::LoadDataObject(CKerName *name) {
	if (!_dataObjectLoader)
		_dataObjectLoader = new CDataObjectLoader(KerMain);
	return _dataObjectLoader->Load(name);
}


void CScriptLoader::LoadObjects(eKerObjectLoadType objectLoadType) {
	for (ObjectsT::iterator i = _objects.begin(); i != _objects.end(); ++i) {
		LoadObject(i->second, TryGetSubRegister(i->first, "Data"), objectLoadType);
	}
}



void CScriptLoader::RunConstructors(eKerObjectLoadType objectLoadType) {
	if (_dataObjectLoader)
		_dataObjectLoader->RunConstructors();

	for (ObjectsT::iterator i = _objects.begin(); i != _objects.end(); ++i) {
		if (i->second) {
			try {
				KerMain->call(0, i->second->thisO, KerMain->KnownNamesPtrs[eKKNloadConstructor], 0); 
			} catch (CKernelError err) {
				KerMain->Errors->LogError(err.ErrorNum);
			}

			CKerName *constructor = objectLoadType == eKOLTlevel ? KerMain->KnownNamesPtrs[eKKNlevelLoaded] : KerMain->KnownNamesPtrs[eKKNgameLoaded];
			try {
				KerMain->call(0, i->second->thisO, constructor, 0); 
			} catch (CKernelError err) {
				KerMain->Errors->LogError(err.ErrorNum);
			}
		}
	}
}



void CScriptLoader::LoadObject(CKerObject *obj, CFSRegister *data, eKerObjectLoadType loadType) {
	if (!obj)
		return;

	CKerOVar *var = obj->Type->OVars;
	for (int f=0; f < obj->Type->NumVars; f++, var++) {
		
		if (var->LT.IsStaticConstant()) {
			if (loadType != eKOLTconstants)
				continue;
		} else {
			if (loadType == eKOLTconstants)
				continue;

			int varFlag = KerMain->ReadAttribute<int>(var, eKKNattrVarFlag);
			if (loadType == eKOLTgame) {
				if (varFlag & eKVFdontSaveToGame)
					continue;
			} else { // loading object from level
				if (varFlag & eKVFdontSaveToLevel)
					continue;
			}
		}

		int varOrdinal = GetNameOrdinal(var->Name);
		CFSRegKey *key = 0;
		if (varOrdinal && data) {
			char buffer[16];
			sprintf_s(buffer, "%i", varOrdinal);
			key = data->FindKey(buffer);
		}
		if (!key) {
			KerMain->Errors->LogError(eKRTEVarLoad, 0, var->Name->name);
		} else {
			ReadObjVar(&var->GetValue<UTempVar>(obj), var->LT, key, _originalKsidLts[varOrdinal - _nameStart]);
		}

	}
}



CFSRegister *CScriptLoader::GetSubRegister(CFSRegister* reg, const char *name) {
	return GetSubRegister(reg->FindKey(name));
}


CFSRegister *CScriptLoader::GetSubRegister(CFSRegKey* key) {
	if (key == 0 || key->CFSGetKeyType() != FSRTregister)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return key->GetSubRegister();
}


CFSRegister *CScriptLoader::TryGetSubRegister(CFSRegister* reg, const char *name) {
	CFSRegKey *key = reg->FindKey(name);
	if (key == 0 || key->CFSGetKeyType() != FSRTregister)
		return 0;
	return key->GetSubRegister();
}



int CScriptLoader::ReadInt(CFSRegister* reg, const char *name) {
	CFSRegKey *key = reg->FindKey(name);
	if (key == 0 || key->CFSGetKeyType() != FSRTint)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return key->readi();
}


char CScriptLoader::ReadChar(CFSRegister* reg, const char *name) {
	CFSRegKey *key = reg->FindKey(name);
	if (key == 0 || key->CFSGetKeyType() != FSRTchar)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return key->readc();
}



CLT CScriptLoader::ReadLT(CFSRegister* reg) {
	CFSRegKey *key = reg->FindKey("LT");
	if (key == 0 || key->CFSGetKeyType() != FSRTint)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return ReadLT(key);
}


CLT CScriptLoader::ReadLT(CFSRegKey* key) {
	unsigned int a = (unsigned int)key->readi();
	int b = key->readi();
	CLT ret((eKerTypes)((a>>16) & 0xFF), (eKerTypeModifier)(a & 0xFFFF), GetName(b), (a>>24) & 0xFF);
	if (ret.Type == eKTobject && !ret.ObjectType)
		ret.ObjectType = KerMain->Object;
	return ret;
}



CKerName* CScriptLoader::ReadKsid(CFSRegister* reg, const char *name) {
	CFSRegKey *key = reg->FindKey(name);
	if (key == 0 || key->CFSGetKeyType() != FSRTint)
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return GetName(key->readi());
}


CKerName* CScriptLoader::GetName(int index) {
	if (!index)
		return NULL;
	index -= _nameStart;
	if (index < 0 || (size_t)index >= ksidNames.size())
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	return ksidNames[index];
}

OPointer CScriptLoader::GetObject(int index) {
	if (!index)
		return NULL;
	index -= _objectStart;
	if (index < 0 || (size_t)index >= _objects.size())
		KerMain->Errors->LogError(eKRTEPELoadingScripts);
	CKerObject *obj = _objects[index].second;
	return obj ? obj->thisO : NULL;
}



int CScriptLoader::GetNameOrdinal(CKerName *name) {
	if (!name)
		return 0;

	if (!_areNamesOrdinalsCalculated) {
		int ordinal = _nameStart;
		for (vector<CKerName*>::iterator f = ksidNames.begin(); f != ksidNames.end(); ++f, ++ordinal) {
			if (*f)
				_namesOrdinlas[*f] = ordinal;
		}
		_areNamesOrdinalsCalculated = true;
	}

	NamesOrdinalsT::iterator i = _namesOrdinlas.find(name);
	return i == _namesOrdinlas.end() ? 0 : i->second;
}





///////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////



void CKernelLoader::Load() {

	CKernelParameters *params = KerMain->KernelParameters;
	const char *file = params->GetFileName();
	string file2;
	eKerFileType fileType = GetFileType(file);
	if (fileType == eKFTksid && TryGetFileAttribute(file, file2)) {
		file = file2.c_str();
		fileType = GetFileType(file);
	}

	// Find and open level if needed
	switch (fileType) {
		case eKFTlevel:
			_level = new CFSRegister(file, "KRKAL3 LEVEL");
			if (!(KerMain->GetRunMode() == eKRMEditor || KerMain->GetRunMode() == eKRMNormal))
				KerMain->Errors->LogError(eKRTEPELoadingScripts, (int)KerMain->GetRunMode(), "wrong RunMode or file type");
			if (KerMain->GetRunMode() == eKRMNormal)
				_levelFileToMove = file;
			break;
		case eKFTgame:
			_level = new CFSRegister(file, "KRKAL3 GAME");
			_objectLoadType = eKOLTgame;
			if (KerMain->GetRunMode() != eKRMNormal)
				KerMain->Errors->LogError(eKRTEPELoadingScripts, (int)KerMain->GetRunMode(), "wrong RunMode or file type");
			break;
		case eKFTdata:
			if (KerMain->GetRunMode() == eKRMDataEdit) {
				_level = new CFSRegister(file, "KRKAL3 DATA");
			} else {
				params->AddRootName(file);
			}
			break;
		case eKFTcode:
		case eKFTksid:
			params->AddRootName(file);
			break;
		case eKFTempty:
			break;
		default:
			KerMain->Errors->LogError(eKRTEPELoadingScripts, 0, "Unknown file type");
	}

	// check level integrity and load root files from it
	if (_level) {
		if (_level->GetOpenError() != FSREGOK)
			KerMain->Errors->LogError(eKRTEPELoadingLevel);
		
		CFSRegKey *mode = TryGetKey(_level, "Kernel Mode", FSRTint);
		if (mode) {
			eKerRunMode rm2 = (eKerRunMode)mode->readi();
			if (!(rm2 == KerMain->GetRunMode() || (rm2 == eKRMEditor && KerMain->IsGameMode()) || (rm2 == eKRMDataEdit && KerMain->GetRunMode() == eKRMDataOnly)))
				KerMain->Errors->LogError(eKRTEPELoadingScripts, (int)KerMain->GetRunMode(), "wrong RunMode or file type");
		}
		
		CFSRegKey *roots = TryGetKey(_level, "Source Project", FSRTstring);
		if (roots) {
			while (!roots->eof()) {
				params->AddRootName(roots->GetDirectAccessFromPos());
				roots->SetPosToNextString();
			}
		}
	}


	InitSourceFiles();

	KerMain->KernelParameters->FindCodeFile(KerMain);

}



bool CKernelLoader::TryGetFileAttribute(const char* inFile, string &outFile) {
	CRootNamesCriticalSection cs(CRootNames::GetRootNames(), true, KerMain);
	CKerMain *rkernel = CRootNames::GetRootNames()->GetKernel();
	CKerName *name = rkernel->KerNamesMain->GetNamePointer(inFile);
	if (name) {
		KString str = rkernel->ReadAttribute<KString>(name, eKKNattrFile);
		if (str && str->GetCount()) {
			char buffer[MAX_PATH];
			if (UnicodeToAnsi(buffer, sizeof(buffer), str->c_str())) {
				outFile = buffer;
				return true;
			}
		}
	}
	return false;
}


void CKernelLoader::InitSourceFiles() {
	CKernelParameters *params = KerMain->KernelParameters;
	params->InitDataFiles();

	bool first = true;
	for (int f=0; f<params->GetRootNamesCount(); f++) {
		if (GetFileType(params->GetRootName(f)) == eKFTksid) {
			vector<string> *files = 0;
			try {
				files = CRootNames::GetRootNames()->GetFiles(params->GetRootName(f), eKerNTrunSource, first, KerMain);
				first = false;
				if (!files) {
					KerMain->Errors->LogError(eKRTEPELoadingScripts, 0, "Unknown file type");
				} else {
					for (vector<string>::iterator i = files->begin(); i != files->end(); ++i) {
						params->AddDataFile(i->c_str(), KerMain->FS, KerMain);
					}
				}
			} catch (CKernelPanic) {
				SAFE_DELETE(files);
				KerMain->Errors->LogError(eKRTEPELoadingScripts, 0, "Error while retrieving data from root names");
			}
			SAFE_DELETE(files);
		} else {
			params->AddDataFile(params->GetRootName(f), KerMain->FS, KerMain);
		}
	}
}




bool CKernelLoader::InitSourceFiles(CKernelParameters *params, CFS *fs) {
	params->InitDataFiles();

	bool first = true;
	for (int f=0; f<params->GetRootNamesCount(); f++) {
		if (GetFileType(params->GetRootName(f)) == eKFTksid) {
			vector<string> *files = 0;
			try {
				files = CRootNames::GetRootNames()->GetFiles(params->GetRootName(f), eKerNTrunSource, first);
				first = false;
				if (!files) {
					return false;
				} else {
					for (vector<string>::iterator i = files->begin(); i != files->end(); ++i) {
						if (!params->AddDataFile(i->c_str(), fs)) {
							SAFE_DELETE(files);
							return false;
						}
					}
				}
			} catch (CKernelPanic) {
				SAFE_DELETE(files);
				return false;
			}
			SAFE_DELETE(files);
		} else {
			if (!params->AddDataFile(params->GetRootName(f), fs))
				return false;
		}
	}

	return true;
}



void CKernelLoader::LoadDataFiles() {
	for (CKernelParameters::UsedFilesT::iterator f = KerMain->KernelParameters->UsedFilesBegin(); f != KerMain->KernelParameters->UsedFilesEnd(); ++f) {
		if (f->second.Type == eKFTdata || f->second.Type == eKFTlevel) {
			KerMain->Errors->LogError(eKRTEloadingDataFile,0,f->second.OriginalName.c_str());
			CScriptLoader loader(KerMain, f->second.OriginalName.c_str(), f->second.Type == eKFTlevel ? CScriptLoader::eLMlevel : CScriptLoader::eLMdata);
			loader.Load();
		}
	}
}


void CKernelLoader::LoadLevelNamesAndDataObjects() {
	if (_level) {
		_levelLoader = new CScriptLoader(KerMain, 0, CScriptLoader::eLMlevel, _level);
		_levelLoader->Load();
	}
}


void CKernelLoader::MoveLevelToDataSources() {
	if (_levelFileToMove.size() > 0)
		KerMain->KernelParameters->AddRootName(_levelFileToMove.c_str());
}



CFSRegKey *CKernelLoader::TryGetKey(CFSRegister *reg, const char *name, int type) {
	CFSRegKey *ret = reg->FindKey(name);
	if (ret && ret->CFSGetKeyType() != type)
		KerMain->Errors->LogError(eKRTEPELoadingLevel);
	return ret;
}



CKernelLoader::~CKernelLoader() {
	SAFE_DELETE(_levelLoader);
	SAFE_DELETE(_level);
}


char * CKernelLoader::FileExtensions[MaxExtensions] = {
	0, 0, ".code", ".level", ".data", ".game", 0, 
};


eKerFileType CKernelLoader::GetFileType(const char *file) {
	if (!file || !*file)
		return eKFTempty;
	if (strncmp(file, "_KSID_", 6) == 0)
		return eKFTksid;
	if (strncmp(file, "_KSId_", 6) == 0)
		return eKFTksid;
	
	size_t len = strlen(file);
	string file2;
	if (file[len-1] == '$') {
		char *fp;
		CFS::GetFS()->GetFullPath(file, &fp);
		file2.assign(fp);
		SAFE_DELETE_ARRAY(fp);
		file = file2.c_str();
		len = file2.size();
	}
	
	for (int f=0; f<MaxExtensions; f++) {
		if (FileExtensions[f]) {
			const char *ptr = file + len;
			ptr -= strlen(FileExtensions[f]);
			if (ptr > file && _stricmp(ptr, FileExtensions[f])==0) {
				return (eKerFileType)f;
			}
		}
	}
	return eKFTunknown;
}








/////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////






OPointer CDataObjectLoader::Load(CKerName *name) {
	if (!name)
		return 0;

	if (!name->DataObject) {
		KerMain->Errors->LogError(eKRTEdataObjectDoesntExist, 0, name->name);
		return 0;
	}

	if (name->DataObject->Instance)
		return name->DataObject->Instance->thisO;

	OPointer ret = name->DataObject->Construct(this);
	_objs.push_back(ret);
	return ret;
}



void CDataObjectLoader::RunConstructors() {
	for ( vector<OPointer>::iterator i = _objs.begin(); i != _objs.end(); ++i) {
		if (*i) {
			try {
				KerMain->call(0, *i, KerMain->KnownNamesPtrs[eKKNloadConstructor], 0); 
			} catch (CKernelError err) {
				KerMain->Errors->LogError(err.ErrorNum);
			}

			try {
				KerMain->call(0, *i, KerMain->KnownNamesPtrs[eKKNdataLoaded], 0); 
			} catch (CKernelError err) {
				KerMain->Errors->LogError(err.ErrorNum);
			}
		}
	}
}
