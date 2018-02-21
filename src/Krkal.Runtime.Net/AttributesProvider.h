#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

#include "CKerMain.Net.h"



namespace Krkal {
	namespace Runtime {


		public ref class AttributesProvider : public IAttributesProvider 
		{
		public:
			AttributesProvider() {
				_collectionCache = gcnew Dictionary<String^, IAttributesCollection^>();
			}


			virtual IAttributesCollection ^LoadAttributes(String ^root, bool dontUseCache);


		private:
			Dictionary<String^, IAttributesCollection^> ^_collectionCache;

		};






		private ref class AttributeDescription : public IAttribute
		{
		public:
			property String ^KsidName {
				virtual String ^get() { return _ksidName; }
			}
			property AttributeLocation Location {
				virtual AttributeLocation get() { return _location; }
			}
			property IEnumerable<NameType> ^Filter {
				virtual IEnumerable<NameType> ^get() { return _filter; }
			}
		internal:
			AttributeDescription(CDataObject *data, CKerName *locationKsid, CKerName *filterKsid);
		private:
			String ^_ksidName;
			AttributeLocation _location;
			cli::array<NameType>^ _filter;
		};





		private ref class AttributesCollection : public IAttributesCollection
		{
		public:
			property String ^Root {
				virtual String ^get() { return _root; }
			}
			virtual bool IsReloadNeeded(); // when this returns true, you need to stop using this collection and ask IAttributesProvider for new one
			virtual IEnumerator<IAttribute^>^ GetEnumeratorA() = IEnumerable<IAttribute^>::GetEnumerator { return _attributes->GetEnumerator(); }
			virtual System::Collections::IEnumerator^ GetEnumeratorB() = System::Collections::IEnumerable::GetEnumerator { return _attributes->GetEnumerator(); }
			~AttributesCollection() { this->!AttributesCollection(); }
			!AttributesCollection();
		internal:
			AttributesCollection(String ^root);
		private:
			String ^_root;
			List<IAttribute^>^ _attributes;
			CKernelParameters *_parameters;
			bool _isReloadNeeded;
		};




	}
}