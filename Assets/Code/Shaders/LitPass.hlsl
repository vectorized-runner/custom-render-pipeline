#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED
#endif

#include "Common.hlsl"

struct VertexInput
{
    float3 objectSpacePosition : POSITION;
    float2 baseUV : TEXCOORD0;
    float3 objectSpaceNormal : NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Varyings: Unity Naming, The Data can vary between fragments of the same triangle.
struct FragmentInput
{
    float4 clipSpacePosition : SV_POSITION;
    float2 baseUV : VAR_BASE_UV;
    float3 worldSpaceNormal : VAR_NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Surface
{
    float3 normal;
    float3 color;
    float alpha;
};

struct Light
{
    float3 color;
    float3 direction;
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

// Makes it SRP Batcher Compatible.
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    // Unity prepares this field for us, Scale and Translation for the Texture
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, Light light)
{
    return IncomingLight(surface, light) * surface.color;
}

Light GetDirectionalLight()
{
    Light light;
    light.color = 1.0;
    light.direction = float3(0.0, 1.0, 0.0);
    return light;
}

float3 GetLighting(Surface surface)
{
    return GetLighting(surface, GetDirectionalLight());
}

FragmentInput LitPassVertex(VertexInput input)
{
    FragmentInput output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    const float3 worldSpacePosition = TransformObjectToWorld(input.objectSpacePosition.xyz);
    output.clipSpacePosition = TransformWorldToHClip(worldSpacePosition);

    const float3 worldSpaceNormal = TransformObjectToWorldNormal(input.objectSpaceNormal);

    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    output.worldSpaceNormal = worldSpaceNormal;

    return output;
}

float4 LitPassFragment(FragmentInput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);

    const float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    const float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base = baseMap * baseColor;

    float3 myNormal = normalize(input.worldSpaceNormal);

    #if defined(_CLIPPING)
    // If you pass x <= 0 to clip, it will discard this fragment
    const float cutoff = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff);
    clip(base.a - cutoff);
    #endif

    base.rgb = input.worldSpaceNormal;

    return base;
}
