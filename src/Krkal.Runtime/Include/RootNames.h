//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - R o o t N a m e s
///
///		The single globally accessible runtime hosting root names, dada objects and dependencies
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __ROOTNAMES_H__
#define __ROOTNAMES_H__


#include "types.h"
#include "Krkal.Runtime.h"
#include "CKernelParameters.h"

class CKerMain;
class CKernelParameters;
class CFS;
class CFSRegister;
class CFSRegKey;




class KRKALRUNTIME_API IKrkalResourceManager {
public:
	virtual const wchar_t *GetUserNameOrComment(const char* ksid, bool isComment) = 0;
	virtual const wchar_t *GetText(const wchar_t* text) = 0;
	virtual const wchar_t *GetText(const char *ksid, const wchar_t *defaultText) = 0;
	virtual bool ReloadIfNeeded() = 0;
};




class KRKALRUNTIME_API CRootNames {
public:
	static IKrkalResourceManager* ResourceManager;

	static CRootNames *GetRootNames() { return _rootNames;}
	static void InitRootNames() {
		if (_rootNames == 0)
			_rootNames = new CRootNames();
		_refCounter++;
	}
	static void DoneRootNames() {
		_refCounter--;
		if (_refCounter <= 0)
			SAFE_DELETE(_rootNames);
	}


	// You should lock all work with this sigleton and with contained runtime inside CriticalSection.
	// For easy work use the helper class MyCriticalSection or RootNamesCriticalSection
	CRITICAL_SECTION *GetCriticalSection() { return &_criticalSection; }
	
	CKerMain *GetKernel();
	void ReloadAllFromDiskIfNeeded();

	vector<string> *GetFiles(const char *rootName, eKerNameType nameType, bool reloadIfNeeded, CKerMain *toRedirectLogging = 0); // function searchs all names in layer under rootName. Searched names are of type nameType, if they have attribute File it will be returned as text. You must delete returned vector. Files can be duplicated.

	void RedirectErrorLogging(CKernelParameters *otherParams) { GetKernelParameters()->LoggingCallBack = otherParams->LoggingCallBack; GetKernelParameters()->Handle = otherParams->Handle; }
	void CleraErrorLoggingRedirection() { GetKernelParameters()->LoggingCallBack = 0; GetKernelParameters()->Handle = 0; }

	static bool IsRegisterUpToDate(const char *path);

private:
	static CRootNames *_rootNames;
	static int _refCounter;
	
	
	CRootNames();
	~CRootNames();
	CKernelParameters *GetKernelParameters();

	CRITICAL_SECTION _criticalSection;
	CKerMain *_kernel;
	CKernelParameters *_kernelParameters;
	CFS *_fs;

	void CreateKernel();

	enum eStartMode {
		eSMunknown,
		eSMfromCache,
		eSMcreateCache,
	};
	eStartMode _startMode;

};




class CRootNamesCriticalSection : public MyCriticalSection {
public:
	CRootNamesCriticalSection(CRootNames *rootNames, bool reloadIfNeeded, CKerMain *toRedirectLogging = 0);
	~CRootNamesCriticalSection();
private:
	CRootNames *_rootNames;
	CKerMain *_toRedirectLogging;
};


class CSourceFilesIterator {
public:
	virtual bool GetNext() = 0;
	virtual const char *File() = 0;
	virtual ::FILETIME &FileTime() = 0;
	virtual int GetCount() = 0;
	virtual ~CSourceFilesIterator() {};
};


class CSourceFileIteratorRegister : public CSourceFilesIterator {
public:
	CSourceFileIteratorRegister(CFSRegister *code);
	virtual ~CSourceFileIteratorRegister();
	virtual bool GetNext();
	virtual const char *File();
	virtual ::FILETIME &FileTime();
	virtual int GetCount();

private:
	CFSRegister *_code;
	::FILETIME _fileTime;
	CFSRegKey *_files;
	CFSRegKey *_dates;
};



class CSourceFileIteratorKernel : public CSourceFilesIterator {
public:
	CSourceFileIteratorKernel(CKernelParameters *params);
	virtual ~CSourceFileIteratorKernel(){}
	virtual bool GetNext();
	virtual const char *File() { return _iter->first.c_str(); }
	virtual ::FILETIME &FileTime() {return _iter->second.FileTime; }
	virtual int GetCount() {return _count;}

private:
	CKernelParameters *_params;
	CKernelParameters::UsedFilesT::iterator _iter;
	int _count;
	bool _started;
};



#endif
