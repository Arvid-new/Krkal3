using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using System.Drawing;
using Krkal.GEn.Base;
using SlimDX;

namespace Krkal.GEn
{

	class AtlasTexture
	{
		int _x = -1, _y = -1;
		public int Y {
			get { return _y; }
		}
		public int X {
			get { return _x; }
		}
		public Vector2 Coords {
			get { return new Vector2(_x, _y); }
		}
		public Vector2 Size {
			get { return new Vector2(_tex.Dx, _tex.Dy); }
		}

		internal Rectangle AtlasRectangle {
			get { return new Rectangle(_x-1, _y-1, _tex.Dx+2, _tex.Dy+2); }
		}
		
		KrkalTexture _tex;
		internal KrkalTexture KrkalTexture {
			get { return _tex; }
		}
		
		AtlasTexture _next, _prev;
		internal AtlasTexture Prev {
			get { return _prev; }
		}
		internal AtlasTexture Next {
			get { return _next; }
		}

		public bool IsHead {
			get { return (_tex == null); }
		}

		// CONSTRUCTOR
		internal AtlasTexture(TextureAtlas atlas, String name, KrkalDataType type, Ramp prm) {
			_tex = (KrkalTexture)atlas.Manager.GetOrLoadData(name, type, prm);
		}

		internal AtlasTexture() { // for list heads
		}



		internal void MoveAfter(AtlasTexture prev) {
			RemoveFromList();
			_next = prev._next;
			if (_next != null)
				_next._prev = this;
			_prev = prev;
			_prev._next = this;
		}

		private void RemoveFromList() {
			if (_prev != null)
				_prev._next = _next;
			if (_next != null)
				_next._prev = _prev;
			_prev = null;
			_next = null;
		}


		public bool IsAdded {
			get { return _x >= 0; }
		}

		internal void Remove() {
			_x = -1;
			_y = -1;
			RemoveFromList();
		}

		internal void Add(int x, int y, AtlasTexture after) {
			_x = x;
			_y = y;
			MoveAfter(after);
		}
	}







	class TextureAtlas : IDisposable
	{
		Ramp _ramp;
		internal Ramp Ramp {
			get { return _ramp; }
		}
		DataManager _manager;
		internal DataManager Manager {
			get { return _manager; }
		}
		Dictionary<String, AtlasTexture> _myTextures = new Dictionary<string, AtlasTexture>(StringComparer.InvariantCultureIgnoreCase);
		protected AtlasTexture _added = new AtlasTexture();
		AtlasTexture _toUse = new AtlasTexture();
		List<AtlasTexture> _toAdd = new List<AtlasTexture>();
		Dictionary<AtlasTexture, int> _toAddD = new Dictionary<AtlasTexture, int>();

		const int _maxSizeX = 4096, _maxSizeY = 2048;
		const int _startSizeX = 256, _startSizeY = 256;
		protected int _sizeX = _startSizeX, _sizeY = _startSizeY;
		QuadTreeMask _qTree;




		// CONSTRUCTOR
		public TextureAtlas(GraphicsDeviceManager manager, Ramp ramp) {
			_ramp = ramp;
			_manager = new DataManager(manager);
			_qTree = new QuadTreeMask(_sizeX, _sizeY);
		}



		public AtlasTexture GetOrCreateTexture(String name, KrkalDataType type) {
			AtlasTexture tex;
			if (_myTextures.TryGetValue(name, out tex))
				return tex;
			tex = new AtlasTexture(this, name, type, _ramp);
			_myTextures.Add(name, tex);
			return tex;
		}



		public void Request(AtlasTexture tex) {
			if (tex.IsAdded) {
				tex.MoveAfter(_added);
				if (!_toUse.IsAdded)
					_toUse.Add(0, 0, tex);
			} else {
				if (!_toAdd.Contains(tex)) {
					_toAdd.Add(tex);
					_toAddD.Add(tex, 0);
				}
			}
		}



