////////////////////////////////////////////////////////////////////////////////////////////////
//
// FS.cpp
//
// veci okolo filesystemu
//
// A: Petr Altman
//
////////////////////////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"
#include "fs.h"

#include <string.h>
#include <io.h>
#include <direct.h>
#include <string>

#include <math.h>
#include <zlib.h>

#include "NameSearch.h"
#include "FS_Macros.h"
#include "archive.h"
#include "MersenneTwister.h"
#include "VersionFeatures.h"


using namespace std;
using namespace stdext;
CFS *FS=NULL;





//////////////////////////////////////////////////////////////////////////////////////////
/*

	Compressed Single File (komprimovany soubor, ktery neni v archivu)
	----------------------
	offset	size	desc.
	0		15		"KRKAL_COMPFILE"+0
	15		5		"v1.0"+0
	20		4		uncompressed len
	24		4		compressed len
	28		4		head CRC
	32		complen	compressed DATA

*/						   
const char sFS_COMPFILE[]="KRKAL_COMPFILE";
const char sFS_COMPFILE_VER[]="v1.0";
const int FS_COMPFILE_HEADLEN=sizeof(sFS_COMPFILE)+sizeof(sFS_COMPFILE_VER)+4+4+4; 

////////////////////////////////////////////////////////////////////////////////////////////////////


int CFS::RefCounter=0;

int CFS::InitFS()
{
	if (FS == 0)
		FS=new CFS();
	RefCounter++;
	return 1;
}
void CFS::DoneFS()
{
	RefCounter--;
	if (RefCounter <= 0)
		SAFE_DELETE(FS);
}


CFS *CFS::GetFS()
{
	return FS;
}


CFS::CFS()
{

	InitializeCriticalSection(&fsCriticalSection);

	archiveNameVerifier = new CArchiveNameVerifier();
	mtr = new MTRand();
	FS = this;
	versionFeatures = new CVersionFeatures();
	cfg=new CFSCfg();
	dirCache = new CFSDirCache(cfg);
	dirCache->RereadAll();
	
}

CFS::~CFS()
{

	SAFE_DELETE(cfg);
	SAFE_DELETE(dirCache);
	SAFE_DELETE(mtr);
	SAFE_DELETE(versionFeatures);
	SAFE_DELETE(archiveNameVerifier);

	DeleteCriticalSection(&fsCriticalSection);

}

void CFS::AddFSDir(const char *key, const char *val)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	if(!cfg) return;
	cfg->AddKey(key,val);
}


void CFS::AddArchiveExtension(const char *ext, int type) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	archiveNameVerifier->AddExtension(ext, type);
}



// add requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
void CFS::AddVersionFeature(eVersionFeature type, const char* header, const char* feature, int version) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	versionFeatures->AddFeature(type, header ? header : "", feature, version);
}


// remove requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
void CFS::RemoveVersionFeature(eVersionFeature type, const char* header, const char* feature) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	versionFeatures->RemoveFeature(type, header ? header : "", feature);
}


// check if version of features of the file matches version of features of the system
bool CFS::CheckVersionFeatures(CFSRegister *reg) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	return versionFeatures->CheckFeatures(reg);
}


// writes versions of features
void CFS::WriteVersionFeatures(CFSRegister *reg) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	versionFeatures->WriteFeatures(reg);
}





int CFS::IsValidFilename(const char *filename)
{
	if(!filename) return 0;

	const UC *c=(UC*)filename;
	if(*c==0) return 0;
	while(*c)
	{
		if(!isprint(*c)) return 0;
		switch(*c){
			case '\\':
			case '?':
			case '|':
			case '>':
			case '<':
			case ':':
			case '/':
			case '*':
			case '"':
				return 0;
		}
		c++;
	}
	return 1;


}

