/* --- Settings --- */
#define Explosion_Radius    5.5     //[0.2 to 100.0] Amount of effect you want.



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
  /                         Explosion                           /
  '-----------------------------------------------------------*/
/* 

*/

float4 ExplosionPass( float4 colorInput, float2 tex )
{

  // -- pseudo random number generator --
  float2 sine_cosine;
  sincos(dot(tex, float2(12.9898,78.233)),sine_cosine.x,sine_cosine.y);
  sine_cosine = sine_cosine * 43758.5453 + tex;
  float2 noise = frac(sine_cosine);

  tex = (-Explosion_Radius * pixel) + tex; //Slightly faster this way because it can be calculated while we calculate noise.
  
  colorInput.rgb = myTex2D(s0, (2.0 * Explosion_Radius * pixel) * noise + tex).rgb;
  
 
  return colorInput;
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = ExplosionPass(c0, tex);
	return c0;
}