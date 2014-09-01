//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - C K e r M a i n
///
///		Main class of the runtime
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __CKERMAIN_H__
#define __CKERMAIN_H__


#include "OPointer.h"
#include "KernelArray.h"
#include "NamePtr.h"
#include "interpret.h"
#include "CKernelParameters.h"
#include "DataObject.h"

class CKerMain;
class CKerName;
struct CKerMessage;
struct CKerContext;
class CKerErrors;
class CKerServices;
class CKerNamesMain;
class CFSRegister;
class CFSRegKey;
class CKerObject;
class CFS;
class CKerObjs;
struct CKerOVar;
struct CKerMethod;
class KsInterface;
class CQueryKsParameters;
class CGarbageCollector;
class CKerInterpret;
class CAttributes;
class CKerSaver;
class CScriptLoader;


typedef void (*SafeCallFunction)(CKerMain *KerMain);



class KRKALRUNTIME_API CKerMain {
friend CKerObject;
friend CKerMessage;
friend CKerContext;
friend CKerServices;
friend class CScriptLoader;
friend CDataObject;
//friend CKerLevelInfo;
public:
	CKerMain(CKernelParameters *parameters); // Inituje kernel
						// runmode muze byt napr EDITOR_RUN nebo NORMAL_RUN (hra)
						// DobugMode urcuje zpusob a mnozstvi logovani errorovych hlasek
	~CKerMain();	// Destruktor

	void Load(); // Loads script, Data files, level or saved game. Initializes and starts kernel. Can throw CKernalPanic exception.
	int Save(const char *file, eKerSavingFlags flags, ArrPtr<NamePtr> names = 0, ArrPtr<NamePtr> dependencies = 0, ArrPtr<NamePtr> dataObjects = 0);

	CKerErrors *Errors;		// Interface pro Errory	(Nemenit, je zkonstruovano pri startu kernelu)
	CGarbageCollector *GarbageCollector;
	IGarbageCollector *GetIGarbageCollector() { return GarbageCollector; }
	CKerObjs *Objs;			// Interface pro Objekty (Nemenit, je zkonstruovano pri startu kernelu)
	CKerNamesMain *KerNamesMain;  // Interface pro Jmena (Nemenit, je zkonstruovano pri startu kernelu)
	CKerServices *KerServices;
	CFS *FS;
	KsInterface *KS;
	CQueryKsParameters *QueryKsParameters;
	CKerContext *KerContext;
	CKerInterpret *KerInterpret;
	CKernelParameters *KernelParameters;
	CAttributes *Attributes; // global attributes


	// Parametr CodeLine je misto(radka), odkud je sluzba volana. Je pocitana relativne k dane metode. 
	// Nasledujicim funkcim se predava pro ladici ucely

	OPointer NewObject(int CodeLine, CKerName *ObjType); // vytvori novy objekt, zavola constructor
	OPointer GetDataObjectInstance(CKerName *name);
	void DeleteObject(int CodeLine, const OPointer &Object); // Zruseni objektu (pozivaji se zpravy. K zruseni dojde az pote, co se objekt prestane pouzivat) Vola se destruktor.

	int GetTime() { return Time; }  // Vrati cas od startu kernelu
	void TerminateKernel() { _TerminateKernel = 1; } // ukonci kernel na konci kola
	int GetLastSaveError() { return _lastSaveError;}
	eKerRunMode GetRunMode() { return RunMode; }
	int IsGameMode() { return (RunMode==eKRMNormal); }
	int IsDataOnlyMode() { return (RunMode == eKRMDataOnly); }
	int IsEditorMode() { return (RunMode==eKRMEditor); }
	bool RunGarbageCollector;

