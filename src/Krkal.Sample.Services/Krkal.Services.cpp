#include "stdafx.h"
#include "Krkal.Services.h"
#include "Krkal.Services.Net.h"
#include <wchar.h>


#pragma unmanaged

void CServices::SetMapSize(int x, int y) {
	int f,g;
	CKerObjectEx **newMap = new CKerObjectEx*[x*y];
	for (g=0; g < y; g++) for (f=0; f < x; f++) {
		newMap[g*x + f] = 0;
	}

	if (Map) {
		for (g=0; g < mapSY; g++) for (f=0; f < mapSX; f++) {
			if (f >= x || g >= y) {
				CKerObjectEx *obj = Map[g*mapSX + f];
				CKerObjectEx *obj2;
				while (obj) {
					obj2 = obj->MapInfo->next;
					RemoveFromMap2(obj);
					obj = obj2;
				}
			} else {
				newMap[g*x + f] = Map[g*mapSX + f];
			}
		}
		delete[] Map;
	}

	Map = newMap;
	mapSX = x;
	mapSY = y;
}




void CServices::RemoveFromMap2(CKerObjectEx *obj) {
	SAFE_DELETE(obj->MapInfo);
	KerMain->GetIGarbageCollector()->Release(obj);
	KerMain->call(0, obj->thisO, MapRemoved, 0);
}




void CServices::RemoveFromList(CKerObjectEx *obj) {
	CKerObjectEx **o = Map + (obj->MapInfo->y * mapSX + obj->MapInfo->x);
	while (*o != obj) {
		o = &(**o).MapInfo->next;
	}

	*o = obj->MapInfo->next;
	Invalidate(obj->MapInfo->x, obj->MapInfo->y);
}


bool CServices::PlaceToMap(OPointer obj) {
	if (!obj)
		return false;

	int fromX, fromY;
	bool moving;

	CKerObjectEx *o = (CKerObjectEx*)obj.KerObject();
	if (!o->MapInfo) {
		o->MapInfo = new CMapInfo();
		KerMain->GetIGarbageCollector()->Hold(o);
		moving = false;
	} else {
		RemoveFromList(o);
		moving = true;
		fromX = o->MapInfo->x;
		fromY = o->MapInfo->y;
		o->MapInfo->Clear();
	}

	CKerOVar *v;
	obj = o->thisO;
	
	obj.TryRead(ObjPosX, o->MapInfo->x);
	obj.TryRead(ObjPosY, o->MapInfo->y);

	if (o->MapInfo->x >= mapSX || o->MapInfo->y >= mapSY || o->MapInfo->x < 0 || o->MapInfo->y < 0) {
		SAFE_DELETE(o->MapInfo);
		KerMain->GetIGarbageCollector()->Release(o);
		KerMain->Errors->LogError(eKRTEmovingOutOfMap);
		return false;
	}

	obj.TryRead(ObjPosZ, o->MapInfo->z);

	v = o->Type->GetVariable(Image);
	if (v) {
		wchar_t* tmp = obj.get<KString>(v->Offset)->c_str();
		size_t size = wcslen(tmp) + 1;
		o->MapInfo->Image = new wchar_t[size];
		wmemcpy(o->MapInfo->Image, tmp, size);
	}


	CKerObjectEx **o1 = Map + (o->MapInfo->y * mapSX + o->MapInfo->x);
	while (*o1 != 0 && (**o1).MapInfo->z < o->MapInfo->z) {
		o1 = &(**o1).MapInfo->next;
	}

	o->MapInfo->next = *o1;
	*o1 = o;
	Invalidate(o->MapInfo->x, o->MapInfo->y);

	if (moving) {
		KerMain->message(0, obj, MapMoved, eKerCTmsg, 0, 0, 2, CLT(eKTint), CLT(eKTint), MapMoved_X, MapMoved_Y, fromX, fromY);
	} else {
		KerMain->message(0, obj, MapPlaced, eKerCTmsg, 0, 0, 0);
	}
	return true;
}



void CServices::RemoveFromMap(OPointer obj) {
	if (!obj)
		return;

	CKerObjectEx *o = (CKerObjectEx*)obj.KerObject();
	if (!o->MapInfo)
		return;

	RemoveFromList(o);
	RemoveFromMap2(o);

}


