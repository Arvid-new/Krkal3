using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

namespace Krkal.GEn
{
	class MeshElement : Elem
	{

		MeshSource _meshSource;
		internal MeshSource MeshSource {
			get { return _meshSource; }
		}

		ManagedTexture _texture;
		public Texture Texture {
			get { return _texture.Texture; }
		}
		
		Matrix _transform = Matrix.Identity;
		public Matrix Transform {
			get { return _transform; }
			set { 
				_transform = value; 
				PositionChanged();
				Matrix temp;
				Matrix.Invert(ref _transform, out temp);
				Matrix.Transpose(ref temp, out _normalTransform);
			}
		}

		float _material;
		public float Material {
			get { return _material; }
		}

		Matrix _normalTransform = Matrix.Identity;
		public Matrix NormalTransform {
			get { return _normalTransform; }
		}

		Matrix _objectMatrix;
		public Matrix ObjectMatrix {
			get { return _objectMatrix; }
		}

		// CONSTRUCTOR
		public MeshElement(Vector3 position, MeshSource meshSource, ManagedTexture texture) {
			_meshSource = meshSource;
			Position = position;
			_geometry = meshSource.Geometry;
			_elemTags = meshSource.ElemTags;
			_texture = texture;
		}
	
		
		public override float GetSizeOnAxis(int axis) {
			return 1; // TODO
		}

		public override void GetSizeOnAxis(ref AxisSelector axis, out Vector3 sizes) {
			sizes = new Vector3(1, 1, 1); // TODO
		}

		public override void GetConeParams(out SlimDX.Vector3 direction, out float radius, out float angle) {
			throw new NotImplementedException();
		}

		protected override void PositionChanged() {
			base.PositionChanged();
			_objectMatrix = _transform * Matrix.Translation(Position);
		}


		public void SetMaterial(String materialInfo, Ramp ramp) {
			_material = ramp.Request(4, materialInfo) / 255;
		}
	}




	class MeshSource : KrkalData
	{
		Geometry _geometry;
		internal Geometry Geometry {
			get { return _geometry; }
		}

		ElemTags _elemTags;
		internal ElemTags ElemTags {
			get { return _elemTags; }
		}

		Mesh _mesh;
		public Mesh Mesh {
			get { return _mesh; }
		}

		ExtendedMaterial[] _materials;
		public IList<ExtendedMaterial> Materials {
			get { return _materials; }
		}

		protected override void LoadData(Device device, object prm) {
			_mesh = Mesh.FromFile(device, Name, MeshFlags.Managed);
			//_mesh.ComputeNormals();
			_materials = _mesh.GetMaterials();
			_geometry = Geometry.Sphere;
			_elemTags = ElemTags.AcceptsLight | ElemTags.Visible;
		}

		protected override void UnLoadData() {
			_mesh.Dispose();
			_mesh = null;
		}
	}



	class ManagedTexture : KrkalData
	{
		Texture _texture;
		public Texture Texture {
			get { return _texture; }
		}

		protected override void LoadData(Device device, object prm) {
			_texture = Texture.FromFile(device, Name, Usage.None, Pool.Managed);
			_texture.GenerateMipSublevels();
		}

		protected override void UnLoadData() {
			_texture.Dispose();
			_texture = null;
		}
	}
}
