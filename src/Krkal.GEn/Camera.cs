using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	//enum VisibilityDirection
	//{
	//    None,
	//    HigherIsInFront,
	//    LowerIsInFront,
	//}


	
	class Camera
	{
		Vector3 _position;
		public Vector3 Position {
			get { return _position; }
			set { _position = value; _viewDirty = true; }
		}

		Vector3 _perspectiveDirection;
		public Vector3 PerspectiveDirection {
			get { return _perspectiveDirection; }
			set { _perspectiveDirection = value; _projDirty = true; }
		}

		Vector3 _screenDirectionX = Vector3.UnitX;
		public Vector3 ScreenDirectionX {
			get { return _screenDirectionX; }
			set { _screenDirectionX = Vector3.Normalize(value); _viewDirty = true; }
		}

		Vector3 _screenDirectionY = Vector3.UnitY;
		public Vector3 ScreenDirectionY {
			get { return _screenDirectionY; }
			set { _screenDirectionY = Vector3.Normalize(value); _viewDirty = true; }
		}

		bool _leftHanded = true;
		public bool LeftHanded {
			get { return _leftHanded; }
			set { _leftHanded = value; _viewDirty = true; }
		}

		Vector2 _viewPortSize;
		public Vector2 ViewPortSize {
			get { return _viewPortSize; }
			set { if (_viewPortSize != value) { _viewPortSize = value; _projDirty = true; _viewDirty = true; } }
		}

		Vector2 _scale = new Vector2(1,1);
		public Vector2 Scale {
			get { return _scale; }
			set { _scale = value; _projDirty = true; }
		}

		float _farPlane = 4096*4;
		public float FarPlane {
			get { return _farPlane; }
			set { _farPlane = value; _projDirty = true; }
		}

		bool _projDirty = true;
		bool _viewDirty = true;

		Matrix _viewProj;
		public Matrix ViewProj {
			get {
				if (_projDirty || _viewDirty)
					CalculateAll();
				return _viewProj; 
			}
		}

		Matrix _view;
		public Matrix View {
			get {
				if (_viewDirty)
					CalculateViewOnly();
				return _view; 
			}
		}


		Matrix _proj;
		public Matrix Proj {
			get {
				if (_projDirty)
					CalculateAll();
				return _proj; 
			}
		}

		public float ProjZScale {
			get { return Proj.M33 * 256; }
		}

		Matrix _viewRotation;
		public Matrix ViewRotation {
			get {
				if (_viewDirty)
					CalculateViewOnly();
				return _viewRotation; 
			}
		}

		public Vector2 PixelCorrection {
			get {
				return new Vector2(-1f / _viewPortSize.X, 1f / _viewPortSize.Y);
			}
		}
		public Vector2 PixelCorrection2 {
			get {
				return new Vector2(0.5f / _viewPortSize.X, 0.5f / _viewPortSize.Y);
			}
		}

		Vector2 _projShift;
		Vector2 _viewShift;
		Vector3 _projToViewShift;
		public Vector3 ProjToViewShift {
			get {
				if (_projDirty || _viewDirty)
					CalculateAll();
				return _projToViewShift; 
			}
		}

		Vector3 _perspectiveDirInVS; // normalized
		public Vector3 PerspectiveDirInVS {
			get {
				if (_projDirty)
					CalculateAll();
				return _perspectiveDirInVS; 
			}
		}

		void CalculateAll() {
			CalculateView();
			CalculateProjection();
			_viewProj = _view * _proj;
		}


		private void CalculateViewOnly() {
			CalculateView();
			if (!_projDirty) {
				_viewProj = _view * _proj;
			}
		}

		private void CalculateProjection() {
			if (_projDirty) {
				float Xs = _scale.X * 2 / _viewPortSize.X;
				float Ys = _scale.Y * -2 / _viewPortSize.Y;
				float Zs = -1 / _farPlane;
				float InvXs = _viewPortSize.X / (_scale.X * 2);
				float InvYs = _viewPortSize.Y / (_scale.Y * -2);
				float InvZs = _farPlane / -1;
				Matrix S = Matrix.Scaling(Xs, Ys, Zs);

				Vector4 shift = Vector3.Transform(_perspectiveDirection, _viewRotation);
				_perspectiveDirInVS = new Vector3(shift.X, shift.Y, shift.Z);
				_perspectiveDirInVS.Normalize();

				_viewShift.X = -shift.X * _farPlane / shift.Z;
				_viewShift.Y = -shift.Y * _farPlane / shift.Z;
				shift = Vector4.Transform(shift, S);
				_projShift.X = -shift.X / shift.Z;
				_projShift.Y = -shift.Y / shift.Z;
				_projToViewShift.X = -_projShift.X * InvXs;
				_projToViewShift.Y = -_projShift.Y * InvYs;
				_projToViewShift.Z = InvZs;

				Matrix Sh = MatrixUtilities.CreateFromColumns(new Vector3(1, 0, _projShift.X), new Vector3(0, 1, _projShift.Y), Vector3.UnitZ);

				_proj = S * Sh;
				_projDirty  = false;
			}
		}

		private void CalculateView() {
			if (_viewDirty) {
				Matrix T = Matrix.Translation(-_position);
				Vector3 v3;
				if (_leftHanded) {
					Vector3.Cross(ref _screenDirectionX, ref _screenDirectionY, out v3);
				} else {
					Vector3.Cross(ref _screenDirectionY, ref _screenDirectionX, out v3);
				}
				_viewRotation = MatrixUtilities.CreateFromColumns(_screenDirectionX, _screenDirectionY, v3);
				_view = T * _viewRotation;
				if ((((int)_viewPortSize.X) & 1) == 0)
					_view.M41 = _view.M41 - 0.5f;
				if ((((int)_viewPortSize.Y) & 1) == 0)
					_view.M42 = _view.M42 - 0.5f;

				_viewDirty = false;
			}
		}

	}
}
