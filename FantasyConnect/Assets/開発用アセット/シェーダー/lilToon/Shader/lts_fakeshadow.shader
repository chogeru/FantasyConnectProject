Shader "_lil/[Optional] lilToonFakeShadow"
{
    Properties
    {
        //----------------------------------------------------------------------------------------------------------------------
        // Base
             _Invisible                  ("", Int) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Main
         [MainColor] _Color                 ("", Color) = (0.925,0.7,0.74,1)
        [MainTexture]   _MainTex                    ("", 2D) = "white" {}
          _FakeShadowVector           ("", Vector) = (0,0,0,0.05)

        //----------------------------------------------------------------------------------------------------------------------
        // Encryption
             _IgnoreEncryption           ("", Int) = 0
                        _Keys                       ("", Vector) = (0,0,0,0)
                        _BitKey0                    ("", Float) = 0
                        _BitKey1                    ("", Float) = 0
                        _BitKey2                    ("", Float) = 0
                        _BitKey3                    ("", Float) = 0
                        _BitKey4                    ("", Float) = 0
                        _BitKey5                    ("", Float) = 0
                        _BitKey6                    ("", Float) = 0
                        _BitKey7                    ("", Float) = 0
                        _BitKey8                    ("", Float) = 0
                        _BitKey9                    ("", Float) = 0
                        _BitKey10                   ("", Float) = 0
                        _BitKey11                   ("", Float) = 0
                        _BitKey12                   ("", Float) = 0
                        _BitKey13                   ("", Float) = 0
                        _BitKey14                   ("", Float) = 0
                        _BitKey15                   ("", Float) = 0
                        _BitKey16                   ("", Float) = 0
                        _BitKey17                   ("", Float) = 0
                        _BitKey18                   ("", Float) = 0
                        _BitKey19                   ("", Float) = 0
                        _BitKey20                   ("", Float) = 0
                        _BitKey21                   ("", Float) = 0
                        _BitKey22                   ("", Float) = 0
                        _BitKey23                   ("", Float) = 0
                        _BitKey24                   ("", Float) = 0
                        _BitKey25                   ("", Float) = 0
                        _BitKey26                   ("", Float) = 0
                        _BitKey27                   ("", Float) = 0
                        _BitKey28                   ("", Float) = 0
                        _BitKey29                   ("", Float) = 0
                        _BitKey30                   ("", Float) = 0
                        _BitKey31                   ("", Float) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Advanced
                                               _Cull               ("", Int) = 2
                 _SrcBlend           ("", Int) = 2
                 _DstBlend           ("", Int) = 0
                 _SrcBlendAlpha      ("", Int) = 0
                 _DstBlendAlpha      ("", Int) = 1
                   _BlendOp            ("", Int) = 0
                   _BlendOpAlpha       ("", Int) = 0
                 _SrcBlendFA         ("", Int) = 1
                 _DstBlendFA         ("", Int) = 1
                 _SrcBlendAlphaFA    ("", Int) = 0
                 _DstBlendAlphaFA    ("", Int) = 1
                   _BlendOpFA          ("", Int) = 4
                   _BlendOpAlphaFA     ("", Int) = 4
                                             _ZClip              ("", Int) = 1
                                             _ZWrite             ("", Int) = 1
           _ZTest              ("", Int) = 4
                                              _StencilRef         ("", Range(0, 255)) = 51
                                              _StencilReadMask    ("", Range(0, 255)) = 255
                                              _StencilWriteMask   ("", Range(0, 255)) = 255
           _StencilComp        ("", Float) = 3
                 _StencilPass        ("", Float) = 0
                 _StencilFail        ("", Float) = 0
                 _StencilZFail       ("", Float) = 0
                                                        _OffsetFactor       ("", Float) = 0
                                                        _OffsetUnits        ("", Float) = 0
                                          _ColorMask          ("", Int) = 15
                                             _AlphaToMask        ("", Int) = 0
                                                        _lilShadowCasterBias ("", Float) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Save (Unused)
                                       _BaseColor          ("", Color) = (1,1,1,1)
                                       _BaseMap            ("", 2D) = "white" {}
                                       _BaseColorMap       ("", 2D) = "white" {}
                                       _lilToonVersion     ("", Int) = 0
    }

    SubShader
    {
        Tags {"RenderType" = "Transparent" "Queue" = "AlphaTest+55"}
        HLSLINCLUDE
            #define LIL_OPTIMIZE_APPLY_SHADOW_FA
            #define LIL_OPTIMIZE_USE_FORWARDADD
            #pragma skip_variants _REFLECTION_PROBE_BLENDING _REFLECTION_PROBE_BOX_PROJECTION
            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants LIGHTMAP_ON DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRLIGHTMAP_COMBINED _MIXED_LIGHTING_SUBTRACTIVE
            #pragma target 3.5
            #pragma fragmentoption ARB_precision_hint_fastest
            #define LIL_FAKESHADOW

            #pragma skip_variants SHADOWS_SCREEN _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN _ADDITIONAL_LIGHT_SHADOWS SCREEN_SPACE_SHADOWS_ON SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH
            #pragma skip_variants LIGHTMAP_ON DYNAMICLIGHTMAP_ON LIGHTMAP_SHADOW_MIXING SHADOWS_SHADOWMASK DIRLIGHTMAP_COMBINED _MIXED_LIGHTING_SUBTRACTIVE
            #pragma skip_variants DECALS_OFF DECALS_3RT DECALS_4RT DECAL_SURFACE_GRADIENT _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants _ADDITIONAL_LIGHT_SHADOWS
            #pragma skip_variants PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma skip_variants _SCREEN_SPACE_OCCLUSION
            #pragma skip_variants _REFLECTION_PROBE_BLENDING _REFLECTION_PROBE_BOX_PROJECTION
        ENDHLSL


        Pass
        {
            Name "FORWARD"
            Tags {"LightMode" = "ForwardBase"}

            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }
            Cull [_Cull]
            ZClip [_ZClip]
            ZWrite [_ZWrite]
            ZTest [_ZTest]
            ColorMask [_ColorMask]
            Offset [_OffsetFactor], [_OffsetUnits]
            BlendOp [_BlendOp], [_BlendOpAlpha]
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            AlphaToMask [_AlphaToMask]

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_vertex _ FOG_LINEAR FOG_EXP FOG_EXP2
            #pragma multi_compile_instancing
            #define LIL_PASS_FORWARD

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_forward_fakeshadow.hlsl"

            ENDHLSL
        }

    }
    Fallback "Unlit/Texture"

    CustomEditor "lilToon.lilToonInspector"
}

