/////////////////////////////////////////////////////////////////////////////
///
///		K E R N E L
///
///		Hlavni soubor Kernelu. Load levlu a skriptu. Rizeni
///		A: Honza M.D. Krcek
///
/////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "KerServices.h"
#include "fs.api.h"







////////////////////////////////////////////////////////////////////////////////
///
///		C K e r M a i n
///
////////////////////////////////////////////////////////////////////////////////








































//////////////////////////////////////////////////
// vraci pointr na promennou objektu
//void *CKerMain::GetAccessToVar(OPointer obj, CKerOVar* Var) { 
//	CKerObject *o = Objs->GetObject(obj);
//	if (!o) return 0;
//	if (!Var->KSVar) {
//		assert(o->ISData);
//		return o->ISData + Var->Offset;
//	} else {
//		assert(o->Type->SetObjectVar);
//		o->Type->SetObjectVar(o->KSVG);
//		return *(Var->KSVar);
//	}
//}


//////////////////////////////////////////////////
// vraci pointr na promennou objektu
//void *CKerMain::GetAccessToVar(OPointer obj, int VarNum) { 
//	CKerObject *o = Objs->GetObject(obj);
//	if (!o) return 0;
//	if (!o->Type->SpecificKnownVars) return 0;
//	CKerOVar *Var = o->Type->SpecificKnownVars[VarNum];
//	if (!Var) return 0;
//	if (!Var->KSVar) {
//		assert(o->ISData);
//		return o->ISData + Var->Offset;
//	} else {
//		assert(o->Type->SetObjectVar);
//		o->Type->SetObjectVar(o->KSVG);
//		return *(Var->KSVar);
//	}
//}


//////////////////////////////////////////////////
// vraci pointr na promennou objektu
//void *CKerMain::GetAccessToVar(CKerObject *o, CKerOVar* Var) { 
//	if (!Var->KSVar) {
//		assert(o->ISData);
//		return o->ISData + Var->Offset;
//	} else {
//		assert(o->Type->SetObjectVar);
//		o->Type->SetObjectVar(o->KSVG);
//		return *(Var->KSVar);
//	}
//}


//////////////////////////////////////////////////
// vraci pointr na promennou objektu
//void *CKerMain::GetAccessToVar(CKerObject *o, int VarNum) { 
//	if (!o->Type->SpecificKnownVars) return 0;
//	CKerOVar *Var = o->Type->SpecificKnownVars[VarNum];
//	if (!Var) return 0;
//	if (!Var->KSVar) {
//		assert(o->ISData);
//		return o->ISData + Var->Offset;
//	} else {
//		assert(o->Type->SetObjectVar);
//		o->Type->SetObjectVar(o->KSVG);
//		return *(Var->KSVar);
//	}
//}



///////////////////////////////////////////////////////////////
// Nastavi pristup k promennym objektu (pozor nici kontext!) Kdyz objekt neexistuje, vraci null
//CKerObject * CKerMain::GetAccessToObject(OPointer obj) {
//	CKerObject *o = Objs->GetObject(obj);
//	if (o && o->Type->SetObjectVar) o->Type->SetObjectVar(o->KSVG);
//	return o;
//}













//////////////////////////////////////////////////////////////////////////////////
///
///		L o a d   a   I n i t   K e r n e l u
///
///////////////////////////////////////////////////////////////////////////////////




///////////////////////////////////////////
// Zjisti jmeno zdrojoveho souboru a provede rekompilace. Vraci 0 v pripade chyby
int KerRecompileScripts(int scriptonly, char *load) {
	//CFSRegister *reg;
	//CFSRegKey *k;
	//int ret;
	//
	//if (!scriptonly) {
	//	FS->ChangeDir("$GAME$");
	//	reg = new CFSRegister(load,"KRKAL LEVEL");
	//	if (!(k=reg->FindKey("Scripts File"))) {FS->ChangeDir("$KRKAL$"); delete reg; return 0;}
	//	FS->ChangeDirByFile(load);
	//	ret = compiler->TestComp(k->GetDirectAccess());
	//	delete reg;
	//} else {
	//	ret = compiler->TestComp(load);
	//}
	//FS->ChangeDir("$KRKAL$");
	//if (ret==0) return 1; else return 0;
	return 0;
}


///////////////////////////////////////////
// Vytvoreni a naloadovani kernelu. Provadi se pripadne rekompilace skriptu
//void KerCreateKernel(int runmode, int debugmode, int scriptonly, char *load) {
//	int err,comp;
//	CKerMain *KerMain;
//
////	DebugMessage(0,0xFFFFFFFF,"XXX");
//	KerMain = new CKerMain(runmode, debugmode);
//	if (scriptonly) KerMain->LoadScriptOnly(load);
//	else KerMain->LoadLevel(load);
//	err = KerMain->GetOpenError();
////	DebugMessage(1,0xFFFFFFFF,"K:LE:%i",err);
//	if (err) {
//		if (err==eKerOLEOpeningLevel) KerMain->Errors->LogError(eKRTEPELoadingLevel,1); else {
//			delete KerMain;
//			comp = KerRecompileScripts(scriptonly,load);
//			KerMain = new CKerMain(runmode, debugmode);
//			if (err==eKerOLEOpeningSCode) KerMain->Errors->LogError(eKRTEELoadingScripts,1);
//			KerMain->Errors->LogError(eKRTECompilating);
//			if (!comp) KerMain->Errors->LogError(eKRTECompilationFailed);
//			if (scriptonly) KerMain->LoadScriptOnly(load);
//			else KerMain->LoadLevel(load);
//			err = KerMain->GetOpenError();
////			DebugMessage(1,0xFFFFFFFF,"K:LE:%i",err);
//			if (err) {
//				if (err == eKerOLEOpeningLevel) KerMain->Errors->LogError(eKRTEPELoadingLevel,2);
//				else if (err == eKerOLEOpeningSCode) KerMain->Errors->LogError(eKRTEPELoadingScripts,2);
//				else KerMain->Errors->LogError(eKRTEPanic);
//			}
//		}
//	}
//}



///////////////////////////////////////////
// Kdyz se nahrava ulozena hra
// Vytvoreni a naloadovani kernelu. Provadi se pripadne rekompilace skriptu
// vraci 1 (uspech), 0 - neuspech, level se nepodarilo nahrat, kernel nevytvoren, 
// nebo vyhodi vyjimku KernelPanic, kernel vytvoren
int KerLoadGameAndKernel(char *load, CFSRegister *SavedGame) {
//	int err=0,comp;
//	CFSRegister *lev;
//	CFSRegKey *k, *sf, *lf;
//	int rmode, dmode;
//	CKerMain *KerMain;
//
////	DebugMessage(0,0xFFFFFFFF,"XXX");
////	FS->ChangeDir("$GAME$");
//	if (!SavedGame)	lev = new CFSRegister(load,"KRKAL SaveGame");
//	else lev = SavedGame;
//	lev->SeekAllTo0();
//	if (lev->GetOpenError()==FSREGOK) {
//		if (k = lev->FindKey("Run Mode")) rmode = k->readi(); else err = 1;
//		if (k = lev->FindKey("Debug Mode")) dmode = k->readi(); else err = 1;
//		if (!(sf=lev->FindKey("Scripts File"))) err = 1;
//		if (!(lf=lev->FindKey("Level File"))) err = 1;
//	} else err=1;
//	if (err) {
////		FS->ChangeDir("$KRKAL$");
////		DebugMessage(1,0xFFFF0000,"K:FileE");
//		if (!SavedGame)	delete lev;
//		return 0;
//	}
//
//	try {
//		KerMain = new CKerMain(rmode, dmode);
//		KerMain->Errors->LogError(eKRTEloadingGame,0,load);
//		KerMain->LoadGame(lev,lf,sf);
//		err = KerMain->GetOpenError();
////		DebugMessage(1,0xFFFFFFFF,"K:LE:%i",err);
//		if (err) {
//			if (err==eKerOLEOpeningLevel) KerMain->Errors->LogError(eKRTEPELoadingLevel,1); else {
//				delete KerMain;
////				FS->ChangeDir("$GAME$");
//				lev->SeekAllTo0();
////				FS->ChangeDirByFile(lf->GetDirectAccess());
////				comp = compiler->TestComp(sf->GetDirectAccess());
//				KerMain = new CKerMain(rmode, dmode);
//				KerMain->Errors->LogError(eKRTECompilating);
//				if (comp) KerMain->Errors->LogError(eKRTECompilationFailed);
//				KerMain->Errors->LogError(eKRTEloadingGame,0,load);
//				KerMain->LoadGame(lev,lf,sf);
//				err = KerMain->GetOpenError();
////				DebugMessage(1,0xFFFFFFFF,"K:LE:%i",err);
//				if (err) {
//					if (err == eKerOLEOpeningLevel) KerMain->Errors->LogError(eKRTEPELoadingLevel,2);
//					else if (err == eKerOLEOpeningSCode) KerMain->Errors->LogError(eKRTEPELoadingScripts,2);
//					else KerMain->Errors->LogError(eKRTEPanic);
//				}
//			}
//		}
//	} catch (CKernelPanic) {
////		FS->ChangeDir("$KRKAL$");
//		if (!SavedGame)	delete lev;
//		throw;
//	}
//
////	FS->ChangeDir("$KRKAL$");
//	if (!SavedGame)	delete lev;
	return 1;

}





