//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - K e r E r r o r N u m b e r s
///
///		Error numbers
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef __KER_ERROR_NUMBERS_H__
#define __KER_ERROR_NUMBERS_H__

#include "Krkal.Runtime.h"


// Skupiny erroru. Ke kazde skupine se Kernel chova jinak. Da se konfigurovat
// podle zvoleni DebugModu.
enum eKerErrGroups {
	eKEGPanicError=0,	// Po nahlaseni erroru z teto skupiny se Kernel shodi
	eKEGFatalError=1,	// Zavazne chyby. Kernel se neukonci, ale rozhodne by nemely nastavat. K ukonceni dojde po zaplneni logu.
	eKEGError=2,
	eKEGWarning=3,
	eKEGInfo=4,			// Info kernelu - nejedna se o error	
	eKEGParamConversionError=5,
	eKEGParamAssignationError=6,
	eKEGCallingError=7,
	eKEGDebug=8,			// Uzivatelska skupina
	eKEGMapError=9,
};


#define KERMAXERRORCOUNT 1000000000 // Cislo, do ktereho jede citac logu
#define KER_MAX_CALLSTACK_LOG 20	// Maximalni velikost logovaneho zasobniku
#define KERGROUPSHIFT 16			// Makra na spojovani a rozpojovani cisla erroru a skupiny:
#define KERGETERRGROUP(err) ((err)>>KERGROUPSHIFT)  // vrati skupinu
#define KERGETERRSNUM(err) ((err)&0x0000FFFF)		// vrati cislo v ramci skupiny
#define KERGETERRNUM(group,num) (((group)<<(KERGROUPSHIFT))+(num))  // spoji skupinu s cislem v ramci skupiny


// ERRORY:
enum eKerRTErrors {
	// Fatals:
	eKRTEDCNoObj = KERGETERRNUM(eKEGFatalError,1), // Directly Calling Method of Not Existing Object
	eKRTEDCBadObj = KERGETERRNUM(eKEGFatalError,2), // Direct Call: This Object doesnt heve called method!
	eKRTEobjcount1 = KERGETERRNUM(eKEGFatalError,3), // Too meny objects was created. Object Counter is 0x10000000
	eKRTEobjcount2 = KERGETERRNUM(eKEGFatalError,4), // Too meny objects was created. Object Counter is 0x80000000
	eKRTEobjcount3 = KERGETERRNUM(eKEGFatalError,5), // Too meny objects was created. Object Counter is 0xffffff00
	eKRTEcallstackw = KERGETERRNUM(eKEGFatalError,6), // 
	eKRTEmessagew = KERGETERRNUM(eKEGFatalError,7), // 
	eKRTEmapNotRegistered = KERGETERRNUM(eKEGFatalError,8), // Map Not Registered!
	eKRTEautoNotFound = KERGETERRNUM(eKEGFatalError,9),  // No Automatic Grafic Found for Object
	eKRTEuserFatal = KERGETERRNUM(eKEGFatalError,10), // User's Fatel Error
	eKRTEnotValidName = KERGETERRNUM(eKEGFatalError,21), // Invalid Name. The variable is probably undefined.
	eKRTEcycleInbjPtrs = KERGETERRNUM(eKEGFatalError,22), // Saving Levels: There is a cycle in obj Pointers!

	// Infos:
	eKRTEKernelInit = KERGETERRNUM(eKEGInfo,1), // Kernel Starts!
	eKRTEPaniCInfo = KERGETERRNUM(eKEGInfo,2), // Kernel Panic: Kernel is shutting Down immediately!!
	eKRTELoadingLevel = KERGETERRNUM(eKEGInfo,3), // Kernel is loading level:
	eKRTELoadingScript = KERGETERRNUM(eKEGInfo,4), // Kernel is loading scripts. Version:
	eKRTELoadComplete = KERGETERRNUM(eKEGInfo,5), // ...Loading Complete.
	eKRTECompilating = KERGETERRNUM(eKEGInfo,6), // Scripts need to be recompiled. Recompiling..
	eKRTEKernelEnds = KERGETERRNUM(eKEGInfo,7), // Kernel is shutting down ..
	eKRTEsavingLevel = KERGETERRNUM(eKEGInfo,8), // Saving Level ...
	eKRTEsaveLOK = KERGETERRNUM(eKEGInfo,9), // Save Successful!
	eKRTEuserInfo = KERGETERRNUM(eKEGInfo,10), // User's Info
	eKRTEgameVictory = KERGETERRNUM(eKEGInfo,21), // You are Victorious!! Congratulations!! :)
	eKRTEloadingGame = KERGETERRNUM(eKEGInfo,22), // Kernel is loading saved game:
	eKRTEsavingGame = KERGETERRNUM(eKEGInfo,23), // Saving Game ...
	eKRTEgarbageCollector = KERGETERRNUM(eKEGInfo,24), // Garbage Collector:
	eKRTEloadingDataFile = KERGETERRNUM(eKEGInfo,25), // Kernel is loading data file:
	eKRTEsavingDataFile = KERGETERRNUM(eKEGInfo,26), // Saving Data File ...

