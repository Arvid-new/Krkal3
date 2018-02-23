#include "stdafx.h"
#include "fs.h"

#include <string.h>
#include <io.h>
#include <string>

#include <math.h>
#include <zlib.h>

#include "NameSearch.h"
#include "FS_Macros.h"
#include "archive.h"

using namespace std;
using namespace stdext;



///////////////////////////////////////////////////////////////////////////////////////////
/*
	
	Archive structure
	-----------------

	Head of archive
	---------------
	offset	size	desc.
	0		9		"KRKAL_FS"+0
	9		5		"v1.1"+0
	14		4		RootDirOffset=EmptySpaceTableOffset+EmptySpaceHeadLen+MaxEmptySpaceItems*EmptySpaceItemLen
	18		4		EmptySpaceTableOffset=30
	22		4		MaxEmptySpaceItems
	26		8		Random number to indicate changes in archive
	34		4		CRC

	38				EmptySpaceTable
*/
const char sFS_KRKAL[]="KRKAL_FS";
const char sFS_VER[]="v1.1";
const int CUR_ARCH_VERSION = 1;
const char sFS_VER0[]="v1.0";
const int defaultEmptySpaceTableOffset=38;

/*
	EmptySpaceTable
	---------------
	offset	size	desc.
	0		1		# of items.(=n)
	1		4		CRC
	
	5		4		offset to 1.EmptyBlock
	9		4		size of 1.EmptyBlock
	
	13		4		offset to 2.EmptyBlock
	17		4		size of 2.EmptyBlock
	.....
*/
const int EmptySpaceHeadLen=5;
const int EmptySpaceItemLen=8;

/*
	File/Dir Head
	-------------
	offset	size	desc.	
	0		1		file/dir (dir=1)

	File
	----
	offset	size	desc.
	0		4		size of (compressed) file = SIZE of DATA
	4		1		compressed (0=no compr.,1=zlib)
	5		4		CRC of data
	9		??		DATA
*/

/*
	Compressed File DATA
	--------------------
	offset	size	desc.
	0		4		size of uncompressed file
	4		?		DATA
*/
const int COMPRESSED_FILE_HEAD_SIZE = 4;

/*
	Dir
	---
	offset	size	desc.
	0		4		# of items
	4		4		CRC


	DirItem
	-------
	offset	size	desc.
	0		1		namelen
	1		namelen	name
	+0		1		file/dir (dir=1)
	+1		4		offset
	+5		8		date&time (FILETIME)
*/



//////////////////////////////////////////////////////////////////////////////////////////


CFSEmptySpace::CFSEmptySpace(int _maxItems, CFSArchive *ar)
{

	assert(ar&&ar->file);

	maxItems =_maxItems;
	numItems = 0;

	items = new CFSEmptySpaceItem[maxItems];
	
	archive=ar;

}
CFSEmptySpace::~CFSEmptySpace()
{
	SAFE_DELETE_ARRAY(items);
}

int CFSEmptySpace::AddEmptySpace(int size, int offset)
{
	int i,pr,nx,o;

	assert(numItems<maxItems);

	if(offset+size==archive->archivesize)
	{
		//prazdny misto je na konci souboru -> zmensim soubor
		archive->archivesize-=size;
		

		i=numItems-1; //posledni
		if(items[i].offset+items[i].size==archive->archivesize)
		{ //posledni prazdne misto se dostalo na konec souboru -> zmensim soubor
			archive->archivesize-=items[i].size;
			Delete(i);
			WriteEmptySpace(archive->EmptySpaceTableOffset);
		}

		_chsize(_fileno(archive->file),archive->archivesize);

		return 1;
	}

	pr=nx=-1;


	//najde blok pred a za
	for(i=0;i<numItems;i++)
	{
		o=items[i].offset+items[i].size;
		if(o>=offset)
		{
			if(o==offset) pr=i;
			if(i+1<numItems&&items[i+1].offset==offset+size) nx=i+1;
			if(items[i].offset==offset+size) nx=i;
			break;
		}
	}

	//slouci volny bloky vedle sebe
	if(pr!=-1&&nx!=-1)
	{
		int o,s;
		o=items[pr].offset;
		s=items[pr].size+size+items[nx].size;
		
		Delete(nx);
		Change(pr,o,s);

	}else
	if(pr!=-1)
		Change(pr,items[pr].offset,items[pr].size+size);
	else
	if(nx!=-1)
		Change(nx,offset,size+items[nx].size);
	else
	{
		//prida volny blok
		Add(offset,size);
		//if(numItems==maxItems) archive->Defragment();
	}

	WriteEmptySpace(archive->EmptySpaceTableOffset);

	return 1;
}

