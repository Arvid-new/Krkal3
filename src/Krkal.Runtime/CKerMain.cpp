//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - C K e r M a i n
///
///		Main class of the runtime
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "CKerMain.h"
#include "KerContext.h"
#include "KerServices.h"
#include "fs.api.h"
#include "Loading.h"
#include "DataObject.h"
#include "Saving.h"
#include "RootNames.h"


/*__declspec( thread )*/ CKerMain *StaticKerMain;



CKerMain::CKerMain(CKernelParameters *parameters) {
	// Inicalizace glob. promenne KerMain
	//if (KerMain) throw  CExc(eKernel,0,"Error - Pokus Vytvorit Objekt KerMain dvakrat!");
	//KerMain=this;

	if (!parameters)
		throw CKernelPanic();
	KernelParameters = parameters;
	startmq[0] = 0; startmq[1] = 0; startmq[2] = 0; startmq[3] = 0;
	endmq[0] = 0; endmq[1] = 0; endmq[2] = 0; endmq[3] = 0;
	timedmsgs = 0;
	RunMode = KernelParameters->Runmode;
	DebugMode = KernelParameters->Debugmode;
	Time = 0;
	TimeCrisis = 0;
	_TerminateKernel = 0;
	RunGarbageCollector = false;
	_RunTurn = 0;
	_message_counter = 0; _message_counter_warning = 0;
	_call_stack_counter = 0; _call_stack_counter_warning = 0;
	Constructor=0; CopyConstructor=0; Destructor=0; Object=0; StaticConstructor=0;

	Errors=0; KerNamesMain=0; FS=0; GarbageCollector=0; QueryKsParameters=0;
	Objs = 0; KerContext=0; KerInterpret=0; KerServices=0; KS=0;
	KnownNamesPtrs = 0; Attributes=0;
	_saving=0; _lastSaveError=0;

	if (sizeof(OPointer) != 4 || sizeof(NamePtr) != 4)
		throw CKernelPanic();
	if (sizeof(ArrPtr<OPointer>) != 4 || sizeof(ArrPtr<int>) != 4 || sizeof(ArrPtr<wchar_t>) != 4 || sizeof(ArrPtr<wchar_t,2>) != 4)
		throw CKernelPanic();

	// Inicializace dalsich casti kernelu:
	ResetTLSKerMain();
	CFS::InitFS();
	FS = CFS::GetFS();
	FS->AddVersionFeature(evfAll, 0, "Krkal", 1);
	Errors = new CKerErrors(this, DebugMode);
	GarbageCollector = new CGarbageCollector(this);
	if (!IsDataOnlyMode())
		KerInterpret = new CKerInterpret(this);
	KerNamesMain = new CKerNamesMain(this);
	Objs = new CKerObjs();
	KerServices = new CKerServices(this);
	if (!IsDataOnlyMode())
		QueryKsParameters = new CQueryKsParameters(this);
}



// Destruktor
CKerMain::~CKerMain() {
	// Smazu zpravy
//	Errors->SaveLogsToFile();
	DeleteMessages(startmq+0,endmq+0);
	DeleteMessages(startmq+1,endmq+1);
	DeleteMessages(startmq+2,endmq+2);
	DeleteMessages(startmq+3,endmq+3);
	DeleteMessages(&timedmsgs,&timedmsgs);
	// Zrusim ostatni soucasti Kernelu:
	SAFE_DELETE(_saving);
	SAFE_DELETE(QueryKsParameters);
	SAFE_DELETE(Attributes);
	if (KS) {
		KS->Destruct();
		KS = 0;
	}
	SAFE_DELETE(Objs);
	SAFE_DELETE_ARRAY(KnownNamesPtrs);
	SAFE_DELETE(KerNamesMain);
	SAFE_DELETE(Errors);
	SAFE_DELETE(KerServices);
	SAFE_DELETE(GarbageCollector);
	SAFE_DELETE(KerInterpret);
	if (KsDllHandle != NULL) {
		FreeLibrary(KsDllHandle);
		KsDllHandle = NULL;
	}
	CFS::DoneFS();
}



void CKerMain::Load() {
	ResetTLSKerMain();

	ReloadTranslations();

	CKernelLoader loader(this);
	loader.Load();

	CScriptLoader *scriptLoader = 0;
	try {

		if (!IsDataOnlyMode()) {
			LoadScriptOnly(KernelParameters->GetCodeFile(), scriptLoader);
		} else {
			FindOrAddKnownNames();
			SetConstructors();
		}

		LoadEngineName();

		if (KernelParameters->CreateEngineAndServices) {
			if (!KernelParameters->CreateEngineAndServices(this, KernelParameters->GetEngineName()))
				Errors->LogError(eKRTEfailedToLoadEngineOrServices);
		}

		loader.LoadDataFiles();
		loader.LoadLevelNamesAndDataObjects();
		loader.MoveLevelToDataSources();

		// final initialization
		GetReadyToStart();

		if (!IsDataOnlyMode()) {
			loader.CreateEmptyObjects();
			scriptLoader->LoadConstants();
			loader.LoadObjects();
			if (!loader.ExistsLevel())
				RunStaticConstructors(); //  vytvori staticke objekty
			loader.RunConstructors(); // runs load constructors
		}

		SAFE_DELETE(scriptLoader);
		Errors->LogError(eKRTELoadComplete);

	} catch(...) {
		SAFE_DELETE(scriptLoader);
		throw;
	}
}


