#include "stdafx.h"
#include "NameSearch.h"
#include <stdio.h>
#include "FS_Macros.h"
#include <io.h>
#include "archive.h"
#include "fs.h"

using namespace std;
using namespace stdext;


CFSCfg::CFSCfg()
{
	_keysTree = new keysTreeT;
	ReadCfg();
}

CFSCfg::~CFSCfg()
{
	_keysTree->DeleteAll();
}

int CFSCfg::ReadCfg()
{
	_keysTree->DeleteChildren();
	_keys.clear();
	_roots.clear();
	_searches.clear();
	FS->archiveNameVerifier->Clear();

	char buff[MAX_PATH];
	char buff2[MAX_PATH];
	GetModuleFileName(0, buff, sizeof(buff));
	char *p = FindLastSlash(buff);

	AddKey("cfg", string(buff, p-buff));

	strcpy_s(p+1, MAX_PATH - (p-buff+1), "KRKALfs.cfg");

	FILE *f=FOPEN(buff,"rt");
	if(f) {
		while (!feof(f)) {
			if (fscanf(f, "%259[a-zA-Z_0-9] = \"%259[^\n\"]", buff, buff2) == 2) {
				if (strcmp(buff, "root") == 0) {
					_roots.push_back(NormalizePath(ParseString(string(buff2))));
				} else if (strcmp(buff, "search") == 0) {
					_searches.push_back(NormalizePath(ParseString(string(buff2))));
				} else if (strcmp(buff, "archiveExt") == 0) {
					FS->archiveNameVerifier->AddExtension(buff2, 0);
				} else if (strcmp(buff, "archiveIgExt") == 0) {
					FS->archiveNameVerifier->AddExtension(buff2, 1);
				} else {
					AddKey(buff, buff2);
				}
			}
			fscanf(f,"%*[^\n]\n"); // readln
		}
		fclose(f);
	}

	return 1;
}

void CFSCfg::AddKey(string key, string val) //prida klic
{
	RemoveKey(key);
	string parsed = ParseString(val);
	_keys[key] = parsed;
	keysTreeT *ptr = _keysTree->Insert(NormalizePath(parsed));
	if (!ptr->GetContent()) {
		string *s = new string("$");
		s->append(key);
		s->push_back('$');
		ptr->GetContent() = s;
	}
}



void CFSCfg::RemoveKey(std::string key) {
	keysT::iterator i = _keys.find(key);
	if (i != _keys.end()) {
		keysTreeT *ptr = _keysTree->Find(NormalizePath(i->second));
		// NOTE: there is potencional bug if two keys point to the same path. When the first key is removed it is not replaced by the second.
		if (ptr && ptr->GetContent()) {
			string s("$");
			s.append(key);
			s.push_back('$');
			if (*ptr->GetContent() == s) {
				ptr->Delete();
			}
		}
	}
}


string CFSCfg::ParseString(const string &instr)
{
	string ret;
	size_t pos;
	bool esc = false;

	for (size_t f=0; f<instr.size(); f++) {
		if (instr[f] == '$') {
			if (!esc) {
				esc = true;
				pos = f+1;
			} else {
				esc = false;
				string s = FindKey(instr.substr(pos, f-pos));
				if (s.empty())
					return string(); // key not found - return ""
				ret.append(s);
			}
		} else if (!esc) {
			ret.push_back(instr[f]);
		}
	}
	return ret;
}


 // converts string to Upper, / to \ and eliminates \..
string CFSCfg::NormalizePath(const string &instr) {
	string ret(instr);
	for (size_t f=0; f < ret.size(); f++) {
		if (ret[f] == '/') {
			ret[f] = '\\';
		} else {
			ret[f] = toupper(ret[f]);
		}
	}

	EliminateUpDirs(ret);

	return ret;
}


// Normalizes to 2 variants. 1st is uppercase, in second the case is not changed
void CFSCfg::NormalizePath(const string &instr, string &outNrm, string &outNrmOC) {
	outNrmOC = instr;

	for (size_t f=0; f < outNrmOC.size(); f++) {
		if (outNrmOC[f] == '/')
			outNrmOC[f] = '\\';
	}

	EliminateUpDirs(outNrmOC);

	outNrm = outNrmOC;

	for (size_t f=0; f < outNrm.size(); f++) {
		outNrm[f] = toupper(outNrm[f]);
	}
}


