//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - O B J E C T S
///
///		Informace o Bezicich objektech. Pristup k jejich Metodam a Datum.
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////



#include "stdafx.h"
#include "objects.h"
#include "KerServices.h"
#include "fs.api.h"
#include <vector>
#include "DataObject.h"

using namespace std;




/////////////////////////////////////////////////////////////

CKerField::~CKerField() {
	if (_deleteMembers) {
		SAFE_DELETE_ARRAY(FieldName);
		SAFE_DELETE(Attributes);
	}
}


void CKerField::Assign(const CKerField &other) {
	_deleteMembers = false;
	FieldName = other.FieldName;
	Name = other.Name;
	LT = other.LT;
	ParentObj = other.ParentObj;
	Attributes = other.Attributes;
}




/////////////////////////////////////////////////////////////


CKerMethod::~CKerMethod() { 
	int f;
	if (_deleteMembers)
		SAFE_DELETE_ARRAY(Params);
	if (ParamCL) { 
		for (f=0;f<PCLstop;f++) SAFE_DELETE_ARRAY(ParamCL[f]);
		SAFE_DELETE_ARRAY(ParamCL);
		SAFE_DELETE_ARRAY(PCLnames);
	}
}


void CKerMethod::Assign(const CKerMethod &other) {
	CKerField::Assign(other);
	Safe = other.Safe;
	Function = other.Function;
	NumP = other.NumP;
	ParamSize = other.ParamSize;
	Params = other.Params;
}



CKerParam::~CKerParam() {
	SAFE_DELETE(DefaultValue);
}



/////////////////////////////////////////////////////////////




CKerObjectT::~CKerObjectT() { 
	int f;
	SAFE_DELETE_ARRAY(Methods);
	if (MethodCL) { 
		for (f=0;f<MCLstop;f++) SAFE_DELETE_ARRAY(MethodCL[f]);
		SAFE_DELETE_ARRAY(MethodCL);
		SAFE_DELETE_ARRAY(MCLnames);
	}
	SAFE_DELETE_ARRAY(OVars);
	SAFE_DELETE_ARRAY(StaticVars);
	SAFE_DELETE_ARRAY(OffsetsArea);
	SAFE_DELETE(BackEnd);
}


void CKerObjectT::CalculateVariableOffsets(CKerMain &KerMain) {
	BackEnd = new CKerObjectT_BackEnd(*this, KerMain);

	int Offset = BackEnd->size() * sizeof(void*);
	OffsetsAreaSize = BackEnd->size();

	for (int i=0; i<NumVars; i++) {
		OVars[i].DataOffset = Offset;
		Offset += OVars[i].LT.SizeOf();
	}

	TotalSize = Offset + SIZE_OF_OBJ_SYSTEM_AREA * sizeof(void*); 

	OffsetsArea = new int[OffsetsAreaSize];
	int f=0;
	CKerObjectT_BackEnd::_ptrAreaT::const_iterator iter = BackEnd->begin();
	for ( ; iter != BackEnd->end(); ++iter, f++) {
		if (*iter) {
			OffsetsArea[f] = (**iter).DataOffset;
		} else {
			OffsetsArea[f] = 0;
		}
	}
}



CKerObjectT_BackEnd::CKerObjectT_BackEnd(CKerObjectT & o, CKerMain &KerMain) {
	// I assume that the names are topologically sorted by compiler. Kernel them orders them in reverse !!
	CKerNameList *nList = KerMain.KerNamesMain->Names;
	while (nList && nList->name != o.Name)
		nList = nList->next;
	if (nList)
		nList = nList->next; // skip myself

	for ( ; nList; nList = nList->next) {
		if (nList->name->Type == eKerNTclass && nList->name->Compare(o.Name) == 1) {
			//nList->name is base of o
			if (GetBaseObjOffset(nList->name) == -1) {
				AddBaseObj(o, nList->name->ObjectType);
			}
		}
	}

	size_t ancestorsSize = _ptrArea.size();

	if (ancestorsSize == 0)
		_ptrArea.push_back(0); // main entry point
	_baseObjects[o.Name] = 0; // add myself


	for (int i = 0; i<o.NumVars; i++) {
		if (!GetVariable(o.OVars[i].Name)) {
			o.OVars[i].Offset = _ptrArea.size();
			_ptrArea.push_back(o.OVars+i);
			_variables[o.OVars[i].Name] = o.OVars+i;
		}
	}

	if (ancestorsSize < _ptrArea.size())
		o.allVariables = &o;	// I have my own variables

}



