//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - K e r n e l A r r a y
///
///		Classes that represents the Kernel Array
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __KERNEL_ARRAT_H__
#define __KERNEL_ARRAT_H__


#include <memory.h>
#include <assert.h>
#include "OPointer.h"



template <typename T> class CKerArrLocker;
template< typename T, int dim> struct KernelArrayType;



class CKerArrBase : public CManagedObject {
public:

	static const int DefaultArrayCapacity = 16;


	CKerArrBase(CGarbageCollector *collector, int capacity=DefaultArrayCapacity ) : CManagedObject(*collector)
	{
		_capacity = capacity;
		_count = 0;
		_startIndex = 0;
		_locked = 0;
	}


	int GetCount() const { return _count; }

	int GetCapacity() const { return _capacity; }

	void Clear()
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		_count = 0;
	}

	void Lock() {
		_locked++;
	}

	void Unlock() {
		if (_locked)
			_locked--;
	}

	bool IsLocked() const {
		return (_locked != 0);
	}


	virtual void MarkLinks(int mark, CGarbageCollector & collector)  { }


private:

	CKerArrBase(const CKerArrBase&);
	CKerArrBase& operator=(const CKerArrBase&);


protected:
	int _capacity;
	int _count;
	int _startIndex;
	int _locked;

	virtual ~CKerArrBase() {}
};









template <typename T>
class CKerArrBase2 : public CKerArrBase
{
public:
	
	typedef CKerArrLocker<T> locker;


	CKerArrBase2(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase(collector, capacity)
	{
		if (capacity != 0) {
			_data = new T[_capacity];
		} else {
			_data = 0;
		}
	}


	CKerArrBase2(CGarbageCollector *collector,  CKerArrBase2 *arr1, CKerArrBase2 *arr2) : CKerArrBase(collector, (arr1 && arr2) ? (arr1->_capacity + arr2->_count) : DefaultArrayCapacity)
	{
		if (_capacity != 0) {
			_data = new T[_capacity];
		} else {
			_data = 0;
		}
		AddRangeLast(arr1);
		AddRangeLast(arr2);
	}

	
	void SetCount( int count )
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		if( count > _capacity )
		{
			Reserve( count );
		}
		if( count > _count )
		{
			InitFields( _count, count-_count );
		}
		_count = count;		
	}



	T RemoveLast()
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		if(!_count) throw CKernelError(eKRTEindexOutOfRange);
		T& tm = At(_count-1);
		_count--;
		return tm;
	}

	T RemoveFirst()
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		if(!_count) throw CKernelError(eKRTEindexOutOfRange);
		T& tm = At(0);
		_count--;
		_startIndex++;
		if(_startIndex>=_capacity) _startIndex = 0;
		return tm;
	}

	void AddLast( const T& t )
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		Reserve( _count + 1 );
		_count++;
		At(_count-1) = t;
	}

	void AddRangeLast( const CKerArrBase2 *arr )
	{
		if (arr == 0) return;
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		Reserve( _count + arr->_count );
		MoveBlock(_count, arr, 0, arr->_count);
		_count+=arr->_count;
	}


	void AddFirst( const T& t )
	{
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		Reserve( _count + 1 );
		_count++;
		_startIndex--;
		if(_startIndex<0) _startIndex = _capacity - 1;
		At(0) = t;
	}

	void AddRangeFirst( const CKerArrBase2 *arr )
	{
		if (arr == 0) return;
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		Reserve( _count + arr->_count );
		_startIndex-= arr->_count;
		if(_startIndex<0) _startIndex += _capacity;
		MoveBlock(0, arr, 0, arr->_count);
		_count+=arr->_count;
	}


	void Insert(int index, const T& t) {
		if (index == 0) {
			AddFirst(t);
		} else if (index == _count) {
			AddLast(t);
		} else {
			if( index <= 0 || index >= _count)
				throw CKernelError(eKRTEindexOutOfRange);
			if(_locked) throw CKernelError(eKRTEarrayIsLocked);
			Reserve( _count + 1 );
			MoveBlock(index+1, this, index, 1);
			_count++;
			At(index) = t;
		}
	}


	void InsertRange(int index, const CKerArrBase2 *arr) {
		if (arr == 0) return;
		if (index == 0) {
			AddFirst(arr);
		} else if (index == _count) {
			AddLast(arr);
		} else {
			if( index <= 0 || index >= _count)
				throw CKernelError(eKRTEindexOutOfRange);
			if(_locked) throw CKernelError(eKRTEarrayIsLocked);
			Reserve( _count + arr->_count );
			MoveBlock(index+arr->_count, this, index, arr->_count);
			_count+=arr->_count;
			MoveBlock(index, arr, 0, arr->_count);
		}
	}



	void Reserve( int capacity )
	{
		if(capacity<=_capacity) return;
		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		
		capacity = capacity>2*_capacity ? capacity : 2*_capacity;

		Reserve2(capacity);
	}


	T& At( int index ) const
	{
		if( index < 0 || index >= _count)
			throw CKernelError(eKRTEindexOutOfRange);

		index += _startIndex; 
		if(index >= _capacity)
			index -= _capacity;

		return _data[index];
	}

	locker lAt(int index) {
		return locker(*this, At(index));
	}


	int Compare(const CKerArrBase2 *arr) const {
		if (!arr)
			return 1;
		if (_count != arr->_count)
			return _count - arr->_count;
		for (int f = 0; f < _count; f++) {
			if (At(f) != arr->At(f)) {
				return At(f) < arr->At(f) ? -1 : 1;
			}
		}
		return 0;
	}



