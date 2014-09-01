//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - K e r C o n t e x t
///
///		Calling context and message
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __KER_CONTEXT_H__
#define __KER_CONTEXT_H__

#include "CKerMain.h"

struct CKerMessage;
struct CKerContext;



// Maximalni pocet vnorenych volani. A zaroven maximalni pocet soucasne vytvorenych zprav:
#define KER_CALL_STACK_MAX 500
#define KER_CALL_STACK_WARNING (KER_CALL_STACK_MAX/2)
#define KER_MESSAGES_MAX 5000
#define KER_MESSAGES_WARNING (KER_MESSAGES_MAX/2)




////////////////////////////////////////////////////////////////////////////
///
///		C K e r M e s s a g e
///
////////////////////////////////////////////////////////////////////////////


struct CKerMessage {
	// Vytvoreni zpravy. When - cas vyvolani, pokud when==eKerCTcallend, tak callendObjekt urcuje
	// objekt, po kterem se ma zprava vyvolat. Zprava se vyvola co nejdrive, ale az zkonci vsechny metody objektu callendObject
	CKerMessage(int when, UI time, const OPointer &callendObject, CKerMain *kerMain); // po zkonstruovani je jeste nutne vyplnit Receiver a Method
	~CKerMessage() {
		KerMain->_message_counter--;
		ReleaseArguments();
		SAFE_DELETE_ARRAY(ArgTypes);
		SAFE_DELETE_ARRAY(ArgNames);
		SAFE_DELETE_ARRAY(Args);
	}
	void HoldArguments();
	void ReleaseArguments();
	OPointer Receiver;
	OPointer Sender;
	CKerName *Method;		// KSID metody
	int NumArgs;			// pocet argumentu	
	CLT *ArgTypes;			// pole s typy argumentu
	CKerName **ArgNames;	// Pole s KSID jmeny argumentu
	char *Args;				// Argumenty (jejich hodnota je do tohoto pole prekopirovana)
	int CodeLine;			// radek, odkud byla zprava volana (pro ladeni)
	const char *MethodName;		// ptr na jmeno metody, odkud byla zprava volana (pro ladeni)
	CKerMessage *next;		// dalsi zprava ve fronte
	UI Time;				// cas vyvolani. Nastavuje se u timed messages
	CKerMain *KerMain;
private:
	bool ArgumentsHeld;
};







//////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
///
///		K o n t e x t :
///		kontext aktualne volane funkce. Nove volani = novy kontext. Kontexty tvori zasobnik
///
//////////////////////////////////////////////////////////////////////////////////////////

// Prace s kontextem:
// 1. New (Kontext se prida na zasobnik kontextu KerContext)
// Pro vsechny zpravy:
//    2. InitContext (kontext bude nastaven na konkretni objekt)
//    Pro vsechny Metody pro konkretni objekt:
//      3. Nastav promenne Params, ParamsInfo, MethodName
//      Volani 
//    4. Deinit : nastav prom. KCthis na 0, zavolej KerMain->RunMessages na zpravy v kontextu
// 5. delete (kontext bude odstranen ze zasobniku, stary kontext bude reinitovan)
struct KRKALRUNTIME_API CKerContext {
	CKerContext(CKerMain *kerMain, CKerMessage *msg) { // called from RunMessages
		KerMain = kerMain;
		ParamsArray = 0;
		KerMain->_call_stack_counter++;
		startmq = 0; endmq = 0; MethodName = 0; line = 0;
		parent = KerMain->KerContext;
		Sender = msg->Sender;
		MsgMethodName = msg->MethodName;
		msgline = msg->CodeLine;
		if ((KerMain->_call_stack_counter >= KER_CALL_STACK_WARNING && !KerMain->_call_stack_counter_warning)||KerMain->_call_stack_counter >= KER_CALL_STACK_MAX) _log_error();
		KerMain->KerContext = this; // kontext se automaticky prida na zasobnik
	}

