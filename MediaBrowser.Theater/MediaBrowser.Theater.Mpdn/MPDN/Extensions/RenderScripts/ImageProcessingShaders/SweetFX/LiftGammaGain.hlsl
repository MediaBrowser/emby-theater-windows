/* --- Settings --- */

#define RGB_Lift  float3(1.000, 1.000, 1.000)  //[0.000 to 2.000] Adjust shadows for Red, Green and Blue.
#define RGB_Gamma float3(1.000, 1.000, 1.000)  //[0.000 to 2.000] Adjust midtones for Red, Green and Blue
#define RGB_Gain  float3(1.000, 1.000, 1.000)  //[0.000 to 2.000] Adjust highlights for Red, Green and Blue

//Note that a value of 1.000 is a neutral setting that leave the color unchanged.


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
  /                      Lift Gamma Gain                        /
  '-----------------------------------------------------------*/
/*
  by 3an and CeeJay.dk
  
  Version 1.1
*/

float4 LiftGammaGainPass( float4 colorInput )
{
	// -- Get input --
	float3 color = colorInput.rgb;
	
	// -- Lift --
	//color = color + (RGB_Lift / 2.0 - 0.5) * (1.0 - color); 
	color = color * (1.5-0.5 * RGB_Lift) + 0.5 * RGB_Lift - 0.5;
	color = saturate(color); //isn't strictly necessary, but doesn't cost performance.
	
	// -- Gain --
	color *= RGB_Gain; 
	
	// -- Gamma --
	colorInput.rgb = pow(color, 1.0 / RGB_Gamma); //Gamma
	
	// -- Return output --
	//return (colorInput);
	return saturate(colorInput);
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = LiftGammaGainPass(c0);
	return c0;
}