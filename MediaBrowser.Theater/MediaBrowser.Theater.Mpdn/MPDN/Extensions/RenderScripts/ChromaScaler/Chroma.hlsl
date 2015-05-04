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
sampler sY : register(s0);
sampler sU : register(s1);
sampler sV : register(s2);

float4 p0 :  register(c0);
float2 p1 :  register(c1);
float4 size1 : register(c2);
float4 args0 : register(c3);

#define width  (p0[0])
#define height (p0[1])

#define ChromaSize  (size1.xy)
#define BC (args0.xy)
#define ChromaOffset (args0.zw)

#define px (size1[2])
#define py (size1[3])

#define BCWeights(B,C,x) (x > 2.0 ? 0 : x <= 1.0 ? ((2-1.5*B-C)*x + (-3+2*B+C))*x*x + (1-B/3.) : (((-B/6.-C)*x + (B+5*C))*x + (-2*B-8*C))*x+((4./3.)*B+4*C))
#define Weight(x) (BCWeights(BC[0],BC[1],abs(x)))
#define taps 2

#define UV(xy)     (float2(tex2D(sU, xy)[0], tex2D(sV, xy)[0]))
#define GetUV(x,y) (UV(pos + float2(px,py)*int2(x,y)))
#define GetY       (tex2D(sY, tex)[0])

#define EWA 1

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float2 Avg = 0;
	float W = 0;

	float2 offset = frac(tex*ChromaSize) - ChromaOffset;

	float2 pos = tex + floor(offset)*float2(px,py);
	offset -= floor(offset);

	for (int X = -taps+1; X<=taps; X++) 
	for (int Y = -taps+1; Y<=taps; Y++) {
		int2 XY = {X,Y};
		#if EWA == 0
			float2 w = Weight(XY-offset);
			Avg += GetUV(X,Y)*w.x*w.y;
			W += w.x*w.y;
		#elif EWA == 1
			float w = Weight(length(XY-offset));
			Avg += GetUV(X,Y)*w;
			W += w;
		#endif
	}

	return float4(GetY, Avg/W, 1);
}