	CKerContext(CKerMain *kerMain) { // called from ISCall
		KerMain = kerMain;
		ParamsArray = 0;
		KerMain->_call_stack_counter++;
		startmq = 0; endmq = 0; MethodName = 0; line = 0;
		parent = KerMain->KerContext;
		Sender = parent ? parent->KCthis : 0;
		MsgMethodName = 0;
		msgline = 0;
		if ((KerMain->_call_stack_counter >= KER_CALL_STACK_WARNING && !KerMain->_call_stack_counter_warning)||KerMain->_call_stack_counter >= KER_CALL_STACK_MAX) _log_error();
		KerMain->KerContext = this; // kontext se automaticky prida na zasobnik
	}

	CKerContext(CKerMain *kerMain, CKerName *mName, const OPointer &thisO, const char *methodName) { // called from DirectMethodInitialization
		KerMain = kerMain;
		ParamsArray = 0;
		KerMain->_call_stack_counter++;
		startmq = 0; endmq = 0; MethodName = methodName; line = 0;
		parent = KerMain->KerContext;
		KCthis = thisO;
		Sender = parent ? parent->KCthis : 0;
		MName = mName;
		MsgMethodName = 0;
		msgline = 0;
		if ((KerMain->_call_stack_counter >= KER_CALL_STACK_WARNING && !KerMain->_call_stack_counter_warning)||KerMain->_call_stack_counter >= KER_CALL_STACK_MAX) _log_error();
		KerMain->KerContext = this; // kontext se automaticky prida na zasobnik
	}


	~CKerContext() { // Odebrani kontextu ze sasobniku, reinicializace stareho kontextu.
		KerMain->_call_stack_counter--;
		KerMain->DeleteMessages(&startmq, &endmq);
		KerMain->KerContext = parent;
	}


	void InitMethod(CStackDealocator &dealocator, CKerMethod *ms, const OPointer &thisO) {
		if (thisO == KerMain->StaticData) {
			KCthis = 0;
		} else {
			KCthis = thisO.Cast(ms->ParentObj);
		}
		line = 0;
		MethodName = ms->FieldName;
		MName = ms->Name;
		Params = KerMain->KerInterpret->Push<UC>(dealocator, ms->ParamSize);
		memset(Params, 0, ms->ParamSize);
		ParamsInfo = KerMain->KerInterpret->Push<UC>(dealocator, ms->NumP);
		ParamsArray = KerMain->KerInterpret->Push<void*>(dealocator, ms->NumP);
		for (int f=0;f<ms->NumP;f++) ParamsArray[f] = Params + ms->Params[f].Offset;
	}

	void InitInitialization(const OPointer &thisO) {
		if (thisO == KerMain->StaticData) {
			KCthis = 0;
		} else {
			KCthis = thisO;
		}
		MethodName = "Initialization";
		MName = 0;
	}


	template <typename T>
	T &ret() { return *(T*)Params; }

	template <typename T>
	T &prm(int index) { return *(T*)ParamsArray[index]; }

	int assigned(int index) { return ParamsInfo[index]; }


	const char *MethodName; // jmeno provadene metody (ptr na string) - pro ladici ucely
	CKerName *MName; // KSID jmeno metody - pro ladici ucely
	int line; // aktualni cislo radky, za zivota kontextu je treba line aktualizovat
	const char *MsgMethodName; // jmeno metody, odkud byla zavolana zprava (ptr na string) - pro ladici ucely
	int msgline; // cislo radky, odkud byla zavolana zprava

	unsigned char *Params;  // Pintr na parametry. 1. je navratova hodnota fce
	void **ParamsArray;		// pole pointru na jednotlive argumenty
	unsigned char *ParamsInfo; // Informace o parametrech. 0-neprirazeno(pouzit default), 1-prirazeno

	CKerMessage *startmq;   // fronta zprav pro aktualni kontext
	CKerMessage *endmq;

	OPointer KCthis;  // this
	OPointer Sender;  // volajici objekt, 0-volano z kernelu, objekt neni znam

	CKerContext *parent;  // Nadrazeny kontext (Kontexty jsou v zasobniku)
	CKerMain *KerMain;
private:
	void _log_error();
};








#endif