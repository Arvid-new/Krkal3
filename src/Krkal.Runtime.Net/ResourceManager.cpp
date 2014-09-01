#include "stdafx.h"

#include "ResourceManager.h"
#include "fs.api.h"

using namespace Krkal::Compiler;


namespace Krkal {
namespace Runtime {




	CTranslation::CTranslation(const char *language) {
		_language = newstrdup(language);
		_files = 0;
	}




	CTranslation::~CTranslation() {
		SAFE_DELETE(_files);
		SAFE_DELETE_ARRAY(_language);
	}





	const wchar_t *CTranslation::GetUserNameOrComment(const char* ksid, bool isComment) {
		MapT::iterator i = _map.find(ksid);
		if (i != _map.end()) {
			if (isComment) {
				if (i->second.Comment.size())
					return i->second.Comment.c_str();
			} else {
				if (i->second.UserName.size())
					return i->second.UserName.c_str();
			}
		}
		return 0;
	}






	bool CTranslation::ReloadIfNeeded() {
		bool startFromCache = false;
		CFS *fs = CFS::GetFS();
		bool ret = true;

		string search1path = string("$TRANSLATIONS$\\") + _language;
		CFSSearchData *search1 = fs->ReadDirectory(search1path.c_str());
		CFSSearchData *search2 = fs->ReadDirectory("$MIXEDTRANSLATIONS$");
		CFSRegister *cache = 0;
		try {

			if (!_files)
				cache = ReadFiles();

			int count = 0;
			CountFiles(search1, count);
			CountFiles(search2, count);


			if (count && _files && _files->size() == count) {
				for (FilesT::iterator i = _files->begin(); i != _files->end(); ++i) {
					::FILETIME ft;
					if (!fs->GetTime(i->first.c_str(), ft))
						break;
					if (ft.dwLowDateTime != i->second.dwLowDateTime || ft.dwHighDateTime != i->second.dwHighDateTime)
						break;
					count--;
				}
				
				if (count == 0) {
					startFromCache = true;
					if (cache)
						LoadFromCache(cache, ret);
				}
			}

		

			if (!startFromCache) {
				Initialize();
				LoadFiles(search1path, search1, ret);
				LoadFiles("$MIXEDTRANSLATIONS$", search2, ret);
				SaveCache(ret);
			}



			SAFE_DELETE(cache);
			if (search1)
				search1->Close();
			if (search2)
				search2->Close();

		} catch (...) {
			if (search1)
				search1->Close();
			if (search2)
				search2->Close();
			SAFE_DELETE(cache);
			throw;
		}

		return ret;
	}

	





	void CTranslation::CountFiles(CFSSearchData *search, int &count) {
		if (search) {
			
			string file;
			int attr;
			
			for (int f=0; f< search->count(); f++) {
				search->GetField(f, file, attr);
				if (attr == 1 && file.size() > 5 && _stricmp(file.c_str() + file.size() - 5, ".tran") == 0)
					count++;
			}
		}
	}







	void CTranslation::Initialize() {
		_map.clear();
		SAFE_DELETE(_files);
		_files = new FilesT();
	}






	void CTranslation::LoadFiles(const string &path, CFSSearchData *search, bool &ret) {
		if (search) {
			
			string file;
			int attr;
			
			for (int f=0; f< search->count(); f++) {
				search->GetField(f, file, attr);
				if (attr == 1 && file.size() > 5 && _stricmp(file.c_str() + file.size() - 5, ".tran") == 0) {
					file = path + '\\' + file;
					LoadFile(file, ret);
				}
			}
		}
	}







	void CTranslation::LoadFile(const string &path, bool &ret) {
		if (!AddTranslationFile(path.c_str())) {
			ret = false; return;
		}

		TextResourceReader ^file = TextResourceReader::ParseFile(gcnew String(path.c_str()));
		if (file == nullptr) {
			ret = false; return;
		}
		if (file->ErrorLog->ErrorCount) {
			ret = false;
		}

		for each(TextResource ^resource in file->TextResources) {
			if (_stricmp(_language, CStringToCharPtr(resource->Laguage)) == 0) {
				string ksid = CStringToCharPtr(resource->Identifier->ToKsidString());
				MapT::iterator i = _map.find(ksid);
				if (i == _map.end() || resource->UIntTime > i->second.date) {
					STrnField item;
					if (resource->Comment != nullptr)
						item.Comment = CStringToWCharPtr(resource->Comment);
					if (resource->UserName != nullptr)
						item.UserName = CStringToWCharPtr(resource->UserName);
					item.date = resource->UIntTime;
					_map[ksid] = item;
				}
			}
		}
	}