void CKerObjectT_BackEnd::AddBaseObj(CKerObjectT & o, CKerObjectT * base) {
	int allVarsOffset;
	if (base->NumVars == 0) {

		_baseObjects[base->Name] = 0;

	} else if ((allVarsOffset = GetBaseObjOffset(base->allVariables->Name)) != -1) {
		// I found that I have all base's variables

		// search for base objects I don't have, but are in the base
		_baseObjectsT::const_iterator iter = base->BackEnd->_baseObjects.begin();
		_baseObjectsT::const_iterator end = base->BackEnd->_baseObjects.end();
		for ( ; iter != end; ++iter) {
			if (GetBaseObjOffset(iter->first) == -1) {
				_baseObjects[iter->first] = iter->second + allVarsOffset;
			}
		}

	} else {

		if (o.allVariables == 0) {
			o.allVariables = base->allVariables;
		} else {
			if ((allVarsOffset = base->BackEnd->GetBaseObjOffset(o.allVariables->Name)) != -1) {

				// rearange offsets
				_baseObjectsT::iterator iter = _baseObjects.begin();
				_baseObjectsT::iterator end = _baseObjects.end();
				for ( ; iter != end; ++iter) {
					iter->second = allVarsOffset + iter->second;
				}

				_ptrArea.clear();
				_variables.clear();
				o.allVariables = base->allVariables;
			} else {
				o.allVariables = &o;
			}
		}

		// search for variavles I don't have, but are in the base
		for (int i = 0; i<o.NumVars; i++) {
			CKerOVar *vInBase;
			if (!GetVariable(o.OVars[i].Name) && (vInBase = base->GetVariable(o.OVars[i].Name))) {
				o.OVars[i].Offset = vInBase->Offset + _ptrArea.size();
				_variables[o.OVars[i].Name] = o.OVars+i;
			}
		}

		{
			// search for base objects I don't have, but are in the base
			_baseObjectsT::const_iterator iter = base->BackEnd->_baseObjects.begin();
			_baseObjectsT::const_iterator end = base->BackEnd->_baseObjects.end();
			for ( ; iter != end; ++iter) {
				if (GetBaseObjOffset(iter->first) == -1) {
					_baseObjects[iter->first] = iter->second + _ptrArea.size();
				}
			}
		}

		{
			// extend ptrArea
			_ptrAreaT::const_iterator iter = base->BackEnd->_ptrArea.begin();
			_ptrAreaT::const_iterator end = base->BackEnd->_ptrArea.end();
			for ( ; iter != end; ++iter) {
				CKerOVar *var = *iter;
				if (var != 0) {
					var = GetVariable(var->Name);
					assert(var != 0);
				}
				_ptrArea.push_back(var);
			}
		}

	}
}




///////////////////////////////////////////////////////////////////
///
///		C K e r O b j e c t   - (instance objektu)
///
///////////////////////////////////////////////////////////////////


////////////////////////////////////////////////////////////////////////////////
/// Vytvoreni nove instance objektu, alokace, prideleni OPointru, pridani do Hashovaci tabulky
CKerObject::CKerObject(CKerName *type, CKerMain *KerMain) : CManagedObject(*KerMain->GarbageCollector) 
{
	
	if (!type||type->Type!=eKerNTclass) { 
		Type=KerMain->Object->ObjectType; 
		KerMain->Errors->LogError(eKRTEInvalidObjType);
	} else Type=type->ObjectType;

	lives = true;
	DataObjectName = 0;

	AllocMemory();

	int offset = sizeof(void*) * (SIZE_OF_OBJ_SYSTEM_AREA + Type->OffsetsAreaSize);
	int restSize = Type->TotalSize - offset;
	if (restSize > 0)
		memset(AllocatedMemory + offset, 0, restSize);
}




CKerObject::CKerObject(CKerObject *source, CKerMain *KerMain) : CManagedObject(*KerMain->GarbageCollector) 
{
	Type = source->Type;
	lives = true;
	DataObjectName = 0;

	AllocMemory();

	int offset = sizeof(void*) * (SIZE_OF_OBJ_SYSTEM_AREA + Type->OffsetsAreaSize);
	int restSize = Type->TotalSize - offset;
	if (restSize > 0)
		memcpy(AllocatedMemory + offset, source->AllocatedMemory + offset, restSize);
}



void CKerObject::AllocMemory() {
	AllocatedMemory = new UC[Type->TotalSize];
	void **memory = (void**)AllocatedMemory;
	memory[0] = this;
	memory[1] = Type;
	memory[2] = Type->Name;
	memory += SIZE_OF_OBJ_SYSTEM_AREA;
	thisO = memory;

	for (int i=0; i<Type->OffsetsAreaSize; i++) {
		memory[i] = (UC*)memory + Type->OffsetsArea[i];
	}
}


