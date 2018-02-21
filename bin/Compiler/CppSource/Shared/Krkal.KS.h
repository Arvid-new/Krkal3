//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.KS - K r k a l . K S 
///
///		Compiled scripts base class
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once
#ifndef __KRKAL_KS_H__
#define __KRKAL_KS_H__


#ifdef KRKALKS_EXPORTS
#define KRKALKS_API __declspec(dllexport)
#else
#define KRKALKS_API __declspec(dllimport)
#endif



#include "CKerMain.h"
#include "KerServices.h"
#include "KerContext.h"
#include <unordered_map>
#include <string>

using namespace stdext;
using namespace std;


typedef unordered_map<string, void (*)()> mPointers_map;
typedef unordered_map<CKerName*, void (*)()> iPointers_map;

class CKrkalKS : public KsInterface {
public:
	CKerMain *KerMain;

	
	virtual void Initialize(CKerMain *kerMain, void *servicesHandle) {
		KerMain = kerMain;
		GetKsParams(KerMain->QueryKsParameters);
		InitInitializations();
		InitializeServices(servicesHandle);
	}
	virtual void (* GetMethodPointer(const char *methodKsf))();
	virtual void (* GetInitializationPointer(CKerName *objectName))();
	virtual void Destruct();
	virtual ~CKrkalKS() {};

protected:
	mPointers_map methodPointers;
	iPointers_map initializations;

	virtual void GetKsParams(CQueryKsParameters *query) = 0;
	virtual void InitInitializations() = 0;
	virtual void InitializeServices(void *servicesHandle) {}

};




KRKALKS_API KsInterface *CreateKs();


#endif