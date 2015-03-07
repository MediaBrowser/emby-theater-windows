/* --- Settings --- */

#define Gamma       1.000  //[0.000 to 2.000] Adjust midtones. 1.000 is neutral. This setting does exactly the same as the one in Lift Gamma Gain, only with less control.

#define Exposure    0.000  //[-1.000 to 1.000] Adjust exposure

#define Saturation  0.000  //[-1.000 to 1.000] Adjust saturation

#define Bleach      0.000  //[0.000 to 1.000] Brightens the shadows and fades the colors

#define Defog       0.000  //[0.000 to 1.000] How much of the color tint to remove
#define FogColor float3(0.00, 0.00, 2.55) //[0.00 to 2.55, 0.00 to 2.55, 0.00 to 2.55] What color to remove - default is blue

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
						TONEMAP
------------------------------------------------------------------------------*/
// Version 1.1

float4 TonemapPass( float4 colorInput )
{
	float3 color = colorInput.rgb;

	color = saturate(color - Defog * FogColor); // Defog
	
	color *= pow(2.0f, Exposure); // Exposure
	
	color = pow(color, Gamma);    // Gamma -- roll into the first gamma correction in main.h ?

	//#define BlueShift 0.00	//Blueshift
	//float4 d = color * float4(1.05f, 0.97f, 1.27f, color.a);
	//color = lerp(color, d, BlueShift);
	
	float3 lumCoeff = float3(0.2126, 0.7152, 0.0722);
	float lum = dot(lumCoeff, color.rgb);
	
	float3 blend = lum.rrr; //dont use float3
	
	float L = saturate( 10.0 * (lum - 0.45) );
  	
	float3 result1 = 2.0f * color.rgb * blend;
	float3 result2 = 1.0f - 2.0f * (1.0f - blend) * (1.0f - color.rgb);
	
	float3 newColor = lerp(result1, result2, L);
	float A2 = Bleach * color.rgb; //why use a float for A2 here and then multiply by color.rgb (a float3)?
	//float3 A2 = Bleach * color.rgb; //
	float3 mixRGB = A2 * newColor;
	
	color.rgb += ((1.0f - A2) * mixRGB);
	
	//float3 middlegray = float(color.r + color.g + color.b) / 3;
	float3 middlegray = dot(color,(1.0/3.0)); //1fps slower than the original on nvidia, 2 fps faster on AMD
	
	float3 diffcolor = color - middlegray; //float 3 here
	colorInput.rgb = (color + diffcolor * Saturation)/(1+(diffcolor*Saturation)); //saturation
	
	return colorInput;
}


/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = TonemapPass(c0);
	return c0;
}