using UnityEngine;
using System.Collections;

public class JumpingDisappearingCloud : MonoBehaviour, ITriggerableObstacle
{
    private bool canTrigger = true;  // 트리거 가능 여부를 확인하는 변수
    private bool isDisappearing = false;  // 사라지는 중인지 확인하는 변수
    
    public float jumpForce = 18f;    // 점프 힘
    public float initialBoost = 8f;  // 초기 가속도 부스트
    
    // 펄스 효과를 위한 변수들
    public float pulseScale = 1.2f;    // 펄스 효과 최대 스케일
    public float pulseDuration = 3f;  // 펄스 효과 지속 시간
    public int pulseCount = 8;  // 펄스 횟수
    
    // 사라짐 및 재생성 관련 변수
    public float disappearDelay = 0.2f;  // 펄스 후 사라지기까지 지연 시간 (짧게 설정)
    public float disappearSpeed = 0.15f; // 사라지는 속도 (작을수록 빠름)
    public float respawnDelay = 3.0f;    // 사라진 후 다시 나타나기까지 시간
    public float respawnAnimTime = 0.5f; // 재생성 애니메이션 시간
    
    private Renderer cloudRenderer;    // 구름의 렌더러
    private Collider cloudCollider;    // 구름의 콜라이더
    private Vector3 originalScale;     // 원래 스케일
    private Vector3 initialPosition;   // 초기 위치
    
    private void Start()
    {
        // 컴포넌트 가져오기
        //cloudRenderer = GetComponent<Renderer>();
        cloudCollider = GetComponent<Collider>();
        
        // 초기 스케일 저장
        originalScale = transform.localScale;
        
        // 초기 위치 저장
        initialPosition = transform.position;
        isDisappearing = false;
    }
    
    public void TriggerObstacle(GameObject player)
    {
        // 트리거 가능하고 사라지는 중이 아닐 때만 실행
        if (canTrigger)
        {
            ImprovedSoundManager.Instance.PlaySound3D("JumpingCloud",transform.position);

            
            // 플레이어 리지드바디 컴포넌트 가져오기 
            Rigidbody rb = player.GetComponent<Rigidbody>();
            PlayerController playerController = player.GetComponent<PlayerController>();
            
            // 바로 점프 효과 적용
            ApplyJump(rb);
            
            playerController.SetState(PlayerState.Jumping);
            
            // 아직 사라지는 중이 아닐 때만 사라짐 효과 시작
            if (!isDisappearing)
            {
                Debug.Log("Disappearing");
                StartCoroutine(DisappearAndRespawn());
            }
            
            // 쿨다운 시작
            StartCoroutine(TriggerCooldown());
        }
    }
    
    // 직접 점프 로직 구현
    private void ApplyJump(Rigidbody rb)
    {
        if (rb != null)
        {
            // 현재 속도 가져오기
            Vector3 currentVelocity = rb.linearVelocity;
            
            // y 속도를 0으로 초기화 (일관된 점프 높이를 위해)
            currentVelocity.y = 0f;
            rb.linearVelocity = currentVelocity;
            
            // 즉각적인 초기 속도 적용
            currentVelocity.y = initialBoost;
            rb.linearVelocity = currentVelocity;
            
            // 추가로 위쪽으로 강한 힘을 가해 탄력있게 점프
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // 점프 효과음이나 파티클 효과 등을 여기에 추가할 수 있습니다
        }
    }
    
    // 펄스 효과 코루틴
    private IEnumerator PulseEffect()
    {
        float singlePulseDuration = pulseDuration / (pulseCount * 2);
        
        for (int i = 0; i < pulseCount; i++)
        {
            // 스케일 확대 (펄스 ON)
            float elapsedTime = 0;
            Vector3 startScale = originalScale;
            Vector3 targetScale = originalScale * pulseScale;
            
            while (elapsedTime < singlePulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / singlePulseDuration);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            // 스케일 복원 (펄스 OFF)
            elapsedTime = 0;
            
            while (elapsedTime < singlePulseDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / singlePulseDuration);
                transform.localScale = Vector3.Lerp(targetScale, startScale, t);
                yield return null;
            }
        }
        
        // 원래 스케일로 확실히 복원
        transform.localScale = originalScale;
    }
    
    // 사라짐 효과 코루틴
    private IEnumerator DisappearEffect()
    {
        // 빠르게 사라지는 효과
        float elapsedTime = 0;
        Vector3 startScale = originalScale;
        Vector3 endScale = Vector3.zero;
        
        // 사라지는 속도를 disappearSpeed로 조절 (작을수록 빠름)
        while (elapsedTime < disappearSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / disappearSpeed);
            
            // 빠르게 사라지는 효과를 위한 이징 함수 적용
            float easeOutT = 1.0f - Mathf.Pow(1.0f - t, 3); // 급격한 감소 효과
            transform.localScale = Vector3.Lerp(startScale, endScale, easeOutT);
            
            yield return null;
        }
        
        // 완전히 사라짐 확인
        transform.localScale = Vector3.zero;
        //cloudRenderer.enabled = false;
        cloudCollider.enabled = false;
    }
    
    // 사라짐 및 재생성 코루틴
    private IEnumerator DisappearAndRespawn()
    {
        isDisappearing = true;
        
        // 펄스 효과 시작 - 이 효과가 완전히 끝날 때까지 기다림
        yield return StartCoroutine(PulseEffect());
        
        // 짧은 지연 시간
        yield return new WaitForSeconds(disappearDelay);
        
        // 빠르게 사라지는 효과 실행
        yield return StartCoroutine(DisappearEffect());
        
        // 지정된 시간만큼 대기 후 재생성
        yield return new WaitForSeconds(respawnDelay);
        
        // 위치 복원
        transform.position = initialPosition;
        
        // 렌더러 활성화하되 스케일은 0에서 시작
        //cloudRenderer.enabled = true;
        transform.localScale = Vector3.zero;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // 서서히 원래 크기로 복원
        float elapsedTime = 0;
        
        while (elapsedTime < respawnAnimTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / respawnAnimTime);
            
            // 스케일 점점 키우기 (부드러운 이징 적용)
            float smoothT = Mathf.SmoothStep(0, 1, t);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, smoothT);
            
            yield return null;
        }
        
        // 스케일 정확히 복원
        transform.localScale = originalScale;
        
        // 콜라이더 활성화
        cloudCollider.enabled = true;
        
        // 사라짐 상태 해제
        isDisappearing = false;
    }
    
    // 쿨다운 코루틴
    private IEnumerator TriggerCooldown()
    {
        canTrigger = false;  // 트리거 비활성화
        yield return new WaitForSeconds(0.1f);  // 0.1초 대기
        canTrigger = true;   // 트리거 다시 활성화
    }
    
    // 게임 매니저나 다른 스크립트에서 즉시 재생성 가능
    public void ForceRespawn()
    {
        if (isDisappearing)
        {
            StopAllCoroutines();
            transform.position = initialPosition;
            transform.localScale = originalScale;
            
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            cloudRenderer.enabled = true;
            cloudCollider.enabled = true;
            
            isDisappearing = false;
            canTrigger = true;
        }
    }
    
    // 효과 설정을 게임 도중 변경할 수 있는 메서드
    public void SetEffectSettings(float pulseMagnitude, int count, float duration, float disappearRate)
    {
        pulseScale = pulseMagnitude;
        pulseCount = count;
        pulseDuration = duration;
        disappearSpeed = disappearRate;
    }
}