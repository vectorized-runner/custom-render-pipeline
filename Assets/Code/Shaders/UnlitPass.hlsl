#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#endif

#include "Common.hlsl"

// Vertex Shader Input
struct Attributes
{
    float3 objectSpacePosition : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Vertex Shader Output, Fragment Shader Input
// Varyings: Unity Naming, The Data can vary between fragments of the same triangle.
struct Varyings
{
    float4 clipSpacePosition : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Makes it SRP Batcher Compatible.
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    const float3 worldSpacePosition = TransformObjectToWorld(input.objectSpacePosition.xyz);
    output.clipSpacePosition = TransformWorldToHClip(worldSpacePosition);
    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
}
