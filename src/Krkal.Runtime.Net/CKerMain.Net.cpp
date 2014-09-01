#include "stdafx.h"

#include "CKerMain.Net.h"
#include "RootNames.Net.h"


namespace Krkal {
namespace Runtime {


	void LoggingCallBackHandler(void *handle, int time, int ErrorNum, int ErrorParam, const char *ErrorStr) {
		KernelParameters ^parameters = safe_cast<KernelParameters ^>(GCHandle::FromIntPtr(IntPtr(handle)).Target);
		parameters->RaiseOnErrorCallback(time, ErrorNum, ErrorParam, gcnew String(ErrorStr));
	}


	bool CreateEngineAndServicesHandler(CKerMain *kerMain, const char *engine) {		
		KernelParameters ^parameters = safe_cast<KernelParameters ^>(GCHandle::FromIntPtr(IntPtr(kerMain->KernelParameters->Handle)).Target);
		return parameters->RaiseCreateEngineHandler(gcnew KerMain(kerMain),  engine ? gcnew String(engine) : nullptr);
	}


}
}