// Nahraje ulozenou hru  - volat po konstruktoru, uz muzou vznikat vyjimky CKernalPanic
//int CKerMain::LoadGame(CFSRegister *lev, CFSRegKey *LevelFile, CFSRegKey *ScriptsFile) {
//	SaveState = eSLSloadGame;
//
////	LevelInfo.LevelFile = newstrdup(LevelFile->GetDirectAccess());
////	FS->ChangeDirByFile(LevelInfo.LevelFile);
//	if (!LoadLevel2(lev)) OpenError=eKerOLEOpeningLevel; 
//
//	SaveState = eSLSnothing;
//	if (OpenError) return 0;	
//	Errors->LogError(eKRTELoadComplete);
//	Objs->LOShakeOffSize = Objs->LoadedObjects.GetCount();
//	return 1;
//}


// LOAD LEVLU
//void CKerMain::LoadLevel(char *level) {
//	CFSRegister *lev;
//	SaveState = eSLSloadConstructor;
//
//	Errors->LogError(eKRTELoadingLevel,0,level);
////	LevelInfo.LevelFile = newstrdup(level);
////	FS->ChangeDir("$GAME$");
//	lev = new CFSRegister(level,"KRKAL LEVEL");
////	if (lev->GetOpenError()==FSREGOK) FS->ChangeDirByFile(level);
//	try {
//		if (!LoadLevel2(lev)) OpenError=eKerOLEOpeningLevel; 
//	} catch (CKernelPanic) {
//		delete lev;
////		FS->ChangeDir("$KRKAL$");
//		throw;
//	}
////	FS->ChangeDir("$KRKAL$");
//	SaveState = eSLSnothing;
//	delete lev;
//	if (OpenError) return;
//	
////	level[34+20] = 0;
////	Objs->KernelDump(level+34);
//	Errors->LogError(eKRTELoadComplete);
//	Objs->LOShakeOffSize = Objs->LoadedObjects.GetCount();
//}






// Inituje globalni promenne na 0, vytvori staticke objekty



// Nahraje hodnoty globalnich promennych z levlu, 1 OK, 0 chyba 
int CKerMain::LLoadGlobals(CFSRegKey *r) {
	//int f,g;
	//for (f=0;f<NumGV;f++) {
	//	// ToDo zefektivnit (az se bude delat bezpecnostni politika), vznikaji memory leaky
	//	for (g=0; g<GlobalVar[f].ArraySize; g++) {
	//		switch (GlobalVar[f].Type) {
	//			case eKTdouble: (*(double**)GlobalVar[f].KSVar)[g] = 0; break;
	//			case eKTint:
	//			case eKTname:
	//			case eKTpointer:
	//			case eKTobject: (*(int**)GlobalVar[f].KSVar)[g] = 0; break;
	//			case eKTarrChar: (*(CKerArrChar***)GlobalVar[f].KSVar)[g] = new CKerArrChar(); break;
	//			case eKTarrDouble: (*(CKerArrDouble***)GlobalVar[f].KSVar)[g] = new CKerArrDouble(); break;
	//			case eKTarrInt: (*(CKerArrInt***)GlobalVar[f].KSVar)[g] = new CKerArrInt(); break;
	//			case eKTarrPointer: (*(CKerArrPointer***)GlobalVar[f].KSVar)[g] = new CKerArrPointer(); break;
	//			case eKTarrObject: (*(CKerArrObject***)GlobalVar[f].KSVar)[g] = new CKerArrObject(); break;
	//			case eKTarrName: (*(CKerArrName***)GlobalVar[f].KSVar)[g] = new CKerArrName(); break;
	//			case eKTvoid: 
	//			case eKTstruct:	break;
	//			case eKTchar: (*(char**)GlobalVar[f].KSVar)[g] = 0; break;
	//			default: **((char**)GlobalVar[f].KSVar) = 0; if (GlobalVar[f].ArraySize>1) throw;
	//		}
	//	}
	//	if (!r) LLoadVariable(0,GlobalVar+f,0);
	//	else LLoadVariable(r->GetSubRegister(),GlobalVar+f,0);
	//}
	return 1;
}


