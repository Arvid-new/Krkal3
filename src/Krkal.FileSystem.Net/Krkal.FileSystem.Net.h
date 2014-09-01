// Krkal.FileSystem.Net.h

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;

#include "fs.api.h"
#include "CStringToCharPtr.h"
#include <string>



namespace Krkal {
	namespace FileSystem {



		public ref class FSFileNotFoundException : public Exception
		{
		public:
			FSFileNotFoundException(Exception ^innerException)
				: Exception("io error", innerException) { }
			FSFileNotFoundException(String ^message)
				: Exception(message) { }
			FSFileNotFoundException()
				: Exception("io error") { }
		};



		public delegate String^ GetCurrentFileCallback();


		public interface class INonChangedFileCollection
		{
			void Add(String ^file);
			bool Remove(String ^file);
			bool Contains(String ^file);
		};


		private ref class NonChangedFileCollection : 
			public INonChangedFileCollection
		{
		public:
			NonChangedFileCollection() 
			{ 
				_data = gcnew Dictionary<String ^, int>(StringComparer::InvariantCultureIgnoreCase);
			}

			virtual void Add(String ^item)  {
				Lock l(_data);
				if (!_data->ContainsKey(item)) {
					_data->Add(item, 0);
				}
			}

			virtual bool Remove(String ^item)  {
				Lock l(_data);
				return _data->Remove(item);
			}

			virtual bool Contains(String ^item)  {
				Lock l(_data);
				return _data->ContainsKey(item);
			}


		private:
			Dictionary<String ^, int> ^_data;

		};



		public interface class IFS
		{
			String ^OpenFileForReading(String ^file);
			String ^OpenFileForReading(String ^file, INonChangedFileCollection ^nonChangedFiles);
			TextWriter ^OpenFileForWriting(String ^file);
			INonChangedFileCollection ^RegisterNonChangedFiles();
			String ^CheckOut(String ^file, GetCurrentFileCallback ^callback);
			void FileChanged(String ^file);
			void CancelCheckOut(String ^file);
			bool IsCheckedOut(String ^file);
		};


		private ref class CheckedOutFile {
		private:
			String ^_content;
			bool _valid;
			bool _differsFromDisk;
			GetCurrentFileCallback ^_callback;

		public:
			property bool Valid {
				bool get() { return _valid; }
				void set(bool value) { _valid = value; }
			}
			property bool DiffersFromDisk {
				bool get() { return _differsFromDisk; }
				void set(bool value) { _differsFromDisk = value; }
			}
			String ^GetContent() {
				if (!_valid) {
					_content = _callback();
					_differsFromDisk = true;
				}
				_valid = true;
				return _content;
			}

			// CONSTRUCTOR

			CheckedOutFile(String ^content, GetCurrentFileCallback ^callback) {
				if (content == nullptr)
					throw gcnew ArgumentNullException("content");
				if (callback == nullptr)
					throw gcnew ArgumentNullException("callback");
				_content = content;
				_callback = callback;
				_valid = true;
				_differsFromDisk = false;
			}

		};




		public ref class FSSearchData : IDisposable {
		private:
			CFSSearchData *_data;
		internal:
			FSSearchData(CFSSearchData *data) { _data = data; }

		public:
			~FSSearchData() {
				this->!FSSearchData();
			}
			!FSSearchData() {
				if (_data) {
					_data->Close();
					_data = 0;
				}
			}


			property int Count {
				int get() { return _data->count(); }
			}

			String ^GetName(int index) {
				if (index < 0 || index >= _data->count())
					throw gcnew ArgumentOutOfRangeException("index");

				std::string s;
				int a;
				_data->GetField(index, s, a);
				return gcnew String(s.c_str());
			}
			int GetAttr(int index) {
				if (index < 0 || index >= _data->count())
					throw gcnew ArgumentOutOfRangeException("index");

				std::string s;
				int a;
				_data->GetField(index, s, a);
				return a;
			}

		};




