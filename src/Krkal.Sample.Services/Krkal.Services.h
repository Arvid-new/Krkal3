#pragma once

#ifndef __KRKAL_SERVICES_H__
#define __KRKAL_SERVICES_H__

#include "CKerMain.h"




#ifdef KRKALSERVICES_EXPORT
#define KRKALSERVICES_API __declspec(dllexport)
#else
#define KRKALSERVICES_API __declspec(dllimport)
#endif

#ifdef _MANAGED
#pragma managed(push, off)
#endif

class CKerObjectEx;

class KRKALSERVICES_API CServices  : public IServices {
public:
	CServices(CKerMain *kerMain) {
		mapSX = 0;
		mapSY = 0;
		KerMain = kerMain;
		Map = 0;
		MapPlaced=0; MapRemoved=0;
		MapMoved=0; MapMoved_X=0; MapMoved_Y=0;
		ObjPosX=0; ObjPosY=0; ObjPosZ=0;
		Image=0;
		iaX=0; iaY=0; iaDX=0; iaDY=0;
	}

	virtual void GetReadyToStart(CKerMain *kerMain);

	~CServices() {
		SAFE_DELETE_ARRAY(Map);
	}

	bool PlaceToMap(OPointer obj);
	void RemoveFromMap(OPointer obj);
	void SetMapSize(int x, int y);
	ArrPtr<OPointer> GetObjects(int x, int y, int dx, int dy, CKerName *type);
	int GetObjectsCount(int x, int y, int dx, int dy, CKerName *type);
	bool IsCollision(OPointer obj, int x, int y);
	bool IsPlaced(OPointer obj);

	CKerName *MapPlaced, *MapRemoved, *ObjPosX, *ObjPosY, *ObjPosZ, *Image, *Placeable, *MapMoved, *MapMoved_X, *MapMoved_Y;

	CKerObjectEx *GetFirstObj(int x, int y) {
		if (x>=0 && y>=0 && x<mapSX && y<mapSY) {
			return Map[y*mapSX + x];
		} else {
			return 0;
		}
	}
	void GetInvalidArea(int &x, int &y, int &dx, int &dy) {
		x=iaX; y=iaY; dx=iaDX; dy=iaDY;
		iaX=0; iaY=0; iaDX=0; iaDY=0;
	}
	bool PlaceIfNoCollision(CKerName *objType, int x, int y);

	int mapSX, mapSY;

private:
	void RemoveFromMap2(CKerObjectEx *obj);
	void RemoveFromList(CKerObjectEx *obj);
	void Invalidate(int x, int y) {
		if (iaDX == 0) {
			iaX=x; iaY=y; iaDX=1; iaDY=1;
		} else {
			if (x < iaX) {
				iaDX += iaX-x;
				iaX=x;
			} else if (x >= iaX+iaDX) {
				iaDX += x-iaX-iaDX+1;
			}
			if (y < iaY) {
				iaDY += iaY-y;
				iaY=y;
			} else if (y >= iaY+iaDY) {
				iaDY += y-iaY-iaDY+1;
			}
		}
	}

	CKerObjectEx **Map;
	CKerMain *KerMain;
	int iaX, iaY, iaDX, iaDY;
};

#ifdef _MANAGED
#pragma managed(pop)
#endif

#endif

