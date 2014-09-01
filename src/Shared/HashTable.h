////////////////////////////////////////////////////////////////////////////////////////
///
///  HASH TABLE
///
///  A: M.D.
///
////////////////////////////////////////////////////////////////////////////////////////

/*
	Hashovacim klicem je promenna name - cos je string ukonceny nulou

	POUZITI
	-------
	Vytvorte si hashovaci tabulku pozadovane velikosti (jen mocnina 2)
	A ukladejde do ni (hledejte, odebirejte) objekty oddedene od objektu typu CHashElem
	POZOR HashElem nerusi svou promennou name!!! To naprogramujte v destruktoru potomka!
*/

#pragma once

#ifndef __HASH_TABLE_H__
#define __HASH_TABLE_H__

#ifdef KRKALFILESYSTEM_EXPORTS
	#define KrkalFSApi __declspec(dllexport)
#else
	#define KrkalFSApi __declspec(dllimport)
#endif



class CHashElem;
class CHashList;



////////////////////////////////////////////////////////////////////////////////////////
///
///		CHashTable - HASHOVACI TABULKA
///
////////////////////////////////////////////////////////////////////////////////////////
class KrkalFSApi CHashTable {
friend CHashElem;
public:
	CHashTable(int hashsize) { // hashsize (= velikost tabulky) musi byt mocniny 2
		int f;
		HashSize = hashsize-1;
		HashTable = new CHashList*[hashsize];
		for (f=0;f<hashsize;f++) HashTable[f]=0;
	};
	CHashElem *Member(const char *name); // Vyhleda v HashTabulce podle jmena
	CHashElem *MemberFirst(const char *name, CHashList **ptr); // Vyhleda prvni vyskyt
	CHashElem *MemberNext(const char *name, CHashList **ptr); // Vyhleda nasledujici vyskyt
	CHashElem *MemberLast(const char *name, CHashList **ptr); // Vyhleda posledni vyskyt
	CHashElem *MemberPrev(const char *name, CHashList **ptr); // Vyhleda predchozi vyskyt
	~CHashTable(); // odebere vsechny polozky z tabulky a zrusi tabulku (polozky neodstrani)
	void DeleteAllMembers(); // odebere a odstrani vsechny polozky
private:
	int HashFunction(const char *name);
protected:	// Jirka
	CHashList **HashTable; // Hash Table 
	int HashSize;
};


//////////////////////////////////////////////////////////////////////////////////
/// Spoljovy seznam Hash Elementu
class KrkalFSApi CHashList {
friend CHashElem;
friend CHashTable;
public:
	CHashList(CHashElem * Name, CHashList *Next) {
		name=Name;
		next=Next;
	}
//private:
public:
	CHashElem *name;
	CHashList *next;
};




/////////////////////////////////////////////////////////////////////////////////
///
///		Hash Element
///
///		Muze byt pridan do jedne hashovaci tabulky. Hashuje se podle jmena (string)
///
/////////////////////////////////////////////////////////////////////////////////
class KrkalFSApi CHashElem {
friend CHashTable;
public:
	CHashElem(){
		name = 0;
		HashTable = 0;
	}
	void SetName(const char *Name) { // Nastaveni jmena. Pozor JMENO NENI RUSENO. Je treba doprogramovat u potomka
		name = new char[strlen(Name)+1]; // zkopiruju si string se jmenem
		strcpy(name,Name);
	}
	void AddToHashTable(CHashTable *ht) { // Pridej do hashTable
		if (HashTable) throw CExc(eHash,0,"Hashed in more than one HashTables!");
		HashTable=ht;
		CHashList **list = HashTable->HashTable+HashTable->HashFunction(name);
		*list = new CHashList(this,*list);
	}
	int AddToHashTableUnique(CHashTable *ht) { // Pridej do hashTable Pokud tam neni
		if (HashTable) throw CExc(eHash,0,"Hashed in more than one HashTables!");
		HashTable=ht;
		CHashList **list = HashTable->HashTable+HashTable->HashFunction(name);
		if (ht->MemberNext(name,list)) return 0;
		*list = new CHashList(this,*list);
		return 1;
	}
	void RemoveFromHashTable() {  // Odeber z HashTable
		if (HashTable) HFindAndDelete(&(HashTable->HashTable[HashTable->HashFunction(name)]),this);
		HashTable = 0;
	}
	virtual ~CHashElem(){ // Prvek se odebere z Hashovaci tabulky
		if (HashTable) HFindAndDelete(&(HashTable->HashTable[HashTable->HashFunction(name)]),this);
		// Pozor JMENO name NENI RUSENO. Je treba doprogramovat u potomka
	}
private:
	void HFindAndDelete(CHashList **p2,CHashElem *name); // najde jmeno v seznamu a odstrani ho
//protected:
public:
	char *name; // string jmena
	CHashTable *HashTable; // Hash Table, kde jsem; 0 - nejsem v zadne HashTable
};




#endif