#include "stdafx.h"

#include "AttributesProvider.h"
#include "fs.api.h"


namespace Krkal {
namespace Runtime {


	IAttributesCollection ^AttributesProvider::LoadAttributes(String ^root, bool dontUseCache) {
		IAttributesCollection ^ret = nullptr;
		_collectionCache->TryGetValue(root, ret);
		if (ret != nullptr) {
			if (dontUseCache || ret->IsReloadNeeded()) {
				_collectionCache->Remove(root);
			} else {
				return ret;
			}
		}

		try {
			ret = gcnew AttributesCollection(root);
			_collectionCache->Add(root, ret);
		} catch (CKernelPanic) {
			ret = nullptr;
		}

		return ret;
	}



	AttributesCollection::AttributesCollection(String ^root)
	{
		_root = root;
		_attributes = gcnew List<IAttribute^>();

		_parameters = new CKernelParameters(CStringToCharPtr(root), eKRMDataOnly, eKerDBDebug, 0,0);
		CKerMain *kerMain = 0;
		try {
			kerMain = new CKerMain(_parameters);
			kerMain->Load();

			NamePtr attributeKsid = kerMain->KerNamesMain->GetNamePointer("_KSID_Attribute");
			CKerName *locationKsid = kerMain->KerNamesMain->GetNamePointer("_KSID_Attribute__M_Location");
			CKerName *filterKsid = kerMain->KerNamesMain->GetNamePointer("_KSID_Attribute__M_Filter");
			if (!attributeKsid || !locationKsid || !filterKsid)
				throw CKernelPanic();

			for (int f=0; f < kerMain->Objs->DataObjectCount; f++) {
				if (NamePtr(kerMain->Objs->DataObjects[f]->GetClass(kerMain)) <= attributeKsid) {
					_attributes->Add(gcnew AttributeDescription(kerMain->Objs->DataObjects[f], locationKsid, filterKsid));
				}
			}
		}
		finally {
			SAFE_DELETE(kerMain);
		}
	}


	// when this returns true, you need to stop using this collection and ask IAttributesProvider for new one
	bool AttributesCollection::IsReloadNeeded() {
		if (_isReloadNeeded)
			return true;

		CKernelParameters params2;
		for (int f=0; f < _parameters->GetRootNamesCount(); f++) {
			params2.AddRootName(_parameters->GetRootName(f));
		}

		if (!params2.InitUsedFiles(CFS::GetFS()) || !params2.CompareUsedFiles(_parameters)) {
			_isReloadNeeded = true;
			return true;
		} else {
			return false;
		}
	}



	AttributesCollection::!AttributesCollection() {
		SAFE_DELETE(_parameters);
	}




	AttributeDescription::AttributeDescription(CDataObject *data, CKerName *locationKsid, CKerName *filterKsid) {
		_ksidName = gcnew String(data->Name->name);
		_location = (AttributeLocation)data->GetValue<int>(locationKsid);
		
		CKerArr<int>* arr = data->GetValue<CKerArr<int>*>(filterKsid);
		if (!arr || !arr->GetCount()) {
			_filter = nullptr;
		} else {
			_filter = gcnew cli::array<NameType>(arr->GetCount());
			for (int f=0; f<arr->GetCount(); f++) {
				_filter[f] = (NameType)arr->At(f);
			}
		}
	}



}
}