
float4x4 ProjectionMatrix;
float4x4 ViewMatrix;
float4x4 ObjectMatrix;
float4x4 NormalMatrix;
float ProjZScale;
float2 AtlasTextureSize;
float3 Ambient;
float3 ToGlobalLight;
float3 HalfToCameraToLight;
float3 GlobalLightColor;
float2 PixelCorrection;
float2 PixelCorrection2;
float3 ZViewShift;
float3 LocalLightPos;
float LightRadiusFactor;
float3 LocalLightColor;
float3 ToCamera;
float2 AttenFunc;
float ObjectMaterial;

texture t_color;
texture t_colorB1;
texture t_colorB2;
texture t_normal;
texture t_coeffs;
texture t_nBuff;
texture t_zBuff;
texture t_ramp;
texture t_oColor;

sampler ts_color = sampler_state
{
    Texture = <t_color>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
    //magfilter = linear;
    //minfilter = linear;
};
sampler ts_normal = sampler_state
{
    Texture = <t_normal>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
   // magfilter = linear;
   // minfilter = linear;
};
sampler ts_coeffs = sampler_state
{
    Texture = <t_coeffs>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
   // magfilter = linear;
   // minfilter = linear;
};
sampler ts_nBuff = sampler_state
{
    Texture = <t_nBuff>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
};
sampler ts_zBuff = sampler_state
{
    Texture = <t_zBuff>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
};
sampler ts_colorB1 = sampler_state
{
    Texture = <t_colorB1>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
};
sampler ts_colorB2 = sampler_state
{
    Texture = <t_colorB2>;
    magfilter = point;
    minfilter = point;
	mipfilter = none;
};
sampler ts_ramp = sampler_state
{
	Texture = <t_ramp>;
    magfilter = linear;
    minfilter = linear;
	mipfilter = none;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler ts_oColor = sampler_state
{
	Texture = <t_oColor>;
    magfilter = linear;
    minfilter = linear;
	mipfilter = linear;
};

///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////

struct vsi_gBuffA
{
	float2 cornerPos : POSITION;
	float4 worldPos	 : TEXCOORD0;
	float2 texCoord  : TEXCOORD1;
	float2 texSize   : TEXCOORD2;
	float scale      : TEXCOORD3;
};

struct vso_gBuffA
{
	float4 position : POSITION;
	float2 textur	: TEXCOORD0;
	float2  zscale	: TEXCOORD1;
};

struct pso_gBuffA
{
	float4 normal : COLOR0;
	float4 z : COLOR1;
	float depth	: DEPTH;
};

vso_gBuffA vs_gBuffA( vsi_gBuffA input )
{
	vso_gBuffA output;
	
	float3 viewPos = mul(input.worldPos, ViewMatrix);
	viewPos.xy += input.cornerPos * input.texSize * input.scale;
	output.position = float4(mul(viewPos, (float3x3)ProjectionMatrix),1);
	output.zscale.x = output.position.z;
	
	output.textur = (input.texCoord + input.cornerPos * input.texSize) * AtlasTextureSize;
	
	output.zscale.y = input.scale * ProjZScale;
		
	return output;
}

pso_gBuffA ps_gBuffA( vso_gBuffA input )
{
    pso_gBuffA output;
    float2 za = tex2D(ts_coeffs, input.textur).ra ;
	za.y -= 0.002;
    clip (za.y);
	output.normal = tex2D(ts_normal, input.textur);
	float z = za.x * input.zscale.y + input.zscale.x;
	output.z = z;
	output.depth = z;
	return output;
}




////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////


struct vso_gBuffB
{
	float4 position : POSITION;
	float2 textur	: TEXCOORD0;
	float2 zscale	: TEXCOORD1;
	float2 buffCoord : TEXCOORD2;
};



vso_gBuffB vs_gBuffB( vsi_gBuffA input )
{
	vso_gBuffB output;
	
	float3 viewPos = mul(input.worldPos, ViewMatrix);
	viewPos.xy += input.cornerPos * input.texSize * input.scale;
	output.position = float4(mul(viewPos, (float3x3)ProjectionMatrix),1);
	output.zscale.x = output.position.z;
	
	output.textur = (input.texCoord + input.cornerPos * input.texSize) * AtlasTextureSize;
	
	output.zscale.y = input.scale * ProjZScale;
		
	output.buffCoord = output.position.xy * float2(0.5, -0.5) + 0.5 + PixelCorrection2;
	
	return output;
}

pso_gBuffA ps_gBuffB( vso_gBuffB input, float2 screen : VPOS )
{
    pso_gBuffA output;
    float2 za = tex2D(ts_coeffs, input.textur).ra;
	float prevZ = tex2D(ts_zBuff, input.buffCoord).r;
	float z = za.x * input.zscale.y + input.zscale.x;
	
	float2 cl = float2(za.y - 0.002, z - prevZ - 0.000016);
	clip(cl);
	
	output.normal = tex2D(ts_normal, input.textur);
	output.z = z;
	output.depth = z;
	
	return output;
}


////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////




struct pso_gDiffuse
{
	float4 color : COLOR0;
	float depth	: DEPTH;
};



pso_gDiffuse ps_gDiffuse( vso_gBuffA input )
{
    pso_gDiffuse output;
    output.color = tex2D(ts_color, input.textur);
	float z = tex2D(ts_coeffs, input.textur).r;
	output.depth = z * input.zscale.y + input.zscale.x;
	return output;
}







////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////


struct vsi_global {
	float2 position : POSITION;
};


struct vso_global
{
	float4 position : POSITION;
	float2 textur	: TEXCOORD;
};


vso_global vs_global( vsi_global input )
{
	vso_global output;
	
	output.position = float4(input.position * 2 - 1 + PixelCorrection, 0, 1);
	output.textur.x = input.position.x;
	output.textur.y = 1 - input.position.y;
	
	return output;
}



half3 CalcDiffuseSpecular(half normalDotLight, half normalDotHalf, half3 colorM, half4 coeffs, half3 colorL) {
	
	half diffuse = tex2Dlod(ts_ramp, half4(normalDotLight * 0.5 + 0.5, coeffs.x, 0,0)).x;
	half specular = tex2Dlod(ts_ramp, half4(normalDotHalf * 0.5 + 0.5, coeffs.z, 0,0)).y;

	half3 color = colorM * colorL;
	return color * diffuse + lerp(colorL, color, coeffs.w) * specular;
}

float4 ps_global( vso_global input ) : COLOR
{
//return 0.5;
    half4 color = tex2D(ts_colorB1, input.textur);
   
	half4 nBuff = tex2D(ts_nBuff, input.textur);
	half3 normal = half3(nBuff.xyz) * 2 - 1;
	half4 coeffs = tex2D(ts_ramp, float4(nBuff.a,0,0,0));
	half3 colorH = half3(color.xyz);
	
	half3 res = colorH * (half3)Ambient; // ambient
	res += tex2D(ts_colorB2, input.textur).rgb; // local lights
	half diffuse = dot(normal, ToGlobalLight);
	[branch]
	if (diffuse > 0) {
		res += CalcDiffuseSpecular(diffuse, dot(normal, HalfToCameraToLight), colorH, coeffs, GlobalLightColor);
	}

	return float4(lerp(res, colorH, coeffs.y), color.a); // lerp the emissive
    
}


float4 ps_setStencil( vso_global input ) : COLOR
{
    return tex2D(ts_colorB1, input.textur);
}





///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////


struct vso_sphereLight
{
	float4 position : POSITION;
	float2 textur	: TEXCOORD0;
	float4 viewPos  : TEXCOORD1;
};


vso_sphereLight vs_sphereLight( float4 pos : POSITION )
{
	vso_sphereLight output;

	float4 worldPos = mul(pos, ObjectMatrix);
	output.viewPos.xyz = mul(worldPos, ViewMatrix);
	output.position = float4(mul(output.viewPos.xyz, (float3x3)ProjectionMatrix),1);
	output.viewPos.w = output.position.z;
	
	output.textur = output.position.xy * float2(0.5, -0.5) + 0.5 + PixelCorrection2;
	
	return output;
}


float4 ps_sphereLight( vso_sphereLight input ) : COLOR
{
	half4 nBuff = tex2D(ts_nBuff, input.textur);
	half3 normal = half3(nBuff.xyz) * 2 - 1;
	float objZ = tex2D(ts_zBuff, input.textur).r;
	float3 myPos = input.viewPos.xyz + ZViewShift * (objZ - input.viewPos.w);
	half3 toLight = LocalLightPos - myPos;
	
	half atten =  2 * (half)tex2D(ts_ramp, float2(dot(toLight, toLight) * (half)LightRadiusFactor, (half)AttenFunc.x)).z;
	toLight = normalize(toLight);
	half diffuse = dot(normal, toLight);
	

	[branch]
	if (diffuse * atten > 0.001) {

	    half3 colorH = tex2Dlod(ts_colorB1, float4(input.textur,0,0)).xyz;  
		half4 coeffs = tex2Dlod(ts_ramp, float4(nBuff.w,0,0,0));
		half3 reflect = 2 * diffuse * normal - toLight;

		return float4(atten * CalcDiffuseSpecular(diffuse, dot(reflect, ToCamera), colorH, coeffs, LocalLightColor), 0);
	} else {
		return 0;
	}
}




///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////


struct vsi_meshElement {
	float4 position : POSITION;
	float2 textur : TEXCOORD;
	float3 normal : NORMAL;
};

struct vso_meshElementA {
	float4 position : POSITION;
	float2 textur	: TEXCOORD0;
	float4 normal	: TEXCOORD1;
};

struct vso_meshElementB {
	float4 position : POSITION;
	float2 textur	: TEXCOORD0;
	float4 normal	: TEXCOORD1;
	float2 buffCoord : TEXCOORD2;	
};

struct pso_meshElement {
	float4 color : COLOR0;
	float4 normal : COLOR1;
	float4 z : COLOR2;
};



vso_meshElementA vs_meshElementA( vsi_meshElement input )
{
	vso_meshElementA output;

	float4 worldPos = mul(input.position, ObjectMatrix);
	float3 viewPos = mul(worldPos, ViewMatrix);
	output.position = float4(mul(viewPos, (float3x3)ProjectionMatrix),1);
	output.normal.w = output.position.z;
	
	float3 worldNormal = mul(input.normal, NormalMatrix);
	output.normal.xyz = mul(worldNormal, (float3x3)ViewMatrix);
	
	output.textur = input.textur;
	
	return output;
}


vso_meshElementB vs_meshElementB( vsi_meshElement input )
{
	vso_meshElementB output;

	float4 worldPos = mul(input.position, ObjectMatrix);
	float3 viewPos = mul(worldPos, ViewMatrix);
	output.position = float4(mul(viewPos, (float3x3)ProjectionMatrix),1);
	output.normal.w = output.position.z;
	
	float3 worldNormal = mul(input.normal, NormalMatrix);
	output.normal.xyz = mul(worldNormal, (float3x3)ViewMatrix);
	
	output.textur = input.textur;
	
	output.buffCoord = output.position.xy * float2(0.5, -0.5) + 0.5 + PixelCorrection2;
	
	return output;
}



pso_meshElement ps_meshElementA(vso_meshElementA input) {
	pso_meshElement output;
	
	output.color = tex2D(ts_oColor, input.textur);
	half cl = output.color.a - 0.002;
	clip(cl);
	
	half3 normal = input.normal.xyz;
	output.normal.xyz = normalize(normal) * 0.5 + 0.5;
	output.normal.w = ObjectMaterial;
	output.z = input.normal.w;
	
	return output;
}


pso_meshElement ps_meshElementB(vso_meshElementB input) {
	pso_meshElement output;
	
	output.color = tex2D(ts_oColor, input.textur);
	half2 cl;
	cl.x = input.normal.w - tex2D(ts_zBuff, input.buffCoord).r;
	cl.y = output.color.a;
	cl -= float2(0.000016, 0.002);
	clip(cl);
	
	half3 normal = input.normal.xyz;
	output.normal.xyz = normalize(normal) * 0.5 + 0.5;
	output.normal.w = ObjectMaterial;
	output.z = input.normal.w;
	
	return output;
}





technique KrkalTechnique
{
	// Prepare GBuff A
	pass P0 
	{
		ZEnable = TRUE;
		ZFunc = LESS;
		ZWriteEnable = TRUE;

		StencilEnable = TRUE;
		StencilFunc = ALWAYS;
		StencilRef = 0xFF;
		StencilPass = REPLACE;
	
		VertexShader = compile vs_3_0 vs_gBuffA();
		PixelShader = compile ps_3_0 ps_gBuffA();

		AlphaBlendEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
		CullMode = NONE;
		AlphaTestEnable = FALSE;
	}

	// Prepare Color
	pass P1
	{
		ZEnable = TRUE;
		ZFunc = EQUAL;
		ZWriteEnable = FALSE;
		AlphaBlendEnable = TRUE;
		DestBlend = DESTALPHA;
		SrcBlend = INVDESTALPHA;

		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = KEEP;
		
		VertexShader = compile vs_3_0 vs_gBuffA();
		PixelShader = compile ps_3_0 ps_gDiffuse();

		AlphaTestEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
		CullMode = NONE;
	}

	// Global light A
	pass P2
	{
		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = KEEP;
	
		VertexShader = compile vs_3_0 vs_global();
		PixelShader = compile ps_3_0 ps_global();
	
		ZEnable = FALSE;
		ZWriteEnable = FALSE;
		AlphaTestEnable = FALSE;
		AlphaBlendEnable = FALSE;
		CullMode = NONE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
	}
	
	// Prepare GBuff B
	pass P3
	{
		ZEnable = TRUE;
		ZFunc = LESS;
		ZWriteEnable = TRUE;

		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = REPLACE;
		
		VertexShader = compile vs_3_0 vs_gBuffB();
		PixelShader = compile ps_3_0 ps_gBuffB();

		AlphaBlendEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
		CullMode = NONE;
		AlphaTestEnable = FALSE;
	}

	// Global light B
	pass P4
	{
		AlphaBlendEnable = TRUE;
		DestBlend = DESTALPHA;
		SrcBlend = INVDESTALPHA;
	
		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = KEEP;
		
		VertexShader = compile vs_3_0 vs_global();
		PixelShader = compile ps_3_0 ps_global();

		ZEnable = FALSE;
		ZWriteEnable = FALSE;
		AlphaTestEnable = FALSE;
		CullMode = NONE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
	}

	
	// Update stencil for next pass
	pass P5
	{
		ColorWriteEnable = 0;

		AlphaTestEnable = TRUE;
		AlphaFunc = EQUAL;
		AlphaRef = 255;
		
		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = ZERO;
	
		VertexShader = compile vs_3_0 vs_global();
		PixelShader = compile ps_3_0 ps_setStencil();

		ZEnable = FALSE;
		ZWriteEnable = FALSE;
		AlphaBlendEnable = FALSE;
		CullMode = NONE;
	}


	// Sphere lights
	pass P6
	{
		ZEnable = TRUE;
		ZFunc = GREATER;
		ZWriteEnable = FALSE;
		AlphaBlendEnable = TRUE;
		DestBlend = ONE;
		SrcBlend = ONE;
		CullMode = CW;
	
		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = KEEP;
		
		VertexShader = compile vs_3_0 vs_sphereLight();
		PixelShader = compile ps_3_0 ps_sphereLight();

		AlphaTestEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;		
	}

	
	// Mesh Element A
	pass P7 
	{
		ZEnable = TRUE;
		ZFunc = LESS;
		ZWriteEnable = TRUE;
		CullMode = CCW;

		StencilEnable = TRUE;
		StencilFunc = ALWAYS;
		StencilRef = 0xFF;
		StencilPass = REPLACE;
	
		VertexShader = compile vs_3_0 vs_meshElementA();
		PixelShader = compile ps_3_0 ps_meshElementA();

		AlphaBlendEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
		AlphaTestEnable = FALSE;
	}

	
	// Mesh Element B
	pass P8 
	{
		ZEnable = TRUE;
		ZFunc = LESS;
		ZWriteEnable = TRUE;
		CullMode = CCW;

		StencilEnable = TRUE;
		StencilFunc = EQUAL;
		StencilRef = 0xFF;
		StencilPass = REPLACE;
	
		VertexShader = compile vs_3_0 vs_meshElementB();
		PixelShader = compile ps_3_0 ps_meshElementB();

		AlphaBlendEnable = FALSE;
		ColorWriteEnable = RED | GREEN | BLUE | ALPHA;
		AlphaTestEnable = FALSE;
	}
	
}