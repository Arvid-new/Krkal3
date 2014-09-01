
float4x4 ProjectionMatrix;
float4x4 ViewMatrix;
float ProjZScale;
float2 AtlasTextureSize;
float3 Ambient;
float3 ToGlobalLight;
float3 HalfToCameraToLight;
float3 GlobalLightColor;
float2 PixelCorection;
float3 ZViewShift;

texture t_color;
texture t_colorB1;
texture t_colorB2;
texture t_normal;
texture t_coeffs;
texture t_gbuff;

sampler ts_color = sampler_state
{
    Texture = <t_color>;
    magfilter = linear;
    minfilter = linear;
};
sampler ts_normal = sampler_state
{
    Texture = <t_normal>;
    magfilter = linear;
    minfilter = linear;
};
sampler ts_coeffs = sampler_state
{
    Texture = <t_coeffs>;
    magfilter = linear;
    minfilter = linear;
};



///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////

struct vsi_zLay
{
	float2 cornerPos 	: POSITION;
	float4 worldPos	 	: TEXCOORD0;
	float2 pixSize   	: TEXCOORD1;
	float3 ScalesAndZ	: TEXCOORD2;
	float2 texCoords1	: TEXCOORD3;
	float2 texCoords2	: TEXCOORD4;
};

struct vso_zLay
{
	float4 position		: POSITION;
	float2 texCoords1	: TEXCOORD0;
	float2 texCoords2	: TEXCOORD1;
};


vso_zLay vs_zLay( vsi_zLay input )
{
	vso_zLay output;
	
	float3 viewPos = mul(input.worldPos, ViewMatrix);
	float2 corner = input.cornerPos * input.pixSize;
	viewPos.xy += corner;
	output.position.xy = mul(viewPos, (float3x3)ProjectionMatrix);
	output.position.z = input.ScalesAndZ.z * ProjZScale;
	output.position.w = 1;

	output.texCoords1 = (input.texCoords1 + corner / input.ScalesAndZ.x) * AtlasTextureSize;
	output.texCoords2 = (input.texCoords2 + corner / input.ScalesAndZ.y) * AtlasTextureSize;

	return output;
}

float4 ps_zLay( vso_zLay input ) : COLOR
{
//	return 0;
	float x = 1 - tex2D(ts_color, input.texCoords1).a; // allways possitive or 0 when a == 1
	x  *= tex2D(ts_color, input.texCoords2).a - 1;		// allways negative or 0 when a == 1
	clip(x);
	return 0;
}






///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////

struct vsi_main
{
	float2 cornerPos 	: POSITION;
	float4 worldPos	 	: TEXCOORD0;
	float2 pixSize   	: TEXCOORD1;
	float4 ScalesAndZ	: TEXCOORD2;
	float2 texCoords1	: TEXCOORD3;
	float2 texCoords2	: TEXCOORD4;
	float4 lights		: TEXCOORD5;
	float  overFlag		: TEXCOORD6;
};

struct vso_main
{
	float4 position		: POSITION;
	float2 texCoords1	: TEXCOORD0;
	float2 texCoords2	: TEXCOORD1;
	float3 worldPos1 	: TEXCOORD2;
	float3 worldPos2 	: TEXCOORD3;
	float3 ScalesAndI	: TEXCOORD4;
	float4 lights		: TEXCOORD5;
};


vso_main vs_main( vsi_main input )
{
	vso_main output;
	
	float3 viewPos = mul(input.worldPos, ViewMatrix);
	float2 corner = input.cornerPos * input.pixSize;
	viewPos.xy += corner;
	output.position.xy = mul(viewPos, (float3x3)ProjectionMatrix);
	output.position.z = input.ScalesAndZ.z * ProjZScale;
	output.position.w = 1;

	float3 viewPosNT = mul(input.worldPos, (float3x3)ViewMatrix);
	viewPosNT.xy += corner;
	output.worldPos1 = viewPosNT;
	output.worldPos2 = viewPosNT + input.ScalesAndZ.w * ZViewShift;
	
	output.ScalesAndI.xy = input.ScalesAndZ.xy;
	output.ScalesAndI.z = input.overFlag;

	output.texCoords1 = (input.texCoords1 + corner / input.ScalesAndZ.x) * AtlasTextureSize;
	output.texCoords2 = (input.texCoords2 + corner / input.ScalesAndZ.y) * AtlasTextureSize;
	
	output.lights = input.lights;

	return output;
}