void CFSCfg::EliminateUpDirs(std::string &path) {
	size_t pos = 1; // search from the 2nd character
	while ((pos = path.find("\\..", pos)) != string::npos) {
		if (pos >= 1 && (pos+3 >= path.length() || path[pos+3] == '\\')) {
			size_t pos2 = path.rfind('\\', pos-1); // find beginning of the upper dir
			if (pos2 == string::npos) {
				pos++; // upper dir not found -> cannot kill \..
			} else {
				path.erase(pos2, pos-pos2+3); // erase "\dir\.."
				pos = pos2;
			}
		} else {
			pos++;
		}
	}
}




const char *CFSCfg::FindLastSlash(const char *instr) {
	const char *p = 0;
	for ( ; *instr; instr++) {
		if (*instr == '/' || *instr == '\\')
			p=instr;
	}
	return p;
}
char *CFSCfg::FindLastSlash(char *instr) {
	char *p = 0;
	for ( ; *instr; instr++) {
		if (*instr == '/' || *instr == '\\')
			p=instr;
	}
	return p;
}


const string &CFSCfg::FindKey(const string &key)
{
	keysT::const_iterator i = _keys.find(key);
	if (i == _keys.end()) {
		return _emptyStr;
	} else {
		return i->second;
	}
}



void CFSCfg::MakeKrkalPath(const std::string &UCpath, std::string &pathToChange) {
	keysTreeT *ptr = _keysTree->GetFirstPrefix(UCpath);
	if (ptr) {
		pathToChange.replace(0, ptr->GetTotalLen()-1, *ptr->GetContent());
	}
}




//////////////////////////////////////////////////////////////////////////////////////////////////////////

CDirChangesReader::CDirChangesReader(const string &dirPath, DWORD notifyFilter) {
	_notifyFilter = notifyFilter;
	_buffPtr = 0;
	_dirPath = dirPath;


	_h = CreateFile(dirPath.c_str(), FILE_LIST_DIRECTORY, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OVERLAPPED, NULL);
	if (_h == INVALID_HANDLE_VALUE)
		throw CExc(eFS,eFSread,"Can't open search directory!");

	try {
		StartReading();
	} catch (...) {
		StopReading();
		throw;
	}
}

void CDirChangesReader::StopReading() {
	if (_h != INVALID_HANDLE_VALUE) {
		//CancelIoEx(_h, NULL); // not supported in WIN XP !!
		CancelIo(_h);
		CloseHandle(_h);
		_h = INVALID_HANDLE_VALUE;
	}
}


CDirChangesReader::~CDirChangesReader() {
	StopReading();
}


void CDirChangesReader::StartReading() {
	memset(&_ol, 0, sizeof(OVERLAPPED));
	if (!ReadDirectoryChangesW(_h, _buffer, BUFFSIZE, TRUE, _notifyFilter, &_bytesReturned, &_ol, NULL)) {
		throw CExc(eFS,eFSread,"Can't read search directory!");
	}
}



CDirChangesReader::ResE CDirChangesReader::ReadChangedItem(std::string &name) {
	ResE res = Reof;

	if (_h == INVALID_HANDLE_VALUE)
		return res;

	if (_buffPtr) {
		res = ReadOne(name);
		if (!_buffPtr)
			StartReading();
	} else if (GetOverlappedResult(_h, &_ol, &_bytesReturned, FALSE)) {
		if (_bytesReturned == 0) {
			res = Roverflow;
		} else {
			_buffPtr = _buffer;
			res = ReadOne(name);
		}
		if (!_buffPtr)
			StartReading();
	} else {
		if (GetLastError() != ERROR_IO_INCOMPLETE)
			throw CExc(eFS,eFSread,"Can't read search directory!");
	}
	
	return res;
}


