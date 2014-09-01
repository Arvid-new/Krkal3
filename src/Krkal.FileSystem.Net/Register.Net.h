#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

#include "fs.api.h"
#include "CStringToCharPtr.h"
#include "Krkal.FileSystem.Net.h"

namespace Krkal {
	namespace FileSystem {


		public enum class FSRegKeyType {
			Char = 0,
			Int,
			Int64,
			Double,
			Register,
			String,
			WChar,
			WString,
		};



		// Mozne hodnoty OPEN ERRORU
		public enum class FSRegOpenError {
			OK = 1,
			FileError = 2,
			ContentError = 0,
		};


		value class FSRegister;
		ref class FSRegisterFile;


		public value class FSRegKey  {
		internal:
			CFSRegKey *_key;
			FSRegKey(CFSRegKey *key) {
				_key = key;
			}

		public:
	
			void WriteC(char a) { _key->writec(a);} // zapis na pos, pos se posune na dalsi polozku,
			void WriteI(int a) {_key->writei(a);}  // pocet dat se pripadne rozsiri (top)
			void WriteD(double a) {_key->writed(a);}  // pokud se data jiz nevejdou do stavajiciho pole, bude zvetseno
			void Write64(__int64 a) {_key->write64(a);}  // pozivej spravnou funkci k danemu typu klice !!
			void WriteW(wchar_t a) {_key->writew(a);}
			
			char ReadC() { return _key->readc(); }  // cteni dat, pokud ctete nezapsana data, nebo data za koncem (za top) je vysledek nedefinovany (vetsinou 0)
			int ReadI() { return _key->readi(); } 	// cteni z pos, pos se posune na dalsi polozku	
			double ReadD()  { return _key->readd(); }   // pozivej spravnou funkci k danemu typu klice !!
			__int64 Read64() { return _key->read64(); } 
			wchar_t ReadW() { return _key->readw(); } 


			void WriteC(array<char> ^a, int pos, int size) {
				if (Type != FSRegKeyType::Char && Type != FSRegKeyType::String)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<char> p = &a[pos];
				_key->blockwrite(p, size);
			}
			void WriteI(array<int> ^ a, int pos, int size) {
				if (Type != FSRegKeyType::Int)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<int> p = &a[pos];
				_key->blockwrite(p, size);
			}
			void WriteD(array<double> ^ a, int pos, int size) {
				if (Type != FSRegKeyType::Double)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<double> p = &a[pos];
				_key->blockwrite(p, size);
			}
			void Write64(array<__int64> ^ a, int pos, int size) {
				if (Type != FSRegKeyType::Int64)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<__int64> p = &a[pos];
				_key->blockwrite(p, size);
			}
			void WriteW(array<wchar_t> ^ a, int pos, int size) {
				if (Type != FSRegKeyType::WChar && Type != FSRegKeyType::WString)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<wchar_t> p = &a[pos];
				_key->blockwrite(p, size);
			}

			int ReadC(array<char> ^a, int pos, int size) {
				if (Type != FSRegKeyType::Char && Type != FSRegKeyType::String)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<char> p = &a[pos];
				return _key->blockread(p, size);
			}
			int ReadI(array<int> ^a, int pos, int size) {
				if (Type != FSRegKeyType::Int)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<int> p = &a[pos];
				return _key->blockread(p, size);
			}
			int ReadD(array<double> ^a, int pos, int size) {
				if (Type != FSRegKeyType::Double)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<double> p = &a[pos];
				return _key->blockread(p, size);
			}
			int Read64(array<__int64> ^a, int pos, int size) {
				if (Type != FSRegKeyType::Int64)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<__int64> p = &a[pos];
				return _key->blockread(p, size);
			}
			int ReadW(array<wchar_t> ^a, int pos, int size) {
				if (Type != FSRegKeyType::WChar && Type != FSRegKeyType::WString)
					throw gcnew NotSupportedException("invalid type");
				pin_ptr<wchar_t> p = &a[pos];
				return _key->blockread(p, size);
			}


			// vraci Null, kdyz se cte za koncem. Cte jak char* tak wchar_t*
			String ^StringRead() {
				String ^ret;
				if (Eof)
					return nullptr;
				if (Type == FSRegKeyType::WChar || Type == FSRegKeyType::WString) {
					ret = gcnew String(_key->GetWStringAccessFromPos());
					_key->SetPosToNextString();
				} else if (Type == FSRegKeyType::Char || Type == FSRegKeyType::String) {
					ret = gcnew String(_key->GetDirectAccessFromPos());
					_key->SetPosToNextString();
				} else {
					throw gcnew NotSupportedException("invalid type");
				}
				return ret;
			}

			void StringWrite(String ^a) {
				if (Type == FSRegKeyType::WChar || Type == FSRegKeyType::WString) {
					_key->wstringwrite(CStringToWCharPtr(a));
				} else if (Type == FSRegKeyType::Char || Type == FSRegKeyType::String) {
					_key->stringwrite(CStringToCharPtr(a));
				} else {
					throw gcnew NotSupportedException("invalid type");
				}
			}


			// pozice, kam se bude cist, nebo zapisovat. Nastav ji = seek
			property int Pos {
				int get() { return _key->pos; }
				void set(int value) { _key->pos = value;}
			}

			// vrchol dat (=pocet dat) nastav, na x, kdyz chces polozky od x dal smazat. (Kdyz top zvetsis, klic se zvetsi o nedefinovane polozky) Funkce write sami nastavuji top.
			property int Top {
				int get() { return _key->top; }
				void set(int value) { _key->top = value;}
			}

			// vrati 1 kdyz jsem za koncem (nejde cist)
			property bool Eof {
				bool get () { return (_key->eof() != 0); }
			}

			void Seek(int position) { _key->seek(position); }

			property String ^Name {
				String ^get() { return gcnew String(_key->GetName()); }
				void set(String ^value) { _key->rename(CStringToCharPtr(value)); }
			}

			// vrati dalsi klic v registru
			property FSRegKey NextKey {
				FSRegKey get() { return FSRegKey(_key->GetNextKey()); }
			}

			// vrati vnoreny registr (0 kdyz neexistuje)
			property FSRegister Subregister {
				FSRegister get();
			}

			property FSRegKeyType Type {
				FSRegKeyType get() { return (FSRegKeyType)_key->CFSGetKeyType();} // vrati typ tohoto klice
			}




			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _key == 0;
				} else if (obj->GetType() == FSRegKey::typeid) {
					return _key == ((FSRegKey)obj)._key;
				} else {
					return false;
				}
			}