// LOAD JEN SKRIPTU
void CKerMain::LoadScriptOnly(const char *script, CScriptLoader *&scriptLoader) {
	// Load scriptu:
	if (!script)
		Errors->LogError(eKRTEPELoadingScripts, 0, script);
	size_t scriptLen = strlen(script);
	if (scriptLen <= 5 || _stricmp(script + scriptLen - 5, ".code") != 0) 
		Errors->LogError(eKRTEPELoadingScripts, 0, script);
		
	const char *shortScript = script;
	for (size_t f =0; f< scriptLen; f++) {
		if (script[f] == '/' || script[f] == '\\')
			shortScript = script+f+1;
	}

	Errors->LogError(eKRTELoadingScript,0,shortScript);

	// load .dll
	string dllPath("$KSBIN$\\");
	dllPath.append(shortScript, strlen(shortScript)-5);
	dllPath.append(".dll");
	char *path2 = 0;

	FS->GetFullPath(dllPath.c_str(), &path2);
	dllPath = path2;
	SAFE_DELETE_ARRAY(path2);

	KsDllHandle = LoadLibrary(dllPath.c_str());
	if (KsDllHandle == NULL)
		Errors->LogError(eKRTEdllLoadFailed, 0, dllPath.c_str());

	FARPROC ptr = GetProcAddress(KsDllHandle, "?CreateKs@@YAPAVKsInterface@@XZ");
	if (ptr == NULL)
		Errors->LogError(eKRTEdllLoadFailed, 0, "?CreateKs@@YAPAVKsInterface@@XZ");

	KS = ((KsInterface *(*)())ptr)();


	// load .code

	scriptLoader = new CScriptLoader(this, script, CScriptLoader::eLMscript);
	scriptLoader->Load();

}




void CKerMain::LoadEngineName() {
	KString str = ReadGlobalAttribute<KString>(eKKNattrEngine);
	if (str && str->GetCount()) {
		char *str2 = UnicodeToAnsi(str->c_str());
		KernelParameters->SetEngineName(str2);
		SAFE_DELETE_ARRAY(str2);
	}
}




int CKerMain::Save(const char *file, eKerSavingFlags flags, ArrPtr<NamePtr> names, ArrPtr<NamePtr> dependencies, ArrPtr<NamePtr> dataObjects) {
	if (_saving)
		return eKRTEsaveLoadNotAllowed;
	_saving = new CKerSaver(this, file, flags, names, dependencies, dataObjects);
	if (_RunTurn || KerContext)
		return -1;
	GarbageCollector->Collect();
	int ret = _saving->Save();
	SAFE_DELETE(_saving);
	return ret;
}




/////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////
///  STartup Support



// Funkce pripravi Kernel na Beh
void CKerMain::GetReadyToStart() {
	if (IsDataOnlyMode()) {

		if (KernelParameters->Services)
			KernelParameters->Services->GetReadyToStart(this);

	} else {

		// Priprava na spusteni skriptu:
	//	KerInterpret->Init_SetStack();
		CalculatePHT();
	//	KerInterpret->ResetMachine();
		if (KernelParameters->Services)
			KernelParameters->Services->GetReadyToStart(this);

		CKerObject *s;
		if (KernelParameters->ObjectConstructor) {
			s = KernelParameters->ObjectConstructor(Static, this);
		} else {
			s = new CKerObject(Static, this);
		}
		GarbageCollector->Hold(s);
		StaticData = s->thisO; 

		KS->Initialize(this, KernelParameters->Services);

		for (int f = 0; f < Objs->NumObjectT; f++) {
			Objs->ObjectTypes[f].InitFunction = KS->GetInitializationPointer(Objs->ObjectTypes[f].Name);
		}

		Errors->LogError(eKRTEKernelInit,RunMode); // Kernel byl uspesne initovan!
	}
}




void CKerMain::CalculatePHT() {				// spocita perfektni hashovaci tabulky
	CKerCalculatePHT *calc = new CKerCalculatePHT(this);
	calc->calc_o();
	calc->calc_m();
	delete calc;
}




void CKerMain::RunStaticConstructors() {
	try {
		call(0, StaticData, StaticConstructor, 0); 
	} catch (CKernelError err) {
		Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
	}
}



void CKerMain::FindOrAddKnownNames() {
	// Vyhledani a doplneni preddefinovanych jmen
	KnownNamesPtrs = new CKerName*[MAXKNOWNNAMES];
	for (int f=0; f < MAXKNOWNNAMES; f++) {
		if (!(KnownNamesPtrs[f] = KerNamesMain->GetNamePointer(KnownNames[f].namestr))) {
			KnownNamesPtrs[f] = new CKerName(KnownNames[f].namestr, KnownNames[f].type == eKerNTclass ? eKerNTundefinedClass : KnownNames[f].type, KerNamesMain);
			KnownNamesPtrs[f]->LT = KnownNames[f].MethodType;
			if (KnownNamesPtrs[f]->LT.ObjectType)
				KnownNamesPtrs[f]->LT.ObjectType = KnownNamesPtrs[(int)KnownNamesPtrs[f]->LT.ObjectType];
		} else {
			assert(KnownNamesPtrs[f]->Type == KnownNames[f].type);
		}
	}

//	for (int f=0; f<MAXKERKNOWNDEPENDENCIES; f++) KerMain->KnownNamesPtrs[KerKnownDependencies[f*2]]->AddChild(KerMain->KnownNamesPtrs[KerKnownDependencies[f*2+1]]);  // pridani dependenci mezi znamymi jmeny
}



// Nastavim, najdu jmena dulezitych metod:
void CKerMain::SetConstructors() {
	Constructor = KnownNamesPtrs[eKKNconstructor];
	CopyConstructor = KnownNamesPtrs[eKKNcopyConstructor];
	Destructor = KnownNamesPtrs[eKKNdestructor];
	Object = KnownNamesPtrs[eKKNobject];
	Static = KnownNamesPtrs[eKKNstatic];
	StaticConstructor = RunMode == eKRMCreateData ? KnownNamesPtrs[eKKNcreateData] : KnownNamesPtrs[eKKNmain];
}





//////////////////////////////////////////////////////////////////////////////////////
///
///		V o l a n i   a   Z p r a v y
///
//////////////////////////////////////////////////////////////////////////////////////




