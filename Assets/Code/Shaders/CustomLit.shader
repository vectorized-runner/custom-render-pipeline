Shader "Custom RP/Lit"
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
        
        _BaseMap("Texture", 2D) = "white" {}
        
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        // Reason why this is a toggle:
        // A material usually uses either transparency blending or alpha clipping, 
        // not both at the same time. A typical clip material is fully opaque except
        // for the discarded fragments and does write to the depth buffer. 
        // It uses the AlphaTest render queue, which means that it gets rendered after all fully opaque objects. 
        // This is done because discarding fragments makes some GPU optimizations impossible, 
        // as triangles can no longer be assumed to entirely cover what's behind them.
        
        // Name of the property '_Clipping' doesn't matter when using a Toggle
        [Toggle(_CLIPPING)] 
        _Clipping ("Alpha Clipping", Float) = 0
    }
    SubShader
    {
        Pass
        {
            Tags {
				"LightMode" = "CustomLit"
			}
            
            // Blend mode is used to make Transparent objects
            // Src -> What gets drawn now, Dst -> What was drawn earlier
            // Default (Opaque): Src: One, Dst: Zero (Source gets added in full, Dst is ignored)
            Blend [_SrcBlend] [_DstBlend]
            
            // Transparent Rendering shouldn't write to the depth buffer, Opaque Rendering should
            ZWrite [_ZWrite]
HLSLPROGRAM
			#pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "LitPass.hlsl"

ENDHLSL
        }
    }
}