		public enum class FSCopyTreeChangeMode {
			None,
			ChangeVersion,
			AddExtension,
			RemoveExtension,
			ChangeExtension,
		};

		public enum class FSFullPathType {
			WindowsUpperCase,		// C:\KRKAL\GAMES\ERRORS_1234_5897_A87C_55FE.TXT
			WindowsOriginalCase,	// C:\Krkal\Games\ERRORS_1234_5897_A87C_55FE.txt
			KrkalUpperCase,			// $GAMES$\ERRORS_1234_5897_A87C_55FE.TXT
			KrkalOriginalCase,		// $GAMES$\ERRORS_1234_5897_A87C_55FE.txt
			InvariantKey,			// ERRORS_1234_5897_A87C_55FE.TXT - file must exist. The path is shortened to name with version if its in search tree. Uppercase is used. If not in search tree efptKrkalUpperCase is used instead.
			InvariantKeyOriginalCase,// Errors_1234_5897_A87C_55FE.txt - file must exist. The path is shortened to name with version if its in search tree. If not in search tree efptKrkalOriginalCase is used instead.
		};


		public enum class FSVersionFeature {
			All = 100,				// afects all types
			SystemSupports = 0,		// system supports this feature
			FileSupports = 1,		// file supports this feature
			SystemRequeres = 2,		// system requieres this feature
			FileRequieres = 3,		// file requieres this feature
		};