	void CTranslation::LoadFromCache(CFSRegister *cache, bool &ret) {
		CFSRegKey *nsk = cache->FindKey("Names");
		if (!nsk) {
			ret = false; return;
		}

		for (CFSRegKey *nk = nsk->GetSubRegister()->GetFirstKey(); nk; nk = nk->GetNextKey()) {
			STrnField field;
			CFSRegKey *key;
			CFSRegister *name = nk->GetSubRegister();
			
			if (key = name->FindKey("UN")) {
				field.UserName = key->GetWStringAccessFromPos();
			}
			if (key = name->FindKey("Co")) {
				field.Comment = key->GetWStringAccessFromPos();
			}
			if (key = name->FindKey("T")) {
				field.date = key->readi();
			}

			_map[nk->GetName()] = field;
		}
	}





	void CTranslation::SaveCache(bool &ret) {
		CFSRegister *reg = OpenCache(true);

		CFSRegKey *fileName = reg->AddKey("Source Files", FSRTstring);
		CFSRegKey *fileDate = reg->AddKey("Source Files Dates", FSRTint);
		for (FilesT::iterator i = _files->begin(); i != _files->end(); ++i) {
			fileName->stringwrite(i->first.c_str());
			fileDate->writei(i->second.dwLowDateTime);
			fileDate->writei(i->second.dwHighDateTime);
		}


		CFSRegister *names = reg->AddKey("Names", FSRTregister)->GetSubRegister();
		for (MapT::iterator i = _map.begin(); i != _map.end(); ++i) {
			SaveField(names, i->first.c_str(), i->second);
		}

		if (!reg->WriteFile())
			ret = false;

		delete reg;
	}




	void CTranslation::SaveField(CFSRegister *names, const char *ksid, const STrnField &field) {
		CFSRegister *name = names->AddKey(ksid, FSRTregister)->GetSubRegister();
		if (field.UserName.size())
			name->AddKey("UN", FSRTwString)->wstringwrite(field.UserName.c_str());
		if (field.Comment.size())
			name->AddKey("Co", FSRTwString)->wstringwrite(field.Comment.c_str());
		name->AddKey("T", FSRTint)->writei(field.date);
	}





	CFSRegister *CTranslation::ReadFiles() {
		SAFE_DELETE(_files);
		CFSRegister *reg = OpenCache(false);
		if (!reg)
			return 0;
		CFSRegKey *files = reg->FindKey("Source Files");
		CFSRegKey *dates = reg->FindKey("Source Files Dates");
		if (!files || !dates) 
			return reg;

		_files = new FilesT();
		while (!files->eof()) {
			FileT file;
			file.second.dwLowDateTime = dates->readi();
			file.second.dwHighDateTime = dates->readi();
			file.first = files->GetDirectAccessFromPos();
			files->SetPosToNextString();
			_files->push_back(file);
		}
		return reg;
	}





	CFSRegister *CTranslation::OpenCache(bool create) {
		string path("$TRANSLATIONS$\\");
		path.append(_language);
		path.append("\\TranslationsCache");
		CFSRegister *ret = new CFSRegister(path.c_str(), "KRKAL3 TRANSLATIONS CACHE", create);
		
		if (!create) {
			if (ret->GetOpenError() != FSREGOK || !ret->CheckVersionFeatures()) {
				SAFE_DELETE(ret);
			}
		} else {
			char buff[21];
			CFS::GetFS()->GenerateVersionNumber(buff);
			ret->AddKey("Unique Compilation Id", FSRTstring)->stringwrite(buff);
			ret->WriteVersionFeatures();
		}

		return ret;
	}






	bool CTranslation::AddTranslationFile(const char *file) {
		char *file2=0;
		FileT info;
		CFS *fs = CFS::GetFS();

		if (!fs->GetFullPath(file, &file2, efptInvariantKey)) {
			return false;
		} else {
			info.first = file2;
			SAFE_DELETE_ARRAY(file2);
		}

		if (!fs->GetTime(info.first.c_str(), info.second)) {
			return false;
		}
			
		_files->push_back(info);
		return true;
	}