	void ResetTLSKerMain() { StaticKerMain = this;}


	
	// METODY PRO BEZPECNE VOLANI A POSILANI ZPRAV:
	// CodeLine - cislo radky, odkud se vola (pro ladeni), kdyz se CodeLine nezadava, predpokladam, ze je spravne nastavena v Kontextu
	// Object - volany objekt, Method - Volana metoda (jeji KSID jmeno), NumArgs - Pocet predavanych argumentu
	// when - cas kdy se ma zprava vyvolat viz. eKerCallTimes. pokud when==eKerCTcallend, tak callendObjekt urcuje
	//		objekt, po kterem se ma zprava vyvolat. Zprava se vyvola co nejdrive, ale az zkonci vsechny metody objektu callendObject
	//      Pokud when==eKerCTtimed, do CallendObject dej cas vyvolani
	// ArgType(s) - Typ argumentu - ten typ v jakem je argument pri volani (Kernel pak argument pripadne sam automaticky prevede na typ, jaky si konkretni metoda vyzaduje)
	// RetType - Typ, jaky chci, aby mi fce vratila (muzu nastavit na eKTvoid)
	// ArgName(s) - KSID Jmeno argumentu.
	// Arg - Argument (primo hodnota), u stringu a eKTret argumentu - pointr na argument)
	// ArgPointers - Pointery na argumenty
	void call(int CodeLine, const OPointer &Object,CKerName *Method,int NumArgs, ...); // int ArgType, CKerName *ArgName, Arg, ... 
	template <typename T> T call(int CodeLine, const OPointer &Object,CKerName *Method,CLT RetType, int NumArgs, ...); // int ArgType, CKerName *ArgName, Arg, ... 
	void message(int CodeLine, const OPointer &Object, CKerName *Method, int when, UI time, const OPointer &callendObject, int NumArgs, ...); // int ArgType, ..., CKerName *ArgName, ..., Arg, ...
	void ISmessage(const OPointer &Object, CKerName *Method, int when, UI time, const OPointer &callendObject, int NumArgs, int *ArgTypes, CKerName **ArgNames, void **ArgPointers);
	void IScall(OPointer Object, CKerName *Method, int NumArgs, CLT *ArgTypes, CKerName **ArgNames, void **ArgPointers, CLT RetType);  // navratova hodnota fce je ponechana na zasobniku

	void RunTurn(int time, int krize);	// Spusti Skripty na jedno kolo, cas je zadavan relativne od posledniho kola. MinTime <= time <= MaxTime
	void RunMessages(CKerMessage **startmq, CKerMessage **endmq); // Provede vsechny zpravy ze zadane fronty

	// Nasledujici funkce najdou vsechny zpravy ( ve vsech frontach), ktere odpovidaji popisu. Pokud nektery parametr je zadan jako null, tak na tomto parametru nezalezi, vyhleda se vse
	int FDeleteMessages(const OPointer &Reciever, CKerName *msg=0, OPointer Sender=0); // Funkce vyhleda a smaze zpravy ze vsech front. Vrati pocet smazanych zprav.
	int FCountMessages(const OPointer &Reciever, CKerName *msg=0, OPointer Sender=0); // Funkce spocita zpravy ve vsech frontach (vraci pocet).

	template<typename T>
	T ReadAttribute(CKerName *name, CKerName *attributeName);
	template<>
	CKerArr<wchar_t>* ReadAttribute<CKerArr<wchar_t>*>(CKerName *name, CKerName *attributeName);
	template<>
	KString ReadAttribute<KString>(CKerName *name, CKerName *attributeName);
	template<typename T>
	T ReadAttribute(CKerField *field, CKerName *attributeName);
	template<typename T>
	T ReadGlobalAttribute(CKerName *attributeName);

	template<typename T>
	T ReadAttribute(CKerName *name, eKerKnownNames attributeName) { return ReadAttribute<T>(name, KnownNamesPtrs[attributeName]); }
	template<typename T>
	T ReadAttribute(CKerField *field, eKerKnownNames attributeName) { return ReadAttribute<T>(field, KnownNamesPtrs[attributeName]); }
	template<typename T>
	T ReadGlobalAttribute(eKerKnownNames attributeName) { return ReadGlobalAttribute<T>(KnownNamesPtrs[attributeName]); }

