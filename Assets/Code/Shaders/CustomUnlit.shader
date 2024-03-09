Shader "Custom RP/Unlit"
{
    Properties
    {
        [HideInInspector]
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
        [Enum(UnityEngine.Rendering.BlendMode)]
        _SrcBlend("Src Blend", Float) = 1

        [Enum(UnityEngine.Rendering.BlendMode)]
        _DstBlend("Dst Blend", Float) = 0

        [Enum(Off, 0, On, 1)] 
        _ZWrite ("Z Write", Float) = 1
    }
    SubShader
    {
        Pass
        {
            // Blend mode is used to make Transparent objects
            // Src -> What gets drawn now, Dst -> What was drawn earlier
            // Default (Opaque): Src: One, Dst: Zero (Source gets added in full, Dst is ignored)
            Blend [_SrcBlend] [_DstBlend]
            
            // Transparent Rendering doesn't write to the depth buffer, Opaque Rendering does
            ZWrite [_ZWrite]
HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"

ENDHLSL
        }
    }
}