	////////////////////////////////////////////////////////////////////////////////////////



	CResourceManager::CResourceManager() {
		InitializeCriticalSection(&_criticalSection);
		_primary = 0; _secondary = 0;
		CFS::InitFS();
		_fs = CFS::GetFS();
	}



	CResourceManager::~CResourceManager() {
		SAFE_DELETE(_primary);
		SAFE_DELETE(_secondary);
		DeleteCriticalSection(&_criticalSection);
		_fs = 0;
		CFS::DoneFS();
	}



	bool CResourceManager::Load(const char *primaryLanguage, const char *secondaryLanguage) {
		MyCriticalSection cs(&_criticalSection);
		CTranslation *primary = 0;
		CTranslation *secondary = 0;
		bool dontDeletePrimary = false;
		bool dontDeleteSecondary = false;

		if (primaryLanguage) {
			if (_primary && _stricmp(_primary->GetLanguage(), primaryLanguage) == 0) {
				primary = _primary;
				dontDeletePrimary = true;
			} else if (_secondary && _stricmp(_secondary->GetLanguage(), primaryLanguage) == 0) {
				primary = _secondary;
				dontDeleteSecondary = true;
			} else {
				primary = new CTranslation(primaryLanguage);
			}
		}

		if (secondaryLanguage && (!primaryLanguage || _stricmp(secondaryLanguage, primaryLanguage) != 0)) {
			if (_primary && _stricmp(_primary->GetLanguage(), secondaryLanguage) == 0) {
				secondary = _primary;
				dontDeletePrimary = true;
			} else if (_secondary && _stricmp(_secondary->GetLanguage(), secondaryLanguage) == 0) {
				secondary = _secondary;
				dontDeleteSecondary = true;
			} else {
				secondary = new CTranslation(secondaryLanguage);
			}
		}

		if (!dontDeletePrimary)
			SAFE_DELETE(_primary);
		if (!dontDeleteSecondary)
			SAFE_DELETE(_secondary);
		_primary = primary;
		_secondary = secondary;

		return ReloadIfNeeded();

	}



	const wchar_t *CResourceManager::GetUserNameOrComment(const char* ksid, bool isComment) {
		MyCriticalSection cs(&_criticalSection);
		if (!ksid)
			return 0;
		
		const wchar_t *ret = 0;
		
		if (_primary)
			ret = _primary->GetUserNameOrComment(ksid, isComment);

		if (!ret && _secondary)
			ret = _secondary->GetUserNameOrComment(ksid, isComment);

		return ret;
	}



	// input "_KSId_....:default text" - both parts are optional
	// if it finds the translation for ksid, it return it
	// otherwise the part after the : is returned (if there is no such default text, empty string is returned)
	// if ksid part is missing whole input text is returned
	// function returns null only if the input text is null
	const wchar_t *CResourceManager::GetText(const wchar_t* text) {
		if (!text)
			return 0;
		if (text[0] == L'_' && text[1] == L'K' && text[2] == L'S' && text[3] == L'I' && text[4] && text[5] == L'_') {
			const wchar_t *pos = text+6;
			for (; *pos != ':' && *pos; pos++) ;
			char *buffer = new char[pos - text + 1];
			try {
				pos = text;
				int f=0;
				for ( ; *pos != ':' && *pos; pos++, f++) {
					buffer[f] = (char)*pos; // simple conversion because ksid can be only from ascii characters
				}
				buffer[f] = 0;

				const wchar_t *ret = GetUserNameOrComment(buffer, true);
				SAFE_DELETE_ARRAY(buffer);

				if (ret)
					return ret;
			} catch (...) {
				SAFE_DELETE_ARRAY(buffer);
				throw;
			}

			if (*pos == ':')
				pos++;
			return pos;
		} else {
			return text;
		}
	}




	bool CResourceManager::ReloadIfNeeded() {
		MyCriticalSection cs(&_criticalSection);
		bool ret = true;
		if (_primary)
			if (!_primary->ReloadIfNeeded()) ret = false;
		if (_secondary)
			if (!_secondary->ReloadIfNeeded()) ret = false;
		return ret;
	}



}
}