﻿/******************************************************************************/
/*
  Project - Unity CJ Lib
            https://github.com/TheAllenChou/unity-cj-lib
  
  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou
*/
/******************************************************************************/

#ifndef CJ_LIB_QUATERNION
#define CJ_LIB_QUATERNION

#include "Math.cginc"
#include "Vector.cginc"

inline float4 quat_identity()
{
  return float4(0.0f, 0.0f, 0.0f, 1.0f);
}

inline float4 quat_conj(float4 q)
{
  return float4(-q.xyz, q.w);
}

// q must be unit quaternion
inline float4 quat_pow(float4 q, float p)
{
  float r = length(q.xyz);
  if (r < kEpsilon)
    return quat_identity();

  float t = p * atan2(q.w, r);

  return float4(sin(t) * q.xyz / r, cos(t));
}

inline float3 quat_rot(float4 q, float3 v)
{
  return
    dot(q.xyz, v) * q.xyz
    + q.w * q.w * v
    + 2.0 * q.w * cross(q.xyz, v)
    - cross(cross(q.xyz, v), q.xyz);
}

inline float4 quat_axis_angle(float3 v, float a)
{
  float h = 0.5 * a;
  return float4(sin(h) * normalize(v), cos(h));
}

inline float4 quat_from_to(float3 from, float3 to)
{
  float3 c = cross(from, to);
  float cc = dot(c, c);

  if (cc < kEpsilon)
    return quat_identity();

  float3 axis = c / sqrt(cc);
  float angle = acos(clamp(dot(from, to), -1.0f, 1.0f));
  return quat_axis_angle(axis, angle);
}

inline float3 quat_get_axis(float4 q)
{
  float d = dot(q.xyz, q.xyz);
  return 
    d > kEpsilon 
    ? q.xyz / sqrt(d)
    : float3(0.0f, 0.0f, 1.0f);
}

inline float3 quat_get_angle(float4 q)
{
  return 2.0f * acos(clamp(q.w, -1.0f, 1.0f));
}

inline float4 quat_concat(float4 q1, float4 q2)
{
  return 
    float4
    (
      q1.w * q2.xyz + q2.w * q1.xyz + cross(q1.xyz, q2.xyz), 
      q1.w * q2.w - dot(q1.xyz, q2.xyz)
    );
}

inline float4 quat_mat(float3x3 m)
{
  float tr = m._m00 + m._m11 + m._m22;
  if (tr > 0.0f) {
    float s = sqrt(tr + 1.0f) * 2.0f;
    float sInv = 1.0f / s;
    return
      float4
      (
      (m._m21 - m._m12) * sInv,
        (m._m02 - m._m20) * sInv,
        (m._m10 - m._m01) * sInv,
        0.25 * s
        );
  }
  else if ((m._m00 > m._m11) && (m._m00 > m._m22))
  {
    float s = sqrt(1.0f + m._m00 - m._m11 - m._m22) * 2.0f;
    float sInv = 1.0f / s;
    return
      float4
      (
        0.25f * s,
        (m._m01 + m._m10) * sInv,
        (m._m02 + m._m20) * sInv,
        (m._m21 - m._m12) * sInv
        );
  }
  else if (m._m11 > m._m22)
  {
    float s = sqrt(1.0f + m._m11 - m._m00 - m._m22) * 2.0f;
    float sInv = 1.0f / s;
    return
      float4
      (
      (m._m01 + m._m10) * sInv,
        0.25 * s,
        (m._m12 + m._m21) * sInv,
        (m._m02 - m._m20) * sInv
        );
  }
  else {
    float s = sqrt(1.0f + m._m22 - m._m00 - m._m11) * 2.0f;
    float sInv = 1.0f / s;
    return
      float4
      (
      (m._m02 + m._m20) * sInv,
        (m._m12 + m._m21) * sInv,
        0.25 * s,
        (m._m10 - m._m01) * sInv
        );
  }
}

inline float4 quat_look_at(float3 dir, float3 up)
{
  return quat_mat(mat_look_at(dir, up));
}

