//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - C K e r n e l P a r a m e t e r s
///
///		This class is used to configure the runtime
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "CKernelParameters.h"
#include "Loading.h"
#include "CKerMain.h"
#include "fs.api.h"



void CKernelParameters::DefaultInit() {
	Runmode = eKRMNormal;
	Debugmode = eKerDBDebug;
	LoggingCallBack = 0;
	Handle = 0;

	CreateEngineAndServices = 0;
	Services = 0;
	ObjectConstructor = 0;
	ObjectCopyConstructor = 0;
	KillHandler = 0;
	_fileName = 0;
	_engineName = 0;
	_codeFile = 0;
	_rootNames = 0;
	_rootNamesCount = 0;
	_usedFiles = 0;
}



CKernelParameters::CKernelParameters(const char* fileName, eKerRunMode runmode, eKerDebugModes debugmode, loggingCallBackT loggingCallBack, void *handle) {
	DefaultInit();
	Runmode = runmode;
	Debugmode = debugmode;
	LoggingCallBack = loggingCallBack;
	Handle = handle;
	_fileName = newstrdup(fileName);
}


CKernelParameters::~CKernelParameters() {
	SAFE_DELETE_ARRAY(_fileName);
	SAFE_DELETE_ARRAY(_engineName);
	SAFE_DELETE_ARRAY(_codeFile);
	SAFE_DELETE(_usedFiles);
	ClearAllRootNames();
}


void CKernelParameters::SetFileName(const char* fileName) {
	SAFE_DELETE_ARRAY(_fileName);
	_fileName = newstrdup(fileName);
}

void CKernelParameters::SetEngineName(const char* engineName) {
	SAFE_DELETE_ARRAY(_engineName);
	_engineName = newstrdup(engineName);
}



void CKernelParameters::SetRootName(int pos, const char *rootName) {
	CheckPos(pos);
	SAFE_DELETE_ARRAY(_rootNames[pos]);
	_rootNames[pos] = newstrdup(rootName);
}



void CKernelParameters::AddRootName(const char *rootName) {
	char **rn = new char*[_rootNamesCount+1];
	for (int f=0; f<_rootNamesCount; f++) {
		rn[f] = _rootNames[f];
	}
	rn[_rootNamesCount] = newstrdup(rootName);
	SAFE_DELETE_ARRAY(_rootNames);
	_rootNames = rn;
	_rootNamesCount++;
}


void CKernelParameters::ClearAllRootNames() {
	for (int f=0; f < _rootNamesCount; f++) {
		SAFE_DELETE_ARRAY(_rootNames[f]);
	}
	SAFE_DELETE_ARRAY(_rootNames);
	_rootNamesCount = 0;
}



void CKernelParameters::FindCodeFile(CKerMain *KerMain) {
	bool found = false;
	for (UsedFilesT::iterator f = _usedFiles->begin(); f != _usedFiles->end(); ++f) {
		if (f->second.Type == eKFTcode) {
			SAFE_DELETE_ARRAY(_codeFile);
			_codeFile = newstrdup(f->second.OriginalName.c_str());
			if (found)
				KerMain->Errors->LogError(eKRTEtwoCodeFiles);
			found = true;
		} 
	}
}




bool CKernelParameters::AddDataFile(const char *file, CFS *fs, CKerMain *KerMain) {
	char *file2=0;
	string file3;
	SUsedFilesInfo info;

	if (!fs->GetFullPath(file, &file2, efptInvariantKey)) {
		file3 = string("$SCRIPTS$\\") + file;
		if (!fs->GetFullPath(file3.c_str(), &file2, efptInvariantKey)) {
			if (KerMain)
				KerMain->Errors->LogError(eKRTEELoadingScripts, 0, file);
			return false;
		} else {
			info.OriginalName = file3;
			file3 = file2;
			SAFE_DELETE_ARRAY(file2);
		}
	} else {
		info.OriginalName = file;
		file3 = file2;
		SAFE_DELETE_ARRAY(file2);
	}

	info.Type = CKernelLoader::GetFileType(file3.c_str());
	if (info.Type != eKFTdata && info.Type != eKFTcode && info.Type != eKFTlevel) {
		KerMain->Errors->LogError(eKRTEunknownFileType, 0, file);
		return false;
	}

	if (!fs->GetTime(file3.c_str(), info.FileTime)) {
		if (KerMain)
			KerMain->Errors->LogError(eKRTEPELoadingScripts, 0, file);
		return false;
	}

		
	_usedFiles->operator [](file3) = info;
	return true;
}






void CKernelParameters::InitDataFiles() { 
	SAFE_DELETE(_usedFiles); 
	_usedFiles = new UsedFilesT();
}



// returns true if they are same and file dates match
bool CKernelParameters::CompareUsedFiles(CKernelParameters *other) {
	if (!other || !_usedFiles || !other->_usedFiles)
		return false;
	if (_usedFiles->size() != other->_usedFiles->size())
		return false;

	for (UsedFilesT::iterator i = _usedFiles->begin(); i != _usedFiles->end(); ++i) {
		UsedFilesT::iterator j = other->_usedFiles->find(i->first);
		if (j == other->_usedFiles->end())
			return false;
		if (i->second.FileTime.dwHighDateTime != j->second.FileTime.dwHighDateTime || i->second.FileTime.dwLowDateTime != j->second.FileTime.dwLowDateTime)
			return false;
	}

	return true;
}



// to init used files manually withaut kernel loading
bool CKernelParameters::InitUsedFiles(CFS *fs) {
	return CKernelLoader::InitSourceFiles(this, fs);
}
