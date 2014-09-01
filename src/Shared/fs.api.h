////////////////////////////////////////////////////////////////////////////////////////////////
//
// FS.api.h
// verejne API dllka
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

#ifndef FS_API_H
#define FS_API_H



#include <stdio.h>
#include <windows.h>
#include <string>
#include "types.h"
#include "HashTable.h"


//forward deklarace
class CFSArchive;

class CFSRegister;
struct CFSRegHT;
class CFSRegKey;
struct CFSRootRegInfo;








/////////////////////////////////////////////////////////////////////////////
//
// crc.h
//
// Vypocet CRC32
//
// A: Petr Altman
//
/////////////////////////////////////////////////////////////////////////////

/*

pouziti:

a)	
	//Spocita CRC z retezce "123456789" (bez koncovy nuly):
	DWORD CRC = CCRC32("123456789",9) 

b)	//Spocita CRC z tri 32bit. integeru
	int a=1,b=2,c=3;

	CCRC32 crc;
	crc.Cmp(&a,4); //zalezi na poradi volani
	crc.Cmp(&b,4);
	crc.Cmp(&c,4);
	DWORD CRC=crc;

	//stejne CRC vyjde i takto:
	int a[3]={1,2,3}
	DWORD CRC=CCRC32(a,12);


*/

/////////////////////////////////////////////////////////////////////////////

#ifndef FSCRC_H
#define FSCRC_H


class KrkalFSApi CCRC32{
public:
	CCRC32(){CRC=0xFFFFFFFF;}
	CCRC32(const void *data, int size){CRC=0xFFFFFFFF;Cmp(data,size);}
	
	void Cmp(const void *data, int size);
	DWORD GetCRC(){return ~CRC;}

	operator DWORD(){return ~CRC;}

private:
	DWORD CRC;
};

#endif




class KrkalFSApi CFSSearchData
{
private:
	~CFSSearchData();
	class CFSSearchData2 *_data;

public:
	CFSSearchData(class CFSSearchData2 *data) {_data = data;}
	int count(); // gets number of items
	bool GetField(int index, std::string &name, int &attr); // gets one item: ints name and attr: 1=file 2=directory 3=archive
	void Close(); // deletes this object
};




enum ECopyTreeChangeMode {
	ectcmNone,
	ectcmChangeVersion,
	ectcmAddExtension,
	ectcmRemoveExtension,
	ectcmChangeExtension,
};

enum EFullPathType {
	efptWindowsUpperCase,		// C:\KRKAL\GAMES\ERRORS_1234_5897_A87C_55FE.TXT
	efptWindowsOriginalCase,	// C:\Krkal\Games\ERRORS_1234_5897_A87C_55FE.txt
	efptKrkalUpperCase,			// $GAMES$\ERRORS_1234_5897_A87C_55FE.TXT
	efptKrkalOriginalCase,		// $GAMES$\ERRORS_1234_5897_A87C_55FE.txt
	efptInvariantKey,			// ERRORS_1234_5897_A87C_55FE.TXT - file must exist. The path is shortened to name with version if its in search tree. Uppercase is used. If not in search tree efptKrkalUpperCase is used instead.
	efptInvariantKeyOriginalCase,// Errors_1234_5897_A87C_55FE.txt - file must exist. The path is shortened to name with version if its in search tree. If not in search tree efptKrkalOriginalCase is used instead.
};

enum eVersionFeature {
	evfAll = 100,				// afects all types
	evfSystemSupports = 0,		// system supports this feature
	evfFileSupports = 1,		// file supports this feature
	evfSystemRequeres = 2,		// system requieres this feature
	evfFileRequieres = 3,		// file requieres this feature
};

////////////////////////////////////////////////////////////////////////////////////////////////
//
// CFS
//
// "filesystem" - umoznuje pracovat s archivy (CFSArchive) jako s adresari
//
// muze byt vytvorena jen jedna instance CFS
//
// pouzivejte globalni objekt FS !!!
//
////////////////////////////////////////////////////////////////////////////////////////////////