// Zkonvertuje Parametr Source typu SType na parametr Dest typu DType.
// Hodnota je prekopirovana. Konverze probehne pro vsechny typy. V nekterych pripadech je ohlasena chyba eKerEGParamConversionError
void CKerMain::ConvertParam(void *Source, CLT SType, void *Dest, CLT DType) {
	if (DType.DimCount>0) {
		if (SType == DType) {
			eKerTypeModifier mod = SType.ConstPointer();
			if ((mod & DType.Modifier) != mod) {
				Errors->LogError(eKRTEcastingOffConst);
				*(void**)Dest = 0;
			} else {
				*(void**)Dest = *(void**)Source;
			}
		} else {
			*(void**)Dest = 0;
			if (SType.Type != eKTnull)
				Errors->LogError(eKRTEarrayconv);
		}
	} else if (SType.DimCount>0) {
		Errors->LogError(eKRTEarrayconv);
		switch (DType.Type) {
			case eKTchar: 
				*(wchar_t*)Dest = 0;
				return;
			case eKTdouble: 
				*(double*)Dest = 0;
				return;
			case eKTint: 
				*(int*)Dest = 0;
				return;
			default: 
				*(void**)Dest = 0;
				return;
		}
	} else {
		switch (DType.Type) {
			case eKTchar: 
				switch (SType.Type) {
					case eKTchar:
						*(wchar_t*)Dest = *(wchar_t*)Source;
						return;
					case eKTint:
						*(wchar_t*)Dest = *(int*)Source;
						return;
					case eKTdouble: 
						*(wchar_t*)Dest = (wchar_t)floor(*(double*)Source + 0.5); 
						return;
					case eKTobject:
					case eKTname: 
						Errors->LogError(eKRTEptrtonum);
					case eKTnull:
						*(wchar_t*)Dest = 0;
						return;
					default:
						assert(false);
						return;
				}
			case eKTdouble:
				switch (SType.Type) {
					case eKTchar:
						*(double*)Dest = *(wchar_t*)Source;
						return;
					case eKTint:
						*(double*)Dest = *(int*)Source;
						return;
					case eKTdouble: 
						*(double*)Dest = *(double*)Source; 
						return;
					case eKTobject:
					case eKTname: 
						Errors->LogError(eKRTEptrtonum);
					case eKTnull:
						*(double*)Dest = 0;
						return;
					default:
						assert(false);
						return;
				}
			case eKTobject:
				switch (SType.Type) {
					case eKTchar:
					case eKTint:
					case eKTdouble: 
						Errors->LogError(eKRTEnumtoptr);
					case eKTnull:
						*(OPointer*)Dest = 0;
						return;
					case eKTname: 
						Errors->LogError(eKRTEptrconv);
						*(OPointer*)Dest = 0;
						return;
					case eKTobject:
						{
							int mod = (SType.Modifier & eKTMconstO);
							if ((mod & DType.Modifier) != mod) {
								Errors->LogError(eKRTEcastingOffConst);
								*(OPointer*)Dest = 0;
							} else if (SType.ObjectType != DType.ObjectType && DType.ObjectType != Object) {
								*(OPointer*)Dest = (*(OPointer*)Source).Cast(DType.ObjectType);
							} else {
								*(OPointer*)Dest = *(OPointer*)Source;
							}
						}
						return;
					default:
						assert(false);
						return;
				}
			case eKTname:
				switch (SType.Type) {
					case eKTchar:
					case eKTint:
					case eKTdouble: 
						Errors->LogError(eKRTEnumtoptr);
					case eKTnull:
						*(CKerName**)Dest = 0;
						return;
					case eKTobject: 
						Errors->LogError(eKRTEptrconv);
						*(CKerName**)Dest = 0;
						return;
					case eKTname:
						*(CKerName**)Dest = *(CKerName**)Source;
						return;
					default:
						assert(false);
						return;
				}
			case eKTint:
				switch (SType.Type) {
					case eKTchar:
						*(int*)Dest = *(wchar_t*)Source;
						return;
					case eKTint:
						*(int*)Dest = *(int*)Source;
						return;
					case eKTdouble: 
						*(int*)Dest = (int)floor(*(double*)Source + 0.5); 
						return;
					case eKTobject:
					case eKTname: 
						Errors->LogError(eKRTEptrtonum);
					case eKTnull:
						*(int*)Dest = 0;
						return;
					default:
						assert(false);
						return;
				}
			default:
				assert(false);
				return;
		}
	}


}




void CKerMain::ClearParam(void *Dest, CLT DType) {
	if (DType.DimCount>0) {
		*(void**)Dest = 0;
	} else {
		switch (DType.Type) {
			case eKTchar: *(wchar_t*)Dest = 0; return;
			case eKTdouble: *(double*)Dest = 0; return;
			case eKTint: *(int*)Dest = 0; return;
			default: *(void**)Dest = 0; return;
		}
	}
}



//////////////////////////////////////////////////////////////////
// Funkce nahraje do neprirazenych argumentu defaultni hodnoty
// Neprerazeny argument je oznacen nulou v KerContext->ParamsInfo[f]
void CKerMain::SetDefaultParams(CKerMethod *method) {
	for (int f=0; f<method->NumP; f++) {
		CKerParam &p = method->Params[f];
		if (KerContext->ParamsInfo[f] == 0 && p.DefaultValue) {
			if (p.LT.DimCount > 0) {
				KerContext->prm<void*>(f) = p.DefaultValue->_pointer;
			} else {
				switch (p.LT.Type) {
					case eKTchar: KerContext->prm<wchar_t>(f) = p.DefaultValue->_char; break;
					case eKTdouble: KerContext->prm<double>(f) = p.DefaultValue->_double; break;
					case eKTint: KerContext->prm<int>(f) = p.DefaultValue->_int; break;
					default: KerContext->prm<void*>(f) = p.DefaultValue->_pointer; 
				}
			}
		}
	}
}





///////////////////////////////////////////////////////////////////
 // Automaticka konverze zakladniho typu. Vystup se prida k vystupu pomoci returnovaci funkce
