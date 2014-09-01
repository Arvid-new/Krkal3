#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

#include "CKerMain.h"
#include "CStringToCharPtr.h"
#include <gcroot.h>
#include "Names.Net.h"
#include "Objects.Net.h"



namespace Krkal {
	namespace Runtime {


		public ref class KernelPanicException : public Exception
		{
		public:
			KernelPanicException(String ^message)
				: Exception(message) { }
			KernelPanicException()
				: Exception("Kernel Panic") { }
		};





		public enum class KernelRunMode {
			Normal = 0,	// Hra
			Editor = 1,	// Editor
			CreateData = 2,
			DataEdit = 3,
			DataOnly = 4,
		};

		public enum class KernelDebugMode {
			Release = 0,  // Loguji se jen FatalErrory a PanicErrory
			Debug = 1		// Loguje se vse, FatalErroru muze byt jen deset
		};





		public delegate void KerLoggingCallBackDelegate(int time, int errorNum, int errorParam, String^ message);
		public delegate bool CreateEngineAndServicesDelegate(KerMain ^kerMain, String ^engine);

		void LoggingCallBackHandler(void *handle, int time, int ErrorNum, int ErrorParam, const char *ErrorStr);
		bool CreateEngineAndServicesHandler(CKerMain *kerMain, const char *engine);



		public ref class KernelParameters : IDisposable{
		private:
			CKernelParameters *_kp;
			GCHandle _handle;
		internal:
			CKernelParameters *GetKP() { return _kp;}
			void RaiseOnErrorCallback(int time, int errorNum, int errorParam, String^ message) {
				OnError(time, errorNum, errorParam, message);
			}
			bool RaiseCreateEngineHandler(KerMain ^kerMain, String ^engine) {
				if (_createEngineAndServicves == nullptr)
					return true;
				return _createEngineAndServicves(kerMain, engine);
			}

		public:
			KernelParameters(String ^fileName, KernelRunMode runMode, KernelDebugMode debugMode) {
				_handle = GCHandle::Alloc(this);
				_kp = new CKernelParameters(CStringToCharPtr(fileName), (eKerRunMode)runMode, (eKerDebugModes)debugMode, LoggingCallBackHandler, static_cast<IntPtr>(_handle).ToPointer());
				_kp->CreateEngineAndServices = CreateEngineAndServicesHandler;
			}

			~KernelParameters() {
				_handle.Free();
				this->!KernelParameters();
			}
			!KernelParameters() {
				SAFE_DELETE(_kp);
			}

			property KernelRunMode RunMode {
				KernelRunMode get() { return (KernelRunMode)_kp->Runmode;}
			}

			property KernelDebugMode DebugMode {
				KernelDebugMode get() { return (KernelDebugMode)_kp->Debugmode;}
			}

			property String^ FileName {
				String^ get() { return gcnew String(_kp->GetFileName()); }
				void set(String^ value) {_kp->SetFileName(CStringToCharPtr(value));}
			}

			List<String^> ^GetRootNames() {
				List<String^> ^ret = gcnew List<String^>(_kp->GetRootNamesCount());
				for (int f=0; f<_kp->GetRootNamesCount(); f++) {
					ret->Add(gcnew String(_kp->GetRootName(f)));
				}
				return ret;
			}
			void SetRootNames(IEnumerable<String^>^ rootNames) {
				_kp->ClearAllRootNames();
				for each(String ^ s in rootNames) {
					_kp->AddRootName(CStringToCharPtr(s));
				}
			}

			event KerLoggingCallBackDelegate^ OnError;

			CreateEngineAndServicesDelegate ^_createEngineAndServicves;
			property CreateEngineAndServicesDelegate ^CreateEngineAndServices {
				CreateEngineAndServicesDelegate ^get() { return _createEngineAndServicves; }
				void set(CreateEngineAndServicesDelegate ^value) {_createEngineAndServicves = value; }
			}

		};



