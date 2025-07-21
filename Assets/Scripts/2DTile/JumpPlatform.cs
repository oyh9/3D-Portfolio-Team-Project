using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPlatform : MonoBehaviour, ITriggerableObstacle
{
    [Tooltip("점프 높이")]
    [SerializeField] private float jumpHeight = 15.0f;
    
    [Tooltip("X축 이동 거리")]
    [SerializeField] private float jumpDistanceX = 4.0f;
    
    [Tooltip("Z축 이동 거리")]
    [SerializeField] private float jumpDistanceZ = 0.0f;
    
    [Tooltip("점프 속도 (낮을수록 점프 시간이 길어짐)")]
    [SerializeField] private float jumpSpeed = 8.0f;
    
    [Tooltip("점프 시작 시 초기 상승 강도")]
    [SerializeField] private float initialUpwardForce = 1.5f;
    
    [Tooltip("점프 곡선 제어점 (0.5~2 사이 권장, 높을수록 더 둥글게)")]
    [SerializeField] private float curveControl = 1.2f;
    
    // [Tooltip("점프 시 효과음")]
    // [SerializeField] private AudioClip jumpSound;
    //
    // [Tooltip("시각 효과 (파티클 등)")]
    // [SerializeField] private GameObject visualEffect;
    
    // 발판 활성화 상태
    private bool isActive = true;
    private Vector3 playerLastDelta;
    
    // 한번 밟으면 비활성화할지 여부
    [SerializeField] private bool deactivateAfterUse = false;
    
    // 다시 활성화되는 시간 (0이면 재활성화 안됨)
    [SerializeField] private float reactivateTime = 0f;
    private CameraController cameraController;
    
    
    private Vector3[] precalculatedPath;
    private int pathResolution = 120; // 경로의 세부 수준, 높을수록 부드럽지만 메모리 사용 증가

    
    private void Awake()
    {
        // 씬에서 카메라 컨트롤러 찾기
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            // 메인 카메라에 없다면 모든 카메라에서 검색
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                cameraController = cam.GetComponent<CameraController>();
                if (cameraController != null) break;
            }
        }
        
        PrecalculateJumpPath();
    }
    
    // 플레이어가 발판을 밟았을 때 호출되는 메서드
    public void TriggerObstacle(GameObject player)
    {
        if (!isActive) return;
        
        // Rigidbody 컴포넌트 확인
        Rigidbody rb = player.GetComponent<Rigidbody>();
        ImprovedSoundManager.Instance.PlaySound3D("SuperJump",transform.position);
        
        if (rb != null)
        {
            GameManager.Instance.ChangeSkybox = true;
            DialogueManager.Instance.TutorialDialogue(1);
            // 점프 코루틴 시작 - 플레이어 방향 사용 안 함
            StartCoroutine(PerformJump(player, rb));
            
            // 효과음 재생
            // if (jumpSound != null)
            // {
            //     AudioSource.PlayClipAtPoint(jumpSound, transform.position);
            // }
            //
            // // 시각 효과 표시
            // if (visualEffect != null)
            // {
            //     Instantiate(visualEffect, transform.position, Quaternion.identity);
            // }
            
            // 사용 후 비활성화 옵션이 켜져 있으면
            if (deactivateAfterUse)
            {
                isActive = false;
                
                // 시각적으로 비활성화 표시 (색상 변경 등)
                UpdateVisualState();
                
                // 재활성화 시간이 설정되어 있으면 타이머 시작
                if (reactivateTime > 0)
                {
                    StartCoroutine(ReactivateAfterDelay());
                }
            }
        }
    }
    private void PrecalculateJumpPath()
    {
        precalculatedPath = new Vector3[pathResolution];
        
        // 시작점과 목표점 (상대 위치로 계산)
        Vector3 startPos = Vector3.zero;
        Vector3 targetPos = new Vector3(jumpDistanceX, jumpHeight, jumpDistanceZ);
        
        // 제어점 설정
        Vector3 p0 = startPos;
        Vector3 p1 = startPos + new Vector3(
            jumpDistanceX * 0.2f,
            jumpHeight * initialUpwardForce,
            jumpDistanceZ * 0.2f
        );
        Vector3 p2 = startPos + new Vector3(
            jumpDistanceX * 0.8f,
            jumpHeight * curveControl,
            jumpDistanceZ * 0.8f
        );
        Vector3 p3 = targetPos;
        
        // 경로 각 지점 계산
        for (int i = 0; i < pathResolution; i++)
        {
            float t = (float)i / (pathResolution - 1);
            float smoothT = Mathf.SmoothStep(0, 1, t);
            precalculatedPath[i] = CalculateCubicBezierPoint(smoothT, p0, p1, p2, p3);
        }
    }
    
    
    private IEnumerator PerformJump(GameObject player, Rigidbody rb)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.SetState(PlayerState.Jumping);
        
        // 물리 제약 임시 저장
        RigidbodyConstraints originalConstraints = rb.constraints;
        
        // 시작 위치
        Vector3 startPosition = player.transform.position;
        
        // 카메라 관련 정보
        Vector3 cameraStartPosition = cameraController.transform.position;
        bool is2DMode = cameraController.Is2DMode();
        float originalFixed2DYPosition = 0f;
        
        if (is2DMode)
        {
            originalFixed2DYPosition = cameraController.GetFixed2DYPosition();
        }
        
        // 점프 시작 설정
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = false;
        
        // 시간 설정
        float totalTime = Vector3.Distance(startPosition, startPosition + new Vector3(jumpDistanceX, jumpHeight, jumpDistanceZ)) / jumpSpeed;
        float elapsedTime = 0f;
        Vector3 previousPosition = startPosition;
        
        // 미리 계산된 경로를 따라 이동
        while (elapsedTime < totalTime)
        {
            float t = elapsedTime / totalTime;
            int index = Mathf.Clamp(Mathf.FloorToInt(t * (pathResolution - 1)), 0, pathResolution - 1);
            
            // 현재 위치 계산 (시작 위치 + 미리 계산된 상대 경로)
            Vector3 newPosition = startPosition + precalculatedPath[index];
            
            // 플레이어 위치 업데이트
            player.transform.position = newPosition;
            
            // 이동 델타 계산 (현재 프레임의 이동량)
            Vector3 playerDelta = newPosition - previousPosition;
            
            if (is2DMode)
            {
                // 2D 모드 카메라 업데이트
                Vector3 newCameraPosition = cameraController.transform.position;
                newCameraPosition.y += playerDelta.y;
                cameraController.transform.position = newCameraPosition;
                
                // fixed2DYPosition 업데이트
                cameraController.AdjustFixed2DYPosition(playerDelta.y);
            }
            
            // 이전 위치 업데이트
            previousPosition = newPosition;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 최종 위치 설정
        player.transform.position = startPosition + precalculatedPath[pathResolution - 1];
        
        // 물리 상태 복원
        rb.constraints = originalConstraints;
        GameManager.Instance.nextPlatform = true;
        playerController.nextPlatform = true;
    }
    
    // 3차 베지어 곡선 계산 함수
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 p = uuu * p0; // (1-t)^3 * P0
        p += 3 * uu * t * p1; // 3(1-t)^2 * t * P1
        p += 3 * u * tt * p2; // 3(1-t) * t^2 * P2
        p += ttt * p3; // t^3 * P3
        
        return p;
    }
    
    // 3차 베지어 곡선의 접선(미분) 계산 - 점프 방향 벡터용
    private Vector3 CalculateCubicBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float uu = u * u;
        float tt = t * t;
        
        // 3차 베지어 곡선 미분
        Vector3 tangent = 3 * uu * (p1 - p0);
        tangent += 6 * u * t * (p2 - p1);
        tangent += 3 * tt * (p3 - p2);
        
        return tangent;
    }
    
    // 재활성화 코루틴
    private IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(reactivateTime);
        isActive = true;
        UpdateVisualState();
    }
    
    // 시각적 상태 업데이트
    private void UpdateVisualState()
    {
        // 발판의 렌더러 컴포넌트 가져오기
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // 활성/비활성 상태에 따라 색상 변경
            renderer.material.color = isActive ? Color.green : Color.gray;
        }
    }
    
    // 시작할 때 시각적 상태 설정
    private void Start()
    {
        UpdateVisualState();
    }
    
}