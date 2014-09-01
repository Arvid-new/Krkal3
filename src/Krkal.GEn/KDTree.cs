using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{

	class KDTreeAxisInfo
	{
		struct ElInfo
		{
			public ElInfo(int axis, Vector3 planeDir, Elem elem) {
				_elem = elem;
				_d = Vector3.Dot(elem.Position, planeDir);
				_size = elem.GetSizeOnAxis(axis);
			}
			float _d, _size;
			public float Size {
				get { return _size; }
			}
			public float D {
				get { return _d; }
			}
			Elem _elem;
			public Elem Elem {
				get { return _elem; }
			}
		}

		List<ElInfo> _sorted;
		public IEnumerable<Elem> Elements {
			get { foreach (ElInfo i in _sorted) yield return i.Elem; }
		}
		public int Count {
			get { return _sorted.Count; }
		}
		Vector2 _limits;
		public Vector2 Limits {
			get { return _limits; }
		}
		Vector2 _moments;
		public Vector2 Moments {
			get { return _moments; }
		}
		float _divD;
		public float DivD {
			get { return _divD; }
		}
		int _divI;

		

		// CONSTRUCTOR
		public KDTreeAxisInfo(int axis, Vector3 planeDir, IEnumerable<Elem> elems) {
			_sorted = new List<ElInfo>();
			foreach (Elem e in elems) {
				_sorted.Add(new ElInfo(axis, planeDir, e));
			}
			Sort();
			CalculateBoundingLimits();
		}


		public KDTreeAxisInfo(KDTreeAxisInfo father, ulong tag, ulong tag2, float min, float max) {
			_sorted = new List<ElInfo>();
			foreach (ElInfo e in father._sorted) {
				if (e.Elem.Tag == tag || e.Elem.Tag == tag2)
					_sorted.Add(e);
			}
			_limits.X = min;
			_limits.Y = max;
			RestrictBoundingLimits();
		}

		void Sort() {
			_sorted.Sort(delegate(ElInfo x, ElInfo y) {
				return (x.D + x.Size) < (y.D + y.Size) ? -1 : 1;
			});
		}

		void CalculateBoundingLimits() {
			if (_sorted.Count > 0) {
				_limits.X = _sorted[0].D - _sorted[0].Size;
				_limits.Y = _sorted[0].D + _sorted[0].Size;
			} else {
				_limits.X = 0;
				_limits.Y = 0;
			}

			foreach (ElInfo e in _sorted) {
				if (e.D - e.Size < _limits.X)
					_limits.X = e.D - e.Size;
				if (e.D + e.Size > _limits.Y)
					_limits.Y = e.D + e.Size;
			}
		}

		void RestrictBoundingLimits() {
			if (_sorted.Count == 0) {
				_limits.X = 0;
				_limits.Y = 0;
				return;
			}

			float min = _sorted[0].D - _sorted[0].Size;
			float max = _sorted[0].D + _sorted[0].Size;

			foreach (ElInfo e in _sorted) {
				if (e.D - e.Size < min)
					min = e.D - e.Size;
				if (e.D + e.Size > max)
					max = e.D + e.Size;
			}

			if (min > _limits.X)
				_limits.X = min;
			if (max < _limits.Y)
				_limits.Y = max;
		}



		internal void CalculateMoments() {
			if (_sorted.Count == 0) {
				_moments.X = 0;
				_moments.Y = 0;
				return;
			}

			float a = 0;
			foreach (ElInfo e in _sorted) {
				a += e.D;
			}

			_moments.X = a / _sorted.Count;
			a = 0;

			foreach (ElInfo e in _sorted) {
				float d = e.D - _moments.X;
				d = d * d;
				a += d;
			}

			_moments.Y = a / _sorted.Count;
		}

		internal int TryToDivide(int tries) {
			int i, i1, i2;
			i = i1 = i2 = _sorted.Count / 2;
			int Bscore = 0;

			int f = 0;
			while ( true ) {
				int score=0;
				float D = _sorted[i].D + _sorted[i].Size;
				for (int g = i + 1; g < _sorted.Count; g++) {
					if (_sorted[g].D - _sorted[g].Size >= D)
						score++;
				}
				if (score > i + 1)
					score = i + 1;
				if (score > Bscore) {
					Bscore = score;
					_divD = D;
					_divI = i;
				}

				f++;
				if (f < tries) {
					if ((f & 1) == 0) {
						D = _sorted[i2].D + _sorted[i2].Size;
						do {
							i2++;
							if (i2 >= _sorted.Count)
								break;
						} while (_sorted[i2].D + _sorted[i2].Size <= D);
						if (i2 >= _sorted.Count)
							break;
						i = i2;
					} else {
						D = _sorted[i1].D + _sorted[i1].Size;
						do {
							i1--;
							if (i1 < 0)
								break;
						} while (_sorted[i1].D + _sorted[i1].Size >= D);
						if (i1<0)
							break;
						i = i1;
					}
				} else {
					break;
				}
			}

			return Bscore;
		}



		internal void Divide(int axis, World world, ulong tag) {
			for (int i = 0; i <= _divI; i++) {
				_sorted[i].Elem.Tag = tag;
			}
			for (int i = _divI+1; i < _sorted.Count; i++) {
				if (_sorted[i].D - _sorted[i].Size >= _divD) {
					_sorted[i].Elem.Tag = tag + 2;
				} else {
					switch (_sorted[i].Elem.PlaneTest(axis, _divD, world)) {
						case PlaneIntersectionType.Back:
							_sorted[i].Elem.Tag = tag;
							break;
						case PlaneIntersectionType.Intersecting:
							_sorted[i].Elem.Tag = tag + 1;
							break;
						case PlaneIntersectionType.Front:
							_sorted[i].Elem.Tag = tag + 2;
							break;
					}
				}
			}
		}
	}


	class KDTree
	{
		KDTreeNode _root;
		World _world;

		const int BucketSize = 4;

		// CONSTRUCTOR
		public KDTree(World world, IEnumerable<Elem> elems) {
			_world = world;

			KDTreeAxisInfo[] ai = new KDTreeAxisInfo[_world.BoundPlanesCount];
			for (int axis = 0; axis < _world.BoundPlanesCount; axis++) {
				ai[axis] = new KDTreeAxisInfo(axis, _world.GetBoundPlane(axis), elems);
			}

			_root = TryToDivide(ai);
		}

		private KDTreeNode TryToDivide(KDTreeAxisInfo[] ai) {
			if (ai[0].Count <= BucketSize) {
				return new KDTreeLeaf(ai);
			}

			float bestM = 0;
			int bestA = 0;
			int aa = 0;
			foreach (KDTreeAxisInfo i in ai) {
				i.CalculateMoments();
				if (i.Moments.Y > bestM) {
					bestM = i.Moments.Y;
					bestA = aa;
				}
				aa++;
			}

			int score = ai[bestA].TryToDivide(1);
			if (score == 0) {
				return new KDTreeLeaf(ai);
				// IMPROVEMENT> try other axis?
			}

			ulong tag = _world.GetNewTag();
			_world.GetNewTag(); // tag2
			_world.GetNewTag(); // tag3

			ai[bestA].Divide(bestA, _world, tag);

			KDTreeAxisInfo[] ai1 = new KDTreeAxisInfo[_world.BoundPlanesCount];
			KDTreeAxisInfo[] ai2 = new KDTreeAxisInfo[_world.BoundPlanesCount];
			for (int axis = 0; axis < _world.BoundPlanesCount; axis++) {
				ai1[axis] = new KDTreeAxisInfo(ai[axis], tag, tag+1, ai[axis].Limits.X, (axis == bestA ? ai[axis].DivD : ai[axis].Limits.Y));
				ai2[axis] = new KDTreeAxisInfo(ai[axis], tag + 2, tag + 1, (axis == bestA ? ai[axis].DivD : ai[axis].Limits.X), ai[axis].Limits.Y);
			}

			KDTreeNode n1 = TryToDivide(ai1);
			KDTreeNode n2 = TryToDivide(ai2);

			return new KDTreeInnerNode(ai, bestA, n1, n2);
		}

		//public KDTree(World world) {
		//    _world = world;
		//    _root = new KDTreeLeaf();
		//}
	}

	class AxisBB {
		Vector2[] _axisLimits;
		public Vector2[] AxisLimits {
			get { return _axisLimits; }
		}

		public AxisBB(KDTreeAxisInfo[] ai) {
			_axisLimits = new Vector2[ai.Length];
			for (int f=0; f< ai.Length; f++) {
				_axisLimits[f] = ai[f].Limits;
			}
		}
	}

	class KDTreeNode : AxisBB
	{
		bool _isLeaf;
		public bool IsLeaf {
			get { return _isLeaf; }
		}

		public KDTreeNode(bool isLeaf, KDTreeAxisInfo[] ai) : base(ai) {
			_isLeaf = isLeaf;
		}
	}

	class KDTreeInnerNode : KDTreeNode
	{
		KDTreeNode _front, _back;
		int _axis;
		float _d;

		public KDTreeInnerNode(KDTreeAxisInfo[] ai, int axis, KDTreeNode back, KDTreeNode front) : base(false, ai) {
			_axis = axis;
			_front = front;
			_back = back;
			_d = ai[axis].DivD;
		}
	}

	class KDTreeLeaf : KDTreeNode
	{
		LinkedList<Elem> _elems = new LinkedList<Elem>();


		public KDTreeLeaf(KDTreeAxisInfo[] ai) : base(true, ai) {
			foreach (Elem el in ai[0].Elements) {
				_elems.AddLast(el);
			}
		}
	}
}
