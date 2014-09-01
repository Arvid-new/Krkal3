// Krkal.NativeConsoleGames.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <conio.h>


#include "CKerMain.h"
#include "fs.api.h"
#include "RootNames.h"


void ReadAllKeys() {
	while (_kbhit()) _getch();
}



CFSRegister *Errors2 = 0;

void LogError(void *handle, int time, int ErrorNum, int ErrorParam, const char *ErrorStr) {
	char buff[16];
	const char *description = "";
	if (Errors2) {
		sprintf_s(buff, "%i", ErrorNum);
		CFSRegKey *key = Errors2->FindKey(buff);
		if (key)
			description = key->GetDirectAccess();
	}

	printf("%i %i %i %s %s\n", time, ErrorNum, ErrorParam, description, ErrorStr);
}




int main(int argc, char* argv[])
{
	const char *project = "AntDeathMatch_3798_03CD_176A_36B2.code";
	if (argc > 1)
		project = argv[1];

	printf("\n\nThis sample demonstarates how Krkal console game can be started independently, using only native environment.\n");
	printf("Only games that run in default console engine are supported.\n");
	printf("In first command line argument you can specify which game will start. By default is started AntDeathMatch.\n");
	printf("\nPress any key to start, then any key to stop\n");

	ReadAllKeys();
	while (!_kbhit()) Sleep(1);

	CFSRegister *errors = 0;
	CKernelParameters *parameters = 0;
	CKerMain *kerMain = 0;

	CRootNames::InitRootNames();

	try {
		errors = new CFSRegister("$ERRORS$", "ERRORS");
		if (errors->GetOpenError() == FSREGOK)
			Errors2 = errors->FindKey("Ker RTE")->GetSubRegister();

		parameters = new CKernelParameters(project, eKRMNormal, eKerDBDebug, LogError, 0);
		kerMain = new CKerMain(parameters);
		kerMain->Load();

		ReadAllKeys();
		while (!_kbhit()) {
			Sleep(33);
			kerMain->RunTurn(33, 0);
		}

	} catch (CKernelPanic) {
		printf("\nKernel panic exception! terminating...\n");
		ReadAllKeys();
		while (!_kbhit()) Sleep(1);
	}

	SAFE_DELETE(kerMain);
	SAFE_DELETE(parameters);
	SAFE_DELETE(errors);

	CRootNames::DoneRootNames();

}

