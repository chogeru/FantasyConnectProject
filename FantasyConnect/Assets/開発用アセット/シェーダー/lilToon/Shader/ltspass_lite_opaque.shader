Shader "Hidden/ltspass_lite_opaque"
{
    Properties
    {
        //----------------------------------------------------------------------------------------------------------------------
        // Base
             _Invisible                  ("", Int) = 0
                        _AsUnlit                    ("", Range(0, 1)) = 0
                        _Cutoff                     ("", Range(-0.001,1.001)) = 0.5
                        _SubpassCutoff              ("", Range(0,1)) = 0.5
             _FlipNormal                 ("", Int) = 0
             _ShiftBackfaceUV            ("", Int) = 0
                        _BackfaceForceShadow        ("", Range(0,1)) = 0
                        _VertexLightStrength        ("", Range(0,1)) = 0
                        _LightMinLimit              ("", Range(0,1)) = 0.05
                        _LightMaxLimit              ("", Range(0,10)) = 1
                        _BeforeExposureLimit        ("", Float) = 10000
                        _MonochromeLighting         ("", Range(0,1)) = 0
                        _AlphaBoostFA               ("", Range(1,100)) = 10
                        _lilDirectionalLightStrength ("", Range(0,1)) = 1
              _LightDirectionOverride     ("", Vector) = (0.001,0.002,0.001,0)
                        _AAStrength                 ("", Range(0, 1)) = 1
         _TriMask                    ("", 2D) = "white" {}

        //----------------------------------------------------------------------------------------------------------------------
        // Main
         [MainColor] _Color                 ("", Color) = (1,1,1,1)
        [MainTexture]   _MainTex                    ("", 2D) = "white" {}
             _MainTex_ScrollRotate       ("", Vector) = (0,0,0,0)

        //----------------------------------------------------------------------------------------------------------------------
        // Shadow
         _UseShadow                  ("", Int) = 0
                        _ShadowBorder               ("", Range(0, 1)) = 0.5
                        _ShadowBlur                 ("", Range(0, 1)) = 0.1
         _ShadowColorTex             ("", 2D) = "black" {}
                        _Shadow2ndBorder            ("", Range(0, 1)) = 0.5
                        _Shadow2ndBlur              ("", Range(0, 1)) = 0.3
         _Shadow2ndColorTex          ("", 2D) = "black" {}
                        _ShadowEnvStrength          ("", Range(0, 1)) = 0
                        _ShadowBorderColor          ("", Color) = (1,0,0,1)
                        _ShadowBorderRange          ("", Range(0, 1)) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // MatCap
         _UseMatCap                  ("", Int) = 0
                        _MatCapTex                  ("", 2D) = "white" {}
              _MatCapBlendUV1             ("", Vector) = (0,0,0,0)
             _MatCapZRotCancel           ("", Int) = 1
             _MatCapPerspective          ("", Int) = 1
                        _MatCapVRParallaxStrength   ("", Range(0, 1)) = 1
             _MatCapMul                  ("", Int) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Rim
         _UseRim                     ("", Int) = 0
                _RimColor                   ("", Color) = (1,1,1,1)
                        _RimBorder                  ("", Range(0, 1)) = 0.5
                        _RimBlur                    ("", Range(0, 1)) = 0.1
        _RimFresnelPower          ("", Range(0.01, 50)) = 3.0
                        _RimShadowMask              ("", Range(0, 1)) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Emmision
         _UseEmission                ("", Int) = 0
        [HDR]   _EmissionColor              ("", Color) = (1,1,1,1)
                        _EmissionMap                ("", 2D) = "white" {}
             _EmissionMap_ScrollRotate   ("", Vector) = (0,0,0,0)
               _EmissionMap_UVMode         ("", Int) = 0
              _EmissionBlink              ("", Vector) = (0,0,3.141593,0)

        //----------------------------------------------------------------------------------------------------------------------
        // Outline
                _OutlineColor               ("", Color) = (0.8,0.85,0.9,1)
                        _OutlineTex                 ("", 2D) = "white" {}
             _OutlineTex_ScrollRotate    ("", Vector) = (0,0,0,0)
            _OutlineWidth               ("", Range(0,1)) = 0.05
         _OutlineWidthMask           ("", 2D) = "white" {}
                        _OutlineFixWidth            ("", Range(0,1)) = 1
               _OutlineVertexR2Width       ("", Int) = 0
             _OutlineDeleteMesh          ("", Int) = 0
                        _OutlineEnableLighting      ("", Range(0, 1)) = 1
                        _OutlineZBias               ("", Float) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Save (Unused)
                                       _BaseColor          ("", Color) = (1,1,1,1)
                                       _BaseMap            ("", 2D) = "white" {}
                                       _BaseColorMap       ("", 2D) = "white" {}
                                       _lilToonVersion     ("", Int) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Advanced
                                               _Cull               ("", Int) = 2
                 _SrcBlend           ("", Int) = 1
                 _DstBlend           ("", Int) = 0
                 _SrcBlendAlpha      ("", Int) = 1
                 _DstBlendAlpha      ("", Int) = 10
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
                                              _StencilRef         ("", Range(0, 255)) = 0
                                              _StencilReadMask    ("", Range(0, 255)) = 255
                                              _StencilWriteMask   ("", Range(0, 255)) = 255
           _StencilComp        ("", Float) = 8
                 _StencilPass        ("", Float) = 0
                 _StencilFail        ("", Float) = 0
                 _StencilZFail       ("", Float) = 0
                                                        _OffsetFactor       ("", Float) = 0
                                                        _OffsetUnits        ("", Float) = 0
                                          _ColorMask          ("", Int) = 15
                                             _AlphaToMask        ("", Int) = 0
                                                        _lilShadowCasterBias ("", Float) = 0

        //----------------------------------------------------------------------------------------------------------------------
        // Outline Advanced
                                               _OutlineCull                ("", Int) = 1
                 _OutlineSrcBlend            ("", Int) = 1
                 _OutlineDstBlend            ("", Int) = 0
                 _OutlineSrcBlendAlpha       ("", Int) = 1
                 _OutlineDstBlendAlpha       ("", Int) = 10
                   _OutlineBlendOp             ("", Int) = 0
                   _OutlineBlendOpAlpha        ("", Int) = 0
                 _OutlineSrcBlendFA          ("", Int) = 1
                 _OutlineDstBlendFA          ("", Int) = 1
                 _OutlineSrcBlendAlphaFA     ("", Int) = 0
                 _OutlineDstBlendAlphaFA     ("", Int) = 1
                   _OutlineBlendOpFA           ("", Int) = 4
                   _OutlineBlendOpAlphaFA      ("", Int) = 4
                                             _OutlineZClip               ("", Int) = 1
                                             _OutlineZWrite              ("", Int) = 1
           _OutlineZTest               ("", Int) = 2
                                              _OutlineStencilRef          ("", Range(0, 255)) = 0
                                              _OutlineStencilReadMask     ("", Range(0, 255)) = 255
                                              _OutlineStencilWriteMask    ("", Range(0, 255)) = 255
           _OutlineStencilComp         ("", Float) = 8
                 _OutlineStencilPass         ("", Float) = 0
                 _OutlineStencilFail         ("", Float) = 0
                 _OutlineStencilZFail        ("", Float) = 0
                                                        _OutlineOffsetFactor        ("", Float) = 0
                                                        _OutlineOffsetUnits         ("", Float) = 0
                                          _OutlineColorMask           ("", Int) = 15
                                             _OutlineAlphaToMask         ("", Int) = 0
    }

    HLSLINCLUDE
        #define LIL_RENDER 0
    ENDHLSL

    SubShader
    {
        HLSLINCLUDE
            #pragma exclude_renderers d3d11_9x
            #pragma fragmentoption ARB_precision_hint_fastest
            #define LIL_LITE

            #pragma skip_variants SHADOWS_SCREEN _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN _ADDITIONAL_LIGHT_SHADOWS SCREEN_SPACE_SHADOWS_ON SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH
            #pragma skip_variants DECALS_OFF DECALS_3RT DECALS_4RT DECAL_SURFACE_GRADIENT _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma skip_variants _ADDITIONAL_LIGHT_SHADOWS
            #pragma skip_variants PROBE_VOLUMES_OFF PROBE_VOLUMES_L1 PROBE_VOLUMES_L2
            #pragma skip_variants _SCREEN_SPACE_OCCLUSION
            #pragma skip_variants _REFLECTION_PROBE_BLENDING _REFLECTION_PROBE_BOX_PROJECTION
        ENDHLSL


        // Forward
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

            #include "Includes/lil_pass_forward.hlsl"

            ENDHLSL
        }

        // Forward Outline
        Pass
        {
            Name "FORWARD_OUTLINE"
            Tags {"LightMode" = "ForwardBase"}

            Stencil
            {
                Ref [_OutlineStencilRef]
                ReadMask [_OutlineStencilReadMask]
                WriteMask [_OutlineStencilWriteMask]
                Comp [_OutlineStencilComp]
                Pass [_OutlineStencilPass]
                Fail [_OutlineStencilFail]
                ZFail [_OutlineStencilZFail]
            }
            Cull [_OutlineCull]
            ZClip [_OutlineZClip]
            ZWrite [_OutlineZWrite]
            ZTest [_OutlineZTest]
            ColorMask [_OutlineColorMask]
            Offset [_OutlineOffsetFactor], [_OutlineOffsetUnits]
            BlendOp [_OutlineBlendOp], [_OutlineBlendOpAlpha]
            Blend [_OutlineSrcBlend] [_OutlineDstBlend], [_OutlineSrcBlendAlpha] [_OutlineDstBlendAlpha]
            AlphaToMask [_OutlineAlphaToMask]

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
            #define LIL_OUTLINE
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_forward.hlsl"

            ENDHLSL
        }

        //----------------------------------------------------------------------------------------------------------------------
        // ForwardAdd Start
        //

        // ForwardAdd
        Pass
        {
            Name "FORWARD_ADD"
            Tags {"LightMode" = "ForwardAdd"}

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
            ZWrite Off
            ZTest LEqual
            ColorMask [_ColorMask]
            Offset [_OffsetFactor], [_OffsetUnits]
            Blend [_SrcBlendFA] [_DstBlendFA], Zero One
            BlendOp [_BlendOpFA], [_BlendOpAlphaFA]
            AlphaToMask [_AlphaToMask]

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment POINT DIRECTIONAL SPOT POINT_COOKIE DIRECTIONAL_COOKIE
            #pragma multi_compile_vertex _ FOG_LINEAR FOG_EXP FOG_EXP2
            #pragma multi_compile_instancing
            #define LIL_PASS_FORWARDADD

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_forward.hlsl"

            ENDHLSL
        }

        // ForwardAdd Outline
        Pass
        {
            Name "FORWARD_ADD_OUTLINE"
            Tags {"LightMode" = "ForwardAdd"}

            Stencil
            {
                Ref [_OutlineStencilRef]
                ReadMask [_OutlineStencilReadMask]
                WriteMask [_OutlineStencilWriteMask]
                Comp [_OutlineStencilComp]
                Pass [_OutlineStencilPass]
                Fail [_OutlineStencilFail]
                ZFail [_OutlineStencilZFail]
            }
            Cull [_OutlineCull]
            ZClip [_OutlineZClip]
            ZWrite Off
            ZTest LEqual
            ColorMask [_OutlineColorMask]
            Offset [_OutlineOffsetFactor], [_OutlineOffsetUnits]
            Blend [_OutlineSrcBlendFA] [_OutlineDstBlendFA], Zero One
            BlendOp [_OutlineBlendOpFA], [_OutlineBlendOpAlphaFA]
            AlphaToMask [_OutlineAlphaToMask]

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment POINT DIRECTIONAL SPOT POINT_COOKIE DIRECTIONAL_COOKIE
            #pragma multi_compile_vertex _ FOG_LINEAR FOG_EXP FOG_EXP2
            #pragma multi_compile_instancing
            #define LIL_PASS_FORWARDADD

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #define LIL_OUTLINE
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_forward.hlsl"

            ENDHLSL
        }

        //
        // ForwardAdd End

        // ShadowCaster
        Pass
        {
            Name "SHADOW_CASTER"
            Tags {"LightMode" = "ShadowCaster"}
            Offset 1, 1
            Cull [_Cull]

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #define LIL_PASS_SHADOWCASTER

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_shadowcaster.hlsl"

            ENDHLSL
        }

        // ShadowCaster Outline
        Pass
        {
            Name "SHADOW_CASTER_OUTLINE"
            Tags {"LightMode" = "ShadowCaster"}
            Offset 1, 1
            Cull [_Cull]

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #define LIL_PASS_SHADOWCASTER

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #define LIL_OUTLINE
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_shadowcaster.hlsl"

            ENDHLSL
        }

        // Meta
        Pass
        {
            Name "META"
            Tags {"LightMode" = "Meta"}
            Cull Off

            HLSLPROGRAM

            //----------------------------------------------------------------------------------------------------------------------
            // Build Option
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature EDITOR_VISUALIZATION
            #define LIL_PASS_META

            //----------------------------------------------------------------------------------------------------------------------
            // Pass
            #include "Includes/lil_pipeline_brp.hlsl"
            #include "Includes/lil_common.hlsl"
            // Insert functions and includes that depend on Unity here

            #include "Includes/lil_pass_meta.hlsl"

            ENDHLSL
        }

    }
    Fallback "Unlit/Texture"
}

