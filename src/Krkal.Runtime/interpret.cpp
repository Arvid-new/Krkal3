////////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - I N T E R P R E T   interpretovanych scriptu
///
///		A: Jiri Margaritov (Interpretovani), Honza M.D. Krcek (Loading)
///
////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "interpret.h"
#include "CKerMain.h"


#define HEAPTAG1 0xCECC5AA6
#define HEAPTAG2 0x663CABC5






////////////////////////////////////////////////////////////////////////////
///
///		C K e r I n t e r p r e t
///
////////////////////////////////////////////////////////////////////////////


/// INICIALIZACE
CKerInterpret::CKerInterpret(CKerMain *kerMain) {
	// inicializace pameti a ukazatelu do ni:
	KerMain = kerMain;
	memory=0;
	memory = new unsigned char[KER_INTERPRET_MEMORY_SIZE];
	CodeStart=memory+0;
	KerNamesStart=memory+0;
	GlobalsStart=memory+0;
	StackStart=memory+0;
	StackTop=memory+0;
	HeapBottom=memory+KER_INTERPRET_MEMORY_SIZE;
	HFstart = 0; HFend = 0;
	new CKerHeapFree(StackTop,HeapBottom,0,0, this);

}





///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////
// Inicializace interpretu. Inicializace zasobniku a haldy
void CKerInterpret::Init_SetStack() { 
	StackStart = StackTop; 
} 


///											/////	
///		P R A C E   S   H A L D O U
///											/////

// MALLOC allokuje pamet na halde
UC *CKerInterpret::ISMalloc(unsigned int size) {
	CKerHeapFree *h = HFstart;
	UI hsize;
	UC *addr;

	size = ((size+12-1)&0xFFFFFFF0)+16; // 12 je na pomocna data (3 inty), adresu dale zvetsim na nejblizsi nasobek 16
	HFend->loweraddr = StackTop;  // Aktualizuju velikost posledniho volneho mista
	while (h && size > (hsize = h->upperaddr-h->loweraddr)) h = h->next; // najdu prvni volne misto, do ktereho se vejdu
	if (!h) { KerMain->Errors->LogError(eKRTENoMemory,size,"malloc"); return 0;}
	addr = h->upperaddr-size;  // adresa, kam umistim vytvareny blok
	if (h->next==0) { // Zvetsuji oblast haldy
		if (size+128 > hsize) { KerMain->Errors->LogError(eKRTENoMemory,size,"malloc"); return 0;}
		HeapBottom = addr;
	}
	((UI*)addr)[0] = HEAPTAG1;  // ulozim tagy a velikost vytvarene oblasti
	((UI*)addr)[1] = size;
	((UI*)addr)[2] = HEAPTAG2;
	if (size == hsize) delete h;  // volne misto jsem zaplnic cele.
	else h->upperaddr -= size;
	return addr+12;
}


//// FREE
void CKerInterpret::ISFree(void *ptr) {
	UC *ptrUp;
	UC *pointer = (UC*)ptr;
	CKerHeapFree *h = HFstart;

	if (!pointer) { KerMain->Errors->LogError(eKRTEFreeToNull); return; } // pointer je NULL
	if (pointer<StackTop || pointer >= memory+KER_INTERPRET_MEMORY_SIZE) { KerMain->Errors->LogError(eKRTEIllegalFree,(int)pointer); return; } // Pointer je mimo pamet
	pointer -=12;
	if (((UI*)pointer)[0] != HEAPTAG1 || ((UI*)pointer)[2] != HEAPTAG2) { KerMain->Errors->LogError(eKRTEIllegalFree,(int)pointer+12); return; }  // Nejsou tam tagy
	while (h && h->upperaddr>pointer) h = h->next; // najdu prvni volne misto pod dealokovanou oblasti
	if (!h) { KerMain->Errors->LogError(eKRTEIllegalFree,(int)pointer+12,"? wierd.."); return; }
	// zjistim velikost a smazu tagy:
	((UI*)pointer)[0]=0; ptrUp = pointer+((UI*)pointer)[1]; ((UI*)pointer)[2]=0;
	// rozsirim volne misto:
	if (h->upperaddr==pointer) {
		if (h->prev && h->prev->loweraddr == ptrUp) {
			h->upperaddr = h->prev->upperaddr;
			delete h->prev;
		} else h->upperaddr = ptrUp;
		HeapBottom = HFend->upperaddr;
	} else {
		if (h->prev && h->prev->loweraddr == ptrUp) h->prev->loweraddr = pointer;
		else new CKerHeapFree(pointer,ptrUp,h,h->prev, this);
	}
}



///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

// DESTRUKTOR
CKerInterpret::~CKerInterpret() {
	while (HFstart) delete HFstart;
	SAFE_DELETE_ARRAY(memory);

}





void CKerInterpret::LogError(int ErrorNum) {
	KerMain->Errors->LogError(eKRTENoMemory);
}
