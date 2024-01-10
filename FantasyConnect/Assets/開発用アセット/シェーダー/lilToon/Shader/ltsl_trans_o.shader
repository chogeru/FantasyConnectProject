Shader "Hidden/lilToonLiteTransparentOutline"
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
                 _DstBlend           ("", Int) = 10
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
                 _OutlineSrcBlend            ("", Int) = 5
                 _OutlineDstBlend            ("", Int) = 10
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

        //----------------------------------------------------------------------------------------------------------------------
        // Pre
         [MainColor]                            _PreColor               ("", Color) = (1,1,1,1)
                                               _PreOutType             ("", Int) = 0
                                                        _PreCutoff              ("", Range(-0.001,1.001)) = 0.5
                                               _PreCull                ("", Int) = 2
                 _PreSrcBlend            ("", Int) = 1
                 _PreDstBlend            ("", Int) = 10
                 _PreSrcBlendAlpha       ("", Int) = 1
                 _PreDstBlendAlpha       ("", Int) = 10
                   _PreBlendOp             ("", Int) = 0
                   _PreBlendOpAlpha        ("", Int) = 0
                 _PreSrcBlendFA          ("", Int) = 1
                 _PreDstBlendFA          ("", Int) = 1
                 _PreSrcBlendAlphaFA     ("", Int) = 0
                 _PreDstBlendAlphaFA     ("", Int) = 1
                   _PreBlendOpFA           ("", Int) = 4
                   _PreBlendOpAlphaFA      ("", Int) = 4
                                             _PreZClip               ("", Int) = 1
                                             _PreZWrite              ("", Int) = 1
           _PreZTest               ("", Int) = 4
                                              _PreStencilRef          ("", Range(0, 255)) = 0
                                              _PreStencilReadMask     ("", Range(0, 255)) = 255
                                              _PreStencilWriteMask    ("", Range(0, 255)) = 255
           _PreStencilComp         ("", Float) = 8
                 _PreStencilPass         ("", Float) = 0
                 _PreStencilFail         ("", Float) = 0
                 _PreStencilZFail        ("", Float) = 0
                                                        _PreOffsetFactor        ("", Float) = 0
                                                        _PreOffsetUnits         ("", Float) = 0
                                          _PreColorMask           ("", Int) = 15
                                             _PreAlphaToMask         ("", Int) = 0
    }

    SubShader
    {
        Tags {"RenderType" = "TransparentCutout" "Queue" = "AlphaTest+10"}
        UsePass "Hidden/ltspass_lite_transparent/FORWARD"
        UsePass "Hidden/ltspass_lite_transparent/FORWARD_OUTLINE"
        UsePass "Hidden/ltspass_lite_transparent/FORWARD_ADD"
        UsePass "Hidden/ltspass_lite_transparent/FORWARD_ADD_OUTLINE"
        UsePass "Hidden/ltspass_lite_transparent/SHADOW_CASTER_OUTLINE"
        UsePass "Hidden/ltspass_lite_transparent/META"
    }
    Fallback "Unlit/Texture"

    CustomEditor "lilToon.lilToonInspector"
}