class KrkalFSApi CFS
{
	friend class CFSDir;
	friend class CFSDirCache;
	friend class CFSArchive;
	friend class CFSCfg;
public:
	static int InitFS(); //initace - vola se z KRKAL.cpp
	static void DoneFS(); //deinitace - dtto
	static CFS *GetFS(); // vrati instanci z globalni promenne (singleton)

	int GetFileSize(const char *path); //vraci velikost souboru, pokud je chyba vraci 0
	int ReadFile(const char *name, char *buf, int bufsize); //nacte soubor do bufferu, vraci 0=err 1=OK; bufsize je velikost buferu -> pokud se soubor cely nevejde nacte se zacatek a funkce vrati 2
	int WriteFile(const char *name, char *buf, int size, int compression=-1);  //zapise soubor, vraci 0=err 1=OK, compression: 0=no compr, 1=zlib, -1=stejne jako existujici soubor (pokud neex.=1)
	int CreateDir(const char *name); //vytvori adresar, vraci 0=err 1=OK

	int Delete(const char *name); //smaze soubor/adresar/archiv, vcetne vnorenych souboru a adresaru!!! (tak bacha ;-) (lze mazat i v nadrazenych adresarich - potom ale neni platny akt. adresar - musi se pak zavolat changedir s abs. cestou!), vraci 0=err 1=OK
	int CopyTree(const char *sourcedir, const char *destdir, ECopyTreeChangeMode changeMode = ectcmNone, const char *ext = 0, int comp=-1, const char *newFileName=0); //zkopiruje soubor, dir nebo dir\* do destdir
	// changeMode: 0 - no change, 1 - change version, 2 - add extension, 3 - remove extension, 4 - change extension
	int Move(const char *source, const char *dest); // presune a prejmenuje source (file,dir,dir\*) do dest.

	int CreateArchive(const char *name); //vytvori archiv, vraci 0=err 1=OK
	int PackOrUnpackArchive(const char *path); // changes directory (and its content) to archive. Or changes archive to normal directory structure.

	int GetTime(const char *name, ::FILETIME &time); //vrati datum a cas souboru/adresare
	int SetTime(const char *name, const ::FILETIME &time); //nastavi datum a cas souboru/adresare

	int IsCompressed(const char *name); //vraci jestli je soubor komprimovany

	int FileExist(const char *name); //testuje jestli existuje soubor/adresar; vraci 0=neex. 1=soubor 2=adresar 3=archiv

	int GetFullPath(const char *relpath, char **fullpath, EFullPathType type = efptWindowsOriginalCase); //vrati celou cestu, vola new na fullpath; vraci 0=err 1=OK

	void AddFSDir(const char *key, const char *val); //prida adresar ( nastavi $key$ = val )
	void AddArchiveExtension(const char *ext, int type);
	
	void AddVersionFeature(eVersionFeature type, const char* header, const char* feature, int version); // add requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
	void RemoveVersionFeature(eVersionFeature type, const char* header, const char* feature); // remove requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
	bool CheckVersionFeatures(CFSRegister *reg); // check if version of features of the file matches version of features of the system
	void WriteVersionFeatures(CFSRegister *reg); // writes versions of features

	CFSSearchData *ReadDirectory(const char *path); // returns all files and directories in a directory specified by path. Returns NULL in case of an error.
	CFSSearchData *GetRoots(); // returns the top most root dirs.

	int IsValidFilename(const char *filename); //testuje jestli 'filename' je platny nazev souboru (tj. neobsahuje / a dalsi znaky)

	int Defragment(const char *archvename); //defragmentuje archiv

	int ComparePath(const char *path1, const char *path2); //porovna 2 cesty jestli jsou stejny, vraci 0 kdyz jsou stejny, vlastne dela case insensitive cmp, navic '\'='/'