private:


	void InitFields( int index, int count )
	{
		assert( count <= _capacity - _count);

		index += _startIndex; 
		if(index >= _capacity)
			index -= _capacity;

		int count1 = index+count<=_capacity ? count : _capacity - index;

		memset( _data + index, 0, count1 * sizeof(T) );
		memset( _data, 0, (count-count1) * sizeof(T) );
	}


	void Reserve2(int capacity) {
		T* data = AllocData(capacity);
		
		int count1 = _startIndex+_count<=_capacity ? _count : _capacity - _startIndex;

		T* tm = data;
		for(int i=0; i<count1; i++,tm++)
			*tm = _data[_startIndex+i];

		for(int i=0; i<_count-count1; i++,tm++)
			*tm = _data[i];

		FreeData();
		_data = data;
		_capacity = capacity;
		_startIndex = 0;
	}


	void MoveBlock(int dIndex, const CKerArrBase2 *source, int sIndex, int count) {
		// move block from last data, so it can by used top move in place. For moving in worward direction only!

		dIndex += _startIndex + count - 1;
		if (dIndex >= _capacity)
			dIndex -= _capacity;

		sIndex += source->_startIndex + count - 1;
		if (sIndex >= source->_capacity)
			sIndex -= source->_capacity;

		if (count-1 <= dIndex && count-1 <= sIndex) {

			for ( ; count; count--, dIndex--, sIndex--)
				_data[dIndex] = source->_data[sIndex];

		} else if (dIndex < sIndex) {

			for ( ; dIndex+1; count--, dIndex--, sIndex--)
				_data[dIndex] = source->_data[sIndex];
			dIndex += _capacity;

			if (count-1 <= sIndex) {
				for ( ; count; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
			} else {
				for ( ; sIndex+1; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
				sIndex += source->_capacity;

				for ( ; count; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
			}

		} else {

			for ( ; sIndex+1; count--, dIndex--, sIndex--)
				_data[dIndex] = source->_data[sIndex];
			sIndex += _capacity;

			if (count-1 <= dIndex) {
				for ( ; count; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
			} else {
				for ( ; dIndex+1; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
				dIndex += source->_capacity;

				for ( ; count; count--, dIndex--, sIndex--)
					_data[dIndex] = source->_data[sIndex];
			}

		}
	}

protected:

	T* _data;

	// to force delete and alloc in the same module
	virtual T* AllocData(int size) {
		return new T[size];
	}
	// to force delete and alloc in the same module
	virtual void FreeData() {
		delete[] _data;
		_data = 0;
	}


	virtual ~CKerArrBase2()
	{
		delete[] _data;
	}

	void SetData (T* data, int count, int capacity)
	{
		FreeData();
		_data = data;
		_count = count;
		_capacity = capacity;
	}


	void PrepareString() {
		int capacity = _capacity;
		if (capacity < _count+1) {
			capacity *= 2;
		} else if (_startIndex == 0) {
			_data[_count] = 0;
			return;
		}

		if(_locked) throw CKernelError(eKRTEarrayIsLocked);
		Reserve2(capacity);
		_data[_count] = 0;
	}

};






template <typename T>
class CKerArr : public CKerArrBase2<T>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}
};













template <typename Type, int dimension=1>
class CKerArrPtrBase
{
public:
	typedef typename KernelArrayType<Type,dimension>::type T;

	CKerArrPtrBase() {
		_array = 0;
	}

	CKerArrPtrBase( const CKerArrPtrBase& arrayPtr )
		: _array(arrayPtr._array)
	{
	}

	CKerArrPtrBase( CKerArr<T> * a )
		: _array(a)
	{
	}

	CKerArrPtrBase(int null) {
		_array = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
	}




	CKerArrPtrBase& operator=(const CKerArrPtrBase& arrayPtr)
	{
		_array = arrayPtr._array;
		return *this;
	}


	CKerArrPtrBase& operator=(CKerArr<T> * a)
	{
		_array = a;
		return *this;
	}

	CKerArrPtrBase& operator=(int null)
	{
		_array = 0;
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return *this;
	}


	operator CKerArr<T> *() const {
		return _array;
	}



	//operator bool() const {
	//	return _array != 0
	//}


	CKerArr<T> & operator *() {
		if (_array == 0)
			_array = new CKerArr<T>(GetGCFromStaticKerMain());
		return *_array;
	}

	CKerArr<T>* operator -> () {
		if (_array == 0)
			_array = new CKerArr<T>(GetGCFromStaticKerMain());
		return _array;
	}





	bool operator == (const CKerArrPtrBase& arrayPtr) const {
		return (_array == arrayPtr._array);
	}

	bool operator == (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (_array == 0);
	}

	bool operator != (const CKerArrPtrBase& arrayPtr) const {
		return (_array != arrayPtr._array);
	}

	bool operator != (int null) const {
		if (null != 0)
			throw CKernelError(eKRTEnullIsNot0);
		return (_array != 0);
	}




	CKerArrPtrBase& operator << (const T & a) {
		if (_array == 0)
			_array = new CKerArr<T>(GetGCFromStaticKerMain());
		_array->AddLast(a);
		return *this;
	}


	T& operator[](int index) {
		if (_array == 0)
			_array = new CKerArr<T>(GetGCFromStaticKerMain());
		return _array->At(index);
	}


	CKerArrPtrBase& Create(int capacity) {
		_array = new CKerArr<T>(GetGCFromStaticKerMain(), capacity);
		return *this;
	}


	CKerArrPtrBase operator + (const CKerArrPtrBase& arrayPtr) const {
		return CKerArrPtrBase(new CKerArr<T>(GetGCFromStaticKerMain(), _array, arrayPtr._array));
	}


	void GCMarkMe(int mark, CGarbageCollector & collector) const {
		if (_array != 0) 
			_array->MarkMe(mark, collector);
	}

	CKerArrPtrBase Clone () const {
		if (!_array)
			return 0;
		CKerArr<T> *arr = new CKerArr<T>(GetGCFromStaticKerMain(), _array->GetCapacity());
		arr->AddRangeLast(_array);
		return arr;
	}



protected:
	CKerArr<T> *_array;
};





template <typename Type, int dimension=1>
class ArrPtr : public CKerArrPtrBase<Type, dimension>
{
public:

	ArrPtr() {}

	ArrPtr( const CKerArrPtrBase& arrayPtr ) : CKerArrPtrBase(arrayPtr) {}

	ArrPtr( CKerArr<T> * a ) : CKerArrPtrBase(a) {}

	ArrPtr(int null) : CKerArrPtrBase(null) {}

};






template <typename Type, int dimension>
inline bool operator == (int null, const ArrPtr<Type, dimension> &arrPtr) {
	return arrPtr == null;
}

template <typename Type, int dimension>
inline bool operator != (int null, const ArrPtr<Type, dimension> &arrPtr) {
	return arrPtr != null;
}






template <typename T>
class CKerArrLocker
{
public:
	CKerArrLocker(CKerArrBase2<T>& kernelArray, T &data) : _data(data) {
		_array = &kernelArray;
		_array->Lock();
	}

	~CKerArrLocker() {
		_array->Unlock();
	}



	operator T&() const
	{
		return _data;
	}

	operator T*() const
	{
		return &_data;
	}

	T * operator &() const
	{
		return &_data;
	}





private:
	CKerArrBase2<T>* _array;
	T &_data;
};



//template <typename T>
//class CKerArrIterator
//{
//public:
//	CKerArrIterator( CKerArr<T>& kernelArray, int index )
//		: _array(&kernelArray), _index(index)
//	{
//	}
//
//	CKerArrIterator( const CKerArrIterator& iterator )
//		: _array(iterator._array), _index(iterator._index)
//	{
//	}
//
//	CKerArrIterator& operator=(const CKerArrIterator& iterator)
//	{
//		_array = iterator._array;
//		_index = iterator._index;
//		return *this;
//	}
//
//	CKerArrIterator& operator=(const T& t)
//	{
//		_array->Write( _index, t );
//		return *this;
//	}
//
//	operator T() const
//	{
//		return _array->Read( _index);
//	}
//
//private:
//	CKerArr<T>* _array;
//	int _index;
//};


template< typename T, int dim>
struct KernelArrayType
{
	typedef ArrPtr<typename KernelArrayType<T,dim-1>::type> type;
};

template< typename T >
struct KernelArrayType<T,1>
{
	typedef T type;
};






template <typename T>
class CKerArr<ArrPtr<T>> : public CKerArrBase2<ArrPtr<T>>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}

	virtual void MarkLinks(int mark, CGarbageCollector & collector)  { 
		for (int i=0; i<GetCount(); i++) {
			At(i).GCMarkMe(mark, collector);
		}
	}


};