int CFS::ReadFile(const char *name, char *buf, int bufsize)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	int ok=1;
	int ln,ln2;
	if(!buf) return 0;
	CGetAccessReturn gaReturn;

	ok=dirCache->GetAccess(name, gaReturn, eGAFfile);
	if(ok)
	{
		//otevru soubor
		if(gaReturn.aDir)
		{
			ok=gaReturn.aDir->ReadFile(gaReturn.aItem,buf,bufsize);
		}else{
			FILE *f;
			f=FOPEN(gaReturn.newPath.c_str(),"rb");
			if(!f) ok=0;
			else{
				ok =1;
				ln=_filelength(_fileno(f));

				if(ln>bufsize) ln2=bufsize; else ln2=ln;

				if(ln>=FS_COMPFILE_HEADLEN)
				{

					int hok=0;
					char hd[FS_COMPFILE_HEADLEN];
					char *head;
					if(bufsize<FS_COMPFILE_HEADLEN) head=hd; else head=buf;

					READ(head,FS_COMPFILE_HEADLEN,f);

					CCRC32 CRC;
					
					DWORD *crc=(DWORD*)(head+FS_COMPFILE_HEADLEN-4);
					int size = * (int*)(head+sizeof(sFS_COMPFILE)+sizeof(sFS_COMPFILE_VER));
					int compsize = * (int*)(head+sizeof(sFS_COMPFILE)+sizeof(sFS_COMPFILE_VER)+4);

					if(memcmp(head,sFS_COMPFILE,sizeof(sFS_COMPFILE))==0&&memcmp(head+sizeof(sFS_COMPFILE),sFS_COMPFILE_VER,sizeof(sFS_COMPFILE_VER))==0)
						hok=1;

					CRC.Cmp(head,FS_COMPFILE_HEADLEN-4);
					
					if(*crc==CRC)
					{
						//compressed file
						char *cmpbuf=new char[compsize];
						uLongf ucs=bufsize;

						READ(cmpbuf,compsize,f);

						if(uncompress((Bytef*)buf,&ucs,(Bytef*)cmpbuf,compsize)!=Z_OK)
							ok=0;

						if(size!=ucs) ok=2;

						delete[] cmpbuf;

					}else{
						//uncompressed file
						if(bufsize<FS_COMPFILE_HEADLEN){
							memcpy(buf,head,bufsize);
						}else
						READ(buf+FS_COMPFILE_HEADLEN,ln2-FS_COMPFILE_HEADLEN,f);
					}
				}else{
					if(ln2>0) READ(buf,ln2,f);
				}

				if(ln>bufsize&&ok==1) ok=2;
			
				fclose(f);
			}


		}
	
	}

	return ok;
}

int CFS::WriteFile(const char *name, char *buf, int size, int compr)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	if(!buf) return 0;
	if(compr!=-1&&compr!=0&&compr!=1) compr=-1;
	CGetAccessReturn gaReturn;

	if (!dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFcreate | eGAFfile | eGAFnonRO) ))
		return 0;

	//zapisu soubor
	if(gaReturn.aDir)
	{
		if(!gaReturn.aDir->WriteFile(gaReturn.newPath.c_str(),buf,size,compr)) 
			return 0;
	}else{

		if(compr==-1)
		{
			compr=IsCompressed(name);
			if(compr==-1) compr=1;
		}

		FILE *f;
		f=FOPEN(gaReturn.newPath.c_str(),"wb");
		if(!f) 
			return 0;

		if(compr!=1)
		{
			WRITE(buf,size,f);
		}else{

			int compsize;
			DWORD crc;CCRC32 CRC;
			char *compdata;
			uLongf compbufsize;

			compbufsize=(int)ceil(size*1.01)+12;

			compdata = new char [compbufsize];
			compress((Bytef*)compdata,&compbufsize,(const Bytef*)buf,size);

            compsize=compbufsize;

			WRITE(sFS_COMPFILE,sizeof(sFS_COMPFILE),f);
			WRITE(sFS_COMPFILE_VER,sizeof(sFS_COMPFILE_VER),f);

			WRITE(&size,4,f);
			WRITE(&compsize,4,f);

			CRC.Cmp(sFS_COMPFILE,sizeof(sFS_COMPFILE));
			CRC.Cmp(sFS_COMPFILE_VER,sizeof(sFS_COMPFILE_VER));
			CRC.Cmp(&size,4);
			CRC.Cmp(&compsize,4);

			crc=CRC;

			WRITE(&crc,4,f);

			WRITE(compdata,compsize,f);

			delete[]compdata;

		}
			
		fclose(f);
	
	}

	return 1;
}

