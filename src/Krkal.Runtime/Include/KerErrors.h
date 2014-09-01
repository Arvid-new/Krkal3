//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - E R R O R S
///
///		Logovani a hlaseni behovych chyb
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////

#pragma once

#ifndef KERERRORS_H
#define KERERRORS_H

#include "OPointer.h"


struct CKerContext;
class CKerErrors;
class CKerErrorFiles;
class CFSRegister;
class CFSRegKey;
class CKerMain;





///////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////
///
///		C K e r E r r o r L o g   a   C K e r E r r o r S t a c k
///
//////////////////////////////////////////////////////////////////////////////

// Spojak, ktery se pridava ke ErrorLogu. Absahuje historii volani 
// (CallStack), vse, ce se da z Kontextu vycist
struct KRKALRUNTIME_API CKerErrorStack {
	// Konstuktor vytvori spojak a okopiruje do nej informace z Kontextu
	CKerErrorStack(CKerContext *ctx, int pocet);
	// Destruktor cely spojak zrusi
	~CKerErrorStack() {
		SAFE_DELETE(parent);
	}
	const char *MethodName; // KSM Jmeno metody
	const char *MsgMethodName; // Jmeno metody odkud byla zavolana zprava
	int line; // pozice ve zdrojaku. Relativni vzhledem k metode
	int msgline; // radka, odkud byla zavolana zprava
	OPointer thisO, Sender;
	CKerName *MName; // KSID jmeno metody
	CKerName *ThisName, *SenderName; // KSID jmena
	CKerErrorStack *parent;
};




//////////////////////////////////////////////
///		Struktura jednoho Logu
class KRKALRUNTIME_API CKerErrorLog { 
public:
	CKerErrorLog(CKerMain *KerMain);
	~CKerErrorLog() { 
		SAFE_DELETE(stack);
		SAFE_DELETE_ARRAY(ErrorStr);
	}
	int time; // cas vzniku chyby od zacatku levlu (behem kola je cas stejny)
	int ErrorNum; // cislo chyby;
	int ErrorParam; // promenne info o chybe
	char *ErrorStr; // String, volitelna zprava
	CKerErrorStack *stack; // CallStack
	CKerErrorLog *next, *prev; // Obousmerny spojak vsech logu
	CKerErrorLog *nextig; // Jednosmerna fronta logu stejne skupiny
	int showed; // zda byl vykreslen v CKerRunLogWindow
};




//////////////////////////////////////////////////////////////////////
///
///		C K e r E L o g Q u e e
///		Konfigurace Skupiny Erroru, a jeji prislusna fronta
///
//////////////////////////////////////////////////////////////////////

class KRKALRUNTIME_API CKerELogQuee {
	friend CKerErrors;
	CKerELogQuee() {
		start=0; end=0;
		counter=0;
		terminateKer = 0;
		max=100;
		error=1;
	}
	CKerErrorLog *start;
	CKerErrorLog *end;
	int counter; // pocet erroru ve skupine (citac jede do 1000000000)
	int max; // maximalni velikost logu - pokud je rovna 0, errory se neloguji
	int terminateKer; // 0 - ne, 1 - ihned, 2 - po zaplneni logu
	int error;	// pokud v teto fronte neco bylo, tak nastavim pri ukladani logu promennou WasError
};






////////////////////////////////////////////////////////////////////////
///
///		C K e r L o g W i n d o w
///		Okno pro prohlizeni logu s errory
///
////////////////////////////////////////////////////////////////////////

//// Okno pro zobrazeni logu z registru
//class CKerLogWindow : public CGUIStdWindow {
//public:
//	CKerLogWindow(float _x, float _y, float _sx, float _sy, char* title );
//	virtual ~CKerLogWindow();
//	int MouseLeft(float x, float y, UINT mouseState, UINT keyState);	
//};
//
//
//// Okno ktere zobrazuje aktualni stav logu za behu. (poslednich n radku) Je aktualizovano 3 krat za sekundu
//class CKerRunLogWindow : public CGUIStdWindow {
//public:
//	CKerRunLogWindow(int _NumLogs, float _x, float _y, float _sx, float _sy, char* title );
//	virtual ~CKerRunLogWindow();
//	int MouseLeft(float x, float y, UINT mouseState, UINT keyState);
//	virtual int TimerImpulse(typeID timerID, float time); // Aktualizacni a vykreslovaci fce
//private:
//	int NumLogs;  // pocet radku
//	CFSRegister *edes;  // odkas na registr s popisem erroru
//};