	__int64 GenerateVersionNumber(char *DestStr = 0);	// vygeneruje nove cislo verze pomoci MersenneTwisteru. Vrati ho jako cislo. Pokud predas funkci pointer na string, kam se vejde 19 znaku (+1 na koncovou nulu), tak i jako string.


private:
	CFS();
	~CFS();

	static int RefCounter; 

	class CFSCfg *cfg;
	class CFSDirCache *dirCache;
	class MTRand *mtr;
	class CArchiveNameVerifier *archiveNameVerifier;
	class CVersionFeatures *versionFeatures;

	int CopyTree2(std::string &destPath, std::string &sourcePath, char *&buff, size_t &buffLen, ECopyTreeChangeMode changeMode, const char *ext, int comp); //zkopiruje vse z akt. adr. do destdir
	int CopyOneFile(const std::string &destPath, const std::string &sourcePath, char *&buff, size_t &buffLen, int comp);
	bool CopyChangeName(std::string &name, ECopyTreeChangeMode changeMode, const char *ext);
	bool DeleteDirContent(const std::string &dir);
	int MoveDirContent(std::string sourcePath, std::string destPath);
	bool SplitPath(const std::string &path, std::string *dir, std::string *name);

	CRITICAL_SECTION fsCriticalSection;

};








#define FSRMAXREGKEYVEL (1024*1024*16) // Maximalni velikost klice. Omezeni je kvuli bezpecnosti.
#define FSMAXHEADSIZE 1024

// Mozne hodnoty OPEN ERRORU
#define FSREGOK 1
#define FSREGFILEERROR 2
#define FSREGCONTENTERROR 0

// Zadaj, kdyz nechces registr loadovat, ale rovnou otevrit prazdny
#define FSREGCLEARIT 1

// Podporovane Typy Klicu:
#define FSRNumTypes 8 // warning currently there is only 3bits space for the type!
enum EFSRTypes {
	FSRTchar = 0,
	FSRTint,
	FSRTint64,
	FSRTdouble,
	FSRTregister,
	FSRTstring,
	FSRTwChar,
	FSRTwString,
};






///////////////////////////////////////////////////////////////////////////////////
///
///			K L I C		- CFSRegKey
///
///////////////////////////////////////////////////////////////////////////////////

class KrkalFSApi CFSRegKey  {
friend CFSRegister;
friend CFSRegHT;
public:
	void writec(char a); // zapis na pos, pos se posune na dalsi polozku,
	void writei(int a);  // pocet dat se pripadne rozsiri (top)
	void writed(double a);  // pokud se data jiz nevejdou do stavajiciho pole, bude zvetseno
	void write64(__int64 a);  // pozivej spravnou funkci k danemu typu klice !!
	void writew(wchar_t a);
	char readc();  // cteni dat, pokud ctete nezapsana data, nebo data za koncem (za top) je vysledek nedefinovany (vetsinou 0)
	int readi();	// cteni z pos, pos se posune na dalsi polozku	
	double readd();  // pozivej spravnou funkci k danemu typu klice !!
	__int64 read64();
	wchar_t readw();
	void blockwrite(const void *source, int Size); // size je v poctech polozek
	int blockread(void *dest, int Size); // size je v poctech polozek, muze precist i min, kdyz narazi na konec streamu. Vraci pocet prectenych polozek.
	char * stringread(); // vraci Null, kdyz se cte za koncem, jinak string ukonceny nulou. (vytvori ho) Je na tobe, abys ho zrusil. Pouzivej jen na typ char nebo string
	void stringwrite(const char *string) { assert(type==FSRTchar||type==FSRTstring); blockwrite(string,strlen(string)+1); } //zapise string pomoci blockwrite. Pouzivej jen na typ String nebo Char!
	wchar_t *wstringread(); // vraci Null, kdyz se cte za koncem, jinak string ukonceny nulou. (vytvori ho) Je na tobe, abys ho zrusil. Pouzivej jen na typ char nebo string
	void wstringwrite(const wchar_t *string) { assert(type==FSRTwChar||type==FSRTwString); blockwrite(string,wcslen(string)+1); } //zapise string pomoci blockwrite. Pouzivej jen na typ wString nebo wChar!
	void SetPosToNextString(); // Najde prvni nulu (testuje polozku, ne char)