int CFSEmptySpace::GetEmptySpace(int size,int &offset)
{
	int i,mx,mxsz=0;

	for(i=0;i<numItems;i++)
	{
		if(items[i].size==size) 
		{
			offset=items[i].offset;
			Delete(i);
			WriteEmptySpace(archive->EmptySpaceTableOffset);
			return 1;
		}else{
			if(items[i].size>mxsz) {mxsz=items[i].size;mx=i;}
		}
	}

	if(numItems)
	{
		i=mx; //vezmu nejvetsi;
		if(items[i].size>size){
			offset=items[i].offset;
			Change(i,offset+size,items[i].size-size);
			WriteEmptySpace(archive->EmptySpaceTableOffset);
			return 1;
		}
	}

	//neexistuje zadna dostatecne velka dira, pridelim misto z konce souboru
	offset = archive->archivesize;
	
	archive->archivesize+=size;
	_chsize(_fileno(archive->file),archive->archivesize);

	return 1;
}

void CFSEmptySpace::Add(int offset, int size)
{
	int i,j;

	assert(numItems<maxItems);

	i=0;
	while(items[i].offset<offset&&i<numItems) i++;

	for(j=numItems+1;j>i;j--)
		items[j]=items[j-1];

	items[j].offset=offset;
	items[j].size=size;

	numItems++;
}

void CFSEmptySpace::Delete(int i)
{
	int j;

	numItems--;
	for(j=i;j<numItems;j++)
		items[j]=items[j+1];

}
void CFSEmptySpace::Change(int i, int newoffset, int newsize)
{
	items[i].offset = newoffset;
	items[i].size = newsize;

/*	int j;
	if(newsize==items[i].size)
		items[i].offset=newoffset;
	else if(newsize<items[i].size)
	{
		j=i;
		while(j>0&&items[j].size>newsize)
		{
			items[j]=items[j-1];
			j--;
		}
		items[j].offset=newoffset;
		items[j].size=newsize;
	}else //newsize>items[i].size
	{
		j=i;
		while(j<numItems-1&&items[j].size<newsize)
		{
			items[j]=items[j+1];
			j++;
		}
		items[j].offset=newoffset;
		items[j].size=newsize;
	}*/
}

int CFSEmptySpace::WriteEmptySpace(int offset)
{
	FILE *f;
	DWORD CRC;
	assert(archive->file&&archive->readonly==0);

	f=archive->file;
	
	CRC=CmpCRC();

	SEEK(f,offset);
	WRITE(&numItems,4,f);
	WRITE(&CRC,4,f);
	for(int i=0;i<numItems;i++)
	{
		WRITE(&items[i].offset,4,f);
		WRITE(&items[i].size,4,f);
	}
	FLUSH(f);
	return 1;
}

int CFSEmptySpace::ReadEmptySpace(int offset)
{
	FILE *f;
	DWORD CRC,CRC2;
	assert(archive->file);

	f=archive->file;
	
	SEEK(f,offset);
	READ(&numItems,4,f);

	if(numItems>=maxItems) return 0;
	
	READ(&CRC,4,f);
	for(int i=0;i<numItems;i++)
	{
		READ(&items[i].offset,4,f);
		READ(&items[i].size,4,f);
	}

	CRC2=CmpCRC();

	if(CRC!=CRC2) return 0;

	return 1;
}

DWORD CFSEmptySpace::CmpCRC()
{
	CCRC32 crc;

	crc.Cmp(&numItems,4);
	for(int i=0;i<numItems;i++)
	{
		crc.Cmp(&items[i].offset,4);
		crc.Cmp(&items[i].size,4);
	}
	return crc;
}


int CFSEmptySpace::CmpOffsetAfterDefrag(int oldoffset)
{
	int esz=0;
	for(int i=0;i<numItems;i++)
	{
		if(items[i].offset>=oldoffset) break;
		esz+=items[i].size;
	}
	return oldoffset-esz;
}

int CFSEmptySpace::NeedDefrag()
{
	if(numItems==0) return 0;

	if(numItems<maxItems - 10) return 0;

	return 1;

}

////////////////////////////////////////////////////////////////////////////////////////////////////
CFSArchive::CFSArchive()
{
	file=NULL;
	emptyspace=NULL;
	rootDir=NULL;
	archiveChanged = false;
}
CFSArchive::~CFSArchive()
{
	Close();
	SAFE_DELETE(emptyspace);
	SAFE_DELETE(rootDir);
}

void CFSArchive::Close(){
	if(file){
		if (archiveChanged && !readonly) {
			randomVersion = FS->GenerateVersionNumber(0);
			WriteArchiveHead();
			archiveChanged = false;
		}
		fclose(file);
		file=NULL;
	}
}

