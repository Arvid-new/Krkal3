using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Krkal.GEn.Base;
using SlimDX.Direct3D9;
using SlimDX;

namespace Krkal.GEn
{

	abstract class RampFunction : KrkalData
	{
		public abstract IEnumerable<byte> GetData();
		public const int FunctionSize = 256;

	}


	enum ComputingFunctionType
	{
		None,
		LinearAtten,
		PowerAtten,
		Diffuse,
		Specular,
		BezierAtten,
		CellShade,
	}

	class RampFunctionComputing : RampFunction
	{
		double[] _p;
		private double GetP(int index) {
			if (_p == null || index >= _p.Length)
				return 0;
			return _p[index];
		}
		ComputingFunctionType _type;


		public override IEnumerable<byte> GetData() {
			double ret = 0;
			for (int f = 0; f < FunctionSize; f++) {
				switch (_type) {
					case ComputingFunctionType.LinearAtten: {
							ret = (double)f / (FunctionSize - 1);
							ret = (1 - Math.Sqrt(ret)) * GetP(0);
							break;
						}
					case ComputingFunctionType.PowerAtten: {
							ret = (double)f / (FunctionSize - 1);
							ret = Math.Pow((1 - Math.Sqrt(ret)), GetP(1)) * GetP(0);
							break;
						}
					case ComputingFunctionType.BezierAtten: 
						{
							double[] lerp = new double[_p.Length+1];
							ret = (double)f / (FunctionSize - 1);
							ret = Math.Sqrt(ret);
							_p.CopyTo(lerp, 0); // last lerp is 0
							for (int iter = 0; iter < _p.Length; iter++) {
								for (int i = 0; i < _p.Length - iter; i++) {
									lerp[i] = lerp[i] * (1 - ret) + lerp[i + 1] * ret;
								}
							}
							ret = lerp[0];
							break;
						}
					case ComputingFunctionType.Diffuse: 
						{
							int a = Math.Max(0, f * 2 - FunctionSize+1);
							ret = (double)a * GetP(0) / (double)(FunctionSize - 1);
							break;
						}
					case ComputingFunctionType.Specular: {
							int a = Math.Max(0, f * 2 - FunctionSize+1);
							ret = (double)a / (double)(FunctionSize - 1);
							ret = Math.Pow(ret, GetP(1)) * GetP(0);
							break;
						}
					case ComputingFunctionType.CellShade: {
							double a = (double)f / (FunctionSize - 1);
							ret = _p[0];
							for (int ff = 1; ff < _p.Length && a > _p[ff]; ff+=2) {
								ret = _p[ff + 1];
							}
							break;
						}
					default:
						throw new InternalGEnException("unknown ramp function");
				}
				ret = Math.Round(ret * 255);
				if (ret < 0)
					ret = 0;
				if (ret > 255)
					ret = 255;
				yield return (byte)ret;
			}
		}

		protected override void LoadData(SlimDX.Direct3D9.Device device, object prm) {
			String[] info = Name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			_type = (ComputingFunctionType)Enum.Parse(typeof(ComputingFunctionType), info[1], true);
			if (info.Length >= 3) {
				_p = new double[info.Length - 2];
				for (int f = 2; f < info.Length; f++) {
					_p[f - 2] = double.Parse(info[f], CultureInfo.InvariantCulture);
				}
			}
		}

		protected override void UnLoadData() {
		}

	}



	class MaterialInfo : RampFunction
	{
		byte[] data;

		
		public override IEnumerable<byte> GetData() {
			return data;
		}

		protected override void LoadData(Device device, object prm) {
			data = new byte[4];
			Ramp ramp = (Ramp)prm;

			if (Name == "DefaultMaterialInfo") {
				data[0] = (byte)ramp.Request(1, "RCF Specular 0.6 8");
				data[1] = 0; // %emissive
				data[2] = (byte)ramp.Request(2, "RCF Diffuse 0.7");
				data[3] = 90; // % matarial color in specular
				//data[0] = (byte)ramp.Request(1, "RCF CellShade 0 0.94 0.6");
				//data[1] = 0; // %emissive
				//data[2] = (byte)ramp.Request(2, "RCF CellShade 0 0.54 0.8");
				//data[3] = 90; // % matarial color in specular
			} else {
				throw new NotImplementedException();
			}
		}

		protected override void UnLoadData() {
			data = null;
		}
	}



	class Ramp : IResource
	{
		public const int TextureYSize = 256;

		DataManager _dataManager;
		GraphicsDeviceManager _graphicsManager;
		Texture _texture;
		public Texture Texture {
			get {
				Reload();
				return _texture; 
			}
		}
		bool _dirty = true;

