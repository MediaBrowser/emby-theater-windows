/* --- Settings --- */

#define VignetteType       1  //[1|2|3] 1 = Original, 2 = New, 3 = TV style
#define VignetteRatio   1.00  //[0.15 to 6.00]  Sets a width to height ratio. 1.00 (1/1) is perfectly round, while 1.60 (16/10) is 60 % wider than it's high.
#define VignetteRadius  1.00  //[-1.00 to 3.00] lower values = stronger radial effect from center
#define VignetteAmount -1.00  //[-2.00 to 1.00] Strength of black. -2.00 = Max Black, 1.00 = Max White.
#define VignetteSlope      8  //[2 to 16] How far away from the center the change should start to really grow strong (odd numbers cause a larger fps drop than even numbers)
#define VignetteCenter float2(0.500, 0.500)  //[0.000 to 1.000, 0.000 to 1.000] Center of effect for VignetteType 1. 2 and 3 do not obey this setting.


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
  /                          Vignette                           /
  '-----------------------------------------------------------*/
/*
  Version 1.3

  Darkens the edges of the image to make it look more like it was shot with a camera lens.
  May cause banding artifacts.
*/

//Make sure the VignetteRatio exits to avoid breaking if the user uses a Settings for a previous version that didn't include this
#ifndef VignetteRatio
  #define VignetteRatio 1.0
#endif

#ifndef VignetteType
  #define VignetteType 1
#endif


//Logical XOR - not used right now but it might be useful at a later time
float XOR( float xor_A, float xor_B )
{
  return saturate( dot(float4(-xor_A ,-xor_A ,xor_A , xor_B) , float4(xor_B, xor_B ,1.0 ,1.0 ) ) ); // -2 * A * B + A + B
}

float4 VignettePass( float4 colorInput, float2 tex )
{

	#if VignetteType == 1
		//Set the center
		float2 tc = tex - VignetteCenter;

		//Adjust the ratio
		tc *= float2((pixel.y / pixel.x),VignetteRatio);

		//Calculate the distance
		tc /= VignetteRadius;
		float v = dot(tc,tc);

		//Apply the vignette
		colorInput.rgb *= (1.0 + pow(v, VignetteSlope * 0.5) * VignetteAmount); //pow - multiply
	#endif

	#if VignetteType == 2 // New round (-x*x+x) + (-y*y+y) method.
    
        tex = -tex * tex + tex;
		colorInput.rgb = saturate(( (pixel.y / pixel.x)*(pixel.y / pixel.x) * VignetteRatio * tex.x + tex.y) * 4.0) * colorInput.rgb;
  #endif

	#if VignetteType == 3 // New (-x*x+x) * (-y*y+y) TV style method.

        tex = -tex * tex + tex;
		colorInput.rgb = saturate(tex.x * tex.y * 100.0) * colorInput.rgb;
	#endif
		
	#if VignetteType == 4
		tex = abs(tex - 0.5);
		//tex = abs(0.5 - tex); //same result
		float tc = dot(float4(-tex.x ,-tex.x ,tex.x , tex.y) , float4(tex.y, tex.y ,1.0 ,1.0 ) ); //XOR

		tc = saturate(tc -0.495);
		colorInput.rgb *= (pow((1.0 - tc * 200),4)+0.25); //or maybe abs(tc*100-1) (-(tc*100)-1)
  #endif
  
	#if VignetteType == 5
		tex = abs(tex - 0.5);
		//tex = abs(0.5 - tex); //same result
		float tc = dot(float4(-tex.x ,-tex.x ,tex.x , tex.y) , float4(tex.y, tex.y ,1.0 ,1.0 ) ); //XOR

		tc = saturate(tc -0.495)-0.0002;
		colorInput.rgb *= (pow((1.0 - tc * 200),4)+0.0); //or maybe abs(tc*100-1) (-(tc*100)-1)
  #endif

	#if VignetteType == 6 //MAD version of 2
		tex = abs(tex - 0.5);
		//tex = abs(0.5 - tex); //same result
		float tc = tex.x * (-2.0 * tex.y + 1.0) + tex.y; //XOR

		tc = saturate(tc -0.495);
		colorInput.rgb *= (pow((-tc * 200 + 1.0),4)+0.25); //or maybe abs(tc*100-1) (-(tc*100)-1)
		//colorInput.rgb *= (pow(((tc*200.0)-1.0),4)); //or maybe abs(tc*100-1) (-(tc*100)-1)
  #endif

  #if VignetteType == 7 // New round (-x*x+x) * (-y*y+y) method.
    
	  //tex.y /= float2((pixel.y / pixel.x),VignetteRatio);
    float tex_xy = dot( float4(tex,tex) , float4(-tex,1.0,1.0) ); //dot is actually slower
		colorInput.rgb = saturate(tex_xy * 4.0) * colorInput.rgb;
	#endif

	return colorInput;
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = VignettePass(c0, tex);
	return c0;
}