int CFSArchive::Create(const char *path)
{
	try{
		Close();

		if (!FS->archiveNameVerifier->IsValidName(string(path)))
			return 0;
		if (_access(path, 0) != -1)
			return 0; // file must not exist

		file=FOPEN(path,"w+b");
		if(!file) return 0;
		readonly=0;
		archiveChanged = true;
		openPath = path;

		EmptySpaceTableOffset=defaultEmptySpaceTableOffset;
		MaxEmptySpaceItems=100;
		RootDirOffset=EmptySpaceTableOffset+EmptySpaceHeadLen+MaxEmptySpaceItems*EmptySpaceItemLen;

		int l=(int)strlen(path)+1;

		WriteArchiveHead();
		archivesize=RootDirOffset;
		_chsize(_fileno(file),archivesize);

		emptyspace=new CFSEmptySpace(MaxEmptySpaceItems,this);

		emptyspace->WriteEmptySpace(EmptySpaceTableOffset);


		rootDir=new CFSDir(this);
		if(!rootDir->Create(0,0))
		{
			SAFE_DELETE(rootDir);
			Close();
			return 0;
		}

		assert(rootDir->GetOffset()==RootDirOffset);
	}
	catch(CExc e)
	{
		if(e.errnum==eFS) return 0;
		throw;
	}

	return 1;
}

int CFSArchive::Open(const char *path, int _readonly)
{

	try
	{

	Close();

	if(_readonly)
		file=FOPEN(path,"rb");
	else
		file=FOPEN(path,"r+b");
	if(!file) return 0;
	openPath = path;
	
	readonly=_readonly;

	archivesize=_filelength(_fileno(file));

	__int64 oldRandomVersion = randomVersion;
	if(!ReadArchiveHead()) 
	{
		Close();
		return 0;
	}

	if (oldRandomVersion == randomVersion && rootDir) {
		// archive is up to date - don't reread it
		return 2;
	} else {
		SAFE_DELETE(emptyspace);
		SAFE_DELETE(rootDir);
	}

	emptyspace=new CFSEmptySpace(MaxEmptySpaceItems,this);

	if(!emptyspace->ReadEmptySpace(EmptySpaceTableOffset))
	{
		Close();
		return 0;
	}

	rootDir=new CFSDir(this);
	if(!rootDir->Read(RootDirOffset, 0,0))
	{
		SAFE_DELETE(rootDir);
		Close();
		return 0;
	}


	}
	catch(CExc e)
	{
		if(e.errnum==eFS) return 0;
		throw;
	}
	return 1;
}


int CFSArchive::ReOpen(int _readonly) {
	if (file && (!readonly || readonly == _readonly))
		return 2;
	if (openPath.empty())
		return 0;
	return Open(openPath.c_str(), _readonly);
}


void CFSArchive::WriteArchiveHead()
{
	assert(file&&readonly==0);
	archiveChanged = true;

	SEEK(file,0);

	WRITE(sFS_KRKAL,sizeof(sFS_KRKAL),file);
	WRITE(sFS_VER,sizeof(sFS_VER),file);

	WRITE(&RootDirOffset,sizeof(int),file); 

	WRITE(&EmptySpaceTableOffset,sizeof(int),file); 
	WRITE(&MaxEmptySpaceItems,sizeof(int),file);
	WRITE(&randomVersion,sizeof(__int64),file);

	DWORD CRC=CmpArchiveHeadCRC(CUR_ARCH_VERSION);
	
	WRITE(&CRC,sizeof(int),file); //CRC
	FLUSH(file);
}

int CFSArchive::ReadArchiveHead()
{
	assert(file);

	SEEK(file,0);

	char buf[sizeof(sFS_KRKAL)];
	READ(buf,sizeof(sFS_KRKAL),file);
	if(strcmp(buf,sFS_KRKAL)!=0) return 0;
	READ(buf,sizeof(sFS_VER),file);
	int ver;
	if(strcmp(buf,sFS_VER)==0) {
		ver=1;
	} else if (strcmp(buf,sFS_VER0)==0) {
		ver=0;
	} else {
		return 0;
	}

	READ(&RootDirOffset,sizeof(int),file); 

	READ(&EmptySpaceTableOffset,sizeof(int),file); 
	READ(&MaxEmptySpaceItems,sizeof(int),file);
	if (ver >= 1) {
		READ(&randomVersion,sizeof(__int64),file);
	} else {
		randomVersion = 0;
	}

	DWORD CRC;
	
	READ(&CRC,sizeof(int),file);

	if(CRC!=CmpArchiveHeadCRC(ver)) return 0;

	return 1;
}

DWORD CFSArchive::CmpArchiveHeadCRC(int ver)
{
	CCRC32 crc;
	crc.Cmp(&RootDirOffset,4);
	crc.Cmp(&EmptySpaceTableOffset,4);
	crc.Cmp(&MaxEmptySpaceItems,4);
	if (ver >= 1) {
		crc.Cmp(&randomVersion,8);
	}
	return crc;
}


