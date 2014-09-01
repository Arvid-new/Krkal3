//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - G a r b a g e C o l l e c t o r
///
///		Garbage collector and base class for managed objects
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "GarbageCollector.h"
#include "CKerMain.h"



void CGarbageCollector::Hold(CManagedObject *obj) {
	mapT::iterator i = heldObjects.find(obj);
	if (i == heldObjects.end()) {
		heldObjects[obj] = 1;
	} else {
		++i->second;
	}
}


void CGarbageCollector::Release(CManagedObject *obj) {
	mapT::iterator i = heldObjects.find(obj);
	if (i != heldObjects.end()) {
		--i->second;
		if (i->second <= 0) {
			heldObjects.erase(i);
		}
	} 
}



void CGarbageCollector::Add(CManagedObject *obj) { 
	managedObjects.push_back(obj); 
}
void CGarbageCollector::AddToQueue(CManagedObject *obj) { 
	searchStack.push(obj); 
}



void CGarbageCollector::Collect() {
	currentMark++;
	
	for (mapT::iterator i = heldObjects.begin(); i != heldObjects.end(); ++i) {
		i->first->MarkMe(currentMark, *this);
	}

	while (!searchStack.empty()) {
		CManagedObject *obj = searchStack.top();
		searchStack.pop();
		obj->MarkLinks(currentMark, *this);
	}


	int currentCount = managedObjects.size();
	for (listT::iterator i = managedObjects.begin(); i != managedObjects.end(); ) {
		CManagedObject *obj = *i;
		if (obj->GetMark() != currentMark) {
			delete obj;
			i = managedObjects.erase(i);
		} else {
			++i;
		}
	}

	KerMain->RunGarbageCollector = false;
	lastCollectObjectCount = managedObjects.size();
	lastCollectTime = KerMain->GetTime();

	char buff[128];
	sprintf_s(buff, sizeof(buff), "From %i total objects (%i held) was %i freed.", currentCount, heldObjects.size(), currentCount-lastCollectObjectCount);
	KerMain->Errors->LogError(eKRTEgarbageCollector, 0, buff);
}



void CGarbageCollector::CollectIfNecessary() {
	if (KerMain->RunGarbageCollector || 
		KerMain->GetTime() > lastCollectTime + GC_TIME_TRESHOLD || 
		((int)managedObjects.size() > lastCollectObjectCount + GC_OBJECT_TRESHOLD && KerMain->GetTime() > lastCollectTime + GC_MINTIME_TRESHOLD)
	)
		Collect();
}



CGarbageCollector::~CGarbageCollector() {
	for (listT::iterator i = managedObjects.begin(); i != managedObjects.end(); ++i) {
		CManagedObject *obj = *i;
		delete obj;
	}
}








KRKALRUNTIME_API CGarbageCollector *GetGCFromStaticKerMain() {
	return StaticKerMain->GarbageCollector;
}


KRKALRUNTIME_API CKerMain *GetStaticKerMain() {
	return StaticKerMain;
}
