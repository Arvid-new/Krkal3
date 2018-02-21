//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - C K e r n e l P a r a m e t e r s
///
///		This class is used to configure the runtime
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __CKERNELPAREMETRS_H__
#define __CKERNELPAREMETRS_H__


#include "KerConstants.h"
#include "KerErrors.h"

class CKerName;
class CKerMain;
class CKerObject;
class CFS;



class IServices {
public:
	virtual void GetReadyToStart(CKerMain *kerMain) = 0;
};




class KRKALRUNTIME_API CKernelParameters {
friend class CKernelLoader;
public:
	struct SUsedFilesInfo {
		eKerFileType Type;
		string OriginalName;
		::FILETIME FileTime;
	};
	typedef void (*loggingCallBackT)(void *handle, int time, int ErrorNum, int ErrorParam, const char *ErrorStr);
	typedef unordered_map<string, SUsedFilesInfo > UsedFilesT;

	CKernelParameters() {
		DefaultInit();
	}

	CKernelParameters(const char* fileName, eKerRunMode runmode, eKerDebugModes debugmode, loggingCallBackT loggingCallBack, void *handle);

	~CKernelParameters();

	void CallLoggingCB(int time, int ErrorNum, int ErrorParam, const char *ErrorStr) {
		if (LoggingCallBack)
			LoggingCallBack(Handle, time, ErrorNum, ErrorParam, ErrorStr);
	}


	eKerRunMode Runmode;
	eKerDebugModes Debugmode;
	loggingCallBackT LoggingCallBack; 
	void *Handle;
	IServices *Services;

	CKerObject *(*ObjectConstructor)(CKerName *type, CKerMain *KerMain);
	CKerObject *(*ObjectCopyConstructor)(CKerObject *source, CKerMain *KerMain);
	void (*KillHandler)(CKerObject *obj, CKerMain *KerMain);

	bool (*CreateEngineAndServices)(CKerMain *kerMain, const char *engine);

	const char *GetFileName() { return _fileName;}
	void SetFileName(const char* fileName);

	const char *GetEngineName() { return _engineName;}
	void SetEngineName(const char* engineName);

	int GetRootNamesCount() { return _rootNamesCount; }
	const char*GetRootName(int pos) {
		CheckPos(pos);
		return _rootNames[pos];
	}
	void SetRootName(int pos, const char *rootName);
	void AddRootName(const char *rootName);
	void ClearAllRootNames();

	// functions below return information that sets kernel once it is loaded.
	UsedFilesT::iterator UsedFilesBegin() { return _usedFiles->begin(); }
	UsedFilesT::iterator UsedFilesEnd() { return _usedFiles->end(); }
	bool CompareUsedFiles(CKernelParameters *other); // returns true if they are same and file dates match
	bool InitUsedFiles(CFS *fs); // to init used files manually withaut kernel loading
	const char *GetCodeFile() { return _codeFile;}

private:
	char *_engineName;
	char *_fileName;
	char *_codeFile;
	char **_rootNames;
	int _rootNamesCount;
	UsedFilesT *_usedFiles;

	void CheckPos(int pos) {
		if (pos<0 || pos >= _rootNamesCount)
			throw CExc(eKernel, 0, "internal error: index out of range");
	}
	void DefaultInit();
	void FindCodeFile(CKerMain *KerMain);
	void InitDataFiles();
	bool AddDataFile(const char *file, CFS *fs, CKerMain *KerMain = 0);

	// disable = and copyconstructor
	CKernelParameters(const CKernelParameters&);
	CKernelParameters& operator=(const CKernelParameters&);

};





#endif