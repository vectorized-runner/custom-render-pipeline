#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#endif

#include "Common.hlsl"

float4 _BaseColor;

float4 UnlitPassVertex(float3 objectSpacePosition : POSITION) : SV_POSITION
{
    const float3 positionWS = TransformObjectToWorld(objectSpacePosition.xyz);
    return TransformWorldToHClip(positionWS);
}

float4 UnlitPassFragment() : SV_TARGET
{
    return _BaseColor;
}