	// Panics:
	eKRTEPanic = KERGETERRNUM(eKEGPanicError,1),	// Kernel Panic z neznameho duvodu :-)
	eKRTENoMemory = KERGETERRNUM(eKEGPanicError,2), // Out Of Skript Stack Memory!!
	eKRTEDelObjInUse = KERGETERRNUM(eKEGPanicError,3), // Deleting Object In Use!! (Dectructor called in incorrect time)
	eKRTEobjcountOVERFLOW = KERGETERRNUM(eKEGPanicError,4), // Object Counter is 0xffffffff. Object counter has overwlowed!!!
	eKRTEPELoadingScripts = KERGETERRNUM(eKEGPanicError,5), // Error while loading Scripts
	eKRTEPELoadingLevel = KERGETERRNUM(eKEGPanicError,6),   // Error while loading Level
	eKRTEOutOfTime = KERGETERRNUM(eKEGPanicError,7),   // Time Counter has Overflowed!
	eKRTEcallstack = KERGETERRNUM(eKEGPanicError,8), // 
	eKRTEmessage = KERGETERRNUM(eKEGPanicError,9), // 
	eKRTEuserPanic = KERGETERRNUM(eKEGPanicError,10), // User's Panic Error
	eKRTECompilationFailed = KERGETERRNUM(eKEGPanicError,31), // Compilation Failed
	eKRTEpictureNotLoaded = KERGETERRNUM(eKEGPanicError,32), // Failed to load Picture!
	eKRTEcyclusInKSID = KERGETERRNUM(eKEGPanicError,33),	// There is a cycle in KSID names!
	eKRTEerrorInAuto = KERGETERRNUM(eKEGPanicError,34),		// Automatic graphic is corrupted. Check the file.
	eKRTEmapNotWorking = KERGETERRNUM(eKEGPanicError,35),		// Objet Map is not working correctly
	eKRTEdeletingBadArray = KERGETERRNUM(eKEGPanicError,36),	// deleting Not existing or Invalid Kernel Array!
	eKRTEobjPtrConflict = KERGETERRNUM(eKEGPanicError,37),	// There are two same, conflicting ObjPtrs!
	eKRTEinvLvlVersion = KERGETERRNUM(eKEGPanicError,38),	// The level version doesn't match! Hack attempt?
	eKRTEdllLoadFailed = KERGETERRNUM(eKEGPanicError,39),	// Failed to load dll with script!
	eKRTEundefinedKnownClass = KERGETERRNUM(eKEGPanicError,40), // Known class name was not defined by the compiler!
	eKRTEfunctionDoesntExist = KERGETERRNUM(eKEGPanicError,41), // Function doesn't exist!
	eKRTEfscriptEntryPointnotFound = KERGETERRNUM(eKEGPanicError,42), // Script entry point not found.
	eKRTEwrongLoadingParameters = KERGETERRNUM(eKEGPanicError,43), // Error while loading Kernel. Invalid parameter.
	eKRTEtwoCodeFiles = KERGETERRNUM(eKEGPanicError,44), // Trying to load 2 code files!
	eKRTEinvalidFileVersion = KERGETERRNUM(eKEGPanicError,45), // Invalid File Version!
	eKRTEfailedToLoadEngineOrServices = KERGETERRNUM(eKEGPanicError,46), // Failed to load game engine or services!
	
	// Conversion:
	eKRTEptrtonum = KERGETERRNUM(eKEGParamConversionError,1), // Chybna konverze z ptr typu na numericke typy
	eKRTEnumtoptr = KERGETERRNUM(eKEGParamConversionError,2), // Chybna konverze z nemerickeho typu na ptr typ
	eKRTEptrconv = KERGETERRNUM(eKEGParamConversionError,3), // Chybna konverze ptr typu
	eKRTEstrtonum = KERGETERRNUM(eKEGParamConversionError,4), // Pokus o konverzi stringu do numerickych nebo ptr typu
	eKRTEnumtostr = KERGETERRNUM(eKEGParamConversionError,5), // Pokus o konverzi numerickeho nebo ptr typu na string 
	eKRTEarrayconv = KERGETERRNUM(eKEGParamConversionError,6), // Chyba pri konverzi promenne typu array
	eKRTEstringError = KERGETERRNUM(eKEGParamConversionError,7), // Error in string. End Null not found
	eKRTEuserConversion = KERGETERRNUM(eKEGParamConversionError,10), // User's Prm Conversion Error
	eKRTEsavingOPtrNotAllowed = KERGETERRNUM(eKEGParamConversionError,11), // Data objects cannot save pointers to normal objects. Null saved instead.

