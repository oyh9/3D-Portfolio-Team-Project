Shader "Custom/GravityBeamShader"
{
    Properties
    {
        _BeamColor("Beam Color", Color) = (0, 0.8, 1, 1)
        _PulseSpeed("Pulse Speed", Range(0, 10)) = 1
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.1
        _BeamWidth("Beam Width", Range(0.1, 1)) = 0.5
        _BeamLength("Beam Length", Range(0.1, 10)) = 1
        _BeamOffset("Beam Offset", Vector) = (0, 0, 0, 0) // 빔 위치 오프셋
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }
        LOD 100

        // 렌더 상태 명령어
        Blend One One // Additive 블렌딩
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _BeamColor;
            float _PulseSpeed;
            float _NoiseIntensity;
            float _BeamWidth;
            float _BeamLength;
            float4 _BeamOffset;
            
            // 간단한 2D 노이즈 함수
            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            v2f vert(appdata v)
            {
                v2f o;
                // 오프셋 적용
                float3 vertexPos = v.vertex.xyz + _BeamOffset.xyz;
                o.vertex = UnityObjectToClipPos(float4(vertexPos, 1.0));
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 시간 기반 애니메이션
                float time = _Time.y * _PulseSpeed;
                
                // UV 애니메이션
                float2 animatedUV = i.uv + float2(time * 0.1, 0);
                
                // 노이즈 생성 (UV 애니메이션 적용)
                float noise = unity_gradientNoise(animatedUV * 20) * _NoiseIntensity;
                
                // 광선 폭 제어 (y축 UV 기준)
                float edgeFade = 1.0 - abs((i.uv.y * 2 - 1) / _BeamWidth);
                edgeFade = saturate(edgeFade);
                
                // 광선 길이 제어 (x축 UV 기준)
                float lengthFade = 1.0 - smoothstep(_BeamLength - 0.1, _BeamLength, i.uv.x);
                
                // 맥동 효과
                float pulse = sin(time * 3) * 0.5 + 0.5;
                float pulseFactor = lerp(1.0, 1.3, pulse);
                
                // 최종 색상 계산
                fixed4 finalColor = _BeamColor * (edgeFade + noise) * pulseFactor * lengthFade;
                
                // 알파 계산
                float alpha = edgeFade * _BeamColor.a * lengthFade;
                
                // 최종 출력 (Additive 블렌딩이므로 알파는 색상에 이미 곱해짐)
                finalColor = fixed4(finalColor.rgb, alpha);
                
                // 안개 적용
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}