bool CServices::IsPlaced(OPointer obj) {
	if (!obj)
		return false;

	CKerObjectEx *o = (CKerObjectEx*)obj.KerObject();
	return (o->MapInfo != 0);
}



void CServices::GetReadyToStart(CKerMain *kerMain) {
	MapPlaced = KerMain->KerNamesMain->GetNamePointer("_KSID_MapPlaced");
	MapRemoved = KerMain->KerNamesMain->GetNamePointer("_KSID_MapRemoved");
	MapMoved = KerMain->KerNamesMain->GetNamePointer("_KSID_MapMoved");
	MapMoved_X = KerMain->KerNamesMain->GetNamePointer("_KSID_MapMoved__M_fromX");
	MapMoved_Y = KerMain->KerNamesMain->GetNamePointer("_KSID_MapMoved__M_fromY");
	ObjPosX = KerMain->KerNamesMain->GetNamePointer("_KSID_Placeable__M_X");
	ObjPosY = KerMain->KerNamesMain->GetNamePointer("_KSID_Placeable__M_Y");
	ObjPosZ = KerMain->KerNamesMain->GetNamePointer("_KSID_Placeable__M_Z");
	Image = KerMain->KerNamesMain->GetNamePointer("_KSID_Placeable__M_Image");
	Placeable = KerMain->KerNamesMain->GetNamePointer("_KSID_Placeable");
}



ArrPtr<OPointer> CServices::GetObjects(int x, int y, int dx, int dy, CKerName *type) {
	KerMain->ResetTLSKerMain();
	ArrPtr<OPointer> arr;

	for (int g=y; g<y+dy; g++) for (int f=x; f<x+dx; f++) {
		for (CKerObjectEx *ex = GetFirstObj(f, g); ex; ex = ex->MapInfo->next) {
			if (!type || OPointer(ex->thisO) <= type)
				arr->AddLast(OPointer(ex->thisO).Cast(Placeable));
		}
	}

	return arr;
}


int CServices::GetObjectsCount(int x, int y, int dx, int dy, CKerName *type) {
	int ret = 0;

	for (int g=y; g<y+dy; g++) for (int f=x; f<x+dx; f++) {
		for (CKerObjectEx *ex = GetFirstObj(f, g); ex; ex = ex->MapInfo->next) {
			if (!type || OPointer(ex->thisO) <= type)
				ret++;
		}
	}

	return ret;
}




bool CServices::IsCollision(OPointer obj, int x, int y) {
	if (x<0 || y<0 || x >= mapSX || y >= mapSY)
		return true;

	int z=0;
	obj.TryRead(ObjPosZ, z);

	for (CKerObjectEx *ex = GetFirstObj(x, y); ex; ex = ex->MapInfo->next) {
		if (ex->MapInfo->z == z && OPointer(ex->thisO) != obj)
			return true;
	}

	return false;
}


bool CServices::PlaceIfNoCollision(CKerName *objType, int x, int y) {
	OPointer obj = KerMain->NewObject(0, objType);

	if (IsCollision(obj, x, y))
		return false;

	obj.TryWrite(ObjPosX, x);
	obj.TryWrite(ObjPosY, y);

	return PlaceToMap(obj);
}




CKerObject *ObjectConstructor(CKerName *type, CKerMain *KerMain) {
	return new CKerObjectEx(type, KerMain);
}
CKerObject *ObjectCopyConstructor(CKerObject *source, CKerMain *KerMain) {
	return new CKerObjectEx(source, KerMain);
}
void KillHandler(CKerObject *obj, CKerMain *KerMain) {
	CServices *services = (CServices*)KerMain->KernelParameters->Services;
	services->RemoveFromMap(obj->thisO);
}


#pragma managed


namespace Krkal { namespace Services {


	SampleServices::SampleServices(KerMain ^kerMain) {
		_oldSizeX=-1; _oldSizeY=-1;
		CKerMain *kernel = (CKerMain *)kerMain->GetKerMain2().ToPointer();
		_services = new CServices(kernel);
		kernel->KernelParameters->Services = _services;
		kernel->KernelParameters->ObjectConstructor = ObjectConstructor;
		kernel->KernelParameters->ObjectCopyConstructor = ObjectCopyConstructor;
		kernel->KernelParameters->KillHandler = KillHandler;
	}

}}