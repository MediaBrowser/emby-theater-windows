// -- Color space options --
#define GammaCurve sRGB
#define gamma 2.2

// -- Misc --
sampler s0 : register(s0);
float4 p0 :  register(c0);
float2 p1 :  register(c1);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

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
#define Kb 0.114
#define Kr 0.299
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

float3x3 DRGBtoLab(float3 rgb) {
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = DLabf(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(D, RGBtoXYZ);
}

float3x3 DLabtoRGB(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	xyz = DLabfinv(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(XYZtoRGB, D);
}

// -- Main code --
float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0.rgb = Gamma(c0.rgb);

	return c0;
}
