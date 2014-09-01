#pragma once

#ifndef VersionFeatures_H
#define VersionFeatures_H

#include "types.h"
#include "fs.api.h"
#include <hash_map>
#include <string>

using namespace std;
using namespace stdext;

class CFSRegister;

class CVersionFeatures {
public:
	typedef hash_map<string, int> FeatureMapT;
	struct FeatureMap {
		FeatureMapT map;
	};
	typedef hash_map<string, FeatureMap> HeadersMapT;

	void AddFeature(eVersionFeature type, string header, string feature, int version); // add requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
	void RemoveFeature(eVersionFeature type, string header, string feature); // remove requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
	bool CheckFeatures(CFSRegister *reg); // check if version of features of the file matches version of features of the system
	void WriteFeatures(CFSRegister *reg); // writes versions of features

private:
	HeadersMapT _features[4];
	bool CheckFeature(const string &feature, int version, FeatureMapT &map);
	bool CheckFeatures2(FeatureMapT &reqMap, FeatureMapT &map);
	void WriteFeatures2(CFSRegKey *key, FeatureMapT &map);
	CFSRegKey * CreateKey(CFSRegister *reg, const char* name);

};



#endif
