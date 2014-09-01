//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - R o o t N a m e s
///
///		The single globally accessible runtime hosting root names, dada objects and dependencies
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "RootNames.h"
#include "fs.api.h"
#include "CKerMain.h"
#include "KerServices.h"

using namespace std;
using namespace stdext;





int CRootNames::_refCounter = 0;
CRootNames* CRootNames::_rootNames = 0;
IKrkalResourceManager* CRootNames::ResourceManager = 0;


CRootNames::CRootNames() { 
	InitializeCriticalSection(&_criticalSection);
	_kernel = 0;
	_kernelParameters = 0;
	_startMode = eSMunknown;
	CFS::InitFS();
	_fs = CFS::GetFS();
}



CRootNames::~CRootNames() {
	DeleteCriticalSection(&_criticalSection);
	SAFE_DELETE(_kernel);
	SAFE_DELETE(_kernelParameters);
	_fs = 0;
	CFS::DoneFS();
}


CKerMain *CRootNames::GetKernel() {
	if (!_kernel)
		CreateKernel();
	_kernel->ResetTLSKerMain();
	return _kernel;
}



CKernelParameters *CRootNames::GetKernelParameters() {
	if (!_kernelParameters) {
		_kernelParameters = new CKernelParameters();
		_kernelParameters->Runmode = eKRMDataOnly;
	}
	return _kernelParameters;
}


void CRootNames::CreateKernel() {
	if (_startMode == eSMunknown)
		ReloadAllFromDiskIfNeeded();
	
	GetKernelParameters();

	_kernelParameters->ClearAllRootNames();
	if (_startMode == eSMfromCache) {
		_kernelParameters->AddRootName("$ROOTCACHE$");
	} else {
		CFSSearchData *search = _fs->ReadDirectory("$ROOTFILES$");
		if (search) {
			string file;
			int attr;
			for (int f=0; f< search->count(); f++) {
				search->GetField(f, file, attr);
				if (attr == 1 && file.size() > 5 && _stricmp(file.c_str() + file.size() - 5, ".data") == 0) {
					file = "$ROOTFILES$\\" + file;
					_kernelParameters->AddRootName(file.c_str());
				}
			}
			search->Close();
		}
	}
	
	_kernel = new CKerMain(_kernelParameters);
	_kernel->Load();
	
	if (_startMode == eSMcreateCache) {
		_kernel->Save("$ROOTCACHE$", eKSFsaveRootCache);
		_startMode = eSMfromCache;
	}
}




void CRootNames::ReloadAllFromDiskIfNeeded() {
	_startMode = eSMcreateCache;

	CFSSearchData *search = _fs->ReadDirectory("$ROOTFILES$");
	CSourceFilesIterator *cache = 0;
	if (_kernel) {
		cache = new CSourceFileIteratorKernel(_kernelParameters);
	} else {
		cache = new CSourceFileIteratorRegister( new CFSRegister("$ROOTCACHE$", "KRKAL3 DATA"));
	}

	if (search) {
		int count = 0;
		string file;
		int attr;
		
		for (int f=0; f< search->count(); f++) {
			search->GetField(f, file, attr);
			if (attr == 1 && file.size() > 5 && _stricmp(file.c_str() + file.size() - 5, ".data") == 0)
				count++;
		}

		if (count && cache->GetCount() == count) {
			while (cache->GetNext()) {
				FILETIME ft;
				if (!_fs->GetTime(cache->File(), ft))
					break;
				if (ft.dwLowDateTime != cache->FileTime().dwLowDateTime || ft.dwHighDateTime != cache->FileTime().dwHighDateTime)
					break;
				count--;
			}
			
			if (count == 0) 
				_startMode = eSMfromCache;
		}

	}

	SAFE_DELETE(cache);
	if (search)
		search->Close();
	if (_startMode == eSMcreateCache) {
		if (_kernel)
			SAFE_DELETE(_kernel);
	}
}




bool CRootNames::IsRegisterUpToDate(const char *path) {
	CSourceFileIteratorRegister cache( new CFSRegister(path, 0));
	if (!cache.GetCount())
		return false;

	while (cache.GetNext()) {
		FILETIME ft;
		if (!CFS::GetFS()->GetTime(cache.File(), ft))
			return false;
		if (ft.dwLowDateTime != cache.FileTime().dwLowDateTime || ft.dwHighDateTime != cache.FileTime().dwHighDateTime)
			return false;
	}

	return true;
}