half3 CalcDiffuseSpecular(half normalDotLight, half normalDotHalf, half3 colorM, half4 coeffs, half3 colorL) {
	normalDotLight *= (1 - coeffs.x - coeffs.y);
	half specular = pow(max(normalDotHalf, 0), coeffs.z * 256) * coeffs.x;

	half3 color = colorM * colorL;
	return color * normalDotLight + lerp(colorL, color, coeffs.w) * specular;
}



half3 Shade(half4 color, float3 pos, half3 normal, float2 texCoords, float4 lights) {
	[branch]
	if (color.a > 0) {
		
		half4 coeffs = tex2Dlod(ts_coeffs, float4(texCoords,0,0));

		half3 ret = lerp(color.xyz * (half3)Ambient, color.xyz, coeffs.y); // ambient + emissive
		half diffuse = dot(normal, ToGlobalLight);
		[branch]
		if (diffuse > 0) {
			ret += CalcDiffuseSpecular(diffuse, dot(normal, HalfToCameraToLight), color, coeffs, GlobalLightColor);
		}
		
		return saturate(ret);
	} else {
		return 0;
	}
}



float4 ps_main( vso_main input ) : COLOR
{
//return 0.3;
	half4 color1 = tex2D(ts_color, input.texCoords1);
	half4 color2 = tex2D(ts_color, input.texCoords2);
	float4 t1 = tex2D(ts_normal, input.texCoords1);
	float3 pos1 = input.worldPos1 + t1.w * input.ScalesAndI.x * ZViewShift;
	half3 normal1 = t1.xyz * 2 - 1;
	t1 = tex2D(ts_normal, input.texCoords2);
	float3 pos2 = input.worldPos2 + t1.w * input.ScalesAndI.y * ZViewShift;
	half3 normal2 = t1.xyz * 2 - 1;
	
	[branch]
	if (color1.a + color2.a <= 0) {
		return float4(0,0,0,1);
	} else {
	
	half3 ret1 = Shade(color1, pos1, normal1, input.texCoords1, input.lights);
	half3 ret2 = Shade(color2, pos2, normal2, input.texCoords2, input.lights);

	half a;/*
	if (input.ScalesAndI.z < 0) {
		if (pos1.z <= pos2.z) {
			ret1 = 0;
			ret2 *= color2.a;			// keep the top
		} else {
			ret1 *= color1.a * color2.a; // exchange
			ret2 *= color2.a * (1-color1.a);
		}
		a = (1-color2.a);
	} else {
		if (pos1.z <= pos2.z) {
			ret1 *= color1.a * (1-color2.a);
			ret2 *= color2.a;
		} else {
			ret1 *= color1.a;
			ret2 *= color2.a * (1-color1.a);
		}
		a = (1-color1.a) * (1-color2.a);
	}
*/
	ret1 *= color1.a * (1-color2.a);
	ret2 *= color2.a;
	a = (1-color1.a) * (1-color2.a);
	
	return float4(ret1 + ret2, a);
	}
}





technique KrkalTechnique
{
	pass P0
	{
		ZEnable = TRUE;
		ZFunc = LESS;
		ZWriteEnable = TRUE;
		AlphaBlendEnable = FALSE;
		ColorWriteEnable = 0;
	
		VertexShader = compile vs_3_0 vs_zLay();
		PixelShader = compile ps_3_0 ps_zLay();
	}

	pass P1
	{
		ZEnable = TRUE;
		ZFunc = LESSEQUAL;
		ZWriteEnable = FALSE;
		AlphaBlendEnable = TRUE;
		ColorWriteEnable = RED|GREEN|BLUE;
		DestBlend = SRCALPHA;
		SrcBlend = ONE;
	
		VertexShader = compile vs_3_0 vs_main();
		PixelShader = compile ps_3_0 ps_main();
	}
	

}