int CFS::Delete(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	int ok=1;
	
	CGetAccessReturn gaReturn;

	ok=dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFfile | eGAFdir | eGAFnonRO | eGAFallowStar));
	if(ok)
	{
		if(gaReturn.aDir)
		{
			//jsem v archivu
			ok = gaReturn.aDir->Delete(gaReturn.aItem);
		}else{
			switch (ok) {
				case 1: // file
					ok = DeleteFile(gaReturn.newPath.c_str());
					break;
				case 2: // dir
					ok = DeleteDirContent(gaReturn.newPath);
					if(ok)
						ok = RemoveDirectory(gaReturn.newPath.c_str()); //smazu adresar
					break;
				case 3: // archive
					gaReturn.archive->Close();
					gaReturn.archive = 0;
					ok = DeleteFile(gaReturn.newPath.c_str());
					break;
				case 4: // star
					ok = DeleteDirContent(gaReturn.newPath);
					break;
				default:
					ok=0;
			}
		}
	}

	return ok;
}


bool CFS::DeleteDirContent(const std::string &dir) {
	WIN32_FIND_DATA findData;
	string s = dir + "\\*";
	
	HANDLE h = FindFirstFile(s.c_str(), &findData);
	if (h == INVALID_HANDLE_VALUE)
		return true;

	size_t sLen = s.size()-1;

	do {
		if (strcmp(findData.cFileName, ".") && strcmp(findData.cFileName, "..")) {
			s.resize(sLen);
			s.append(findData.cFileName);
			if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
				if (!DeleteDirContent(s) || !RemoveDirectory(s.c_str()))
					return false;
			} else  {
				if (!DeleteFile(s.c_str()))
					return false;
			}
		}
	} while (FindNextFile(h, &findData));
	FindClose(h);

	return true;
}



