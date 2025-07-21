Shader "UI/CircleMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        
        _Radius ("Radius", Range(0, 2)) = 1.5
        _CenterX ("Center X", Range(0, 1)) = 0.5
        _CenterY ("Center Y", Range(0, 1)) = 0.5
        _SmoothEdge ("Smooth Edge", Range(0, 0.1)) = 0.005
        _AspectRatio ("Aspect Ratio", Float) = 1.777 // 16:9 기본값
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        _ColorMask ("Color Mask", Float) = 15
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
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            float _Radius;
            float _CenterX;
            float _CenterY;
            float _SmoothEdge;
            float _AspectRatio;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // 중심점으로부터의 거리 계산 (화면 비율 고려)
                float2 center = float2(_CenterX, _CenterY);
                
                // 화면 비율을 고려한 UV 좌표 조정
                float2 adjustedUV = IN.texcoord;
                adjustedUV.x = (adjustedUV.x - center.x) * _AspectRatio + center.x;
                
                // 조정된 좌표로 거리 계산
                float dist = distance(adjustedUV, center);
                
                // 원본 색상 가져오기
                half4 color = IN.color;
                
                // 원형 마스크 - 원 내부는 투명, 원 외부는 불투명
                float t = smoothstep(_Radius - _SmoothEdge, _Radius, dist);
                
                // t가 0이면 원 내부(투명), 1이면 원 외부(불투명)
                color.a *= t;
                
                return color;
            }
            ENDCG
        }
    }
}