// -- Main parameters --
#define strength (args0[0])
#define sharpness (args0[1])
#define anti_aliasing (args0[2])
#define anti_ringing (args0[3])
#define softness (args1[0])

// -- Edge detection options -- 
#define edge_adaptiveness 1.0
#define baseline 0.0
#define acuity 6.0
#define radius 1.5

// -- Color space options --
#define GammaCurve sRGB
#define gamma 2.2

// -- Misc --
sampler s0 	  : register(s0);
sampler sDiff : register(s1);
sampler sU	  : register(s2);
sampler sV    : register(s3);

float4 p0	  : register(c0);
float2 p1	  : register(c1);
float4 size2  : register(c2);
float4 args0  : register(c3);
float4 args1  : register(c4);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

#define ppx (size2[2])
#define ppy (size2[3])

#define sqr(x) dot(x,x)
#define spread (exp(-1/(2.0*radius*radius)))
#define h 1.5

// -- Option values --
#define None  1
#define sRGB  2
#define Power 3
#define Fast  4
#define true  5
#define false 6

// -- Gamma processing --
#define A (0.272433)

#if GammaCurve == sRGB
float3 Gamma(float3 x)   { return x < (0.0392857 / 12.9232102) ? x * 12.9232102 : 1.055*pow(x, 1 / 2.4) - 0.055; }
float3 GammaInv(float3 x){ return x <  0.0392857			   ? x / 12.9232102 : pow((x + 0.055) / 1.055, 2.4); }
#elif GammaCurve == Power
float3 Gamma(float3 x)   { return pow(saturate(x), 1 / gamma); }
float3 GammaInv(float3 x){ return pow(saturate(x), gamma); }
#elif GammaCurve == Fast
float3 Gamma(float3 x)   { return saturate(x)*rsqrt(saturate(x)); }
float3 GammaInv(float3 x){ return x*x; }
#elif GammaCurve == None
float3 Gamma(float3 x)   { return x; }
float3 GammaInv(float3 x){ return x; }
#endif

// -- Colour space Processing --
#define Kb args1[1]
#define Kr args1[2]
#define RGBtoYUV float3x3(float3(Kr, 1 - Kr - Kb, Kb), float3(-Kr, Kr + Kb - 1, 1 - Kb) / (2*(1 - Kb)), float3(1 - Kr, Kr + Kb - 1, -Kb) / (2*(1 - Kr)))
#define YUVtoRGB float3x3(float3(1, 0, 2*(1 - Kr)), float3(Kb + Kr - 1, 2*(1 - Kb)*Kb, 2*Kr*(1 - Kr)) / (Kb + Kr - 1), float3(1, 2*(1 - Kb),0))
#define RGBtoXYZ float3x3(float3(0.4124,0.3576,0.1805),float3(0.2126,0.7152,0.0722),float3(0.0193,0.1192,0.9502))
#define XYZtoRGB (625.0*float3x3(float3(67097680, -31827592, -10327488), float3(-20061906, 38837883, 859902), float3(1153856, -4225640, 21892272))/12940760409.0)
#define YUVtoXYZ mul(RGBtoXYZ,YUVtoRGB)
#define XYZtoYUV mul(RGBtoYUV,XYZtoRGB)

float3 Labf(float3 x)   { return x < (6.0*6.0*6.0) / (29.0*29.0*29.0) ? (x * (29.0 * 29.0) / (3.0 * 6.0 * 6.0)) + (4.0 / 29.0) : pow(x, 1.0 / 3.0); }
float3 Labfinv(float3 x){ return x < (6.0 / 29.0)					  ? (x - (4.0 / 29.0)) * (3.0 * 6.0 * 6.0) / (29.0 * 29.0) : x*x*x; }

float3 DLabf(float3 x)   { return min((29.0 * 29.0) / (3.0 * 6.0 * 6.0), (1.0/3.0) / pow(x, (2.0 / 3.0))); }
float3 DLabfinv(float3 x){ return max((3.0 * 6.0 * 6.0) / (29.0 * 29.0), 3.0*x*x); }

float3 RGBtoLab(float3 rgb) {
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = Labf(xyz);
	return float3(1.16*xyz.y - 0.16, 5.0*(xyz.x - xyz.y), 2.0*(xyz.y - xyz.z));
}

float3 LabtoRGB(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	return saturate(mul(XYZtoRGB, Labfinv(xyz)));
}

// -- Input processing --
//Current high res value
#define Get(x,y)  	(tex2D(s0,tex+float2(px,py)*int2(x,y)).xyz)
//Difference between downsampled result and original
#define Diff(x,y)	(tex2D(sDiff,tex+float2(px,py)*int2(x,y)).xyz)
//Original YUV
#define Original(x,y)	float2(tex2D(sU,tex+float2(ppx,ppy)*int2(x,y))[0], tex2D(sV,tex+float2(ppx,ppy)*int2(x,y))[0])

// -- Main Code --
float4 main(float2 tex : TEXCOORD0) : COLOR{
	float4 c0 = tex2D(s0, tex);
	
	float3 stab = 0;
	float W = 0;
	for (int i = -1; i <= 1; i++)
	for (int j = -1; j <= 1; j++) {
		float3 d = Get(0, 0) - Get(i, j);
		float x2 = sqr(acuity*d);
		float w = pow(spread, i*i + j*j)*lerp(1 / sqr(1 + x2), rsqrt(1 + x2), baseline);
		stab += d*w;
		W += w;
	}
	[branch] if (softness != 0)
	stab = softness*(stab / W)*pow(W / (1 + 4 * spread + 4 * spread*spread), edge_adaptiveness - 1.0);
	
	float3 Ix = (Get(1, 0) - Get(-1, 0)) / (2.0*h);
	float3 Iy = (Get(0, 1) - Get(0, -1)) / (2.0*h);
	float3 Ixx = (Get(1, 0) - 2 * Get(0, 0) + Get(-1, 0)) / (h*h);
	float3 Iyy = (Get(0, 1) - 2 * Get(0, 0) + Get(0, -1)) / (h*h);
	float3 Ixy = (Get(1, 1) - Get(1, -1) - Get(-1, 1) + Get(-1, -1)) / (4.0*h*h);
	//	Ixy = (Get(1,1) - Get(1,0) - Get(0,1) + 2*Get(0,0) - Get(-1,0) - Get(0,-1) + Get(-1,-1))/(2.0*h*h);
	float2x3 I = transpose(float3x2(
		normalize(float2(Ix[0], Iy[0])),
		normalize(float2(Ix[1], Iy[1])),
		normalize(float2(Ix[2], Iy[2]))
	));
	[branch] if (anti_aliasing != 0)
	stab -= anti_aliasing*(I[0] * I[0] * Iyy - 2 * I[0] * I[1] * Ixy + I[1] * I[1] * Ixx);
	stab += sharpness*0.5*(Ixx + Iyy);

	//Calculate faithfulness force
	float3 diff = Diff(0, 0);

	//Apply forces
	c0.yz -= strength*(diff + stab).yz;

	//Find extrema
	float2 Min = min(min(Original(0, 0), Original(1, 0)),
					 min(Original(0, 1), Original(1, 1)));
	float2 Max = max(max(Original(0, 0), Original(1, 0)),
					 max(Original(0, 1), Original(1, 1)));

	//Apply anti-ringing
	float2 AR = c0.yz - clamp(c0.yz, Min, Max);
	c0.yz -= AR*smoothstep(0, (Max - Min) / anti_ringing - (Max - Min) + pow(2, -16), abs(AR));

	return c0;
}