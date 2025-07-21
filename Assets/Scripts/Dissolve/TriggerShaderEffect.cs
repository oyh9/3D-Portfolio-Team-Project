using System.Collections;
using UnityEngine;

public class TriggerShaderEffect : MonoBehaviour
{
    // 트리거 감지를 위한 태그
    public string playerTag = "Player";
    
    // 비활성화할 박스 콜라이더
    public BoxCollider targetCollider;
    
    // 쉐이더를 가진 렌더러
    public Renderer targetRenderer;
    
    // 쉐이더 프로퍼티 이름
    private readonly string _shaderPropertyName = "_Split_Value";
    
    // 시작 값
    public float startValue = 0.25f;
    
    // 종료 값
    public float endValue = -100f;
    
    // 애니메이션 시간 (초)
    public float duration = 1.0f;
    
    // 이펙트가 이미 트리거 되었는지 확인하는 플래그
    private bool isTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        // 아직 트리거되지 않았고 플레이어 태그를 가진 오브젝트와 충돌했는지 확인
        if (!isTriggered && other.CompareTag(playerTag))
        {
            isTriggered = true;
            ImprovedSoundManager.Instance.PlaySound2D("Split");
            // 박스 콜라이더 비활성화
            if (targetCollider != null)
            {
                targetCollider.enabled = false;
            }
            
            
            // 쉐이더 프로퍼티 애니메이션 코루틴 시작
            StartCoroutine(AnimateShaderProperty());
        }
    }
    
    private IEnumerator AnimateShaderProperty()
    {
        if (targetRenderer == null)
        {
            Debug.LogError("타겟 렌더러가 설정되지 않았습니다.");
            yield break;
        }
        
        Material material = targetRenderer.material;
        float currentTime = 0f;

        if (material.HasProperty(_shaderPropertyName))
        {
            Debug.Log(_shaderPropertyName);
        }
        // 시작 값으로 초기화
        material.SetFloat(_shaderPropertyName, startValue);
        
        // 값을 점차 변경
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration; // 0과 1 사이의 시간 비율
            
            // 현재 값 계산 (선형 보간)
            float currentValue = Mathf.Lerp(startValue, endValue, t);
            
            // 쉐이더 프로퍼티 업데이트
            material.SetFloat(_shaderPropertyName, currentValue);
            yield return null; // 다음 프레임까지 대기
        }
        
        // 마지막 값 설정
        material.SetFloat(_shaderPropertyName, endValue);
    }
}