	// Assign:
	eKRTEmenyToOne = KERGETERRNUM(eKEGParamAssignationError,1), // Predavani vice argumentu do jednoho
	eKRTEretMenyToOne = KERGETERRNUM(eKEGParamAssignationError,2), // Vraceni vice argumentu do jednoho
	eKRTEfceNotReturning = KERGETERRNUM(eKEGParamAssignationError,3), // Called Function Doesn't return a value.
	eKRTEnothingRetInArg = KERGETERRNUM(eKEGParamAssignationError,4), // Nothing was returned in this Argument.
	eKRTEretTypeChanged = KERGETERRNUM(eKEGParamAssignationError,5), // Type of return Function was changed
	eKRTEcastingOffConst = KERGETERRNUM(eKEGParamAssignationError,6), // You cannot assign reference to a constant object to a reference to non constant object.
	eKRTEuserAssignation = KERGETERRNUM(eKEGParamAssignationError,10), // User's Prm Assignation Error

	// Call:
	eKRTESCnoObj = KERGETERRNUM(eKEGCallingError,1), // Error pri primem SAFE volani. Neexistuje volany objekt!
	eKRTEBadMethod = KERGETERRNUM(eKEGCallingError,2), // Calling Bad Method
	eKRTEuserCallingE = KERGETERRNUM(eKEGCallingError,10), // User's Calling Error

	// Error:
	eKRTEInvalidObjType = KERGETERRNUM(eKEGError,1), // Constructing Object of unknown or invalid Type.
	eKRTEELoadingScripts = KERGETERRNUM(eKEGError,2), // Error while loading Scripts
	eKRTEIllegalFree = KERGETERRNUM(eKEGError,3), // Freeing illegal pointer
	eKRTEVarLoad = KERGETERRNUM(eKEGError,4),		// Unable to Load Variable	
	eKRTECOnoObj = KERGETERRNUM(eKEGError,5),		// Copying not Existing Object
	eKRTEarrayAccErr = KERGETERRNUM(eKEGError,6),	// Chybny pristup do dynamickeho pole
	eKRTEarrayIsNull = KERGETERRNUM(eKEGError,7),	// pointer je Null
	eKRTEaccessingNEarray = KERGETERRNUM(eKEGError,8),	// Accessing Not existing or Invalid Array
	eKRTEsaveLoadNotAllowed = KERGETERRNUM(eKEGError,9),  // SaveLoad Operation is not allowed here.
	eKRTEuserError = KERGETERRNUM(eKEGError,10), // User's Error
	eKRTESLwriteInMiddle = KERGETERRNUM(eKEGError,21), // This type has a variable length. You cannot write it in the middle of the stream.
	eKRTESLloadingVar = KERGETERRNUM(eKEGError,22), // Manual Load of Variable failed. Invalid type or end of source stream.
	eKRTEstaticLoad = KERGETERRNUM(eKEGError,23), // Unable to Load Static Object. (object not found in level)
	eKRTEobjectLoad = KERGETERRNUM(eKEGError,24), // Unable to load Object (object not found in scripts)
	eKRTEsavingOptrToNoSavO = KERGETERRNUM(eKEGError,25), // Saving Pointer to Object, that will not be saved (pointer converted to Null)
	eKRTEsavingGlobalObjPtr = KERGETERRNUM(eKEGError,26), // You Cannot Save Obj pointer in a Global Variable
	eKRTEsavingLIOErr = KERGETERRNUM(eKEGError,27),  // Error writing Level to Disk (bad path, disk full, access denied ..?)
	eKRTEbadKeyName = KERGETERRNUM(eKEGError,28),		// This Cannot be a name for key Input!
	eKRTEbadSoundName = KERGETERRNUM(eKEGError,29),		// This Cannot be a name for Sound!
	eKRTEinvalidEditType = KERGETERRNUM(eKEGError,30),	// This is not valid modifier for editable item
	eKRTEgameNotSaved = KERGETERRNUM(eKEGError,31),		// Error while saving Game. Game NOT saved.
	eKRTEaccessingNullObject = KERGETERRNUM(eKEGError,32),		// Accessing null object.
	eKRTEindexOutOfRange = KERGETERRNUM(eKEGError,33),		// Array index is out of range.
	eKRTEarrayIsLocked = KERGETERRNUM(eKEGError,34),		// Array Is Locked.
	eKRTEnullIsNot0 = KERGETERRNUM(eKEGError,35),		// Null has to be 0.
	eKRTEunknownFileType = KERGETERRNUM(eKEGError,36),		// Unknown file type.
	eKRTEksidTypeConflict = KERGETERRNUM(eKEGError,37),		// Conflict in type of KSID name.
	eKRTEksidLTConflict = KERGETERRNUM(eKEGError,38),		// Conflict in Language Type of KSID name.
	eKRTEconstantValueConflict = KERGETERRNUM(eKEGError,39),		// Conflict while loading a constant value.
	eKRTEdataObjectDoesntExist = KERGETERRNUM(eKEGError,40),		// Data Object of this name doesn't exist.

	
	// Warnings:
	eKRTENoError = KERGETERRNUM(eKEGWarning,0),
	eKRTEFreeToNull = KERGETERRNUM(eKEGWarning,1), // Freeing NULL pointer.
	eKRTEarrAddedNDef = KERGETERRNUM(eKEGWarning,2), // Array was expanded using Non Defined values. (accesing far above end)
	eKRTEuserWarning = KERGETERRNUM(eKEGWarning,10), // User's Warning
	eKRTEKeyNameExpected = KERGETERRNUM(eKEGWarning,3),		// This Isn't a name for Key Input
	eKRTESoundNameExpected = KERGETERRNUM(eKEGWarning,4),		// This Isn't a name for Sound

