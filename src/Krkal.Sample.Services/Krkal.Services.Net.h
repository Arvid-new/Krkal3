#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace Krkal::Runtime;

#include "CStringToCharPtr.h"
#include "CKerMain.h"
#include "ObjectEx.h"
#include "Krkal.Services.h"



namespace Krkal {
	namespace Services {
		

		public ref class SampleServices  : IDisposable {
		private:
			CServices *_services;
			int _oldSizeX, _oldSizeY;

		public:

			SampleServices(KerMain ^kerMain);

			property bool MapSizeChanged {
				bool get() { return _services->mapSX != _oldSizeX || _services->mapSY != _oldSizeY;}
			}
			void GetSize(int %x, int %y) {
				x = _oldSizeX = _services->mapSX;
				y = _oldSizeY = _services->mapSY;
			}
			void GetInvalidArea(int %x, int %y, int %dx, int %dy) {
				int X,Y,DX,DY;
				_services->GetInvalidArea(X,Y,DX,DY);
				x=X; y=Y; dx=DX; dy=DY;
			}

			array<String^>^ GetBitmapNames(int x, int y) {
				int count=0;
				for (CKerObjectEx *ex = _services->GetFirstObj(x, y); ex; ex = ex->MapInfo->next) count++;
				array<String^>^ arr = gcnew array<String^>(count);

				count=0;
				for (CKerObjectEx *ex = _services->GetFirstObj(x, y); ex; ex = ex->MapInfo->next, count++) {
					arr[count] = gcnew String(ex->MapInfo->Image);
				}
				return arr;
			}


			array<KerObject>^ GetObjects(int x, int y) {
				int count=0;
				for (CKerObjectEx *ex = _services->GetFirstObj(x, y); ex; ex = ex->MapInfo->next) count++;
				array<KerObject>^ arr = gcnew array<KerObject>(count);

				count=0;
				for (CKerObjectEx *ex = _services->GetFirstObj(x, y); ex; ex = ex->MapInfo->next, count++) {
					arr[count] = KerObject(IntPtr(ex->thisO));
				}
				return arr;
			}

			bool PlaceIfNoCollision(KerName objType, int x, int y) {
				return _services->PlaceIfNoCollision((CKerName*)objType.GetCKerName().ToPointer(), x, y);
			}


			~SampleServices() {
				this->!SampleServices();
			}
			!SampleServices() {
				delete _services;
			}

		};





	}
}