void CKerMain::ConvertParamRet(void *Source, CLT SType, void *Dest, eKerTypes DType, eKerTypeModifier RetFce) {
	assert(RetFce);
	if (DType==eKTchar) {
		wchar_t a;
		ConvertParam(Source,SType,&a,eKTchar);
		if (RetFce==eKTMretOr) *(wchar_t*)Dest |= a;
		else if (RetFce==eKTMretAnd) *(wchar_t*)Dest &= a;
		else /*ADD*/ *(wchar_t*)Dest += a;
	} else if (DType==eKTint) {
		int a;
		ConvertParam(Source,SType,&a,eKTint);
		if (RetFce==eKTMretOr) *(int*)Dest |= a;
		else if (RetFce==eKTMretAnd) *(int*)Dest &= a;
		else /*ADD*/ *(int*)Dest += a;
	} else assert(false);
}


//////////////////////////////////////////////////////////////////////////
// vola se v pripade ze nebylo nic predano. Pokud je tam korektni retFce, preda se default, jinak nahlasi chybu ErrorNum
void CKerMain::FillRetFceDefault(void *Dest, CLT DType, int RetFce, int ErrorNum) {
	ClearParam(Dest, DType);
	if (DType.IsRetCalculable()) {
		switch (RetFce) {
			case 0: Errors->LogError(ErrorNum); break;
			case eKTMretOr:
			case eKTMretAdd:
				if (DType.Type == eKTint) *(int*)Dest = 0;
				else if (DType.Type == eKTchar) *(wchar_t*)Dest = 0;
				else Errors->LogError(ErrorNum);
				break;
			case eKTMretAnd:
				if (DType.Type == eKTint) *(int*)Dest = 0xFFFFFFFF;
				else if (DType.Type == eKTchar) *(wchar_t*)Dest = 0xFF;
				else Errors->LogError(ErrorNum);
				break;
		}
	} else {
		Errors->LogError(ErrorNum);
	}
}


struct CKerICretInfo {
	CLT retType;	// to co jde na vystup. Zde si pamatuju RetFci a zda uz jsem vracel
	void *retPtr;	//		pointer do RetBuffu
	CLT fceType;	// to co mi vratila volana funkce
	void *fcePtr;	//		pointer do volane fce
};


