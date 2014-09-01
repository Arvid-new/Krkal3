using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Krkal.GEn
{
	enum QTreeState : byte {
		Dirty = 0,
		Free = 1,
		Full = 2,
	}

	enum QTReeQuartal {
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight,
	}

	
	class QuadTreeMask
	{
		int _sizeMax;
		public int Size {
			get { return _sizeMax; }
		}


		QTreeState[] _quadTree;

		// CONSTRUCTOR
		public QuadTreeMask(int sizeX, int sizeY) {
			_sizeMax = NearestPowerOf2(Math.Max(sizeX, sizeY));

			int arrSize = 0;
			for (int f = 0; _sizeMax >= (1 << f); f++) {
				arrSize += (1 << (f * 2));
			}

			_quadTree = new QTreeState[arrSize];
			_quadTree[0] = QTreeState.Free;
		}

		public void Add(Rectangle rect) {
			int pos = 0;
			Rectangle qRect = new Rectangle(0, 0, _sizeMax, _sizeMax);

			bool goDown;
			do {
				goDown = false;

				// test new qRect
				if (_quadTree[pos] != QTreeState.Full) {
					if (rect.IntersectsWith(qRect)) {
						if (rect.Contains(qRect)) {
							_quadTree[pos] = QTreeState.Full;
						} else {
							if (_quadTree[pos] == QTreeState.Free) {
								for (int f = (pos << 2) + 1; f <= (pos << 2) + 4; f++) {
									_quadTree[f] = QTreeState.Free;
								}
								_quadTree[pos] = QTreeState.Dirty;
							}
							goDown = true;
						}
					}
				}

			} while (GoNextAndChange(goDown, ref pos, ref qRect));
		}


		public void Clear(Rectangle rect) {
			int pos = 0;
			Rectangle qRect = new Rectangle(0, 0, _sizeMax, _sizeMax);

			bool goDown;
			do {
				goDown = false;

				// test new qRect
				if (_quadTree[pos] != QTreeState.Free) {
					if (rect.IntersectsWith(qRect)) {
						if (rect.Contains(qRect)) {
							_quadTree[pos] = QTreeState.Free;
						} else {
							if (_quadTree[pos] == QTreeState.Full) {
								for (int f = (pos << 2) + 1; f <= (pos << 2) + 4; f++) {
									_quadTree[f] = QTreeState.Full;
								}
								_quadTree[pos] = QTreeState.Dirty;
							}
							goDown = true;
						}
					}
				}

			} while (GoNextAndChange(goDown, ref pos, ref qRect));
		}


		public bool IsFree(Rectangle rect) {
			int pos = 0;
			Rectangle qRect = new Rectangle(0, 0, _sizeMax, _sizeMax);

			bool goDown;
			do {
				goDown = false;

				// test new qRect
				if (_quadTree[pos] != QTreeState.Free) {
					if (rect.IntersectsWith(qRect)) {
						if (_quadTree[pos] == QTreeState.Full)
							return false;
						if (rect.Contains(qRect)) {
							return false;
						} else {
							goDown = true;
						}
					}
				}

			} while (GoNext(goDown, ref pos, ref qRect));
			return true;
		}


		public bool Contains(Rectangle rect) {
			int pos = 0;
			Rectangle qRect = new Rectangle(0, 0, _sizeMax, _sizeMax);

			bool goDown;
			do {
				goDown = false;

				// test new qRect
				if (_quadTree[pos] != QTreeState.Full) {
					if (rect.IntersectsWith(qRect)) {
						if (_quadTree[pos] == QTreeState.Free)
							return false;
						if (rect.Contains(qRect)) {
							return false;
						} else {
							goDown = true;
						}
					}
				}

			} while (GoNext(goDown, ref pos, ref qRect));
			return true;
		}


		private bool GoNextAndChange(bool goDown, ref int pos, ref Rectangle rect) {
			if (goDown) {
				pos = (pos << 2) + 1;
				rect.Width = rect.Width >> 1;
				rect.Height = rect.Height >> 1;
			} else {
				bool goUp;
				do {
					goUp = false;
					switch (pos & 3) {
						case 0: // go up
							if (pos == 0)
								return false; // all done
							int a = (byte)_quadTree[pos] & (byte)_quadTree[pos - 1] & (byte)_quadTree[pos - 2] & (byte)_quadTree[pos - 3];
							pos = (pos - 4) >> 2;
							if ((QTreeState)a == QTreeState.Full) {
								_quadTree[pos] = QTreeState.Full;
							} else if ((QTreeState)a == QTreeState.Free) {
								_quadTree[pos] = QTreeState.Free;
							}
							goUp = true;
							rect.X = rect.X - rect.Width;
							rect.Y = rect.Y - rect.Height;
							rect.Height = rect.Height << 1;
							rect.Width = rect.Width << 1;
							break;
						case 1: // go left
						case 3:
							pos++;
							rect.X = rect.X + rect.Width;
							break;
						case 2: // go right down
							pos++;
							rect.X = rect.X - rect.Width;
							rect.Y = rect.Y + rect.Height;
							break;

					}
				} while (goUp);
			}
			return true;
		}


		private bool GoNext(bool goDown, ref int pos, ref Rectangle rect) {
			if (goDown) {
				pos = (pos << 2) + 1;
				rect.Width = rect.Width >> 1;
				rect.Height = rect.Height >> 1;
			} else {
				bool goUp;
				do {
					goUp = false;
					switch (pos & 3) {
						case 0: // go up
							if (pos == 0)
								return false; // all done
							pos = (pos - 4) >> 2;
							goUp = true;
							rect.X = rect.X - rect.Width;
							rect.Y = rect.Y - rect.Height;
							rect.Height = rect.Height << 1;
							rect.Width = rect.Width << 1;
							break;
						case 1: // go left
						case 3:
							pos++;
							rect.X = rect.X + rect.Width;
							break;
						case 2: // go right down
							pos++;
							rect.X = rect.X - rect.Width;
							rect.Y = rect.Y + rect.Height;
							break;

					}
				} while (goUp);
			}
			return true;
		}


		private static int NearestPowerOf2(int p) {
			int f = 1;
			while (p > f) f <<= 1;
			return f;
		}



		public void Expand(QTReeQuartal oldAreaPosition, QTreeState stateForNewArea) {
			if (stateForNewArea == QTreeState.Dirty)
				throw new ArgumentException();

			QTreeState[] newTree = new QTreeState[_quadTree.Length * 4 + 1];

			if (_quadTree[0] == stateForNewArea) {
				newTree[0] = stateForNewArea;
			} else {
				newTree[0] = QTreeState.Dirty;

				int size = 1;
				int size2 = 1;
				int posS = 0;
				int posD = 1;

				while (size2 <= _sizeMax) {
					for (int g = 0; g < 4; g++) {
						if (g == (int)oldAreaPosition) {
							for (int f = 0; f < size; f++) {
								newTree[posD++] = _quadTree[posS++];
							}
						} else {
							posD += size;
						}
					}
					size <<= 2;
					size2 <<= 1;
				}

				for (int g = 0; g < 4; g++) {
					if (g != (int)oldAreaPosition) {
						newTree[g + 1] = stateForNewArea;
					}
				}
			}

			_quadTree = newTree;
			_sizeMax <<= 1;
		}


		public void SetAll(QTreeState qTreeState) {
			if (qTreeState == QTreeState.Dirty)
				throw new ArgumentException();
			_quadTree[0] = qTreeState;
		}
	}
}
