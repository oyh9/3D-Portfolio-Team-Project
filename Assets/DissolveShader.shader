
Shader "Custom/EffectShader" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _MetallicGlossMap ("Metallic", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // 디졸브 관련 프로퍼티
        [Toggle] _EnableDissolve ("Enable Dissolve", Float) = 0
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _EdgeColor ("Dissolve Edge Color", Color) = (1,0,0,1)
        _EdgeWidth ("Dissolve Edge Width", Range(0,0.1)) = 0.05
        
        // 아웃라인 관련 프로퍼티
        [Toggle] _EnableOutline ("Enable Outline", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0,0,1,1)
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.02
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        // 아웃라인 패스 (먼저 렌더링)
        Pass {
            Name "OUTLINE"
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _EnableOutline;
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            v2f vert(appdata v) {
                v2f o;
                // 아웃라인이 활성화된 경우에만 확장
                float3 normal = normalize(v.normal) * _OutlineWidth * _EnableOutline;
                float3 pos = v.vertex + normal;
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
        
        // 메인 렌더링 패스
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NoiseTex;
        sampler2D _NormalMap;
        sampler2D _EmissionMap;
        sampler2D _MetallicGlossMap;
        
        float _EnableDissolve;

        struct Input {
            float2 uv_MainTex;
            float2 uv_NoiseTex;
            float2 uv_NormalMap;
            float2 uv_EmissionMap;
            float2 uv_MetallicGlossMap;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _EmissionColor;
        float _DissolveAmount;
        fixed4 _EdgeColor;
        float _EdgeWidth;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // 기본 텍스처 색상
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            
            // 노말맵 적용
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
            
            // 메탈릭/스무스니스 값 가져오기
            fixed4 metallicGloss = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap);
            o.Metallic = metallicGloss.r * _Metallic;
            o.Smoothness = metallicGloss.a * _Glossiness;
            
            // 이미션맵 적용
            fixed4 emission = tex2D(_EmissionMap, IN.uv_EmissionMap) * _EmissionColor;
            o.Emission = emission.rgb;
            
            // 디졸브 효과가 활성화된 경우에만 처리
            if (_EnableDissolve > 0.5) {
                // 노이즈 텍스처에서 디졸브 패턴 값 가져오기
                fixed noiseValue = tex2D(_NoiseTex, IN.uv_NoiseTex).r;
                
                // 디졸브 클리핑 값 계산
                half dissolveValue = noiseValue - _DissolveAmount;
                
                // 가장자리 효과 계산
                half edgeValue = noiseValue - (_DissolveAmount - _EdgeWidth);
                fixed edgeLerp = saturate(edgeValue / _EdgeWidth);
                
                // 디졸브 가장자리 색상 적용
                o.Emission += (1.0 - edgeLerp) * _EdgeColor.rgb * step(0.0001, dissolveValue);
                
                // 알파값 설정 (디졸브 효과를 위한 클리핑)
                clip(dissolveValue);
            }
            
            // 최종 색상 설정
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}