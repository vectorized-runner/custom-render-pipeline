#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#endif

#include "Common.hlsl"

// Makes it SRP Batcher Compatible.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
CBUFFER_END

float4 UnlitPassVertex(float3 objectSpacePosition : POSITION) : SV_POSITION
{
    const float3 positionWS = TransformObjectToWorld(objectSpacePosition.xyz);
    return TransformWorldToHClip(positionWS);
}

float4 UnlitPassFragment() : SV_TARGET
{
    return _BaseColor;
}