int CKerObject::Compare(CKerObject *other) {
	int offset1 = sizeof(void*) * (SIZE_OF_OBJ_SYSTEM_AREA + Type->OffsetsAreaSize);
	int restSize1 = Type->TotalSize - offset1;
	int offset2 = sizeof(void*) * (SIZE_OF_OBJ_SYSTEM_AREA + other->Type->OffsetsAreaSize);
	int restSize2 = other->Type->TotalSize - offset2;

	if (restSize1 != restSize2)
		return (restSize1 - restSize2);

	return memcmp(AllocatedMemory + offset1, other->AllocatedMemory + offset2, restSize1);
}




/// Odstraneni objektu (uvolneni pameti, odebrani s Hash tabulek)
CKerObject::~CKerObject() {
	SAFE_DELETE_ARRAY(AllocatedMemory);
}



void CKerObject::MarkLinks(int mark, CGarbageCollector & collector) {
	for (int f=0; f < Type->NumVars; f++) {
		CKerOVar *var = Type->OVars + f;
		if (var->LT.DimCount > 0) {
			OPointer(thisO).get<ArrPtr<int>>(var->Offset).GCMarkMe(mark, collector);
		} else if (var->LT.Type == eKTobject) {
			OPointer(thisO).get<OPointer>(var->Offset).GCMarkMe(mark, collector);
		}
	}
}




//////////////////////////////////////////////////////////////////////
//CKerObjSaveRec::CKerObjSaveRec() { 
//	reg=0; Tag=0; SaveGraph= new CKerArrObject();
//	pointers = 0;
//	GlobalVar = 0;
//	shortcut=0;
//}
//
//CKerObjSaveRec::~CKerObjSaveRec() {
//	CKerOSRPointers *p1 = pointers, *p2;
//	SAFE_DELETE(reg);
//	SAFE_DELETE(SaveGraph);
//	while (p1) {
//		p2 = p1;
//		p1 = p1->next;
//		delete p2;
//	}
//}



/// Odebere objekt z Hashovaci tabulky





///////////////////////////////////////////////////////////////////
///
///		C K e r O b j s   - HLAVNI OBJEKT PRO PRACI S OBJEKTY
///
///////////////////////////////////////////////////////////////////






CKerObjs::~CKerObjs() {
	SAFE_DELETE_ARRAY(ObjectTypes);
	for (int f=0; f < DataObjectCount; f++) {
		delete DataObjects[f];
	}
	SAFE_DELETE_ARRAY(DataObjects);
}



void CKerObjs::AddDataObject(CDataObject *dataObject) {
	if (_maxDataObject <= DataObjectCount) {
		int newmax = _maxDataObject * 2 > 16 ? _maxDataObject * 2 : 16;
		CDataObject **newObjs = new CDataObject*[newmax];
		if (DataObjectCount)
			memcpy(newObjs, DataObjects, DataObjectCount * sizeof(CDataObject*));
		SAFE_DELETE_ARRAY(DataObjects);
		DataObjects = newObjs;
		_maxDataObject = newmax;
	}
	DataObjects[DataObjectCount] = dataObject;
	DataObjectCount++;
}