//////////////////////////////////////////////////////////
// PRIME SAFE VOLANI (Je mozne vracet hodnoty: navratova hodnota fce, parametry type eKTret)
// Funkce nalezne prislusne metody volaneho objektu a zavola je:
// Inicializuje se kontext, predaji se argumenty, volani, argumenty se predaji zpatky, deinit kontextu
// Navratova hodnota fce je ponechana na zasobniku
void CKerMain::IScall(OPointer Object, CKerName *Method, int NumArgs, CLT *ArgTypes, CKerName **ArgNames, void **ArgPointers, CLT RetType) {
	ResetTLSKerMain();
	CKerMethod **ms; // volane fce
	int destruct=-1; // Nasatavim na 1 kdyz zjistim, ze se vola destruktor. Po zkonceni volani provedu dealokaci objektu
	int returned=0; // zda uz bylo neco vraceno
	void *retplace;  // kam mam na zasobnik hodit vracenou hodnotu.
	CStackDealocator StackPushSize(KerInterpret);
	int f,a;
	CKerParam **ps;
	CKerICretInfo *ArgInfos=0; // hledam parametry volane fce odpovidajici parametrum pri volani a zapamatuju si to. Pouziti: Vraceni hodnotou
	UC *ArgBuff=0;
	ScriptActivity++;

	// Vyhradim misto na zasobniku pro navratovou hodnotu fce
	retplace = KerInterpret->Push(RetType.SizeOf());
	
	if (NumArgs) { // Vytvorym pole ArgTypes2, ArgPointers2:
		int size=0;
		for (f=0;f<NumArgs;f++) if (ArgTypes[f].IsRet()) size+= ArgTypes[f].SizeOf();
		if (size>0) {
			// pomocne pole si dam na zasobnik
			ArgBuff = KerInterpret->Push<UC>(StackPushSize, size); 
			ArgInfos = KerInterpret->Push<CKerICretInfo>(StackPushSize, NumArgs);
			size = 0;
			for (f=0;f<NumArgs;f++) {
				ArgInfos[f].retPtr = ArgBuff + size;
				if (ArgTypes[f].IsRet()) size+= ArgTypes[f].SizeOf();
			}
		}
	}


	if (Method && Method->Type == eKerNTclass && !Object) {
		Object = Construct(Method);
		if (RetType.Type==eKTobject) {
			returned++;
			ConvertParam(&Object, CLT(eKTobject, eKTMnone, Method, 0), retplace, RetType);
		}
		Method = Constructor;
	}

	if (!Object || !Method || (Method->Type != eKerNTsafeMethod && Method->Type != eKerNTstaticSafeMethod)) { 
		if (!Object) {
			Errors->LogError(eKRTESCnoObj); // neexistuje objekt!
		} else {
			Errors->LogError(eKRTEBadMethod); 
		}
	} else {
		CKerObjectT *objT = Object.KerObjectT();

		ms = 0;
		if ((a = Method->KerPHTpos - objT->MCLstart) < objT->MCLstop) {
			if (a>=0 && objT->MCLnames[a]==Method) {
				ms = objT->MethodCL[a];
			}
		}

		bool runInitializations = false;
		if ((Method == Constructor || Method == StaticConstructor) && objT->InitFunction)
			runInitializations = true;

		if (ms || runInitializations) {
			// Objekt ma volanou metodu(y) muzu pokracovat:
			destruct = 0;

			CKerContext ctx(this); // inicializace kontextu:

			if (runInitializations) {
				ctx.InitInitialization(Object);
				SafeCallFunction function = (SafeCallFunction)(objT->InitFunction);
				function(this);
			}

			while (ms && *ms) {
				assert((**ms).Safe);
				if ((**ms).Name == Destructor) {
					//if (destruct!=1) MapInfo->RemoveObjFromMap(0,Object);
					destruct = 1;
				}
				CStackDealocator MethodStackSize(KerInterpret);
				ctx.InitMethod(MethodStackSize, *ms, Object);

				// Predani Argumentu:
				for (f=0;f<(**ms).NumP;f++) KerContext->ParamsInfo[f] = 0;
				for (f=0;f<NumArgs;f++) {
					if (ArgInfos) ArgInfos[f].fceType.Type = eKTunasigned;
					if ((a = ArgNames[f]->KerPHTpos - (**ms).PCLstart) < (**ms).PCLstop) {
						if (a>=0 && (**ms).PCLnames[a] == ArgNames[f]) {
							ps = (**ms).ParamCL[a]; // <- Pole argumentu kam argument predavam
							while (*ps) {
								if (KerContext->ParamsInfo[*ps - (**ms).Params] != 0) Errors->LogError(eKRTEmenyToOne); 
								KerContext->ParamsInfo[*ps - (**ms).Params] = 1;
								if ((**ps).LT.IsRet() && ArgTypes[f].IsRet()) { // bude se vracet hodnotou:
									eKerTypeModifier a = (**ps).LT.RetFce();
									if (!a) a = ArgInfos[f].retType.RetFce();
									a = (eKerTypeModifier)(a | eKTMret);
									if (!ArgInfos[f].retType.IsUnasigned() && a != ArgInfos[f].retType.RetFce()) Errors->LogError(eKRTEretTypeChanged);
									ArgInfos[f].retType.Modifier = a;
									if (!ArgInfos[f].fceType.IsUnasigned()) Errors->LogError(eKRTEretMenyToOne);
									ArgInfos[f].fceType = (**ps).LT;
									ArgInfos[f].fcePtr = KerContext->Params+(**ps).Offset;
								}
								ConvertParam(ArgPointers[f],ArgTypes[f],ctx.Params + (**ps).Offset,(**ps).LT);
								ps+=1;
							}
						}
					}
				}
				SetDefaultParams(*ms);

				// Volani:
				SafeCallFunction function = (SafeCallFunction)((**ms).Function);
				function(this);

				// Vraceni argumentu:
				for (f=0;f<NumArgs;f++) if (ArgInfos && ArgInfos[f].fceType.IsRet()) {
					if (ArgInfos[f].retType.IsUnasigned()) {
						// predavam poprve
						ConvertParam(ArgInfos[f].fcePtr,ArgInfos[f].fceType,ArgInfos[f].retPtr,ArgTypes[f]);
						ArgInfos[f].retType.Type = ArgTypes[f].Type;
					} else {
						// predavam vicekreat
						eKerTypeModifier retFce = ArgInfos[f].retType.RetFce();
						if (retFce && ArgTypes[f].IsRetCalculable()) {
							ConvertParamRet(ArgInfos[f].fcePtr,ArgInfos[f].fceType,ArgInfos[f].retPtr,ArgTypes[f].Type,retFce);
						} else Errors->LogError(eKRTEretMenyToOne);
					}
				}
				// vraceni navratove hodnoty
				if (RetType.Type!=eKTvoid && (**ms).LT.Type!=eKTvoid) {
					returned++;
					if (returned==1) {
						ConvertParam(ctx.Params,(**ms).LT,retplace,RetType);
					} else {
						if (Method->LT.RetFce() && RetType.IsRetCalculable()) {
							ConvertParamRet(ctx.Params,(**ms).LT,retplace,RetType.Type,Method->LT.RetFce());
						} else if (returned==2) Errors->LogError(eKRTEretMenyToOne);
					}
				}
				// Uvolneni zasobniku:
				ms+=1;
			}

			// Deinicializace kontextu:
			ctx.KCthis = 0;
			if (destruct) Destruct(Object); // dealokace objektu, kdyz se volal destruktor
			if (ctx.startmq) RunMessages(&(ctx.startmq), &(ctx.endmq));
		}

		if (destruct==-1&&(Method==Destructor||Method->Compare(Destructor)>=2)) {
			//MapInfo->RemoveObjFromMap(GET_CODE_LINE,Object);
			Destruct(Object); // dealokace objektu, kdyz se volal destruktor
		}
	}


	// zkontroluju zda vsechny argumenty vratily a hodnoty prekopiruju
	for (f=0;f<NumArgs;f++) if (ArgTypes[f].IsRet()) {
		if (ArgInfos[f].retType.IsUnasigned()) {
			FillRetFceDefault(ArgPointers[f],ArgTypes[f],ArgTypes[f].RetFce(),eKRTEnothingRetInArg);
		} else memcpy(ArgPointers[f],ArgInfos[f].retPtr,ArgTypes[f].SizeOf());
	}

	// zkontroluju zda vratila fce
	if (!returned && RetType.Type!=eKTvoid) {
		FillRetFceDefault(retplace,RetType,RetType.RetFce(),eKRTEfceNotReturning);
	}
}











/////////////////////////////////////////////////////////////////
//	Prime volani, kdyz chci vratit void.
//	fce prevede promenny pocet argumentu do formy jakou chce IScall a zavola ji
void CKerMain::call(int CodeLine, const OPointer &Object,CKerName *Method,int NumArgs, ...) { // int ArgType, CKerName *ArgName, Arg, ... 
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

	IScall(Object,Method,NumArgs,ArgTypes,ArgNames,ArgPointers,eKTvoid);


}






/////////////////////////////////////////////////////////////////////////
///		Messages


