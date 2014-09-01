//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - K e r C o n t e x t
///
///		Calling context and message
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "KerContext.h"




/////////////////////////////////////////////////////////////////////////////////
///
///		C K e r M e s s a g e
///
/////////////////////////////////////////////////////////////////////////////////

// Vytvoreni zpravy. When - cas vyvolani, pokud when==eKerCTcallend, tak callendObjekt urcuje
// objekt, po kterem se ma zprava vyvolat. Zprava se vyvola co nejdrive, ale az zkonci vsechny metody objektu callendObject
CKerMessage::CKerMessage(int when, UI time, const OPointer &callendObject, CKerMain *kerMain) {
	ArgumentsHeld = false;
	KerMain = kerMain;
	CKerContext *KerContext = KerMain->KerContext;
	CKerContext *ctx, *ctx2;
	CKerMessage **m;
	// Zjistim jestli zprav uz nebylo vytvoreno priliz moc:
	KerMain->_message_counter++;
	if (KerMain->_message_counter >= KER_MESSAGES_WARNING && !KerMain->_message_counter_warning && KerMain->_RunTurn) { KerMain->Errors->LogError(eKRTEmessagew); KerMain->_message_counter_warning = 1;}
	if (KerMain->_message_counter >= KER_MESSAGES_MAX && KerMain->_RunTurn) KerMain->Errors->LogError(eKRTEmessage);
	// Vyplnim nektere polozky zpravy:
	if (KerContext) {
		Sender = KerContext->KCthis;
		MethodName = KerContext->MethodName;
		CodeLine = KerContext->line;
	} else {
		Sender = 0;
		assert (when != eKerCTcallend);
		MethodName = 0;
		CodeLine = 0;
	}
	assert (when != eKerCTnow);
	NumArgs=0;
	ArgTypes=0;
	ArgNames=0;
	Args=0;
	if (when==eKerCTtimed) {
		time = time + KerMain->Time;
		if (time <= KerMain->Time) when = eKerCTmsg; // casovane zprave uz vyprsel cas, udelam z ni normalni zpravu
	}
	if (when==eKerCTcallend) {
		// najdu prvni posledni kontext, kde je callendObject pouzivan
		ctx = KerContext; // je to aktualni kontext, pokud zadny takovy nenajdu.
		if (callendObject) {
			ctx2 = ctx->parent;
			while (ctx2) {
				if (ctx2->KCthis == callendObject) ctx = ctx2;
				ctx2 = ctx2->parent;
			}
		}
		// a tam pridam zpravu
		if (!ctx->startmq) ctx->startmq=this;
		else ctx->endmq->next = this;
		next=0;
		ctx->endmq = this;
	} else if (when==eKerCTtimed) {
		// zpravu zatridim mezi casovane zpravy
		m = &(KerMain->timedmsgs);
		while (*m && (**m).Time <= time) m = &((**m).next);
		Time = time;
		next = *m;
		*m = this;
	} else {
		// zpravu pridam do jedne ze 4 standardnich front
		if (!KerMain->startmq[when]) KerMain->startmq[when]=this;
		else KerMain->endmq[when]->next = this;
		next=0;
		KerMain->endmq[when] = this;
	}
}






void CKerMessage::HoldArguments() {
	if (ArgumentsHeld)
		return;
	ArgumentsHeld = true;
	if (Receiver)
		KerMain->GarbageCollector->Hold(Receiver.KerObject());
	if (Sender)
		KerMain->GarbageCollector->Hold(Sender.KerObject());
	int offset = 0;
	for (int f=0; f<NumArgs; f++) {
		if (ArgTypes[f].DimCount > 0) {
			CKerArr<int>* arr = *(CKerArr<int>**)(Args + offset);
			if (arr)
				KerMain->GarbageCollector->Hold(arr);
		} else if (ArgTypes[f].Type == eKTobject) {
			OPointer obj = *(OPointer*)(Args + offset);
			if (obj)
				KerMain->GarbageCollector->Hold(obj.KerObject());
		}
		offset+=ArgTypes[f].SizeOf();
	}
}




void CKerMessage::ReleaseArguments() {
	if (!ArgumentsHeld)
		return;
	ArgumentsHeld = false;
	if (Receiver)
		KerMain->GarbageCollector->Release(Receiver.KerObject());
	if (Sender)
		KerMain->GarbageCollector->Release(Sender.KerObject());
	int offset = 0;
	for (int f=0; f<NumArgs; f++) {
		if (ArgTypes[f].DimCount > 0) {
			CKerArr<int>* arr = *(CKerArr<int>**)(Args + offset);
			if (arr)
				KerMain->GarbageCollector->Release(arr);
		} else if (ArgTypes[f].Type == eKTobject) {
			OPointer obj = *(OPointer*)(Args + offset);
			if (obj)
				KerMain->GarbageCollector->Release(obj.KerObject());
		}
		offset+=ArgTypes[f].SizeOf();
	}
}





///////////////////////////////////////////////////////////////////////////
///
///		C K e r C o n t e x t
///
///////////////////////////////////////////////////////////////////////////

// Pomocna funkce, ktara hlasi errory pri priliz mnoha vnorenych volani
void CKerContext::_log_error() {
	if (KerMain->_call_stack_counter >= KER_CALL_STACK_WARNING && !KerMain->_call_stack_counter_warning) { KerMain->Errors->LogError(eKRTEcallstackw); KerMain->_call_stack_counter_warning = 1;}
	if (KerMain->_call_stack_counter >= KER_CALL_STACK_MAX) KerMain->Errors->LogError(eKRTEcallstack);
}