// function searchs all names in layer under rootName. Searched names are of type nameType, if they have attribute File it will be returned as text. You must delete returned vector. Files can be duplicated.
vector<string> *CRootNames::GetFiles(const char *rootName, eKerNameType nameType, bool reloadIfNeeded, CKerMain *toRedirectLogging) {
	if (!rootName)
		return 0;
	CRootNamesCriticalSection criticalSection(this, reloadIfNeeded, toRedirectLogging);

	CKerMain *kernel = GetKernel();

	CKerName *name = kernel->KerNamesMain->GetNamePointer(rootName);
	if (!name)
		return 0;

	vector<string> *ret = new vector<string>();
	char buffer[MAX_PATH];
	CKerNameList *list = kernel->KerNamesMain->FindLayer(name, 0, nameType);
	CKerNameList *list2 = list;
	try {

		for ( ; list; list = list->next) {
			KString file = kernel->ReadAttribute<KString>(list->name, eKKNattrFile);
			if (file && file->GetCount()) {
				if (UnicodeToAnsi(buffer, sizeof(buffer), file->c_str())) {
					ret->push_back(buffer);
				}
			}
		}

		if(list2) list2->DeleteAll();
	} catch (...) {
		if(list2) list2->DeleteAll();
		SAFE_DELETE(ret);
		throw;
	}
	return ret;
}





/////////////////////////////////////////////////////////////////////////////////////////////



CSourceFileIteratorRegister::CSourceFileIteratorRegister(CFSRegister *code) {
	_code = code; _files=0; _dates=0;
	if (_code && _code->GetOpenError() == FSREGOK) {
		_files = _code->FindKey("Source Files");
		_dates = _code->FindKey("Source Files Dates");
		if (!_files || !_dates) {
			_files = 0; _dates = 0;
		}
	}
}

CSourceFileIteratorRegister::~CSourceFileIteratorRegister() {
	SAFE_DELETE(_code);
}

bool CSourceFileIteratorRegister::GetNext() {
	if (!_files || _dates->eof())
		return false;
	if (_dates->pos)
		_files->SetPosToNextString();
	_fileTime.dwLowDateTime = (UI)_dates->readi();
	_fileTime.dwHighDateTime = (UI)_dates->readi();
	return true;
}


const char *CSourceFileIteratorRegister::File() { return _files->GetDirectAccessFromPos();}
FILETIME &CSourceFileIteratorRegister::FileTime() {return _fileTime; }
int CSourceFileIteratorRegister::GetCount() { return (_code && _dates) ? _dates->top / 2 : 0; }




CSourceFileIteratorKernel::CSourceFileIteratorKernel(CKernelParameters *params) {
	_params = params;
	_count = 0;
	_started = false;
	for(_iter = _params->UsedFilesBegin(); _iter != _params->UsedFilesEnd(); ++_iter) {
		_count++;
	}
}



bool CSourceFileIteratorKernel::GetNext() {
	if (!_count)
		return false;
	if (_started) {
		++_iter;
	} else {
		_started = true;
		_iter = _params->UsedFilesBegin();
	}
	return _iter != _params->UsedFilesEnd();
}





//////////////////////////////////////////////////////////////////////////////////////////////////////////


CRootNamesCriticalSection::CRootNamesCriticalSection(CRootNames *rootNames, bool reloadIfNeeded, CKerMain *toRedirectLogging) : MyCriticalSection(rootNames->GetCriticalSection()) {
	_rootNames = rootNames;
	_toRedirectLogging = toRedirectLogging;
	if (_toRedirectLogging)
		_rootNames->RedirectErrorLogging(toRedirectLogging->KernelParameters);
	if (reloadIfNeeded)
		_rootNames->ReloadAllFromDiskIfNeeded();
}
CRootNamesCriticalSection::~CRootNamesCriticalSection() {
	if (_toRedirectLogging) {
		_rootNames->CleraErrorLoggingRedirection();
		_toRedirectLogging->ResetTLSKerMain();
	}
}
