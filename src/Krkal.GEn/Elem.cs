using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	enum CollisionUpdateType
	{
		Remove,
		Move,
		Add,
	}

	[Flags]
	enum ElemTags
	{
		None = 0,

		Visible = 1,
		AcceptsLight = 2,
		CastsBBShadow = 4,
		CastsTextureShadow = 8,
		CastsPolygonShadow = 16,

		IsLight = 32,
		IsLightWithShadows = 64,

		IsTrigger = 128,
		IsVisibleForTrigger = 256,
	}

	enum Geometry
	{
		BB,
		Sphere,
		Cone,
	}

	
	abstract class Elem
	{
		protected ElemTags _elemTags;
		public ElemTags ElemTags {
			get { return _elemTags; }
		}

		ulong _tag;
		public ulong Tag {
			get { return _tag; }
			set { _tag = value; }
		}

		Vector3 _position;
		public Vector3 Position {
			get { return _position; }
			set { _position = value; PositionChanged(); }
		}

		protected Geometry _geometry;
		public Geometry Geometry {
			get { return _geometry; }
		}

		public abstract float GetSizeOnAxis(int axis);
		public abstract void GetSizeOnAxis(ref AxisSelector axis, out Vector3 sizes);
		public abstract void GetConeParams(out Vector3 direction, out float radius, out float angle);

		protected virtual void PositionChanged() {}

		public PlaneIntersectionType PlaneTest(int axis, float d, World world) {
			float size = GetSizeOnAxis(axis);
			float pos = Vector3.Dot(_position, world.GetBoundPlane(axis));
			if (pos - size >= d)
				return PlaneIntersectionType.Front;
			if (pos + size <= d)
				return PlaneIntersectionType.Back;

			if (_geometry == Geometry.Cone) {
				Vector3 direction;
				float radius, coneAngle;
				GetConeParams(out direction, out radius, out coneAngle);

				float planeAngle = (float)Math.Acos(Math.Abs(pos - d) / radius);

				float planeConeAngle = (float)Math.Acos(Vector3.Dot(direction, pos < d ? world.GetBoundPlane(axis) : Vector3.Negate(world.GetBoundPlane(axis))));

				if (coneAngle + planeAngle > planeConeAngle)
					return PlaneIntersectionType.Intersecting;

				if (pos >= d) {
					return PlaneIntersectionType.Front;
				} else {
					return PlaneIntersectionType.Back;
				}
			}

			return PlaneIntersectionType.Intersecting;
		}

		//public virtual bool IsPerspectiveCollision(Rectangle other) { return false; }
		//public virtual bool IsCollision(BoundingBox other) { return false; }
		//public virtual bool IsCollision(BoundingSphere other) { return false; }

		//public virtual void UpdateCollision(Elem other, CollisionUpdateType type) {return;}
	}


	//class Mover
	//{
	//    TimeSpan _startTime;
	//    Vector3 _startPosition;
	//    Elem _elem;
	//    Vector3 _speed;
	//    // _customFunction
	//}
}
