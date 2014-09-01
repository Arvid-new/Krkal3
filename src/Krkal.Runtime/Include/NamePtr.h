//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - N a m e P t r
///
///		Pointer to KSID name
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __NAME_PTR_H__
#define __NAME_PTR_H__

#include "OPointer.h"

class KRKALRUNTIME_API NamePtr
{
public:

	NamePtr() {
		_name = 0;
	}

	NamePtr( const NamePtr& namePtr )
		: _name(namePtr._name)
	{
	}

	NamePtr( CKerName * a )
		: _name(a)
	{
	}

	NamePtr(int null) {
		_name = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
	}




	NamePtr& operator=(const NamePtr& namePtr)
	{
		_name = namePtr._name;
		return *this;
	}


	NamePtr& operator=(CKerName * a)
	{
		_name = a;
		return *this;
	}

	NamePtr& operator=(int null)
	{
		_name = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return *this;
	}


	operator CKerName *() const {
		return _name;
	}





	CKerName & operator *() const {
		if (_name == 0)
			throw CKernelError(eKRTEaccessingNullObject);
		return *_name;
	}

	CKerName* operator -> () const {
		if (_name == 0)
			throw CKernelError(eKRTEaccessingNullObject);
		return _name;
	}





	bool operator == (const NamePtr& namePtr) const {
		return (_name == namePtr._name);
	}

	bool operator == (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (_name == 0);
	}

	bool operator != (const NamePtr& namePtr) const {
		return (_name != namePtr._name);
	}

	bool operator != (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (_name != 0);
	}


	bool operator < (CKerName *name) const {
		if (_name == 0 || name==0)
			return false;
		return (_name->Compare(name) == 2);
	}

	bool operator > (CKerName *name) const {
		if (_name == 0 || name==0)
			return false;
		return (_name->Compare(name) == 1);
	}


	bool operator <= (CKerName *name) const {
		if (_name == name)
			return true;
		if (_name == 0 || name==0)
			return false;
		return (_name->Compare(name) == 2);
	}

	bool operator >= (CKerName *name) const {
		if (_name == name)
			return true;
		if (_name == 0 || name==0)
			return false;
		return (_name->Compare(name) == 1);
	}



	bool operator < (const OPointer &oPtr) const {
		return *this < oPtr.Type();
	}

	bool operator > (const OPointer &oPtr) const {
		return *this > oPtr.Type();
	}


	bool operator <= (const OPointer &oPtr) const {
		return *this <= oPtr.Type();
	}

	bool operator >= (const OPointer &oPtr) const {
		return *this >= oPtr.Type();
	}



private:
	CKerName *_name;
};


inline bool operator == (int null, const NamePtr &name) {
	return name == null;
}

inline bool operator != (int null, const NamePtr &name) {
	return name != null;
}



#endif
