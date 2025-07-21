using UnityEngine;
using System.Collections;
public class MoveJumpingCloud : MonoBehaviour, ITriggerableObstacle
{
    private bool canTrigger = true;  // 트리거 가능 여부를 확인하는 변수
    public float jumpForce = 18f;    // 점프 힘
    public float initialBoost = 8f;  // 초기 가속도 부스트
    
    // 이동 관련 변수
    public float moveDistance = 8f;  // 이동할 최대 거리
    public float moveSpeed = 6f;     // 이동 속도
    private Vector3 startPosition;   // 시작 위치
    private Vector3 endPosition;     // 종료 위치
    private float moveProgress = 0f; // 이동 진행도 (0~1)
    private bool movingForward = true; // 이동 방향 (true: 정방향, false: 역방향)
    private Vector3 initialPosition; // 초기 위치 저장 (리셋용)
    private bool isInitialized = false; // 초기화 여부

    void Awake()
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = new Vector3(122, transform.position.y, transform.position.z);
       
        isInitialized = true;
        
        // 초기 설정
        ResetCloudState();
        
    }
    
    // 구름의 상태를 초기화하는 메서드
    public void ResetCloudState()
    {
        
        // 시작 위치와 종료 위치 설정
        startPosition = initialPosition;
        endPosition = new Vector3(130, startPosition.y, startPosition.z);
        
        // 이동 상태 초기화
        moveProgress = 0f;
        movingForward = true;
    }

    // Update is called once per frame
    void Update()
    {
        // 왕복 이동 처리
        MoveBackAndForth();
    }
    
    // 왕복 이동 메서드
    private void MoveBackAndForth()
    {
        // 이동 진행도 업데이트
        if (movingForward)
        {
            moveProgress += moveSpeed * Time.deltaTime / moveDistance;
            if (moveProgress >= 1f)
            {
                moveProgress = 1f;
                movingForward = false;  // 방향 전환
            }
        }
        else
        {
            moveProgress -= moveSpeed * Time.deltaTime / moveDistance;
            if (moveProgress <= 0f)
            {
                moveProgress = 0f;
                movingForward = true;  // 방향 전환
            }
        }
        
        // 새 위치 계산하여 적용
        transform.position = Vector3.Lerp(startPosition, endPosition, moveProgress);
    }

    public void TriggerObstacle(GameObject player)
    {
        // 트리거 가능한 상태일 때만 실행
        if (canTrigger)
        {
            ImprovedSoundManager.Instance.PlaySound3D("JumpingCloud",transform.position);
            
            // 플레이어 리지드바디 컴포넌트 가져오기 
            Rigidbody rb = player.GetComponent<Rigidbody>();
            PlayerController playerController = player.GetComponent<PlayerController>();
            
            // 바로 점프 효과 적용
            ApplyJump(rb);
            
            if (playerController != null)
            {
                playerController.SetState(PlayerState.Jumping);
            }
            
            // 쿨다운 시작
            StartCoroutine(TriggerCooldown());
        }
    }
    
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
    
    // 쿨다운 코루틴
    private IEnumerator TriggerCooldown()
    {
        canTrigger = false;  // 트리거 비활성화
        yield return new WaitForSeconds(0.1f);  // 0.1초 대기
        canTrigger = true;  // 트리거 다시 활성화
    }
    
    // 오브젝트가 비활성화될 때 호출
    private void OnDisable()
    {
        if (isInitialized)
        {
            // 비활성화될 때 위치를 초기 위치로 설정 (실제 Transform 위치)
            transform.position = initialPosition;
        }
    }
    
    // 오브젝트가 활성화될 때 호출
    private void OnEnable()
    {
        if (isInitialized)
        {
            // 활성화될 때 상태 초기화
            ResetCloudState();
        }
    }
}