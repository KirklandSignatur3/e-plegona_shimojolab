
#ifndef SIMPLEX_NOISE_GRAD_4D
#define SIMPLEX_NOISE_GRAD_4D

#include "Assets/CjLib/Shader/Noise/NoiseCommon.cginc"

#define F4 0.309016994374947451

float mod289(float x) 
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float permute(float x) 
{
    return mod289(((x*34.0)+1.0)*x);
}

float taylorInvSqrt(float r) 
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

bool4 lessThan(float4 x, float4 y)
{
	return bool4(x.x < y.x, x.y < y.y, x.z < y.z, x.w < y.w);
}

float4 grad4(float j, float4 ip) 
{
    const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
    float4 p, s;

    p.xyz = floor( frac (float3(j,j,j) * ip.xyz) * 7.0) * ip.z - 1.0;
    p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
    s = float4(lessThan(p, float4(0.0, 0.0, 0.0, 0.0)));
    p.xyz = p.xyz + (s.xyz*2.0 - 1.0) * s.www;

    return p;
}

float4 snoise_grad(float4 v) 
{
    const float4  C = float4( 0.138196601125011,0.276393202250021,0.414589803375032,-0.447213595499958);
	float4 i  = floor(v + dot(v, float4(F4, F4, F4, F4)) );
    float4 x0 = v - i + dot(i, C.xxxx);

	float4 i0;
    float3 isX = step( x0.yzw, x0.xxx );
    float3 isYZ = step( x0.zww, x0.yyz );
    i0.x = isX.x + isX.y + isX.z;
    i0.yzw = 1.0 - isX;
    i0.y += isYZ.x + isYZ.y;
    i0.zw += 1.0 - isYZ.xy;
    i0.z += isYZ.z;
    i0.w += 1.0 - isYZ.z;

	float4 i3 = clamp( i0, 0.0, 1.0 );
    float4 i2 = clamp( i0-1.0, 0.0, 1.0 );
    float4 i1 = clamp( i0-2.0, 0.0, 1.0 );

    float4 x1 = x0 - i1 + C.xxxx;
    float4 x2 = x0 - i2 + C.yyyy;
    float4 x3 = x0 - i3 + C.zzzz;
    float4 x4 = x0 + C.wwww;

    i = mod289(i);
    float j0 = permute( permute( permute( permute(i.w) + i.z) + i.y) + i.x);
    float4 j1 = permute( permute( permute( permute (
             i.w + float4(i1.w, i2.w, i3.w, 1.0 ))
           + i.z + float4(i1.z, i2.z, i3.z, 1.0 ))
           + i.y + float4(i1.y, i2.y, i3.y, 1.0 ))
           + i.x + float4(i1.x, i2.x, i3.x, 1.0 ));

    float4 ip = float4(1.0/294.0, 1.0/49.0, 1.0/7.0, 0.0) ;

    float4 p0 = grad4(j0,   ip);
    float4 p1 = grad4(j1.x, ip);
    float4 p2 = grad4(j1.y, ip);
    float4 p3 = grad4(j1.z, ip);
    float4 p4 = grad4(j1.w, ip);

    float4 norm = taylorInvSqrt(float4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;
    p4 *= taylorInvSqrt(dot(p4,p4));

	float3 values0 = float3(dot(p0, x0), dot(p1, x1), dot(p2, x2)); //value of contributions from each corner at point
    float2 values1 = float2(dot(p3, x3), dot(p4, x4));

    float3 m0 = max(0.5 - float3(dot(x0,x0), dot(x1,x1), dot(x2,x2)), 0.0); //(0.5 - x^2) where x is the distance
    float2 m1 = max(0.5 - float2(dot(x3,x3), dot(x4,x4)), 0.0);

    float3 temp0 = -6.0 * m0 * m0 * values0;
    float2 temp1 = -6.0 * m1 * m1 * values1;

    float3 mmm0 = m0 * m0 * m0;
    float2 mmm1 = m1 * m1 * m1;
	
    float dx = temp0[0] * x0.x + temp0[1] * x1.x + temp0[2] * x2.x + temp1[0] * x3.x + temp1[1] * x4.x + mmm0[0] * p0.x + mmm0[1] * p1.x + mmm0[2] * p2.x + mmm1[0] * p3.x + mmm1[1] * p4.x;
    float dy = temp0[0] * x0.y + temp0[1] * x1.y + temp0[2] * x2.y + temp1[0] * x3.y + temp1[1] * x4.y + mmm0[0] * p0.y + mmm0[1] * p1.y + mmm0[2] * p2.y + mmm1[0] * p3.y + mmm1[1] * p4.y;
    float dz = temp0[0] * x0.z + temp0[1] * x1.z + temp0[2] * x2.z + temp1[0] * x3.z + temp1[1] * x4.z + mmm0[0] * p0.z + mmm0[1] * p1.z + mmm0[2] * p2.z + mmm1[0] * p3.z + mmm1[1] * p4.z;
    float dw = temp0[0] * x0.w + temp0[1] * x1.w + temp0[2] * x2.w + temp1[0] * x3.w + temp1[1] * x4.w + mmm0[0] * p0.w + mmm0[1] * p1.w + mmm0[2] * p2.w + mmm1[0] * p3.w + mmm1[1] * p4.w;

    return float4(dx, dy, dz, dw) * 49.0;
}

#endif