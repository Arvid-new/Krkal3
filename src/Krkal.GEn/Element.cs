using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	class Element : Elem
	{
		float _scale = 1;
		public float Scale {
			get { return _scale; }
			set { _scale = value; }
		}

		ElementSource _elSource;
		internal ElementGeometry ElGeometry {
			get { return _elSource.Geometry; }
		}
		internal AtlasTexture Texture {
			get { return _elSource.Texture; }
		}

		public Vector3 TexturePosition {
			get { return Position  + _scale * _elSource.Geometry.TextureShift; }
		}


		// CONSTRUCTOR
		public Element(Vector3 position, ElementSource elSource) {
			_elSource = elSource;
			Position = position;
			_geometry = Geometry.BB;
			_elemTags = _elSource.Geometry.ElemTags;
		}

		public override float GetSizeOnAxis(int axis) {
			return _elSource.Geometry.GetSizeOnAxis(axis) * _scale;
		}

		public override void GetSizeOnAxis(ref AxisSelector axis, out Vector3 sizes) {
			_elSource.Geometry.GetSizeOnAxis(ref axis, out sizes);
			sizes *= _scale;
		}

		public override void GetConeParams(out Vector3 direction, out float radius, out float angle) {
			throw new NotImplementedException();
		}
	}
}
