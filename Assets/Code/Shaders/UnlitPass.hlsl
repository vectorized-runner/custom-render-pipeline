#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#endif

#include "Common.hlsl"

struct Attributes
{
    float3 objectSpacePosition : POSITION;
};

// Makes it SRP Batcher Compatible.
CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
CBUFFER_END

float4 UnlitPassVertex(Attributes input) : SV_POSITION
{
    const float3 positionWS = TransformObjectToWorld(input.objectSpacePosition.xyz);
    return TransformWorldToHClip(positionWS);
}

float4 UnlitPassFragment() : SV_TARGET
{
    return _BaseColor;
}
