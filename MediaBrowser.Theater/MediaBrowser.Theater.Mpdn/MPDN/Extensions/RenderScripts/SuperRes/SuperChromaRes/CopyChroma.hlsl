// -- Misc --
sampler s0 : register(s0);
sampler sU : register(s1);
sampler sV : register(s2);
float4 p0 :  register(c0);
float2 p1 :  register(c1);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

// -- Main code --
float4 main(float2 tex : TEXCOORD0) : COLOR{
	float y = tex2D(s0, tex)[0];
	float u = tex2D(sU, tex)[0];
	float v = tex2D(sV, tex)[0];

	return float4(y, u, v, 1);
}