int CFS::CopyTree(const char *sourcedir, const char *destdir, ECopyTreeChangeMode changeMode, const char *ext, int comp, const char *newFileName)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	CFSDirItem **aItem=0;

	if (changeMode == ectcmAddExtension || changeMode == ectcmRemoveExtension || changeMode == ectcmChangeExtension)
		if (!ext || !*ext)
			return 0;

	int sRet=0;
	{
		CGetAccessReturn gaRet;
		if (!(sRet = dirCache->GetAccess(sourcedir, gaRet, (EFSGetAccessFlags)(eGAFdir | eGAFfile | eGAFallowStar))))
			return 0;
		aItem = gaRet.aItem;
	}
	{
		CGetAccessReturn gaRet;
		if (!dirCache->GetAccess(destdir, gaRet, (EFSGetAccessFlags)(eGAFdirContent | eGAFcreate | eGAFnonRO)))
			return 0;
	}

	string sfp, dfp;
	if (!dirCache->GetFullPath(sourcedir, sfp) || !dirCache->GetFullPath(destdir, dfp))
		return 0;
	if (*sfp.rbegin() == '*')
		sfp.resize(sfp.size()-1);
	if (*sfp.rbegin() != '\\')
		sfp.push_back('\\');
	if (*dfp.rbegin() != '\\')
		dfp.push_back('\\');

	//if destination is inside source -> abort
	if (dfp.size() >= sfp.size() && dfp.compare(0, sfp.size(), sfp) == 0)
		return 0;

	sfp.resize(sfp.size()-1); // remove the ending slash
	if (sRet == 4) {
		dfp.resize(dfp.size()-1);
	} else {
		// no star
		string name;
		if (newFileName && *newFileName) {
			name = newFileName;
		} else {
			if (aItem) {
				name = (**aItem).GetName(); // get the the real name from archive
			} else {
				WIN32_FIND_DATA fdata; // read the real name from disk
				HANDLE h = FindFirstFile(sfp.c_str(), &fdata);
				if (h == INVALID_HANDLE_VALUE)
					return 0;
				FindClose(h);
				name = fdata.cFileName;
			}
			if (!CopyChangeName(name, changeMode, ext))
				return 0;
		}
		dfp.append(name);
	}

	char *buff=0;
	size_t buffLen=0;
	int ok=1;

	try {
		switch (sRet) {
			case 3: // archive
				CreateArchive(dfp.c_str());
				ok = CopyTree2(dfp, sfp, buff, buffLen, changeMode, ext, comp);
				break;
			case 1: // file
				ok = CopyOneFile(dfp, sfp, buff, buffLen, comp);
				break;
			case 2:
			case 4: // dir and star
				ok = CopyTree2(dfp, sfp, buff, buffLen, changeMode, ext, comp);
				break;
			default:
				throw CExc(eFS, eFSgeneral, "Unexpected");
		}
	} catch (...) {
		SAFE_DELETE_ARRAY(buff);
		throw;
	}
	SAFE_DELETE_ARRAY(buff);

	return ok;
}


int CFS::CopyOneFile(const std::string &destPath, const std::string &sourcePath, char *&buff, size_t &buffLen, int comp) {
	if (ComparePath(destPath.c_str(), sourcePath.c_str()) == 0)
		return 0; // don't need to copy on self. This can be caused by version redirection.
	size_t fsize = this->GetFileSize(sourcePath.c_str());
	if (!fsize)
		return 0;
	if (comp == -1) {
		comp = IsCompressed(sourcePath.c_str());
	}
	if (fsize > buffLen) {
		SAFE_DELETE_ARRAY(buff);
		buff = new char[fsize];
		buffLen = fsize; 
	}
	if (!ReadFile(sourcePath.c_str(),buff,fsize))
		return 0;
	if (!WriteFile(destPath.c_str(), buff, fsize, comp))
		return 0;

	return 1;
}


int CFS::CopyTree2(std::string &destPath, std::string &sourcePath, char *&buff, size_t &buffLen, ECopyTreeChangeMode changeMode, const char *ext, int comp)
{

	CFSSearchData *sdata = ReadDirectory(sourcePath.c_str());
	if (!sdata)
		return 0;

	int ok = 1;
	destPath.push_back('\\');
	sourcePath.push_back('\\');
	size_t dLen = destPath.size();
	size_t sLen = sourcePath.size();
	string name;
	int attr;

	try {
		for (int f=0; f<sdata->count(); f++) {
			sdata->GetField(f, name, attr);

			sourcePath.resize(sLen);
			sourcePath.append(name);

			if (!CopyChangeName(name, changeMode, ext)) {
				ok = 2;
				continue;
			}

			destPath.resize(dLen);
			destPath.append(name);

			if (attr == 3)
				CreateArchive(destPath.c_str());

			if (attr == 2 || attr == 3) {
				if (1 != CopyTree2(destPath, sourcePath, buff, buffLen, changeMode, ext, comp))
					ok = 2;
			} else {
				if (1 != CopyOneFile(destPath, sourcePath, buff, buffLen, comp))
					ok = 2;
			}
		}
	} catch(...) {
		sdata->Close();
		throw;
	}

	sdata->Close();

	return ok;
}