	template<typename T>
	bool DataObjectTryRead(CKerName *dataObject, CKerName *varName, T &output);
	template<typename T>
	bool DataObjectTryWrite(CKerName *dataObject, CKerName *varName, T &input);
	template<typename T>
	bool DataObjectTryForcedWrite(CKerName *dataObject, CKerName *varName, T &input);

	CKerName *Constructor, *StaticConstructor, *CopyConstructor, *Destructor, *Object, *Static;  // Jmena zakladnich metod
	CKerName **KnownNamesPtrs;

	OPointer StaticData;


private:
	// Funkce pro Init a Load Kernelu:
	void LoadScriptOnly(const char *script, CScriptLoader *&scriptLoader); // Inits script dll and code file
	void GetReadyToStart(); // Pripravi naloadovanej Kernel K Behu scriptu
	void RunStaticConstructors();
	void CalculatePHT();
	void FindOrAddKnownNames();
	void SetConstructors(); // vola se pote, co jsou nactene jmena. Vyhleda jmena typu Constructor a ulozi je do promenych
	void ReloadTranslations();
	void LoadEngineName();
	
	eKerRunMode RunMode; // 0 - Normal Run, 1 - Editor Run
	eKerDebugModes DebugMode; //
	int _RunTurn; // Nastaveno na 1, jestlize je Kernel volan pres funkci RunTurn, jinak 0
	int _TerminateKernel; // Kdyz je 1 Kernel se na konci kola pomoci vyjimky CKernelPanic radne ukonci
	unsigned int Time; // cas od zacatku levlu
	int TimeCrisis; // 1 - Pokud jsem v casove krizi (nestiham), 0 - OK
	int ScriptActivity; // Pro sledovani vykonu. Kolik volani se uskutecnilo v tomto kole
	CKerSaver *_saving;
	int _lastSaveError;

	CKerMessage *startmq[4];  // Fronty zprav. 0 - eKerCTmsg, 1 - eKerCTend, 2 - eKerCTnext, 3 - eKerCTnextend
	CKerMessage *endmq[4];
	CKerMessage *timedmsgs;   // Casovane zpravy (nepotrebuju konec)
	void DeleteMessages(CKerMessage **startmq, CKerMessage **endmq); // Vyprazdni frontu zprav, aniz zpravy provede.
	void PopTimedMessages(); // Prida casovane zpravy na ktere uz prisel cas od aktualni fronty zprav.
	int FindCountDeleteMessages(CKerMessage **startmq, CKerMessage **endmq, const OPointer &Reciever, CKerName *msg, const OPointer &Sender, int Delete = 0); // Vyhleda zadane zpravy ve fronte, spocita je a pripadne smaze

	void PassParams(CKerMethod *method, CKerMessage *msg); // Pomocna rutina - predani Argumentu
	void SetDefaultParams(CKerMethod *method);			   // Pomocna rutina - nazadane argumenty nastavi na jejich defaultni hodnotu
	void ClearParam(void *Dest, CLT DType);
	void ConvertParam(void *Source, CLT SType, void *Dest, CLT DType); // Automaticka konverze zakladniho typu
	void ConvertParamRet(void *Source, CLT SType, void *Dest, eKerTypes DType, eKerTypeModifier RetFce); // Automaticka konverze zakladniho typu. Vystup se prida k vystupu pomoci returnovaci funkce
	void FillRetFceDefault(void *Dest, CLT DType, int RetFce, int ErrorNum); // vola se v pripade ze nebylo nic predano. Pokud je tam korektni retFce, preda se default, jinak nahlasi chybu ErrorNum
	void Destruct(const OPointer &ObjPtr); // Provede dealokaci objektu (volano pote, co probehly destruktory)
	OPointer Construct(CKerName *className);

	template<typename T>
	bool TryReadAttribute(CKerName *attributeName, CAttributes *attributes, T &output);
	bool TryReadUserNameOrComment(CKerName *name, bool isComment, KString &output);


	// Citace pro kontrolu priliz mnoha zasobnikovych volani a priliz mnoha zprav:
	int _message_counter, _message_counter_warning;  // Pocet vnorenych volani. A zda uz byl hlasen warning
	int _call_stack_counter, _call_stack_counter_warning; // Pocet nevyrizenych zprav. A zda uz byl hlasen warning