CDirChangesReader::ResE CDirChangesReader::ReadOne(std::string &name) {
	FILE_NOTIFY_INFORMATION *info = (FILE_NOTIFY_INFORMATION*)_buffPtr;

	_buffPtr = info->NextEntryOffset ? _buffPtr + info->NextEntryOffset : 0;

	int size = WideCharToMultiByte(CP_ACP, 0, info->FileName, info->FileNameLength / sizeof(WCHAR), 0, 0, 0, 0);
	char *str = new char[size];
	WideCharToMultiByte(CP_ACP, 0, info->FileName, info->FileNameLength / sizeof(WCHAR), str, size, 0, 0);
	name.assign(str, size);
	delete[] str;

	if (info->Action == FILE_ACTION_REMOVED || info->Action == FILE_ACTION_RENAMED_OLD_NAME) {
		return Rdeleted;
	} else {
		return Radded;
	}
	
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////


CFSDirCache::CFSDirCache(CFSCfg *cfg) {
	_cfg = cfg;
	_tree = new treeT();
	_changeTree = new changeTreeT();
}
CFSDirCache::~CFSDirCache() {
	Clear();
	_tree->DeleteAll();
	_changeTree->DeleteAll();
}



void CFSDirCache::Clear() {
	_map.clear();
	_tree->DeleteChildren();
	_changeTree->DeleteChildren();
	
	for (size_t f=0; f<_fileReaders.size(); f++) {
		//delete _fileReaders[f]; // keep memory leak rather than memory corruption
		_fileReaders[f]->StopReading();
	}
	_fileReaders.clear();
	for (size_t f=0; f<_dirReaders.size(); f++) {
		//delete _dirReaders[f]; // keep memory leak rather than memory corruption
		_dirReaders[f]->StopReading();
	}
	_dirReaders.clear();
}


void CFSDirCache::RereadAll() {
	CFSCfg::slistT::const_iterator i;

	Clear();

	for (i=_cfg->_roots.begin(); i != _cfg->_roots.end(); ++i) {
		if (!i->empty() && _access_s(i->c_str(),0) == 0) {
			if (!_tree->GetFirstPrefix(*i)) {
				Insert(*i, eFSDIroot, true);
			}
		} // else try to create dir - future idea
	}


	for (i=_cfg->_searches.begin(); i != _cfg->_searches.end(); ++i) {
		if (!i->empty()) {
			treeT *ptr = _tree->GetFirstPrefix(*i);
			if (ptr && !(ptr->GetContent()->_info & eFSDIsearch) && _access_s(i->c_str(),0) == 0) {
				Insert(*i, eFSDIsearch, true);
			}
		} 
	}

	for (i=_cfg->_searches.begin(); i != _cfg->_searches.end(); ++i) {
		if (!i->empty() && _tree->Find(*i)) {
			try {
				CDirChangesReader *freader = new CDirChangesReader(*i, FILE_NOTIFY_CHANGE_FILE_NAME);
				try {
					CDirChangesReader *dreader = new CDirChangesReader(*i, FILE_NOTIFY_CHANGE_DIR_NAME);
					_fileReaders.push_back(freader);
					_dirReaders.push_back(dreader);
					InsertChange(*i, CTaddedDir);
				} catch (...) {
					//delete freader; // keep memory leak rather than memory corruption
					freader->StopReading();
					throw;
				}
			} catch (CExc) {/* maybe it should be removed from _tree*/}
		} 
	}


	ApplyFileChanges();
}



void CFSDirCache::GetRoots(class CFSSearchData2 *sd) {
	CFSCfg::slistT::const_iterator i;
	treeT *ptr;
	string fp;
	for (i=_cfg->_roots.begin(); i != _cfg->_roots.end(); ++i) {
		if (!i->empty() && (ptr=_tree->Find(*i))) {
			if (!ptr->GetNextPrefix()) {
				if (GetFullPath(*i, fp, efptKrkalOriginalCase)) {
					sd->AddField(fp.c_str(), 2);
				}
			}
		} 
	}
}




void CFSDirCache::ReadFileChanges() {
	readersT::const_iterator i;

	for (i=_dirReaders.begin(); i!=_dirReaders.end(); ++i) {
		ReadFileChanges2(*i, CTaddedDir);
	}
	for (i=_fileReaders.begin(); i!=_fileReaders.end(); ++i) {
		ReadFileChanges2(*i, CTaddedFile);
	}

	ApplyFileChanges();
}


void CFSDirCache::ReadFileChanges2(CDirChangesReader *reader, EChangeType addedChName) {
	string item;
	string item2;
	CDirChangesReader::ResE res;

	while ((res = reader->ReadChangedItem(item)) != CDirChangesReader::Reof) {
		if (res == CDirChangesReader::Roverflow) {
			InsertChange(reader->GetDirPath(), CTverify);
			break;
		} else {
			item2 = reader->GetDirPath();
			item2.push_back('\\');
			item2.append(CFSCfg::NormalizePath(item));
			InsertChange(item2, res == CDirChangesReader::Rdeleted ? CTdeleted : addedChName);
		}
	}
}



void CFSDirCache::InsertChange(const std::string &str, EChangeType type) {
	changeTreeT *ptr = _changeTree->GetFirstPrefix(str);
	if (ptr && ptr->GetTotalLen() < str.length() && *ptr->GetContent() != CTaddededEmptyDir)
		return;

	ptr = _changeTree->Insert(str);
	EChangeType *ct = ptr->GetContent();
	if (!ct) {
		ct = new EChangeType(CTnone);
		ptr->GetContent() = ct;
	}

	if (*ct == CTnone) {
		*ct = type;
	} else if (*ct == CTaddededEmptyDir && (type == CTaddededEmptyDir || type == CTaddedDir)) {
		// keep CTaddededEmptyDir
	} else if (*ct != type) {
		*ct = CTverify;
	}

	if (*ct != CTaddededEmptyDir)
		ptr->DeleteChildren();
}




void CFSDirCache::ApplyFileChanges() {
	string path;
	SearchChanges(_changeTree, path, false);
	SearchChanges(_changeTree, path, true);
	_changeTree->DeleteChildren();
}


void CFSDirCache::SearchChanges(changeTreeT *node, std::string &path, bool add) {
	size_t oldSize = path.size();
	path.append(node->GetText());
	
	if (node->GetContent() && *node->GetContent() != CTnone) {
		string dir;
		string version;
		treeT *p;
		if (!add) {
			if (SplitVersion(path, &dir, &version)) {
				p = _tree->Find(dir);
				if (p && p->GetContent())
					p->GetContent()->RemoveMapKey(version);
			}
			p = _tree->FindChildren(path);
			if (p) {
				if (!p->GetNextPrefix()) { // don't delete the root
					p->DeleteChildren(false);
					if (p->GetContent())
						p->GetContent()->RemoveAllMapKeys();
				} else {
					p->DeleteChildren(true);
				}
			}

		} else if (*node->GetContent() != CTdeleted) {

			string pathNoSlash = path.substr(0, path.size()-1);
			EChangeType type = *node->GetContent();

			treeT *p = _tree->GetFirstPrefix(path);
			bool addMapKeys = ((p->GetContent()->_info & eFSDIsearch) != 0);

			if (p->GetTotalLen() == path.size())
				p = p->GetNextPrefix(); // if it is archive directory I want one dir up
			CFSDir *aDir = 0;
			if (p && (p->GetContent()->_info & eFSDIarchiveDir)) {
				aDir = p->GetContent()->_archiveDir;
				if (pathNoSlash.find('\\', p->GetTotalLen()) != string::npos)
					throw CExc(eFS, eFSgeneral, "unexpected error in Search Changes! File Name contains \\");
			}

			if (type == CTverify)
				type = VerifyFile(pathNoSlash, aDir, p->GetTotalLen());
			if (type == CTaddededEmptyDir || type == CTaddedDir || type == CTaddedFile) {
				if (addMapKeys && SplitVersion(path, &dir, &version))
					AddMapKey(dir, version, pathNoSlash);
				if (type == CTaddedDir) {
					if (aDir) {
						// here archive is not opened - nothing new is reed - no ugly recursion
						// CTaddededEmptyDir is not allowed for archives - allways recursivelly search aDir
						CFSDirItem **item;
						if (aDir->FindItem(pathNoSlash.substr(p->GetTotalLen()).c_str(), &item) && (**item).GetDir()) 				
							SearchArchiveDir(pathNoSlash, (**item).GetDir(), addMapKeys);
					} else {
						SearchDir(pathNoSlash);
					}
				} else if (!aDir && type == CTaddedFile) {
					CheckForArchive(pathNoSlash, addMapKeys);
				}
			}
		}
	}

	for (size_t f=0; f<node->ChildrenCount(); f++) {
		SearchChanges(node->GetChild(f), path, add);
	}

	path.resize(oldSize);
}


EChangeType CFSDirCache::VerifyFile(const std::string &path, CFSDir *aDir, size_t aDirLen) {
	if (aDir) {

		CFSDirItem **item;
		if (!aDir->FindItem(path.substr(aDirLen).c_str(), &item))
			return CTdeleted;
		return (**item).GetDir() ? CTaddedDir : CTaddedFile;

	} else {

		WIN32_FIND_DATA findData;
		HANDLE h = FindFirstFile(path.c_str(), &findData);
		if (h == INVALID_HANDLE_VALUE)
			return CTdeleted;
		FindClose(h);
		if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
			return CTaddedDir;
		} else {
			return CTaddedFile;
		}

	}
}


