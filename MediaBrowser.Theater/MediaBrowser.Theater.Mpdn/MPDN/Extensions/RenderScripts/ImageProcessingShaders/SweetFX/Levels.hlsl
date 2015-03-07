/* --- Settings --- */
#define Levels_black_point 16    //[0 to 255] The black point is the new black - literally. Everything darker than this will become completely black. Default is 16.0
#define Levels_white_point 235   //[0 to 255] The new white point. Everything brighter than this becomes completely white. Default is 235.0

//Colors between the two points will stretched, which increases contrast, but details above and below the points are lost (this is called clipping).


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
  /                          Levels                             /
  '------------------------------------------------------------/

by Christian Cann Schuldt Jensen ~ CeeJay.dk

Allows you to set a new black and a white level.
This increases contrast, but clips any colors outside the new range to either black or white
and so some details in the shadows or highlights can be lost.

The shader is very useful for expanding the 16-235 TV range to 0-255 PC range.
You might need it if you're playing a game meant to display on a TV with an emulator that does not do this.
But it's also a quick and easy way to uniformly increase the contrast of an image.

*/

#define black_point_float ( Levels_black_point / 255.0 )
#define white_point_float ( 255.0 / (Levels_white_point - Levels_black_point))

float4 LevelsPass( float4 colorInput )
{
  colorInput.rgb = colorInput.rgb * white_point_float - (black_point_float *  white_point_float);
  return colorInput;
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = LevelsPass(c0);
	return c0;
}