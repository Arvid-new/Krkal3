////////////////////////////////////////////////////////////////////////////////////////////////
//
// FS.h
//
// veci okolo filesystemu
//
// A: Petr Altman
//
////////////////////////////////////////////////////////////////////////////////////////////////

/*

Co umi filesystem?
------------------
Umoznuje balit soubory a adresare do jednoho souboru archivu.
Archiv se chova stejne jako adresar. Je tedy mozne normalne prochazet adresarovou
strukturou i dovnitr archivu.

Filesystem umoznuje kompresi.
Neumoznuje uzivatelska prava - musi si to zaridit sam uzivatel.

Hlavni objekty:
---------------

class CFS
	hlavni trida FileSystemu
	zastinuje archivy, takze je mozne do nich normalne lizt
	pri startu je vytvorena instance CFS - pointr na ni je v globalni prom. FS
			
class CFSArchive
	reprezentuje jeden archiv
	struktura souboru je popsana na zacatku fs.cpp

*/

#pragma once

#ifndef FS_H
#define FS_H

#include "fs.api.h"
#include <vector>

extern class CFS *FS; //globalni objekt FS




////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSFolder
//
// adresar FS, muze byt na disku, nebo v archivu CFSArchive
//
////////////////////////////////////////////////////////////////////////////////////////////////

//class CFSFolder
//{
//public:
//	CFSFolder();
//	~CFSFolder();
//
//	CFSFolder(const CFSFolder &f);
//
//	// cesta:  'path'+'/'+archivename+'pathinarchive'
//	char *path; //absolutni cesta na disku k akt. adresari, pokud je aktualni adresar v archivu, tak je to jen cesta k archivu, zbytek cesy je v 'pathinarchive'
//	char *archivename; //jmeno archivu (bez cesty)
//	char *pathinarchive; //cesta v aktualnim archivu
//
//	int archivedepth; //hloubka zanoreni v archivu: 0=neni v archivu, 1=rootdir, ...
//
//	int chdir(const char *name, int archive=0); //zmeni adresar; nastavit archive na 1, kdyz se leze do archivu
//												//name muze byt jen jednoduchy (bez '/','\'), podporuje jen '..'
//
//	int SetDir(const char *dir); //nastavi adr, dir je absolutni cesta, neni v archivu
//
//	int GetDir(char **dir, const char *filename=NULL); //do dir ulozi aktualni abs. cestu, dir se alokuje, vola new; vraci 0=err 1=OK
//
//private:
//	int pathlen,pathinarchivelen,archivenamelen; //velikost bufferu
//};


////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSDirMgr
//
// "manazer" adresaru - pocita reference na adresar, zajistuje aktualnost CFSDir
//
////////////////////////////////////////////////////////////////////////////////////////////////

//class CFSDirMgr
//{
//public:
//	CFSDirMgr(CFSArchive *archive);
//	~CFSDirMgr();
//
//	CFSDir* ReadDir(int offset);
//	CFSDir* CreateDir(int parrentoffset);
//
//	void DeleteDir(CFSDir *dir);
//	void DeleteAll();
//
//private:
//	CFSDir* FindDir(int offset);
//
//	void Add(CFSDir *dir);
//
//	CListK<CFSDir*> list;
//	CFSArchive *archive;
//};


////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFSDirRef
//
// umoznuje zapamatovat si aktualni adresar archivu
//
////////////////////////////////////////////////////////////////////////////////////////////////

//class CFSDirRef
//{
//	friend class CFSArchive;
//public:
//	void Close();
//
//private:
//
//	CFSDirRef(CFSDir *dir);
//	~CFSDirRef();
//
//	CFSArchive* GetArchive();
//
//	CFSDir *dir;
//};



class CFSSearchData2
{
private:

	typedef std::vector<std::string> namesT;
	typedef std::vector<int> attrT;

	namesT _names;
	attrT _attribs;


public:
	int count() {
		return (int)_names.size();
	}
	bool GetField(int index, std::string &name, int &attr) {
		if (index<0 || index >= (int)_names.size())
			return false;
		name = _names[index];
		attr = _attribs[index];
		return true;
	}
	void AddField(const char* name, int attr) {
		_names.push_back(std::string(name));
		_attribs.push_back(attr);
	}
};


#endif