////////////////////////////////////////////////////////////////////////////
// Pomocna fce, ktera preda argumenty ze zpravy do volane metody
void CKerMain::PassParams(CKerMethod *method, CKerMessage *msg) {
	int f,a;
	int Offset=0; // pozice v argumentech
	CKerParam **ps;

	for (f=0;f<method->NumP;f++) KerContext->ParamsInfo[f] = 0; 
	for (f=0;f<msg->NumArgs;f++) {
		if ((a = msg->ArgNames[f]->KerPHTpos - method->PCLstart) < method->PCLstop) {
			if (a>=0 && method->PCLnames[a] == msg->ArgNames[f]) {
				ps = method->ParamCL[a]; // <- Pole argumentu kam argument predavam
				while (*ps) {
					if (KerContext->ParamsInfo[*ps - method->Params] != 0) Errors->LogError(eKRTEmenyToOne);
					KerContext->ParamsInfo[*ps - method->Params] = 1;
					ConvertParam(msg->Args+Offset,msg->ArgTypes[f],KerContext->Params+(**ps).Offset,(**ps).LT);
					ps+=1;
				}
			}
		}
		Offset += msg->ArgTypes[f].SizeOf();
	}
	SetDefaultParams(method);
}



//////////////////////////////////////////////////////////////////////////
//	 PROVEDENI ZPRAV
//	fce projde frontu zprav, vyzvedava zpravy a provadi je (vola prislusne metody, predava jim argumenty)
void CKerMain::RunMessages(CKerMessage **startmq, CKerMessage **endmq) {
	CKerMessage *msg;  // provadena zprava
	CKerMethod **ms;	// volane fce
	int a;
	int destruct;		// Nasatavim na 1 kdyz zjistim, ze se vola destruktor. Po zkonceni volani provedu dealokaci objektu

	while (*startmq) {
		msg = *startmq;  // vyzvednu zpravu z fronty
		*startmq = msg->next;
		if (*startmq==0) *endmq=0;
		ScriptActivity++;

		try {
			CKerName *Method = msg->Method;
			OPointer Object = msg->Receiver;

			if (Method->Type == eKerNTclass && !Object) {
				Object = Construct(Method);
				Method = Constructor;
			}

			if (Method->Type != eKerNTsafeMethod && Method->Type != eKerNTstaticSafeMethod) {
				Errors->LogError(eKRTEBadMethod); 
			} else if (Object.Lives()) { // pokud existuje Receiver
				
				destruct = -1;
				CKerContext ctx(this, msg); // vytvorim kontext
				CKerObjectT *objT = Object.KerObjectT();

				if ((Method == Constructor || Method == StaticConstructor) && objT->InitFunction) {
					ctx.InitInitialization(Object);
					SafeCallFunction function = (SafeCallFunction)(objT->InitFunction);
					try {
						function(this);
					} catch (CKernelError err) {
						Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
					}
				}


				if ((a = Method->KerPHTpos - objT->MCLstart) < objT->MCLstop) {
					if (a>=0 && objT->MCLnames[a]==Method) {
						// Objekt ma volanou metodu(y) muzu pokracovat:
						destruct = 0;
						ms = objT->MethodCL[a]; // <- metody, ktere budu volat

						while (*ms) {			
							assert((**ms).Safe);
							if ((**ms).Name == Destructor) {
								//if (destruct==0) MapInfo->RemoveObjFromMap(0,Object);
								destruct = 1;
							}
							// Vyhradim pro argumenty misto na zasobniku :
							CStackDealocator StackSize(KerInterpret);
							ctx.InitMethod(StackSize, *ms, Object);
							// Predani argumentu:
							if ((**ms).NumP) PassParams(*ms,msg);
							
							// Volani:
							try {
								SafeCallFunction function = (SafeCallFunction)((**ms).Function);
								function(this);
							} catch (CKernelError err) {
								Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
							}

							ms+=1;
						}
						// Deinicializace kontextu:
						ctx.KCthis = 0;
						if (destruct) Destruct(Object); // dealokace objektu, kdyz se volal destruktor
						if (ctx.startmq) RunMessages(&(ctx.startmq), &(ctx.endmq));
					}
				} 
				if (destruct==-1&&(Method==Destructor||Method->Compare(Destructor)>=2)) {
					//MapInfo->RemoveObjFromMap(0,Object);
					Destruct(Object); // dealokace objektu, kdyz se volal destruktor
				}
			}
		} catch(CKernelPanic) {
			delete msg;
			throw;
		}

		delete msg;
	}
}




///////////////////////////////////////////////////////////////////
// Vytvoreni noveho objektu daneho typu (dojde k alokaci pameti a volani konstruktoru)
OPointer CKerMain::NewObject(int CodeLine, CKerName *ObjType) {
	SET_CODE_LINE(CodeLine); 
	try {
		OPointer ptr = Construct(ObjType); 
		call(CodeLine,ptr,Constructor,0); 
		return ptr; 
	} catch (CKernelError err) {
		Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
		return 0;
	}
}




OPointer CKerMain::Construct(CKerName *className) {
	try {
		CKerObject *obj;
		if (KernelParameters->ObjectConstructor) {
			obj = KernelParameters->ObjectConstructor(className, this);
		} else {
			obj = new CKerObject(className, this);
		}
		return obj->thisO;
	} catch (CKernelError err) {
		Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
		return 0;
	}
}




OPointer CKerMain::GetDataObjectInstance(CKerName *name) {
	CDataObjectLoader loader(this);
	OPointer ret = loader.Load(name);
	loader.RunConstructors();
	return ret;
}




void CKerMain::DeleteObject(int CodeLine, const OPointer &Object) // Zruseni objektu (pozivaji se zpravy. K zruseni dojde az pote, co se objekt prestane pouzivat) Vola se destruktor.
{ 
	if (!Object.Lives()) return; 
	try {
		if (KerContext) {
			message(CodeLine, Object,Destructor,eKerCTcallend,0, Object, 0); 
		} else {
			call(CodeLine, Object,Destructor,0); 
		}
	} catch (CKernelError err) {
		Errors->LogError2(GET_CODE_LINE, err.ErrorNum);
	}
}



///////////////////////////////////////////////////////////////////
// Dealokace objektu
void CKerMain::Destruct(const OPointer &ObjPtr) {
	CKerContext *ctx = KerContext;
	while (ctx) {
		if (ctx->KCthis == ObjPtr) Errors->LogError(eKRTEDelObjInUse);
		ctx = ctx->parent;
	}
	if (KernelParameters->KillHandler)
		KernelParameters->KillHandler(ObjPtr.KerObject(), this);
	ObjPtr.KerObject()->lives = 0;
}