// Paokud najde v registru promennou, tak ji nahraje
void CKerMain::LLoadVariable(CFSRegister *r, CKerOVar *OV, UC *offset) {
	//CFSRegKey *l;
	//int g,a,size;
	//char ch;
	//CKerName *name;
	//void *var;

	//if (!(OV->Use&eKVUBlevelLoad) && SaveState==eSLSloadConstructor) return;
	//if (OV->Type == eKTvoid || OV->Type == eKTstruct) return;
	//if (!r) {
	//	Errors->LogError(eKRTEVarLoad,0,OV->NameStr);
	//	return;
	//}
	//l = r->FindKey(OV->NameStr);
	//if (!l) {
	//	Errors->LogError(eKRTEVarLoad,0,OV->NameStr);
	//	return;
	//}
	//r = l->GetSubRegister();
	//l = r->FindKey("Type");
	//if (!l || l->readi() != OV->Type) {
	//	Errors->LogError(eKRTEVarLoad,0,OV->NameStr);
	//	return;
	//}

	//if (OV->KSVar) var = *(OV->KSVar);  // najdu umisteni prom
	//else var = offset + OV->Offset; 

	//if (!(l=r->FindKey("Value"))) {
	//	if (OV->Type >= eKTarrChar && OV->Type <= eKTarrName && OV->ArraySize==1) {
	//		*((int*)var) = 0;
	//	} else {
	//		Errors->LogError(eKRTEVarLoad,0,OV->NameStr);
	//	}
	//	return;
	//}


	//int gg;
	//if (l->CFSGetKeyType() == FSRTchar || l->CFSGetKeyType() == FSRTstring ) KerSaveLoad.Open(l,SaveState);
	//for (gg=0; gg<OV->ArraySize; gg++) {

	//	switch (OV->Type) {					// prekopiruju obsah prom
	//		case eKTdouble: 
	//			((double*)var)[gg] = l->readd();
	//			break;
	//		case eKTint:
	//			(((int*)var))[gg] = l->readi(); 
	//			break;
	//		case eKTpointer:
	//			(((int*)var))[gg] = 0 ;
	//			break;
	//		case eKTname:
	//			if (OV->ArraySize>1) {
	//				((CKerName**)var)[gg] = KerSaveLoad.LoadName();
	//				break;
	//			}
	//			if (l->GetDirectAccessFromPos()[0]==0) ((CKerName**)var)[gg] = 0; else {
	//				name = KerNamesMain->GetNamePointer(l->GetDirectAccessFromPos());
	//				((CKerName**)var)[gg] = name;
	//				if (!name) Errors->LogError(eKRTEVarLoad,0,"Name n Exists"); //error
	//			}
	//			break;
	//		case eKTobject: 
	//			a = l->readi(); 
	//			if (a==0) ((OPointer*)var)[gg] = 0; 
	//			else if (SaveState == eSLSloadGame) ((OPointer*)var)[gg] = a;
	//			else if (a >= Objs->LoadedObjects.GetCount() || Objs->LoadedObjects[a]==0) {
	//				Errors->LogError(eKRTEVarLoad,0,"Object n Exists");
	//				((OPointer*)var)[gg] = 0;
	//			} else ((OPointer*)var)[gg] = Objs->LoadedObjects[a];
	//			break;
	//		case eKTchar: 
	//			((char*)var)[gg] = l->readc(); 
	//			break;
	//		case eKTarrChar:
	//			if (OV->ArraySize>1) {
	//				((CKerArrChar**)var)[gg] = KerSaveLoad.LoadCharA();
	//			} else {
	//				((CKerArrChar**)var)[gg] = new CKerArrChar();
	//				while (!l->eof()) ((CKerArrChar**)var)[gg]->Add(l->readc());
	//			}
	//			break;
	//		case eKTarrDouble:
	//			if (OV->ArraySize>1) {
	//				((CKerArrDouble**)var)[gg] = KerSaveLoad.loadDoubleA();
	//			} else {
	//				((CKerArrDouble**)var)[gg] = new CKerArrDouble();
	//				while (!l->eof()) ((CKerArrDouble**)var)[gg]->Add(l->readd());
	//			}
	//			break;
	//		case eKTarrInt:
	//			if (OV->ArraySize>1) {
	//				((CKerArrInt**)var)[gg] = KerSaveLoad.LoadIntA();
	//			} else {
	//				((CKerArrInt**)var)[gg] = new CKerArrInt();
	//				while (!l->eof()) ((CKerArrInt**)var)[gg]->Add(l->readi());
	//			}
	//			break;
	//		case eKTarrPointer:
	//			if (OV->ArraySize>1) {
	//				((CKerArrPointer**)var)[gg] = 0;
	//			} else {
	//				((CKerArrPointer**)var)[gg] = new CKerArrPointer();
	//				while (!l->eof()) ((CKerArrPointer**)var)[gg]->Add((void*)l->readi());
	//			}
	//			break;
	//		case eKTarrObject:
	//			if (OV->ArraySize>1) {
	//				((CKerArrObject**)var)[gg] = KerSaveLoad.LoadObjPtrA();
	//			} else {
	//				((CKerArrObject**)var)[gg] = new CKerArrObject();
	//				while (!l->eof()) {
	//					a = l->readi();
	//					if (a==0) ((CKerArrObject**)var)[gg]->Add(0);
	//					else if (SaveState == eSLSloadGame) ((CKerArrObject**)var)[gg]->Add(a);
	//					else if (a >= Objs->LoadedObjects.GetCount() || Objs->LoadedObjects[a]==0) {
	//						Errors->LogError(eKRTEVarLoad,0,"Object n Exists");
	//						((CKerArrObject**)var)[gg]->Add(0);
	//					} else ((CKerArrObject**)var)[gg]->Add(Objs->LoadedObjects[a]);
	//				}
	//			}
	//			break;
	//		case eKTarrName:
	//			if (OV->ArraySize>1) {
	//				((CKerArrName**)var)[gg] = KerSaveLoad.LoadNameA();
	//			} else {
	//				((CKerArrName**)var)[gg] = new CKerArrName();
	//				while (!l->eof()) {
	//					if (l->GetDirectAccessFromPos()[0] == 0) ((CKerArrName**)var)[gg]->Add(0);
	//					else {
	//						name = KerNamesMain->GetNamePointer(l->GetDirectAccessFromPos());
	//						if (!name) Errors->LogError(eKRTEVarLoad,0,"Name n Exists"); //error
	//						((CKerArrName**)var)[gg]->Add(name);
	//					}
	//					l->SetPosToNextString();
	//				}
	//			}
	//			break;
	//		default: 
	//			if (OV->ArraySize>1) throw;
	//			size = GetTypeSize(OV->Type)-1;
	//			g=0;
	//			while ((ch=l->readc())&&size) {
	//				((char*)var)[g] = ch;
	//				g++; size--; 
	//			}
	//			(((char*)var))[g] = 0;
	//			if(ch) Errors->LogError(eKRTEVarLoad,size,"String too big");
	//	}
	//}
	//KerSaveLoad.Close();
}



// sejvne promennou do registru
void CKerMain::LSaveVariable(CFSRegister *r, CKerOVar *OV, UC *offset, OPointer thisO) {
	//CFSRegKey *l;
	//int f;
	//CKerName *name;
	//void *var;

	//if (!(OV->Use&eKVUBlevelLoad) && SaveState==eSLSsaveLevel) return;
	//if (OV->Type == eKTvoid || OV->Type == eKTstruct) return;
	//r = r->AddKey(OV->NameStr,FSRTregister)->GetSubRegister();
	//r->AddKey("Type",FSRTint)->writei(OV->Type);

	//if (OV->KSVar) var = *(OV->KSVar);  // najdu umisteni prom
	//else var = offset + OV->Offset; 

	//int gg;
	//char *str = "Value";
	//if (OV->Type >= eKTarrChar && OV->Type <= eKTarrName && (*(int*)var)==0 && OV->ArraySize == 1) return; // pole je NULL

	//switch (OV->Type) {
	//	case eKTdouble: l = r->AddKey(str,FSRTdouble); break;
	//	case eKTint:
	//	case eKTobject:
	//	case eKTpointer: l = r->AddKey(str,FSRTint); break;
	//	case eKTarrName:
	//	case eKTname: l = r->AddKey(str,FSRTstring); break;
	//	case eKTchar: 
	//	case eKTarrChar: l = r->AddKey(str,FSRTchar); break;
	//	case eKTarrDouble: if (OV->ArraySize>1) l = r->AddKey(str,FSRTchar); else l = r->AddKey(str,FSRTdouble); break;
	//	case eKTarrInt:
	//	case eKTarrPointer:
	//	case eKTarrObject: if (OV->ArraySize>1) l = r->AddKey(str,FSRTchar); else l = r->AddKey(str,FSRTint); break;
	//	default: l = r->AddKey(str,FSRTstring); break;
	//}
	//if (l->CFSGetKeyType() == FSRTchar || l->CFSGetKeyType() == FSRTstring ) KerSaveLoad.Open(l,SaveState);

	//for (gg=0; gg<OV->ArraySize; gg++) {

	//	switch (OV->Type) {					// prekopiruju obsah prom
	//		case eKTdouble: 
	//			l->writed(((double*)var)[gg]);
	//			break;
	//		case eKTint:
	//		case eKTpointer:
	//			l->writei(((int*)var)[gg]);
	//			break;
	//		case eKTname:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveName(((CKerName**)var)[gg]);
	//			} else {
	//				name = ((CKerName**)var)[gg];
	//				if(name == 0) l->writec(0);
	//				else {
	//					if (/*TODO v release runu to netestovat*/ !KerNamesMain->TestPointerValidity(name)) {
	//						l->writec(0);
	//						Errors->LogError(eKRTEnotValidName,0,"SaveName");
	//					} else l->stringwrite( name->GetNameString());
	//				}
	//			}
	//			break;
	//		case eKTobject: 
	//			if (SaveState == eSLSsaveGame) l->writei(((OPointer*)var)[gg]); 
	//			else {
	//				l->writei(0);
	//				if (!thisO) Errors->LogError(eKRTEsavingGlobalObjPtr);
	//				else KerSaveLoad.AddEdge(thisO,((OPointer*)var)[gg],l,0);
	//			}
	//			break;
	//		case eKTchar: 
	//			l->writec(((char*)var)[gg]);
	//			break;
	//		case eKTarrChar:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveCharA(((CKerArrChar**)var)[gg]);
	//			} else {
	//				for (f=0; f<((CKerArrChar**)var)[gg]->GetCount(); f++) l->writec(((CKerArrChar**)var)[gg]->Read(f));
	//			}
	//			break;
	//		case eKTarrDouble:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveDoubleA(((CKerArrDouble**)var)[gg]);
	//			} else {
	//				for (f=0; f<((CKerArrDouble**)var)[gg]->GetCount(); f++) l->writed(((CKerArrDouble**)var)[gg]->Read(f));
	//			}
	//			break;
	//		case eKTarrInt:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveIntA(((CKerArrInt**)var)[gg]);
	//			} else {
	//				for (f=0; f<((CKerArrInt**)var)[gg]->GetCount(); f++) l->writei(((CKerArrInt**)var)[gg]->Read(f));
	//				break;
	//			}
	//		case eKTarrPointer:
	//			if (OV->ArraySize>1) {
	//				// nedelaj nic
	//			} else {
	//				for (f=0; f<((CKerArrPointer**)var)[gg]->GetCount(); f++) l->writei((int)(((CKerArrPointer**)var)[gg]->Read(f)));
	//			}
	//			break;
	//		case eKTarrObject:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveObjPtrA(((CKerArrObject**)var)[gg]);
	//			} else {
	//				if (SaveState == eSLSsaveGame) {
	//					for (f=0; f<((CKerArrObject**)var)[gg]->GetCount(); f++) {
	//						l->writei(((CKerArrObject**)var)[gg]->Read(f));
	//					}
	//				} else {
	//					if (!thisO) Errors->LogError(eKRTEsavingGlobalObjPtr);
	//					for (f=0; f<((CKerArrObject**)var)[gg]->GetCount(); f++) {
	//						if (thisO) KerSaveLoad.AddEdge(thisO,((CKerArrObject**)var)[gg]->Read(f),l,l->pos);
	//						l->writei(0);
	//					}
	//				}
	//			}
	//			break;
	//		case eKTarrName:
	//			if (OV->ArraySize>1) {
	//				KerSaveLoad.SaveNameA(((CKerArrName**)var)[gg]);
	//			} else {
	//				for (f=0; f<((CKerArrName**)var)[gg]->GetCount(); f++) {
	//					name = ((CKerArrName**)var)[gg]->Read(f);
	//					if(name == 0) l->writec(0);
	//					else {
	//						if (/*TODO v release runu to netestovat*/ !KerNamesMain->TestPointerValidity(name)) {
	//							l->writec(0);
	//							Errors->LogError(eKRTEnotValidName,0,"SaveName");
	//						} else l->stringwrite( name->GetNameString());
	//					}
	//				}
	//			}
	//			break;
	//		default: 
	//			if (OV->ArraySize>1) throw;
	//			((char*)var)[OV->Type-eKTstring] = 0;
	//			l->stringwrite(((char*)var));
	//	}
	//}
	//KerSaveLoad.Close();
}



