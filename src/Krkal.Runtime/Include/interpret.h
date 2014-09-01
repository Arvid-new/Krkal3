////////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - I N T E R P R E T   interpretovanych scriptu
///
///		A: Jiri Margaritov (Interpretovani), Honza M.D. Krcek (Loading)
///
////////////////////////////////////////////////////////////////////////////////

#pragma once

#ifndef INTERPRET_H
#define INTERPRET_H

#include "KerErrors.h"


// Velikost pameti pro interpretovane skripty:
//#define KER_INTERPRET_MEMORY_SIZE 8  // 4 Mega 
//#define KER_INTERPRET_MEMORY_SIZE 4194304  // 4 Mega 
#define KER_INTERPRET_MEMORY_SIZE (1024*1024)  // 1 Mega 
#define KER_INTERPRET_MEMORY_AND (KER_INTERPRET_MEMORY_SIZE-1)




//////////////////////////////////////////////////////////////////////////////
///
///		C K e r I n t e r p r e t
///
//////////////////////////////////////////////////////////////////////////////


class CKerHeapFree;
class CKerMain;
class CKerInterpret;



class CStackDealocator
{
public:
	CStackDealocator(CKerInterpret* kerInterpret) {
		KerInterpret = kerInterpret;
		Size=0;
	}
	void Add(size_t size) {
		Size += size;
	}
	~CStackDealocator();
private:
	size_t Size;
	CKerInterpret *KerInterpret;
};



// Hlavni objekt interpretu
class KRKALRUNTIME_API CKerInterpret {
friend CKerHeapFree;
public:
	CKerInterpret(CKerMain *kerMain);
	~CKerInterpret();
	void Init_SetStack(); // Inicializace interpretu


	UC *ISMalloc(unsigned int size); // allokuje pamet na halde. Nikdy nevrati 0, kdyz se malloc nepovede, vyvola Panic Error
	void ISFree(void *pointer);
//	UC *ISMalloc(int CodeLine, unsigned int size) { SET_CODE_LINE(CodeLine); return ISMalloc(size); }// allokuje pamet na halde. Nikdy nevrati 0, kdyz se malloc nepovede, vyvola Panic Error
//	void ISFree(int CodeLine, void *pointer) { SET_CODE_LINE(CodeLine); ISFree(pointer); }

	UC *GetMemory() { return memory; }


	template<typename T>
	T *Push(CStackDealocator &dealocator, size_t size=1) {
		T *ret = (T*)StackTop;
		StackTop += sizeof(T)*size;
		dealocator.Add(sizeof(T)*size);
		if (StackTop>=HeapBottom) LogError(eKRTENoMemory);
		return ret;
	}

	void *Push(size_t sizeInBytes) {
		void *ret = StackTop;
		StackTop += sizeInBytes;
		if (StackTop>=HeapBottom) LogError(eKRTENoMemory);
		return ret;
	}

	void Pop(size_t sizeInBytes) {
		StackTop-=sizeInBytes;
	}

	template<typename T>
	T& Pop() {
		StackTop-=sizeof(T);
		return *(T*)StackTop;
	}


private:


	void LogError(int ErrorNum);


	unsigned char *memory;
	// 0
	unsigned char *CodeStart; // = memory + 0
	  // code
	unsigned char *KerNamesStart; 
	  // KerNames (CKerNames*)
	unsigned char *GlobalsStart;
	  // Pointry na globalni data
	  // Globlani data, ktera nepotrebuji kompilovane skripty
	unsigned char *StackStart;
      // Stack
	unsigned char *StackTop;
	  // Empty Space
	unsigned char *HeapBottom;
	  // Heap
	// memory + KER_INTERPRET_MEMORY_SIZE

	CKerHeapFree *HFstart; // Nejhorejsi volna oblast
	CKerHeapFree *HFend; // vzdy existuje - volna oblast od StackTop do HeapBottom

	CKerMain *KerMain;





};



inline CStackDealocator::~CStackDealocator() {
	KerInterpret->Pop(Size);
}


/////////////////////////////////////////////////////////////////////
///
///		C K e r H e a p F r e e
///		Seznam volneho mista na halde
///		Seznam obsahuje vzdy alespon jeden prvek - ten posledni - Je to misto od StackTop do HeapBottom-1 vcetne
///
/////////////////////////////////////////////////////////////////////

class CKerHeapFree {
	friend CKerInterpret;
private:
	CKerHeapFree(UC *laddr, UC *uaddr, CKerHeapFree *_next, CKerHeapFree *_prev, CKerInterpret *kerInterpret) {
		KerInterpret = kerInterpret;
		loweraddr = laddr; upperaddr = uaddr;  // pridam se do seznamu
		next = _next; prev = _prev;
		if (next) next->prev = this; else KerInterpret->HFend = this;
		if (prev) prev->next = this; else KerInterpret->HFstart = this;
	}
	~CKerHeapFree() {  // odeberu se ze seznamu
		if (next) next->prev = prev; else KerInterpret->HFend = prev;
		if (prev) prev->next = next; else KerInterpret->HFstart = next;
	}
	CKerHeapFree *next; // volne misto na nizsich adresach
	CKerHeapFree *prev; // volne misto na vyssich adresach
	UC *loweraddr;		// adresy volneho mista. Volne misto je od loweraddr do upperaddr-1 vcetne.
	UC *upperaddr;
	CKerInterpret *KerInterpret;
};





#endif
