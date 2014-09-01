#pragma once

#ifndef NameSearch_H
#define NameSearch_H

#include "types.h"
#include <hash_map>
#include <string>
#include <vector>
#include <set>
#include "PathTree.h"
#include "FS_Macros.h"
#include "fs.api.h"


////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSCfg
//
// definice adresarovych zkratek: $neco$
// nacita se ze souboru (KRKALfs.cfg)
//
////////////////////////////////////////////////////////////////////////////////////////////////

#define FS_MAX_CFGPATH 100
#define FS_MAX_KEYLEN 25
#define FS_MAX_VALLEN 1024

class CFSCfg
{
	friend class CFSDirCache;
public:
	CFSCfg(); //nacte konfiguraci
	~CFSCfg();

	std::string ParseString(const std::string &instr); //vezme vstupni string a nahradi v nem $...$, v pripade chybi vraci ""
	static std::string NormalizePath(const std::string &instr); // converts string to Upper, / to \ and eliminates \..
	static void NormalizePath(const std::string &instr, std::string &outNrm, std::string &outNrmOC); // Normalizes to 2 variants. 1st is uppercase, in second the case is not changed
	const char *FindLastSlash(const char *instr);
	char *FindLastSlash(char *instr);

	void AddKey(std::string key, std::string val); //prida klic
	void RemoveKey(std::string key); // odebere klic
	void MakeKrkalPath(const std::string &UCpath, std::string &pathToChange);

	int ReadCfg(); //nacte konfiguraci

private:

	typedef stdext::hash_map<std::string, std::string> keysT;
	typedef std::vector<std::string> slistT;
	typedef CPathTree<std::string> keysTreeT;

	keysT _keys;
	slistT _roots;
	slistT _searches;
	std::string _emptyStr;
	keysTreeT *_keysTree;

	const std::string &FindKey(const std::string &key);
	static void EliminateUpDirs(std::string &path);

};


enum eFSdirInfo {
	eFSDInone = 0,
	eFSDIroot = 1,
	eFSDIsearch = 2,
	eFSDIarchiveDir = 4,
};

class CFSDirCacheInfo {
public:
	typedef CPathTree<CFSDirCacheInfo> treeT;
	eFSdirInfo _info;
	CFSDirCacheInfo(eFSdirInfo info, CFSDirCache *dirCache, treeT *treeNode) { _info = info; _archiveDir=0; _dirCache=dirCache; _treeNode = treeNode; }
	~CFSDirCacheInfo();
	class CFSDir *_archiveDir;
	void RemoveMapKey(const std::string &key);
	void RemoveAllMapKeys();
	void AddMapKey(const std::string &key) { _keys.insert(key); }
private:
	typedef std::set<std::string> keysT;
	keysT _keys;
	CFSDirCache *_dirCache;
	treeT *_treeNode;
};




class CDirChangesReader {
public:
	enum ResE {
		Reof,
		Roverflow,
		Radded,
		Rdeleted,
	};
	CDirChangesReader(const std::string &dirPath, DWORD notifyFilter);
	~CDirChangesReader();
	void StopReading();
	ResE ReadChangedItem(std::string &name);
	const std::string &GetDirPath() {return _dirPath;}
private:
	static const size_t BUFFSIZE = 8 * 1024;
	HANDLE _h;
	BYTE _buffer[BUFFSIZE];
	BYTE *_buffPtr;
	OVERLAPPED _ol;
	DWORD _bytesReturned;
	DWORD _notifyFilter;
	std::string _dirPath;

	void StartReading();
	ResE ReadOne(std::string &name);
};