// Nahraje objekt z levlu a vytvori ho, ptr objektu OK, 0 chyba, -1 zahodil jsem shortcut
int CKerMain::LLoadObject(CFSRegister *r, CKerName *ObjType, OPointer *StaticVar) {
	//CFSRegKey *l=0;
	//int f;
	//int order;
	//CKerObject *Obj;
	//int InMap = 0;

	//if (l = r->FindKey("Order")) {
	//	order = l->readi();
	//} else RETERR;
	//if (l = r->FindKey("ShortCut")) {
	//	if (!editor /*|| RunMode==NORMAL_RUN*/) return -1; // objekt je shortcut, zahodim ho
	//}
	//
	//if (SaveState == eSLSloadGame) Obj = new CKerObject(ObjType,order);
	//else Obj = new CKerObject(ObjType);
	//OPointer ptr = Obj->thisO;
	//if (l) editor->shortCutArray.Add(new CEDShortCut(ptr,l->GetDirectAccess())); // pridam jmeno shortcutu

	//if (StaticVar) *StaticVar = ptr;
	//CKerObjectT *objt = ObjType->ObjectType;
	//unsigned char *offset = Obj->ISData;

	//if (SaveState == eSLSloadConstructor)	Objs->LoadedObjects[order] = ptr;

	//if (l = r->FindKey("Tag")) Obj->Tag = l->readi();

	//// umistim obj do mapy
	//if ((l = r->FindKey("Is In Map")) && (l->readc() == 1)) InMap = 1;
	//if (SaveState == eSLSloadGame && InMap) {
	//	if (!MapInfo->GPlaceObjToMap(Obj,r->FindKey("Placed Info"))) return 0;
	//	if (l = r->FindKey("Mover")) MapInfo->GLoadMover(l,Obj);
	//}

	//if ((l = r->FindKey("Edited")) && (l->readc() == 1)) { // loaduju promenne pro objekt z editoru
	//	GetAccessToObject(Obj);
	//	l = r->FindKey("Basic Data");
	//	for (f=0;f<objt->NumVars;f++) { // je tam loadovana prom.
	//		if (!l) LLoadVariable(0,objt->OVars+f,offset);
	//		else LLoadVariable(l->GetSubRegister(),objt->OVars+f,offset);
	//	}
	//	SaveReg = r;
	//	if (SaveState==eSLSloadConstructor) call(0,ptr,LoadConstructor,0);
	//	else call(0,ptr,KnownNames[eKKNloadGeme].Name,0);
	//	SaveReg = 0;
	//} else call(0,ptr,Constructor,0);
	//// umistim obj do mapy
	//if (SaveState == eSLSloadConstructor && InMap) MapInfo->PlaceObjToMap(0,ptr);

	//return ptr;
	return 0;

}


// Nahraje objekty z levlu a vytvori je, 1 OK, 0 chyba 
int	CKerMain::LLoadObjects(CFSRegKey *r) {
	CFSRegKey *r2;
	CFSRegister *reg;
	CKerName *name;
	OPointer nula=0;
	//if (!r) RETERR;
	reg = r->GetSubRegister();

//	Objs->LoadedObjects.Add(nula); // nulta pozice - NULL
//	for (f=0; f<reg->GetNumberOfKeys();f++) Objs->LoadedObjects.Add(nula);// vynuluju polozky na mistech objektu
	
	// load statickych objektu:
	//for (f=0;f<NumGV;f++) if (GlobalVar[f].Type==eKTobject && GlobalVar[f].Name) {
	//	StaticObjectsCount++;
	//	if (SaveState == eSLSloadGame) continue; // globalni objekty nenahravam zde
	//	r2 = reg->FindKey(GlobalVar[f].NameStr);
	//	if (!r2) {  // objekt v levlu neni
	//		Errors->LogError(eKRTEstaticLoad,0,GlobalVar[f].NameStr);
	//		ptr = (new CKerObject(GlobalVar[f].Name))->thisO;
	//		**((OPointer**)GlobalVar[f].KSVar) = ptr;
	//		Objs->LoadedObjects.Add(ptr);
	//		call(0,ptr,Constructor,0);
	//	} else {
	//		if (!LLoadObject(r2->GetSubRegister(),GlobalVar[f].Name,*((OPointer**)GlobalVar[f].KSVar))) return 0;
	//	}
	//}
//	if (!MapInfo->registered) Errors->LogError(eKRTEmapNotRegistered);
	
	// loadovani objektu umistenych v editoru:
	r2 = reg->GetFirstKey();
	while (r2) {
		if (r2->GetName()[3]=='I') {
			name = KerNamesMain->GetNamePointer(r2->GetName());
			if (!name) {
				Errors->LogError(eKRTEobjectLoad,0,r2->GetName());
			} else {
				if (!LLoadObject(r2->GetSubRegister(),name)) return 0;
			}
		}
		r2 = r2->GetNextKey();
	}


	//if (SaveState == eSLSloadConstructor) {
	//	// zahodim ze seznamu objektu shortcuty
	//	if (editor) for (f=0; f<editor->shortCutArray.GetSize(); f++) if (editor->shortCutArray.Get(f)) {
	//		OPointer a = editor->shortCutArray.Get(f)->obj;
	//		for (g=0; g<Objs->LoadedObjects.GetCount(); g++) if (Objs->LoadedObjects.Get(g)==a) Objs->LoadedObjects[g] = 0;
	//	}
	//} 

	return 1;
}






///// Prevede cestu k souboru se zdrojakem na soubor s codem
//// ocekavam "test_xxxx_xxxx_xxxx_xxxx" (bez pripony a bez cesty)
//char *CKerMain::GetCodeName(const char *script) {
//	sprintf(GCNstrbuff, "$SCRIPTS$\\%s.code", script);
//	return GCNstrbuff;
//}