		public bool LoadAllRequested() {
			if (_manager == null)
				throw new ObjectDisposedException("TextureAtlas");
			try {
				if (_toAdd.Count > 0) {
					_toAdd.Sort(delegate(AtlasTexture x, AtlasTexture y) {
						int res = y.KrkalTexture.Dx * y.KrkalTexture.Dy - x.KrkalTexture.Dx * x.KrkalTexture.Dy;
						return res != 0 ? res : y.KrkalTexture.Dy - x.KrkalTexture.Dy;
					});

					int deleteCount = 1;
					AtlasTexture toDelete = null;
					FindPlaceState state = new FindPlaceState();
					int oldX = _sizeX, oldY = _sizeY;

					foreach (AtlasTexture tex in _toAdd) {
						if (!Insert(tex, ref deleteCount, ref toDelete, state))
							return false;
					}

					if (oldX != _sizeX || oldY != _sizeY) {
						OnResize();
					} else {
						OnInsert(_toAdd);
					}
				}
			}
			finally {
				_toAdd.Clear();
				_toAddD.Clear();
				_toUse.Remove();
			}
			return true;
		}

		protected virtual void OnInsert(IList<AtlasTexture> toAdd) {}


		protected virtual void OnResize() {}



		private bool Insert(AtlasTexture tex, ref int deleteCount, ref AtlasTexture toDelete, FindPlaceState state) {
			int x, y;
			while (!FindPlace(tex.KrkalTexture.Dx+2, tex.KrkalTexture.Dy+2, out x, out y, state)) {
				if (!DeleteLast(ref deleteCount, ref toDelete))
					return false;
			}
			tex.Add(x+1, y+1, _added);
			if (!_toUse.IsAdded)
				_toUse.Add(0, 0, tex);

			Rectangle rect = tex.AtlasRectangle;

			while (_sizeX < rect.Right) _sizeX <<= 1;
			while (_sizeY < rect.Bottom) _sizeY <<= 1;

			while (_sizeX > _qTree.Size || _sizeY > _qTree.Size) {
				_qTree.Expand(QTReeQuartal.TopLeft, QTreeState.Free);
			}

			_qTree.Add(rect);

			return true;
		}




		private bool DeleteLast(ref int deleteCount, ref AtlasTexture toDelete) {

			if (toDelete == null) {
				toDelete = _added;
				while (toDelete.Next != null) {
					toDelete = toDelete.Next;
				}
			}

			if (toDelete.IsHead)
				return false;

			for (int f = 0; f < deleteCount && !toDelete.IsHead; f++) {
				AtlasTexture tex = toDelete;
				toDelete = toDelete.Prev;
				_qTree.Clear(tex.AtlasRectangle);
				tex.Remove();
			}

			deleteCount *= 2;

			return true;
		}




		#region FindPlaceState
		class FindPlaceState
		{
			int _g, _f;
			int _dx = -1, _dy = -1;
			int _oX, _oY;
			public void Store(int g, int f, int dx, int dy, int oX, int oY) {
				_g = g; _f = f;
				_dx = dx; _dy = dy;
				_oX = oX; _oY = oY;
			}
			public void ReStore(int newDX, int newDY, out int g, out int f, out int oX, out int oY) {
				if (newDX == _dx && newDY == _dy) {
					g = _g; f = _f;
					oX = _oX; oY = _oY;
				} else {
					g = 0; f = 0;
					oX = -1; oY = -1;
				}

				// clear
				_dx = -1; _dy = -1;
			}
		}
		#endregion

		//bool _test;
		private bool FindPlace(int dx, int dy, out int x, out int y, FindPlaceState state) {
			//if (!_test) {
			//    _test = true;
			//    x = 3900;
			//    y = 1900;
			//    state.Store(34, 71, dx, dy, -1, -1);
			//    return true;
			//}
			int f,g;
			state.ReStore(dx, dy, out g, out f, out x, out y);

			int maxX = GetIterCount(_sizeX, dx, _sizeX <= _sizeY);
			int maxY = GetIterCount(_sizeY, dy, _sizeX > _sizeY);

			Rectangle test = new Rectangle(0, 0, dx, dy);

			for ( ; g < maxY; g++) {
				for ( ; f < maxX; f++) {
					test.X = f * dx;
					test.Y = g * dy;
					if (test.Right <= _maxSizeX && test.Bottom <= _maxSizeY) {
						if (_qTree.IsFree(test)) {
							if (test.Right <= _sizeX && test.Bottom <= _sizeY) {
								state.Store(g, f, dx, dy, x, y);
								x = test.X;
								y = test.Y;
								return true;
							} else if (x == -1) {
								x = test.X;
								y = test.Y;
							}
						}
					}
				}
				f = 0;
			}
			return (x != -1);
		}