bool CFS::CopyChangeName(std::string &name, ECopyTreeChangeMode changeMode, const char *ext) {
	int pos;
	size_t pos2;
	char buffer[20];
	switch (changeMode) {
		case ectcmChangeVersion : // change version
			if (CFSDirCache::SplitVersion(name, 0,0, &pos)) {
				GenerateVersionNumber(buffer);
				name.replace(pos+1, 19, buffer);
			}
			break;
		case ectcmAddExtension : // add extension
			name.push_back('.');
			name.append(ext);
			break;
		case ectcmRemoveExtension : // remove extension
			pos2 = name.rfind('.');
			if (pos2 == string::npos || pos2 == 0)
				return false;
			if (_stricmp(ext, name.substr(pos2+1).c_str()))
				return false;
			name.resize(pos2);
			break;
		case ectcmChangeExtension : // change extension
			pos2 = name.rfind('.');
			if (pos2 == string::npos || pos2 == 0)
				return false;
			name.resize(pos2+1);
			name.append(ext);
			break;
	}
	return true;
}


// presune a prejmenuje source (file,dir,dir\*) do dest.
int CFS::Move(const char *source, const char *dest) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	if (!dest || !*dest)
		return 0;

	CGetAccessReturn garSource;
	int sRet;
	if (!(sRet = dirCache->GetAccess(source, garSource, (EFSGetAccessFlags)(eGAFfile | eGAFdir| eGAFallowStar))))
		return 0;

	// move dir content
	if (sRet == 4) {
		garSource.CloseArchive();
		return MoveDirContent(source, dest);
	}

	string Npath, NpathOC;
	CFSCfg::NormalizePath(cfg->ParseString(string(dest)), Npath, NpathOC);
	string dir;
	string name;
	if (!SplitPath(NpathOC, &dir, &name))
		return 0;

	CGetAccessReturn garDest;
	// create and check dest dir
	if (!dirCache->GetAccess(dir, garDest, (EFSGetAccessFlags)(eGAFcreate | eGAFdirContent | eGAFnonRO)))
		return 0;

	if (garSource.aDir && garDest.aDir && garSource.archive == garDest.archive) {
		// its inside the same archive
		garSource.archive = 0; // to not close it twice
		return garSource.aDir->Rename((**garSource.aItem).GetName(), name.c_str(), garDest.aDir);
	}

	garDest.CloseArchive();
	garSource.CloseArchive();

	if (garSource.aDir || garDest.aDir) {
		// moving across archives. Its needed to copy through temp dir

		string temp = "move_";
		char buff[20];
		GenerateVersionNumber(buff);
		temp.append(buff);

		if (1 != CopyTree(source, "$TEMP$", ectcmAddExtension, "copy", -1, temp.c_str()))
			return 0;
		if (!Delete(source))
			return 0;
		temp = "$TEMP$\\" + temp;
		int ret = CopyTree(temp.c_str(), dir.c_str(), ectcmRemoveExtension, "copy", -1, name.c_str());
		if (ret == 1)
			Delete(temp.c_str());

		return ret;
	}

	// can use windows Move function
	name = garDest.newPath + "\\" + name;
	return MoveFile(garSource.newPath.c_str(), name.c_str()) ? 1 : 0;
}


int CFS::MoveDirContent(string sourcePath, string destPath) {
	sourcePath.resize(sourcePath.size()-2); // remove the *

	CFSSearchData *sdata = ReadDirectory(sourcePath.c_str());
	if (!sdata)
		return 0;

	int ok = 1;
	if (*destPath.rbegin() != '/' && *destPath.rbegin() != '\\')
		destPath.push_back('\\');
	if (*sourcePath.rbegin() != '/' && *sourcePath.rbegin() != '\\')
		sourcePath.push_back('\\');
	size_t dLen = destPath.size();
	size_t sLen = sourcePath.size();
	string name;
	int attr;

	try {
		for (int f=0; f<sdata->count(); f++) {
			sdata->GetField(f, name, attr);

			destPath.resize(dLen);
			sourcePath.resize(sLen);
			destPath.append(name);
			sourcePath.append(name);

			if (1 != Move(sourcePath.c_str(), destPath.c_str()))
				ok = 2;
		}
	} catch(...) {
		sdata->Close();
		throw;
	}

	sdata->Close();

	return ok;
}


