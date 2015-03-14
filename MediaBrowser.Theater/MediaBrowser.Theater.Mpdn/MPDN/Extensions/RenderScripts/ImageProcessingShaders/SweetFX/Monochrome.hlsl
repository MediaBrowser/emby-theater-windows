/* --- Settings --- */

#define Monochrome_conversion_values float3(0.18,0.41,0.41) //[0.00 to 1.00] Percentage of RGB to include (should sum up to 1.00)

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

   /*-----------------------------------------------------------.   
  /                          Monochrome                             /
  '-----------------------------------------------------------*/
/*
  by Christian Cann Schuldt Jensen ~ CeeJay.dk
  
  Monochrome removes color and makes everything black and white.
*/

float4 MonochromePass( float4 colorInput )
{
  //calculate monochrome
  colorInput.rgb = dot(Monochrome_conversion_values, colorInput.rgb);
	
  //Return the result
  return saturate(colorInput);
}


/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = MonochromePass(c0);
	return c0;
}