// nahraje frontu zprav daneho typu
int CKerMain::GLoadMessageQueue(CFSRegKey *k2, int typ) {
	//CFSRegister *r;
	//CFSRegKey *k;
	//int ctime = 0;
	//int f;
	//
	//r = k2->GetSubRegister();
	//k2 = r->GetFirstKey();

	//while (k2) {
	//	r = k2->GetSubRegister(); // projdu kazdou zpravu
	//	if (typ==eKerCTtimed) {
	//		if (k = r->FindKey("Time")) ctime = k->readi() - Time; else RETERR 
	//	}
	//	CKerMessage *msg = new CKerMessage(typ,ctime);
	//	if (k = r->FindKey("Reciever")) msg->Receiver = k->readi();  else RETERR 
	//	if (k = r->FindKey("Method")) msg->Method = KerSaveLoad.LoadName2(k);  else RETERR 
	//	if (k = r->FindKey("Sender")) msg->Sender = k->readi(); 
	//	if (k = r->FindKey("NumArgs")) msg->NumArgs = k->readi();  else msg->NumArgs = 0; 

	//	if (msg->NumArgs) {
	//		int NumArgs = msg->NumArgs;
	//		CFSRegKey *k1,*k2,*k3;
	//		int a=0;
	//		if (!(k1 = r->FindKey("ArgTypes"))) RETERR
	//		if (!(k2 = r->FindKey("ArgNames"))) RETERR
	//		if (!(k3 = r->FindKey("Args"))) RETERR
	//		msg->ArgTypes = new int[NumArgs];
	//		msg->ArgNames = new CKerName*[NumArgs];

	//		for (f=0;f<NumArgs;f++) {
	//			msg->ArgTypes[f] = k1->readi(); // nactu typy argumentu
	//			msg->ArgNames[f] = KerSaveLoad.LoadName2(k2); // nactu jmena argumentu
	//			a+=GetTypeSize(msg->ArgTypes[f]);	// spocitam celkovou velikost argumentu
	//		}
	//		
	//		msg->Args = new char[a];  // Vytvorim pole pro argumenty
	//		a=0;
	//		KerSaveLoad.Open(k3,eSLSloadGame);
	//		for (f=0;f<NumArgs;f++) {
	//		// a nakopiruju do nej samotne argumenty:
	//			switch (msg->ArgTypes[f]) {
	//				case eKTchar : *(char*)(msg->Args+a) = KerSaveLoad.LoadChar(); a+=1; break;
	//				case eKTdouble : *(double*)(msg->Args+a) = KerSaveLoad.LoadDouble(); a+=8; break;
	//				case eKTint: *(int*)(msg->Args+a) = KerSaveLoad.LoadInt(); a+=4; break;
	//				case eKTobject: *(OPointer*)(msg->Args+a) = KerSaveLoad.LoadObjPtr(); a+=4; break;
	//				case eKTpointer: assert(false); *(int*)(msg->Args+a) = 0; a+=4; break;
	//				case eKTname: *(CKerName**)(msg->Args+a) = KerSaveLoad.LoadName(); a+=4; break;
	//				case eKTarrChar:			
	//				case eKTarrDouble:
	//				case eKTarrInt:
	//				case eKTarrPointer:
	//				case eKTarrObject:
	//				case eKTarrName: assert(false); *(int*)(msg->Args+a) = 0; a+=4; break;
	//				default: KerSaveLoad.LoadString(msg->Args+a,msg->ArgTypes[f]); a+=GetTypeSize(msg->ArgTypes[f]);
	//			}
	//		}
	//		KerSaveLoad.Close();
	//	}

	//	k2 = k2->GetNextKey();
	//}
	return 1;
}



// ulozi frontu zprav daneho typu
void CKerMain::GSaveMessageQueue(CFSRegKey *k2, CKerMessage *mq) {
	//CFSRegister *r, *r2;
	//int f;
	//
	//r2 = k2->GetSubRegister();

	//while (mq) {
	//	r = r2->AddKey("msg",FSRTregister)->GetSubRegister(); // projdu kazdou zpravu
	//	r->AddKey("Time",FSRTint)->writei(mq->Time);
	//	r->AddKey("Reciever",FSRTint)->writei(mq->Receiver);
	//	r->AddKey("Sender",FSRTint)->writei(mq->Sender);
	//	r->AddKey("NumArgs",FSRTint)->writei(mq->NumArgs);
	//	KerSaveLoad.SaveName2(r->AddKey("Method",FSRTstring),mq->Method);

	//	if (mq->NumArgs) {
	//		int NumArgs = mq->NumArgs;
	//		CFSRegKey *k1,*k2,*k3;
	//		int a=0;
	//		k1 = r->AddKey("ArgTypes",FSRTint);
	//		k2 = r->AddKey("ArgNames",FSRTstring);
	//		k3 = r->AddKey("Args",FSRTchar);

	//		KerSaveLoad.Open(k3,eSLSsaveGame);
	//		for (f=0;f<NumArgs;f++) {
	//			k1->writei(mq->ArgTypes[f]);
	//			KerSaveLoad.SaveName2(k2,mq->ArgNames[f]);

	//			switch (mq->ArgTypes[f]) {
	//				case eKTchar :  KerSaveLoad.SaveChar(*(char*)(mq->Args+a)); a+=1; break;
	//				case eKTdouble : KerSaveLoad.SaveDouble(*(double*)(mq->Args+a)); a+=8; break;
	//				case eKTint:  KerSaveLoad.SaveInt(*(int*)(mq->Args+a)); a+=4; break;
	//				case eKTobject: KerSaveLoad.SaveObjPtr(*(OPointer*)(mq->Args+a)); a+=4; break;
	//				case eKTpointer: assert(false);  a+=4; break;
	//				case eKTname: KerSaveLoad.SaveName(*(CKerName**)(mq->Args+a)); a+=4; break;
	//				case eKTarrChar:			
	//				case eKTarrDouble:
	//				case eKTarrInt:
	//				case eKTarrPointer:
	//				case eKTarrObject:
	//				case eKTarrName: assert(false);  a+=4; break;
	//				default: KerSaveLoad.SaveString(mq->Args+a,mq->ArgTypes[f]); a+=GetTypeSize(mq->ArgTypes[f]);
	//			}
	//		}
	//		KerSaveLoad.Close();
	//	}

	//	mq = mq->next;
	//}
}


// nahraje stav kenelu a zpravy - pro GameLoad
int CKerMain::GLoadKernelState(CFSRegister *lev) {
	CFSRegKey *k;
//	if (k=lev->FindKey("Time")) Time = k->readi(); else RETERR
//	if (k=lev->FindKey("ObjCounter")) Objs->Counter = k->readi(); else RETERR
	if ((k=lev->FindKey("Message Queue 1"))) if (!GLoadMessageQueue(k,eKerCTmsg)) return 0;
	if ((k=lev->FindKey("Message Queue 2"))) if (!GLoadMessageQueue(k,eKerCTend)) return 0;
	if ((k=lev->FindKey("Message Queue 3"))) if (!GLoadMessageQueue(k,eKerCTtimed)) return 0;
//	if (!MapInfo->GLoadMapInfo(lev->FindKey("Map Info"))) return 0;

	if (k=lev->FindKey("Global Light")) {
		unsigned char r,g,b;
		r = k->readc(); g = k->readc(); b = k->readc(); 
//		GEnMain->SetTopLightIntenzity(r,g,b);
	}

	return 1;
}



// ulozi stav kenelu a zpravy - pro GameLoad
void CKerMain::GSaveKernelState(CFSRegister *lev) {
	lev->AddKey("Run Mode",FSRTint)->writei(RunMode);
	lev->AddKey("Debug Mode",FSRTint)->writei(DebugMode);
	lev->AddKey("Time",FSRTint)->writei(Time);
//	lev->AddKey("ObjCounter",FSRTint)->writei(Objs->Counter);
	GSaveMessageQueue(lev->AddKey("Message Queue 1",FSRTregister),startmq[0]);
	GSaveMessageQueue(lev->AddKey("Message Queue 2",FSRTregister),startmq[1]);
	GSaveMessageQueue(lev->AddKey("Message Queue 3",FSRTregister),timedmsgs);
//	MapInfo->GSaveMapInfo(lev->AddKey("Map Info",FSRTregister));

	int f=0; 
	CFSRegKey *k = lev->AddKey("LoadedObjects",FSRTint);
//	Objs->LOShakeOff();
//	for(f=0; f<Objs->LoadedObjects.GetCount(); f++) k->writei(Objs->LoadedObjects[f]);

	k = lev->AddKey("Global Light",FSRTchar);
//	unsigned char r,g,b;
//	GEnMain->GetTopLightIntenzity(&r,&g,&b);
//	k->writec(r); k->writec(g); k->writec(b); 
}


