#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Security::Cryptography;
using namespace System::Text;
using namespace System::Globalization;

#include "CStringToCharPtr.h"


namespace Krkal {
	namespace FileSystem {



		public ref class KrkalPath {
		public:


			KrkalPath(String ^full, IList<String^>^ extensions) {
				if (full == nullptr)
					throw gcnew ArgumentNullException("full");
				_full = full;
				_extensions = extensions;
				ParseName();
			}

			KrkalPath(String ^full, String^ extension) {
				if (full == nullptr)
					throw gcnew ArgumentNullException("full");
				_full = full;
				_extensions = safe_cast<IList<String^>^>(gcnew array<String^>(1));
				_extensions[0] = extension;
				ParseName();
			}



			static String^ GenerateVersion() {
				Lock l(locker);
				array<Byte> ^buffer = gcnew array<Byte>(8);
				Random->GetBytes(buffer);

				StringBuilder ^sb = gcnew StringBuilder(20);
				unsigned short int a;

				for (int f=0; f<4; f++) {
					if (f>0)
						sb->Append(L'_');
					a = (buffer[f*2] << 8) | buffer[f*2+1];
					sb->AppendFormat("{0:X4}", a);
				}

				return sb->ToString();
			}


			static String^ CompleteName(String ^name, String ^extension) {
				KrkalPath^ krkalPath = gcnew KrkalPath(name, extension);
				krkalPath->Complete(true, 0);
				return krkalPath->Full;
			}


			property String ^Full {
				String^ get() { return _full; }
			}

			property String ^LongWithoutExtension {
				String^ get() { return _full->Substring(0, _longLength); }
			}

			property String ^Version {
				String^ get() { return _version; }
			}

			property int KnownExtension {
				int get() { return _knownExtension;}
			}

			property String ^ShortWithExtension {
				String^ get() { 
					if (_shortWithExtension == nullptr) {
						_shortWithExtension = _full->Substring(0, _shortLength);
						if (_knownExtension >= 0) {
							_shortWithExtension += _extensions[_knownExtension];
						} else if (_unknownExtension != nullptr) {
							_shortWithExtension += _unknownExtension;
						}
					}
					return _shortWithExtension;
				}
			}

			property String ^ShortWithoutExtension {
				String^ get() { 
					if (_shortWithoutExtension == nullptr) {
						_shortWithoutExtension = _full->Substring(0, _shortLength);
					}
					return _shortWithoutExtension;
				}
			}

			void Complete(bool addVersion, int extensionToAdd) {
				StringBuilder ^sb = gcnew StringBuilder(ShortWithoutExtension, _shortLength + 20 + 16);
				if (addVersion) {
					sb->Append(L'_');
					_version = GenerateVersion();
					sb->Append(_version);
				}
				_longLength = sb->Length;
				if (extensionToAdd >= 0) {
					sb->Append(_extensions[extensionToAdd]);
				}
				_full = sb->ToString();
				_shortWithExtension = nullptr;
				_knownExtension = extensionToAdd;
				_unknownExtension = nullptr;
			}



		private:
			static Object ^locker = gcnew Object();
			static RNGCryptoServiceProvider^ _random;
			static property RNGCryptoServiceProvider^ Random {
				RNGCryptoServiceProvider^ get() {
					if (_random == nullptr) 
						_random = gcnew RNGCryptoServiceProvider();
					return _random;
				}
			}


			String ^_full;
			String ^_version;
			int _knownExtension;
			String ^_shortWithExtension;
			String ^_shortWithoutExtension;
			String ^_unknownExtension;
			IList<String^>^ _extensions;
			int _shortLength;
			int _longLength;


			void ParseName() {
				_knownExtension = -1;
				_unknownExtension = nullptr;
				_shortLength = _full->Length;
				if (_extensions != nullptr) {
					for(int f=0; f<_extensions->Count; f++) {
						if (_full->EndsWith(_extensions[f], true, CultureInfo::CurrentCulture)) {
							_knownExtension = f;
							_shortLength -= _extensions[f]->Length;
							break;
						}
					}
				}

				_longLength = _shortLength;

				int counter = 0;
				int pos = _longLength-1;

				for (; pos >= 0; pos--) {
					if (_full[pos] == '\\' || _full[pos] == '/')
						break;
					bool ok = false;
					
					if (_full[pos] == '.') {
						counter=-1;
						ok=true;
					} else if (counter >= 0) {
						if (counter % 5 == 4) {
							if (_full[pos] == '_') ok=true;
						} else {
							if (IsHexadecimalDigit(_full[pos])) ok=true;
						}
					}

					if (ok) {
						counter++;
						if (counter == 20) {
							_shortLength = pos;
							_version = _full->Substring(_shortLength+1, 19);
							if (_longLength > _shortLength+20) {
								_unknownExtension = _full->Substring(_shortLength+20);
								_longLength = _shortLength+20;
							}
							break;
						}
					} else {
						counter=-1;
					}
				}
			}


			static array<bool>^ _hexadecimalDigitsMap;

			static bool IsHexadecimalDigit(System::Char ch) {
				unsigned int index = ch;
				if (index > 127)
					return false;
				if (_hexadecimalDigitsMap == nullptr) {
					array<bool>^ digitsMap = gcnew array<bool>(128);
					for (int f = '0'; f <= '9'; f++) {
						digitsMap[f] = true;
					}
					for (int f = 'A'; f <= 'F'; f++) {
						digitsMap[f] = true;
					}
					for (int f = 'a'; f <= 'f'; f++) {
						digitsMap[f] = true;
					}

					_hexadecimalDigitsMap = digitsMap;

				}
				return _hexadecimalDigitsMap[index];
			}


		};



	}
}