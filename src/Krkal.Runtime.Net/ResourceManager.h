
#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

#include "RootNames.h"
#include "CStringToCharPtr.h"

class CFS;
class CFSSearchData;


namespace Krkal {
	namespace Runtime {




		class CTranslation {
		public:
			CTranslation(const char *language);
			~CTranslation();
			const wchar_t *GetUserNameOrComment(const char* ksid, bool isComment);
			bool ReloadIfNeeded();
			const char *GetLanguage() { return _language; }

		private:
			struct STrnField {
				wstring UserName;
				wstring Comment;
				UI date;
			};
			typedef unordered_map<string, STrnField> MapT;
			typedef pair<string, ::FILETIME> FileT;
			typedef vector<FileT> FilesT;

			char *_language;
			MapT _map;
			FilesT *_files;

			bool AddTranslationFile(const char *file);
			CFSRegister *ReadFiles();
			CFSRegister *OpenCache(bool create);
			void CountFiles(CFSSearchData *search, int &count);
			void LoadFromCache(CFSRegister *cache, bool &ret);
			void Initialize();
			void LoadFiles(const string &path, CFSSearchData *search, bool &ret);
			void LoadFile(const string &path, bool &ret);
			void SaveCache(bool &ret);
			void SaveField(CFSRegister *names, const char *ksid, const STrnField &field);

		};








		class CResourceManager : public ::IKrkalResourceManager {
		public:
			CResourceManager();
			~CResourceManager();
			bool Load(const char *primaryLanguage, const char *secondaryLanguage);

			virtual const wchar_t *GetUserNameOrComment(const char* ksid, bool isComment);

			// input "_KSId_....:default text" - both parts are optional
			// if it finds the translation for ksid, it return it
			// otherwise the part after the : is returned (if there is no such default text, empty string is returned)
			// if ksid part is missing whole input text is returned
			// function returns null only if the input text is null
			virtual const wchar_t *GetText(const wchar_t* text);
			virtual const wchar_t *GetText(const char *ksid, const wchar_t *defaultText) {
				const wchar_t *str = GetUserNameOrComment(ksid, true);
				if (!str || !*str)
					return defaultText;
				return str;
			}
			virtual bool ReloadIfNeeded();

		private:
			CRITICAL_SECTION _criticalSection;
			CTranslation *_primary;
			CTranslation *_secondary;
			CFS *_fs;
		};










		public ref class KrkalResourceManager : public Krkal::IKrkalResourceManager, public IDisposable{
		private:
			CResourceManager *_manager;
		public:
			virtual String ^GetUserNameOrComment(String ^ksid, bool isComment) {
				return gcnew String(_manager->GetUserNameOrComment(CStringToCharPtr(ksid), isComment));
			}
			virtual String ^GetText(String ^text) {
				return gcnew String(_manager->GetText(CStringToWCharPtr(text)));
			}
			virtual bool ReloadIfNeeded() {
				return _manager->ReloadIfNeeded();
			}
			virtual String ^GetText(String ^ksid, String ^defaultText) {
				const wchar_t *str = _manager->GetUserNameOrComment(CStringToCharPtr(ksid), true);
				if (!str || !*str)
					return defaultText;
				return gcnew String(str);
			}


			KrkalResourceManager() {
				_manager = new CResourceManager();
				CRootNames::ResourceManager = _manager;
			}
			bool Load(String ^primaryLanguage, String ^secondaryLanguage) {
				return _manager->Load(CStringToCharPtr(primaryLanguage), CStringToCharPtr(secondaryLanguage));
			}

			~KrkalResourceManager() {
				this->!KrkalResourceManager();
			}

			!KrkalResourceManager() {
				SAFE_DELETE(_manager);
			}

		};


	}
}