// Druha cast loadovani levlu - vlastni load
int CKerMain::LoadLevel2(CFSRegister *lev) {
//	CFSRegister *code;
//	CFSRegKey *k;
//	if (lev->GetOpenError()!=FSREGOK) RETERR; 
////	if (!LevelInfo.LoadLevel(lev)) RETERR;
//	if (!(k=lev->FindKey("Scripts File"))) RETERR;
//	// Load scriptu:
////	LevelInfo.ScriptsFile = newstrdup(k->GetDirectAccess());
//	Errors->LogError(eKRTELoadingScript,0,k->GetDirectAccess());
//	code = new CFSRegister(GetCodeName(k->GetDirectAccess()),"KRKAL SCRIPT CODE");
//	try {
//		if (!LoadScriptsCode(code)) OpenError=eKerOLEOpeningSCode;
//	} catch (CKernelPanic) {
//		delete code;
//		throw;
//	}
//	delete code;
//	if (OpenError) return 1;
//	
//	GetReadyToStart();
//
//	if (!LLoadGlobals(lev->FindKey("Globals"))) RETERR;
//	if (SaveState == eSLSloadGame) 	if (!GLoadKernelState(lev)) RETERR;
//	if (!LLoadObjects(lev->FindKey("Objects"))) RETERR;
//	if (SaveState == eSLSloadGame) {
//		int f=0;
//		if (!(k=lev->FindKey("LoadedObjects"))) RETERR
////		while (!k->eof()) { Objs->LoadedObjects[f] = k->readi(); f++;}
//	}
	return 1;
}



// Druha cast loadovani scriptu - vlastni load
int CKerMain::LoadScriptsCode(CFSRegister *code) {
	//CFSRegKey *k, *l;
	//int f;
	//
	//if (code->GetOpenError()!=FSREGOK) RETERR;
	//if (!(k=code->FindKey("Compiled Scripts Version"))) RETERR;
	//if (k->readi()!=KS_VERSION) {
	//	// nesouhlasi mi verze kernelu!
	//	OpenError = eKerOLENeedToRecompile;
	//	return 1;
	//}
	//if (!LevelInfo.LoadScript(code)) RETERR;
	//// Loaduju KOD:
	//if (!(k=code->FindKey("Code"))) RETERR;
	//KerInterpret->LoadCode(k);
	//// Loaduju jmena a inicializuju vse kolem:
	//if (!(k=code->FindKey("Number of Ker Names"))) RETERR;
	//if (!(l=code->FindKey("Ker Names"))) RETERR;
	//if (!KerInterpret->LoadNames(k->readi(),l)) RETERR;
	//SetConstructors();
	//for (f=0; f<MAXKERKNOWNDEPENDENCIES; f++) KnownNames[KerKnownDependencies[f*2]].Name->AddChild(KnownNames[KerKnownDependencies[f*2+1]].Name);  // pridani dependenci mezi znamymi jmeny
	//if (!(l=code->FindKey("Dependencies"))) RETERR;
	//k->pos=0;
	//if (!LoadDependencies(k->readi(),l,KerInterpret->GetNamesArray())) RETERR;
	//// Loaduju typy objektu:
	//if (!(k=code->FindKey("Objects"))) RETERR;
	//if (!Objs->LoadObjects(k->GetSubRegister())) RETERR;
	//// load stromecky automatismu
	//AutosMain->LoadAllAutos();
	//KSNamesAssign();
	//KerNamesMain->CreateMatrix();
	//AutosMain->CheckAutosIntegrity();
	//AutosMain->AssignAutosToObjs();
	//// Loaduju Globalni promenne:
	//if (!KerInterpret->LoadGlobals(code->FindKey("Globals"))) RETERR;
	//if (!Objs->CheckVarGroups(&GlobalVar,NumGV)) RETERR;
	//Input->RegisterKernelKeys(); // Registrace Klaves ke Jmenum
	//ME->RegisterKernelSound();
	return 1;
}




// Load hran mezi KSID jmeny







//////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
///
///		SAVE LEVEL



//ulozi vsechny objekty
void CKerMain::GSaveObjects(CFSRegister *objs) {
	//int f,g;
	//CKerObject *ko;
	//CFSRegister *reg, *r2;

	//for (f=0; f<OBJECT_HT_SIZE; f++) {
	//	ko = Objs->HT[f];
	//	while (ko) {
	//		reg = objs->AddKey(ko->Type->Name->GetNameString(),FSRTregister)->GetSubRegister();

	//		reg->AddKey("Edited",FSRTchar)->writec(1);
	//		reg->AddKey("Order",FSRTint)->writei(ko->thisO);
	//		if (ko->SaveRec) {
	//			reg->AddKey("ShortCut",FSRTstring)->stringwrite(ko->SaveRec->shortcut);
	//			SAFE_DELETE(ko->SaveRec);
	//		}
	//		// necham objekt at si sejvne sve pointery
	//		SaveReg = reg;
	//		call(0,ko->thisO,KnownNames[eKKNsaveGame].Name,0);
	//		SaveReg = 0;

	//		// sejvnu zbytek objektu
	//		reg->AddKey("Tag",FSRTint)->writei(ko->Tag);
	//		GetAccessToObject(ko);
	//		r2 = reg->AddKey("Basic Data", FSRTregister)->GetSubRegister();
	//		for (g=0; g<ko->Type->NumVars; g++) LSaveVariable(r2,ko->Type->OVars+g,ko->ISData,ko->thisO);

	//		if (MapInfo->IsObjInMap(ko->thisO)) {
	//			reg->AddKey("Is In Map",FSRTchar)->writec(1);
	//			ko->PlacedInfo->GSave(reg->AddKey("Placed Info",FSRTregister));
	//			if (ko->PlacedInfo->Mover) ko->PlacedInfo->Mover->GSaveMover(reg->AddKey("Mover",FSRTregister));
	//		}

	//		ko = ko->next;
	//	}
	//}
	//KerSaveLoad.Close();
}




void CKerMain::LSaveObjects() {
	//int f,g;
	//CKerObject *ko;
	//CFSRegister *reg, *r2;

	//// pripravim promenne kam se bude sejvovat:
	//for (f=0; f<Objs->LoadedObjects.GetCount(); f++) {
	//	ko = Objs->GetObject(Objs->LoadedObjects[f]);
	//	if (ko) ko->SaveRec = new CKerObjSaveRec();
	//}

	//for (f=0; f<Objs->LoadedObjects.GetCount(); f++) {
	//	ko = Objs->GetObject(Objs->LoadedObjects[f]);
	//	if (ko && ko->SaveRec) {
	//		ko->SaveRec->reg = reg = new CFSRegister();
	//		reg->AddKey("Edited",FSRTchar)->writec(1);
	//		// necham objekt at si sejvne sve pointery
	//		SaveReg = reg;
	//		call(0,Objs->LoadedObjects[f],KnownNames[eKKNEsaveMe].Name,0);
	//		SaveReg = 0;

	//		// sejvnu zbytek objektu
	//		ko = GetAccessToObject(Objs->LoadedObjects[f]);
	//		if (ko) {
	//			r2 = reg->AddKey("Basic Data", FSRTregister)->GetSubRegister();
	//			for (g=0; g<ko->Type->NumVars; g++) LSaveVariable(r2,ko->Type->OVars+g,ko->ISData,ko->thisO);
	//		}
	//		if (MapInfo->IsObjInMap(Objs->LoadedObjects[f])) reg->AddKey("Is In Map",FSRTchar)->writec(1);
	//	}
	//}
	//KerSaveLoad.Close();
}