bool CFS::SplitPath(const std::string &path, std::string *dir, std::string *name) {
	size_t pos2 = path.size();
	if (!pos2)
		return false;
	if (*path.rbegin() == '/' || *path.rbegin() == '\\')
		pos2--;
	if (!pos2)
		return false;

	size_t pos = path.find_last_of("/\\", pos2-1);
	if (dir)
		*dir = pos == string::npos ? "" : path.substr(0, pos);
	if (name)
		*name = pos == string::npos ? path.substr(0, pos2) : path.substr(pos+1, pos2-pos-1);
	return true;
}



// changes directory (and its content) to archive. Or changes archive to normal directory structure.
int CFS::PackOrUnpackArchive(const char *path) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	int ret;
	string newPath;
	{
		CGetAccessReturn gaReturn;
		if (!(ret = dirCache->GetAccess(path, gaReturn, eGAFdir)))
			return 0;
		if (gaReturn.aDir)
			return 0; // inside archive
		newPath = gaReturn.newPath;
	}

	string temp = "$TEMP$\\unpack_";
	char buff[20];
	GenerateVersionNumber(buff);
	temp.append(buff);

	if (!Move(newPath.c_str(), temp.c_str()))
		return 0;
	if (ret == 3) {
		if (!CreateDir(newPath.c_str())) {
			Move(temp.c_str(), newPath.c_str());
			return 0;
		}
	} else {
		if (!CreateArchive(newPath.c_str())) {
			Move(temp.c_str(), newPath.c_str());
			return 0;
		}
	}

	ret = CopyTree((temp + "\\*").c_str(), newPath.c_str());
	if (ret == 1)
		Delete(temp.c_str());

	return ret;
}



int CFS::FileExist(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	CGetAccessReturn gaReturn;

	return dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFfile | eGAFdirContent));
}


int CFS::Defragment(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	int ok=1;

	CGetAccessReturn gaReturn;
	ok=dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFdirContent | eGAFnonRO));
	if (!ok || !gaReturn.archive)
		return 0;
	return gaReturn.archive->Defragment();
	
}


int CFS::GetFileSize(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	int ln=0;
	CGetAccessReturn gaReturn;

	if (!dirCache->GetAccess(name, gaReturn, eGAFfile))
		return 0;

	if(gaReturn.aDir)
	{
		ln=gaReturn.aDir->GetFileSize(gaReturn.aItem);
	}else{
		FILE *f;
		f=FOPEN(gaReturn.newPath.c_str(),"rb");
		if(!f) 
			return 0;
		ln=_filelength(_fileno(f));

		int compr=0;

		char h1[sizeof(sFS_COMPFILE)];
		char h2[sizeof(sFS_COMPFILE_VER)];
		int size;
		int compsize;
		DWORD crc;
		CCRC32 CRC;

		try{
			READ(h1,sizeof(sFS_COMPFILE),f);
			READ(h2,sizeof(sFS_COMPFILE_VER),f);

			READ(&size,4,f);
			READ(&compsize,4,f);

			READ(&crc,4,f);

			if(memcmp(h1,sFS_COMPFILE,sizeof(sFS_COMPFILE))==0&&memcmp(h2,sFS_COMPFILE_VER,sizeof(sFS_COMPFILE_VER))==0)
			{

				CRC.Cmp(h1,sizeof(sFS_COMPFILE));
				CRC.Cmp(h2,sizeof(sFS_COMPFILE_VER));
				CRC.Cmp(&size,4);
				CRC.Cmp(&compsize,4);

				if(crc==CRC) compr=1;

			}
		}
		catch (CExc c)
		{
			if(c.errnum!=eFS) {
				fclose(f);
				throw;
			}
		}

		if(compr) ln=size;

		fclose(f);
	}

	return ln;
}


