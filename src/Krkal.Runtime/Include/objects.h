//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - O B J E C T S
///
///		Informace o Bezicich objektech. Pristup k jejich Metodam a Datum.
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef KEROBJECTS_H
#define KEROBJECTS_H

#include "LanguageType.h"
#include "GarbageCollector.h"

#include <hash_map>
#include <vector>

using namespace std;
using namespace stdext;

class CKerObjs;
//class CGEnElement;
//struct CKerAutoConnect;
class CKerName;
class CKerNamesMain;
class CKerMain;
class CKerObjectT;
class CKerObject;
class CAttributes;
class CDataObject;
union UTempVar;

#define SIZE_OF_OBJ_SYSTEM_AREA 3 // in pointers


///////////////////////////////////////////////////////////
///
///		POPIS TYPU OBJEKTU
///
///////////////////////////////////////////////////////////


struct KRKALRUNTIME_API CKerField {
	CKerField() { FieldName = 0; Name = 0; ParentObj = 0; Attributes=0; _deleteMembers=true;}
	void Assign(const CKerField &other);
	~CKerField();
	char *FieldName;	// text name of the field
	CKerName *Name;		// KSID jmeno
	CLT LT;			// typ
	CKerName *ParentObj; // odkud byla metoda zdedena?
	CAttributes *Attributes;
protected:
	bool _deleteMembers;
};

// Popis argumentu konkretni metody (u direct call argumenty nejsou popsany)
struct KRKALRUNTIME_API CKerParam : public CKerField {
	CKerParam() { Offset=0; DefaultValue=0;}
	~CKerParam();
	int Offset;			// Kde je argument umisten na zasobniku. pocita se relativne vzhledem k KerContext->Params
	UTempVar *DefaultValue;
};


// KOnkretni metoda konkretniho typu objektu
struct KRKALRUNTIME_API CKerMethod  : public CKerField {
	CKerMethod() { Safe=1; Function=0; Params=0; NumP=0; ParamCL=0; PCLnames=0; ParamSize=0; PCLstart = 0; PCLstop = 0; ParentObj=0;}
	void Assign(const CKerMethod &other);
	~CKerMethod();
	char Safe;			// Safe=1. Direct=2
	void (*Function)(); // Pointr na fci. Nenula pro kompilovane
	int NumP;			// Pocet argumentu
	int ParamSize; // Velikost Navratove hodnoty + Parametru na zasobniku (Nepocita se vel infa o parametrech)
	CKerParam ***ParamCL;	// seznam seznamu parametru, na ktere se prevede konkretni KSID jmeno (podle HT)
	CKerName **PCLnames;	// Perfektni Hashovaci tabulka. 0-nic, KSID jmeno - na teto pozici je toto jmeno
	int PCLstart, PCLstop;	// start - posud indexace v Hash tabulce (na jakem indexu HT zacina); stop - velikost HT. pokud jmeno je mimo meze, neni v HT
	CKerParam *Params;  // seznam argumentu
};


// Popis promenne o kterou se zajima kernal (editor)
struct KRKALRUNTIME_API CKerOVar : public CKerField {
	CKerOVar() { Offset=0;}
	int Offset;		// Variable offset. Where is the pointer to data? (its offset in array of pointers from main object entry point)
	int DataOffset; // Offset to data. In Bytes array from main object Entry point
	
	template <typename T>
	T &GetValue(CKerObject *obj) { 
		if (!obj)
			throw CKernelError(eKRTEaccessingNullObject);
		return *(T*)((BYTE*)(obj->thisO) + DataOffset);
	}

};




class CKerObjectT_BackEnd {
	typedef hash_map<CKerName*, int> _baseObjectsT;
	typedef hash_map<CKerName*, CKerOVar*> _variablesT;
public:
	typedef vector<CKerOVar*> _ptrAreaT;


	CKerObjectT_BackEnd(CKerObjectT & o, CKerMain &KerMain);
	int GetBaseObjOffset(CKerName* name) {
		_baseObjectsT::iterator i = _baseObjects.find(name);
		return i == _baseObjects.end() ? -1 : i->second;
	}
	CKerOVar *GetVariable(CKerName* name) {
		_variablesT::iterator i = _variables.find(name);
		return i == _variables.end() ? 0 : i->second;
	}
	_ptrAreaT::const_iterator begin() { return _ptrArea.begin();}
	_ptrAreaT::const_iterator end() { return _ptrArea.end();}
	int size() { return _ptrArea.size(); }
private:
	void AddBaseObj(CKerObjectT & o, CKerObjectT * base);

