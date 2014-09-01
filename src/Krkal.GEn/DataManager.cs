using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using Krkal.GEn.Base;

namespace Krkal.GEn
{


	public enum KrkalDataType
	{
		KrkalTexture,
		ElementGeometry,
		EmptyKrkalTexture,
		RampFunctionComputing,
		RampConfiguration,
		MaterialInfo,
		MeshSource,
		ManagedTexture,
	}


	abstract class KrkalData
	{
		String _name;
		public String Name {
			get { return _name; }
		}
		KrkalDataType _type;
		public KrkalDataType Type {
			get { return _type; }
		}

		int _refCounter;


		public void Load(Device device, object prm) {
			_refCounter++;
			if (_refCounter == 1)
				LoadData(device, prm);
		}

		protected abstract void LoadData(Device device, object prm);

		public void UnLoad() {
			if (_refCounter > 0)
				_refCounter--;
			if (_refCounter == 0) {
				UnLoadData();
			}
		}

		protected abstract void UnLoadData();


		internal static KrkalData Create(string name, KrkalDataType type) {
			KrkalData ret;
			switch (type) {
				case KrkalDataType.KrkalTexture:
				case KrkalDataType.EmptyKrkalTexture:
					ret = new KrkalTexture();
					break;
				case KrkalDataType.ElementGeometry:
					ret = new ElementGeometry();
					break;
				case KrkalDataType.RampFunctionComputing:
					ret = new RampFunctionComputing();
					break;
				case KrkalDataType.RampConfiguration:
					ret = new RampConfiguration();
					break;
				case KrkalDataType.MaterialInfo:
					ret = new MaterialInfo();
					break;
				case KrkalDataType.MeshSource:
					ret = new MeshSource();
					break;
				case KrkalDataType.ManagedTexture:
					ret = new ManagedTexture();
					break;
				default:
					throw new InternalGEnException("unknown data type");
			}
			ret._name = name;
			ret._type = type;
			return ret;
		}
	}









	class DataManager : IDisposable
	{
		Dictionary<String, KrkalData> _myData = new Dictionary<string, KrkalData>(StringComparer.InvariantCultureIgnoreCase);
		static Dictionary<String, KrkalData> _allData = new Dictionary<string, KrkalData>(StringComparer.InvariantCultureIgnoreCase);

		GraphicsDeviceManager _manager;
		Device Device {
			get { return _manager.Direct3D9.Device; }
		}

		// CONSTRUCTOR
		public DataManager(GraphicsDeviceManager manager) {
			_manager = manager;
		}

		public KrkalData GetOrLoadData(String name, KrkalDataType type) {
			return GetOrLoadData(name, type, null);
		}
		public KrkalData GetOrLoadData(String name, KrkalDataType type, object prm) {
			if (_myData == null)
				throw new ObjectDisposedException("DataManager");
			KrkalData tex;
			if (_myData.TryGetValue(name, out tex))
				return tex;
			if (!_allData.TryGetValue(name, out tex)) {
				tex = KrkalData.Create(name, type);
				_allData.Add(name, tex);
			}
			tex.Load(Device, prm);
			_myData.Add(name, tex);
			return tex;
		}

		public void Dispose() {
			try {
				if (_myData != null) {
					foreach (KrkalData tex in _myData.Values) {
						tex.UnLoad();
					}
				}
			}
			finally {
				_myData = null;
			}
		}

	}
}
