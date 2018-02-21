#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Runtime::InteropServices;


#include "CStringToCharPtr.h"
#include "RootNames.h"


namespace Krkal {
	namespace Runtime {


		public ref class RootNames : IDisposable {
		private:
			CRootNames *_rootNames;
		public:
			static void InitRootNames() { CRootNames::InitRootNames();}
			static void DoneRootNames() { CRootNames::DoneRootNames();}

			RootNames(bool reloadIfNeeded) {
				_rootNames = CRootNames::GetRootNames();
				EnterCriticalSection(_rootNames->GetCriticalSection());
				if (reloadIfNeeded)
					_rootNames->ReloadAllFromDiskIfNeeded();
			}

			~RootNames() {
				this->!RootNames();
			}

			!RootNames() {
				LeaveCriticalSection(_rootNames->GetCriticalSection());
			}


			KerMain ^GetKernel() { return gcnew KerMain(_rootNames->GetKernel()); }

			static cli::array<String^>^ GetFiles(String ^rootName, KerNameType nameType, bool reloadIfNeeded) {
				vector<string> *files = CRootNames::GetRootNames()->GetFiles(CStringToCharPtr(rootName), (eKerNameType)nameType, reloadIfNeeded);
				if (!files)
					return nullptr;
				cli::array<String^>^ ret = gcnew cli::array<String^>(files->size());
				int f=0;
				for (vector<string>::iterator i = files->begin(); i != files->end(); ++i, ++f) {
					ret[f] = gcnew String(i->c_str());
				}
				return ret;
			}

			static bool IsRegisterUpToDate(String ^path) {
				return CRootNames::IsRegisterUpToDate(CStringToCharPtr(path));
			}
		};


	}
}
