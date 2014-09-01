#pragma once


class CStringToCharPtr {
public:
	CStringToCharPtr(System::String^ string) {
		_charPtr = 0;
		if (string)
			_charPtr = (char*)System::Runtime::InteropServices::Marshal::StringToHGlobalAnsi(string).ToPointer();
	}
	~CStringToCharPtr() {
		if (_charPtr)
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr(_charPtr));
	}
	operator char*() {
		return _charPtr;
	}
private:
	char *_charPtr;
};


class CStringToWCharPtr {
public:
	CStringToWCharPtr(System::String^ string) {
		_charPtr = 0;
		if (string)
			_charPtr = (wchar_t*)System::Runtime::InteropServices::Marshal::StringToHGlobalUni(string).ToPointer();
	}
	~CStringToWCharPtr() {
		if (_charPtr)
			System::Runtime::InteropServices::Marshal::FreeHGlobal(IntPtr(_charPtr));
	}
	operator wchar_t*() {
		return _charPtr;
	}
private:
	wchar_t *_charPtr;
};



class CCharPtrToString_D {
public:
	CCharPtrToString_D() {
		_charPtr = 0;
	}
	~CCharPtrToString_D() {
		if (_charPtr)
			delete[] _charPtr;
	}

	char **GetAddress() {
		return &_charPtr;
	}
	operator char*() {
		return _charPtr;
	}

	System::String ^ToString() {
		if (_charPtr) {
			return gcnew System::String(_charPtr);
		} else {
			return nullptr;
		}
	}
private:
	char *_charPtr;
};




class CCharPtrToString_ND {
public:
	CCharPtrToString_ND() {
		_charPtr = 0;
	}

	const char **GetAddress() {
		return &_charPtr;
	}
	operator const char*() {
		return _charPtr;
	}

	System::String ^ToString() {
		if (_charPtr) {
			return gcnew System::String(_charPtr);
		} else {
			return nullptr;
		}
	}
private:
	const char *_charPtr;
};



using namespace System::Threading;

ref class Lock {
   Object^ m_pObject;
 //  Lock % operator=( Lock const % );
  // Lock( Lock const % );
public:
   Lock( Object ^ pObject ) : m_pObject( pObject ) {
      Monitor::Enter( m_pObject );
   }
   ~Lock() {
      Monitor::Exit( m_pObject );
   }
};