bool CFSDirCache::CheckForArchive(const std::string &path, bool addMapKeys) {
	if (!FS->archiveNameVerifier->IsValidName(path))
		return false;

	CFSArchive *archive = new CFSArchive();
	if (archive->Open(path.c_str(), TRUE)) {
		SearchArchiveDir(string(path), archive->GetRootDir(), addMapKeys);
		archive->Close();
		return true;
	} else {
		delete archive;
		return false;
	}

}


void CFSDirCache::SearchDir(const std::string &path) {
	WIN32_FIND_DATA findData;
	string s = path + "\\*";
	string s2 = path + '\\';
	size_t s2_oldL = s2.size();
	
	HANDLE h = FindFirstFile(s.c_str(), &findData);
	if (h == INVALID_HANDLE_VALUE)
		return;

	try {
		do {
			if (findData.cFileName[0] != '.') {// names beginning with . are for FS invisible
				s = CFSCfg::NormalizePath(string(findData.cFileName));
				s2.resize(s2_oldL);
				s2.append(s);
				if (SplitVersion(s, 0, 0))
					AddMapKey(path, s, s2);
				if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
					SearchDir(s2);
				} else  {
					CheckForArchive(s2, true);
				}
			}
		} while (FindNextFile(h, &findData));
	} catch (...) {
		FindClose(h);
		throw;
	}
	FindClose(h);
}


