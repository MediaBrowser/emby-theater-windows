sampler s0 : register(s0);
sampler3D s1 : register(s1);
float4  p0 : register(c0);
float2  p1 : register(c1);
float4 size0 : register(c2);
float4 size1 : register(c3);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

float4 main(float2 tex : TEXCOORD0) : COLOR
{
    float3 src = saturate(tex2D(s0, tex).rgb);
    return tex3D(s1, src);
}