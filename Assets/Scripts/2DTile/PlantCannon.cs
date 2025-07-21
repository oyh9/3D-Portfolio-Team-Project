using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantCannon : MonoBehaviour
{
    [Header("탐지 설정")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float cooldownTime = 3f; // 캡처 쿨다운 시간
    [SerializeField] private float detectionCooldown = 0.5f; // 감지 함수 호출 간격
    
    [Header("발사 설정")]
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float minAngle = 0f;  // X+ 방향 (3시 방향)
    [SerializeField] private float maxAngle = 90f; // Y+ 방향 (12시 방향)
    [SerializeField] private float growDuration = 0.2f; // 플레이어가 원래 크기로 돌아오는 시간
    
    [Header("시각 효과")]
    [SerializeField] private bool showDirectionLine = true;
    [SerializeField] private float directionLineLength = 3f;
    [SerializeField] private Color directionLineColor = Color.red;
    [SerializeField] private float directionLineWidth = 0.1f;
    
    [Header("참조")]
    [SerializeField] private Animator deviceAnimator;
    
    // 내부 변수
    private GameObject player;
    private Rigidbody playerRigidbody;
    private PlayerController playerController;
    private SphereCollider playerCollider;
    private Vector3 playerOriginalScale;
    private float currentAngle = 45f;
    private bool hasPlayer = false;
    private bool isOnCooldown = false;
    private bool isDetecting = false; // 감지 프로세스 중복 방지
    private LineRenderer directionLine;
    private float nextDetectionTime = 0f; // 다음 감지 가능 시간
    
    private void Start()
    {
        // 피봇 포인트가 없으면 현재 오브젝트를 사용
        if (launchPoint == null)
            launchPoint = transform;
        
        // 방향 라인 설정
        if (showDirectionLine)
        {
            GameObject lineObj = new GameObject("DirectionLine");
            lineObj.transform.SetParent(transform);
            directionLine = lineObj.AddComponent<LineRenderer>();
            directionLine.startWidth = directionLineWidth;
            directionLine.endWidth = directionLineWidth;
            directionLine.material = new Material(Shader.Find("Sprites/Default"));
            directionLine.startColor = directionLineColor;
            directionLine.endColor = directionLineColor;
            directionLine.positionCount = 2;
            UpdateDirectionLine();
            directionLine.enabled = false; // 초기에는 비활성화
        }
    }
    
    private void Update()
    {
        // 플레이어 감지 (쿨다운 중이 아닐 때만, 감지 중복 방지)
        if (!hasPlayer && !isOnCooldown && !isDetecting && Time.time >= nextDetectionTime)
        {
            DetectPlayer();
        }
        else if (hasPlayer)
        {
            // 플레이어가 잡혔을 때 각도 제어
            HandleAngleControl();
            
            // 발사 방향 라인 업데이트
            if (showDirectionLine)
            {
                UpdateDirectionLine();
            }
            
            // 발사
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(LaunchPlayerCoroutine());
            }
        }
    }
    
    private void DetectPlayer()
    {
        // 감지 프로세스 중복 방지
        isDetecting = true;
        nextDetectionTime = Time.time + detectionCooldown;
        
        // 구체 형태의 레이캐스트로 플레이어 감지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        
        if (hitColliders.Length > 0 && player == null) // player가 null일 때만 처리
        {
            player = hitColliders[0].gameObject;
            
            // 플레이어가 이미 다른 장치에 의해 처리 중인지 확인
            if (player.transform.localScale.magnitude < 0.1f)
            {
                // 이미 다른 장치에 의해 처리 중이면 무시
                player = null;
                isDetecting = false;
                return;
            }
            
            // 플레이어 방향을 바라봄 (캡처 전에만)
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0; // Y축 회전만 고려
            transform.forward = directionToPlayer.normalized;
            
            // 플레이어 컴포넌트 참조 저장
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerController = player.GetComponent<PlayerController>(); // 실제 컨트롤러 스크립트 이름으로 수정 필요
            playerCollider = player.GetComponent<SphereCollider>();
            playerOriginalScale = player.transform.localScale;
            
            // 애니메이션 실행
            if (deviceAnimator != null)
            {
                Debug.Log("Eat 애니메이션 재생");
                deviceAnimator.SetTrigger("Eat");
            }
            
            // 플레이어 제어권 변경
            StartCoroutine(CapturePlayer());
        }
        else
        {
            // 감지된 플레이어가 없으면 즉시 감지 상태 해제
            isDetecting = false;
        }
    }
    
    private IEnumerator CapturePlayer()
    {
        
        
        yield return new WaitForSeconds(0.5f); // 애니메이션 시간에 맞게 조정
        ImprovedSoundManager.Instance.PlaySound3D("Eat", transform.position);
        // 플레이어가 여전히 유효한지 확인
        if (player == null)
        {
            isDetecting = false;
            yield break;
        }
    
        // 플레이어 컨트롤러 비활성화
        if (playerController != null)
            playerController.enabled = false;
        
        // 플레이어 콜라이더 비활성화
        if (playerCollider != null)
            playerCollider.enabled = false;
    
        // 리지드바디 비활성화 (추가)
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = true;
    
        // 플레이어를 발사 위치로 이동하고 보이지 않게 함
        player.transform.position = launchPoint.position;
        player.transform.localScale = Vector3.zero;
    
        // 플레이어가 월드 좌표계 기준 x+ 방향을 바라보도록 설정 (추가)
        player.transform.forward = Vector3.right;
    
        // 캡처 후 월드 기준 x+ 방향으로 회전 설정
        transform.rotation = Quaternion.LookRotation(Vector3.right);
    
        // 플레이어를 잡았음을 표시
        hasPlayer = true;
        isDetecting = false;
    
        // 방향 라인 활성화
        if (directionLine != null)
        {
            directionLine.enabled = true;
        }
    }
    
    private void HandleAngleControl()
    {
        // W와 S 키로 각도 조절
        if (Input.GetKey(KeyCode.W))
        {
            currentAngle = Mathf.Min(currentAngle + Time.deltaTime * 50f, maxAngle);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentAngle = Mathf.Max(currentAngle - Time.deltaTime * 50f, minAngle);
        }
    }
    
    private void UpdateDirectionLine()
    {
        if (directionLine != null)
        {
            Vector3 direction = GetLaunchDirection();
            directionLine.SetPosition(0, launchPoint.position);
            directionLine.SetPosition(1, launchPoint.position + direction * directionLineLength);
        }
    }
    
    private Vector3 GetLaunchDirection()
    {
        // 순수 월드 좌표계 기준으로 x+ 방향에서 현재 각도만큼 회전
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
    
        // 월드 좌표 기준 (0도는 오른쪽(x+), 90도는 위쪽(y+)) - XY 평면에서만
        Vector3 direction = new Vector3(
            Mathf.Cos(angleInRadians), 
            Mathf.Sin(angleInRadians), 
            0  // Z 방향은 항상 0
        );
    
        return direction.normalized;
    }
    
    private IEnumerator LaunchPlayerCoroutine()
    {
        deviceAnimator.SetTrigger("Eat");
        ImprovedSoundManager.Instance.PlaySound3D("Launch", transform.position);
        // 플레이어가 여전히 유효한지 확인
        if (player == null)
        {
            hasPlayer = false;
            yield break;
        }
        
        // 플레이어가 성장하는 애니메이션
        float elapsedTime = 0;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = playerOriginalScale;
        
        // 월드 좌표계 기준으로 발사 방향 계산 및 저장
        Vector3 launchDirection = GetLaunchDirection();
        Debug.Log("발사 방향 (XY 평면만): " + launchDirection);
        
        // 방향 라인 비활성화
        if (directionLine != null)
        {
            directionLine.enabled = false;
        }
        
        // 플레이어 콜라이더 활성화
        if (playerCollider != null)
            playerCollider.enabled = true;
        
        if (playerRigidbody != null)
            playerRigidbody.isKinematic = false;
        
        // 크기 애니메이션 실행
        while (elapsedTime < growDuration)
        {
            // 플레이어가 여전히 유효한지 확인
            if (player == null)
            {
                hasPlayer = false;
                yield break;
            }
            
            // 시간 측정
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / growDuration;
            
            // 이지 함수로 부드러운 크기 변화
            float smoothT = Mathf.SmoothStep(0, 1, t);
            player.transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            
            // 다음 프레임까지 대기
            yield return null;
        }
        
        // 플레이어가 여전히 유효한지 확인
        if (player == null)
        {
            hasPlayer = false;
            yield break;
        }
        
        // 크기 애니메이션이 끝난 후 확실히 원래 크기로 설정
        player.transform.localScale = targetScale;
        
        // 발사 직전에 플레이어 컨트롤러 활성화
        if (playerController != null)
            playerController.enabled = true;
        
        
        
        // LaunchPlayerCoroutine 메서드 내 발사 부분을 수정
        if (playerRigidbody != null)
        {
            // 각도에 따른 힘 계산
            float angleFactor = Mathf.Clamp01(currentAngle / 90f); // 0에서 1 사이 값
    
            // 수평 방향에서 더 강한 힘을 위한 계산
            // 각도가 0에 가까울수록 수평 방향 힘 증폭
            float horizontalMultiplier = 1.5f + (1.0f - angleFactor) * 1.5f; // 0도에서 3.0, 90도에서 1.5
            float verticalMultiplier = 0.8f + angleFactor * 0.7f;           // 0도에서 0.8, 90도에서 1.5
    
            // 힘 적용 - x 방향으로 더 강한 힘
            Vector3 force = new Vector3(
                launchDirection.x * launchForce * horizontalMultiplier,
                launchDirection.y * launchForce * verticalMultiplier,
                0  // Z 방향으로는 힘을 가하지 않음
            );
    
            playerRigidbody.AddForce(force, ForceMode.Impulse);
    
            // 플레이어 상태 업데이트
            playerController.fireSpeed = launchDirection.x * launchForce * horizontalMultiplier;
            playerController.SetState(PlayerState.Fire);
    
            // 디버그 정보
            Debug.Log("발사 각도: " + currentAngle + "도");
            Debug.Log("수평 배율: " + horizontalMultiplier + ", 수직 배율: " + verticalMultiplier);
            Debug.Log("최종 발사 힘: " + force);
            Debug.DrawRay(player.transform.position, force * 0.1f, Color.red, 3f);
    
            // 발사 후 속도 확인
            StartCoroutine(CheckVelocityAfterLaunch());
        }
        
        // 여기에 플레이어 입력 막기 코드를 추가하세요
        // 예: StartCoroutine(playerController.GetComponent<YourPlayerController>().DisableInputUntilLanding());
        
        // 발사 후 쿨다운 시작
        StartCoroutine(StartCooldown());
        
        // 상태 초기화
        hasPlayer = false;
        player = null;
    }
    
    // CheckVelocityAfterLaunch 코루틴을 수정
    private IEnumerator CheckVelocityAfterLaunch()
    {
        // 플레이어가 발사된 직후의 속도를 확인 (디버그용)
        yield return new WaitForFixedUpdate();
    
        // 초기 속도 확인
        if (playerRigidbody != null)
        {
            Vector3 velocity = playerRigidbody.linearVelocity;
            Debug.Log("발사 직후 플레이어 X축 속도: " + velocity.x);
        }
    
        // 3초 동안 0.5초 간격으로 X축 속도 모니터링
        float monitorDuration = 3f;
        float checkInterval = 0.5f;
        float elapsedTime = 0f;
    
        while (elapsedTime < monitorDuration && playerRigidbody != null)
        {
            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;
        
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            Debug.Log($"발사 후 {elapsedTime:F1}초 - 플레이어 X축 속도: {currentVelocity.x:F2}");
        }
    }
    
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        
        // 쿨다운 시각 효과 (선택사항)
        // 예: 색상 변경 또는 파티클 효과
        
        yield return new WaitForSeconds(cooldownTime);
        
        isOnCooldown = false;
    }
    
}