void CFSDirCache::SearchArchiveDir(string &path, CFSDir *fsDir, bool addMapKeys) {
	size_t oldSize = path.size();
	path.push_back('\\');

	CFSDirCacheInfo *cInfo = Insert(path, addMapKeys ? (eFSdirInfo)(eFSDIarchiveDir|eFSDIsearch) : eFSDIarchiveDir);
	cInfo->_archiveDir = fsDir;

	for (CFSDirItem *item = fsDir->GetFirsItem(); item; item = item->GetNextItem()) {
		if (item->GetName()[0] != '.') {
			string s(item->GetName());
			bool vOK = addMapKeys && SplitVersion(s,0,0);
			if (vOK || item->GetDir()) {
				s = CFSCfg::NormalizePath(s);
				path.resize(oldSize+1);
				path.append(s);
				if (vOK)
					AddMapKey(cInfo, s, path);
				if (item->GetDir())
					SearchArchiveDir(path, item->GetDir(), addMapKeys);
			}
		}
	}

	path.resize(oldSize);
}



CFSDirCacheInfo* CFSDirCache::Insert(const std::string &str, eFSdirInfo info, bool deleteChildren) {
	treeT *ptr = _tree->Insert(str);
	CFSDirCacheInfo  *ret;
	if (ret = ptr->GetContent()) {
		ret->_info = (eFSdirInfo)(ret->_info | info);
	} else {
		ret = new CFSDirCacheInfo(info, this, ptr);
		ptr->GetContent() = ret;
	}
	if (deleteChildren) {
		ptr->DeleteChildren();
	}

	return ret;
}






int CFSDirCache::GetAccess(string path, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags) {
	string Npath, NpathOC;
	CFSCfg::NormalizePath(_cfg->ParseString(path), Npath, NpathOC);
	bool starDetected = false;
	
	if (Npath.empty())
		return 0;
	if (*Npath.rbegin() == '\\')
		Npath.resize(Npath.size()-1);
	if (Npath.empty())
		return 0;
	if (*Npath.rbegin() == '*') {
		if (Npath.size() < 3 || Npath.rbegin()[1] != '\\' || !(flags & eGAFallowStar))
			return 0;
		starDetected = true;
		flags = (EFSGetAccessFlags)((flags & ~(eGAFfile | eGAFdir)) | eGAFdirContent);
		Npath.resize(Npath.size()-2);
	}
	NpathOC.resize(Npath.size());

	ReadFileChanges();

	int ret = GetAccess2(Npath, NpathOC, gaReturn, flags);
	if (ret == -1)
		ret = GetAccess2(Npath, NpathOC, gaReturn, flags);
	if (ret == -1)
		ret = 0;

	if (ret && starDetected)
		ret = 4;

	return ret;
}



