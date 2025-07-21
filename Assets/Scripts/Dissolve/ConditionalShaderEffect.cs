using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalShaderEffect : MonoBehaviour
{
    // 인스펙터에서 직접 게임오브젝트들을 드래그 앤 드롭으로 추가할 수 있는 리스트
    [SerializeField] public List<GameObject> targetObjects = new List<GameObject>();

    // 자동으로 할당될 렌더러 리스트
    [SerializeField] public List<Renderer> targetRenderers = new List<Renderer>();

    // 각 대상에 쉐이더 효과를 적용할지 여부
    [SerializeField] public List<bool> applyShaderEffects = new List<bool>();

    // 쉐이더 프로퍼티 이름
    public string shaderPropertyName = "_Split_Value";

    // 시작 값
    public float startValue = -50f;

    // 종료 값
    public float endValue = 100f;

    // 애니메이션 시간 (초)
    public float duration = 1.0f;

    // 이펙트가 이미 트리거 되었는지 확인하는 플래그
    private bool isTriggered = false;

    // 조건 확인을 위한 변수들 (필요에 따라 수정)
    public bool useKeyPress = true;
    public bool useDistanceCheck = false;
    public Transform playerTransform;
    public float activationDistance = 3.0f;

    // 인스펙터에서 값이 변경될 때마다 호출되는 함수
    private void OnValidate()
    {
        // targetObjects 리스트 변경 시 관련 리스트 업데이트
        UpdateRenderersList();
    }

    // targetObjects 리스트를 기반으로 다른 리스트들 업데이트
    private void UpdateRenderersList()
    {
        // targetRenderers와 applyShaderEffects 리스트 크기를 targetObjects 크기에 맞춤
        while (targetRenderers.Count > targetObjects.Count)
        {
            targetRenderers.RemoveAt(targetRenderers.Count - 1);
            applyShaderEffects.RemoveAt(applyShaderEffects.Count - 1);
        }

        // 각 게임오브젝트에 대해 처리
        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject obj = targetObjects[i];

            // 해당 인덱스의 렌더러와 적용 여부가 리스트에 없으면 새로 추가
            if (i >= targetRenderers.Count)
            {
                targetRenderers.Add(null);
                applyShaderEffects.Add(true);
            }

            // 오브젝트가 유효한 경우에만 렌더러 자동 할당
            if (obj != null)
            {
                // 렌더러 자동 할당 (해당 오브젝트의 렌더러 컴포넌트 가져오기)
                targetRenderers[i] = obj.GetComponent<Renderer>();

                // 렌더러가 없을 경우 자식에서 찾아보기
                if (targetRenderers[i] == null)
                {
                    targetRenderers[i] = obj.GetComponentInChildren<Renderer>();
                }
            }
        }
    }

    private void Start()
    {
        // 시작할 때 모든 타겟 오브젝트가 비활성화되어 있는지 확인
        for (int i = 0; i < targetObjects.Count; i++)
        {
            if (targetObjects[i] != null && targetObjects[i].activeSelf)
            {
                targetObjects[i].SetActive(false);
            }
        }
    }

    private void Update()
    {
        
    }

    // 각 오브젝트 사이의 지연 시간 (초)
    public float delayBetweenObjects = 0.5f;

    // 다른 스크립트에서 호출할 수 있는 공개 메서드
    public void TriggerEffect()
    {
        if (!isTriggered)
        {
            isTriggered = true;
            // 순차적으로 오브젝트 활성화를 위한 코루틴 시작
            StartCoroutine(SequentiallyActivateObjects());
        }
    }

    // 오브젝트를 순차적으로 활성화하는 코루틴
    private IEnumerator SequentiallyActivateObjects()
    {
        if (GameManager.Instance.currentSceneName == "Tutorial")
        {
            DialogueManager.Instance.TutorialDialogue(2);
            
        }
        // 모든 타겟 오브젝트를 하나씩 순차적으로 활성화
        for (int i = 0; i < targetObjects.Count; i++)
        {
            ImprovedSoundManager.Instance.PlaySound2D("Split");
            // 오브젝트 활성화
            if (targetObjects[i] != null)
            {
                targetObjects[i].SetActive(true);

                // 쉐이더 효과가 필요한 렌더러에만 애니메이션 적용
                if (i < applyShaderEffects.Count && i < targetRenderers.Count &&
                    applyShaderEffects[i] && targetRenderers[i] != null)
                {
                    StartCoroutine(AnimateShaderProperty(targetRenderers[i]));
                }

                // 다음 오브젝트 활성화 전에 지정된 시간만큼 대기
                yield return new WaitForSeconds(delayBetweenObjects);
            }
        }
        DialogueManager.Instance.TutorialDialogue(3);
        
    }

    private IEnumerator AnimateShaderProperty(Renderer renderer)
    {
        if (renderer == null)
        {
            yield break;
        }

        Material material = renderer.material;
        float currentTime = 0f;

        // 쉐이더 프로퍼티 존재 확인
        if (material.HasProperty(shaderPropertyName))
        {
            Debug.Log("쉐이더 프로퍼티 애니메이션 시작: " + shaderPropertyName);
        }
        else
        {
            Debug.LogError("쉐이더에 " + shaderPropertyName + " 프로퍼티가 없습니다: " + renderer.gameObject.name);
            yield break;
        }

        // 시작 값으로 초기화
        material.SetFloat(shaderPropertyName, startValue);

        // 값을 점차 변경
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration; // 0과 1 사이의 시간 비율

            // 현재 값 계산 (선형 보간)
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            // 쉐이더 프로퍼티 업데이트
            material.SetFloat(shaderPropertyName, currentValue);
            yield return null; // 다음 프레임까지 대기
        }

        // 마지막 값 설정
        material.SetFloat(shaderPropertyName, endValue);
    }
}