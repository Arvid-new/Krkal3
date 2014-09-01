//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - S E R V I C E S
///
///		Sluzby Kernelu, Interface Systemu Krkal pro skripty
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////

#pragma once

#ifndef KERSERVICESBACKEND_H
#define KERSERVICESBACKEND_H

#include "MersenneTwister.h"

class CKerName;


class CKerServicesBackend {
public:
	typedef std::vector<CKerName*> NamesT;
	
	MTRand mtr;
	NamesT NewNames;
	NamesT NewDataObjects;
	NamesT NewDependencies;

	void ForgetAllNewEntities() { NewNames.clear(); NewDataObjects.clear(); NewDependencies.clear(); }
};


#endif