//////////////////////////////////////////////////////////////////////////////////
/// Nahraje informace o jedne promenne
//int CKerObjs::LoadOVar(CFSRegKey *_km, CKerOVar *OV, CKerOVar ***SpecKnownVars, int GLOBAL) {
	//CFSRegister *km = _km->GetSubRegister();
	//CFSRegKey *l;
	//CKSKSOV *KSOV;
	//OV->NameStr = newstrdup(_km->GetName());
	//OV->Type = km->FindKey("Type")->readi();
	//if (OV->Type != eKTvoid && OV->Type != eKTstruct) {
	//	if (!GLOBAL) {
	//		if (l=km->FindKey("Offset")) {
	//			OV->Offset = l->readi();
	//			OV->KSVar = 0; // promenna je interpretovana
	//		} else {
	//			// promenna je kompilovana -> Najdu promennou
	//			KSOV = (CKSKSOV*)(KSMain->KSOVs->Member(_km->GetName()));
	//			if (KSOV) OV->KSVar = KSOV->Variable; else RETERR;
	//		}
	//		if (l=km->FindKey("Auto Control Name")) OV->Name = KerInterpret->GetNamesArray()[l->readi()];
	//	} else if (OV->Type == eKTobject) {
	//		if (l=km->FindKey("Object")) OV->Name = KerInterpret->GetNamesArray()[l->readi()];
	//	}
	//} 
	//if (l=km->FindKey("Use")) OV->Use = l->readi();
	//if (l=km->FindKey("Array Size")) OV->ArraySize = l->readi();

	//int a = OV->Use >> KERVARUSESSHIFT;
	//if (a) {
	//	if (!*SpecKnownVars) { // nasel jsem prvni znamou promennou -> vytvorim pole a vynuluju ho:
	//		int ii;
	//		*SpecKnownVars = new CKerOVar*[KERVARUSESSIZE];
	//		for (ii=0; ii<KERVARUSESSIZE; ii++) (*SpecKnownVars)[ii] = 0;
	//	}
	//	(*SpecKnownVars)[a ] = OV; // Na sparavnou pozici do pole dam pointr na znamou promennou
	//	if (KerVarUsesInfo[a].PozorPozor) OV->Use |= eKVUBpozorPozor;
	//	if (KerVarUsesInfo[a].SpecialEdit) OV->Use |= eKVUBspecialEdit;
	//	OV->EditType = KerVarUsesInfo[a].EditType;
	//	OV->NamesMask = KerVarUsesInfo[a].NamesMask;
	//}

	//// tagy pro editor
	//if (l=km->FindKey("Edit Type")) OV->EditType = l->readi();
	//if (l=km->FindKey("User Name")) OV->UserName = newstrdup(l->GetDirectAccess());
	//if (l=km->FindKey("Comment")) OV->Comment = newstrdup(l->GetDirectAccess());

	//if (OV->EditType == eKETscripted) {
	//	if (!(l=km->FindKey("Method")) || OV->Type != eKTvoid) RETERR
	//	OV->Name = KerInterpret->GetNamesArray()[l->readi()];
	//	OV->Use = eKVUBeditable;
	//	if (l=km->FindKey("Param")) OV->ItemID = l->readi();
	//	return 1;
	//}

	//if (l=km->FindKey("Names Mask")) OV->NamesMask = l->readi();
	//if (l=km->FindKey("Default")) {
	//	OV->DefaultValue = new CKerValue;
	//	if (!LoadObjectsLD(l,OV->DefaultValue,OV->Type)) RETERR
	//}
	//if (OV->Type>=eKTarrChar && OV->Type<eKTstring && (l=km->FindKey("Default Member"))) {
	//	OV->DefaultMember = new CKerValue;
	//	if (!LoadObjectsLD(l,OV->DefaultMember,OV->Type-eKTarrChar)) RETERR
	//}
	//if (l=km->FindKey("Limits")) OV->LimintsCfg = l->readi();
	//l=0;  // nacteni intervalu hodnot
	//l=km->FindKey("Interval");
	//if (!l) l=km->FindKey("List");
	//if (!l) l=km->FindKey("Limit Name");
	//if (l) {
	//	int f;
	//	if (OV->Type >= eKTstring) { // zjistim pocet stringu
	//		OV->LimitsListCount = 0;
	//		while (!l->eof()) { OV->LimitsListCount++; l->SetPosToNextString();}
	//		l->pos = 0;
	//	} else OV->LimitsListCount = l->top;
	//	OV->LimitsList = new CKerValue[OV->LimitsListCount];
	//	int typ2 = OV->Type;
	//	if (typ2>=eKTarrChar && typ2<eKTstring) typ2-=eKTarrChar;
	//	if (typ2==eKTobject) typ2=eKTname;
	//	for (f=0; f<OV->LimitsListCount; f++) {
	//		if (!LoadObjectsLD(l,OV->LimitsList+f,typ2)) RETERR
	//	}
	//} else if (a = (OV->Use >> KERVARUSESSHIFT)) {
	//	// zadani mezi pro zname promenne
	//	if (a>=eKVUcellz && a<=eKVUcellr) {
	//		OV->LimintsCfg = eKLCup;
	//		OV->LimitsListCount = 1;
	//		OV->LimitsList = new CKerValue[OV->LimitsListCount];
	//		OV->LimitsList[0].typ = eKTchar;
	//		OV->LimitsList[0].Dchar = 0;
	//	}
	//}
//	return 1;
//}


/// Vytvori Dvojnika objektoveho jmena pro automatismy
//CKerName *CKerObjs::DuplicateObjName(CKerName *name) {
//	char *dvojce = newstrdup(name->GetNameString());
//	dvojce[3] = 'A';
//	CKerName *ret = new CKerName(dvojce,KerMain->KerNamesMain);
//	ret->Type = eKerNTobjectShadow;
//	delete[] dvojce;
//	return ret;
//}


//////////////////////////////////
// Inicializuje AutoVars
//void CKerObjectT::FindAutoVars() {
//	int f,count;
//	NumAVars=0;
//	for (f=0;f<NumVars;f++) if (OVars[f].Use & eKVUBauto) NumAVars++;
//	if (NumAVars) {
//		AutoVars = new CKerOVar*[NumAVars];
//		count=0;
//		for (f=0;f<NumVars;f++) if (OVars[f].Use & eKVUBauto) {
//			AutoVars[count] = OVars+f;
//			count++;
//		}
//	}
//}


