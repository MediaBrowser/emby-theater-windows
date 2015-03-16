/* --- Settings --- */
#define HDRPower 1.30  //[0.00 to 8.00] Strangely lowering this makes the image brighter
#define radius2  0.87  //[0.00 to 8.00] Raising this seems to make the effect stronger and also brighter

/* ---  Defining Constants --- */
#define myTex2D(s,p) tex2D(s,p)

#ifndef s0
  sampler s0 : register(s0);
  #define s1 s0
//sampler s1 : register(s1);

  float4 p0 : register(c0);
  float4 p1 : register(c1);

//  #define width (p0[0])
//  #define height (p0[1])
//  #define counter (p0[2])
//  #define clock (p0[3])
//  #define px (p1[0]) //one_over_width 
//  #define py (p1[1]) //one_over_height

  #define px (p1.x) //one_over_width 
  #define py (p1.y) //one_over_height
  
  #define screen_size float2(p0.x,p0.y)

  #define pixel float2(px,py)

//#define pxy float2(p1.xy)

//#define PI acos(-1)
#endif

/* ---  Main code --- */

/*------------------------------------------------------------------------------
						HDR
------------------------------------------------------------------------------*/

float4 HDRPass( float4 colorInput, float2 Tex )
{
	float3 c_center = myTex2D(s0, Tex).rgb; //reuse SMAA center sample or lumasharpen center sample?
	//float3 c_center = colorInput.rgb; //or just the input?
	
	//float3 bloom_sum1 = float3(0.0, 0.0, 0.0); //don't initialize to 0 - use the first tex2D to do that
	//float3 bloom_sum2 = float3(0.0, 0.0, 0.0); //don't initialize to 0 - use the first tex2D to do that
	//Tex += float2(0, 0); // +0 ? .. oh riiiight - that will surely do something useful
	
	float radius1 = 0.793;
	float3 bloom_sum1 = myTex2D(s0, Tex + float2(1.5, -1.5) * radius1).rgb;
	bloom_sum1 += myTex2D(s0, Tex + float2(-1.5, -1.5) * radius1).rgb; //rearrange sample order to minimize ALU and maximize cache usage
	bloom_sum1 += myTex2D(s0, Tex + float2(1.5, 1.5) * radius1).rgb;
	bloom_sum1 += myTex2D(s0, Tex + float2(-1.5, 1.5) * radius1).rgb;
	
	bloom_sum1 += myTex2D(s0, Tex + float2(0, -2.5) * radius1).rgb;
	bloom_sum1 += myTex2D(s0, Tex + float2(0, 2.5) * radius1).rgb;
	bloom_sum1 += myTex2D(s0, Tex + float2(-2.5, 0) * radius1).rgb;
	bloom_sum1 += myTex2D(s0, Tex + float2(2.5, 0) * radius1).rgb;
	
	bloom_sum1 *= 0.005;
	
	float3 bloom_sum2 = myTex2D(s0, Tex + float2(1.5, -1.5) * radius2).rgb;
	bloom_sum2 += myTex2D(s0, Tex + float2(-1.5, -1.5) * radius2).rgb;
	bloom_sum2 += myTex2D(s0, Tex + float2(1.5, 1.5) * radius2).rgb;
	bloom_sum2 += myTex2D(s0, Tex + float2(-1.5, 1.5) * radius2).rgb;


	bloom_sum2 += myTex2D(s0, Tex + float2(0, -2.5) * radius2).rgb;	
	bloom_sum2 += myTex2D(s0, Tex + float2(0, 2.5) * radius2).rgb;
	bloom_sum2 += myTex2D(s0, Tex + float2(-2.5, 0) * radius2).rgb;
	bloom_sum2 += myTex2D(s0, Tex + float2(2.5, 0) * radius2).rgb;

	bloom_sum2 *= 0.010;
	
	float dist = radius2 - radius1;
	
	float3 HDR = (c_center + (bloom_sum2 - bloom_sum1)) * dist;
	float3 blend = HDR + colorInput.rgb;
	colorInput.rgb = HDR + pow(blend, HDRPower); // pow - don't use fractions for HDRpower
	
	return saturate(colorInput);
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = HDRPass(c0, tex);
	return c0;
}