using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace Krkal.GEn
{
	class GlobalLight
	{
		Vector3 _direction;
		public Vector3 Direction {
			get { return _direction; }
			set { Vector3.Normalize(ref value, out _direction); }
		}

		Vector3 _color;
		public Vector3 Color {
			get { return _color; }
			set { _color = value; }
		}

		public Vector3 Ambient {
			get {
				if (_useSpecificAmbient) {
					return _specificAmbient;
				} else {
					return _ambientLightIntensity * _color;
				}
			}
		}

		float _ambientLightIntensity;
		public float AmbientLightIntensity {
			get { return _ambientLightIntensity; }
			set { _ambientLightIntensity = value; }
		}

		bool _useSpecificAmbient;
		Vector3 _specificAmbient;

		public void SetDefaultAmbient() {
			_useSpecificAmbient = false;
		}
		public void SetSpecificAmbient(Vector3 ambient) {
			_specificAmbient = ambient;
			_useSpecificAmbient = true;
		}

		Camera _camera;

		// CONTRUCTOR
		public GlobalLight(Vector3 direction, Vector3 color, float ambientLightIntensity, Camera camera) {
			Direction = direction;
			_color = color;
			_ambientLightIntensity = ambientLightIntensity;
			_camera = camera;
		}

		public void GetLightDirections(out Vector3 toLight, out Vector3 halfToCameraToLight) {
			Vector4 temp = Vector3.Transform(-_direction, _camera.ViewRotation);
			toLight = new Vector3(temp.X, temp.Y, temp.Z);
			halfToCameraToLight = toLight - _camera.PerspectiveDirInVS;
			halfToCameraToLight.Normalize();
		}
	}
}
