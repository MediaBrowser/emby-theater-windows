/* --- Settings --- */
#define CartoonPower         1.5     //[0.1 to 10.0] Amount of effect you want.
#define CartoonEdgeSlope     1.5     //[0.1 to 8.0] Raise this to filter out fainter edges. You might need to increase the power to compensate. Whole numbers are faster.


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
						Cartoon
------------------------------------------------------------------------------*/

#ifndef CartoonEdgeSlope //for backwards compatiblity with settings preset from earlier versions of SweetFX
  #define CartoonEdgeSlope 1.5 
#endif

float4 CartoonPass( float4 colorInput, float2 Tex )
{
  float3 CoefLuma2 = float3(0.2126, 0.7152, 0.0722);  //Values to calculate luma with
  
  float diff1 = dot(CoefLuma2,myTex2D(s0, Tex + pixel).rgb);
  diff1 = dot(float4(CoefLuma2,-1.0),float4(myTex2D(s0, Tex - pixel).rgb , diff1));
  
  float diff2 = dot(CoefLuma2,myTex2D(s0, Tex +float2(pixel.x,-pixel.y)).rgb);
  diff2 = dot(float4(CoefLuma2,-1.0),float4(myTex2D(s0, Tex +float2(-pixel.x,pixel.y)).rgb , diff2));
    
  float edge = dot(float2(diff1,diff2),float2(diff1,diff2));
  
  colorInput.rgb =  pow(edge,CartoonEdgeSlope) * -CartoonPower + colorInput.rgb;
	
  return saturate(colorInput);
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = CartoonPass(c0, tex);
	return c0;
}