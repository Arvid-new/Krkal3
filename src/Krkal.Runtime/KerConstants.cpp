//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Runtime - K e r C o n s t a n t s
///
///		Definice zakladnich konstant
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


#include "stdafx.h"
#include "KerConstants.h"


//// typy znamych promennych
//eKerTypes KerVarUsesTypes[KERVARUSESSIZE] = {
//	eKTvoid, eKTchar, eKTint, eKTint, eKTint, eKTint,
//	eKTint, eKTint, eKTint, eKTint, eKTint, eKTint,
//	eKTint, eKTint, eKTint,
//	eKTname, eKTname, eKTname, eKTint,
//	eKTname, eKTobject,
//};
//
//// jmena znamych promennych
//char* KerVarUsesNames[KERVARUSESSIZE] = {
//	"_KN_nonevar", "_KN_CollisionCfg", "_KN_NumCellZ","_KN_NumCellX","_KN_NumCellY","_KN_CellRadius",
//	"_KN_BCubeX1", "_KN_BCubeY1", "_KN_BCubeZ1", "_KN_BCubeX2", "_KN_BCubeY2", "_KN_BCubeZ2", 
//	"_KN_ObjPosX", "_KN_ObjPosY", "_KN_ObjPosZ", 
//	"_KN_clzAddGr", "_KN_clzSubGr", "_KN_APicture", "_KN_ANoConnect",
//	"_KN_clzFceGr", "_KN_MsgRedirect",
//};
//
//CKerVarUsesInfo KerVarUsesInfo[KERVARUSESSIZE] = {
//	{0,0,eKETdefault,0}, {1,0,eKETnumeric,0}, {1,0,eKETnumeric,0},  {1,0,eKETnumeric,0},  {1,0,eKETnumeric,0},  {1,0,eKETnumeric,0},
//	{1,0,eKETnumeric,0}, {1,0,eKETnumeric,0}, {1,0,eKETnumeric,0}, {1,0,eKETnumeric,0}, {1,0,eKETnumeric,0}, {1,0,eKETnumeric,0}, 
//	{1,1,eKETnumeric,0}, {1,1,eKETnumeric,0}, {1,1,eKETnumeric,0}, 
//	{0,0,eKETdefault,(1<<eKerNTobjectVoid)|(1<<eKerNTclass)}, {0,0,eKETdefault,(1<<eKerNTobjectVoid)|(1<<eKerNTclass)}, {1,0,eKETautomaticGr,0xFFFFFFFF}, {1,0,eKETconnectionMask,0},
//	{0,0,eKETdefault,(1<<eKerNTobjectVoid)|(1<<eKerNTclass)}, {0,0,eKETdefault,0xFFFFFFFF},
//};
//// Limity se zadavaj rucne CKerObjs::LoadOVar
//
//
//CKerVarGroupInfos KerVarGroupInfos[KERNUMVARGROUPS] = {
//	{eKTint,2}, {eKTint,3}, {eKTint,2}, {eKTint,3}, {eKTint,4}, {eKTint,6}, 
//};


