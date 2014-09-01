using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	abstract class Light : Elem
	{
		float _radius;
		public float Radius {
			get { return _radius; }
			set { _radius = value; PositionChanged(); }
		}
		Vector3 _color;
		public Vector3 Color {
			get { return _color; }
			set { _color = value; }
		}
		protected Matrix _objectMatrix;
		public Matrix ObjectMatrix {
			get { return _objectMatrix; }
		}
		Vector2 _attenFunc;
		public Vector2 AttenFunc {
			get { return _attenFunc; }
		}


		// CONSTRUCTOR
		protected Light(Vector3 color, float radius) {
			_color = color;
			_radius = radius;
			_elemTags = ElemTags.IsLight;
		}


		public float LightRadiusFactor {
			get { return 1 / (_radius * _radius); }
		}

		public void SetAttenFunc(String func1, String func2, Ramp ramp) {
			if (func1 != null)
				_attenFunc.X = (float)ramp.Request(0, func1) / 255f;
			if (func2 != null)
				_attenFunc.Y = (float)ramp.Request(0, func2) / 255f;
		}
	}




	class SphereLight : Light
	{
		// CONSTRUCTOR
		public SphereLight(Vector3 position, Vector3 color, float radius) : base(color, radius) {
			_geometry = Geometry.Sphere;
			_objectMatrix = Matrix.Identity;
			Position = position;
		}



		public override float GetSizeOnAxis(int axis) {
			return Radius;
		}

		public override void GetSizeOnAxis(ref AxisSelector axis, out Vector3 sizes) {
			sizes = new Vector3(Radius);
		}

		public override void GetConeParams(out Vector3 direction, out float radius, out float angle) {
			throw new NotImplementedException();
		}

		protected override void PositionChanged() {
			base.PositionChanged();
			_objectMatrix.M11 = _objectMatrix.M22 = _objectMatrix.M33 = Radius;
			Vector3 pos = Position;
			_objectMatrix.M41 = pos.X;
			_objectMatrix.M42 = pos.Y;
			_objectMatrix.M43 = pos.Z;
		}
	}
}