	HMODULE KsDllHandle;



private:
		/// NOT USED
// public:
//	void LoadLevel(char *level); // Naloaduje Kernel - volat po konstruktoru, uz muzou vznikat vyjimky CKernalPanic
//	int LoadGame(CFSRegister *lev, CFSRegKey *LevelFile, CFSRegKey *ScriptsFile); // Nahraje ulozenou hru  - volat po konstruktoru, uz muzou vznikat vyjimky CKernalPanic
//	int SaveLevel(); // sejvne Level s vyuzitim LevelInfa. 1 - OK, 0 - chyba;
//	int GetOpenError() {return OpenError;} // 0 - OK, 1 a vic Error, vraci eKerOLErrors
//	void SaveGame(char *file=0) { SAFE_DELETE_ARRAY(_FileToSaveGame); _SaveGame = 1; if (file) _FileToSaveGame = newstrdup(file);} // sejvne hru na konci kola


	// pro pristup k objektovym promennym:
	//void *GetAccessToVar(OPointer obj, CKerOVar* Var); // vraci pointr na promennou objektu (pozor nici kontext!) kdyz promenna neni vraci null
	//void *GetAccessToVar(OPointer obj, int VarNum); // vraci pointr na promennou objektu (pozor nici kontext!) kdyz promenna neni vraci null
	//void *GetAccessToVar(CKerObject *kobj, CKerOVar* Var); // vraci pointr na promennou objektu (pozor nici kontext!) kdyz promenna neni vraci null
	//void *GetAccessToVar(CKerObject *kobj, int VarNum); // vraci pointr na promennou objektu (pozor nici kontext!) kdyz promenna neni vraci null
	//CKerObject * GetAccessToObject(OPointer obj); // Nastavi pristup k promennym objektu (pozor nici kontext!) Kdyz objekt neexistuje, vraci null
	//CKerObject * GetAccessToObject(CKerObject *kobj) { // Nastavi pristup k promennym objektu (pozor nici kontext!)
	//	if (kobj->Type->SetObjectVar) kobj->Type->SetObjectVar(kobj->KSVG);	return kobj;
	//}
	//void *GetAccessToVar2(CKerObject *kobj, CKerOVar* Var) { // vraci pointr na promennou objektu, napred je treba volat fci GetAccesToObject
	//	if (!Var->KSVar) {	assert(kobj->ISData); return kobj->ISData + Var->Offset; } else return *(Var->KSVar);
	//}
	//void *GetAccessToVar3(CKerObject *kobj, int VarNum) { // vraci pointr na promennou objektu, napred je treba volat fci GetAccesToObject, vrati NULL, kdyz objekt promennou nema
	//	if (!kobj->Type->SpecificKnownVars) return 0;
	//	CKerOVar *Var = kobj->Type->SpecificKnownVars[VarNum]; if (!Var) return 0;
	//	if (!Var->KSVar) {	assert(kobj->ISData); return kobj->ISData + Var->Offset; } else return *(Var->KSVar);
	//}

	//int NumGV;				// pocet globalnich prom.		
	//CKerOVar *GlobalVar;	// Informace o typech a umistenich globalch promennych
	//CKerOVar **SpecificKnownVars;	// pole pointru na OVars, jejichz vyznamy Kernel zna - pole muze byt prazdne nebo ma velikost KERVARUSESSIZE, undexuje se pomoci eKarVarUses
	//int StaticObjectsCount; // pocet statickych objektu
	
	//CKerGarbageCollector GarbageCollector;



