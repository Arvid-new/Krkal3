#include "stdafx.h"
#include "VersionFeatures.h"
#include "fs.api.h"



void CVersionFeatures::AddFeature(eVersionFeature type, string header, string feature, int version) {
	int start = type == evfAll ? 0 : (int)type;
	int stop = type == evfAll ? 3 : (int)type;
	for (int f=start; f<=stop; f++) {
		int &ver = _features[f][header].map[feature];
		if (ver < version)
			ver = version;
	}
}




void CVersionFeatures::RemoveFeature(eVersionFeature type, string header, string feature) {
	int start = type == evfAll ? 0 : (int)type;
	int stop = type == evfAll ? 3 : (int)type;
	for (int f=start; f<=stop; f++) {
		_features[f][header].map.erase(feature);
	}
}




bool CVersionFeatures::CheckFeatures(CFSRegister *reg) {
	FeatureMapT fileFeatures;
	CFSRegKey *key = reg->FindKey("Supported Features");
	if (key) {
		key->pos=0;
		for ( ; !key->eof(); key->SetPosToNextString()) {
			const char *pos = strchr(key->GetDirectAccessFromPos(), ':');
			if (!pos)
				break;
			fileFeatures[string(key->GetDirectAccessFromPos(), pos - key->GetDirectAccessFromPos())] = atoi(pos+1);
		}
	}

	if (!CheckFeatures2(_features[(int)evfSystemRequeres][""].map, fileFeatures))
		return false;
	if (!CheckFeatures2(_features[(int)evfSystemRequeres][reg->GetHeader()].map, fileFeatures))
		return false;
			
	key = reg->FindKey("Requiered Features");
	if (key) {
		FeatureMapT &kerSupp1 = _features[(int)evfSystemSupports][""].map;
		FeatureMapT &kerSupp2 = _features[(int)evfSystemSupports][reg->GetHeader()].map;
		key->pos=0;
		for ( ; !key->eof(); key->SetPosToNextString()) {
			const char *pos = strchr(key->GetDirectAccessFromPos(), ':');
			if (!pos)
				return false;
			string s(key->GetDirectAccessFromPos(), pos - key->GetDirectAccessFromPos());
			int ver = atoi(pos+1);
			if (!CheckFeature(s, ver, kerSupp1) && !CheckFeature(s, ver, kerSupp2))
				return false;
		}
	}

	return true;
}


bool CVersionFeatures::CheckFeature(const string &feature, int version, hash_map<string, int> &map) {
	FeatureMapT::iterator i = map.find(feature);
	int ver = i == map.end() ? 0 : i->second;
	return ver >= version;
}

bool CVersionFeatures::CheckFeatures2(FeatureMapT &reqMap, FeatureMapT &map) {
	for (FeatureMapT::iterator i = reqMap.begin(); i != reqMap.end(); ++i) {
		if (!CheckFeature(i->first, i->second, map))
			return false;
	}
	return true;
}




void CVersionFeatures::WriteFeatures(CFSRegister *reg) {
	CFSRegKey *key = CreateKey(reg, "Supported Features");
	WriteFeatures2(key, _features[(int)evfFileSupports][""].map);
	WriteFeatures2(key, _features[(int)evfFileSupports][reg->GetHeader()].map);
	key = CreateKey(reg, "Requiered Features");
	WriteFeatures2(key, _features[(int)evfFileRequieres][""].map);
	WriteFeatures2(key, _features[(int)evfFileRequieres][reg->GetHeader()].map);
}


void CVersionFeatures::WriteFeatures2(CFSRegKey *key, FeatureMapT &map) {
	char buff[256];
	for (FeatureMapT::iterator i = map.begin(); i != map.end(); ++i) {
		sprintf_s(buff, sizeof(buff), "%s:%i", i->first.c_str(), i->second);
		key->stringwrite(buff);
	}
}


CFSRegKey * CVersionFeatures::CreateKey(CFSRegister *reg, const char* name) {
	CFSRegKey *key = reg->FindKey(name);
	if (key) {
		key->pos=0; key->top=0;
		assert(key->CFSGetKeyType() == FSRTstring);
	} else {
		key = reg->AddKey(name, FSRTstring);
	}
	return key;
}
