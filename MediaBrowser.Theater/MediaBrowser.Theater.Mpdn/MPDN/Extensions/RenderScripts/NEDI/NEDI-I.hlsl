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
#include "NEDI-Common.hlsl"

float4 main(float2 tex : TEXCOORD0) : COLOR {
	//Define window and directions
	float2 dir[4] = {{-1,-1},{1,1},{-1,1},{1,-1}};
	float4x2 wind[4] = {{{0,0},{1,1},{0,1},{1,0}},{{-1,0},{2,1},{0,2},{1,-1}},{{0,-1},{1,2},{-1,1},{2,0}},{{-1,-1},{2,2},{-1,2},{2,-1}}};

	return NediProcess(tex, dir, wind);
}
