// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
// -- Color space options --
#define GammaCurve Rec709
#define gamma 2.2
#define QuasiLab true

// -- Option values --
#define None   1
#define Rec709 2
#define sRGB   3
#define Power  4
#define Fast   5
#define true   6
#define false  7

// -- Gamma processing --
#define A (0.272433)

#if GammaCurve == Rec709
float3 Gamma(float3 x)   { return x < 0.018						 ? x * 4.506198600878514 : 1.099 * pow(x, 0.45) - 0.099; }
float3 GammaInv(float3 x){ return x < 0.018 * 4.506198600878514  ? x / 4.506198600878514 : pow((x + 0.099) / 1.099, 1 / 0.45); }
#elif GammaCurve == sRGB
float3 Gamma(float3 x)   { return x < 0.00303993442528169  ? x * 12.9232102 : 1.055*pow(x, 1 / 2.4) - 0.055; }
float3 GammaInv(float3 x){ return x < 0.039285711572131475 ? x / 12.9232102 : pow((x + 0.055) / 1.055, 2.4); }
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
#define D65 float3(0.9505, 1.0, 1.0890)
#define RGBtoXYZ float3x3(float3(0.4124,0.3576,0.1805)/D65.x,float3(0.2126,0.7152,0.0722)/D65.y,float3(0.0193,0.1192,0.9505)/D65.z)
#define XYZtoRGB (125*float3x3(D65*float3(67119136, -31838320, -10327488), D65*float3(-20068284, 38850255, 859902), D65*float3(1153856, -4225640, 21892272))/2588973042.0)
#define YUVtoXYZ mul(RGBtoXYZ,YUVtoRGB)
#define XYZtoYUV mul(RGBtoYUV,XYZtoRGB)

float3 Labf(float3 x)   { return x < (6.0*6.0*6.0) / (29.0*29.0*29.0) ? (x * (29.0 * 29.0) / (3.0 * 6.0 * 6.0)) + (4.0 / 29.0) : pow(x, 1.0 / 3.0); }
float3 Labfinv(float3 x){ return x < (6.0 / 29.0)					  ? (x - (4.0 / 29.0)) * (3.0 * 6.0 * 6.0) / (29.0 * 29.0) : x*x*x; }

float3 DLabf(float3 x)   { return min((29.0 * 29.0) / (3.0 * 6.0 * 6.0), (1.0/3.0) / pow(x, (2.0 / 3.0))); }
float3 DLabfinv(float3 x){ return max((3.0 * 6.0 * 6.0) / (29.0 * 29.0), 3.0*x*x); }

float3 RGBtoLab(float3 rgb) {	
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = Labf(xyz);
	#if QuasiLab == true
		return 1.16*xyz - 0.16;
	#else
		return float3(1.16*xyz.y - 0.16, 5.0*(xyz.x - xyz.y), 2.0*(xyz.y - xyz.z));
	#endif
}

const static float3x3 QuasiLabTransform = {{0, 1.16, 0}, {5.0, -5.0, 0}, {0.0, -2.0, 2.0}};
const static float3x3 QuasiLabInverse = {{25.0/29.0, 1.0/5.0, 0}, {25.0/29.0, 0, 0}, {25.0/29.0, 0, 1.0/2.0}};

float QuasiLabNorm(float3 xyz) {
	xyz = float3(1.16*xyz.y, 5.0*(xyz.x - xyz.y), 2.0*(xyz.y - xyz.z));
	return dot(xyz,xyz);
}

float3 LabtoRGB(float3 lab) {
	#if QuasiLab == true
		float3 xyz = (lab + 0.16) / 1.16;
	#else
		float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	#endif
	return mul(XYZtoRGB, Labfinv(xyz));
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

float3x3 DinvRGBtoLab(float3 lab) {
	float3 xyz = (lab.x + 0.16) / 1.16 + float3(lab.y / 5.0, 0, -lab.z / 2.0);
	xyz = 1 / DLabfinv(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(XYZtoRGB, D);
}

float3x3 DinvLabtoRGB(float3 rgb) {
	float3 xyz = mul(RGBtoXYZ, rgb);
	xyz = 1 / DLabf(xyz);
	float3x3 D = { { xyz.x, 0, 0 }, { 0, xyz.y, 0 }, { 0, 0, xyz.z } };
	return mul(D, RGBtoXYZ);
}