int CFS::GetTime(const char *name, FILETIME& time)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	CGetAccessReturn gaReturn;

	if (!dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFfile | eGAFdir)))
		return 0;

	if(gaReturn.aDir)
	{
		return gaReturn.aDir->GetTime(gaReturn.aItem,time);
	}else{

		HANDLE hFile;

		hFile = CreateFile (
			gaReturn.newPath.c_str(),
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			FILE_FLAG_BACKUP_SEMANTICS,
			NULL
		);

		if(hFile!=INVALID_HANDLE_VALUE)
		{
			GetFileTime(hFile, NULL, NULL, &time);
			CloseHandle(hFile); 
			return 1;
		}else
			return 0;

	}
}

int CFS::SetTime(const char *name, const FILETIME& time)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	CGetAccessReturn gaReturn;

	if (!dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFdir | eGAFfile | eGAFnonRO)))
		return 0;

	if(gaReturn.aDir)
	{
		return gaReturn.aDir->SetTime(gaReturn.aItem,time);
	}else{

		HANDLE hFile;

		hFile = CreateFile (
			gaReturn.newPath.c_str(),
			GENERIC_WRITE ,
			FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			FILE_FLAG_BACKUP_SEMANTICS,
			NULL
		);

		int ok=0;
		if(hFile!=INVALID_HANDLE_VALUE)
		{
			if(SetFileTime(hFile, NULL, &time, &time)) ok=1;
			CloseHandle(hFile); 
		}
		return ok;
	}

}


int CFS::IsCompressed(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	int ok=1;
	int compr=0;

	CGetAccessReturn gaReturn;

	ok=dirCache->GetAccess(name, gaReturn, eGAFfile);
	if(ok)
	{
		if(gaReturn.aDir)
		{
			compr=gaReturn.aDir->IsCompressed(gaReturn.aItem);
			ok=1;
		}else{

			compr=0;

			FILE *f;

			f=FOPEN(gaReturn.newPath.c_str(),"rb");
			if(f)
			{

				char h1[sizeof(sFS_COMPFILE)];
				char h2[sizeof(sFS_COMPFILE_VER)];
				int size;
				int compsize;
				DWORD crc;
				CCRC32 CRC;

				try{
					READ(h1,sizeof(sFS_COMPFILE),f);
					READ(h2,sizeof(sFS_COMPFILE_VER),f);
		
					READ(&size,4,f);
					READ(&compsize,4,f);

					READ(&crc,4,f);

					if(memcmp(h1,sFS_COMPFILE,sizeof(sFS_COMPFILE))==0&&memcmp(h2,sFS_COMPFILE_VER,sizeof(sFS_COMPFILE_VER))==0)
					{

						CRC.Cmp(h1,sizeof(sFS_COMPFILE));
						CRC.Cmp(h2,sizeof(sFS_COMPFILE_VER));
						CRC.Cmp(&size,4);
						CRC.Cmp(&compsize,4);

						if(crc==CRC) compr=1;

					}
				}
				catch (CExc c)
				{
					if(c.errnum!=eFS) {
						fclose(f);
						throw;
					}
				}

				fclose(f);
				ok=1;
			}else
				ok=0;
		}

	}

	if(!ok) compr=-1;

	return compr;
}


int CFS::CreateDir(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	CGetAccessReturn gaReturn;
	return dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFcreate | eGAFdirContent | eGAFnonRO));
}


int CFS::CreateArchive(const char *name)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	CGetAccessReturn gaReturn;

	if (dirCache->GetAccess(name, gaReturn, (EFSGetAccessFlags)(eGAFcreate | eGAFdir | eGAFnoArchive))) {
		CFSArchive ar;
		return ar.Create(gaReturn.newPath.c_str()) ? 1 : 0;
	}
	
	return 0;
}


