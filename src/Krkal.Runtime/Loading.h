//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - L o a d i n g
///
///		Loading of scripts, levels, games and data files
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __LOADING_H__
#define __LOADING_H__

#include "LanguageType.h"
#include "OPointer.h"


class CKerMain;
class CFSRegister;
class CFSRegKey;
class CKerName;
class CKerObjectT;
struct CKerField;
struct CKerMethod;
class CAttributes;
class CDataObject;
class CDataObjectBase;
union UTempVar;
class CKerObject;
class CDataObjectLoader;
class CKernelParameters;
class CFS;


using namespace std;
using namespace stdext;

enum eKerObjectLoadType {
	eKOLTconstants,
	eKOLTlevel,
	eKOLTgame,
};

class CScriptLoader {
public:
	enum eMode {
		eLMscript,
		eLMdata,
		eLMlevel,
	};

	CScriptLoader(CKerMain *kerMain, const char *codePath, eMode mode, CFSRegister *code = 0) {
		KerMain = kerMain;
		_codePath = codePath;
		_mode = mode;
		_code = code;
		_dontDeleteCode = _code != 0;
		_areNamesOrdinalsCalculated = false;
		_dataObjectLoader = 0;
	}
	~CScriptLoader();

	void Load();
	void CreateEmptyObjects();
	void LoadConstants();
	void LoadObjects(eKerObjectLoadType objectLoadType);
	void RunConstructors(eKerObjectLoadType objectLoadType);

private:
	typedef pair<CFSRegister*, CKerObject*> RegObjT;
	typedef vector<RegObjT> ObjectsT;
	typedef unordered_map<CKerName*, int> NamesOrdinalsT;
	typedef unordered_map<string, CKerField*> FieldsCacheT;

	CKerMain *KerMain;
	const char *_codePath;
	CFSRegister *_code;
	bool _dontDeleteCode;
	CDataObjectLoader *_dataObjectLoader;
	vector<CKerName*> ksidNames;
	NamesOrdinalsT _namesOrdinlas;
	bool _areNamesOrdinalsCalculated;
	vector<CLT> _originalKsidLts;
	vector<pair<CFSRegKey*, void*> > _arrays;
	ObjectsT _objects;
	eMode _mode;
	int _nameStart;
	int _arrayStart;
	int _objectStart;
	FieldsCacheT _fieldCache;

	CFSRegister *GetSubRegister(CFSRegister* reg, const char *name);
	CFSRegister *GetSubRegister(CFSRegKey* key);
	CFSRegister *TryGetSubRegister(CFSRegister* reg, const char *name);
	int ReadInt(CFSRegister* reg, const char *name);
	char ReadChar(CFSRegister* reg, const char *name);
	CLT ReadLT(CFSRegister* reg);
	CLT ReadLT(CFSRegKey* key);
	CKerName* ReadKsid(CFSRegister* reg, const char *name = "Ksid");
	CKerName* GetName(int index);
	OPointer GetObject(int index);
	void LoadNames(CFSRegister *names);
	void LoadDependencies();
	void AssignLTs(CFSRegister *names);
	void LoadAttributesOfNames(CFSRegister *names);
	void LoadClasses(CFSRegister *names);
	void LoadDataObjects(CFSRegister *names);
	void LoadMethods(CFSRegister *reg, CKerObjectT *ot);
	void LoadVariables(CFSRegister *reg, CKerObjectT *ot);
	void LoadStaticVariables(CFSRegister *reg, CKerObjectT *ot);
	void LoadParams(CFSRegister *reg, CKerMethod *m);
	void LoadAttributes(CFSRegister *reg, CAttributes *&attributes);
	void ReadDataValues(CFSRegister *reg, CDataObjectBase *dataObj);
	void ReadVar(UTempVar *dest, CLT &dType, CFSRegKey *source, CLT &sType);
	void ReadObjVar(UTempVar *dest, CLT &dType, CFSRegKey *source, CLT &sType);
	void *LoadArray(int ordinal, CLT lt, bool isDataObject);
	void *CreatePtrArray(CFSRegKey *key, CLT lt, bool isDataObject);
	template <typename T>
	void *CreateArray(CFSRegKey *key);
	CFSRegister * LoadField(CFSRegKey *key, CKerField *currentMethod);
	void LoadOrdinalStarts();
	void PrepareArrays();
	void LoadObject(CKerObject *obj, CFSRegister *data, eKerObjectLoadType loadType);
	int GetNameOrdinal(CKerName *name);
	OPointer LoadDataObject(CKerName *name);
	template <typename T>
	T *TestFieldCache(CFSRegKey *key, T *currentField);
};




class CKernelLoader {
public:

	CKernelLoader(CKerMain *kerMain) { 
		KerMain = kerMain;
		_level = 0; _levelLoader=0;
		_objectLoadType = eKOLTlevel;
	}
	void Load(); 
	~CKernelLoader();
	static eKerFileType GetFileType(const char *file);
	void LoadDataFiles();
	void LoadLevelNamesAndDataObjects();
	void CreateEmptyObjects() { if (_levelLoader) _levelLoader->CreateEmptyObjects();  }
	void LoadObjects() { if (_levelLoader) _levelLoader->LoadObjects(_objectLoadType);  }
	void RunConstructors() { if (_levelLoader) _levelLoader->RunConstructors(_objectLoadType);  }
	bool ExistsLevel() { return _levelLoader != 0;}
	static bool InitSourceFiles(CKernelParameters *params, CFS *fs);
	void MoveLevelToDataSources();

private:
	CKerMain *KerMain;
	CFSRegister *_level;
	string _levelFileToMove;
	CScriptLoader *_levelLoader;
	eKerObjectLoadType _objectLoadType;

	static const int MaxExtensions = 7;
	static char *FileExtensions[MaxExtensions];

	CFSRegKey *TryGetKey(CFSRegister *reg, const char *name, int type);
	bool TryGetFileAttribute(const char* inFile, string &outFile);
	void InitSourceFiles();
};






class CDataObjectLoader {
public:
	CDataObjectLoader(CKerMain *kerMain) { KerMain = kerMain; }
	OPointer Load(CKerName *name);
	void RunConstructors();

	CKerMain *KerMain;
private:
	vector<OPointer> _objs;
};


#endif