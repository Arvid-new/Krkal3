// This is the main DLL file.

#include "stdafx.h"

#include "Krkal.FileSystem.Net.h"



namespace Krkal {
	namespace FileSystem {



		Stream^ FS::SreamReadFile(String ^file) {
			int fileSize = this->GetFileSize(file);
			if (!fileSize)
				throw gcnew FSFileNotFoundException();
			array<Byte> ^buffer = gcnew array<Byte>(fileSize);
			if (1 != this->ReadFile(file, buffer))
				throw gcnew FSFileNotFoundException();

			return gcnew MemoryStream(buffer, false);
		}


		Stream^ FS::StreamWriteFile(String ^file, int comp) {
			return gcnew FSStreamWriter(this, file, comp);
		}

		TextWriter ^FS::OpenFileForWriting(String ^file) {
			// no need to lock

			MemoryStream ^stream = gcnew MemoryStream();
			TextWriter ^writer = gcnew FSStringWriter(this, file, stream);
			return writer;

		}


		String ^FS::OpenFileForReading(String ^file, INonChangedFileCollection ^nonChangedFiles) {
			Lock l(_locker);

			String ^ret = nullptr;

			CheckedOutFile ^checkedOut = nullptr;
			if (_checkedOutFiles->TryGetValue(file, checkedOut)) {
				ret = checkedOut->GetContent();

			} else {

				int fileSize = this->GetFileSize(file);
				if (!fileSize)
					throw gcnew FSFileNotFoundException();
				array<Byte> ^buffer = gcnew array<Byte>(fileSize);
				if (1 != this->ReadFile(file, buffer))
					throw gcnew FSFileNotFoundException();

				MemoryStream stream(buffer);
				StreamReader reader(%stream, System::Text::Encoding::UTF8, true);

				ret = reader.ReadToEnd();
			}

			if (nonChangedFiles != nullptr)
				nonChangedFiles->Add(file);

			return ret;

		}


		String ^FS::CheckOut(String ^file, GetCurrentFileCallback ^callback) {
			Lock l(_locker);
			String ^output = OpenFileForReading(file);

			CheckedOutFile ^checkedOut = gcnew CheckedOutFile(output, callback);
			_checkedOutFiles->Add(file, checkedOut);

			return output;
		}



		void FS::FileChanged(String ^file) {
			Lock l(_locker);

			CheckedOutFile ^checkedOut = nullptr;
			if (_checkedOutFiles->TryGetValue(file, checkedOut)) {
				if (checkedOut->Valid == false)
					return; // there was no read since last invalidate -> nothing to do
				checkedOut->Valid = false;
			}

			for each (NonChangedFileCollection ^nonChangedFiles in _listOfNonChangedFiles)
				nonChangedFiles->Remove(file);

		}


		void FS::CancelCheckOut(String ^file) {
			Lock l(_locker);
			CheckedOutFile ^checkedOut = nullptr;
			_checkedOutFiles->TryGetValue(file, checkedOut);
			_checkedOutFiles->Remove(file);
			if (checkedOut != nullptr && checkedOut->DiffersFromDisk) {
				FileChanged(file);
			}
		}



		void FS::SavingFileNotification(String ^file) {
			// no need to lock
			CheckedOutFile ^checkedOut = nullptr;
			if (_checkedOutFiles->TryGetValue(file, checkedOut)) {
				checkedOut->DiffersFromDisk = false;
			} else {
				FileChanged(file);
			}

		}


	}
}