/////////////////////////////////////////////////////////////////////
// trideni grafu DFS, objekty predavoji sve sejvovaci egistry na vystup.
int CKerMain::LSSortGraph(OPointer obj, OPointer parent, CFSRegister *sreg, int &count) {
	//if (!obj || obj==parent) return 1;
	//CKerObject *ko = Objs->GetObject(obj);
	//if (!ko) return 1;
	//assert(ko->SaveRec);
	//if (ko->SaveRec->Tag == 2) return 1; // dopredna hrana
	//if (ko->SaveRec->Tag == 1) {
	//	Errors->LogError(eKRTEcycleInbjPtrs,obj);
	//	return 0; // zpetna hrana!!
	//}

	//// jdu po hranach na dalsi objekty:
	//ko->SaveRec->Tag = 1;
	//int ret=1;
	//int f, _count = ko->SaveRec->SaveGraph->GetCount();
	//for (f=0; f<_count; f++) {
	//	ret = LSSortGraph(ko->SaveRec->SaveGraph->Read(f),obj,sreg,count);
	//	if (!ret) return 0;
	//}
	//ko->SaveRec->Tag = 2;

	//// sejvnu objekt.
	//ko->SaveRec->reg->AddKey("Order",FSRTint)->writei(count);
	//if (ko->SaveRec->GlobalVar) {
	//	sreg->AddRegisterToKey(ko->SaveRec->GlobalVar,ko->SaveRec->reg);
	//} else {
	//	sreg->AddRegisterToKey(ko->Type->Name->GetNameString(),ko->SaveRec->reg);
	//}
	//ko->SaveRec->reg = 0;

	//// spravne nastavim pointery na objekt
	//CKerOSRPointers *p = ko->SaveRec->pointers;
	//while (p) {
	//	if (Objs->GetObject(p->obj)) {
	//		p->key->pos = p->pos;
	//		if (p->key->CFSGetKeyType() == FSRTchar) p->key->blockwrite(&count,4);
	//		else p->key->writei(count);
	//	}
	//	p = p->next;
	//}

	//count ++;
	return 1;
}




// sejvne Level s vyuzitim LevelInfa. 1 - OK, 0 - chyba;
//int CKerMain::SaveLevel() {
//	//int ObjCounter = 1;
//	//CFSRegister *reg, *objs;
//	//int ret = 1;
//	//int f;
//	//OPointer obj1,obj2;
//	//CKerObject *ko;
//	//int OldCount;
//
//	//Errors->LogError(eKRTEsavingLevel,0,LevelInfo.LevelFile);
//	//
//	//if (!LevelInfo.LevelFile || !LevelInfo.ScriptsFile || !LevelInfo.SVersion) {
//	//	// error
//	//	return 0;
//	//}
//	//SaveState = eSLSsaveLevel;
//
//	//// pridam ShortCuty k sejvovani
//	//Objs->LOShakeOff();
//	//OldCount = Objs->LoadedObjects.GetCount();
//	//if (editor) {
//	//	for (f = 0; f<editor->shortCutArray.GetSize(); f++) {
//	//		if (editor->shortCutArray.Get(f)) Objs->LoadedObjects.Add(editor->shortCutArray.Get(f)->obj);
//	//	}
//	//}
//
//	//LSaveObjects();
//
//	//// sejvnu k shortCutum jejich jmena
//	//if (editor) {
//	//	for (f = 0; f<editor->shortCutArray.GetSize(); f++) {
//	//		if (editor->shortCutArray.Get(f)) {
//	//			if (ko=Objs->GetObject(editor->shortCutArray.Get(f)->obj)) {
//	//				assert(ko->SaveRec);
//	//				ko->SaveRec->reg->AddKey("ShortCut",FSRTstring)->stringwrite(editor->shortCutArray.Get(f)->name);
//	//			}
//	//		}
//	//	}
//	//}
//
//	//reg = new CFSRegister(LevelInfo.LevelFile,"KRKAL LEVEL",1);
//	//try {
//	//	LevelInfo.SaveLevel(reg);
//
//	//	objs = reg->AddKey("Objects",FSRTregister)->GetSubRegister();
//
//	//	// Pridam hrany do grafu tak aby se Globalni promenne sejvly ve spravnem poradi na zacatku
//	//	// Ke statickym objektum dale pridam jmeno jejich globalni promenne
//	//	obj2 = 0;
//	//	for (f=0; f<NumGV; f++) if (GlobalVar[f].Type==eKTobject && GlobalVar[f].Name) {
//	//		obj1 = **(OPointer**)GlobalVar[f].KSVar;
//	//		ko = Objs->GetObject(obj1);
//	//		if (ko) {
//	//			assert(ko->SaveRec);
//	//			ko->SaveRec->GlobalVar = GlobalVar[f].NameStr;
//	//			if (obj2) ko->SaveRec->SaveGraph->Add(obj2);
//	//			obj2 = obj1;
//	//		}
//	//	}
//	//	if (obj2) for (f=0; f<Objs->LoadedObjects.GetCount(); f++) {
//	//		ko = Objs->GetObject(Objs->LoadedObjects[f]);
//	//		if (ko && ko->SaveRec && !ko->SaveRec->GlobalVar) {
//	//			ko->SaveRec->SaveGraph->Add(obj2);
//	//		}
//	//	}
//
//	//	// sejvnu objekty ve spravnem poradi
//	//	for (f=0; f<Objs->LoadedObjects.GetCount(); f++) {
//	//		ret = LSSortGraph(Objs->LoadedObjects[f],0,objs,ObjCounter);
//	//		if (!ret) break;
//	//	}
//
//	//	// sejvnu globalni promenne
//	//	if (ret) {
//	//		CFSRegister *r2 = reg->AddKey("Globals", FSRTregister)->GetSubRegister();
//	//		for (f=0; f<NumGV; f++) LSaveVariable(r2,GlobalVar+f,0,0);
//	//	}
//
//
//	//	// smazi sejvovaci zaznamy
//	//	for (f=0; f<Objs->LoadedObjects.GetCount(); f++) {
//	//		ko = Objs->GetObject(Objs->LoadedObjects[f]);
//	//		if (ko) SAFE_DELETE(ko->SaveRec);
//	//	}
//
//	//	Objs->LoadedObjects.SetCount(OldCount);
//
//	//	if (ret) {
//	//		ret = reg->WriteFile();
//	//		if (!ret) Errors->LogError(eKRTEsavingLIOErr);
//	//	}
//	//} catch (CKernelPanic) {
//	//	delete reg;
//	//	Objs->LoadedObjects.SetCount(OldCount);
//	//	throw;
//	//}
//	//delete reg;
//	//SaveState = eSLSnothing;
//	//if (ret) Errors->LogError(eKRTEsaveLOK);
//	//return ret;
//	return 0;
//}




// ulozi hru do pripravenoho registru
void CKerMain::GSaveGame() {
//	CFSRegister *reg;
//	int err=0;
//	int f;
//	Errors->LogError(eKRTEsavingGame,0,_FileToSaveGame);
//	
//	if (!LevelInfo.LevelFile || !LevelInfo.ScriptsFile || !LevelInfo.SVersion) {
//		// error
//		Errors->LogError(eKRTEgameNotSaved,0,0);
//		return;
//	}
//	SaveState = eSLSsaveGame;
//
//	if (_FileToSaveGame) reg = new CFSRegister(_FileToSaveGame,"KRKAL SaveGame",1);
//	else {
//		SAFE_DELETE(KerServices.QuickSave);
//		reg = KerServices.QuickSave = new CFSRegister("$KRKAL$/Quick Save","KRKAL SaveGame",1);
//	}
//
//	try {
//		LevelInfo.SaveLevel(reg);
//		reg->AddKey("Level File",FSRTstring)->stringwrite(LevelInfo.LevelFile);
//
//		// poznamenam si docasne u objektu jmena shortcutu
//		if (editor) {
//			CKerObject *ko;
//			for (f = 0; f<editor->shortCutArray.GetSize(); f++) {
//				if (editor->shortCutArray.Get(f)) {
//					if (ko=Objs->GetObject(editor->shortCutArray.Get(f)->obj)) {
//						ko->SaveRec = new CKerObjSaveRec();
//						ko->SaveRec->shortcut = editor->shortCutArray.Get(f)->name;
//					}
//				}
//			}
//		}
//
//		// sejv objektu
//		GSaveObjects(reg->AddKey("Objects", FSRTregister)->GetSubRegister());
//
//		CFSRegister *r2 = reg->AddKey("Globals", FSRTregister)->GetSubRegister();
//		for (f=0; f<NumGV; f++) LSaveVariable(r2,GlobalVar+f,0,0);
//
//		GSaveKernelState(reg);
//
//	} catch (CKernelPanic) {
//		if (_FileToSaveGame) delete reg; else SAFE_DELETE(KerServices.QuickSave);
//		SaveState = eSLSnothing;
//		throw;
//	}
//
//	if (!err) {
//		if (_FileToSaveGame) err = !reg->WriteFile();
////		err = !reg->WriteFile();
//	}
//	if (_FileToSaveGame) delete reg;
//	if (err) {
//		if (!_FileToSaveGame) SAFE_DELETE(KerServices.QuickSave);
//		Errors->LogError(eKRTEgameNotSaved,0,0);
//	} else Errors->LogError(eKRTEsaveLOK);
//	SaveState = eSLSnothing;
}