/////////////////////////////////////////////////////////////////////
///		NAHRAJE POPIS OBJEKTU (typu) DO KERNELU
//int CKerObjs::LoadObjects(CFSRegister *o) {
	//int f, g, NumM, h, NumP, NumOV;
	//int PrmOfs;
	//CFSRegKey *_ko,*l, *_km, *_kp;
	//CFSRegister *ko, *km, *kp;
	//CKSKSVG *ksKSVG;
	//CKerMethod *M;
	//CKerParam *P;
	//if (!o) RETERR
	//NumObjectT = o->GetNumberOfKeys()+1;		// pocet objektu
	//ObjectTypes = new CKerObjectT[NumObjectT];  // alokuju pole pro informaci o vsech typech o.
	//
	//DefaultObjectT = &ObjectTypes[0];			// sam vytvorim defaultni (prazdny objekt)
	//DefaultObjectT->Name = KnownNames[eKKNdefaultObject].Name;
	//DefaultObjectT->AName = DuplicateObjName(DefaultObjectT->Name);
	//DefaultObjectT->Name->ObjectType = &ObjectTypes[0];
	//_ko = o->GetFirstKey();
	//for (f=1;f<NumObjectT;f++) {
	//	if(!(ko = _ko->GetSubRegister())) RETERR
	//	// nactu jmeno (z i te pozice)
	//	ObjectTypes[f].Name = KerInterpret->GetNamesArray()[ko->FindKey("Name")->readi()];
	//	ObjectTypes[f].AName = DuplicateObjName(ObjectTypes[f].Name);
	//	ObjectTypes[f].Name->Type=eKerNTclass;
	//	ObjectTypes[f].Name->ObjectType = &ObjectTypes[f];
	//	ObjectTypes[f].EditTag = ko->FindKey("Edit Tag")->readi(); // load edit tagu
	//	if (l=ko->FindKey("User Name")) ObjectTypes[f].Name->UserName = ObjectTypes[f].UserName = newstrdup(l->GetDirectAccess());
	//	if (l=ko->FindKey("Comment")) ObjectTypes[f].Name->Comment = ObjectTypes[f].Comment = newstrdup(l->GetDirectAccess());
	//	// nahraju informace o alokaci kompilovanych promennych
	//	if (l=ko->FindKey("KSVG")) {
	//		ksKSVG = (CKSKSVG*)(KSMain->KSVGs->Member(l->GetDirectAccess()));
	//		ObjectTypes[f].AllocKSVG = ksKSVG->AllocKSVG;
	//		ObjectTypes[f].SetObjectVar = ksKSVG->SetObjectVar;
	//		ObjectTypes[f].KSVGsize = ksKSVG->Size;
	//	}
	//	if (l=ko->FindKey("IS D Size")) ObjectTypes[f].ISDataSize=l->readi(); // interpretovanych

	//	// nacteni popsanych dat
	//	if (l=ko->FindKey("Data")) {
	//		_km = l->GetSubRegister()->GetFirstKey();
	//		NumOV = ObjectTypes[f].NumVars = l->GetSubRegister()->GetNumberOfKeys();
	//		ObjectTypes[f].OVars = new CKerOVar[NumOV];
	//		for (g=0;g<NumOV;g++) {
	//			if (!LoadOVar(_km,&(ObjectTypes[f].OVars[g]),&(ObjectTypes[f].SpecificKnownVars))) return 0;
	//			_km = _km->GetNextKey();
	//		}
	//		if (!CheckVarGroups(&ObjectTypes[f].OVars,NumOV)) RETERR
	//		ObjectTypes[f].FindAutoVars();
	//	}

	//	// metody
	//	if (l=ko->FindKey("Methods")) {
	//		_km = l->GetSubRegister()->GetFirstKey();
	//		NumM = ObjectTypes[f].NumM = l->GetSubRegister()->GetNumberOfKeys();
	//		ObjectTypes[f].Methods = new CKerMethod[NumM];
	//		for (g=0;g<NumM;g++) {
	//			PrmOfs = 0;
	//			km = _km->GetSubRegister();
	//			M = &(ObjectTypes[f].Methods[g]);
	//			M->Compiled = km->FindKey("Compiled")->readc();
	//			M->Safe = km->FindKey("Safe")->readc();
	//			M->MethodName = new char[strlen(_km->GetName())+1]; // nactu jmeno fce pro ladici ucely
	//			strcpy(M->MethodName,_km->GetName());
	//			if (M->Compiled) {
	//				M->Function = ((CKSKSM*)KSMain->KSMs->Member(_km->GetName()))->Fce;
	//			} else {
	//				M->Jump = km->FindKey("Jump")->readi();
	//			}
	//			if (l=km->FindKey("Parent Object")) M->ParentObj = KerMain->KerNamesMain->GetNamePointer(l->GetDirectAccess());
	//			if (M->Safe) {
	//				if (l=km->FindKey("Name")) {
	//					M->Name = KerInterpret->GetNamesArray()[l->readi()];
	//					M->Name->Type=eKerNTmethod;
	//				} else RETERR
	//				if (l=km->FindKey("ReturnType")) {
	//					M->ReturnType = l->readi();
	//					PrmOfs+=KerMain->GetTypeSize(M->ReturnType);
	//				}
	//				if (l=km->FindKey("Params")) {
	//					_kp = l->GetSubRegister()->GetFirstKey();
	//					NumP = M->NumP = l->GetSubRegister()->GetNumberOfKeys();
	//					M->Params = new CKerParam[NumP];
	//					for (h=0;h<NumP;h++) {
	//						kp = _kp->GetSubRegister();
	//						P = &(M->Params[h]);
	//						if (l=kp->FindKey("Name")) {
	//							P->Name = KerInterpret->GetNamesArray()[l->readi()];
	//							P->Name->Type=eKerNTparam;
	//						} else RETERR
	//						P->Type = kp->FindKey("Type")->readi();
	//						P->Offset = PrmOfs;
	//						PrmOfs += KerMain->GetTypeSize(P->Type);
	//						if (!LoadObjectsLD(kp->FindKey("Default"),&(P->DefaultValue),P->Type)) RETERR
	//						_kp = _kp->GetNextKey();
	//					}
	//				}
	//			} else { // direct:
	//				if (M->Compiled) if (l=km->FindKey("DirectName")) {
	//					*(((CKSKSDM*)KSMain->KSDMs->Member(l->GetDirectAccess()))->Fce) = M->Function;
	//				} else RETERR
	//			}
	//			M->ParamSize = PrmOfs;
	//			_km = _km->GetNextKey();
	//		}
	//	}
	//	
	//	_ko=_ko->GetNextKey();
	//}