	_baseObjectsT _baseObjects;
	_variablesT _variables;
	_ptrAreaT _ptrArea;
};


// Popis objektoveho TYpu
class KRKALRUNTIME_API CKerObjectT {
friend CKerObjectT_BackEnd;
public:
CKerObjectT() { TotalSize=SIZE_OF_OBJ_SYSTEM_AREA*sizeof(void*); OffsetsArea=0; OffsetsAreaSize=0; Name=0; Methods=0; NumM=0; MethodCL=0; MCLnames=0; MCLstart=0; MCLstop=0; OVars=0; NumVars=0; NumStaticVars=0; StaticVars=0; EditTag=0; BackEnd=0; allVariables=0;}
	~CKerObjectT();
	void CalculateVariableOffsets(CKerMain &KerMain);
	int GetBaseObjOffset(CKerName* name) { return BackEnd->GetBaseObjOffset(name); }
	CKerOVar *GetVariable(CKerName* name) { return BackEnd->GetVariable(name); }

	CKerName *Name;			// Jmeno typu
	int EditTag;			// Edit tag - editovatelny, umistitelny, ...
	int NumM;				// Pocet metod
	CKerMethod ***MethodCL;	// seznam seznamu metod, na ktere se prevede konkretni KSID jmeno (podle HT)
	CKerName **MCLnames;	// Perfektni Hashovaci tabulka. 0-nic, KSID jmeno - na teto pozici je toto jmeno
	int MCLstart, MCLstop;	// start - posud indexace v Hash tabulce (na jakem indexu HT zacina); stop - velikost HT. pokud jmeno je mimo meze, neni v HT
	CKerMethod *Methods;    // pole metod objektu
	int NumVars;			// pocet promennych zakladniho typu, ke kterym ma kernel/editor pistup
	CKerOVar *OVars;		// pole promennych zakladniho typu
	int NumStaticVars;		// number of static varibales
	CKerField *StaticVars;	// array of static variables
	void (*InitFunction)(); // Pointr na fci. ktera inicilaizuje promenne

	int TotalSize;			// Celkova Velikost dat objektu 
	int *OffsetsArea;		// How will look pointers to data?
	int OffsetsAreaSize;	// size of OffsetsArea
private:
	CKerObjectT_BackEnd *BackEnd;
	CKerObjectT *allVariables;	// me or my ancestor, where are all my variables
};



//struct CKerOSRPointers {
//	CKerOSRPointers(CFSRegKey *_key, int _pos, OPointer _obj, CKerOSRPointers *_next) {
//		next = _next; pos = _pos; key = _key; obj = _obj;
//	}
//	CKerOSRPointers *next;
//	CFSRegKey *key;
//	OPointer obj;	// objekt u ktereho pointer je. Nula znamena globalni promennou
//	int pos;
//};


//struct CKerObjSaveRec {
//	CKerObjSaveRec();
//	~CKerObjSaveRec();
//	void AddPointer(CFSRegKey *key, int pos, OPointer obj) {	// prida pozici dalsiho pointeru na tento objekt.
//		pointers = new CKerOSRPointers(key,pos,obj,pointers);
//	}
//	CKerArrObject *SaveGraph;  // graf konstruovany pri sejvovani levlu. (pole hran, Null znamena, ze objekt neni zarazen do grafu)
//	CFSRegister *reg;			// sem se objekt sejvne
//	int Tag;
//	CKerOSRPointers *pointers;  // informace o tom kam se sejvly pointery na tento objekt.
//	char *GlobalVar;
//	char *shortcut;		// text s popiskem shortcutu (pro GameLoad) - nevlastnim to
//};




//////////////////////////////////////////////////////////////////////////
///
///		BEZICI INSTANCE OBJEKTU
///
//////////////////////////////////////////////////////////////////////////

class KRKALRUNTIME_API CKerObject : public CManagedObject {
friend CKerObjs;
friend CKerMain;
public:
	CKerObject(CKerName *type, CKerMain *KerMain);
	CKerObject(CKerObject *source, CKerMain *KerMain);
	virtual void MarkLinks(int mark, CGarbageCollector & collector); 
	int Compare(CKerObject *other);

	CKerObjectT *Type;		// Typ objektu - vzdy nastaveno (alespon na DefaultObject
	CKerName *DataObjectName;
	void** thisO;			// this
	bool lives;				
private:
	void AllocMemory();
	UC *AllocatedMemory;
protected:
	~CKerObject();
};





//////////////////////////////////////////////////////////////////////////
///
///		VYPOCET PERFEKTNI HASHOVACI TABULKY
///
//////////////////////////////////////////////////////////////////////////


