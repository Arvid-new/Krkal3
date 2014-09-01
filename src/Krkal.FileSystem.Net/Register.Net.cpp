#include "stdafx.h"

#include "Register.Net.h"


namespace Krkal {
	namespace FileSystem {

			FSRegister FSRegKey::Subregister::get() { return FSRegister(_key->GetSubRegister()); }

			FSRegKey FSRegister::AddRegisterToKey(String ^name, FSRegisterFile ^subRegister)  // Prida do registru registr. (propoji je) Pridavany registr musi byt samostatne vytvoreny (nepropojeny)
			{			
				return FSRegKey(_reg->AddRegisterToKey(CStringToCharPtr(name), subRegister->Detach()._reg));
			}


	}
}