// Znama jmena.  Polozku Name spravne nastavuje Kernel pri loadingu
CKnownNames KnownNames[MAXKNOWNNAMES] = {
	{"_KSID_Static",eKerNTclass,eKTunasigned},
	{"_KSID_Constructor",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_Destructor",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_Main",eKerNTstaticSafeMethod,CLT(eKTvoid, eKTMstatic|eKTMpublic,0,0)},
	{"_KSID_LoadConstructor",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_CopyConstructor",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_Everything",eKerNTvoid,eKTunasigned},
	{"_KSID_Nothing",eKerNTvoid,eKTunasigned},
	{"_KSID_Object",eKerNTclass,eKTunasigned},
	{"_KSID_CreateData", eKerNTstaticSafeMethod,CLT(eKTvoid, eKTMstatic|eKTMpublic,0,0)},
	{"_KSID_Attribute__M_UserName", eKerNTvariable,CLT(eKTchar, eKTMconst1|eKTMpublic,0,1)},
	{"_KSID_Attribute__M_Comment", eKerNTvariable,CLT(eKTchar, eKTMconst1|eKTMpublic,0,1)},
	{"_KSID_Attribute__M_File", eKerNTvariable,CLT(eKTchar, eKTMconst1|eKTMpublic,0,1)},
	{"_KSID_VariableFlags", eKerNTenum,eKTunasigned},
	{"_KSID_ClassFlags", eKerNTenum,eKTunasigned},
	{"_KSID_Attribute__M_VarFlag", eKerNTvariable,CLT(eKTint, eKTMretOr|eKTMpublic, (CKerName*)13, 0)},
	{"_KSID_Attribute__M_ClassFlag", eKerNTvariable,CLT(eKTint, eKTMretOr|eKTMpublic, (CKerName*)14, 0)},
	{"_KSID_LevelLoaded",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_GameLoaded",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_DataLoaded",eKerNTsafeMethod,CLT(eKTvoid, eKTMpublic)},
	{"_KSID_AllGames",eKerNTvoid,eKTunasigned},
	{"_KSID_AllLanguages",eKerNTvoid,eKTunasigned},
	{"_KSID_Attribute__M_Engine", eKerNTvariable,CLT(eKTchar, eKTMconst1|eKTMpublic,0,1)},
	//{"_KSID_Constructor",eKerNTmethod,eKTvoid,0},
	//{"_KSID_LoadConstructor",eKerNTmethod,eKTvoid,0},
	//{"_KSID_CopyConstructor",eKerNTmethod,eKTvoid,0},
	//{"_KSID_Destructor",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_DefaultObject",eKerNTclass,0,0},
	//{"_KSID__KN_ESaveMe",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_MapPlaced",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_MapRemoved",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_MPlaceObjToMap",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_MGetObjects",eKerNTmethod,eKTarrObject,0},
	//{"_KSID__KN_CellsArray",eKerNTparam,0,0},
	//{"_KSID__KN_Object",eKerNTparam,0,0},
	//{"_KSID__KN_MRemoveObjFromMap",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_MMoveObjInMap",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_Everything",eKerNTvoid,0,0},
	//{"_KSID__KN_Nothing",eKerNTvoid,0,0},
	//{"_KSID__KN_TestCollision",eKerNTmethod,eKTint|eKTretOR,0},
	//{"_KSID__KN_CollisionKill",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_KeepCellsArray",eKerNTparam,0,0},
	//{"_KSID__KN_RemoveCellsArray",eKerNTparam,0,0},
	//{"_KSID__KN_PlaceCellsArray",eKerNTparam,0,0},
	//{"_KSID__KN_MoveEnded",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_ObjectArray",eKerNTparam,0,0},
	//{"_KSID__KN_IsMoveCorrect",eKerNTmethod,eKTint|eKTretAND,0},
	//{"_KSID__KN_CoordX",eKerNTparam,0,0},
	//{"_KSID__KN_CoordY",eKerNTparam,0,0},
	//{"_KSID__KN_CoordZ",eKerNTparam,0,0},
	//{"_KSID__KN_DefaultAuto",eKerNTautoVoid,0,0},
	//{"_KSID__KN_TriggerOn",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_TriggerOff",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_ObjType",eKerNTparam,0,0},
	//{"_KSID__KN_MResizeMap",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_ScrollObj",eKerNTclass,0,0},
	//{"_KSID__KN_ItemID",eKerNTparam,0,0},
	//{"_KSID__KN_ButtonID",eKerNTparam,0,0},
	//{"_KSID__KN_ButtonUserID",eKerNTparam,0,0},
	//{"_KSID__KN_GroupID",eKerNTparam,0,0},
	//{"_KSID__KN_PublicMethods",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_LoadGame",eKerNTmethod,eKTvoid,0},
	//{"_KSID__KN_SaveGame",eKerNTmethod,eKTvoid,0},
};

//int KerKnownDependencies[MAXKERKNOWNDEPENDENCIES*2] = {
	//eKKNdestructor, eKKNCollisionKill,
	//eKKNpublicMethods, eKKNdestructor,
//};

