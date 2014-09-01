// Utils.ErrorGenerator.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "KerErrorNumbers.h"
#include "fs.api.h"








int main()
{
	char str[256];
	CFS::InitFS();

/////////////////////////////////////////////////////////////////////////////////
	CFSRegister *r = new CFSRegister("$ERRORS$","ERRORS",FSREGCLEARIT);
	r->SetRegisterToBeUnCompressed();
	// FATALS:
	CFSRegister *r2 = r->AddKey("Ker RTE",FSRTregister)->GetSubRegister();
	sprintf(str,"%u",eKRTEDCNoObj);
	r2->AddKey(str,FSRTstring)->stringwrite("Directly Calling Method of Not Existing Object");
	sprintf(str,"%u",eKRTEDCBadObj);
	r2->AddKey(str,FSRTstring)->stringwrite("Direct Call: This Object doesnt heve called method!");
	sprintf(str,"%u",eKRTEobjcount1);
	r2->AddKey(str,FSRTstring)->stringwrite("Too meny objects was created. Object Counter is 0x10000000");
	sprintf(str,"%u",eKRTEobjcount2);
	r2->AddKey(str,FSRTstring)->stringwrite("Too meny objects was created. Object Counter is 0x80000000");
	sprintf(str,"%u",eKRTEobjcount3);
	r2->AddKey(str,FSRTstring)->stringwrite("Too meny objects was created. Object Counter is 0xFFFFFF00");
	sprintf(str,"%u",eKRTEcallstackw);
	r2->AddKey(str,FSRTstring)->stringwrite("Warning: Too Many function Calls");
	sprintf(str,"%u",eKRTEmessagew);
	r2->AddKey(str,FSRTstring)->stringwrite("Warning: There's too many messages");
	sprintf(str,"%u",eKRTEmapNotRegistered);
	r2->AddKey(str,FSRTstring)->stringwrite("Map Not Registered!");
	sprintf(str,"%u",eKRTEautoNotFound);
	r2->AddKey(str,FSRTstring)->stringwrite("No Automatic Graphic Found for Object");
	sprintf(str,"%u",eKRTEuserFatal);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Fatel Error");
	sprintf(str,"%u",eKRTEnotValidName);
	r2->AddKey(str,FSRTstring)->stringwrite("Invalid Name. The variable is probably undefined.");
	sprintf(str,"%u",eKRTEcycleInbjPtrs);
	r2->AddKey(str,FSRTstring)->stringwrite("Saving Levels: There is a cycle in obj Pointers!");

	// INFOS:
	sprintf(str,"%u",eKRTEKernelInit);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel Starts! Ready to run scripts");
	sprintf(str,"%u",eKRTEPaniCInfo);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel Panic: Kernel is shutting Down immediately!!");
	sprintf(str,"%u",eKRTELoadingLevel);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel is loading level:");
	sprintf(str,"%u",eKRTELoadingScript);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel is loading scripts. Version:");
	sprintf(str,"%u",eKRTELoadComplete);
	r2->AddKey(str,FSRTstring)->stringwrite("...Loading Complete.");
	sprintf(str,"%u",eKRTECompilating);
	r2->AddKey(str,FSRTstring)->stringwrite("Scripts need to be recompiled. Recompiling..");
	sprintf(str,"%u",eKRTEKernelEnds);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel is shutting down ..");
	sprintf(str,"%u",eKRTEsavingLevel);
	r2->AddKey(str,FSRTstring)->stringwrite("Saving Level ...");
	sprintf(str,"%u",eKRTEsaveLOK);
	r2->AddKey(str,FSRTstring)->stringwrite("Save Successful!");
	sprintf(str,"%u",eKRTEuserInfo);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Info");
	sprintf(str,"%u",eKRTEgameVictory);
	r2->AddKey(str,FSRTstring)->stringwrite("You are Victorious!! Congratulations!! :)");
	sprintf(str,"%u",eKRTEloadingGame);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel is loading saved game:");
	sprintf(str,"%u",eKRTEsavingGame);
	r2->AddKey(str,FSRTstring)->stringwrite("Saving Game ...");
	sprintf(str,"%u",eKRTEgarbageCollector);
	r2->AddKey(str,FSRTstring)->stringwrite("Garbage Collector status:");
	sprintf(str,"%u",eKRTEloadingDataFile);
	r2->AddKey(str,FSRTstring)->stringwrite("Kernel is loading data file:");
	sprintf(str,"%u",eKRTEsavingDataFile);
	r2->AddKey(str,FSRTstring)->stringwrite("Saving Data File ...");



	// PANICS:
	sprintf(str,"%u",eKRTENoMemory);
	r2->AddKey(str,FSRTstring)->stringwrite("Out Of Script's Stack Memory!!");
	sprintf(str,"%u",eKRTEDelObjInUse);
	r2->AddKey(str,FSRTstring)->stringwrite("Deleting Object In Use!! (Dectructor called in incorrect time)");
	sprintf(str,"%u",eKRTEobjcountOVERFLOW);
	r2->AddKey(str,FSRTstring)->stringwrite("Object Counter is 0xFFFFFFFF. Object counter has overflowed!!!");
	sprintf(str,"%u",eKRTEPELoadingLevel);
	r2->AddKey(str,FSRTstring)->stringwrite("Error while loading Level");
	sprintf(str,"%u",eKRTEPELoadingScripts);
	r2->AddKey(str,FSRTstring)->stringwrite("Error while loading Scripts");
	sprintf(str,"%u",eKRTEOutOfTime);
	r2->AddKey(str,FSRTstring)->stringwrite("Time Counter has Overflowed!");
	sprintf(str,"%u",eKRTEcallstack);
	r2->AddKey(str,FSRTstring)->stringwrite("Too Many function Calls! Stack Full!");
	sprintf(str,"%u",eKRTEmessage);
	r2->AddKey(str,FSRTstring)->stringwrite("There's too many messages! Quees Full!");
	sprintf(str,"%u",eKRTEuserPanic);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Panic Error");
	sprintf(str,"%u",eKRTECompilationFailed);
	r2->AddKey(str,FSRTstring)->stringwrite("Compilatin Failed!!");
	sprintf(str,"%u",eKRTEpictureNotLoaded);
	r2->AddKey(str,FSRTstring)->stringwrite("Failed to load Picture!");
	sprintf(str,"%u",eKRTEcyclusInKSID);
	r2->AddKey(str,FSRTstring)->stringwrite("There is a cycle in KSID names!");
	sprintf(str,"%u",eKRTEerrorInAuto);
	r2->AddKey(str,FSRTstring)->stringwrite("Automatic graphic is corrupted. Check the file.");
	sprintf(str,"%u",eKRTEmapNotWorking);
	r2->AddKey(str,FSRTstring)->stringwrite("Objet Map is not working correctly.");
	sprintf(str,"%u",eKRTEdeletingBadArray);
	r2->AddKey(str,FSRTstring)->stringwrite("deleting Not existing or Invalid Kernel Array!");
	sprintf(str,"%u",eKRTEbadKeyName);
	r2->AddKey(str,FSRTstring)->stringwrite("This Cannot be a name for key Input!");
	sprintf(str,"%u",eKRTEbadSoundName);
	r2->AddKey(str,FSRTstring)->stringwrite("This Cannot be a name for Sound!");
	sprintf(str,"%u",eKRTEobjPtrConflict);
	r2->AddKey(str,FSRTstring)->stringwrite("There are two same, conflicting ObjPtrs!");
	sprintf(str,"%u",eKRTEinvLvlVersion);
	r2->AddKey(str,FSRTstring)->stringwrite("The level version doesn't match! Hack attempt?");
	sprintf(str,"%u",eKRTEdllLoadFailed);
	r2->AddKey(str,FSRTstring)->stringwrite("Failed to load dll with script!");
	sprintf(str,"%u",eKRTEundefinedKnownClass);
	r2->AddKey(str,FSRTstring)->stringwrite("Known class name was not defined by the compiler!");
	sprintf(str,"%u",eKRTEfunctionDoesntExist);
	r2->AddKey(str,FSRTstring)->stringwrite("Function doesn't exist in KS.dll!");
	sprintf(str,"%u",eKRTEfscriptEntryPointnotFound);
	r2->AddKey(str,FSRTstring)->stringwrite("Script entry point not found.");
	sprintf(str,"%u",eKRTEwrongLoadingParameters);
	r2->AddKey(str,FSRTstring)->stringwrite("Error while loading Kernel. Invalid parameter.");
	sprintf(str,"%u",eKRTEtwoCodeFiles);
	r2->AddKey(str,FSRTstring)->stringwrite("Trying to load 2 code files!");
	sprintf(str,"%u",eKRTEinvalidFileVersion);
	r2->AddKey(str,FSRTstring)->stringwrite("Invalid File Version!");
	sprintf(str,"%u",eKRTEfailedToLoadEngineOrServices);
	r2->AddKey(str,FSRTstring)->stringwrite("Failed to load game engine or services!");


	// CONVERSION:
	sprintf(str,"%u",eKRTEptrtonum);
	r2->AddKey(str,FSRTstring)->stringwrite("Converting pointer-type param to numeric-type param.");
	sprintf(str,"%u",eKRTEnumtoptr);
	r2->AddKey(str,FSRTstring)->stringwrite("Converting numeric-type param to pointer-type param.");
	sprintf(str,"%u",eKRTEptrconv);
	r2->AddKey(str,FSRTstring)->stringwrite("Bad conversion of pointer-type parameter");
	sprintf(str,"%u",eKRTEstrtonum);
	r2->AddKey(str,FSRTstring)->stringwrite("Converting string to numeric or pointer-type param.");
	sprintf(str,"%u",eKRTEnumtostr);
	r2->AddKey(str,FSRTstring)->stringwrite("Converting numeric or pointer-type param to string.");
	sprintf(str,"%u",eKRTEarrayconv);
	r2->AddKey(str,FSRTstring)->stringwrite("Converting Arrays to Different type");
	sprintf(str,"%u",eKRTEstringError);
	r2->AddKey(str,FSRTstring)->stringwrite("Error in string. End Null not found");
	sprintf(str,"%u",eKRTEuserConversion);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Prm Conversion Error");
	sprintf(str,"%u",eKRTEsavingOPtrNotAllowed);
	r2->AddKey(str,FSRTstring)->stringwrite("Data objects cannot save pointers to normal objects. Null saved instead.");



	// Assign:
	sprintf(str,"%u",eKRTEmenyToOne);
	r2->AddKey(str,FSRTstring)->stringwrite("Passing many arguments to one.");
	sprintf(str,"%u",eKRTEretMenyToOne);
	r2->AddKey(str,FSRTstring)->stringwrite("Returning many arguments to one.");
	sprintf(str,"%u",eKRTEfceNotReturning);
	r2->AddKey(str,FSRTstring)->stringwrite("Called Function Doesn't return a value.");
	sprintf(str,"%u",eKRTEnothingRetInArg);
	r2->AddKey(str,FSRTstring)->stringwrite("Nothing was returned in this Argument.");
	sprintf(str,"%u",eKRTEretTypeChanged);
	r2->AddKey(str,FSRTstring)->stringwrite("Type of return Function was changed.");
	sprintf(str,"%u",eKRTEuserAssignation);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Prm Assignation Error.");
	sprintf(str,"%u",eKRTEcastingOffConst);
	r2->AddKey(str,FSRTstring)->stringwrite("You cannot assign reference to a constant object to a reference to non constant object.");



	// Call:
	sprintf(str,"%u",eKRTESCnoObj);
	r2->AddKey(str,FSRTstring)->stringwrite("Safe Inmediate Call: Calling object doesnt exist.");
	sprintf(str,"%u",eKRTEBadMethod);
	r2->AddKey(str,FSRTstring)->stringwrite("Calling Bad Method. Name is probably Null");
	sprintf(str,"%u",eKRTEuserCallingE);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Calling Error");

	// Error:
	sprintf(str,"%u",eKRTEInvalidObjType);
	r2->AddKey(str,FSRTstring)->stringwrite("Constructing Object of unknown or invalid Type.");
	sprintf(str,"%u",eKRTEELoadingScripts);
	r2->AddKey(str,FSRTstring)->stringwrite("Error while loading Scripts");
	sprintf(str,"%u",eKRTEIllegalFree);
	r2->AddKey(str,FSRTstring)->stringwrite("Freeing illegal pointer!");
	sprintf(str,"%u",eKRTEVarLoad);
	r2->AddKey(str,FSRTstring)->stringwrite("Unable to Load Variable");
	sprintf(str,"%u",eKRTECOnoObj);
	r2->AddKey(str,FSRTstring)->stringwrite("Copying not Existing Object");
	sprintf(str,"%u",eKRTEarrayAccErr);
	r2->AddKey(str,FSRTstring)->stringwrite("Invalid acces to Array");
	sprintf(str,"%u",eKRTEarrayIsNull);
	r2->AddKey(str,FSRTstring)->stringwrite("Accessing Null (empty) array");
	sprintf(str,"%u",eKRTEaccessingNEarray);
	r2->AddKey(str,FSRTstring)->stringwrite("Accessing Not existing or Invalid Array");
	sprintf(str,"%u",eKRTEsaveLoadNotAllowed);
	r2->AddKey(str,FSRTstring)->stringwrite("SaveLoad Operation is not allowed here.");
	sprintf(str,"%u",eKRTEuserError);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Error");
	sprintf(str,"%u",eKRTESLwriteInMiddle);
	r2->AddKey(str,FSRTstring)->stringwrite("This type has a variable length. You cannot write it in the middle of the stream.");
	sprintf(str,"%u",eKRTESLloadingVar);
	r2->AddKey(str,FSRTstring)->stringwrite("Manual Load of Variable failed. Invalid type or end of source stream.");
	sprintf(str,"%u",eKRTEstaticLoad);
	r2->AddKey(str,FSRTstring)->stringwrite("Unable to Load Object. (object not found in level)");
	sprintf(str,"%u",eKRTEobjectLoad);
	r2->AddKey(str,FSRTstring)->stringwrite("Unable to load Object (object not found in scripts)");
	sprintf(str,"%u",eKRTEsavingOptrToNoSavO);
	r2->AddKey(str,FSRTstring)->stringwrite("Saving Pointer to Object, that will not be saved (pointer converted to Null)");
	sprintf(str,"%u",eKRTEsavingGlobalObjPtr);
	r2->AddKey(str,FSRTstring)->stringwrite("You Cannot Save Obj pointer in a Global Variable");
	sprintf(str,"%u",eKRTEsavingLIOErr);
	r2->AddKey(str,FSRTstring)->stringwrite("Error writing Level to Disk (bad path, disk full, access denied ..?)");
	sprintf(str,"%u",eKRTEinvalidEditType);
	r2->AddKey(str,FSRTstring)->stringwrite("This is not valid modifier for editable item");
	sprintf(str,"%u",eKRTEgameNotSaved);
	r2->AddKey(str,FSRTstring)->stringwrite("Error while saving Game. Game NOT saved.");
	sprintf(str,"%u",eKRTEaccessingNullObject);
	r2->AddKey(str,FSRTstring)->stringwrite("Accessing null object.");
	sprintf(str,"%u",eKRTEindexOutOfRange);
	r2->AddKey(str,FSRTstring)->stringwrite("Array index is out of range.");
	sprintf(str,"%u",eKRTEarrayIsLocked);
	r2->AddKey(str,FSRTstring)->stringwrite("Array Is Locked.");
	sprintf(str,"%u",eKRTEnullIsNot0);
	r2->AddKey(str,FSRTstring)->stringwrite("Null has to be 0.");
	sprintf(str,"%u",eKRTEunknownFileType);
	r2->AddKey(str,FSRTstring)->stringwrite("Unknown file type.");
	sprintf(str,"%u",eKRTEksidTypeConflict);
	r2->AddKey(str,FSRTstring)->stringwrite("Conflict in type of KSID name.");
	sprintf(str,"%u",eKRTEksidLTConflict);
	r2->AddKey(str,FSRTstring)->stringwrite("Conflict in Language Type of KSID name.");
	sprintf(str,"%u",eKRTEconstantValueConflict);
	r2->AddKey(str,FSRTstring)->stringwrite("Conflict while loading a constant value.");
	sprintf(str,"%u",eKRTEdataObjectDoesntExist);
	r2->AddKey(str,FSRTstring)->stringwrite("Data Object of this name doesn't exist.");



	// Warning:
	sprintf(str,"%u",eKRTENoError);
	r2->AddKey(str,FSRTstring)->stringwrite("No Error.");
	sprintf(str,"%u",eKRTEFreeToNull);
	r2->AddKey(str,FSRTstring)->stringwrite("Freeing NULL pointer.");
	sprintf(str,"%u",eKRTEarrAddedNDef);
	r2->AddKey(str,FSRTstring)->stringwrite("Array was expanded using Non Defined values. (accesing far above end)");
	sprintf(str,"%u",eKRTEuserWarning);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Warning");
	sprintf(str,"%u",eKRTEKeyNameExpected);
	r2->AddKey(str,FSRTstring)->stringwrite("IsKeyDown: This Isn't a name for Key Input");
	sprintf(str,"%u",eKRTESoundNameExpected);
	r2->AddKey(str,FSRTstring)->stringwrite("PlaySound: This Isn't a name for Sound");

	// Map Errors:
	sprintf(str,"%u",eKRTEuserMapError);
	r2->AddKey(str,FSRTstring)->stringwrite("User's Map Error");
	sprintf(str,"%u",eKRTEplacingOutOfMap);
	r2->AddKey(str,FSRTstring)->stringwrite("Placing Object out of Map (nothing placed)");
	sprintf(str,"%u",eKRTEplacedOhNoGraphic);
	r2->AddKey(str,FSRTstring)->stringwrite("Placing Object with No Graphic");
	sprintf(str,"%u",eKRTEmovingOutOfMap);
	r2->AddKey(str,FSRTstring)->stringwrite("Moving Object out of Map (move canceled)");

	// J.M.: chyby interpretu
	sprintf(str,"%u",eKRTEIllegalAddressRead);
	r2->AddKey(str,FSRTstring)->stringwrite("Cteni mimo pamet interpretu");
	sprintf(str,"%u",eKRTEInstrOutsideCode);
	r2->AddKey(str,FSRTstring)->stringwrite("Pokus o provedeni instrukce mimo oblast kodu");
	sprintf(str,"%u",eKRTEUnknownInstr);
	r2->AddKey(str,FSRTstring)->stringwrite("Pokus o provedeni nezname instrukce ");
	sprintf(str,"%u",eKRTEIllegalAddressWrite);
	r2->AddKey(str,FSRTstring)->stringwrite("Zapis mimo pamet interpretu");
	sprintf(str,"%u",eKRTEAddrNotAlligned);
	r2->AddKey(str,FSRTstring)->stringwrite("Adresa neni zarovnana na 4 B");
	sprintf(str,"%u",eKRTEStackOverflow);
	r2->AddKey(str,FSRTstring)->stringwrite("Preteceni zasobniku");
	sprintf(str,"%u",eKRTEHeapOverflow);
	r2->AddKey(str,FSRTstring)->stringwrite("Preteceni haldy");
	sprintf(str,"%u",eKRTEStackUnderflow);
	r2->AddKey(str,FSRTstring)->stringwrite("Podteceni zasobniku");
	sprintf(str,"%u",eKRTEDivideByZero);
	r2->AddKey(str,FSRTstring)->stringwrite("Deleni nulou");
	sprintf(str,"%u",eKRTEOutsideTmpStack);
	r2->AddKey(str,FSRTstring)->stringwrite("Pristup mimo tmpStack");
	sprintf(str,"%u",eKRTECorruptedInstr);
	r2->AddKey(str,FSRTstring)->stringwrite("eKRTECorruptedInstr");
	sprintf(str,"%u",eKRTETooLongExecution);
	r2->AddKey(str,FSRTstring)->stringwrite("Zacykleni interpretu, resp. timeout");

	r->WriteFile();
	delete r;

	CFS::DoneFS();

	return 0;
}

