#pragma once

#ifndef FS_ARCHIVE_H
#define FS_ARCHIVE_H

#include "fs.api.h"
#include <unordered_map>
#include <set>
#include <string>


////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSDirItem
//
// jedna polozka adresare archivu
//
////////////////////////////////////////////////////////////////////////////////////////////////

class CFSDirItem
{
	friend class CFSDir;
public:

	CFSDirItem(){name=NULL;dir=0;Dir=0;offset=0; next=NULL;}
	~CFSDirItem();

	int Create(const char *name, char dir, int offset);
	int Read(FILE *f);
	
	void CmpCRC(CCRC32 &crc);
	int GetSize();	

	int GetOffset(){return offset;}
	void GetTime(FILETIME &tm){tm=ittime;}
	void SetTime(const FILETIME &tm) {ittime=tm;}
	const char *GetName() {return name;}
	CFSDir* GetDir() { return dir ? Dir : 0; }
	CFSDirItem *GetNextItem() { return next; }

protected:

	int Write(FILE *f);

	char *name;
	unsigned char namelen;
	char dir;
	CFSDir *Dir;
	int offset;
	FILETIME ittime;
	CFSDirItem *next;
};

////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSDir
//
// adresar archivu
//
////////////////////////////////////////////////////////////////////////////////////////////////

class CFSDir
{
	friend class CFSArchive;
public:
	CFSDir(CFSArchive *archive);
	~CFSDir();

	int Create(CFSDir *parent, CFSDirItem *pItem);
	int Read(int offset, CFSDir *parent, CFSDirItem *pItem);

	CFSDir *CreateSubDir(const char *name);

	int WriteFile(const char *name, const char *buf, int size, unsigned char compr);
	int GetFileSize(CFSDirItem **itm);
	int ReadFile(CFSDirItem **itm, char *buf, int bufsize);

	int IsCompressed(CFSDirItem **itm);
	int GetTime(CFSDirItem **itm, FILETIME &item_time);
	int SetTime(CFSDirItem **itm, const FILETIME &item_time);

	int GetSize();
	int GetOffset(){return offset;}
	CFSDir *GetParent(){return Parent;}
	void GetFullPath(std::string &path, const char *fileName);
	void GetFullPath(std::string &path);

	int FileExist(const char *name);

	int FindItem(const char *name,CFSDirItem*** it, int namelen=-1);
	int FindItem(int offset,CFSDirItem*** it);

	int Rename(const char *oldname, const char *newname, CFSDir *newdir);
	int Delete(CFSDirItem **itm);

	int DefragDir(int newoffset); //pouziva se pri defragmentaci

	int GetNumItems(){return numitems;}
	//int GetItem(int itnum, const char **name, int &dir);
	CFSDirItem *GetFirsItem() { return items; }

	void DeleteArchive();
	CFSArchive *GetArchive() { return archive; }

private:

	int Write();
	int WriteDirData();
	int WriteFileData(const char *buf, int size, int oldoffset, unsigned char compr);
	int ReadFileData(int offset, char *buf, int bufsize);

	typedef std::unordered_map<int, CFSDir*> dirOffsetsT;
	void FillDirOffsets(dirOffsetsT &offsets);

	CFSDirItem *items;
	int numitems;
	
	int offset;
	int wrsize;
	CFSDir *Parent;
	CFSDirItem *parentItem;
	
	CFSArchive *archive;
};



////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSEmptySpace, CFSEmptySpaceItem
//
// volne misto uvnitr archivu
//
////////////////////////////////////////////////////////////////////////////////////////////////


struct CFSEmptySpaceItem
{
	int offset;
	int size;
};

class CFSEmptySpace
{
	friend CFSArchive;
public:
	CFSEmptySpace(int maxItems, CFSArchive *archive);
	~CFSEmptySpace();

	int AddEmptySpace(int size, int offset);
	int GetEmptySpace(int size, int &offset);

	int WriteEmptySpace(int offset);
	int ReadEmptySpace(int offset);

	int CmpOffsetAfterDefrag(int oldoffset);

	void DeleteAll(){numItems=0;}

	int NeedDefrag();

private:
	int numItems;
	int maxItems;
	CFSEmptySpaceItem *items;
	CFSArchive *archive;

	void Add(int offset, int size);
	void Delete(int i);
	void Change(int i, int newoffset, int newsize);

	DWORD CmpCRC();
};

////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSArchive
//
// Archiv souboru
//
////////////////////////////////////////////////////////////////////////////////////////////////

class CFSArchive
{
	friend class CFSEmptySpace;
	friend class CFSDir;
public:

	CFSArchive();
	~CFSArchive();

	int Create(const char *path);
	int Open(const char *path, int _readonly);
	int ReOpen(int _readonly);

	void Close();

	int Defragment();

	CFSDir *GetRootDir() { return rootDir; }
	const std::string &GetOpenPath() { return openPath; }


private:

	void WriteArchiveHead();
	int ReadArchiveHead();
	DWORD CmpArchiveHeadCRC(int ver);

	FILE *file;
	int readonly;
	int archivesize;
	std::string openPath;

	int RootDirOffset;
	int EmptySpaceTableOffset;
	int MaxEmptySpaceItems;
	__int64 randomVersion;
	bool archiveChanged;
	
	CFSEmptySpace *emptyspace;
	CFSDir *rootDir;

};




class CArchiveNameVerifier {
public:
	void AddExtension(const char *ext, int type);
	bool IsValidName(const std::string &path);
	void Clear() { _arcgiveExt.clear(); _ignoreExt.clear(); }
private:
	typedef std::set<std::string> strSetT;
	strSetT _arcgiveExt;
	strSetT _ignoreExt;
};

#endif