/// Spojak seznamu metod (seznamy jsou v tomto spojaku poze docasne v prubehu vypoctu)
struct CKerMList {
	CKerMList(CKerMList *Next, int Pos, CKerMethod **Methods, CKerName *Name) {
		next=Next; pos=Pos; methods=Methods; name=Name;
	}
	int pos;			// pridelena pozice
	CKerName *name;		// jmeno (klic)
	CKerMList *next;
	CKerMethod **methods;  // seznam volanych metod
};


/// Spojak seznamu parametru (seznamy jsou v tomto spojaku poze docasne v prubehu vypoctu)
struct CKerPList {
	CKerPList(CKerPList *Next, int Pos, CKerParam **Params, CKerName *Name) {
		next=Next; pos=Pos; params=Params; name=Name;
	}
	int pos;			// pridelena pozice
	CKerName *name;		// jmeno (klic)
	CKerPList *next;
	CKerParam **params; // seznam parametru, kam budu predavat
};



///////////////////////////////////////////////////////////////////////////////
///		Trida, ktera pocita perfektni hashovani pro metody a parametry
///		Algoritmus: vezmu jmeno metody, ke kazdymu objektu vygeneruju
///		seznam metod KSM, ktere se pod timto jmenem maji zavolat (seznam muze byt prazdny)
///		Tyto seznamy hashuju (klic je KSID jmeno metody)
///		Hashovani: Najdu prvni pozici, kde jeste zadny objekt nema dany seznam,
///		na tuto pozici seznam ulozim, u jmena si zapomatuju pozici.
class CKerCalculatePHT {
public:
	CKerCalculatePHT(CKerMain *kerMain);		// inicializace pomocnych promennych
	~CKerCalculatePHT();	// dinicializace
	void calc_o();			// provede vypocet PHT pro metody objektu
	void calc_m();			// provede vypocet PHT pro parametry metod
private:
	void find_pos_o(int &pos);	// najdi kandidata na volnou pozici (zacina se od nuly (zadat), kdyz se pozice po druhem volani nezmeni, je volna
	void place_at_pos_o(int pos, CKerName *name);  // proda seznamy tohoto jmena na danou pozici (kazdy objekt siseznamy nyni pamatuje ve spojaku olist)
	CKerMethod ** CreateOMList(CKerName *name, CKerObjectT *o); // najdu na jake metody se jmeno pro dany objekt prevede. Funkce vraci pole s pointry na metodu, zakoncene nulou
	void find_pos_m(int &pos);  // pro parametry - na hledani pozice
	void place_at_pos_m(int pos, CKerName *name);  // umisteni na pozici, zarazeni do spojaku
	CKerParam ** CreateMPList(CKerName *name, CKerMethod *m);  // Nalezeni seznamu parametru, do kterych se dany parametr preda

	CKerMethod **ms;		// vsechny metody (vsech objektu)
	int NumM;				// pocet metod
	int *mstart, *mstop;	// pro kazdou metodu: Pozice prvniho prvku ze spojaku, velikost spojaku, kdyby byl v poli (pozice posledniho - pozice prvniho + 1)
	CKerPList **mlist;		// spojak pro kazdou metodu
	CKerParam ***mplist;	// Do tohoto pole prubezne pocitam seznamy parametru, na ktere se jmeno parametru prevadi
	int NumO;				// pocet objektu
	CKerObjectT *os;		// vsechny objekty
	int *ostart, *ostop;	// pro kazdy objekt: Pozice prvniho prvku ze spojaku, velikost spojaku, kdyby byl v poli (pozice posledniho - pozice prvniho + 1)
	CKerMList **olist;		// pro kazdy objekt jeho spojak
	CKerMethod ***omlist;   // Do tohoto pole prubezne pocitam seznamy metod, na ktere se jmeno metody prevadi
	CKerMain *KerMain;
};




//////////////////////////////////////////////////////////////////////
///
///		HLAVNI OBJEKT PRO PRACI S OBJEKTY
///
//////////////////////////////////////////////////////////////////////


class KRKALRUNTIME_API CKerObjs {
public:
	CKerObjs() { ObjectTypes = 0; NumObjectT = 0; DataObjects=0; DataObjectCount=0; _maxDataObject=0; }
	~CKerObjs();

	CKerObjectT *ObjectTypes;	// Pole vsech typu objektu
	int NumObjectT;				// Pocet typu Objektu

	CDataObject **DataObjects;
	int DataObjectCount;
	void AddDataObject(CDataObject *dataObject);
private:
	int _maxDataObject;
};

#endif
