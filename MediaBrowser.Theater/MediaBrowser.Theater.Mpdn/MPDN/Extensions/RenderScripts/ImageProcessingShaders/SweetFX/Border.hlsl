/* --- Settings --- */

#define border_width float2(1,20)     //[0 to 2048, 0 to 2048] (X,Y)-width of the border. Measured in pixels.
#define border_color float3(0, 0, 0)  //[0 to 255, 0 to 255, 0 to 255] What color the border should be. In integer RGB colors, meaning 0,0,0 is black and 255,255,255 is full white.


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
 /                          Border                            /
'-----------------------------------------------------------*/
// Version 1.2

/*
Version 1.0 by Oomek
- Fixes light, one pixel thick border in some games when forcing MSAA like i.e. Dishonored

Version 1.1 by CeeJay.dk
- Optimized the shader. It still does the same but now it runs faster.

Version 1.2 by CeeJay.dk
- Added border_width and border_color features

Version 1.3 by CeeJay.dk
- Optimized the performance further
*/

#ifndef border_width
  #define border_width float2(1,0)
#endif
#ifndef border_color
  #define border_color float3(0, 0, 0)
#endif

float4 BorderPass( float4 colorInput, float2 tex )
{
float3 border_color_float = border_color / 255.0;

float2 border = (pixel * border_width); //Translate integer pixel width to floating point
float2 within_border = saturate((-tex * tex + tex) - (-border * border + border)); //becomes positive when inside the border and 0 when outside

colorInput.rgb = all(within_border) ?  colorInput.rgb : border_color_float ; //

return colorInput; //return the pixel

} 

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = BorderPass(c0, tex);
	return c0;
}