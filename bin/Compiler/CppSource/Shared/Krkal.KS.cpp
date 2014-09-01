//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.KS - K r k a l . K S 
///
///		Compiled scripts base class
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "Krkal.KS.h"




void CKrkalKS::Destruct() {
	delete this;
}


void (* CKrkalKS::GetMethodPointer(const char *methodKsf))() {
	mPointers_map::iterator i = methodPointers.find(methodKsf);
	if (i == methodPointers.end()) {
		return 0;
	} else {
		return i->second;
	}

}


void (* CKrkalKS::GetInitializationPointer(CKerName *objectName))() {
	iPointers_map::iterator i = initializations.find(objectName);
	if (i == initializations.end()) {
		return 0;
	} else {
		return i->second;
	}
}
