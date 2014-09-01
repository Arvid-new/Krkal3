#pragma once

#include "CKerMain.h"

#pragma unmanaged

class CKerObjectEx;

class CMapInfo {
public:
	CMapInfo() { Image=0; x=0; y=0; z=0; next=0;}
	~CMapInfo() { SAFE_DELETE_ARRAY(Image); }
	void Clear() { SAFE_DELETE_ARRAY(Image); x=0; y=0; z=0; next=0; }


	wchar_t *Image;
	int x,y,z;

	CKerObjectEx *next;
};


class CKerObjectEx : public CKerObject {
public:
	CKerObjectEx(CKerName *type, CKerMain *KerMain) : CKerObject(type, KerMain) {
		MapInfo = 0;
	}
	CKerObjectEx(CKerObject *source, CKerMain *KerMain) : CKerObject(source, KerMain) {
		MapInfo = 0;
	}
	virtual ~CKerObjectEx() {
		SAFE_DELETE(MapInfo);
	}

	CMapInfo *MapInfo;
};

#pragma managed