int CFSArchive::Defragment()
{
	if(!file||readonly) return 0;
	archiveChanged = true;

	if(emptyspace->numItems==0) return 1; //neni treba defragmentovat

	CFSDir::dirOffsetsT offsets;
	if (rootDir)
		rootDir->FillDirOffsets(offsets);

	int curofs=EmptySpaceTableOffset+EmptySpaceHeadLen+MaxEmptySpaceItems*EmptySpaceItemLen;
	int wrofs=curofs;
	int esindex=0;
	int esbefore=0;
	int sz,s;
	char d;
	DWORD CRC;
	char *buf;
	const int bufsz=64*1024;
	CFSDir *dir;
	int w,r;
	unsigned char compr;

	buf=new char[bufsz];
	if(!buf) return 0;

	RootDirOffset=emptyspace->CmpOffsetAfterDefrag(RootDirOffset);
	WriteArchiveHead();

	SEEK(file,curofs);

	while(curofs<archivesize)
	{
		if(esindex<emptyspace->numItems&&emptyspace->items[esindex].offset==curofs)
		{
			esbefore+=emptyspace->items[esindex].size;
			curofs+=emptyspace->items[esindex].size;
			esindex++;
		}else
		{
			SEEK(file,curofs);
			READ(&d,1,file);
			if(d)
			{
				dir=0;
				CFSDir::dirOffsetsT::const_iterator i = offsets.find(curofs);
				if (i != offsets.end())
					dir = i->second;
				if(!dir)
				{
					delete[] buf;
					return 0;
				}

				sz=dir->GetSize();

				dir->DefragDir(wrofs);
				
				curofs+=sz;
				wrofs+=sz;
			}else
			{
				READ(&sz,4,file);
				READ(&compr,1,file);
				READ(&CRC,4,file);
				SEEK(file,wrofs);
				WRITE(&d,1,file);
				WRITE(&sz,4,file);
				WRITE(&compr,1,file);
				WRITE(&CRC,4,file);
				s=sz;
				w=wrofs+1+4+1+4;
				r=curofs+1+4+1+4;
				while(s>bufsz)
				{
					SEEK(file,r);
					READ(buf,bufsz,file);
					SEEK(file,w);
					WRITE(buf,bufsz,file);
					r+=bufsz;w+=bufsz;
					s-=bufsz;
				}
				if(s)
				{
					SEEK(file,r);
					READ(buf,s,file);
					SEEK(file,w);
					WRITE(buf,s,file);
				}
				sz=CMP_FILE_SPACE(sz);//zvetsi na nasobek 32
				curofs+=sz;
				wrofs+=sz;
			}
		}
	}

	emptyspace->DeleteAll();
	emptyspace->WriteEmptySpace(EmptySpaceTableOffset);

	archivesize=wrofs;
	_chsize(_fileno(file),archivesize);

	FLUSH(file);

	delete[] buf;

	return 1;
}





//////////////////////////////////////////////////////////////////////////////////////////////////////

CFSDir::CFSDir(CFSArchive *ar)
{
	assert(ar&&ar->file);

	archive=ar;

	numitems=0;
	items=NULL;
	offset=0;
	wrsize=0;
	Parent = 0;
	parentItem = 0;

}

CFSDir::~CFSDir()
{
	SAFE_DELETE(items);
}

int CFSDir::Create(CFSDir *parent, CFSDirItem *pItem)
{
	assert(archive->readonly==0);

	SAFE_DELETE(items);

	Parent = parent;
	parentItem = pItem;

	numitems=1;
	items = new CFSDirItem;
	items->Create("..",1,parent?parent->offset:0);
	items->Dir = parent;
	
	offset=0;
	wrsize=0;

	Write();

	return offset;
	
}

int CFSDir::Write()
{
	assert(archive->readonly==0);
	archive->archiveChanged = true;

	int sz=GetSize();
	int o,res;
	int testdefrag=0;

	if(sz!=wrsize)
	{
		if(wrsize>0) //vratim stary misto
		{
			archive->emptyspace->AddEmptySpace(wrsize,offset);
			testdefrag = 1;
		}

		//alokuju novy misto
		if(!archive->emptyspace->GetEmptySpace(sz,o))
			return 0;

		if(o!=offset&&offset!=0)
		{
			//offset se zmenil musim zmenit rodice a deti
			CFSDir *dir;
				
			CFSDirItem *it=items; 
			CFSDirItem **it2;
			int oo;
			while(it)
			{
				if(it->dir)
				{
					oo=it->GetOffset();

					if(oo)
					{
						dir=it->Dir;
						if(dir)
						{
							if(dir->FindItem(offset,&it2))
							{
								(*it2)->offset=o;
								dir->Write();
							}
						}
					}else{
						//rootdir
						if(archive->RootDirOffset!=o)
						{
							archive->RootDirOffset=o;
							archive->WriteArchiveHead();
						}
					}
				}
				it=it->next;
			}

		}
		offset=o;
	}

	wrsize=sz;

	res = WriteDirData();
	if(!res) return 0;

	if(testdefrag)
	{
		if( archive->emptyspace->NeedDefrag() )
			archive->Defragment();
	}

	return res;
}

