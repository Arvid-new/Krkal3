using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace Krkal.GEn
{
	enum MainEffectParams
	{
		ProjectionMatrix,
		ViewMatrix,
		ProjZScale,
		ZViewShift,
		PixelCorrection,
		PixelCorrection2,
		AtlasTextureSize,
		Ambient,
		GlobalLightColor,
		ToGlobalLight,
		HalfToCameraToLight,
		ToCamera,
		t_color,
		t_normal,
		t_coeffs,
		t_colorB1,
		t_nBuff,
		t_colorB2,
		t_ramp,
		t_zBuff,
		ObjectMatrix,
		LocalLightPos,
		LightRadiusFactor,
		LocalLightColor,
		AttenFunc,
		NormalMatrix,
		ObjectMaterial,
		t_oColor,
	}

	class EffectParametersMap
	{
		EffectHandle[] _handles;

		//CONSTRUCTOR
		public EffectParametersMap(Type enumType, Effect effect) {
			String[] names = Enum.GetNames(enumType);
			_handles = new EffectHandle[names.Length];
			for (int f=0; f<names.Length; f++) {
				_handles[f] = effect.GetParameter(null, names[f]);
			}
		}

		public EffectHandle this[object pos] {
			get {
				return _handles[(int)pos];
			}			
		}

	}
}