inline float4 slerp(float4 a, float4 b, float t)
{
  float d = dot(normalize(a), normalize(b));
  if (d > kEpsilonComp)
  {
    return lerp(a, b, t);
  }

  float r = acos(clamp(d, -1.0f, 1.0f));
  return (sin((1.0 - t) * r) * a + sin(t * r) * b) / sin(r);
}

inline float4 nlerp(float4 a, float b, float t)
{
  return normalize(lerp(a, b, t));
}


/*
nabe add.
*/

float4x4 quaternion_to_matrix(float4 quat)
{
    float4x4 m = float4x4(float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0), float4(0, 0, 0, 0));

    float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, xy = x * y2, xz = x * z2;
    float yy = y * y2, yz = y * z2, zz = z * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    m[0][0] = 1.0 - (yy + zz);
    m[0][1] = xy - wz;
    m[0][2] = xz + wy;

    m[1][0] = xy + wz;
    m[1][1] = 1.0 - (xx + zz);
    m[1][2] = yz - wx;

    m[2][0] = xz - wy;
    m[2][1] = yz + wx;
    m[2][2] = 1.0 - (xx + yy);

    m[3][3] = 1.0;

    return m;
}

float3x3 quaternion_to_matrix3x3(float4 quat)
{
    float3x3 m = float3x3(float3(0, 0, 0), float3(0, 0, 0), float3(0, 0, 0));

    float x = quat.x, y = quat.y, z = quat.z, w = quat.w;
    float x2 = x + x, y2 = y + y, z2 = z + z;
    float xx = x * x2, xy = x * y2, xz = x * z2;
    float yy = y * y2, yz = y * z2, zz = z * z2;
    float wx = w * x2, wy = w * y2, wz = w * z2;

    m[0][0] = 1.0 - (yy + zz);
    m[0][1] = xy - wz;
    m[0][2] = xz + wy;

    m[1][0] = xy + wz;
    m[1][1] = 1.0 - (xx + zz);
    m[1][2] = yz - wx;

    m[2][0] = xz - wy;
    m[2][1] = yz + wx;
    m[2][2] = 1.0 - (xx + yy);

    return m;
}

float4 q_look_at(float3 forward, float3 up)
{
    float3 right = normalize(cross(forward, up));
    up = normalize(cross(forward, right));

    float m00 = right.x;
    float m01 = right.y;
    float m02 = right.z;
    float m10 = up.x;
    float m11 = up.y;
    float m12 = up.z;
    float m20 = forward.x;
    float m21 = forward.y;
    float m22 = forward.z;

    float num8 = (m00 + m11) + m22;
    float4 q = quat_identity();
    if (num8 > 0.0)
    {
        float num = sqrt(num8 + 1.0);
        q.w = num * 0.5;
        num = 0.5 / num;
        q.x = (m12 - m21) * num;
        q.y = (m20 - m02) * num;
        q.z = (m01 - m10) * num;
        return q;
    }

    if ((m00 >= m11) && (m00 >= m22))
    {
        float num7 = sqrt(((1.0 + m00) - m11) - m22);
        float num4 = 0.5 / num7;
        q.x = 0.5 * num7;
        q.y = (m01 + m10) * num4;
        q.z = (m02 + m20) * num4;
        q.w = (m12 - m21) * num4;
        return q;
    }

    if (m11 > m22)
    {
        float num6 = sqrt(((1.0 + m11) - m00) - m22);
        float num3 = 0.5 / num6;
        q.x = (m10 + m01) * num3;
        q.y = 0.5 * num6;
        q.z = (m21 + m12) * num3;
        q.w = (m20 - m02) * num3;
        return q;
    }

    float num5 = sqrt(((1.0 + m22) - m00) - m11);
    float num2 = 0.5 / num5;
    q.x = (m20 + m02) * num2;
    q.y = (m21 + m12) * num2;
    q.z = 0.5 * num5;
    q.w = (m01 - m10) * num2;
    return q;
}

#endif