	// MapErrors:
	eKRTEuserMapError = KERGETERRNUM(eKEGMapError,10), // User's Map Error
	eKRTEplacingOutOfMap = KERGETERRNUM(eKEGMapError,1), // Placing Object out of Map
	eKRTEplacedOhNoGraphic = KERGETERRNUM(eKEGMapError,2), // Placed Object has No graphic
	eKRTEmovingOutOfMap = KERGETERRNUM(eKEGMapError,3), // Moving Object out of Map

	// J.M.: chyby interpretu
	eKRTEIllegalAddressRead = KERGETERRNUM(eKEGPanicError,11),	// Cteni mimo pamet interpretu
	eKRTEInstrOutsideCode = KERGETERRNUM(eKEGPanicError,12),		// Pokus o provedeni instrukce mimo oblast kodu
	eKRTEUnknownInstr= KERGETERRNUM(eKEGPanicError,13),			// Pokus o provedeni nezname instrukce 
	eKRTEIllegalAddressWrite = KERGETERRNUM(eKEGPanicError,14),	// Zapis mimo pamet interpretu
	eKRTEAddrNotAlligned = KERGETERRNUM(eKEGPanicError,15),		// Adresa neni zarovnana na 4 B
	eKRTEStackOverflow = KERGETERRNUM(eKEGPanicError,16),		// Preteceni zasobniku
	eKRTEHeapOverflow = KERGETERRNUM(eKEGPanicError,17),			// Preteceni haldy
	eKRTEStackUnderflow = KERGETERRNUM(eKEGPanicError,18),		// Podteceni zasobniku
	eKRTEDivideByZero = KERGETERRNUM(eKEGPanicError,19),			// Deleni nulou
	eKRTEOutsideTmpStack = KERGETERRNUM(eKEGPanicError,20),			// Pristup mimo tmpStack
	eKRTECorruptedInstr = KERGETERRNUM(eKEGPanicError,21),
			
	eKRTETooLongExecution = KERGETERRNUM(eKEGError,11),			// Zacykleni interpretu, resp. timeout
};


// DobugMody:
enum eKerDebugModes {
	// Defaultni mody: (Pokud neni urceno jinak tak delka logu pro kazdou skupinu je 100)
	eKerDBRelease = 0,  // Loguji se jen FatalErrory a PanicErrory
	eKerDBDebug = 1		// Loguje se vse, FatalErroru muze byt jen deset
};


class KRKALRUNTIME_API CKernelPanic { // Pro hazeni vyjimek - ukonci kernel
};

class KRKALRUNTIME_API CKernelError : public CKernelPanic
{
public:
	CKernelError(int errorNum) { ErrorNum = errorNum;}
	int ErrorNum;
};



#endif