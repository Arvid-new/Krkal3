//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - L a n g u a g e T y p e
///
///		Class that represents the language type
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#pragma once

#ifndef LANGUAGETYPE_H
#define LANGUAGETYPE_H

#include "types.h"

class CKerName;

#include "Krkal.Runtime.h"

// typy zakladnich promennych
enum eKerTypes : UC {
	eKTunasigned,
	eKTvoid,
	eKTchar,
	eKTint,
	eKTdouble,
	eKTname,
	eKTobject,
	eKTnull,


	//eKTchar=0,		// unsigned char, 1B
	//eKTdouble,		// 8B
	//eKTint,			// signed int, 4B
	//eKTpointer,		// pointer, 4B, u SAFE volani zakazano
	//eKTobject,		// OPointer, 4B
	//eKTname,		// ID jmena, CKerName* , 4B
	//eKTvoid,		// pouziti jen u navratoveho typu funkce, 0B
	//eKTarrChar,		// dynamicke pole zakladnich typu
	//eKTarrDouble,
	//eKTarrInt,
	//eKTarrPointer,
	//eKTarrObject,
	//eKTarrName,
	//eKTstruct,
	//eKTstring=100,  // 100 az 350 jsou stringy velikosti 0 az 250
	//eKTret=512,		// pricte se k beznemu typu, kdyz jde o vraceni hodnotou
	//eKTretNormal	= 0,		// popis navratove funkce:
	//eKTretOR		= 1*1024,
	//eKTretAND		= 2*1024,
	//eKTretADD		= 3*1024,
	//eKTretMask		= 3*1024,	// maska, pomoci ktere z typu vyziskam navratovou funkci
	//eKTsubRetMask   = 1024-1,	// maska pro vse co je pod ret fci
};



enum eKerTypeModifier : unsigned short {
	eKTMnone		= 0,
	eKTMret			= 1,
	eKTMretOr		= 2,
	eKTMretAnd		= 4,
	eKTMretAdd		= 6,
	eKTMstatic		= 8,
	eKTMpublic		= 16,
	eKTMoverride	= 32,
	eKTMdirect		= 64,
	eKTMprotected	= 128,
	eKTMconstV		= 0x0100,
	eKTMconstO		= 0x0200,
	eKTMconstM		= 0x0400,
	//				= 0x0800,
	//				= 0x1000,
	eKTMconst3		= 0x2000,
	eKTMconst2		= 0x4000,
	eKTMconst1		= 0x8000,
};



struct KRKALRUNTIME_API CLT {

	CLT() {
		Type = eKTunasigned;
		ObjectType = 0;
		Modifier = eKTMnone;
		DimCount = 0;
	}

	CLT(eKerTypes type) {
		Type = type;
		ObjectType = 0;
		Modifier = eKTMnone;
		DimCount = 0;
	}

	CLT(eKerTypes type, eKerTypeModifier modifier, CKerName *objectType, UC dimCount) {
		Type = type;
		ObjectType = objectType;
		Modifier = modifier;
		DimCount = dimCount;
	}

	CLT(eKerTypes type, eKerTypeModifier modifier) {
		Type = type;
		ObjectType = 0;
		Modifier = modifier;
		DimCount = 0;
	}

	CLT(UC type) {
		Type = (eKerTypes)type;
		ObjectType = 0;
		Modifier = eKTMnone;
		DimCount = 0;
	}

	CLT(UC type, unsigned short modifier, CKerName *objectType, UC dimCount) {
		Type = (eKerTypes)type;
		ObjectType = objectType;
		Modifier = (eKerTypeModifier)modifier;
		DimCount = dimCount;
	}


	size_t SizeOf() {
		if (DimCount == 0) {
			switch (Type) {
				case eKTchar: return sizeof(wchar_t);
				case eKTint: return sizeof(int);
				case eKTdouble: return sizeof(double);
				case eKTvoid: return 0;
				case eKTunasigned: return 0;
				default: return sizeof(void*);
			}
		} else {
			return sizeof(void*);
		}
	}

	bool IsRet() {
		return (Modifier & eKTMret);
	}

	bool IsUnasigned() {
		return (Type == eKTunasigned);
	}

	bool IsRetCalculable() {
		return (DimCount == 0 && (Type == eKTchar || Type == eKTint));
	}

	eKerTypeModifier RetFce() {
		return (eKerTypeModifier)(Modifier & (eKTMretOr|eKTMretAnd|eKTMretAdd));
	}

	eKerTypeModifier ConstPointer() {
		return (eKerTypeModifier)(Modifier & (eKTMconst1|eKTMconst2|eKTMconst3|eKTMconstO));
	}

	bool IsStaticConstant() {
		return (Modifier & eKTMstatic) && (Modifier & eKTMconstV);
	}

	bool operator == (const CLT &other) const {
		return (Type == other.Type && ObjectType == other.ObjectType && DimCount == other.DimCount);
	}

	bool operator != (const CLT &other) const {
		return (Type != other.Type || ObjectType != other.ObjectType || DimCount != other.DimCount);
	}

	CKerName *ObjectType;
	eKerTypeModifier Modifier;
	eKerTypes Type;
	UC DimCount;
};



#endif