using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{

	struct AxisSelector
	{
		int _a1, _a2, _a3;
		public int A3 {
			get { return _a3; }
			set { _a3 = value; }
		}
		public int A2 {
			get { return _a2; }
			set { _a2 = value; }
		}
		public int A1 {
			get { return _a1; }
			set { _a1 = value; }
		}

		public AxisSelector(int a1, int a2, int a3) {
			_a1 = a1;
			_a2 = a2;
			_a3 = a3;
		}
	}


	class World
	{

		Vector3[] _boundPlanes;
		public Vector3 GetBoundPlane(int axis) {
			return _boundPlanes[axis];
		}
		public int BoundPlanesCount {
			get { return _boundPlanes.Length; }
		}
		uint[] _neigbours;
		bool IsNeigbour(int a, int b) {
			return (((1 << a) & _neigbours[b]) != 0);
		}


		//Texture3D _mainTexture;

		//List<Mover> _movers = new List<Mover>();

		ulong _tagCounter;
		public ulong GetNewTag() {
			_tagCounter++;
			return _tagCounter;
		}


		// CONSTRUCTOR
		public World(int boundPlanesCount) {
			_boundPlanes = new Vector3[boundPlanesCount];
			_neigbours = new uint[boundPlanesCount];
		}

		public void SetBoundPlane(int axis, Vector3 direction, uint neigbours) {
			Vector3.Normalize(ref direction, out _boundPlanes[axis]);
			_neigbours[axis] = neigbours;
		}


		public void FindNearestPiont(Vector3 direction, out Matrix toGetIt, out AxisSelector axis) {
			direction.Normalize();

			// first select the one with highest dot product:
			axis = new AxisSelector(0, -1, -1);
			for (int f = 0; f < BoundPlanesCount; f++) {
				if (Math.Abs(Vector3.Dot(_boundPlanes[f], direction)) > Math.Abs(Vector3.Dot(_boundPlanes[axis.A1], direction))) {
					axis.A1 = f;
				}
			}
			float dir = Vector3.Dot(direction, _boundPlanes[axis.A1]) >= 0 ? 1 : -1;
			Vector3 v1 = _boundPlanes[axis.A1] * dir;
			Vector3 hs1 = MakeAcceptableHalfSpace(direction, v1);

			// select best first neigbour
			Vector3 v2 = new Vector3();
			for (int f = 0; f < BoundPlanesCount; f++) {
				if (IsNeigbour(axis.A1, f)) {
					dir = Vector3.Dot(hs1, _boundPlanes[f]) >= 0 ? 1 : -1;
					Vector3 v = _boundPlanes[f] * dir;
					if (axis.A2 == -1) {
						axis.A2 = f;
						v2 = v;
					} else {
						if (Vector3.Dot(v, direction) > Vector3.Dot(v2, direction)) {
							axis.A2 = f;
							v2 = v;
						}
					}
				}
			}
			Vector3 hs2 = MakeAcceptableHalfSpace(direction, v2);

			// select best second neigbour
			Vector3 v3 = new Vector3();
			for (int f = 0; f < BoundPlanesCount; f++) {
				if (IsNeigbour(axis.A1, f) && IsNeigbour(axis.A2, f)) {
					float aa = Vector3.Dot(hs1, _boundPlanes[f]);
					float bb = Vector3.Dot(hs2, _boundPlanes[f]);
					if (bb * aa >= 0 /*same sign or one is 0*/) {
						if (Math.Abs(aa) > Math.Abs(bb)) {
							dir = aa >= 0 ? 1 : -1;
						} else {
							dir = bb >= 0 ? 1 : -1;
						}
						Vector3 v = _boundPlanes[f] * dir;
						if (axis.A3 == -1) {
							axis.A3 = f;
							v3 = v;
						} else {
							if (Vector3.Dot(v, direction) > Vector3.Dot(v3, direction)) {
								axis.A3 = f;
								v3 = v;
							}
						}
					}
				}
			}


			// fill the axis directions to columns
			toGetIt = MatrixUtilities.CreateFromColumns(v1, v2, v3);

			toGetIt.Invert();
		}

		private Vector3 MakeAcceptableHalfSpace(Vector3 direction, Vector3 a) {
			// (a x d) x a - See http://en.wikipedia.org/wiki/Triple_product
			return direction - Vector3.Dot(direction, a) * a;
		}
	}
}