			static bool operator == (FSRegKey key1, FSRegKey key2) {
				return (key1._key == key2._key);
			}

			static bool operator != (FSRegKey key1, FSRegKey key2) {
				return (key1._key != key2._key);
			}

			property bool IsNull {
				bool get() { return (_key == 0);}
			}

		};







		public value class FSRegister  {
		private:
			CFSRegister *_reg;
		internal:
			CFSRegister *GetReg() { return _reg;}

			FSRegister(CFSRegister *reg) {
				_reg = reg;
			}
			//path - cesta k souboru s registrem. Head hlavicka souboru pr. "KRKAL LEVEL" - bude kontrolovana
			// Nastav ClearIt na 1 (FSREGCLEARIT) pokud chces vytvorit prazdny registr. Pri operaci save bude pripadny stary registr prepsan (OpenErroe je v tomto pripade vzdy FSREGOK)
			// Pokud Nastavis Head na NULL hlavicka se precte z fajlu (nepouzivat dohromady s flagem FSREGCLEARIT a pro tvoreni novych registru)
			FSRegister(String ^path, String ^head) { _reg = new CFSRegister(CStringToCharPtr(path), CStringToCharPtr(head)); } 
			FSRegister(String ^path, String ^head, bool clearIt) { _reg = new CFSRegister(CStringToCharPtr(path), CStringToCharPtr(head), (int)clearIt); } 

			// KONSTRUKTOR  pro vytvoreni prazdneho (nepropojeneho!) subregistru
			void CreateEmpty() { _reg = new CFSRegister();}
			// Registr se zrusi, zmeny se neulozi
			void FreeMemory() { SAFE_DELETE(_reg); }


		public:



			FSRegKey AddKey(String ^name, FSRegKeyType type) {return FSRegKey(_reg->AddKey(CStringToCharPtr(name), (int)type)); } // Vytvori novy klic typu typ v registru, jmeno bude zkopirovano
			FSRegKey AddRegisterToKey(String ^name, FSRegisterFile ^subRegister);  // Prida do registru registr. (propoji je) Pridavany registr musi byt samostatne vytvoreny (nepropojeny)
			void DeleteKey(FSRegKey key) { _reg->DeleteKey(key._key); } // zrusi klic
			
			FSRegKey GetFirstKey() {return FSRegKey(_reg->GetFirstKey());} // vrati prvni klic ze seznamu vsech klicu registru
			FSRegKey FindKey(String ^name) { return FSRegKey(_reg->FindKey(CStringToCharPtr(name))); } // vyhleda klic pogle jmena
			FSRegKey FindNextKey(String ^name, FSRegKey prevKey) { return FSRegKey(_reg->FindNextKey(CStringToCharPtr(name), prevKey._key)); } // vyhleda nasledujici klic podle jmena
			
			int FindKeyPos(FSRegKey key) { return _reg->FindKeyPos(key._key); } // postupnym prohledavanim zjisti pozici klice. vrati pozici nebo -1 v pripade neuspechu
			void DeleteAllKeys() {_reg->DeleteAllKeys(); } // zrusi vsechny klice
			void SeekAllTo0() { _reg->SeekAllTo0(); }  // Nastavi u vsech klicu v registru a podregistrech seek na 0

			// Vrati pocet klicu v tomto registru
			property int CountOfKeys {
				int get() { return _reg->GetNumberOfKeys();}
			}



			virtual bool Equals(Object ^obj) override {
				if (obj == nullptr) {
					return _reg == 0;
				} else if (obj->GetType() == FSRegister::typeid) {
					return _reg == ((FSRegister)obj)._reg;
				} else {
					return false;
				}
			}


			static bool operator == (FSRegister reg1, FSRegister reg2) {
				return (reg1._reg == reg2._reg);
			}

			static bool operator != (FSRegister reg1, FSRegister reg2) {
				return (reg1._reg != reg2._reg);
			}

			property bool IsNull {
				bool get() { return (_reg == 0);}
			}


		};




		public ref class FSRegisterFile : IDisposable {
		private:
			FSRegister _reg;

		public:

			property FSRegister Reg {
				FSRegister get() { return _reg;}
			}



			//path - cesta k souboru s registrem. Head hlavicka souboru pr. "KRKAL LEVEL" - bude kontrolovana
			// Nastav ClearIt na 1 (FSREGCLEARIT) pokud chces vytvorit prazdny registr. Pri operaci save bude pripadny stary registr prepsan (OpenErroe je v tomto pripade vzdy FSREGOK)
			// Pokud Nastavis Head na NULL hlavicka se precte z fajlu (nepouzivat dohromady s flagem FSREGCLEARIT a pro tvoreni novych registru)
			FSRegisterFile(String ^path, String ^head) { _reg = FSRegister(path, head);} 
			FSRegisterFile(String ^path, String ^head, bool clearIt) { _reg = FSRegister(path, head, clearIt);} 

			// KONSTRUKTOR  pro vytvoreni prazdneho (nepropojeneho!) subregistru
			FSRegisterFile() { _reg.CreateEmpty();}

	

			~FSRegisterFile() {
				this->!FSRegisterFile();
			}
			!FSRegisterFile() {
				_reg.FreeMemory();
			}




			// Zapsani registru na disk, vrati 1 - OK, 0 - Chyba
			void WriteFile() {
				if (!_reg.GetReg()->WriteFile())
					throw gcnew FSFileNotFoundException();
			}

			// zmeni cestu k souboru (save as .. nekam jinam)
			void ChangePath(String ^path) {
				if (!_reg.GetReg()->ChangePath(CStringToCharPtr(path)))
					throw gcnew FSFileNotFoundException();
			}

			// 1 - OK Register Loaded; 2 - Error in file (not exists); 0 - Error In Registr; viz makyrka FSREGOK, ..
			// Pri chybe je vytvoren prazdny registr. Zapsan do souboru bude ale az prikazem WriteFile
			property FSRegOpenError OpenError {
				FSRegOpenError get() { return (FSRegOpenError)_reg.GetReg()->GetOpenError(); }
			}

			void SetRegisterToBeCompressed() {_reg.GetReg()->SetRegisterToBeCompressed();}
			void SetRegisterToBeUnCompressed() {_reg.GetReg()->SetRegisterToBeUnCompressed();}

			// check if version of features of the file matches version of features of the system
			bool CheckVersionFeatures() { return _reg.GetReg()->CheckVersionFeatures(); }
			// writes versions of features
			void WriteVersionFeatures() { _reg.GetReg()->WriteVersionFeatures(); } 


			// precte ze souboru hlavicku a overi zda sedi. Vraci OpenError.
			static FSRegOpenError VerifyRegisterFile(String ^path) { return VerifyRegisterFile(path, nullptr); }
			static FSRegOpenError VerifyRegisterFile(String ^path, String ^head) { return (FSRegOpenError)CFSRegister::VerifyRegisterFile(CStringToCharPtr(path), CStringToCharPtr(head)); }

		internal:
			FSRegister Detach() {
				FSRegister ret = _reg;
				_reg = FSRegister();
				return ret;
			}
		};


	}
}