//////////////////////////////////////////////////////////////
// Smazani vsech zprav z fronty. Zpravy se neprovedou.
void CKerMain::DeleteMessages(CKerMessage **startmq, CKerMessage **endmq) {
	CKerMessage *msg = *startmq, *msg2;
	while(msg) {
		msg2 = msg;
		msg = msg->next;
		delete msg2;
	}
	*startmq=0; *endmq=0;
}




////////////////////////////////////////////////////////////////////
///  RUN TURN
///  Entry Point. Provede se jedno kolo. Volat opakovane kazde kolo.
void CKerMain::RunTurn(int time, int krize) {
	ResetTLSKerMain();
	_RunTurn = 1;
	ScriptActivity = 0;
	if (Time > (UI)(Time+time)) Errors->LogError(eKRTEOutOfTime);
	Time += time;
	TimeCrisis = krize;
	PopTimedMessages();  // vyzvednu casovane zpravy
//	MapInfo->MoveMovingObjs(); // provedu pohyby
	// Provedu zpravy pro toto kolo.
	do {
		if (startmq[0]) RunMessages(startmq,endmq); // Provedu normalni zpravy
		startmq[0]=startmq[1]; startmq[1]=0;  // Dam zpravy z konce kola do normalnich zprav
		endmq[0]=endmq[1]; endmq[1]=0;
	} while (startmq[0]); // dokud jsou v tomto kole nejake zpravy
	// dam zpravy pro dalsi kolo do tohoto kola:
	startmq[0]=startmq[2]; startmq[2]=0;
	endmq[0]=endmq[2]; endmq[2]=0;
	startmq[1]=startmq[3]; startmq[3]=0;
	endmq[1]=endmq[3]; endmq[3]=0;
	// Podivam se jestli nemam ukoncit Kernel

	_RunTurn = 0;

	if (_saving) {
		GarbageCollector->Collect();
		_lastSaveError = _saving->Save();
		SAFE_DELETE(_saving);
	} else {
		GarbageCollector->CollectIfNecessary();
	}

	if (_TerminateKernel) {
		Errors->LogError(eKRTEKernelEnds);
//		DebugMessage(1,0xFFFF0000,"K:END");
		throw CKernelPanic();
	}

//	DebugMessage(0,0xFFFF8800,"SA:%03i",ScriptActivity);
}




///////////////////////////////////////////////////////
//	Prida casovane zpravy na ktere uz prisel cas od aktualni fronty zprav.
void CKerMain::PopTimedMessages() {
	CKerMessage *msg1=timedmsgs, *msg2=0;
	while (msg1 && msg1->Time <= Time) { msg2 = msg1; msg1 = msg1->next; }
	if (msg2) {
		msg2->next = 0;
		if (endmq[0]) endmq[0]->next = timedmsgs; else startmq[0] = timedmsgs;
		endmq[0] = msg2;
		timedmsgs = msg1;
	}
}



/////////////////////////////////////////////////////////////////////
/// VYTVORENI ZPRAVY
/// Argumenty jsou zkopirovany do zpravy.
void CKerMain::message(int CodeLine, const OPointer &Object, CKerName *Method, int when, UI time, const OPointer &callendObject, int NumArgs, ...) { // int ArgType, ..., CKerName *ArgName, ..., void Arg, ...
	ResetTLSKerMain();
	int f,a=0;

	SET_CODE_LINE(CodeLine);
	if (!Method) return;
	CKerMessage *msg = new CKerMessage(when, time, callendObject, this); // Vytvorim zpravu. Pridam ji do patricne fronty zprav (podle when)
	// redirekce nekterych zprav:
	//if (Method == KnownNames[eKKNtriggerOn].Name || Method == KnownNames[eKKNtriggerOff].Name) {
	//	void *var = GetAccessToVar(Object,eKVUmsgRedirect);
	//	if (var && *(OPointer*)var) Object = *(OPointer*)var;
	//	REINIT_CONTEXT;
	//}
	msg->Receiver = Object;
	msg->Method = Method;
	if (NumArgs) {
		// Okopirovani Argumentu:
		msg->NumArgs = NumArgs;
		msg->ArgTypes = new CLT[NumArgs];
		msg->ArgNames = new CKerName*[NumArgs];
		va_list list;
		va_start( list, NumArgs ); 
		for (f=0;f<NumArgs;f++) { // prectu typy argumentu
			msg->ArgTypes[f] = va_arg( list, CLT);
			assert (!msg->ArgTypes[f].IsRet());
			a+=msg->ArgTypes[f].SizeOf(); // pocitam celkovou velikost argumentu
		}
		for (f=0;f<NumArgs;f++) {
			msg->ArgNames[f] = va_arg( list, CKerName*); // prectu jmena argumentu
		}
		msg->Args = new char[a]; // vytvorim pole pro argumenty
		a=0;
		// okopiruju argumenty:
		for (f=0;f<NumArgs;f++) {
			if (msg->ArgTypes[f].DimCount>0) {
				*(void**)(msg->Args+a) = va_arg(list, void*); a+=4;
			} else {
				switch (msg->ArgTypes[f].Type) {
					case eKTchar : *(wchar_t*)(msg->Args+a) = va_arg(list, wchar_t); a+=2; break;
					case eKTdouble : *(double*)(msg->Args+a) = va_arg(list, double); a+=8; break;
					default:*(int*)(msg->Args+a) = va_arg(list, int); a+=4; break;
				}
			}
		}
		va_end( list );
	}
	msg->HoldArguments();
}




