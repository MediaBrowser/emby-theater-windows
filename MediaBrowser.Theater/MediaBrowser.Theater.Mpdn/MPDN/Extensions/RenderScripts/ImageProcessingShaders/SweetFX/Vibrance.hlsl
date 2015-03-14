/* --- Settings --- */

#define Vibrance 0.3  //Intelligently saturates (or desaturates if you use negative values) the pixels depending on their original saturation.
#define Vibrance_RGB_balance float3(1.00, 1.00, 1.00)  //[-10.00 to 10.00,-10.00 to 10.00,-10.00 to 10.00] A per channel multiplier to the Vibrance strength so you can give more boost to certain colors over others


/* ---  Defining Constants --- */

sampler s0 : register(s0);


/* --- Vibrance --- */
/*
  by Christian Cann Schuldt Jensen ~ CeeJay.dk
  
  Vibrance intelligently boosts the saturation of pixels
  so pixels that had little color get a larger boost than pixels that had a lot.
  
  This avoids oversaturation of pixels that were already very saturated.
*/

float4 VibrancePass( float4 colorInput )
{
  #ifndef Vibrance_RGB_balance //for backwards compatibility with setting presets for older version.
    #define Vibrance_RGB_balance float3(1.00, 1.00, 1.00)
  #endif
  
  #define Vibrance_coeff float3(Vibrance_RGB_balance * Vibrance)

	float4 color = colorInput; //original input color
  float3 lumCoeff = float3(0.212656, 0.715158, 0.072186);  //Values to calculate luma with

	float luma = dot(lumCoeff, color.rgb); //calculate luma (grey)

	float max_color = max(colorInput.r, max(colorInput.g,colorInput.b)); //Find the strongest color
	float min_color = min(colorInput.r, min(colorInput.g,colorInput.b)); //Find the weakest color

  float color_saturation = max_color - min_color; //The difference between the two is the saturation

  //color.rgb = lerp(luma, color.rgb, (1.0 + (Vibrance * (1.0 - color_saturation)))); //extrapolate between luma and original by 1 + (1-saturation) - simple

  //color.rgb = lerp(luma, color.rgb, (1.0 + (Vibrance * (1.0 - (sign(Vibrance) * color_saturation))))); //extrapolate between luma and original by 1 + (1-saturation) - current
  color.rgb = lerp(luma, color.rgb, (1.0 + (Vibrance_coeff * (1.0 - (sign(Vibrance_coeff) * color_saturation))))); //extrapolate between luma and original by 1 + (1-saturation) - current

  //color.rgb = lerp(luma, color.rgb, 1.0 + (1.0-pow(color_saturation, 1.0 - (1.0-Vibrance))) ); //pow version

	return color; //return the result
	//return color_saturation.xxxx; //Visualize the saturation
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = VibrancePass(c0);

	return c0;
}