template <typename T>
class CKerArr<CKerArr<T>*> : public CKerArrBase2<CKerArr<T>*>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}

	virtual void MarkLinks(int mark, CGarbageCollector & collector)  { 
		for (int i=0; i<GetCount(); i++) {
			CKerArr<T>* arr = At(i);
			if (arr)
				arr->MarkMe(mark, collector);
		}
	}


};


template <>
class CKerArr<CKerArrBase*> : public CKerArrBase2<CKerArrBase*>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}

	virtual void MarkLinks(int mark, CGarbageCollector & collector)  { 
		for (int i=0; i<GetCount(); i++) {
			CKerArrBase* arr = At(i);
			if (arr)
				arr->MarkMe(mark, collector);
		}
	}


};



template <>
class CKerArr<OPointer> : public CKerArrBase2<OPointer>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}

	virtual void MarkLinks(int mark, CGarbageCollector & collector)  { 
		for (int i=0; i<GetCount(); i++) {
			At(i).GCMarkMe(mark, collector);
		}
	}


};



template <>
class CKerArr<wchar_t> : public CKerArrBase2<wchar_t>
{
public:
	
	CKerArr(CGarbageCollector *collector,  int capacity=DefaultArrayCapacity ) : CKerArrBase2(collector, capacity) {}
	CKerArr(CGarbageCollector *collector,  CKerArr *arr1, CKerArr *arr2) : CKerArrBase2(collector, arr1, arr2) {}