	int pos; // pozice, kam se bude cist, nebo zapisovat. Nastav ji = seek
	int top; // vrchol dat (=pocet dat) nastav, na x, kdyz chces polozky od x dal smazat. (Kdyz top zvetsis, klic se zvetsi o nedefinovane polozky) Funkce write sami nastavuji top.
	int eof() {return (pos>=top); } // vrati 1 kdyz jsem za koncem (nejde cist)
	void seek(int position) {pos=position;} // nastaveni pozice
	
	void rename(const char *Name); // prejmenovani klice (jmeno bude zkopirovano)
	const char *GetName() {return name;} // jmeno nemenit! Pointer muze prestat platit!
	CFSRegKey *GetNextKey() {return next;} // vrati dalsi klic v registru
	CFSRegister *GetSubRegister() {return subregister;} // vrati vnoreny registr (0 kdyz neexistuje)
	int CFSGetKeyType() {return type;} // vrati typ tohoto klice

	int resize(int vel); // zvetsi pole s daty na dvojnasobek nebo na vel (podle toho, co je vic)	
	char * GetDirectAccess() { return data; } // primy pristup. Je treba si volat resize a settop
											  // po resize se musi GetDirectAccess zavolat znova!!
											  // Nerusit!
	char * GetDirectAccessFromPos(); // vrati ukazatel na aktualni pozici v datech (Po resize se musi volat znova)
	wchar_t * GetWStringAccessFromPos() { assert(type==FSRTwChar||type==FSRTwString); return ((wchar_t*)data) + pos; } // vrati ukazatel na aktualni pozici v datech (Po resize se musi volat znova)
	void SetAllData(char *Data, int Size); // Data v klici budou zahozena a nahrazena temito daty. Size je bajtech. o Data* se od teto chvile stara registr. Uz nemenit, nerusit, Data zrusi registr
	int GetDataArraySize() {return size; } // vrati velikost pole s daty v bajtech (vyuziti jen na primi pristup)
private:
	CFSRegKey(CFSRegister *reg,char Type, const char *Name, CFSRegister *SubReg = 0); //Konstruktor pro tvorbu Noveho klice
	CFSRegKey(CFSRegister *reg,unsigned int *datasource,char *buffer, int &POS, int vel); //Konstruktor pro nacteni klice ze souboru
	virtual ~CFSRegKey(); 
	void zjistivelikostazmenu(int &vel,int &changed); // zjisti aktualni velikost
	void collectall(char *buff, int &pos, int &tablepos); // posbira vsechny data a sesype je do jednoho pole buff (pos-pozice v poli, tablepos - pozice tabulky klicu)

	void AddToHashTable();  // Pridej do hashTable
	void HFindAndDelete(CFSRegKey **p2,CFSRegKey *key); // najde jmeno key v seznamu p2 a odstrani ho

	char *data; // pointer na data
	char *name; // jmeno klice
	int size;   // velikost pole na data v Bajtech
	int size2;  // velikost pole na data v polozkach
	UC type;   // typ klice
	UC state;  //zda ma klic okopirovany data - bit0 (jmeno - bit1) k sobe a zda je ma smazat
	CFSRegKey *next;  // dalsi klic v registru
	CFSRegKey *HTnext; // dalsi klic v hashovacim retizku
	CFSRegister *subregister; // pointer na subregister
	CFSRegister *MyRegister; // registr, ve kterem je tento klic ulozen
};



/////////////////////////////////////////////////////////////////////////////////
///
///			R E G I S T E R		- CFSRegister
///
/////////////////////////////////////////////////////////////////////////////////