		public ref class KerMain : IDisposable {
		private:
			CKerMain *_KerMain;
			bool _deleteKerMain;

		internal:
			CKerMain *GetKerMain() { return _KerMain; }
			KerMain(CKerMain *kerMain) { 
				_KerMain=kerMain; 
				_deleteKerMain = false;
			}
		public:
			IntPtr GetKerMain2() { return IntPtr(_KerMain); }


			KerMain(KernelParameters ^parameters) { 
				_KerMain=0; 
				_deleteKerMain = true;
				
				try {
					_KerMain = new CKerMain(parameters->GetKP());
				} catch (CKernelPanic) {
					throw gcnew KernelPanicException();
				}
			}



			~KerMain() {
				this->!KerMain();
			}
			!KerMain() {
				if (_deleteKerMain)
					SAFE_DELETE(_KerMain);
			}



			void Load() { 
				try {
					_KerMain->Load(); 
				} catch (CKernelPanic) {
					throw gcnew KernelPanicException();
				}
			}

			void TerminateKernel() { _KerMain->TerminateKernel(); }

			property bool RunGarbageCollector {
				bool get() { return _KerMain->RunGarbageCollector; }
				void set(bool value) {_KerMain->RunGarbageCollector = value;}
			}

			void RunTurn(int timeIncrease, bool crisis) { 
				try {
					_KerMain->RunTurn(timeIncrease, crisis);
				} catch (CKernelPanic) {
					throw gcnew KernelPanicException();
				}
			}


			KerName FindName(String^ name) {
				return KerName(_KerMain->KerNamesMain->GetNamePointer(CStringToCharPtr(name)));
			}
			KerName FindName(KsidName^ name) {
				if (!name)
					return KerName(0);
				return KerName(_KerMain->KerNamesMain->GetNamePointer(CStringToCharPtr(name->Identifier->ToKsidString())));
			}

			property KerObject StaticData {
				KerObject get() { return KerObject(_KerMain->StaticData); }
			}

			IList<KerName>^  GetNames(KerName bellow) {
				List<KerName>^ outputCollection = gcnew List<KerName>();
				CKerNameList *list = _KerMain->KerNamesMain->Names;
				for ( ; list; list = list->next) {
					if (bellow.IsNull || NamePtr(list->name) < bellow)
						outputCollection->Add(KerName(list->name));
				}
				return outputCollection;
			}


			String ^ReadUserName(KerName name) {
				KString str = _KerMain->ReadAttribute<KString>(name, eKKNattrUserName);
				if (!str)
					return String::Empty;
				return gcnew String(str->c_str());
			}

			String ^ReadComment(KerName name) {
				KString str = _KerMain->ReadAttribute<KString>(name, eKKNattrComment);
				if (!str)
					return String::Empty;
				return gcnew String(str->c_str());
			}

			String ^ReadUserName(KsidName ^name) {
				return ReadUserName(FindName(name));
			}

			String ^ReadComment(KsidName ^name) {
				return ReadComment(FindName(name));
			}

			String ^ReadFileAttribute(KerName name) {
				KString str = _KerMain->ReadAttribute<KString>(name, eKKNattrFile);
				if (!str)
					return String::Empty;
				return gcnew String(str->c_str());
			}

			String ^ReadFileAttribute(KsidName ^name) {
				return ReadFileAttribute(FindName(name));
			}

			String ^ReadStringAttribute(KerName name, KerName attribute) {
				KString str = _KerMain->ReadAttribute<KString>(name, attribute);
				if (!str)
					return String::Empty;
				return gcnew String(str->c_str());
			}

			String ^ReadStringAttribute(KsidName ^name, KsidName ^attribute) {
				return ReadStringAttribute(FindName(name), FindName(attribute));
			}


			int ReadIntAttribute(KerName name, KerName attribute) {
				return _KerMain->ReadAttribute<int>(name, attribute);
			}

			int ReadIntAttribute(KsidName ^name, KsidName ^attribute) {
				return ReadIntAttribute(FindName(name), FindName(attribute));
			}


			KerName ReadNameAttribute(KerName name, KerName attribute) {
				return KerName(_KerMain->ReadAttribute<CKerName*>(name, attribute));
			}

			KerName ReadNameAttribute(KsidName ^name, KsidName ^attribute) {
				return ReadNameAttribute(FindName(name), FindName(attribute));
			}
		};

	}
}