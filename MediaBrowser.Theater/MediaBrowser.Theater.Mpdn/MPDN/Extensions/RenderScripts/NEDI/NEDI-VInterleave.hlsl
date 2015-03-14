sampler s0 : register(s0);
sampler s1 : register(s1);
float4  p0 : register(c0);
float2  p1 : register(c1);
float4 size0 : register(c2);
float4 size1 : register(c3);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

#define dxdy0 (size0.zw)
#define dxdy1 (size1.zw)

float4 main(float2 tex : TEXCOORD0) : COLOR {
	int2 par = round(frac(tex*p0.xy / 2.0));

	//Fix size mismatch.
	int2 pos = floor(tex*p0.xy / float2(1.0, 2.0));

	if (par.x == par.y) {
		return tex2D(s0, (pos + 0.5)*dxdy0);
	} else {
		return tex2D(s1, (pos + 0.5)*dxdy1);
	}
}