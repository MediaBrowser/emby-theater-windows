sampler s0 : register(s0);
sampler s1 : register(s1);
float4 p0 : register(c0);
float4 p1 : register(c1);
float4 args0 : register(c2);
float4 size1 : register(c3);

#define acuity args0[0]
#define threshold args0[1]
#define margin args0[2]

#define ppx (size1[2])
#define ppy (size1[3])

// Input Processing
#define Get(x,y)  	(tex2D(s1,float2(ppx,ppy)*(pos + 0.5 + float2(x,y))).xyz)

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);
	//float4 avg = tex2D(s1, tex);

	float2 pos = tex * size1.xy - 0.5;
	float2 offset = frac(pos);
	pos -= offset;

	//Find extrema
	float3 Min = min(min(Get(0, 0), Get(1, 0)),
					 min(Get(0, 1), Get(1, 1)));
	float3 Max = max(max(Get(0, 0), Get(1, 0)),
					 max(Get(0, 1), Get(1, 1)));

	float3 avg = lerp(lerp(Get(0, 0), Get(1, 0), offset.x),
					  lerp(Get(0, 1), Get(1, 1), offset.x), offset.y);

	float3 ext = clamp(c0, Min, Max);

	c0.rgb = lerp(c0, avg, smoothstep(0, margin, threshold + margin
		- abs(avg - c0)*acuity
		- abs(ext - c0)*acuity));

	return c0;
}