int CFSDirCache::GetAccess2(std::string Npath, std::string NpathOC, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags) {
	MapSearch(Npath, NpathOC);
	if (Npath.empty())
		return 0;
	CPathPtr Mpath(Npath, NpathOC);

	treeT *tptr = _tree->GetFirstPrefix(Mpath.FullPath());
	// Problem (toto) if somebody changes archive file from outside, I have out of date keys from that archive
	if (!tptr)
		return 0;

	// in archive
	if (tptr->GetContent()->_info & eFSDIarchiveDir) {
		if (flags & eGAFnoArchive)
			return 0;
		CFSDir *dir = tptr->GetContent()->_archiveDir;
		CFSArchive *archive = dir->GetArchive();
		int res = archive->ReOpen(!(flags & eGAFnonRO));
		if (res == 2) {
			Mpath.SetPtr(tptr->GetTotalLen()-1);
			int ret = GetAccesCheckArchive(Mpath, dir, gaReturn, flags);
			if (!ret)
				archive->Close();
			return ret;
		} else if (res == 1) {
			string path = archive->GetOpenPath();
			tptr = _tree->Find(path);

			tptr->DeleteChildren(false);
			tptr->GetContent()->RemoveAllMapKeys();
			dir = archive->GetRootDir();
			SearchArchiveDir(path, dir, (tptr->GetContent()->_info & eFSDIsearch) != 0);
			archive->Close();
			
			return -1;

		} else {
			treeT *tptr2 = _tree->Find(archive->GetOpenPath());
			tptr = tptr2->GetNextPrefix();
			tptr2->DeleteChildren(true);
			if (!tptr)
				return 0;
		}
	} 

	// assuming tptr is out of archive its root dir or search dir. That dir exists.

	// out of archive
	for (;;) {
		switch (VerifyFile(Mpath.CurrentPath())) {
			case CTaddedFile:
				if (CheckForArchive(Mpath.CurrentPath(), (tptr->GetContent()->_info & eFSDIsearch) != 0))
					return -1; // do it again
				if (!Mpath.IsFullPath()) // detected file name in the middle of the path
					return 0;
				if (flags & eGAFfile) { // want a file
					// verify that's correctly inside a root dir
					if (!Mpath.MoveLeft() || !_tree->GetFirstPrefix(Mpath.CurrentPath()))
						return 0;
					gaReturn.newPath = Mpath.FullPath2();
					return 1;
				} else {
					return 0;
				}
			case CTaddedDir:
				if (!Mpath.IsFullPath()) {
					if (flags & eGAFcreate) {
						if (!CreateDirectories(Mpath, flags, (tptr->GetContent()->_info & eFSDIsearch) != 0))
							return 0;
						gaReturn.newPath = Mpath.FullPath2();
						return 2;
					} else {
						return 0;
					}
				} else {
					if (flags & (eGAFdir | eGAFdirContent)) {
						if (flags & eGAFdir) {
							// verify that's correctly inside a root dir
							if (!Mpath.MoveLeft() || !_tree->GetFirstPrefix(Mpath.CurrentPath()))
								return 0;
						}
						gaReturn.newPath = Mpath.FullPath2();
						return 2;
					} else {
						return 0;
					}
				}
			default:
				if (Mpath.GetPtr() <= tptr->GetTotalLen())
					return 0; // end loop - no valid dir found
				// test parent dir:
				if (!Mpath.MoveLeft())
					return 0;
				// loop again
		}
	}
}



bool CFSDirCache::CreateDirectories(CPathPtr &Mpath, EFSGetAccessFlags flags, bool searchDir) {
	for (;;) {
		if (Mpath.IsFullPath())
			return true;
		if (Mpath.IsOneLeft() && !(flags & eGAFdirContent))
			return true;

		Mpath.MoveRight();
		if (!CreateDirectory(Mpath.CurrentPath2().c_str(), 0))
			return false;
		if (searchDir) {
			InsertChange(Mpath.CurrentPath(), CTaddededEmptyDir);
		}
	}
}