int CFSDir::WriteDirData()
{

	//vlastni zapis
	FILE *f=archive->file;
	char d=1;
	CCRC32 crc; 
	DWORD CRC;
	CFSDirItem *it=items;

	SEEK(f,offset);
	WRITE(&d,1,f);
	WRITE(&numitems,4,f);
	
	crc.Cmp(&d,1);
	crc.Cmp(&numitems,4);

	while(it){
		it->CmpCRC(crc);
		it=it->next;
	} 
	
	CRC=crc;
	WRITE(&CRC,4,f);


	it=items;
	while(it){
		it->Write(f);
		it=it->next;
	}

	FLUSH(f);

	return 1;
}

int CFSDir::Read(int _offset, CFSDir *parent, CFSDirItem *pItem)
{
	SAFE_DELETE(items);
	offset=0;
	wrsize=0;
	numitems=0;
	Parent = parent;
	parentItem = pItem;

	FILE *f=archive->file;

	SEEK(f,_offset);

	char d;
	DWORD CRCr;
	CCRC32 crc;
	CFSDirItem **it=&items;
	CFSDirItem *dit;


	READ(&d,1,f);
	if(d!=1) return 0;

	READ(&numitems,4,f);
	READ(&CRCr,4,f);
	for(int i=0;i<numitems;i++)
	{
		*it=new CFSDirItem;
		(*it)->Read(f);
		it=&(*it)->next;
	}
	
	crc.Cmp(&d,1);
	crc.Cmp(&numitems,4);
	dit=items;
	while(dit){
		dit->CmpCRC(crc);
		dit=dit->next;
	} 
	
	if(CRCr!=crc)
	{
		SAFE_DELETE(items);
		numitems=0;
		wrsize=0;
		return 0;
	}

	
	wrsize=GetSize();

	offset=_offset;

	// read subdirs
	dit=items;
	while(dit){
		if (dit->dir) {
			if (strcmp("..", dit->name) == 0) {
				dit->Dir = Parent;
			} else {
				dit->Dir = new CFSDir(archive);
				if (!dit->Dir->Read(dit->offset, this, dit)) {
					SAFE_DELETE(dit->Dir);
					return 0;
				}
			}
		}
		dit=dit->next;
	} 

	return 1;
}

int CFSDir::FindItem(const char *name,CFSDirItem ***it, int namelen)
{

	if(namelen==-1)
		namelen=(int)strlen(name);

	*it=&items;

	while(**it)
	{
		if((**it)->namelen==namelen&&
			_strnicmp((**it)->name,name,namelen)==0
			) return 1;
		*it=&(**it)->next;
	}

	it = 0;
	return 0;
}

int CFSDir::FindItem(int offset,CFSDirItem ***it)
{

	*it=&items;

	while(**it)
	{
		if((**it)->offset==offset) return 1;
		*it=&(**it)->next;
	}

	it = 0;
	return 0;
}

int CFSDir::GetSize()
{
	int sz;
	CFSDirItem *it=items;

	sz=1+4+4;

	while(it){
        sz+=it->GetSize();
		it=it->next;
	}

	//zaokrouhlim na nasobek 32
	sz=CMP_CEIL(sz);

	return sz;
}





void CFSDir::GetFullPath(std::string &path) {
	if (!Parent) {
		path = archive->GetOpenPath();
	} else {
		Parent->GetFullPath(path);
		path.push_back('\\');
		path.append(parentItem->GetName());
	}
}


void CFSDir::GetFullPath(std::string &path, const char *fileName) {
	GetFullPath(path);
	if (fileName && *fileName) {
		path.push_back('\\');
		path.append(fileName);
	}
}




CFSDir * CFSDir::CreateSubDir(const char *name)
{
	if(archive->readonly) return 0;
	if(!FS->IsValidFilename(name)) return 0;

	CFSDirItem **itm;
	if(FindItem(name,&itm)) return 0; //uz existuje file/dir se stejnym jmenem

	CFSDirItem *it=new CFSDirItem;
	CFSDir *dir = new CFSDir(archive);
	if (!dir->Create(this,it)) {
		SAFE_DELETE(it);
		SAFE_DELETE(dir);
		return 0;
	}
	
	int o=dir->GetOffset();

	it->Create(name,1,o);
	it->Dir = dir;

	assert(items);

	numitems++;

	it->next=items->next;
	items->next=it;
	
	Write();

	string s;
	dir->GetFullPath(s);
	FS->dirCache->InsertChange(CFSCfg::NormalizePath(s), CTaddedDir);

	return dir;
}