		public ref class FS : public IDisposable, public IFS
		{

		public:
			static property FS^ FileSystem {
				FS^ get() {
					if (_FS == nullptr)
						_FS = gcnew FS();
					return _FS;
				}
			}
			~FS() {
				this->!FS();
			}
			!FS() {
				CFS::DoneFS();
				_FS = nullptr;
			}



			//vraci velikost souboru, pokud je chyba vraci 0
			int GetFileSize(String ^path) {
				return _fs->GetFileSize(CStringToCharPtr(path));
			}

			int GetfileTime(String ^path, System::Runtime::InteropServices::ComTypes::FILETIME %fileTime) {
				pin_ptr<System::Runtime::InteropServices::ComTypes::FILETIME> p = &fileTime;
				return _fs->GetTime(CStringToCharPtr(path), *(FILETIME*)p);
			}

			int SetFileTime(String ^path, System::Runtime::InteropServices::ComTypes::FILETIME fileTime) {
				return _fs->SetTime(CStringToCharPtr(path), *(FILETIME*)&fileTime);
			}

			//nacte soubor do bufferu, vraci 0=err 1=OK; bufsize je velikost buferu -> pokud se soubor cely nevejde nacte se zacatek a funkce vrati 2
			int ReadFile(String ^name, array<Byte> ^buf) {
				pin_ptr<Byte> p = &buf[0];
				return _fs->ReadFile(CStringToCharPtr(name), (char*)p, buf->Length);
			}

			//zapise soubor, vraci 0=err 1=OK, compression: 0=no compr, 1=zlib, -1=stejne jako existujici soubor (pokud neex.=1)
			int WriteFile(String ^name, array<Byte> ^buf, int length) {
				return this->WriteFile(name, buf, length, -1);
			}
			int WriteFile(String ^name, array<Byte> ^buf, int length, int compression) {
				pin_ptr<Byte> p = &buf[0];
				return _fs->WriteFile(CStringToCharPtr(name), (char*)p, length, compression);
			}



			//vytvori adresar, vraci 0=err 1=OK
			int CreateDir(String ^name) {
				return _fs->CreateDir(CStringToCharPtr(name));
			}


			//smaze soubor/adresar/archiv, vcetne vnorenych souboru a adresaru!!! (tak bacha ;-) (lze mazat i v nadrazenych adresarich - potom ale neni platny akt. adresar - musi se pak zavolat changedir s abs. cestou!), vraci 0=err 1=OK
			int Delete(String ^name) {
				return _fs->Delete(CStringToCharPtr(name));
			}

			//vytvori archiv, vraci 0=err 1=OK
			int CreateArchive(String ^name) {
				return _fs->CreateArchive(CStringToCharPtr(name));
			}
			
			// changes directory (and its content) to archive. Or changes archive to normal directory structure.
			int PackOrUnpackArchive(String ^path) {
				return _fs->PackOrUnpackArchive(CStringToCharPtr(path));
			}


			//vraci jestli je soubor komprimovany
			int IsCompressed(String ^name) {
				return _fs->IsCompressed(CStringToCharPtr(name));
			}

			//testuje jestli existuje soubor/adresar; vraci 0=neex. 1=soubor 2=adresar 3=archiv
			int FileExist(String ^name) {
				return _fs->FileExist(CStringToCharPtr(name));
			}


			//vrati celou cestu, vola new na fullpath; vraci 0=err 1=OK
			int GetFullPath(String ^relpath, String ^%fullpath) {
				return GetFullPath(relpath, fullpath, FSFullPathType::WindowsOriginalCase);
			}
			int GetFullPath(String ^relpath, String ^%fullpath, FSFullPathType type) {
				int ret;
				CCharPtrToString_D fp;
				ret = _fs->GetFullPath(CStringToCharPtr(relpath), fp.GetAddress(), (EFullPathType)type);
				fullpath = fp.ToString();
				return ret;
			}


			// returns all files and directories in a directory specified by path. Returns NULL in case of an error.
			FSSearchData ^ReadDirectory(String ^path) {
				CFSSearchData *ret = _fs->ReadDirectory(CStringToCharPtr(path));
				return ret ? gcnew FSSearchData(ret) : nullptr;
			}

			// returns the top most root dirs.
			FSSearchData ^GetRoots() {
				CFSSearchData *ret = _fs->GetRoots();
				return ret ? gcnew FSSearchData(ret) : nullptr;
			}


			//zkopiruje soubor, dir nebo dir\* do destdir
			// changeMode: 0 - no change, 1 - change version, 2 - add extension, 3 - remove extension, 4 - change extension
			int CopyTree(String ^sourcedir, String ^destdir) {
				return CopyTree(sourcedir, destdir, FSCopyTreeChangeMode::None, nullptr, -1);
			}
			int CopyTree(String ^sourcedir, String ^destdir, FSCopyTreeChangeMode changeMode, String ^ext, int comp) {
				return _fs->CopyTree(CStringToCharPtr(sourcedir), CStringToCharPtr(destdir), (ECopyTreeChangeMode)changeMode, CStringToCharPtr(ext), comp);
			}

			// presune a prejmenuje source (file,dir,dir\*) do dest.
			int Move(String ^source, String ^dest) {
				return _fs->Move(CStringToCharPtr(source), CStringToCharPtr(dest));
			}


			//testuje jestli 'filename' je platny nazev souboru (tj. neobsahuje / a dalsi znaky)
			int IsValidFilename(String ^filename) {
				return _fs->IsValidFilename(CStringToCharPtr(filename));
			}

			//defragmentuje archiv
			int Defragment(String ^archvename) {
				return _fs->Defragment(CStringToCharPtr(archvename));
			}

			//porovna 2 cesty jestli jsou stejny, vraci 0 kdyz jsou stejny, vlastne dela case insensitive cmp, navic '\'='/'
			int ComparePath(String ^path1, String ^path2) {
				return _fs->ComparePath(CStringToCharPtr(path1), CStringToCharPtr(path2));
			}


			// add requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
			void AddVersionFeature(FSVersionFeature type, String^ header, String^ feature, int version) {
				_fs->AddVersionFeature((eVersionFeature)type, CStringToCharPtr(header), CStringToCharPtr(feature), version);
			}

			// remove requested or supported feature to system or file. You can use specific Register header or use "" to affect all register files
			void RemoveVersionFeature(FSVersionFeature type, String^ header, String^ feature) {
				_fs->RemoveVersionFeature((eVersionFeature)type, CStringToCharPtr(header), CStringToCharPtr(feature));
			}


			//prida adresar ( nastavi $key$ = val )
			void AddFSDir(String ^key, String ^val) {
				_fs->AddFSDir(CStringToCharPtr(key), CStringToCharPtr(val));
			}

			void AddArchiveExtension(String ^ext, int type) {
				_fs->AddArchiveExtension(CStringToCharPtr(ext), type);
			}


			Stream^ SreamReadFile(String ^file);
			Stream^ StreamWriteFile(String ^file, int comp);
			Stream^ StreamWriteFile(String ^file) {
				return this->StreamWriteFile(file, -1);
			}



			virtual INonChangedFileCollection ^RegisterNonChangedFiles() {
				Lock l(_locker);
				NonChangedFileCollection ^ret = gcnew NonChangedFileCollection();
				_listOfNonChangedFiles->Add(ret);
				return ret;
			}

			virtual String ^OpenFileForReading(String ^file, INonChangedFileCollection ^nonChangedFiles);
			virtual String ^OpenFileForReading(String ^file) {
				return OpenFileForReading(file, nullptr);
			}


			virtual TextWriter ^OpenFileForWriting(String ^file); 

			virtual void FileChanged(String ^file);
			virtual String ^CheckOut(String ^file, GetCurrentFileCallback ^callback);
			virtual void CancelCheckOut(String ^file);
			virtual bool IsCheckedOut(String ^file) {
				Lock l(_locker);
				return _checkedOutFiles->ContainsKey(file);
			}


		private:
			CFS *_fs;
			Object^ _locker;
			static FS^ _FS;
			List<NonChangedFileCollection^> ^_listOfNonChangedFiles;
			Dictionary<String^, CheckedOutFile^> ^_checkedOutFiles;
			FS() {
				_locker = gcnew Object();
				CFS::InitFS();
				_fs = CFS::GetFS();
				_listOfNonChangedFiles = gcnew List<NonChangedFileCollection^>();
				_checkedOutFiles = gcnew Dictionary<String^, CheckedOutFile^>(StringComparer::CurrentCultureIgnoreCase);
			}


		internal:
			void SavingFileNotification(String ^file);
			Object^ GetLocker() { return _locker;}

		};