/////////////////////////////////////////////////////////////////////
/// VYTVORENI ZPRAVY
/// Argumenty jsou zkopirovany do zpravy.
void CKerMain::ISmessage(const OPointer &Object, CKerName *Method, int when, UI time, const OPointer &callendObject, int NumArgs, int *ArgTypes, CKerName **ArgNames, void **ArgPointers) {
	ResetTLSKerMain();
	assert(false);
	//int f,g,a=0;
	//if (!Method) return;
	//CKerMessage *msg = new CKerMessage(when, callendObject); // Vytvorim zpravu. Pridam ji do patricne fronty zprav (podle when)
	//// redirekce nekterych zprav:
	//if (Method == KnownNames[eKKNtriggerOn].Name || Method == KnownNames[eKKNtriggerOff].Name) {
	//	void *var = GetAccessToVar(Object,eKVUmsgRedirect);
	//	if (var && *(OPointer*)var) Object = *(OPointer*)var;
	//	REINIT_CONTEXT;
	//}
	//msg->Receiver = Object;
	//msg->Method = Method;
	//if (NumArgs) {
	//	// Okopiruju argumenty:
	//	msg->NumArgs = NumArgs;
	//	msg->ArgTypes = new int[NumArgs];
	//	msg->ArgNames = new CKerName*[NumArgs];
	//	for (f=0;f<NumArgs;f++) {
	//		msg->ArgTypes[f] = ArgTypes[f]; // nactu typy argumentu
	//		msg->ArgNames[f] = ArgNames[f]; // nactu jmena argumentu
	//		assert (ArgTypes[f]<eKTret);
	//		a+=GetTypeSize(ArgTypes[f]);	// spocitam celkovou velikost argumentu
	//	}
	//	msg->Args = new char[a];  // Vytvorim pole pro argumenty
	//	a=0;
	//	for (f=0;f<NumArgs;f++) {
	//		// a nakopiruju do nej samotne argumenty:
	//		switch (ArgTypes[f]) {
	//			case eKTchar : *(wchar_t*)(msg->Args+a) = *(wchar_t*)(ArgPointers[f]); a+=1; break;
	//			case eKTdouble : *(double*)(msg->Args+a) = *(double*)(ArgPointers[f]); a+=8; break;
	//			case eKTint:
	//			case eKTpointer:
	//			case eKTobject:
	//			case eKTname: *(int*)(msg->Args+a) = *(int*)(ArgPointers[f]); a+=4; break;
	//			case eKTarrChar:			
	//			case eKTarrDouble:
	//			case eKTarrInt:
	//			case eKTarrPointer:
	//			case eKTarrObject:
	//			case eKTarrName: assert(false); *(int*)(msg->Args+a) = 0; a+=4; break;
	//			default: for (g=a;g<a+ArgTypes[f]-100+1;g++) msg->Args[g] = ((char*)(ArgPointers[f]))[g-a]; a=g;
	//		}
	//	}
	//}
}



///////////////////////////////////////////////////////////////////////////
// Nasledujici funkce najdou vsechny zpravy ( ve vsech frontach), ktere odpovidaji popisu. Pokud nektery parametr je zadan jako null, tak na tomto parametru nezalezi, vyhleda se vse
//////////////////////////////////////////////////////////////////////////

// Funkce vyhleda a smaze zpravy ze vsech front. Vrati pocet smazanych zprav.
int CKerMain::FDeleteMessages(const OPointer &Reciever, CKerName *msg, OPointer Sender) {
	ResetTLSKerMain();
	CKerContext *ctx = KerContext;
	int Count = 0;
	int f;
	while (ctx) { // callend
		Count += FindCountDeleteMessages(&ctx->startmq,&ctx->endmq,Reciever,msg,Sender,1);
		ctx = ctx->parent;
	}
	for (f=0; f<4; f++) { // zakladni fronty
		Count += FindCountDeleteMessages(startmq+f,endmq+f,Reciever,msg,Sender,1);
	}
	Count += FindCountDeleteMessages(&timedmsgs,0,Reciever,msg,Sender,1);  // timed
	return Count;
}


// Funkce spocita zpravy ve vsech frontach (vraci pocet).
int CKerMain::FCountMessages(const OPointer &Reciever, CKerName *msg, OPointer Sender) {
	ResetTLSKerMain();
	CKerContext *ctx = KerContext;
	int Count = 0;
	int f;
	while (ctx) { // callend
		Count += FindCountDeleteMessages(&ctx->startmq,&ctx->endmq,Reciever,msg,Sender);
		ctx = ctx->parent;
	}
	for (f=0; f<4; f++) { // zakladni fronty
		Count += FindCountDeleteMessages(startmq+f,endmq+f,Reciever,msg,Sender);
	}
	Count += FindCountDeleteMessages(&timedmsgs,0,Reciever,msg,Sender);  // timed
	return Count;
}


// Vyhleda zadane zpravy ve fronte, spocita je a pripadne smaze
int CKerMain::FindCountDeleteMessages(CKerMessage **startmq, CKerMessage **endmq, const OPointer &Reciever, CKerName *msg, const OPointer &Sender, int Delete ) {
	CKerMessage *m, *m2=0;
	int count = 0;
	while (*startmq) {
		m = *startmq;
		if ((!Reciever || m->Receiver == Reciever) && (!msg || m->Method == msg) && (!Sender || m->Sender == Sender)) {
			count ++;
			if (Delete) {
				*startmq = m->next;
				if (endmq && !m->next) *endmq = m2;
				delete m;
			} else {
				m2 = m; // v m2 je zprava navstivena v predchozim kroku
				startmq = &m->next;
			}
		} else {
			m2 = m; // v m2 je zprava navstivena v predchozim kroku
			startmq = &m->next;
		}
	}
	return count;
}







bool CKerMain::TryReadUserNameOrComment(CKerName *name, bool isComment, KString &output) {
	ResetTLSKerMain();
	const wchar_t *str = 0;

	if (CRootNames::ResourceManager && (str = CRootNames::ResourceManager->GetUserNameOrComment(name->name, isComment))) {
		output = KString(str);
		return true;
	}

	return false;
}



void CKerMain::ReloadTranslations() {
	if (CRootNames::ResourceManager)
		CRootNames::ResourceManager->ReloadIfNeeded();
}