//	return 1;
//}


///////////////////////////////////////////////////////////////////////
/// Pomocna funkce pro nahrani defaultnich hodnot predavanych argumentu





/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
///
///			C K e r C a l c u l a t e P H T
///
/////////////////////////////////////////////////////////////////////////////

// Inicializace pomocnych promennych
CKerCalculatePHT::CKerCalculatePHT(CKerMain *kerMain) {
	KerMain = kerMain;

	int f,g,pos;
	// metody Objektu
	NumO = KerMain->Objs->NumObjectT;
	os = KerMain->Objs->ObjectTypes;
	ostart = new int[NumO]; for (f=0;f<NumO;f++) ostart[f]=0;
	ostop = new int[NumO]; for (f=0;f<NumO;f++) ostop[f]=0;
	olist = new CKerMList*[NumO]; for (f=0;f<NumO;f++) olist[f]=0;
	omlist = new CKerMethod**[NumO];

	// parametry Metod
	NumM=0;
	for (f=0;f<NumO;f++) NumM += os[f].NumM; // spocitam metody
	ms = new CKerMethod*[NumM];
	pos=0;
	for (f=0;f<NumO;f++) {  // najdu vsechny metody, pointry na ne si dam k sobe do pole
		for (g=0;g<os[f].NumM;g++) ms[pos+g]= os[f].Methods+g;
		pos+=os[f].NumM;
	}

	mstart = new int[NumM]; for (f=0;f<NumM;f++) mstart[f]=0;
	mstop = new int[NumM]; for (f=0;f<NumM;f++) mstop[f]=0;
	mlist = new CKerPList*[NumM]; for (f=0;f<NumM;f++) mlist[f]=0;
	mplist = new CKerParam**[NumM];
}



/////////////////////////////////////////////////////////////////
// najdu na jake metody se jmeno pro dany objekt prevede. 
// Funkce vraci pole s pointry na metodu, zakoncene nulou
CKerMethod ** CKerCalculatePHT::CreateOMList(CKerName *name, CKerObjectT *o) {
	int f,g;
	CKerMethod **ret;
	int NumM=o->NumM;
	if (NumM<=0) return 0;
	int *stav = new int[o->NumM];
	for (f=0;f<NumM;f++) {
		if (o->Methods[f].Name&&(o->Methods[f].Name==name||name->Compare(o->Methods[f].Name)>=2)) stav[f]=1; else stav[f]=0;
	}
	for (f=0;f<NumM;f++) for (g=0;g<NumM;g++) {
		if (stav[f]&&stav[g]&&o->Methods[f].Name->Compare(o->Methods[g].Name)==1) stav[f]=2;
	}
	g=0;
	for (f=0;f<NumM;f++) if (stav[f]==1) g++;
	if (!g) {
		delete[] stav;
		return 0;
	}
	ret = new CKerMethod*[g+1];
	g=0;
	for (f=0;f<NumM;f++) if (stav[f]==1) {
		ret[g]=o->Methods+f;
		g++;
	}
	ret[g]=0;
	delete[] stav;
	return ret;
}



