using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using SlimDX;
using System.Runtime.InteropServices;
using Krkal.GEn.Base;

namespace Krkal.GEn
{
	class KrkalTexture : KrkalData
	{

		Byte[] _data;
		int _dx, _dy, _channels;

		public int Channels {
			get { return _channels; }
		}
		public int Dy {
			get { return _dy; }
		}
		public int Dx {
			get { return _dx; }
		}



		protected override void LoadData(Device device, object prm) {
			switch (Type) {
				case KrkalDataType.KrkalTexture:
					LoadKrkal(device, (Ramp)prm);
					break;
				case KrkalDataType.EmptyKrkalTexture:
					LoadEmpty();
					break;
				default:
					throw new InternalGEnException("unknown texture type");
			}
		}

		private void LoadEmpty() {
			_channels = _krkalFormatCfg.Length;
			_dx = 1;
			_dy = 1;
			_data = new byte[_channels * _dx * _dy];
		}


		struct KrkalFormatCfg
		{
			public String ext;
			public String ext2;
			public int chS;
			public int chD;
			public byte def;
			public bool invert;
			public int ramp;
		}

		static readonly KrkalFormatCfg[] _krkalFormatCfg = {
			new KrkalFormatCfg { chS = 0, chD = 0, def=100, ext = "_KRKAL Diffuse.png", ext2 = "_KRKAL_Diffuse.png"},	// b
			new KrkalFormatCfg { chS = 1, chD = 1, def=100, ext = null, ext2 = null},									// g
			new KrkalFormatCfg { chS = 2, chD = 2, def=100, ext = null, ext2 = null},									// r
			new KrkalFormatCfg { chS = 3, chD = 3, def=255, ext = "_KRKAL Alpha.png", ext2 = "_KRKAL_Alpha.png"},		// a
			new KrkalFormatCfg { chS = 3, chD = 9, def=255, ext = null, ext2 = null},									// a
			new KrkalFormatCfg { chS = 0, chD = 4, def=255, ext = "_KRKAL Normal.png", ext2 = "_KRKAL_Normal.png"},		// z
			new KrkalFormatCfg { chS = 1, chD = 5, def=128, ext = null, ext2 = null, invert = true},					// y
			new KrkalFormatCfg { chS = 2, chD = 6, def=128, ext = null, ext2 = null},									// x
			new KrkalFormatCfg { chS = 0, chD = 7, def=0, ext = "_KRKAL Coefficients.png", ext2 = null, ramp=5},		// material
			new KrkalFormatCfg { chS = 0, chD = 8, def=0, ext = "_KRKAL Z Depth.png", ext2 = "_KRKAL_Z_Depth.png"},		// height
		};


		private void LoadKrkal(Device device, Ramp ramp) {
			_channels = _krkalFormatCfg.Length;
			ramp.LoadRampConfiguration("DefaultRampCfg");

			Texture texture = null;
			byte[] sdata = new byte[4];
			DataRectangle src = null;
		
			try {
				foreach (KrkalFormatCfg cfg in _krkalFormatCfg) {
					if (cfg.ext != null) {
						if (texture != null) {
							texture.UnlockRectangle(0);
							texture.Dispose();
							texture = null;
						}
						ImageInformation imageInfo = new ImageInformation();
						try {
							texture = Texture.FromFile(device, Name + cfg.ext, 0, 0, 1, Usage.None, Format.A8R8G8B8, Pool.SystemMemory, Filter.None, Filter.None, 0, out imageInfo);
						}
						catch (Direct3D9Exception) {
							texture = null;
						}
						if (texture == null && cfg.ext2 != null) {
							try {
								texture = Texture.FromFile(device, Name + cfg.ext2, 0, 0, 1, Usage.None, Format.A8R8G8B8, Pool.SystemMemory, Filter.None, Filter.None, 0, out imageInfo);
							} 
							catch (Direct3D9Exception) {
								texture = null;
							}
						}
						if (texture != null && _data == null) {
							_dx = imageInfo.Width;
							_dy = imageInfo.Height;
							_data = new byte[_channels * _dx * _dy];
						}
						if (texture != null)
							src = texture.LockRectangle(0, LockFlags.ReadOnly);
					}

					if (_data == null)
						throw new InternalGEnException("Failed to load texture file");


					for (int y = 0; y < _dy; y++) {
						if (texture != null) {
							src.Data.Seek(y * src.Pitch, System.IO.SeekOrigin.Begin);
						}
						for (int x = 0; x < _dx; x++) {
							byte aa = cfg.def;
							if (texture != null) {
								src.Data.Read(sdata, 0, 4);
								aa = sdata[cfg.chS];
								if (cfg.invert)
									aa = (byte)(255 - aa);
							}
							if (cfg.ramp > 0)
								aa = (byte)ramp.GetPosition(cfg.ramp-1, aa); 
							_data[cfg.chD + x*_channels + y*_channels*_dx] = aa;
						}
					}

				}


				//for (int y = 0; y < _dy; y++) {
				//    for (int x = 0; x < _dx; x++) {
				//        int ptr = x * _channels + y * _channels * _dx;
				//        if (_data[ptr + 3] == 0)
				//            _data[ptr + 7] = 255; // if the aplpha is fully transparent set z to 255 -> pixel will be skipped
				//    }
				//}
			}
			finally {
				if (texture != null) {
					texture.UnlockRectangle(0);
					texture.Dispose();
				}
			}
		}

		protected override void UnLoadData() {
			_data = null;
		}


		public void CopyData(DataRectangle dest, int x, int y, int channel, int numChannels) {
			for (int g = 0; g < _dy; g++) {
				dest.Data.Seek((y+g) * dest.Pitch + (x-1) * numChannels, System.IO.SeekOrigin.Begin);
				dest.Data.Write(_data, g * _dx * _channels + 0 + channel, numChannels); // diplicate 1st pixel
				for (int f = 0; f < _dx; f++) {
					dest.Data.Write(_data, g * _dx * _channels + f * _channels + channel, numChannels);
				}
				dest.Data.Write(_data, g * _dx * _channels + (_dx - 1) * _channels + channel, numChannels); // diplicate last pixel
			}

			// duplicate 1st row
			dest.Data.Seek((y - 1) * dest.Pitch + (x - 1) * numChannels, System.IO.SeekOrigin.Begin);
			dest.Data.Write(_data, 0 + channel, numChannels); // diplicate 1st pixel
			for (int f = 0; f < _dx; f++) {
				dest.Data.Write(_data, 0 + f * _channels + channel, numChannels);
			}
			dest.Data.Write(_data, 0 + (_dx - 1) * _channels + channel, numChannels); // diplicate last pixel

			// duplicate last row
			dest.Data.Seek((y + _dy) * dest.Pitch + (x - 1) * numChannels, System.IO.SeekOrigin.Begin);
			dest.Data.Write(_data, (_dy - 1) * _dx * _channels + 0 + channel, numChannels); // diplicate 1st pixel
			for (int f = 0; f < _dx; f++) {
				dest.Data.Write(_data, (_dy - 1) * _dx * _channels + f * _channels + channel, numChannels);
			}
			dest.Data.Write(_data, (_dy - 1) * _dx * _channels + (_dx - 1) * _channels + channel, numChannels); // diplicate last pixel
		}
	}










}
