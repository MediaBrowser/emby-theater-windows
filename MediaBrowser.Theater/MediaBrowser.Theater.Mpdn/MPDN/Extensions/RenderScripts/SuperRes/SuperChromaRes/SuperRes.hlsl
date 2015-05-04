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
// -- Main parameters --
#define strength (args0[0])
#define sharpness (args0[1])
#define anti_aliasing (args0[2])
#define anti_ringing (args0[3])
#define softness (args1[0])

// -- Edge detection options -- 
#define edge_adaptiveness 0.0
#define baseline 0.0
#define acuity 6.0
#define radius 1.5

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
float4 args2  : register(c5);

#define width  (p0[0])
#define height (p0[1])

#define dxdy (p1.xy)
#define ddxddy (size2.zw)
#define offset (args2.xy)

#define sqr(x) dot(x,x)
#define spread (exp(-1/(2.0*radius*radius)))
#define h 1.2

// -- Colour space Processing --
#include "../../Common/ColourProcessing.hlsl"
#define Kb args1[1] //redefinition
#define Kr args1[2] //redefinition

// -- Input processing --
//Current high res value
#define Get(x,y)  	(tex2D(s0,tex+dxdy*int2(x,y)).xyz)
//Difference between downsampled result and original
#define Diff(x,y)	(tex2D(sDiff,tex+dxdy*int2(x,y)).xyz)
//Original YUV
#define Original(x,y)	float2(tex2D(sU,pos+ddxddy*int2(x,y))[0], tex2D(sV,tex+ddxddy*int2(x,y))[0])

// -- Main Code --
float4 main(float2 tex : TEXCOORD0) : COLOR{
	float4 c0 = tex2D(s0, tex);
	float3 stab = 0;

	float3 Ix = (Get(1, 0) - Get(-1, 0)) / (2.0*h);
	float3 Iy = (Get(0, 1) - Get(0, -1)) / (2.0*h);
	float3 Ixx = (Get(1, 0) - 2 * Get(0, 0) + Get(-1, 0)) / (h*h);
	float3 Iyy = (Get(0, 1) - 2 * Get(0, 0) + Get(0, -1)) / (h*h);
	float3 Ixy = (Get(1, 1) - Get(1, -1) - Get(-1, 1) + Get(-1, -1)) / (4.0*h*h);
	//	Ixy = (Get(1,1) - Get(1,0) - Get(0,1) + 2*Get(0,0) - Get(-1,0) - Get(0,-1) + Get(-1,-1))/(2.0*h*h);
	
#ifndef SkipAntiAliasing
	// Mean curvature flow
	float3 N = rsqrt(Ix*Ix + Iy*Iy);
	Ix *= N; Iy *= N;
	stab -= anti_aliasing*(Ix*Ix*Iyy - 2*Ix*Iy*Ixy + Iy*Iy*Ixx);
#endif

#ifndef SkipSharpening
	// Inverse heat equation
	stab += sharpness*0.5*(Ixx + Iyy);
#endif

#ifndef SkipSoftening
	// Softening
	float W = 1;
	float3 soft = 0;
	float3 D[8] = {	{Get(0,0) - Get(0,1), Get(0,0) - Get(1, 0), Get(0,0) - Get(0 ,-1), Get(0,0) - Get(-1,0)},
				 	{Get(0,0) - Get(1,1), Get(0,0) - Get(1,-1), Get(0,0) - Get(-1,-1), Get(0,0) - Get(-1,1)} };
	[unroll] for( int k = 0; k < 8; k++)
	{
		float3 d = D[k];
		float x2 = sqr(acuity*d);
		float w = pow(spread, k < 4 ? 1.0 : 2.0)*exp(-x2);
		soft += w*d;
		W += w;
	}
	stab += softness * soft * pow(W / (1 + 4 * spread + 4 * spread * spread), edge_adaptiveness);
#endif

	// Calculate faithfulness force
	float3 diff = Diff(0, 0);

	// Apply forces
	c0.yz -= strength*(diff + stab).yz;

#ifndef SkipAntiRinging
	// Calculate Position
	float2 pos = tex - ddxddy*offset;

	// Find extrema
	float2 Min = min(min(Original(0, 0), Original(1, 0)),
					 min(Original(0, 1), Original(1, 1)));
	float2 Max = max(max(Original(0, 0), Original(1, 0)),
					 max(Original(0, 1), Original(1, 1)));

	// Apply anti-ringing
	float2 AR = c0.yz - clamp(c0.yz, Min, Max);
	c0.yz -= AR*smoothstep(0, (Max - Min) / anti_ringing - (Max - Min) + pow(2, -16), abs(AR));
#endif

	return c0;
}