	//private:
	int LoadScriptsCode(CFSRegister *code);
	int LoadLevel2(CFSRegister *lev);

	
	void LLoadVariable(CFSRegister *r, CKerOVar *OV, UC *offset); // Paokud najde v registru promennou, tak ji nahraje
	void LSaveVariable(CFSRegister *r, CKerOVar *OV, UC *offset, OPointer thisO); // Sejvne promennou do registru
	int LLoadGlobals(CFSRegKey *r); // Nahraje hodnoty globalnich promennych z levlu, 1 OK, 0 chyba 
	int LLoadObjects(CFSRegKey *r); // Nahraje objekty z levlu a vytvori je, 1 OK, 0 chyba 
	void LSaveObjects();	// sejvne objekty do nezarazenych registru.
	int LSSortGraph(OPointer obj, OPointer parent, CFSRegister *sreg, int &count); // trideni grafu DFS, objekty predavoji sve sejvovaci egistry na vystup.
	int LLoadObject(CFSRegister *r, CKerName *ObjType, OPointer *StaticVar=0); // Nahraje objekt z levlu a vytvori ho, ptr objektu OK, 0 chyba , -1 zahodil jsem shortcut
	int GLoadKernelState(CFSRegister *lev); // nahraje stav kenelu a zpravy - pro GameLoad
	int GLoadMessageQueue(CFSRegKey *k2, int typ); // nahraje frontu zprav daneho typu
	void GSaveKernelState(CFSRegister *lev); // ulozi stav kenelu a zpravy - pro GameLoad
	void GSaveMessageQueue(CFSRegKey *k2, CKerMessage *mq); // ulozi frontu zprav daneho typu
	void GSaveGame(); // ulozi hru do pripravenoho registru
	void GSaveObjects(CFSRegister *objs); //ulozi vsechny objekty

//	int OpenError; // Podarilo se kernel nahrat?
//	int SaveState; // zda sejvuju/loaduju. viz eSaveloadState
//	CFSRegister *SaveReg;  // registr, ve kterem maji volane funkce otevirat streamy k save/load. Nastaven jenkdyz je to povoleno, jinak null!

	//int _SaveGame; // zda mam na konci kola sejvnout hru.
	//char *_FileToSaveGame; // jemno souboru, kam se bude sejvovat.

};




// Mokro ulozi aktualni radku do kontextu (pro ladeni, pro logovani chyb)
#define SET_CODE_LINE(cline) {if (KerContext) KerContext->line=(cline);}
// Mokro zjisti aktualni radku z kontextu (pro ladeni, pro logovani chyb)
#define GET_CODE_LINE (KerContext?KerContext->line:0)






/////////////////////////////////////////////////////////////////
//	Prime volani, kdyz chci vratit typ.
//	fce prevede promenny pocet argumentu do formy jakou chce IScall a zavola ji
template <typename T>
T CKerMain::call(int CodeLine, const OPointer &Object,CKerName *Method,CLT RetType, int NumArgs, ...) { // int ArgType, CKerName *ArgName, Arg, ... 
	ResetTLSKerMain();
	va_list list;
	int f;
	CLT *ArgTypes=0;
	CKerName **ArgNames=0;
	void **ArgPointers=0;
	CStackDealocator StackPushSize(KerInterpret);


	SET_CODE_LINE(CodeLine);
	if (NumArgs) {
		ArgTypes = KerInterpret->Push<CLT>(StackPushSize, NumArgs);
		ArgNames = KerInterpret->Push<CKerName *>(StackPushSize, NumArgs);
		ArgPointers = KerInterpret->Push<void *>(StackPushSize, NumArgs);

		va_start( list, NumArgs ); 
		for (f=0;f<NumArgs;f++) {
			ArgTypes[f] = va_arg(list,CLT);
			ArgNames[f] = va_arg(list,CKerName*);
			if (ArgTypes[f].IsRet()) {
				ArgPointers[f] = va_arg(list, void*); 
			} else if (ArgTypes[f].DimCount > 0) {
				ArgPointers[f] = &va_arg(list, void*);
			} else {
				switch (ArgTypes[f].Type) {
					case eKTchar: ArgPointers[f] = &va_arg(list, wchar_t); break;
					case eKTdouble: ArgPointers[f] = &va_arg(list, double); break;
					default:	ArgPointers[f] = &va_arg(list, int); break;
				}
			}
		}
		va_end(list);
	}

	IScall(Object,Method,NumArgs,ArgTypes,ArgNames,ArgPointers,RetType);

	assert(sizeof(T) == RetType.SizeOf());
	return KerInterpret->Pop<T>();
}