int CFSDir::WriteFile(const char *name,const char *buf, int size, unsigned char compr)
{
	int ex=0;
	if(archive->readonly) return 0;
	if(!FS->IsValidFilename(name)) return 0;
	int oldoffset=0;

	CFSDirItem **itm;
	if(FindItem(name,&itm)) {
		ex=1; //uz existuje file/dir se stejnym jmenem
		if((*itm)->dir) return 0; //existuje dir

		oldoffset = (*itm)->offset;

		if(compr==-1)
		{
			compr=IsCompressed(itm);
		}

	}

	if(compr==-1) compr=1;

	int o;

	o=WriteFileData(buf,size,oldoffset,compr);
	if(!o) return 0;

	if(ex)
	{
		SYSTEMTIME st;
		FILETIME ft;

		GetSystemTime(&st);              // gets current time
	    SystemTimeToFileTime(&st, &ft);  // converts to file time format

		(*itm)->SetTime(ft);
		(*itm)->offset=o;
	}else
	{
		CFSDirItem *it=new CFSDirItem;
		it->Create(name,0,o);

		assert(items);

		numitems++;

		it->next=items->next;
		items->next=it;
	}

	Write();

	if( archive->emptyspace->NeedDefrag() )
		archive->Defragment();

	if (!ex) {
		string s;
		GetFullPath(s, name);
		FS->dirCache->InsertChange(CFSCfg::NormalizePath(s), CTaddedFile);
	}

	return 1;
}

int CFSDir::WriteFileData(const char *buf, int size, int oldoffset, unsigned char compr)
{
	//v buf jsou nekoprimovana data, size je velikost nekoprimovanych dat
	archive->archiveChanged = true;

	int sz,osz;
	int datasize;
	int compressedsize;
	int o;
	uLongf compbufsize;

	const char *outbuf;

	char d=0;

	assert(archive->readonly==0);


	if(compr)
	{
		compbufsize=(int)ceil(size*1.01)+12;
		outbuf=new char[compbufsize];

		compress((Bytef*)outbuf,&compbufsize,(const Bytef*)buf,size);

		compressedsize=compbufsize;
		datasize=compressedsize+COMPRESSED_FILE_HEAD_SIZE;

	}else{
		outbuf=buf;
		datasize=size;		
		compressedsize=size;
	}
	

	sz=CMP_FILE_SPACE(datasize);

	FILE *f=archive->file;


	if(oldoffset)
	{

		SEEK(f,oldoffset);

		READ(&d,1,f);
		if(d!=0){
			if(compr) delete[] outbuf;
			return 0;
		}
		READ(&osz,4,f);

		osz=CMP_FILE_SPACE(osz);

		if(osz!=sz)
		{
			archive->emptyspace->AddEmptySpace(osz,oldoffset); //vratim stary misto
			//alokuju novy misto
			if(!archive->emptyspace->GetEmptySpace(sz,o))
			{
				if(compr) delete[] outbuf;
				return 0;
			}

		}else o=oldoffset;


	}else{
		//alokuju novy misto
		if(!archive->emptyspace->GetEmptySpace(sz,o))
		{
			if(compr) delete[] outbuf;
			return 0;
		}
	}

	SEEK(f,o);
	
	CCRC32 crc;
	DWORD CRC;

	crc.Cmp(&d,1);
	crc.Cmp(&datasize,4);
	crc.Cmp(&compr,1);
	if(compr){
		crc.Cmp(&size,4); //velikost nekomprimovanych dat
	}
	crc.Cmp(outbuf,compressedsize);
	CRC=crc;

	WRITE(&d,1,f);
	WRITE(&datasize,4,f);
	WRITE(&compr,1,f);
	WRITE(&CRC,4,f);
	if(compr){
		WRITE(&size,4,f); //velikost nekomprimovanych dat
	}
	WRITE(outbuf,compressedsize,f);

	FLUSH(f);

	if(compr) delete[] outbuf;

	return o;
}

int CFSDir::ReadFile(CFSDirItem **itm, char *buf, int bufsize)
{

	if(!itm || !*itm) return 0; //neexistuje
    
	if((*itm)->dir) return 0; //je to adresar
	
	return ReadFileData((*itm)->offset,buf,bufsize);
}

int CFSDir::ReadFileData(int offset, char *buf, int bufsize)
{
	int ok=1;

	char d=1;
	unsigned char compr;
	int size;
	int uncompsize,datasize;

	char *inbuf;

	FILE *f=archive->file;

	SEEK(f,offset);
	
	CCRC32 crc;
	DWORD CRC;

	READ(&d,1,f);
	if(d!=0) return 0; 
	READ(&size,4,f);
	READ(&compr,1,f);
	READ(&CRC,4,f);
	if(compr){
		READ(&uncompsize,4,f);
		datasize=size-COMPRESSED_FILE_HEAD_SIZE;
		
		inbuf=new char[datasize];

	}else{
		if(size<=bufsize) datasize=size; else datasize=bufsize;		
		inbuf=buf;
	}

	crc.Cmp(&d,1);
	crc.Cmp(&size,4);
	crc.Cmp(&compr,1);
	if(compr)
		crc.Cmp(&uncompsize,4);

	READ(inbuf,datasize,f);

	crc.Cmp(inbuf,datasize);

	if(CRC==crc) 
	{

		if(compr)
		{
			//dekomprese

			uLongf ucs;
			
			if(uncompsize<=bufsize) ucs=uncompsize; else ucs=bufsize;

			if(uncompress((Bytef*)buf,&ucs,(Bytef*)inbuf,datasize)!=Z_OK)
				ok=0;

			if(ucs!=uncompsize) ok=2;
			
		}
	}else ok=0;

	if(compr) delete[]inbuf;
	
	return ok;

}

