/* --- Settings --- */

#define TechniAmount 0.4         //[0.00 to 1.00]
#define TechniPower  4.0         //[0.00 to 8.00]
#define redNegativeAmount   0.88 //[0.00 to 1.00]
#define greenNegativeAmount 0.88 //[0.00 to 1.00]
#define blueNegativeAmount  0.88 //[0.00 to 1.00]

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
  /                        TECHICOLOR                           /
  '-----------------------------------------------------------*/
// Original by DKT70
// Optimized by CeeJay.dk
// - Version 1.1

#define cyanfilter float3(0.0, 1.30, 1.0)
#define magentafilter float3(1.0, 0.0, 1.05) 
#define yellowfilter float3(1.6, 1.6, 0.05)

#define redorangefilter float2(1.05, 0.620) //RG_
#define greenfilter float2(0.30, 1.0)       //RG_
#define magentafilter2 magentafilter.rb     //R_B

float4 TechnicolorPass( float4 colorInput )
{
	float3 tcol = colorInput.rgb;
	
  float2 rednegative_mul   = tcol.rg * (1.0 / (redNegativeAmount * TechniPower));
	float2 greennegative_mul = tcol.rg * (1.0 / (greenNegativeAmount * TechniPower));
	float2 bluenegative_mul  = tcol.rb * (1.0 / (blueNegativeAmount * TechniPower));
	
  float rednegative   = dot( redorangefilter, rednegative_mul );
	float greennegative = dot( greenfilter, greennegative_mul );
	float bluenegative  = dot( magentafilter2, bluenegative_mul );
	
	float3 redoutput   = rednegative.rrr + cyanfilter;
	float3 greenoutput = greennegative.rrr + magentafilter;
	float3 blueoutput  = bluenegative.rrr + yellowfilter;
	
	float3 result = redoutput * greenoutput * blueoutput;
	colorInput.rgb = lerp(tcol, result, TechniAmount);
	return colorInput;
}


/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = TechnicolorPass(c0);
	return c0;
}