class CPathPtr {
private:
	std::string _path;
	std::string _currentPath;
	std::string _fileName;
	size_t _ptr;
	CPathPtr *_pathPtr2;
public:
	CPathPtr(const std::string &path) : _path(path), _currentPath(path) { _ptr = path.size(); _pathPtr2=0;}
	CPathPtr(const std::string &path, const std::string &path2) : _path(path), _currentPath(path) { _ptr = path.size(); _pathPtr2=new CPathPtr(path2);}
	~CPathPtr() { SAFE_DELETE(_pathPtr2); }
	const std::string &FullPath() { return _path; }
	const std::string &CurrentPath() { return _currentPath; }
	const std::string &FileName() { return _fileName; }
	const std::string &FullPath2() { return _pathPtr2->_path; }
	const std::string &CurrentPath2() { return _pathPtr2->_currentPath; }
	const std::string &FileName2() { return _pathPtr2->_fileName; }
	bool IsFullPath() { return _ptr == _path.size(); }
	bool IsOneLeft() { return _ptr + 1 + _fileName.size() == _path.size(); }
	size_t GetPtr() { return _ptr; }
	bool MoveLeft() {
		size_t ptr = _path.rfind('\\', _ptr-1);
		if (!ptr || ptr == std::string::npos)
			return false;
		_currentPath.resize(ptr);
		_fileName.assign(_path, ptr+1, _ptr-ptr-1);
		_ptr = ptr;
		if (_pathPtr2)
			_pathPtr2->MoveLeft();
		return true;
	}
	bool MoveRight() {
		if (_fileName.empty())
			return false;
		size_t ptr = _ptr+1+_fileName.size();
		_currentPath.append(_path, _ptr, ptr - _ptr);
		_ptr = ptr;
		ReadFileName();
		if (_pathPtr2)
			_pathPtr2->MoveRight();
		return true;
	}
	void SetPtr(size_t ptr) {
		if (!ptr || ptr>_path.size() || (ptr<_path.size() && _path[ptr] != '\\'))
			throw CExc(eFS, eFSgeneral, "Invalid parameter ptr for CPathPtr");
		_currentPath.assign(_path, 0, ptr);
		_ptr = ptr;
		ReadFileName();
		if (_pathPtr2)
			_pathPtr2->SetPtr(ptr);
	}

private:
	void ReadFileName() {
		if (_ptr == _path.size()) {
			_fileName.clear();
		} else {
			size_t ptr2 = _path.find('\\', _ptr+1);
			if (ptr2 == std::string::npos)
				ptr2 = _path.size();
			_fileName.assign(_path, _ptr+1, ptr2-_ptr-1);
		}
	}

};



enum EChangeType {
	CTnone,
	CTdeleted,
	CTaddededEmptyDir, // not allowed for archives!!
	CTaddedDir,
	CTaddedFile,
	CTverify,
};


enum EFSGetAccessFlags {
	eGAFcreate = 1,
	eGAFnonRO = 2,
	eGAFfile = 4,
	eGAFdir = 8,
	eGAFdirContent = 16,
	eGAFallowStar = 32,
	eGAFnoArchive = 64,
};

class CGetAccessReturn {
public:
	CGetAccessReturn() { archive = 0; aDir=0; aItem=0; }
	~CGetAccessReturn();
	void CloseArchive();
	class CFSArchive *archive;
	class CFSDir *aDir;
	class CFSDirItem **aItem;
	std::string newPath;
};

class CFSDirCache {
public:
	CFSDirCache(CFSCfg *cfg);
	~CFSDirCache();
	void RereadAll();
	int GetAccess(std::string path, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags);
	bool GetFullPath(std::string path, std::string &fullPath, EFullPathType type = efptWindowsUpperCase);
	void RemoveMapKey(const std::string &key, const std::string &value);
	void InsertChange(const std::string &str, EChangeType type);
	static bool SplitVersion(const std::string &str, std::string *dir, std::string *version, int *versionPos=0);
	void GetRoots(class CFSSearchData2 *sd);
private:
	typedef CPathTree<CFSDirCacheInfo> treeT;
	typedef stdext::hash_multimap<std::string, std::string> mapT;
	typedef std::vector<CDirChangesReader*> readersT;
	typedef CPathTree<EChangeType> changeTreeT;
	
	void Clear();
	CFSDirCacheInfo* Insert(const std::string &str, eFSdirInfo info, bool deleteChildren = false);
	size_t MapSearch(std::string &path, std::string &pathOC);	
	void ReadFileChanges();
	void ReadFileChanges2(CDirChangesReader *reader, EChangeType addedChName);
	void ApplyFileChanges();
	void SearchChanges(changeTreeT *node, std::string &path, bool add);
	EChangeType VerifyFile(const std::string &path, class CFSDir *aDir = 0, size_t aDirLen = 0);
	void AddMapKey(const std::string &dir, const std::string &version, const std::string &value);
	void AddMapKey(CFSDirCacheInfo *cInfo, const std::string &version, const std::string &value);
	bool CheckForArchive(const std::string &path, bool addMapKeys);
	void SearchDir(const std::string &path);
	void SearchArchiveDir(std::string &path, class CFSDir *fsDir, bool addMapKeys);
	
	int GetAccess2(std::string Npath, std::string NpathOC, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags);
	int GetAccesCheckArchive(CPathPtr &Mpath, class CFSDir *dir, CGetAccessReturn &gaReturn, EFSGetAccessFlags flags);
	bool CreateDirectories(CPathPtr &Mpath, EFSGetAccessFlags flags, bool searchDir);

	treeT *_tree;
	changeTreeT *_changeTree;
	mapT _map;
	CFSCfg *_cfg;
	readersT _fileReaders;
	readersT _dirReaders;

};




class CCurDirectorySaver {
public:
	CCurDirectorySaver();
	~CCurDirectorySaver();
private:
	char _dir[MAX_PATH];
};

#endif
