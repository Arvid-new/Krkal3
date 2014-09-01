//////////////////////////////////////////////////////////////////////////
///
///		R E G I S T R Y  - Prace se strukturovanym souborem
///
///		A: M.D.
///
//////////////////////////////////////////////////////////////////////////

/*
	Co to umi?
	----------
	Umi pracovat se soubory se specialni strukturou. Soubor zacina hlavickou
	KrkalReg\0. Nasleduje hlavicka zadana uzivatelem
	(To je string ukonceny nulou) Podle hlavicky se kontroluje jestli je soubor OK
	Dale soubor obsahuje klice. Klic je tvoren jmenem (0-string) a 0 az n datovymi 
	polozkami. Ke kazdemu klici je prirazen typ, ktery urcuje jakeho typu budou
	polozky klice. Mozne typy jsou: Char 1B, Int 4B, Double 8B, _in64 8B, string 1B 
	(string se chova naprosto stejne jako typ char) a typ Register = vnoreny registr.
	Data klicu je mozne menit, pridavat a ubirat. Velikost klicu se podle toho 
	automaticky meni. Na data u klice se lze divat jako na samostatny stream (soubor)
	a vetsina operaci s daty je take velmi podobna klasickym operacim se souborem.

	Pouziti
	-------
	Registr se otevre nebo vytvori volanim:  new CFSRerister(<cesta>,<hlavicka>);
	Metodou GetOpenError() lze otestovat chyby pri otevirani
	Trida CFSRegister umoznuje vyhledavat, pridavat a rusit klice.
	Metoda WriteFile ulozi cely registr na disk (vcetne vnorenych registru)
	Trida CFSRegKey umoznuje praci se samotnym klicem.
	Pouzivejte stravne typy metod pro dany typ klice!!
	Pokud je klic typu registr, metodou GetSubRegister() ziskate pristup k vnorenemu 
		registru. Data samotneho klice v zadnem pripade nemente!!
	Dulezite promene u klice:
		pos - aktualni polozka - zde se bude zapisovat nebo cist. Zmena pos = seek
		top - pocet polozek - muzete menit (tim se daji umazavat polozky)
	Je mozny i primy pristup, Ale pozor, pointery na primy pristup muzou prestat platit!
		(Vhodne treba, kdyz nejaka funkce si chce precist (=okopirovat) string)

*/


#ifndef FSREGISTER_H
#define FSREGISTER_H

#include "types.h"
#include "fs.api.h"
#include "fs.h"


#define KER_REGISTER_HTS 251


extern int FSRTypeSizes[FSRNumTypes];
extern CFSRegHT *RegActualHT;




///////////////////////////////////////////////////////////////////////////////////
///
///			H A S H O V A C I   T A B U L K A   P R O   R E G I S T R Y
///
///////////////////////////////////////////////////////////////////////////////////


struct CFSRegHT {
	CFSRegHT() {
		int f;
		for (f=0; f<KER_REGISTER_HTS; f++) HT[f] = 0;
		NumKeys = 0;
		NumRefs = 0;
	}
	~CFSRegHT() {
		if (RegActualHT == this) RegActualHT = 0;
	}
	void Release() {
		NumRefs--;
		if (NumRefs<=0) delete this;
	}
	CFSRegKey *HT[KER_REGISTER_HTS];
	int NumKeys;
	int NumRefs;
	CFSRegKey *Member(const char *name, CFSRegister *r, CFSRegKey *PrevKey = 0); // Vyhleda nasledujici vyskyt, nebo prvni vyskyt, pokud je PrevKey ponechan na 0
	int HashFunction(const char *name, CFSRegister *r);
};








/////////////////////////////////////////////////////////////////////////////////
///
///			R E G I S T E R		- CFSRegister
///
/////////////////////////////////////////////////////////////////////////////////


struct CFSRootRegInfo {
	int deletebuff;  // zda vlastnim buffer a budu ho muset tedy smazat
	char *buffer;  // data
	char *Path;    //  absolutni cesta k souboru (u vnorenych registru je to NULL)
	char *Head;    // hlavicka (u vnorenych registru je to NULL)
	int CompressMode; // Hodnota nema smysl u SubRegistru. -1 Povodni nastaveni (viz WriteFile u FS), 0 - NoCompress, 1 - Compress
	CFSRootRegInfo() {deletebuff=0; buffer=0; Path=0; Head=0; CompressMode=-1; }
};






#endif