class KrkalFSApi CFSRegister {
friend CFSRegKey;
public:
	CFSRegister(const char *path, char *head, int ClearIt=0); //path - cesta k souboru s registrem. Head hlavicka souboru pr. "KRKAL LEVEL" - bude kontrolovana
															  // Nastav ClearIt na 1 (FSREGCLEARIT) pokud chces vytvorit prazdny registr. Pri operaci save bude pripadny stary registr prepsan (OpenErroe je v tomto pripade vzdy FSREGOK)
															  // Pokud Nastavis Head na NULL hlavicka se precte z fajlu (nepouzivat dohromady s flagem FSREGCLEARIT a pro tvoreni novych registru)
	CFSRegister(); // KONSTRUKTOR  pro vytvoreni prazdneho (nepropojeneho!) subregistru
	~CFSRegister(); // Registr se zrusi, zmeny se neulozi
	int WriteFile(); // Zapsani registru na disk, vrati 1 - OK, 0 - Chyba
	int ChangePath(const char *path); // zmeni cestu k souboru (save as .. nekam jinam)
	CFSRegKey *AddKey(const char *name, int typ) {return new CFSRegKey(this,typ,name);} // Vytvori novy klic typu typ v registru, jmeno bude zkopirovano
	CFSRegKey *AddRegisterToKey(const char *name, CFSRegister *SubReg) {return new CFSRegKey(this,FSRTregister,name,SubReg);} // Prida do registru registr. (propoji je) Pridavany registr musi byt samostatne vytvoreny (nepropojeny)
	void DeleteKey(CFSRegKey *key); // zrusi klic
	CFSRegKey *GetFirstKey() {return keys;} // vrati prvni klic ze seznamu vsech klicu registru
	CFSRegKey *FindKey(const char *name); // vyhleda klic pogle jmena
	CFSRegKey *FindNextKey(const char *name,CFSRegKey *PrevKey); // vyhleda nasledujici klic podle jmena
	int FindKeyPos(CFSRegKey *key); // postupnym prohledavanim zjisti pozici klice. vrati pozici nebo -1 v pripade neuspechu
	void DeleteAllKeys(); // zrusi vsechny klice
	void SeekAllTo0(); // Nastavi u vsech klicu v registru a podregistrech seek na 0
	int GetOpenError() { return OpenError;} // 1 - OK Register Loaded; 2 - Error in file (not exists); 0 - Error In Registr; viz makyrka FSREGOK, ..
										   // Pri chybe je vytvoren prazdny registr. Zapsan do souboru bude ale az prikazem WriteFile
	void SetRegisterToBeCompressed();
	void SetRegisterToBeUnCompressed();
	const char* GetHeader();
	bool CheckVersionFeatures(); // check if version of features of the file matches version of features of the system
	void WriteVersionFeatures(); // writes versions of features
	int GetNumberOfKeys() {return NumKeys;} // Vrati pocet klicu v tomto registru
	static int VerifyRegisterFile(const char *path, const char *head = 0); // precte ze souboru hlavicku a overi zda sedi. Vraci OpenError.
private:
	void zjistivelikostazmenu(int &vel,int &changed); // zjisti aktualni velikost
	void collectall(char *buff, int &pos, int &tablepos); // posbira vsechny data a sesype je do jednoho pole buff (pos-pozice v poli, tablepos - pozice tabulky klicu)
	CFSRegister(CFSRegKey *key); // Nahraje registr z klice
	void CreateHT(); // vytvori hashovaci tabulku
	UC RegChanged; // Kdyz 1 tak registr byl zmene, kdyz 0 tak se nevi
	UC OpenError;
	short NumKeys;  // pocet klicu
	CFSRegKey *keys; // seznam vsech klicu - prvni klic
	CFSRegKey *lastkey; //posledni klic
	CFSRegHT *HashTable;  // hashovaci tabulka jmen klicu tohoto registru
	CFSRootRegInfo *RootInfo; // informace pro korenovy registr
};





#endif