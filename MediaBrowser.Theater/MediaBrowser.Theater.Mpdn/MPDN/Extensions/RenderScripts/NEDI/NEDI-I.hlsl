// $MinimumShaderProfile: ps_3_0
sampler s0 : register(s0);
float4  p0 : register(c0);
float2  p1 : register(c1);
float4  args0 : register(c2);

#define width  (p0[0])
#define height (p0[1])

#define px (p1[0])
#define py (p1[1])

#define offset 0.5
#define Value(xy)		(tex2D(s0,tex+float2(px,py)*(xy)))
#define Get(xy) 		(Lum(Value(xy))+offset)
#define Get4(xy) 		(float2(Get(xy+dir[0])+Get(xy+dir[1]),Get(xy+dir[2])+Get(xy+dir[3])))

#define sqr(x) (dot(x,x))
#define I (float2x2(1,0,0,1))

//Luma channel
float Lum(float4 x) { return dot(args0.xyz, x.rgb); }

//Conjugate residual 
float2 solve(float2x2 A,float2 b) {
	float2 x = 1/4.0;
	float2 r = b - mul(A,x);
	float2 p = r;
	float2 Ar = mul(A,r);
	float2 Ap = Ar;
	for (int k = 0;k < 2; k++){
		float a = min(100,dot(r,Ar)/dot(Ap,Ap));
		x = x + a*p;
		float2 rk = r; float2 Ark = Ar;
		r = r - a*Ap;
		Ar = mul(A,r);
		float b = dot(r,Ar)/dot(rk,Ark);
		p = r + b*p;
		Ap = Ar + b*Ap;
	}
	return x;
}

//Cramer's method
float2 solvex(float2x2 A,float2 b) { return float2(determinant(float2x2(b,A[1])),determinant(float2x2(A[0],b)))/determinant(A); }

float4 main(float2 tex : TEXCOORD0) : COLOR {
	float4 c0 = tex2D(s0,tex);

	//Define window and directions
	float2 dir[4] = {{-1,-1},{1,1},{-1,1},{1,-1}};
	float4x2 wind[4] = {{{0,0},{1,1},{0,1},{1,0}},{{-1,0},{2,1},{0,2},{1,-1}},{{0,-1},{1,2},{-1,1},{2,0}},{{-1,-1},{2,2},{-1,2},{2,-1}}};

	//Initialization
	float2x2 R = 0;
	float2 r = 0;
	float4 d = 0;

	//Define weights
	float4 lancz = {0.328511,-0.0365013,-0.0365013,0.0040557};
	lancz /= dot(lancz,4);
	float4 w = {1,1,1,0};

	//Calculate (local) autocorrelation coefficients
	for (int k = 0; k<4; k+= 1){
		float4 y		= float4(  Get (wind[k][0]),Get (wind[k][1]),Get (wind[k][2]),Get (wind[k][3]));
		float4x2 C 	= float4x2(Get4(wind[k][0]),Get4(wind[k][1]),Get4(wind[k][2]),Get4(wind[k][3]));
		R += w[k]*mul(transpose(C),C);
		r += w[k]*mul(y,C);
		d += lancz[k]*(Value(wind[k][0])+Value(wind[k][1])+Value(wind[k][2])+Value(wind[k][3]));
	}
	
	//Normalize
	float n = 24;
	R /= n; r /= n;

	//Calculate a =  R^-1 . r
	float e = 0.005;
	float2 a = solve(R+e*e*I,r+e*e/2.0);

	//Nomalize 'a' (prevents overshoot)
	a = .25 + float2(.5,-.5)*clamp(a[0]-a[1],-1,1);

	//Calculate result
	float2x4 x = float2x4(Value(wind[0][0])+Value(wind[0][1]),Value(wind[0][2])+Value(wind[0][3]));
	float4 c = mul(float1x2(a),x);

	//Fallback to lanczos
	float t = saturate(1-500*sqr(x[0]-x[1]));
	//	t = saturate(1-100*sqrt(sqr(mul(R,float2(0.25,0.25))-r)-sqr(mul(R,a)-r)));
	c += t*(d-mul(float1x2(1,1)/4.0,x));

	return c;
}