		Dictionary<RampFunction, int>[] _positions = new Dictionary<RampFunction, int>[5];
		Dictionary<int, int>[] _indexToPos = new Dictionary<int, int>[5];
		int[] _notUsedPosition = new int[5] { 3, 3, 3, 3, 1 };

		Device Device {
			get { return _graphicsManager.Direct3D9.Device; }
		}

		// CONSTRUCTOR
		public Ramp(GraphicsDeviceManager manager) {
			_graphicsManager = manager;
			_dataManager = new DataManager(manager);

			for (int f = 0; f < 5; f++) {
				_positions[f] = new Dictionary<RampFunction, int>();
				_indexToPos[f] = new Dictionary<int, int>();
			}
		}


		public void Initialize(GraphicsDeviceManager graphicsDeviceManager) {
			_texture = new Texture(Device, RampFunction.FunctionSize, TextureYSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
			_dirty = true;
			Reload();
		}

		public void LoadContent() {
		}

		public void UnloadContent() {
		}


		public void Dispose() {
			if (_texture != null)
				_texture.Dispose();
			_texture = null;
			_dataManager.Dispose();
		}


		public int Request(int channel, String name) {
			return Request(channel, name, -1);
		}

		public int Request(int channel, String name, int key) {
			RampFunction func;
			if (channel == 4) {
				func = (RampFunction)_dataManager.GetOrLoadData(name, KrkalDataType.MaterialInfo, this);
			} else {
				func = (RampFunction)_dataManager.GetOrLoadData(name, KrkalDataType.RampFunctionComputing);
			}
			int pos;
			if (!_positions[channel].TryGetValue(func, out pos)) {
				_dirty = true;
				pos = _notUsedPosition[channel];
				if (pos >= TextureYSize)
					throw new InternalGEnException("Too many ramp functions");
				_notUsedPosition[channel] += 2;
				_positions[channel].Add(func, pos);
			}
			if (key >= 0) {
				_indexToPos[channel].Remove(key);
				_indexToPos[channel].Add(key, pos);
			}
			return pos;
		}


		public void LoadRampConfiguration(String name) {
			RampConfiguration cfg = (RampConfiguration)_dataManager.GetOrLoadData(name, KrkalDataType.RampConfiguration);
			cfg.Apply(this);
		}

		public int GetPosition(int channel, int key) {
			return _indexToPos[channel][key];
		}


		public void Reload() {
			if (_dirty) {
				_dirty = false;
				DataRectangle data = _texture.LockRectangle(0, LockFlags.Discard);
				for (int ch = 0; ch < 4; ch++) {
					foreach (var pair in _positions[ch]) {
						int f = 0;
						foreach (byte value in pair.Key.GetData()) {
							data.Data.Seek((pair.Value - 1) * data.Pitch + f * 4 + ch, System.IO.SeekOrigin.Begin);
							data.Data.Write(value);
							data.Data.Seek((pair.Value) * data.Pitch + f * 4 + ch, System.IO.SeekOrigin.Begin);
							data.Data.Write(value);
							f++;
						}
					}
				}
				foreach (var pair in _positions[4]) { // write to all 4 channels in 0st row
					int f = 0;
					foreach (byte value in pair.Key.GetData()) {
						data.Data.Seek((pair.Value - 1) * 4 + f, System.IO.SeekOrigin.Begin);
						data.Data.Write(value);
						data.Data.Seek((pair.Value) * 4 + f, System.IO.SeekOrigin.Begin);
						data.Data.Write(value);
						f++;
					}
				}
				_texture.UnlockRectangle(0);
			}
		}
	}



	class RampConfiguration : KrkalData
	{

		#region Data
		class Data
		{
			String _name;
			public String Name {
				get { return _name; }
			}
			int _channel;
			public int Channel {
				get { return _channel; }
			}
			int _key;
			public int Key {
				get { return _key; }
			}

			// CONSTRUCTOR
			public Data(String name, int channel, int key) {
				_name = name;
				_channel = channel;
				_key = key;
			}
		}
		#endregion

		List<Data> _data = new List<Data>();


		protected override void LoadData(Device device, object prm) {
			if (Name == "DefaultRampCfg") {
				_data.Add(new Data("DefaultMaterialInfo", 4, 0));
				//_data.Add(new Data("RCF LinearAtten 1", 0, 0));
				_data.Add(new Data("RCF PowerAtten 1 1.8", 0, 0));
				//_data.Add(new Data("RCF BezierAtten 0 2 -1 -1 2", 0, 0));
			} else {
				throw new NotImplementedException();
			}
		}

		protected override void UnLoadData() {
			_data.Clear();
		}

		internal void Apply(Ramp ramp) {
			foreach (Data data in _data) {
				ramp.Request(data.Channel, data.Name, data.Key);
			}
		}
	}
}
