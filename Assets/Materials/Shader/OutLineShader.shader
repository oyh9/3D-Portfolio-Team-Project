Shader "Custom/TrueEdgeOutlineShader"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _EdgeThreshold ("Edge Threshold", Range(0.0, 1.0)) = 0.2
        _EdgeSharpness ("Edge Sharpness", Range(1.0, 32.0)) = 16.0
        _PulseSpeed ("Pulse Speed", Float) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        // 모서리 검출 패스
        Pass
        {
            ZWrite Off
            Blend One One // 가산 블렌딩
            Cull Back // 앞면만 렌더링
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 viewDir : TEXCOORD0;
                float3 tangent : TEXCOORD1;
                float3 bitangent : TEXCOORD2;
            };

            float4 _EdgeColor;
            float _EdgeThreshold;
            float _EdgeSharpness;
            float _PulseSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                // 탄젠트 공간 계산
                o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangent.w);
                
                // 뷰 방향
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 탄젠트 공간에서의 뷰 방향
                float3 tangentViewDir;
                tangentViewDir.x = dot(i.viewDir, i.tangent);
                tangentViewDir.y = dot(i.viewDir, i.bitangent);
                tangentViewDir.z = dot(i.viewDir, i.normal);
                
                // 탄젠트 공간 뷰 벡터의 xy 성분만 사용 (z축과의 각도 계산)
                float2 viewDirXY = normalize(tangentViewDir.xy);
                float edgeFactor = length(viewDirXY);
                
                // 에지 강화 및 임계값 적용
                edgeFactor = pow(edgeFactor, _EdgeSharpness);
                edgeFactor = smoothstep(_EdgeThreshold, _EdgeThreshold + 0.1, edgeFactor);
                
                // 맥동 효과
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) * 0.5 + 0.5;
                
                // 최종 모서리 강도
                edgeFactor *= pulse;
                
                // 모서리가 아닌 부분은 완전히 투명하게
                clip(edgeFactor - 0.01);
                
                return float4(_EdgeColor.rgb * edgeFactor, 0);
            }
            ENDCG
        }
    }
}