int CFSDir::GetFileSize(CFSDirItem **itm)
{

	if(!itm || !*itm) return 0; //neexistuje
    
	if((*itm)->dir) return 0; //je to adresar

	FILE *f=archive->file;

	SEEK(f,(*itm)->offset);
	
	char d;
	unsigned char compr;
	int size;
	int CRC;

	READ(&d,1,f);
	if(d!=0) return 0; 
	READ(&size,4,f);
	READ(&compr,1,f);
	if(!compr) return size;

	READ(&CRC,4,f);
	READ(&size,4,f);

	return size;	
}

int CFSDir::IsCompressed(CFSDirItem **itm)
{

	if(!itm || !*itm) return -1; //neexistuje
    
	if((*itm)->dir) return -1; //je to adresar

	FILE *f=archive->file;

	SEEK(f,(*itm)->offset);
	
	char d;
	unsigned char compr;
	int size;

	READ(&d,1,f);
	if(d!=0) return -1; 
	READ(&size,4,f);
	READ(&compr,1,f);

	return compr;	
}

int CFSDir::FileExist(const char *name)
{

	CFSDirItem **itm;
	if(!FindItem(name,&itm)) return 0; //neexistuje
    
	if((*itm)->dir) return 2; //je to adresar

	return 1; //je to soubor
}

int CFSDir::GetTime(CFSDirItem **itm, FILETIME &item_time)
{

	if(!itm || !*itm) return 0; //neexistuje

	(*itm)->GetTime(item_time);
	
	return 1;
}

int CFSDir::SetTime(CFSDirItem **itm, const FILETIME &item_time)
{
	if(archive->readonly) return 0;

	if(!itm || !*itm) return 0; //neexistuje

	(*itm)->SetTime(item_time);

	Write();
	
	return 1;
}

int CFSDir::Delete(CFSDirItem **itm)
{

	if(archive->readonly) return 0;

	CFSDirItem *it;
	int delall=0;

	if(!itm)
	{
		//mazu vse
		itm=&items->next;
		it=*itm;
		delall=1;
	}else{
		if(!itm || !*itm) return 0; 
		it=*itm;
	}

	while(it)
	{

		if(strcmp(it->GetName(),"..")==0) return 0;

		string s;
		GetFullPath(s, it->GetName());
		FS->dirCache->InsertChange(CFSCfg::NormalizePath(s), CTdeleted);


		if(it->dir)
		{
			//mazu adresar
			CFSDir *d = it->Dir;

			d->Delete(0);

			int sz=d->GetSize();
			int o=d->GetOffset();

			//d->Invalidate();

			archive->emptyspace->AddEmptySpace(sz,o); //vratim stary misto

		}else
		{
			//mazu soubor
			FILE *f=archive->file;

			SEEK(f,it->offset);
			
			char d;
			int size;
			READ(&d,1,f);
			if(d!=0) return 0; 
			READ(&size,4,f);

			size=CMP_FILE_SPACE(size);

			archive->emptyspace->AddEmptySpace(size,it->offset); //vratim stary misto
		}	

		*itm=it->next;
		it->next=NULL;
		delete it;
		numitems--;

		if(!delall) break;

		it=*itm;

		if( archive->emptyspace->NeedDefrag() )
			archive->Defragment();

	}

	Write();

	if( archive->emptyspace->NeedDefrag() )
		archive->Defragment();

	return 1;
}

int CFSDir::Rename(const char *oldname, const char *newname, CFSDir *newdir)
{

	if(archive->readonly) return 0;

	if(!FS->IsValidFilename(oldname)) return 0;
	if(!FS->IsValidFilename(newname)) return 0;
	size_t newnameLen = strlen(newname);
	if (newnameLen >= 256) return 0;

	CFSDirItem **itm,*it;

	if(newdir->FindItem(newname,&itm)) return 0; //uz existuje novy

	if(!FindItem(oldname,&itm)) return 0;  //neexistuje stary


	it=*itm; 

	if(it->dir) //presouvam adresar -> zkontroluju jestli ho nepresouvam do sebe
	{
		for (CFSDir *d=newdir; d; d = d->Parent) {
			if (d == it->Dir) return 0;
		}
	}

	string s;
	GetFullPath(s, it->GetName());
	FS->dirCache->InsertChange(CFSCfg::NormalizePath(s), CTdeleted);

	SAFE_DELETE_ARRAY(it->name);
	it->namelen = (UC)newnameLen;
	it->name=new char[it->namelen+1];
	strcpy(it->name,newname);

	newdir->GetFullPath(s, it->GetName());
	FS->dirCache->InsertChange(CFSCfg::NormalizePath(s), it->dir ? CTaddedDir : CTaddedFile);

	if(newdir&&newdir->offset!=offset)
	{
		//vyhodim ze staryho adresare
		(*itm)=it->next; 
		numitems--;
		//dam do novyho
		it->next=newdir->items->next;
		newdir->items->next=it;
		newdir->numitems++;
		
		if(it->dir) //presouval jsem adresar -> musim zmenit rodice
		{
			CFSDir *dir = it->Dir;
			CFSDirItem **it2;
			if(dir)
			{
				dir->Parent=newdir;
				if(dir->FindItem(offset,&it2))
				{
					(*it2)->offset=newdir->offset;
					dir->Write();
				}
			}

		}

		//zapisu novej adr.
		newdir->Write();
	}
	Write(); //zapisu starej adr.

	return 1;
}


