//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - O P o i n t e r
///
///		Pointer to Krkal object
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "OPointer.h"
#include "CKerMain.h"



OPointer OPointer::Clone(int codeLine, CKerMain *KerMain) const {
	if (!ptr) {
		KerMain->Errors->LogError2(codeLine, eKRTECOnoObj);
		return 0;
	}
	try {
		size_t offset = ptr - (void**)(*ptr);
		CKerObject* obj1 = KerObject();
		CKerObject* obj2;
		if (KerMain->KernelParameters->ObjectCopyConstructor) {
			obj2 = KerMain->KernelParameters->ObjectCopyConstructor(obj1, KerMain);
		} else {
			obj2 = new CKerObject(obj1, KerMain);
		}
		// kopiruju data:
		KerMain->call(codeLine,obj2->thisO,KerMain->CopyConstructor,0); // zavolam copy constructor
		return obj2->thisO + offset;
	} catch (CKernelError err) {
		KerMain->Errors->LogError2(codeLine, err.ErrorNum);
		return 0;
	}
}



void OPointer::Kill(int codeLine, CKerMain *KerMain) const {
	if (!ptr) 
		return;
	KerMain->DeleteObject(codeLine, *this);
}



int OPointer::Compare(const OPointer& other) const {
	if (ptr == other.ptr)
		return 0;
	if (!ptr)
		return -1;
	if (!other.ptr)
		return 1;
	return KerObject()->Compare(other.KerObject());
}
