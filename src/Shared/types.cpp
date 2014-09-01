/////////////////////////////////////////////////////////////////////////////
//
// types.cpp
//
// typy a makra pouzivane v KRKALovi
//
// A: Petr Altman
//
/////////////////////////////////////////////////////////////////////////////
#include "stdafx.h"

#include "types.h"
#include "fs.api.h"

#include <stdio.h> 
#include <stdarg.h>

//#define suprdebuk
void MyDebug(char* file, char* str, int i, int info)
{
#ifdef suprdebuk
	FS->ChangeDir("$COMPFILES$");
	FILE* f = fopen(file, "a");
	fprintf(f, "%s  %d", str, i);
	if(info != -1)
		fprintf(f, "   (%d)", info);
	fprintf(f, "\n");
	fclose(f);
#endif
}

void MyDebugTab(char* file, int tabs)
{
#ifdef suprdebuk
	FS->ChangeDir("$COMPFILES$");
	FILE* f = fopen(file, "a");
	while(tabs--)
		fprintf(f, "\t");
	fclose(f);
#endif
}

void MyDebugStr(char* file, char* str)
{
#ifdef suprdebuk
	FS->ChangeDir("$COMPFILES$");
	FILE* f = fopen(file, "a");
	fprintf(f, "%s", str);
	fclose(f);
#endif
}

char *newstrdup(const char *str)
{
	char *s;

	if (!str) return 0;

	if (s = new char[strlen(str) + 1] )
			return strcpy(s,str);

	return 0;
}

void ToUpper(std::string &s) {
	for (size_t f=0; f < s.size(); f++) {
		s[f] = toupper(s[f]);
	}
}

#pragma managed(push, off)

CExc::CExc(int etype, int par, char *format, ... )
{

	va_list list;
	
	va_start( list, format ); 
	_vsnprintf( errstr,sizeof(errstr), format, list );
	va_end( list );

	errnum = etype;
	param=par;

}


#pragma managed(pop)