int CFSDir::DefragDir(int newoffset)
{

	if(!archive||archive->readonly) return 0;
	archive->archiveChanged = true;

	CFSEmptySpace *es=archive->emptyspace;
	if(!es) return 0;

	offset=newoffset;

	CFSDirItem *it=items;
	while(it)
	{
		it->offset=es->CmpOffsetAfterDefrag(it->offset);
		it=it->next;
	}

	WriteDirData();

	return 1;
}



void CFSDir::FillDirOffsets(dirOffsetsT &offsets) {
	offsets[offset] = this;
	CFSDirItem *it=items;
	while(it)
	{
		if (it->Dir && strcmp(it->name, "..")) {
			it->Dir->FillDirOffsets(offsets);
		}
		it=it->next;
	}
}


//int CFSDir::GetItem(int itnum, const char **name, int &dir)
//{
//	//assert(valid);
//
//	if(itnum<0||itnum>=numitems) return 0;
//	
//	CFSDirItem *it=items;
//	
//	for(int i=0;i<itnum;i++) it=it->next;
//
//	dir=it->dir;
//	*name=it->name;
//
//	return 1;
//}





void CFSDir::DeleteArchive() {
	if (!Parent)
		delete archive;
}


///////////////////////////////////////////////////////////////////////////////////////////////////


CFSDirItem::~CFSDirItem() {
	if (strcmp(name, "..") == 0) {
		Dir = 0;
	} else {
		SAFE_DELETE(Dir); 
	}
	SAFE_DELETE_ARRAY(name); 
	SAFE_DELETE(next); 
}

int CFSDirItem::Write(FILE *f)
{
	WRITE(&namelen,1,f);
	WRITE(name,namelen,f);
	WRITE(&dir,1,f);
	WRITE(&offset,4,f);
	WRITE(&ittime,8,f);
	return 1;
}

int CFSDirItem::Read(FILE *f)
{
	SAFE_DELETE_ARRAY(name);

	READ(&namelen,1,f);
	
	name=new char[namelen+1];
	READ(name,namelen,f);
	name[namelen]=0;

	READ(&dir,1,f);
	READ(&offset,4,f);
	READ(&ittime,8,f);

	return 1+namelen+1+4+8;

}


void CFSDirItem::CmpCRC(CCRC32 &crc)
{
	crc.Cmp(&namelen,1);
	crc.Cmp(name,namelen);
	crc.Cmp(&dir,1);
	crc.Cmp(&offset,4);
	crc.Cmp(&ittime,8);
}

int CFSDirItem::GetSize()
{
	return 1+namelen+1+4+8;
}


int CFSDirItem::Create(const char *_name, char _dir, int _offset)
{
	SAFE_DELETE_ARRAY(name);

	size_t len = strlen(_name);
	if (len>=256) return 0;
	namelen = (UC)len;

	name=new char[namelen+1];
	memcpy(name,_name,namelen+1);

	dir=_dir;
	offset=_offset;

    SYSTEMTIME st;

    GetSystemTime(&st);              // gets current time
    SystemTimeToFileTime(&st, &ittime);  // converts to file time format

	return 1;

}






////////////////////////////////////////////////////////////////////////////////////////////////////////////


void CArchiveNameVerifier::AddExtension(const char *ext, int type) {
	string s = ext;
	ToUpper(s);
	if (type==0) {
		_arcgiveExt.insert(s);
	} else {
		_ignoreExt.insert(s);
	}
}

bool CArchiveNameVerifier::IsValidName(const std::string &path) {
	size_t pos=path.size(), pos2;
	for (;;) {
		if (!pos)
			return false;
		pos2 = pos-1;
		pos = path.find_last_of("./\\", pos2);
		if (pos == string::npos || path[pos] != '.')
			return true;

		string s = path.substr(pos+1, pos2-pos);
		ToUpper(s);
		if (_arcgiveExt.find(s) != _arcgiveExt.end())
			return true;
		if (_ignoreExt.find(s) == _ignoreExt.end())
			return false;
	}
}


