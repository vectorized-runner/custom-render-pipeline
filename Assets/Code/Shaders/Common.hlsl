#ifndef CUSTOM_COMMON_INCLUDED
#define CUSTOM_COMMON_INCLUDED

#include "UnityInput.hlsl"

float3 TransformObjectToWorld (float3 objectSpacePosition) {
    return mul(unity_ObjectToWorld, float4(objectSpacePosition, 1.0)).xyz;
}

float4 TransformWorldToHClip (float3 worldSpacePosition) {
    return mul(unity_MatrixVP, float4(worldSpacePosition, 1.0));
}
	
#endif