///////////////////////////////////////////////////////////////
// najdi kandidata na volnou pozici (zacina se od nuly (zadat), 
// kdyz se pozice po druhem volani nezmeni, je volna
void CKerCalculatePHT::find_pos_o(int &pos) {
	int f;
	CKerMList *ml;
	for (f=0;f<NumO;f++) { // objekty projdu jenom jednou, kdyz je nekde pozice obsazena, zvysim ji 
		ml = olist[f];
		if (omlist[f]&&ostart[f]<=pos&&ostart[f]+ostop[f]>pos) {
			while (ml&&ml->pos<pos) ml=ml->next;
			while (ml&&ml->pos==pos) { pos++; ml=ml->next;}
		}
	}
	// vysledna pozice nemusi sedet pro vsechny objekty, pokud se nekde zmenila
}



//////////////////////////////////////////////////////////////////////
// proda seznamy tohoto jmena na danou pozici (kazdy objekt siseznamy nyni pamatuje ve spojaku olist)
void CKerCalculatePHT::place_at_pos_o(int pos, CKerName *name) {
	int f;
	CKerMList **ml;
	for (f=0;f<NumO;f++) {
		ml = olist+f;
		if (omlist[f]) { // zda se musi neco pridavat
			while (*ml&&(*ml)->pos<pos) ml=&(*ml)->next; // najdu v seznamu misto, kam mam pridat
			*ml = new CKerMList(*ml,pos,omlist[f],name);
			if (ostop[f]==0) { // tvoreni noveho sezname
				ostart[f]=pos;
				ostop[f]=1;
			} else	if (pos<ostart[f]) { // pridani pred zacatek
				ostop[f]+=(ostart[f]-pos);
				ostart[f] = pos;
			} else if (pos>=ostart[f]+ostop[f]) ostop[f]=pos-ostart[f]+1; // pridani za konec
		}
	}
}



////////////////////////////////////////////////////
// provede vypocet PHT pro metody objektu
void CKerCalculatePHT::calc_o() {
	CKerNameList *name = KerMain->KerNamesMain->Names;
	int f;
	int pos, opos;
	while (name) {
		if(name->name->Type==eKerNTsafeMethod || name->name->Type==eKerNTstaticSafeMethod) {  // pro kazde jmeno metody
			for (f=0;f<NumO;f++) omlist[f] = CreateOMList(name->name,os+f);  // spocitam k objektum seznamy
			pos = 0;  // hledam od pozice 0
			do {
				opos=pos;
				find_pos_o(pos);
			} while (pos!=opos);  // najdu prvni pozici, ktera je u vsech objektu volna (pozice se uz nemeni)
			place_at_pos_o(pos,name->name);  // aktualizuju spojaky
			name->name->KerPHTpos=pos;       // u jmena si zapamatuju pozici
		}
		name = name->next;
	}
}



////////////////////////////////////////////////////////////////////////////



//////////////////////////////////////////////////
// Nalezeni seznamu parametru, do kterych se dany parametr preda
// seznam je zakoncen Nulou
CKerParam ** CKerCalculatePHT::CreateMPList(CKerName *name, CKerMethod *m) {
	int f,g;
	CKerParam **ret;
	int NumP=m->NumP;
	if (NumP<=0) return 0;
	if (!m->Safe) return 0;
	int *stav = new int[NumP];
	for (f=0;f<NumP;f++) {
		if (m->Params[f].Name==name||name->Compare(m->Params[f].Name)>=2) stav[f]=1; else stav[f]=0;
	}
	for (f=0;f<NumP;f++) for (g=0;g<NumP;g++) {
		if (stav[f]&&stav[g]&&m->Params[f].Name->Compare(m->Params[g].Name)==1) stav[f]=2;
	}
	g=0;
	for (f=0;f<NumP;f++) if (stav[f]==1) g++;
	if (!g) {
		delete[] stav;
		return 0;
	}
	ret = new CKerParam*[g+1];
	g=0;
	for (f=0;f<NumP;f++) if (stav[f]==1) {
		ret[g]=m->Params+f;
		g++;
	}
	ret[g]=0;
	delete[] stav;
	return ret;
}


////////////////////////////////////////////////////////////////////////
// pro parametry - na hledani pozice
void CKerCalculatePHT::find_pos_m(int &pos) {
	int f;
	CKerPList *ml;
	for (f=0;f<NumM;f++) {		// metody projdu jenom jednou, kdyz je nekde pozice obsazena, zvysim ji 
		ml = mlist[f];
		if (mplist[f]&&mstart[f]<=pos&&mstart[f]+mstop[f]>pos) {
			while (ml&&ml->pos<pos) ml=ml->next;
			while (ml&&ml->pos==pos) { pos++; ml=ml->next;}
		}
	}
	// vysledna pozice nemusi sedet pro vsechny objekty, pokud se nekde zmenila
}