template<typename T>
bool CKerMain::TryReadAttribute(CKerName *attributeName, CAttributes *attributes, T &output) {
	if (!attributeName)
		return false;
	assert(sizeof(T) == attributeName->LT.SizeOf());
	if (!attributes)
		return false;
	CDataObjectField *field = attributes->FindField(attributeName);
	if (!field)
		return false;
	output = field->GetValue<T>();
	return true;
}



template<typename T>
T CKerMain::ReadAttribute(CKerName *name, CKerName *attributeName) {
	ResetTLSKerMain();
	T res = 0;
	if (name) {
		TryReadAttribute<T>(attributeName, name->Attributes, res);
	}
	return res;
}


template<>
KString CKerMain::ReadAttribute<KString>(CKerName *name, CKerName *attributeName) {
	ResetTLSKerMain();
	KString res;
	if (name) {
		if (attributeName == KnownNamesPtrs[eKKNattrUserName] && TryReadUserNameOrComment(name, false, res)) {
			return res;
		}
		if (attributeName == KnownNamesPtrs[eKKNattrComment] && TryReadUserNameOrComment(name, true, res)) {
			return res;
		}
		if (!TryReadAttribute<KString>(attributeName, name->Attributes, res)) {
			if (attributeName == KnownNamesPtrs[eKKNattrUserName]) {
				return KString(name->ToShortString());
			}
		}
	}
	return res;
}


template<>
CKerArr<wchar_t>* CKerMain::ReadAttribute<CKerArr<wchar_t>*>(CKerName *name, CKerName *attributeName) {
	return ReadAttribute<KString>(name, attributeName);
}


template<typename T>
T CKerMain::ReadAttribute(CKerField *field, CKerName *attributeName) {
	ResetTLSKerMain();
	T res = 0;
	if (field) {
		TryReadAttribute<T>(attributeName, field->Attributes, res);
	}
	return res;
}


template<typename T>
T CKerMain::ReadGlobalAttribute(CKerName *attributeName) {
	ResetTLSKerMain();
	T res = 0;
	TryReadAttribute<T>(attributeName, Attributes, res);
	return res;
}




template<typename T>
bool CKerMain::DataObjectTryRead(CKerName *dataObject, CKerName *varName, T &output) {
	ResetTLSKerMain();
	if (!dataObject || !varName || !dataObject->DataObject)
		return false;
	if (dataObject->DataObject->Instance)
		return ((OPointer)dataObject->DataObject->Instance->thisO).TryRead<T>(varName, output);
	
	CDataObjectField *field = dataObject->DataObject->FindField(varName);
	if (!field)
		return false;
	output = field->GetValue<T>();
	return true;
}

template<typename T>
bool CKerMain::DataObjectTryWrite(CKerName *dataObject, CKerName *varName, T &input) {
	ResetTLSKerMain();
	if (!dataObject || !varName || !dataObject->DataObject)
		return false;
	if (dataObject->DataObject->Instance)
		return ((OPointer)dataObject->DataObject->Instance->thisO).TryWrite<T>(varName, input);
	
	CDataObjectField *field = dataObject->DataObject->FindField(varName);
	if (!field)
		return false;
	field->Assign(input, this);
	return true;
}


template<typename T>
bool CKerMain::DataObjectTryForcedWrite(CKerName *dataObject, CKerName *varName, T &input) {
	ResetTLSKerMain();
	if (!dataObject || !varName || !dataObject->DataObject)
		return false;
	if (dataObject->DataObject->Instance)
		return ((OPointer)dataObject->DataObject->Instance->thisO).TryWrite<T>(varName, input);
	
	CDataObjectField *field = dataObject->DataObject->FindField(varName);
	if (!field) {
		field = dataObject->DataObject->AddField(varName);
		if (!field)
			return false;
	}
	field->Assign(input, this);
	return true;
}


#endif