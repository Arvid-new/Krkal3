//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - O P o i n t e r
///
///		Pointer to Krkal object
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __OPOINTER_H__
#define __OPOINTER_H__

#include "names.h"
#include "GarbageCollector.h"
#include "objects.h"
#include "KerErrorNumbers.h"


class CKerObjectT;
class CKerObject;


class KRKALRUNTIME_API OPointer
{
public:

	OPointer() {
		ptr = 0;
	}

	OPointer( const OPointer& opointer )
		: ptr(opointer.ptr)
	{
	}

	OPointer( void ** a )
		: ptr(a)
	{
	}

	OPointer(int null) {
		ptr = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
	}




	OPointer& operator=(const OPointer& opointer)
	{
		ptr = opointer.ptr;
		return *this;
	}


	OPointer& operator=(void ** a)
	{
		ptr = a;
		return *this;
	}

	OPointer& operator=(int null)
	{
		ptr = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return *this;
	}


	operator void **() const {
		return ptr;
	}






	bool operator == (const OPointer& opointer) const {
		if (ptr == 0 || opointer.ptr == 0)
			return (ptr == opointer.ptr);
		return (*ptr == *opointer.ptr);
	}

	bool operator == (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (ptr == 0);
	}

	bool operator == (CKerName *type) const {
		return Type() == type;
	}

	bool operator != (const OPointer& opointer) const {
		if (ptr == 0 || opointer.ptr == 0)
			return (ptr != opointer.ptr);
		return (*ptr != *opointer.ptr);
	}

	bool operator != (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (ptr != 0);
	}

	bool operator != (CKerName *type) const {
		return Type() != type;
	}


	bool operator < (CKerName *type) const {
		if (type==0 || Type()==0)
			return false;
		return (Type()->Compare(type) == 2);
	}

	bool operator > (CKerName *type) const {
		if (type==0 || Type()==0)
			return false;
		return (Type()->Compare(type) == 1);
	}


	bool operator <= (CKerName *type) const {
		if (Type() == type)
			return true;
		if (type==0 || Type()==0)
			return false;
		return (Type()->Compare(type) == 2);
	}

	bool operator >= (CKerName *type) const {
		if (Type() == type)
			return true;
		if (type==0 || Type()==0)
			return false;
		return (Type()->Compare(type) == 1);
	}




	OPointer Cast(int offset) const {
		return (void**)( (size_t)(ptr + offset) & (!(size_t)ptr - 1) );
	}

	OPointer Cast(CKerName* type) const {
		if (ptr != 0) {
			int offset = KerObjectT()->GetBaseObjOffset(type);
			if (offset == -1) {
				return 0;
			} else {
				return (void**)(*ptr) + offset;
			}
		} else {
			return 0;
		}
	}


	CKerName* Type() const {
		if (ptr != 0) {
			return ((CKerName**)(*ptr))[-1];
		} else {
			return 0;
		}
	}


	CKerObjectT* KerObjectT() const {
		if (ptr == 0) 
			throw CKernelError(eKRTEaccessingNullObject);
		return ((CKerObjectT**)(*ptr))[-2];
	}


	CKerObject* KerObject() const {
		if (ptr == 0) 
			throw CKernelError(eKRTEaccessingNullObject);
		return ((CKerObject**)(*ptr))[-3];
	}


	void GCMarkMe(int mark, CGarbageCollector & collector) const {
		if (ptr != 0) 
			KerObject()->MarkMe(mark, collector);
	}



	template<typename T>
	T & get (int offset) const {
		if (ptr == 0) 
			throw CKernelError(eKRTEaccessingNullObject);
		return *((T**)(ptr))[offset];
	}

	template<typename T>
	bool TryRead(CKerName *varName, T &output) const {
		if (ptr == 0)
			return false;
		CKerOVar *v = KerObjectT()->GetVariable(varName);
		if (!v)
			return false;
		output = *(T*)((BYTE*)(*ptr) + v->DataOffset);
		return true;
	}

	template<typename T>
	bool TryWrite(CKerName *varName, T &input) const {
		if (ptr == 0)
			return false;
		CKerOVar *v = KerObjectT()->GetVariable(varName);
		if (!v)
			return false;
		*(T*)((BYTE*)(*ptr) + v->DataOffset) = input;
		return true;
	}



	bool Lives() const {
		if (ptr==0)
			return false;
		return KerObject()->lives;
	}


	OPointer Clone(int codeLine, CKerMain *KerMain) const;
	void Kill(int codeLine, CKerMain *KerMain) const;
	int Compare(const OPointer& other) const;


private:
	void **ptr;
};




inline bool operator == (int null, const OPointer &obj) {
	return obj == null;
}

inline bool operator != (int null, const OPointer &obj) {
	return obj != null;
}




#endif