	CKerArr(const wchar_t *str, CGarbageCollector *collector) : CKerArrBase2(collector, 0) {
		int len = wcslen(str);
		wchar_t *str2 = new wchar_t[len+1];
		wcscpy_s(str2, len+1, str);
		SetData(str2, len, len+1);
	}

	CKerArr(const char *str, CGarbageCollector *collector) : CKerArrBase2(collector, 0) {
		int size = MultiByteToWideChar(CP_ACP, 0, str, -1, 0, 0);
		wchar_t *str2 = new wchar_t[size];
		MultiByteToWideChar(CP_ACP, 0, str, -1, str2, size);
		SetData(str2, size-1, size);
	}

	// you have to copy the string before the array changes!
	locker c_str() { 
		PrepareString();
		return locker(*this, *_data);
	}


};



template <>
class ArrPtr<wchar_t, 1> : public CKerArrPtrBase<wchar_t, 1>
{
public:

	ArrPtr() {}

	ArrPtr( const CKerArrPtrBase& arrayPtr ) : CKerArrPtrBase(arrayPtr) {}

	ArrPtr( CKerArr<T> * a ) : CKerArrPtrBase(a) {}

	ArrPtr(int null) : CKerArrPtrBase(null) {}

	ArrPtr(const wchar_t *str) {
		if (!str) {
			_array = 0;
		} else {
			_array = new CKerArr<wchar_t>(str, GetGCFromStaticKerMain());
		}
	}

	ArrPtr(const char *str) {
		if (!str) {
			_array = 0;
		} else {
			_array = new CKerArr<wchar_t>(str, GetGCFromStaticKerMain());
		}
	}

};




typedef ArrPtr<wchar_t> KString;



#endif