///////////////////////////////////////////////////////////////////
// umisteni na pozici, zarazeni do spojaku
void CKerCalculatePHT::place_at_pos_m(int pos, CKerName *name) {
	int f;
	CKerPList **ml;
	for (f=0;f<NumM;f++) {
		ml = mlist+f;
		if (mplist[f]) {  // zda se musi neco pridavat
			while (*ml&&(*ml)->pos<pos) ml=&(*ml)->next;  // najdu pozici ve spojaku, kam mam pridavat
			*ml = new CKerPList(*ml,pos,mplist[f],name);
			if (mstop[f]==0) {		// spojak byl prazdny
				mstart[f]=pos;
				mstop[f]=1;
			} else	if (pos<mstart[f]) {  // pridavam pred zacatek
				mstop[f]+=(mstart[f]-pos);
				mstart[f] = pos;
			} else if (pos>=mstart[f]+mstop[f]) mstop[f]=pos-mstart[f]+1;  // pridavam za konec
		}
	}
}



///////////////////////////////////////////////////
// provede vypocet PHT pro parametry metod
void CKerCalculatePHT::calc_m() {
	CKerNameList *name = KerMain->KerNamesMain->Names;
	int f;
	int pos, opos;
	while (name) {
		if(name->name->Type==eKerNTparam) {  // pro kazde jmeno parametru
			for (f=0;f<NumM;f++) mplist[f] = CreateMPList(name->name,ms[f]);  // k metodam vytvorim prislusne seznamy
			pos = 0;		// hledam volnou pozici (od 0)
			do {
				opos=pos;
				find_pos_m(pos);
			} while (pos!=opos);  // najdu prvni pozici, ktera je u vsech metod volna (to je kdyz se pozice prestane menit)
			place_at_pos_m(pos,name->name);		// aktualizuju spojaky
			name->name->KerPHTpos=pos;
		}
		name = name->next;
	}
}


//////////////////////////////////////////////////////////////////////////////////



/////////////////////////////////////////////////////////////////////////
///		Prevedeni spojaku na pole. Ulozeni tecto poli, startu a endu 
///		k objektum a metodam, uvolneni pomocnych promennych
CKerCalculatePHT::~CKerCalculatePHT() {
	int f,g;
	CKerMList *ml, *ml2;
	CKerPList *pl, *pl2;
	// pro vsechny objekty
	for (f=0;f<NumO;f++) {
		if (ostop[f]>0) {
			os[f].MethodCL = new CKerMethod**[ostop[f]];	// tvorim pole se seznamy
			os[f].MCLnames = new CKerName*[ostop[f]];		// tvorim pole se jmeny prislusejicim seznamu
			ml = olist[f];
			for (g=0;g<ostop[f];g++) {				// okopiruju a znicim spojak
				if(ml&&ml->pos==ostart[f]+g) {
					os[f].MethodCL[g] = ml->methods;
					os[f].MCLnames[g] = ml->name;
					ml2=ml;
					ml=ml->next;
					delete ml2;
				} else {
					os[f].MethodCL[g]=0;
					os[f].MCLnames[g]=0;
				}
			}
			os[f].MCLstart = ostart[f];
			os[f].MCLstop = ostop[f];
		} else {  // spojak byl prazdny
			os[f].MCLstart = 0;
			os[f].MCLstop = 0;
		};
	}
	SAFE_DELETE_ARRAY(ostart);
	SAFE_DELETE_ARRAY(ostop);
	SAFE_DELETE_ARRAY(olist);
	SAFE_DELETE_ARRAY(omlist);

	// pro vsechny metody
	for (f=0;f<NumM;f++) {
		if (mstop[f]>0) {
			ms[f]->ParamCL = new CKerParam**[mstop[f]];		// tvorim pole se seznamy
			ms[f]->PCLnames = new CKerName*[mstop[f]];		// tvorim pole s prislusejicimi jmeny
			pl = mlist[f];
			for (g=0;g<mstop[f];g++) {			// okopiruju spojak do pole a znicim ho
				if(pl&&pl->pos==mstart[f]+g) {
					ms[f]->ParamCL[g] = pl->params;
					ms[f]->PCLnames[g] = pl->name;
					pl2=pl;
					pl=pl->next;
					delete pl2;
				} else {
					ms[f]->ParamCL[g]=0;
					ms[f]->PCLnames[g]=0;
				}
			}
			ms[f]->PCLstart = mstart[f];
			ms[f]->PCLstop = mstop[f];
		} else {	// spojak byl prazdny
			ms[f]->PCLstart = 0;
			ms[f]->PCLstop = 0;
		}
	}
	SAFE_DELETE_ARRAY(mstart);
	SAFE_DELETE_ARRAY(mstop);
	SAFE_DELETE_ARRAY(mlist);
	SAFE_DELETE_ARRAY(mplist);
	SAFE_DELETE_ARRAY(ms);
}




