Shader "Unlit/CustomBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        _SamplingDistance ("Sampling Distance", Range(0.5, 3)) = 1.5

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        GrabPass {}

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        CGINCLUDE

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        #pragma multi_compile __ UNITY_UI_CLIP_RECT
        #pragma multi_compile __ UNITY_UI_ALPHACLIP

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            float2 texcoord  : TEXCOORD0;
            float4 worldPosition : TEXCOORD1;
            float4 grabPos : TEXCOORD2;
            fixed4 color : COLOR;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        sampler2D _MainTex;
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;
        float4 _MainTex_ST;
        sampler2D _GrabTexture;
        fixed4 _GrabTexture_TexelSize;
        float _SamplingDistance;

        static const int samplingCount = 7;
        static const half weights[samplingCount] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };

        v2f vert(appdata_t v)
        {
            v2f OUT;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
            OUT.worldPosition = v.vertex;
            OUT.color = v.color;
            OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
            OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
            return OUT;
        }

        float alpha(float2 uv, float2 xy)
        {
            // Alpha Clip は元の画像ので行う
            fixed4 color = tex2D(_MainTex, uv);

            #ifdef UNITY_UI_CLIP_RECT
            color.a *= UnityGet2DClipping(xy, _ClipRect);
            #endif

            #ifdef UNITY_UI_ALPHACLIP
            clip (color.a - 0.001);
            #endif

            return color.a;
        }

        fixed4 blur(float4 grabPos, float4 pos)
        {
            fixed4 col = fixed4(0, 0, 0, 0);

            [unroll]
            for (int j = -3; j <= 3; j++)
            {
                float4 offset = float4(j * _GrabTexture_TexelSize.xy * pos.xy * _SamplingDistance, 0, 0);
                col += tex2Dproj(_GrabTexture, grabPos + offset) * weights[j + 3];
            }

            return col;
        }

        ENDCG

        // 横ブラー
        Pass
        {
        CGPROGRAM

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 blurColor = blur(IN.grabPos, fixed4(1, 0, 0, 0));
                blurColor.a = alpha(IN.texcoord, IN.worldPosition.xy);

                return blurColor;
            }
        ENDCG
        }

        GrabPass {}

        // 横ブラー
        Pass
        {
        CGPROGRAM
            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 blurColor = blur(IN.grabPos, fixed4(0, 1, 0, 0));

                blurColor.a = alpha(IN.texcoord, IN.worldPosition.xy);

                blurColor.rgb += IN.color.rgb;
                blurColor.a *= IN.color.a;

                return blurColor;
            }
        ENDCG
        }

    }
}