		private int GetIterCount(int size, int dx, bool expand) {
			int ret = size / dx;
			if (expand) {
				if (ret * dx == size) {
					ret++;
				} else {
					ret += 2;
				}
			}
			if (ret == 0) {
				ret++;
			}
			return ret;
		}



		void DeleteAll() {
			if (_toUse.IsAdded || _toAdd.Count > 0)
				throw new InternalGEnException("Cannot do delete while adding");
			while (_added.Next != null) {
				_added.Next.Remove();
			}
			_qTree.SetAll(QTreeState.Free);
		}


		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			try {
				_manager.Dispose();
			}
			finally {
				_manager = null;
				_myTextures = null;
			}			
		}



	}






	class KrkalTextureAtlas : TextureAtlas, IResource
	{
		Texture _color;
		public Texture Color {
			get { return _color; }
		}
		Texture _normal;
		public Texture Normal {
			get { return _normal; }
		}
		Texture _coefficients;
		public Texture Coefficients {
			get { return _coefficients; }
		}
		GraphicsDeviceManager _manager;
		Device Device {
			get { return _manager.Direct3D9.Device; }
		}
		public Vector2 InvSize {
			get { return new Vector2(1f/(float)_sizeX, 1f/(float)_sizeY); }
		}

		AtlasTexture _empty;
		internal AtlasTexture Empty {
			get { return _empty; }
		}


		// CONSTRUCTOR
		public KrkalTextureAtlas(GraphicsDeviceManager manager, Ramp ramp): base(manager, ramp) {
			_manager = manager;
			_empty = this.GetOrCreateTexture("Empty", KrkalDataType.EmptyKrkalTexture);
		}


		public void RequestEmpty() {
			Request(_empty);
		}


		public void Initialize(GraphicsDeviceManager graphicsDeviceManager) {			
		}

		public void LoadContent() {
			_color = new Texture(Device, _sizeX, _sizeY, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
			_normal = new Texture(Device, _sizeX, _sizeY, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
			_coefficients = new Texture(Device, _sizeX, _sizeY, 1, Usage.Dynamic, Format.A8L8, Pool.Default);
			DataRectangle colorR = _color.LockRectangle(0, LockFlags.Discard);
			DataRectangle normalR = _normal.LockRectangle(0, LockFlags.Discard);
			DataRectangle coefR = _coefficients.LockRectangle(0, LockFlags.Discard);
			for (AtlasTexture tex = _added; tex != null; tex = tex.Next) {
				Write(tex, colorR, normalR, coefR);
			}
			Unlock();
		}

		private void Unlock() {
			_color.UnlockRectangle(0);
			_normal.UnlockRectangle(0);
			_coefficients.UnlockRectangle(0);
		}

		private void Write(AtlasTexture tex, DataRectangle colorR, DataRectangle normalR, DataRectangle coefR) {
			if (!tex.IsHead) {
				tex.KrkalTexture.CopyData(colorR, tex.X, tex.Y, 0, 4);
				tex.KrkalTexture.CopyData(normalR, tex.X, tex.Y, 4, 4);
				tex.KrkalTexture.CopyData(coefR, tex.X, tex.Y, 8, 2);
			}
		}

		public void UnloadContent() {
			FreeTextures();
		}



		private void FreeTextures() {
			if (_color != null) {
				_color.Dispose();
				_color = null;
				_normal.Dispose();
				_normal = null;
				_coefficients.Dispose();
				_coefficients = null;
			}
		}

		protected override void Dispose(bool disposing) {
			FreeTextures();
			base.Dispose(disposing);
		}


		protected override void OnResize() {
			base.OnResize();
			FreeTextures();
			LoadContent();
		}


		protected override void OnInsert(IList<AtlasTexture> toAdd) {
			base.OnInsert(toAdd);
			if (_color != null) {
				DataRectangle colorR = _color.LockRectangle(0, LockFlags.None);
				DataRectangle normalR = _normal.LockRectangle(0, LockFlags.None);
				DataRectangle coefR = _coefficients.LockRectangle(0, LockFlags.None);
				foreach (AtlasTexture tex in toAdd) {
					Write(tex, colorR, normalR, coefR);
				}
				Unlock();
			}
		}
	}
}