int CFS::GetFullPath(const char *relpath, char **fullpath, EFullPathType type)
{
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	if (!fullpath)
		return 0;
	*fullpath = 0;

	string fp;
	if (!dirCache->GetFullPath(relpath, fp, type))
		return 0;

	*fullpath = new char[fp.size()+1];
	strcpy_s(*fullpath, fp.size()+1, fp.c_str());

	return 1;
}



CFSSearchData *CFS::ReadDirectory(const char *path) { // returns all files and directories in a directory specified by path. Returns NULL in case of an error.
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	CGetAccessReturn gaReturn;

	if (!dirCache->GetAccess(path, gaReturn, eGAFdirContent))
		return 0;

	CFSSearchData2 *sd = new CFSSearchData2();
	if (gaReturn.aDir) {
		for (CFSDirItem *item = gaReturn.aDir->GetFirsItem(); item; item = item->GetNextItem()) {
			if (item->GetName()[0] != '.') {
				sd->AddField(item->GetName(), item->GetDir() ? 2 : 1);
			}
		}
	} else {
		WIN32_FIND_DATA findData;
		string s = gaReturn.newPath+"\\*";
		
		HANDLE h = FindFirstFile(s.c_str(), &findData);
		if (h != INVALID_HANDLE_VALUE) {

			size_t sLen = s.size()-1;

			do {
				if (findData.cFileName[0] != '.') {
					int type=0;
					if (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
						type = 2;
					} else  {
						s.resize(sLen);
						s.append(findData.cFileName);
						CGetAccessReturn gar;
						// check for archive:
						type = dirCache->GetAccess(s, gar, (EFSGetAccessFlags)(eGAFfile | eGAFdir));
					}
					if (type)
						sd->AddField(findData.cFileName, type);
				}
			} while (FindNextFile(h, &findData));
			FindClose(h);
		}
	}

	return new CFSSearchData(sd);
}



// returns the top most root dirs.
CFSSearchData *CFS::GetRoots() {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	CFSSearchData2 *sd = new CFSSearchData2();
	dirCache->GetRoots(sd);
	return new CFSSearchData(sd);
}


int CFS::ComparePath(const char *path1, const char *path2)
{	
	MyCriticalSection myCriticalSection(&fsCriticalSection);

	string fp1, fp2;

	if (!path1 || !dirCache->GetFullPath(path1, fp1))
		return -1;
	if (!path2 || !dirCache->GetFullPath(path2, fp2))
		return 1;

	return fp1.compare(fp2);
}



/////////////////////////////////////////////////////////////////////
// vygeneruje nove cislo verze pomoci MersenneTwisteru. Vrati ho jako cislo. 
// Pokud predas funkci pointer na string, kam se vejde 19 znaku (+1 na koncovou nulu), tak i jako string. 
__int64 CFS::GenerateVersionNumber(char *DestStr ) {
	MyCriticalSection myCriticalSection(&fsCriticalSection);
	__int64 a,b,c,d;
	a = mtr->randInt(0xFFFF);
	b = mtr->randInt(0xFFFF);
	c = mtr->randInt(0xFFFF);
	d = mtr->randInt(0xFFFF);
	if (DestStr) {
		sprintf(DestStr,"%04X_%04X_%04X_%04X",(UI)a,(UI)b,(UI)c,(UI)d);
	}
	return (a<<48)|(b<<32)|(c<<16)|d;
}


/////////////////////////////////////////////////////////////////////////////////////////////////


CFSSearchData::~CFSSearchData() {
	SAFE_DELETE(_data);
}

void CFSSearchData::Close() {
	delete this;
}

int CFSSearchData::count() {
	return _data->count();
}
bool CFSSearchData::GetField(int index, std::string &name, int &attr) {
	return _data->GetField(index, name, attr);
}



/////////////////////////////////////////////////////////////////////////////////////////////////


