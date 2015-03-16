/* --- Settings --- */
#define Red   8.0  //[1.0 to 15.0]
#define Green 8.0  //[1.0 to 15.0]
#define Blue  8.0  //[1.0 to 15.0]

#define ColorGamma    2.5  //[0.1 to 2.5] Adjusts the colorfulness of the effect in a manner similar to Vibrance. 1.0 is neutral.
#define DPXSaturation 3.0  //[0.0 to 8.0] Adjust saturation of the effect. 1.0 is neutral.

#define RedC   0.36  //[0.60 to 0.20]
#define GreenC 0.36  //[0.60 to 0.20]
#define BlueC  0.34  //[0.60 to 0.20]

#define Blend 0.2    //[0.00 to 1.00] How strong the effect should be.

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

/*
DPX/Cineon shader by Loadus ( http://www.loadusfx.net/virtualdub/DPX.fx )

Version 1.0
Ported to SweetFX by CeeJay.dk

Version 1.1
Improved performance

*/

static float3x3 RGB =
{
2.67147117265996,-1.26723605786241,-0.410995602172227,
-1.02510702934664,1.98409116241089,0.0439502493584124,
0.0610009456429445,-0.223670750812863,1.15902104167061
};

static float3x3 XYZ =
{
0.500303383543316,0.338097573222739,0.164589779545857,
0.257968894274758,0.676195259144706,0.0658358459823868,
0.0234517888692628,0.1126992737203,0.866839673124201
};

float4 DPXPass(float4 InputColor){

	float DPXContrast = 0.1;

	float DPXGamma = 1.0;

	float RedCurve = Red;
	float GreenCurve = Green;
	float BlueCurve = Blue;
	
	float3 RGB_Curve = float3(Red,Green,Blue);
	float3 RGB_C = float3(RedC,GreenC,BlueC);

	float3 B = InputColor.rgb;
	//float3 Bn = B; // I used InputColor.rgb instead.

	B = pow(B, 1.0/DPXGamma);

  B = B * (1.0 - DPXContrast) + (0.5 * DPXContrast);


    //B = (1.0 /(1.0 + exp(- RGB_Curve * (B - RGB_C))) - (1.0 / (1.0 + exp(RGB_Curve / 2.0))))/(1.0 - 2.0 * (1.0 / (1.0 + exp(RGB_Curve / 2.0))));
	
    float3 Btemp = (1.0 / (1.0 + exp(RGB_Curve / 2.0)));	  
	  B = ((1.0 / (1.0 + exp(-RGB_Curve * (B - RGB_C)))) / (-2.0 * Btemp + 1.0)) + (-Btemp / (-2.0 * Btemp + 1.0));


     //TODO use faster code for conversion between RGB/HSV  -  see http://www.chilliant.com/rgb2hsv.html
	   float value = max(max(B.r, B.g), B.b);
	   float3 color = B / value;
	
	   color = pow(color, 1.0/ColorGamma);
	
	   float3 c0 = color * value;

	   c0 = mul(XYZ, c0);

	   float luma = dot(c0, float3(0.30, 0.59, 0.11)); //Use BT 709 instead?

 	   //float3 chroma = c0 - luma;
	   //c0 = chroma * DPXSaturation + luma;
	   c0 = (1.0 - DPXSaturation) * luma + DPXSaturation * c0;
	   
	   c0 = mul(RGB, c0);
	
	InputColor.rgb = lerp(InputColor.rgb, c0, Blend);

	return InputColor;
}

/* --- Main --- */

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0, tex);

	c0 = DPXPass(c0);
	return c0;
}