		private ref class FSStringWriter : public StreamWriter
		{
		public:
			FSStringWriter(FS ^fs, String ^file, MemoryStream ^myMemoryStream)
				: StreamWriter(myMemoryStream, System::Text::Encoding::UTF8)
			{
				_fs = fs;
				_myMemoryStream = myMemoryStream;
				_file = file;
			}

			~FSStringWriter() {
				try {
					if (_file != nullptr) {
						Lock l(_fs->GetLocker());

						this->Flush();

						_fs->SavingFileNotification(_file);

						int ret = _fs->WriteFile(_file, _myMemoryStream->GetBuffer(), (int)_myMemoryStream->Length, 0);
						_myMemoryStream->Close();
						if (!ret)
							throw gcnew FSFileNotFoundException();
					}
				} finally {
					_file = nullptr;
				}
			}

		private:
			FS ^_fs;
			MemoryStream ^_myMemoryStream;
			String ^_file;
		};



		private ref class FSStreamWriter : public MemoryStream
		{
		public:
			FSStreamWriter(FS ^fs, String ^file, int comp)
			{
				_fs = fs;
				_file = file;
				_comp = comp;
			}

			~FSStreamWriter() {
				try {
					if (_file != nullptr) {
						int ret = _fs->WriteFile(_file, this->GetBuffer(), (int)this->Length, _comp);
						if (!ret)
							throw gcnew FSFileNotFoundException();
					}
				} finally {
					_file = nullptr;
				}
			}

		private:
			FS ^_fs;
			String ^_file;
			int _comp;
		};


	}
}