int CFSDirCache::GetAccesCheckArchive(CPathPtr &Mpath, class CFSDir *dir, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags) {
	CFSDirItem **item=0;
	int ok = 0;
	if (flags & eGAFcreate) {
		for (;;) {
			if (Mpath.IsFullPath()) break;
			if (Mpath.IsOneLeft() && !(flags & eGAFdirContent)) break;
			if (!(dir = dir->CreateSubDir(Mpath.FileName2().c_str())))
				return 0;
			Mpath.MoveRight();
		}
	}

	if (Mpath.IsFullPath()) {
		
		if (flags & eGAFdirContent) {
			ok = 2;
		} else if (flags & eGAFdir) {
			if (!Mpath.MoveLeft())
				return 0;
			gaReturn.archive = dir->GetArchive();
			dir = dir->GetParent();
			if (!dir) {
				ok = 3;
			} else {
				dir->FindItem(Mpath.FileName().c_str(), &item);
				ok = 2;
			}
		} else {
			return 0;	
		}

	} else if (Mpath.IsOneLeft()) {
		
		if (dir->FindItem(Mpath.FileName().c_str(), &item)) {
			if (!(**item).GetDir() && (flags & eGAFfile)) {
				ok = 1;
			} else {
				return 0;
			}
		} else if (flags & eGAFcreate) {
			ok = 2;
		} else {
			return 0;
		}

	} else {
		return 0;
	}

	gaReturn.aDir = dir;
	gaReturn.aItem = item;
	if (ok != 3) {
		gaReturn.archive = dir->GetArchive();
		gaReturn.newPath = Mpath.FileName2();
	} else {
		gaReturn.newPath = Mpath.FullPath2();
	}
	return ok;
}



bool CFSDirCache::GetFullPath(string path, string &fullPath, EFullPathType type) {
	fullPath.clear();
	if (type == efptInvariantKey) {
		CGetAccessReturn gaRet;
		if (!GetAccess(path, gaRet, (EFSGetAccessFlags)(eGAFfile | eGAFdirContent)))
			return false; // file doesnt exist
	} else {
		ReadFileChanges();
	}

	string Npath, NpathOC;
	CFSCfg::NormalizePath(_cfg->ParseString(path), Npath, NpathOC);
	if (Npath.empty())
		return false;

	size_t keySize = MapSearch(Npath, NpathOC);
	if (!_tree->GetFirstPrefix(Npath))
		return false;

	switch (type) {
		case efptWindowsUpperCase:
			fullPath = Npath;
			return true;
		case efptWindowsOriginalCase:
			fullPath = NpathOC;
			return true;
		case efptKrkalUpperCase:
			fullPath = Npath;
			_cfg->MakeKrkalPath(Npath, fullPath);
			return true;
		case efptKrkalOriginalCase:
			fullPath = NpathOC;
			_cfg->MakeKrkalPath(Npath, fullPath);
			return true;
		case efptInvariantKey:
			if (keySize < Npath.size()) {
				fullPath = Npath.substr(Npath.size()-keySize);
			} else {
				fullPath = Npath;
				_cfg->MakeKrkalPath(Npath, fullPath);
			}
			return true;
		case efptInvariantKeyOriginalCase:
			if (keySize < Npath.size()) {
				fullPath = NpathOC.substr(NpathOC.size()-keySize);
			} else {
				fullPath = NpathOC;
				_cfg->MakeKrkalPath(Npath, fullPath);
			}
			return true;
		default:
			return false;
	}

}





size_t CFSDirCache::MapSearch(std::string &path, std::string &pathOC) {
	size_t pos2 = path.length();
	size_t pos1;
	while (pos2 != string::npos && pos2 > 0) {
		pos1 = path.rfind('\\', pos2-1);
		size_t pos = pos1 == string::npos ? 0 : pos1+1;
		pair<mapT::const_iterator,mapT::const_iterator> i = _map.equal_range(path.substr(pos, pos2-pos)); 
		if (i.first != i.second) {
			if (i.first != --i.second) { // duplicite key (there are 2 files of the same name) - function will fail
				path = ""; pathOC = "";
				return 0;
			}
			size_t ret = path.size() - pos;
			size_t vSize = i.first->second.size();
			for ( ; pos2>0 && vSize > 0 && i.first->second[vSize-1] == path[pos2-1]; pos2--, vSize--) ;
			path.replace(0,pos2, i.first->second, 0, vSize);
			pathOC.replace(0,pos2, i.first->second, 0, vSize);
			return ret;
		}
		pos2 = pos1;
	}
	return path.size();
}



