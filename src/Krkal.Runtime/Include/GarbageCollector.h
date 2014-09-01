//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - G a r b a g e C o l l e c t o r
///
///		Garbage collector and base class for managed objects
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __GARBAGE_COLLECTOR_H__
#define __GARBAGE_COLLECTOR_H__

#include "Krkal.Runtime.h"
#include <list>
#include <hash_map>
#include <stack>

using namespace std;
using namespace stdext;


class CManagedObject;
class CGarbageCollector;
class CKerMain;


extern /*__declspec( thread )*/ CKerMain *StaticKerMain;
KRKALRUNTIME_API CGarbageCollector *GetGCFromStaticKerMain();
KRKALRUNTIME_API CKerMain *GetStaticKerMain();


#define GC_OBJECT_TRESHOLD 100
#define GC_TIME_TRESHOLD 30000
#define GC_MINTIME_TRESHOLD 1000


class KRKALRUNTIME_API IGarbageCollector {
public:
	virtual void Hold(CManagedObject *obj) = 0;
	virtual void Release(CManagedObject *obj) = 0;
};

class CGarbageCollector : public IGarbageCollector{
friend CKerMain;
friend class CKerSaver;
	typedef list<CManagedObject*> listT;
	typedef hash_map<CManagedObject*, int> mapT;
	typedef stack<CManagedObject*> stackT;

public:
	virtual void Hold(CManagedObject *obj);
	virtual void Release(CManagedObject *obj);

	void Add(CManagedObject *obj);
	void AddToQueue(CManagedObject *obj);

private:
	CGarbageCollector(CKerMain *kerMain) { currentMark = 0; KerMain = kerMain; lastCollectObjectCount = 0; lastCollectTime=0;}
	void Collect();
	void CollectIfNecessary();
	~CGarbageCollector();

	listT managedObjects;
	mapT heldObjects;
	stackT searchStack;
	int currentMark;
	CKerMain *KerMain;
	int lastCollectObjectCount;
	int lastCollectTime;
};






class KRKALRUNTIME_API CManagedObject {
friend CGarbageCollector;
public:
	CManagedObject(CGarbageCollector & collector) : _mark(0)
	{
		collector.Add(this);
	}
	void MarkMe(int mark, CGarbageCollector & collector) {
		if (_mark != mark) {
			_mark = mark;
			collector.AddToQueue(this);
		}
	}
	// This method has to call MarkMe on all nested objects
	virtual void MarkLinks(int mark, CGarbageCollector & collector) = 0; 
	int GetMark() { return _mark;}
private:
	int _mark;
protected:
	virtual ~CManagedObject() {};
};




#endif