////////////////////////////////////////////////////////////////////////
///
///		C K e r E r r o r F i l e s
///		Objekt, ktery uchovava otevrene soubory s popisy erroru
///		a s logy. Umoznuje zobrazovani logu, bez Kernelu
///		Pouziti: Vytvorit Pri startu krkala (pred kernelem), Pri ukoncovani zrusit
///			Objekt bude ulozen v promenne KerErrorFiles
///
////////////////////////////////////////////////////////////////////////

//class CKerErrorFiles {
//	friend CKerErrors;
//	friend CKerLogWindow;
//	friend CKerRunLogWindow;
//public:
//	CKerErrorFiles() {
//		err_file = 0;  err_log = 0;	 LogWindow = 0;	WasError = 0;
//		if (KerErrorFiles) throw CExc(eKernel,0,"Error - Pokus Vytvorit Objekt CKerErrorFiles dvakrat!");
//		KerErrorFiles=this;
//	}
//	~CKerErrorFiles();
//	void ShowAllLogs(float _x, float _y, float _sx, float _sy); // zobrazi log ve wokne. 
//		// Bere log z err_file - tzv neaktualni log. Pro aktualizaci zavolej KerMain->Errors->saveLogsToFile()
//		// Pokud je err_file prazdna, fce se pokusi nacist log ze souboru
//	int WasError;	// zda v poslednim logu, ktery se ukladal byl nejaky error
//private:
//	CKerLogWindow *LogWindow;  // Prislusne wokno
//	CFSRegister *err_file;	// registr s popisy erroru. zavira se v destruktoru
//	CFSRegister *err_log;	// registr s naposledy vytvorenym logem. Vytvari a sejvuje se
//							// v SaveLogsToFile, rusi se v destruktoru, nebo v SLTF, pokud existoval
//};



///////////////////////////////////////////////////////////////////////
///
///		C K e r E r r o r s
///		Hlavni objekt pro praci s Errory
///
///////////////////////////////////////////////////////////////////////


class KRKALRUNTIME_API CKerErrors {
//	friend CKerRunLogWindow;
public:
	CKerErrors(CKerMain *kerMain, eKerDebugModes DebugMode); // Nakonfiguruje chovani jednotlivych skupin E (front)
	~CKerErrors();
	// Hlaseni erroru: Posledni dva parametry jsou nepovinne (dodatecna informace)
	// Line urcuje cislo radky, kde error vznikl. Kdyz nezadana, bude radka zjistena z kontextu
	// V kontextu je KSM jmeno metody, radka se pocita relativne k teto metode.
	inline void LogError2(int Line, int errornum, int errorparam = 0, const char *errorstr = 0); 
	void LogError(int errornum, int errorparam = 0, const char *errorstr = 0);
//	void SaveLogsToFile(char *path = "$DEFERRLOG$"); // ulozi log do registru a na disk. Registr zustava otevreny, pro dalsi potrebu.
//	CKerRunLogWindow* ShowRunLogWindow(int _NumLogs, float _x, float _y, float _sx, float _sy); // Zobrazi okno s aktualnimi logy. Log je prubezne aktualizovan.
	CKerErrorLog *start, *end; // obousmerny seznam vsech error logu
private:
	CKerMain *KerMain;
	CKerELogQuee Logs[16]; // 16 skupin erroru. V CKerLogQuee je popsano prislusne chovani
	void SaveOneLog(CFSRegister *r, CKerErrorLog *log, CFSRegister *edes); // Pomocna fce pro SaveLogsToFile
//	CKerRunLogWindow *LogWindow;  
};



#endif






