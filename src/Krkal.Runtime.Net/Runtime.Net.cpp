#include "stdafx.h"

#include "Names.Net.h"
#include "Objects.Net.h"
#include "KerArray.Net.h"


namespace Krkal {
namespace Runtime {


	KerObjectT KerName::ObjectType::get() {
		return KerObjectT(_name->Type == eKerNTclass ? _name->ObjectType : 0);
	}


	KerArrBase^ KerArrBase::Create(CKerArrBase *arr, LanguageType lt) {
		if (lt.DimensionsCount == 0) {
			throw gcnew ArgumentException("Array cannot have 0 dimensions.");
		} else if (lt.DimensionsCount>1) {
			return gcnew KerArrArray(arr, lt);
		} else {
			switch (lt.BasicType) {
				case BasicType::Char:
					return gcnew KerArrChar(arr, lt);
				case BasicType::Int:
					return gcnew KerArrInt(arr, lt);
				case BasicType::Double:
					return gcnew KerArrDouble(arr, lt);
				case BasicType::Object:
					return gcnew KerArrObject(arr, lt);
				case BasicType::Name:
					return gcnew KerArrName(arr, lt);
				default:
					throw gcnew ArgumentException("Invalid type.");
			}
		}
	}



	KerArrBase ^KerObject::ReadArray(KerOVar var) {
		if (var.LangType.DimensionsCount == 0)
			throw gcnew ArgumentException("Invalid type.");
		CKerArrBase* arr = OPointer(_obj->thisO).get<CKerArrBase*>(var.Offset);
		if (!arr)
			return nullptr;
		return KerArrBase::Create(arr, var.LangType);
	}


	void KerObject::WriteArray(KerOVar var, KerArrBase ^value) {
		if (var.LangType.DimensionsCount == 0)
			throw gcnew ArgumentException("Invalid type.");

		if (value == nullptr) {
			OPointer(_obj->thisO).get<CKerArrBase*>(var.Offset) = 0;
		} else {
			if (var.LangType != value->LT)
				throw gcnew ArgumentException("Invalid type.");
			OPointer(_obj->thisO).get<CKerArrBase*>(var.Offset) = value->GetCKerArrBase();
		}
	}


	Object ^KerObject::Read(KerOVar var) {
		if (var.LangType.DimensionsCount != 0) {
			CKerArrBase* arr = OPointer(_obj->thisO).get<CKerArrBase*>(var.Offset);
			if (!arr)
				return nullptr;
			return KerArrBase::Create(arr, var.LangType);
		} else {
			switch (var.LangType.BasicType) {
				case BasicType::Char:
					return (wchar_t)OPointer(_obj->thisO).get<wchar_t>(var.Offset);
				case BasicType::Int:
					return (int)OPointer(_obj->thisO).get<int>(var.Offset);
				case BasicType::Double:
					return (double)OPointer(_obj->thisO).get<double>(var.Offset);
				case BasicType::Name:
					return KerName(OPointer(_obj->thisO).get<CKerName*>(var.Offset));
				case BasicType::Object:
					return KerObject(OPointer(_obj->thisO).get<OPointer>(var.Offset));
				default:
					throw gcnew ArgumentException("Invalid type.");
			}
		}
	}




	void KerObject::Hold(KerMain ^KerMain) {
		if (_obj == 0)
			return;
		KerMain->GetKerMain()->GetIGarbageCollector()->Hold(_obj);
	}

	void KerObject::Release(KerMain ^KerMain) {
		if (_obj == 0)
			return;
		KerMain->GetKerMain()->GetIGarbageCollector()->Release(_obj);
	}


	void KerArrBase::Hold(KerMain ^KerMain) {
		if (_arr == 0)
			return;
		KerMain->GetKerMain()->GetIGarbageCollector()->Hold(_arr);
	}

	void KerArrBase::Release(KerMain ^KerMain) {
		if (_arr == 0)
			return;
		KerMain->GetKerMain()->GetIGarbageCollector()->Release(_arr);
	}



	void KerObject::Kill(KerMain ^KerMain) {
		if (_obj == 0)
			return;
		KerMain->GetKerMain()->DeleteObject(0, _obj->thisO);
	}


}
}