//////////////////////////////////////////////////////////////////////////////






/////////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

//void CKerGarbageCollector::EmptyMe() {
//	while (GCArrChar) delete GCArrChar;
//	while (GCArrInt) delete GCArrInt;
//	while (GCArrDouble) delete GCArrDouble;
//	while (GCArrPointer) delete GCArrPointer;
//	while (GCArrObject) delete GCArrObject;
//	while (GCArrName) delete GCArrName;
//	GCArrChar = 0;
//	GCArrInt = 0;
//	GCArrDouble = 0;
//	GCArrPointer = 0;
//	GCArrObject = 0;
//	GCArrName = 0;
//
//}


// Hodnoty ktere vrati dynamicka pole v pripade chyby
//char* KRValueCKerArrChar = 0;
//double* KRValueCKerArrDouble = 0;
//int* KRValueCKerArrInt = 0;
//void** KRValueCKerArrPointer = 0;
//OPointer* KRValueCKerArrObject = 0;
//CKerName** KRValueCKerArrName = 0;





//////////////////////////////////////////////////////////////////
///
///		C K e r L e v e l I n f o
///
//////////////////////////////////////////////////////////////////

//CKerLevelInfo::~CKerLevelInfo() {
//	SAFE_DELETE_ARRAY(LevelFile);
//	SAFE_DELETE_ARRAY(ScriptsFile);
//	SAFE_DELETE_ARRAY(SVersion);
//	SAFE_DELETE_ARRAY(Author);
//	SAFE_DELETE_ARRAY(Game);
//	SAFE_DELETE_ARRAY(Comment);
//	SAFE_DELETE_ARRAY(Music);
//	SAFE_DELETE_ARRAY(Directory);
//	SAFE_DELETE_ARRAY(Password);
//	SAFE_DELETE(LocalNames);
//}
//
//
//int CKerLevelInfo::LoadScript(CFSRegister *reg) {
//	CFSRegKey *k;
//	if (k=reg->FindKey("Scripts Version")) SVersion = k->stringread();
//	else return 0;
//	if (k=reg->FindKey("Scripts Version 64")) SVersion64 = k->read64();
//	else return 0;
//	if (!Game && (k=reg->FindKey("Game"))) Game = k->stringread();
//
//	return 1;
//}
//
//
//int CKerLevelInfo::LoadLevel(CFSRegister *reg) {
//	CFSRegKey *k;
//	if (k=reg->FindKey("Author")) Author = k->stringread();
//	if (k=reg->FindKey("Game")) Game = k->stringread();
//	if (k=reg->FindKey("Comment")) Comment = k->stringread();
//	if (k=reg->FindKey("Music")) {
//		Music = k->stringread();
//		if(ME) ME->Play(Music,1,2);
//	}
//	if (k=reg->FindKey("Difficulty")) Difficulty = k->readd();
//	if (k=reg->FindKey("Tags")) Tags = k->readi();
//	if (k=reg->FindKey("LocalNames")) {
//		CFSRegister *r = k->GetSubRegister();
//		LocalNames = new CFSRegister();
//		k = r->GetFirstKey();
//		while (k) {
//			if (k->CFSGetKeyType() == FSRTstring) LocalNames->AddKey(k->GetName(),FSRTstring)->stringwrite(k->GetDirectAccess());
//			k = k->GetNextKey();
//		}
//	}
//	if (k=reg->FindKey("Directory")) Directory = k->stringread();
//	if (k=reg->FindKey("Password")) Password = k->stringread();
//	if ((KerMain->SaveState == eSLSloadConstructor) && (k=reg->FindKey("LVersion"))) {
//		// otestuju zda sedi verze levlu - kvuli bezpecnosti
//		CFSRegister *r;
//		CFSRegKey *k2;
//		int err=0;
//		if (strcmp(k->GetDirectAccess(),MMLevelDirs->GetVersionString(LevelFile)) != 0) KerMain->Errors->LogError(eKRTEinvLvlVersion);
//		r = new CFSRegister("!level.info","KRKAL LEVEL I");
//		if (r->GetOpenError() != FSREGOK) err=1; else {
//			k2 = r->FindKey("LVersion");
//			if (!k2 || strcmp (k->GetDirectAccess(), k2->GetDirectAccess()) != 0) err=1;
//		}
//		delete r;
//		if (err) KerMain->Errors->LogError(eKRTEinvLvlVersion);
//	}
//	return 1;
//}
//
//
//void CKerLevelInfo::SaveLevel(CFSRegister *reg) {
//	char *ver, *str;
//	if (ScriptsFile) reg->AddKey("Scripts File",FSRTstring)->stringwrite(ScriptsFile);
//	if (SVersion) reg->AddKey("Scripts Version",FSRTstring)->stringwrite(SVersion);
//	reg->AddKey("Scripts Version 64",FSRTint64)->write64(SVersion64);
//	if (Author) reg->AddKey("Author",FSRTstring)->stringwrite(Author);
//	if (Game) reg->AddKey("Game",FSRTstring)->stringwrite(Game);
//	if (Comment) reg->AddKey("Comment",FSRTstring)->stringwrite(Comment);
//	if (Music) reg->AddKey("Music",FSRTstring)->stringwrite(Music);
//	reg->AddKey("Difficulty",FSRTdouble)->writed(Difficulty);
//	reg->AddKey("Tags",FSRTint)->writei(Tags);
//	if (LocalNames) {
//		CFSRegister *r = reg->AddKey("LocalNames",FSRTregister)->GetSubRegister();
//		CFSRegKey *k = LocalNames->GetFirstKey();
//		while (k) {
//			if (k->CFSGetKeyType() == FSRTstring) r->AddKey(k->GetName(),FSRTstring)->stringwrite(k->GetDirectAccess());
//			k = k->GetNextKey();
//		}
//	}
//	if (Directory) reg->AddKey("Directory",FSRTstring)->stringwrite(Directory);
//	if (Password) reg->AddKey("Password",FSRTstring)->stringwrite(Password);
//
//	if (KerMain->SaveState == eSLSsaveLevel) { // sejvnu verzi levlu a level.info
//		CFSRegister *reg2;
//		ver = MMLevelDirs->GetVersionString(LevelFile);
//		str = new char[strlen(LevelFile)+10];
//		sprintf(str,"%s.info",LevelFile);
//		reg->AddKey("LVersion",FSRTstring)->stringwrite(ver);
//
//		reg2 = new CFSRegister(str,"KRKAL LEVEL I",1);
//		if (Author) reg2->AddKey("Author",FSRTstring)->stringwrite(Author);
//		if (Game) reg2->AddKey("Game",FSRTstring)->stringwrite(Game);
//		if (Comment) reg2->AddKey("Comment",FSRTstring)->stringwrite(Comment);
//		reg2->AddKey("Difficulty",FSRTdouble)->writed(Difficulty);
//		reg2->AddKey("Tags",FSRTint)->writei(Tags);
//		if (LocalNames) {
//			CFSRegister *r = reg2->AddKey("LocalNames",FSRTregister)->GetSubRegister();
//			CFSRegKey *k = LocalNames->GetFirstKey();
//			while (k) {
//				if (k->CFSGetKeyType() == FSRTstring) r->AddKey(k->GetName(),FSRTstring)->stringwrite(k->GetDirectAccess());
//				k = k->GetNextKey();
//			}
//		}
//		if (Directory) reg2->AddKey("Directory",FSRTstring)->stringwrite(Directory);
//		if (Password) reg2->AddKey("Password",FSRTstring)->stringwrite(Password);
//		reg2->AddKey("LVersion",FSRTstring)->stringwrite(ver);
//
//		reg2->WriteFile();
//		delete reg2;
//	}
//}