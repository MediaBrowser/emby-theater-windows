sampler s0 : register(s0);
sampler s1 : register(s1);
float4 args0 : register(c2);

#define acuity args0[0]
#define threshold args0[1]
#define margin args0[2]

#define norm(x) (rsqrt(rsqrt(dot(x*x,x*x))))

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);
	float4 avg = tex2D(s1, tex);

	// maximum at (10*a + b + sqrt(3)*sqrt(12 a^2 - 4*a*b + 11*b^2))/16  
	// a = threshold, b = margin, (maximum at 0.75 with default settings)
	c0.rgb = lerp(c0, avg, smoothstep(0, margin, threshold + margin - norm((avg - c0).rgb*acuity)));

	return c0;
}