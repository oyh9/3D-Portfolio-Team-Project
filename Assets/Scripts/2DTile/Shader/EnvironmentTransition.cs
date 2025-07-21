using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class EnvironmentTransition : MonoBehaviour
{
    [Header("스카이박스 설정")]
    [Tooltip("현재 스카이박스 머티리얼")]
    public Material currentSkybox;
    [Tooltip("변경할 스카이박스 머티리얼")]
    public Material targetSkybox;
    [Tooltip("부드러운 블렌딩 사용 (같은 종류의 스카이박스일 때)")]
    public bool useSmoothBlending = true;
    
    public Shader cubemapBlendShader;
    
    [Header("조명 설정")]
    [Tooltip("Directional Light")]
    public Light directionalLight;
    [Tooltip("타겟 라이트 색상")]
    public Color targetLightColor = Color.white;
    [Tooltip("타겟 라이트 강도")]
    public float targetLightIntensity = 1.0f;
    
    [Header("앰비언트 설정")]
    [Tooltip("타겟 앰비언트 색상")]
    public Color targetAmbientColor = Color.gray;
    
    [Header("전환 설정")]
    [Tooltip("전환에 걸리는 시간(초)")]
    public float transitionDuration = 3.0f;
    [Tooltip("페이드 아웃-인 효과 사용")]
    public bool useFadeEffect = true;
    [Tooltip("페이드 효과 강도 (0-1)")]
    [Range(0f, 1f)]
    public float fadeIntensity = 0.3f;
    
    // 초기 값들을 저장할 변수
    private Material originalSkybox;
    private Color originalLightColor;
    private float originalLightIntensity;
    private Color originalAmbientColor;
    
    // 블렌딩을 위한 머티리얼
    private Material blendMaterial;
    
    // 전환 중인지 체크하는 플래그
    private bool isTransitioning = false;
    private bool _isChanged = false;
    
    
    
    void Start()
    {
        // 현재 설정값들을 저장
        originalSkybox = RenderSettings.skybox;
        if (directionalLight != null)
        {
            originalLightColor = directionalLight.color;
            originalLightIntensity = directionalLight.intensity;
        }
        originalAmbientColor = RenderSettings.ambientLight;
        
        // currentSkybox가 null이면 현재 렌더링 설정의 스카이박스를 사용
        if (currentSkybox == null)
        {
            currentSkybox = RenderSettings.skybox;
        }
    }
    
    public void StartTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionCoroutine());
        }
    }
    
    public void RevertToOriginal()
    {
        if (!isTransitioning)
        {
            StartCoroutine(RevertCoroutine());
        }
    }
    
    private IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;
        float elapsedTime = 0f;
        
        // 시작 값들을 저장
        Material startSkybox = RenderSettings.skybox;
        Color startLightColor = directionalLight.color;
        float startLightIntensity = directionalLight.intensity;
        Color startAmbientColor = RenderSettings.ambientLight;
        
        // 스카이박스 블렌딩 준비
        bool canBlendSkybox = false;
        
        // 스카이박스 타입 확인 및 블렌딩 설정
        if (useSmoothBlending && startSkybox != null && targetSkybox != null)
        {
            if (startSkybox.shader == targetSkybox.shader)
            {
                canBlendSkybox = true;
                
                if (IsCubemapSkybox(startSkybox))
                {
                    // 큐브맵 블렌딩을 위한 머티리얼 생성
                    Shader blendShader = cubemapBlendShader;
                    if (blendShader != null)
                    {
                        blendMaterial = new Material(blendShader);
                        // 초기 설정 - 시작 큐브맵으로 시작
                        blendMaterial.SetTexture("_Tex", startSkybox.GetTexture("_Tex"));
                        blendMaterial.SetTexture("_Tex2", targetSkybox.GetTexture("_Tex"));
                        blendMaterial.SetFloat("_Blend", 0f);
                        
                        // 기타 속성들도 복사
                        CopyMaterialProperties(startSkybox, blendMaterial);
                    }
                    else
                    {
                        // 커스텀 셰이더가 없으면 기본 머티리얼 사용
                        blendMaterial = new Material(startSkybox);
                        canBlendSkybox = false;
                    }
                }
                else
                {
                    // Procedural 또는 기타 스카이박스를 위한 블렌드 머티리얼
                    blendMaterial = new Material(startSkybox);
                }
                
                if (canBlendSkybox)
                {
                    RenderSettings.skybox = blendMaterial;
                }
            }
        }
        
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            
            // 스카이박스 전환
            if (canBlendSkybox)
            {
                // 부드러운 전환 곡선 적용 (Ease In-Out)
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                BlendSkyboxProperties(startSkybox, targetSkybox, smoothT);
            }
            else
            {
                // 블렌딩이 불가능한 경우
                if (useFadeEffect)
                {
                    // 페이드 효과로 부드럽게 전환
                    float fadeT = GetFadeCurve(t);
                    
                    // 조명과 앰비언트의 밝기도 함께 조절
                    if (t < 0.5f)
                    {
                        // 페이드 아웃
                        float fadeOut = 1f - (fadeT * fadeIntensity);
                        
                        if (directionalLight != null)
                        {
                            directionalLight.intensity = startLightIntensity * fadeOut;
                            // 색상은 원래 색상을 유지하면서 강도만 줄임
                            directionalLight.color = startLightColor;
                        }
                        // 앰비언트 라이트도 색상은 유지하고 강도만 조절
                        float ambientFade = Mathf.Lerp(1f, 1f - fadeIntensity, fadeT);
                        RenderSettings.ambientLight = startAmbientColor * ambientFade;
                        
                        // 스카이박스의 Exposure나 Tint 조절 (있는 경우)
                        if (RenderSettings.skybox != null)
                        {
                            if (RenderSettings.skybox.HasProperty("_Exposure"))
                            {
                                float originalExposure = startSkybox.HasProperty("_Exposure") ? startSkybox.GetFloat("_Exposure") : 1.0f;
                                RenderSettings.skybox.SetFloat("_Exposure", originalExposure * fadeOut);
                            }
                            if (RenderSettings.skybox.HasProperty("_Tint"))
                            {
                                Color originalTint = startSkybox.HasProperty("_Tint") ? startSkybox.GetColor("_Tint") : Color.white;
                                RenderSettings.skybox.SetColor("_Tint", originalTint * fadeOut);
                            }
                        }
                    }
                    else
                    {
                        // 스카이박스 교체
                        if (RenderSettings.skybox != targetSkybox)
                        {
                            RenderSettings.skybox = targetSkybox;
                        }
                        
                        // 페이드 인
                        float fadeIn = (fadeT * fadeIntensity) + (1f - fadeIntensity);
                        
                        if (directionalLight != null)
                        {
                            // 색상과 강도 모두 목표값으로 점진적 전환
                            directionalLight.intensity = Mathf.Lerp(startLightIntensity * (1f - fadeIntensity), targetLightIntensity, (t - 0.5f) * 2f);
                            directionalLight.color = Color.Lerp(startLightColor, targetLightColor, (t - 0.5f) * 2f);
                        }
                        
                        // 앰비언트 라이트도 점진적 전환
                        float ambientFade = Mathf.Lerp(1f - fadeIntensity, 1f, (t - 0.5f) * 2f);
                        RenderSettings.ambientLight = Color.Lerp(startAmbientColor * (1f - fadeIntensity), targetAmbientColor, (t - 0.5f) * 2f);
                        
                        // 새 스카이박스의 Exposure나 Tint 조절
                        if (RenderSettings.skybox != null)
                        {
                            if (RenderSettings.skybox.HasProperty("_Exposure"))
                            {
                                float targetExposure = targetSkybox.HasProperty("_Exposure") ? targetSkybox.GetFloat("_Exposure") : 1.0f;
                                RenderSettings.skybox.SetFloat("_Exposure", targetExposure * fadeIn);
                            }
                            if (RenderSettings.skybox.HasProperty("_Tint"))
                            {
                                Color targetTint = targetSkybox.HasProperty("_Tint") ? targetSkybox.GetColor("_Tint") : Color.white;
                                RenderSettings.skybox.SetColor("_Tint", targetTint * fadeIn);
                            }
                        }
                    }
                }
                else
                {
                    // 단순 전환
                    if (t > 0.5f && RenderSettings.skybox != targetSkybox)
                    {
                        RenderSettings.skybox = targetSkybox;
                    }
                }
            }
            
            // 라이트 색상과 강도 보간 (페이드 효과가 없는 경우)
            if (!useFadeEffect || canBlendSkybox)
            {
                if (directionalLight != null)
                {
                    // 부드러운 전환을 위한 SmoothStep 적용
                    float smoothT = Mathf.SmoothStep(0f, 1f, t);
                    directionalLight.color = Color.Lerp(startLightColor, targetLightColor, smoothT);
                    directionalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, smoothT);
                }
                
                // 앰비언트 색상 보간
                RenderSettings.ambientLight = Color.Lerp(startAmbientColor, targetAmbientColor, Mathf.SmoothStep(0f, 1f, t));
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 값 설정
        RenderSettings.skybox = targetSkybox;
        
        if (directionalLight != null)
        {
            directionalLight.color = targetLightColor;
            directionalLight.intensity = targetLightIntensity;
        }
        RenderSettings.ambientLight = targetAmbientColor;
        
        DynamicGI.UpdateEnvironment();
        
        isTransitioning = false;
    }
    
    // 페이드 곡선 함수 (0에서 1로 갔다가 다시 0으로 가는 부드러운 곡선)
    private float GetFadeCurve(float t)
    {
        if (t < 0.5f)
        {
            // 더 부드러운 페이드 아웃
            return t * 2f;
        }
        else
        {
            // 더 부드러운 페이드 인
            return (t - 0.5f) * 2f;
        }
    }
    
    // 머티리얼 속성 복사 함수
    private void CopyMaterialProperties(Material source, Material target)
    {
        if (source.HasProperty("_Tint"))
            target.SetColor("_Tint", source.GetColor("_Tint"));
        if (source.HasProperty("_Exposure"))
            target.SetFloat("_Exposure", source.GetFloat("_Exposure"));
        if (source.HasProperty("_Rotation"))
            target.SetFloat("_Rotation", source.GetFloat("_Rotation"));
    }
    
    private IEnumerator RevertCoroutine()
    {
        isTransitioning = true;
        float elapsedTime = 0f;
        
        // 시작 값들을 저장
        Material startSkybox = RenderSettings.skybox;
        Color startLightColor = directionalLight.color;
        float startLightIntensity = directionalLight.intensity;
        Color startAmbientColor = RenderSettings.ambientLight;
        
        // 스카이박스 블렌딩 준비
        bool canBlendSkybox = useSmoothBlending && CanBlendSkyboxes(startSkybox, originalSkybox);
        
        if (canBlendSkybox && IsCubemapSkybox(startSkybox))
        {
            Shader blendShader = cubemapBlendShader;
            if (blendShader != null)
            {
                blendMaterial = new Material(blendShader);
                blendMaterial.SetTexture("_Tex", startSkybox.GetTexture("_Tex"));
                blendMaterial.SetTexture("_Tex2", originalSkybox.GetTexture("_Tex"));
                blendMaterial.SetFloat("_Blend", 0f);
                
                // 속성 복사
                CopyMaterialProperties(startSkybox, blendMaterial);
                    
                RenderSettings.skybox = blendMaterial;
            }
            else
            {
                canBlendSkybox = false;
            }
        }
        else if (canBlendSkybox)
        {
            blendMaterial = new Material(startSkybox);
            RenderSettings.skybox = blendMaterial;
        }
        
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            // 스카이박스 복원
            if (canBlendSkybox)
            {
                BlendSkyboxProperties(startSkybox, originalSkybox, smoothT);
            }
            else
            {
                if (useFadeEffect)
                {
                    // 동일한 페이드 효과 적용
                    float fadeT = GetFadeCurve(t);
                    
                    if (t < 0.5f)
                    {
                        float fadeOut = 1f - (fadeT * fadeIntensity);
                        
                        if (directionalLight != null)
                        {
                            directionalLight.intensity = startLightIntensity * fadeOut;
                            directionalLight.color = startLightColor;
                        }
                        
                        float ambientFade = Mathf.Lerp(1f, 1f - fadeIntensity, fadeT);
                        RenderSettings.ambientLight = startAmbientColor * ambientFade;
                        
                        if (RenderSettings.skybox != null)
                        {
                            if (RenderSettings.skybox.HasProperty("_Exposure"))
                            {
                                float originalExposure = startSkybox.HasProperty("_Exposure") ? startSkybox.GetFloat("_Exposure") : 1.0f;
                                RenderSettings.skybox.SetFloat("_Exposure", originalExposure * fadeOut);
                            }
                            if (RenderSettings.skybox.HasProperty("_Tint"))
                            {
                                Color originalTint = startSkybox.HasProperty("_Tint") ? startSkybox.GetColor("_Tint") : Color.white;
                                RenderSettings.skybox.SetColor("_Tint", originalTint * fadeOut);
                            }
                        }
                    }
                    else
                    {
                        if (RenderSettings.skybox != originalSkybox)
                        {
                            RenderSettings.skybox = originalSkybox;
                        }
                        
                        float fadeIn = (fadeT * fadeIntensity) + (1f - fadeIntensity);
                        
                        if (directionalLight != null)
                        {
                            directionalLight.intensity = Mathf.Lerp(startLightIntensity * (1f - fadeIntensity), originalLightIntensity, (t - 0.5f) * 2f);
                            directionalLight.color = Color.Lerp(startLightColor, originalLightColor, (t - 0.5f) * 2f);
                        }
                        
                        float ambientFade = Mathf.Lerp(1f - fadeIntensity, 1f, (t - 0.5f) * 2f);
                        RenderSettings.ambientLight = Color.Lerp(startAmbientColor * (1f - fadeIntensity), originalAmbientColor, (t - 0.5f) * 2f);
                        
                        if (RenderSettings.skybox != null)
                        {
                            if (RenderSettings.skybox.HasProperty("_Exposure"))
                            {
                                float originalExposure = originalSkybox.HasProperty("_Exposure") ? originalSkybox.GetFloat("_Exposure") : 1.0f;
                                RenderSettings.skybox.SetFloat("_Exposure", originalExposure * fadeIn);
                            }
                            if (RenderSettings.skybox.HasProperty("_Tint"))
                            {
                                Color originalTint = originalSkybox.HasProperty("_Tint") ? originalSkybox.GetColor("_Tint") : Color.white;
                                RenderSettings.skybox.SetColor("_Tint", originalTint * fadeIn);
                            }
                        }
                    }
                }
                else
                {
                    if (t > 0.5f && RenderSettings.skybox != originalSkybox)
                    {
                        RenderSettings.skybox = originalSkybox;
                    }
                }
            }
            
            // 라이트 값들 복원
            if (!useFadeEffect || canBlendSkybox)
            {
                if (directionalLight != null)
                {
                    directionalLight.color = Color.Lerp(startLightColor, originalLightColor, smoothT);
                    directionalLight.intensity = Mathf.Lerp(startLightIntensity, originalLightIntensity, smoothT);
                }
                
                // 앰비언트 색상 복원
                RenderSettings.ambientLight = Color.Lerp(startAmbientColor, originalAmbientColor, smoothT);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 원래 값으로 설정
        RenderSettings.skybox = originalSkybox;
        if (directionalLight != null)
        {
            directionalLight.color = originalLightColor;
            directionalLight.intensity = originalLightIntensity;
        }
        RenderSettings.ambientLight = originalAmbientColor;
        
        DynamicGI.UpdateEnvironment();
        
        isTransitioning = false;
    }
    
    private bool CanBlendSkyboxes(Material mat1, Material mat2)
    {
        if (mat1 == null || mat2 == null)
            return false;
            
        return mat1.shader == mat2.shader;
    }
    
    private bool IsCubemapSkybox(Material mat)
    {
        return mat.shader.name == "Skybox/Cubemap" || mat.HasProperty("_Tex");
    }
    
    private void BlendSkyboxProperties(Material from, Material to, float t)
    {
        // 큐브맵 스카이박스 블렌딩
        if (IsCubemapSkybox(from))
        {
            if (blendMaterial.HasProperty("_Blend"))
            {
                blendMaterial.SetFloat("_Blend", t);
            }
            
            // 색상 속성 블렌딩
            if (from.HasProperty("_Tint") && to.HasProperty("_Tint"))
            {
                blendMaterial.SetColor("_Tint", Color.Lerp(from.GetColor("_Tint"), to.GetColor("_Tint"), t));
            }
            
            // Exposure 블렌딩
            if (from.HasProperty("_Exposure") && to.HasProperty("_Exposure"))
            {
                blendMaterial.SetFloat("_Exposure", Mathf.Lerp(from.GetFloat("_Exposure"), to.GetFloat("_Exposure"), t));
            }
            
            // Rotation 블렌딩
            if (from.HasProperty("_Rotation") && to.HasProperty("_Rotation"))
            {
                blendMaterial.SetFloat("_Rotation", Mathf.Lerp(from.GetFloat("_Rotation"), to.GetFloat("_Rotation"), t));
            }
        }
        else
        {
            // Procedural 스카이박스 블렌딩
            string[] colorProperties = { "_Tint", "_GroundColor", "_SkyTint" };
            foreach (string prop in colorProperties)
            {
                if (from.HasProperty(prop) && to.HasProperty(prop))
                {
                    blendMaterial.SetColor(prop, Color.Lerp(from.GetColor(prop), to.GetColor(prop), t));
                }
            }
            
            string[] floatProperties = { "_Exposure", "_Rotation", "_AtmosphereThickness", "_SunSize", "_SunSizeConvergence" };
            foreach (string prop in floatProperties)
            {
                if (from.HasProperty(prop) && to.HasProperty(prop))
                {
                    blendMaterial.SetFloat(prop, Mathf.Lerp(from.GetFloat(prop), to.GetFloat(prop), t));
                }
            }
        }
        if (blendMaterial.HasProperty("_Tint"))
        {
            // 기본 Tint를 흰색으로 설정하여 원래 색상이 유지되도록 함
            blendMaterial.SetColor("_Tint", Color.white);
        }
        
    }
    
    void Update()
    {
        if(_isChanged)return;
        
        if (GameManager.Instance.ChangeSkybox)
        {
            StartTransition();
            _isChanged = true;
        }
        
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     StartTransition();
        // }
        //
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     RevertToOriginal();
        // }
    }
    
    public void TransitionToEnvironment(Material skybox, Color lightColor, float lightIntensity, Color ambientColor, float duration)
    {
        targetSkybox = skybox;
        targetLightColor = lightColor;
        targetLightIntensity = lightIntensity;
        targetAmbientColor = ambientColor;
        transitionDuration = duration;
        
        StartTransition();
    }
    
    public bool IsTransitioning
    {
        get { return isTransitioning; }
    }
}