void CFSDirCache::RemoveMapKey(const std::string &key, const std::string &value) {
	pair<mapT::iterator,mapT::iterator> i = _map.equal_range(key);
	for ( ; i.first != i.second; ++i.first) {
		if (i.first->second == value) {
			_map.erase(i.first);
			break;
		}
	}
}

void CFSDirCache::AddMapKey(const std::string &dir, const std::string &version, const std::string &value) {
	if (!_tree->GetFirstPrefix(dir))
		return;
	//mapT::const_iterator i = _map.find(version);
	//if (i != _map.end())
	//	throw CExc(eFS,eFSgeneral,"Found duplicated version file!");

	Insert(dir, eFSDIsearch)->AddMapKey(version);
	_map.insert(mapT::value_type(version,value));
}

void CFSDirCache::AddMapKey(CFSDirCacheInfo *cInfo, const std::string &version, const std::string &value) {
	//mapT::const_iterator i = _map.find(version);
	//if (i != _map.end())
	//	throw CExc(eFS,eFSgeneral,"Found duplicated version file!");

	cInfo->AddMapKey(version);
	_map.insert(mapT::value_type(version,value));
}


bool ishexa(int c)
{
	return (c>='0' && c <='9') || (c>='A' && c<='F') || (c>='a' && c<='f');
}


bool CFSDirCache::SplitVersion(const std::string &str, std::string *dir, std::string *version, int *versionPos) {
	int counter = 0;
	int pos = (int)str.length()-1;
	if (pos >= 0 && str[pos] == '\\' || str[pos] == '/')
		pos--;
	int endPos = pos;

	for (; pos >= 0; pos--) {
		if (str[pos] == '\\' || str[pos] == '/')
			return false;
		bool ok = false;
		
		if (str[pos] == '.') {
			counter=-1;
			ok=true;
		} else if (counter >= 0) {
			if (counter % 5 == 4) {
				if (str[pos] == '_') ok=true;
			} else {
				if (ishexa(str[pos])) ok=true;
			}
		}

		if (ok) {
			counter++;
			if (counter == 20) {
				if (versionPos)
					*versionPos = pos;
				if (dir && version) {
					for ( ; pos >= 0 && str[pos] != '\\' && str[pos] != '/'; pos--);
					*version = str.substr(pos+1, endPos-pos);
					*dir = str.substr(0,pos+1);
				}
				return true;
			}
		} else {
			counter=-1;
		}
	}

	return false;
}


//////////////////////////////////////////////////////////////////////////////////////////////////////////





CCurDirectorySaver::CCurDirectorySaver() {
	GetCurrentDirectory(sizeof(_dir), _dir);
}


CCurDirectorySaver::~CCurDirectorySaver() {
	SetCurrentDirectory(_dir);
}





//////////////////////////////////////////////////////////////////////////////////////////////////////////






CFSDirCacheInfo::~CFSDirCacheInfo() {
	if (_info == eFSDIarchiveDir && _archiveDir) {
		_archiveDir->DeleteArchive();
		_archiveDir = 0;
	}

	RemoveAllMapKeys();
}



void CFSDirCacheInfo::RemoveMapKey(const std::string &key) {
	keysT::iterator i = _keys.find(key);
	if (i != _keys.end()) {
		_keys.erase(i);
		string value = _treeNode->GetTotalText();
		value.append(key);
		_dirCache->RemoveMapKey(key, value);
	}
}



void CFSDirCacheInfo::RemoveAllMapKeys() {
	string value = _treeNode->GetTotalText();
	size_t size = value.size();
	for (keysT::const_iterator i = _keys.begin(); i != _keys.end(); ++i) {
		value.resize(size);
		value.append(*i);
		_dirCache->RemoveMapKey(*i, value);
	}
	_keys.clear();
}




CGetAccessReturn::~CGetAccessReturn() {
	CloseArchive();
}

void CGetAccessReturn::CloseArchive() {
	if (archive)
		archive->Close();
	archive = 0;
}
