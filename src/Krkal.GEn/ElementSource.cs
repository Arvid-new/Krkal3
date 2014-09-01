using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	class ElementSource
	{
		ElementGeometry _geometry;
		internal ElementGeometry Geometry {
			get { return _geometry; }
		}

		AtlasTexture _texture;
		internal AtlasTexture Texture {
			get { return _texture; }
		}


		// CONSTRUCTOR
		public ElementSource(TextureAtlas atlas, String textureName, String geometryName) {
			_texture = atlas.GetOrCreateTexture(textureName, KrkalDataType.KrkalTexture);
			_geometry = (ElementGeometry)atlas.Manager.GetOrLoadData(geometryName, KrkalDataType.ElementGeometry);
		}
	}






	class ElementGeometry : KrkalData
	{
		float[] _boundingBoxSizes; // x,y,z,u1,u2,...

		Vector3 _textureShift;
		public Vector3 TextureShift {
			get { return _textureShift; }
		}

		ElemTags _elemTags;
		internal ElemTags ElemTags {
			get { return _elemTags; }
		}

		protected override void LoadData(SlimDX.Direct3D9.Device device, object prm) {
			_boundingBoxSizes = new float[3];
			if (Name == "stena") {
				_boundingBoxSizes[0] = 20;
				_boundingBoxSizes[1] = 20;
				_boundingBoxSizes[2] = 20;
				_textureShift.X = -34;
				_textureShift.Y = -34;
				_textureShift.Z = -20;
				_elemTags = ElemTags.AcceptsLight | ElemTags.CastsBBShadow | ElemTags.Visible;
			} else if (Name == "podlaha") {
				_boundingBoxSizes[0] = 20;
				_boundingBoxSizes[1] = 20;
				_boundingBoxSizes[2] = 0;
				_textureShift.X = -20;
				_textureShift.Y = -20;
				_textureShift.Z = 0;
				_elemTags = ElemTags.AcceptsLight | ElemTags.CastsBBShadow | ElemTags.Visible;
			} else throw new InternalGEnException("not implemented");
		}

		protected override void UnLoadData() {
			_boundingBoxSizes = null;
		}




		public float GetSizeOnAxis(int axis) {
			return _boundingBoxSizes[axis];
		}

		public void GetSizeOnAxis(ref AxisSelector axis, out Vector3 sizes) {
			sizes = new Vector3(_boundingBoxSizes[axis.A1], _boundingBoxSizes[axis.A2], _boundingBoxSizes[axis.A3]);
		}

	}
}
