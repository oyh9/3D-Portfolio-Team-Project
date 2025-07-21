using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    // 디졸브 속성의 이름 (셰이더에 정의된 이름과 일치해야 함)
    private readonly string dissolvePropertyName = "_DissolveHeight";
    
    // 디졸브 애니메이션 속도
    public float dissolveSpeed = 1.0f;
    
    // 여러 대상 오브젝트를 저장할 리스트
    public List<GameObject> targetObjects = new List<GameObject>(); // Inspector에서 할당

    // 디졸브 애니메이션이 1에서 0으로 변하는 코루틴
    public IEnumerator DissolveFromOneToZero(GameObject targetObject, float duration)
    {
        if (targetObject == null)
            yield break;
            
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
        
        // 모든 렌더러의 디졸브 값을 1로 설정 (완전히 디졸브된 상태)
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty(dissolvePropertyName))
                {
                    material.SetFloat(dissolvePropertyName, 1.0f);
                }
            }
        }
        
        // 시간에 따라 1에서 0으로 디졸브 값 감소
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            // 1에서 0으로 변하는 값 계산
            float dissolveValue = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            
            // 모든 렌더러에 디졸브 값 적용
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty(dissolvePropertyName))
                    {
                        material.SetFloat(dissolvePropertyName, dissolveValue);
                    }
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;  // 다음 프레임까지 대기
        }
        
        // 최종적으로 디졸브 값을 0으로 설정 (완전히 보이는 상태)
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty(dissolvePropertyName))
                {
                    material.SetFloat(dissolvePropertyName, 0f);
                }
            }
        }
        bool isPlayerLayer = targetObject.layer == LayerMask.NameToLayer("Player");
    
        if (isPlayerLayer)
        {
            PlayerRespawnManager.Instance.Respawn();
            //UICircleTransition.Instance.CircleFadeIn();
        }
        
        //PlayerRespawnManager.Instance.Respawn();
        //UICircleTransition.Instance.CircleFadeIn();
        // 디졸브 효과가 끝난 후 오브젝트를 비활성화
        targetObject.SetActive(false);
    }
    
    // 이 함수를 호출하여 단일 오브젝트에 애니메이션 시작
    public void StartDissolveAnimation(GameObject targetObject, float duration = 2.0f)
    {
        StartCoroutine(DissolveFromOneToZero(targetObject, duration));
    }
    
    // 모든 타겟 오브젝트에 디졸브 애니메이션 적용
    public void ApplyDissolveToAllTargets(float duration = 2.0f)
    {
        ImprovedSoundManager.Instance.PlaySound2D("Dissolve");
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                StartCoroutine(DissolveFromOneToZero(obj, duration));
            }
        }
    }
    
    void Start()
    {
        // 필요시 시작할 때 실행
        // ApplyDissolveToAllTargets();
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.N))
        // {
        //     ApplyDissolveToAllTargets();
        // }
    }
    
    // 버튼 이벤트 등에서 호출할 수 있는 퍼블릭 메서드
    public void ApplyDissolveToObject(GameObject obj)
    {
        StartDissolveAnimation(obj);
    }
}