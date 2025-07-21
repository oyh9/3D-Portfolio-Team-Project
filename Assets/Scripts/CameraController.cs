using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] public Transform target;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private LayerMask obstacleLayerMask;
    
    // 2D 모드 관련 변수 추가
    [SerializeField] private bool is2DMode = false;
    [SerializeField] private float fixed2DYPosition = 5f;
    [SerializeField] private float fixed2DZDistance = -7f;
    
    // 단순 경계 카메라 설정
    [Header("2D 단순 경계 카메라 설정")]
    [SerializeField] private float maxHorizontalDistance = 4f; // 카메라 중앙에서 캐릭터가 벗어날 수 있는 최대 X축 거리
    [SerializeField] private bool boundCamera = false;        // 카메라 이동 제한 여부
    [SerializeField] private float boundMinX = -100f;         // 카메라 최소 X 위치
    [SerializeField] private float boundMaxX = 100f;          // 카메라 최대 X 위치
    [SerializeField] private bool showBounds = true;          // 디버깅용 - 경계 시각화
    
    [SerializeField] private bool isSubCamera=false;
    private float _azimuthAngle;
    private float _polarAngle = 60f;
    
    // 탭 카메라 전환 관련 변수 추가 (2D 모드 전용)
    [Header("2D 모드 탭 카메라 전환 설정")]
    [SerializeField] private float tabCameraXOffset = -3f;    // 탭 눌렀을 때 카메라 X축 오프셋
    [SerializeField] private float boundaryOffsetInTabMode = -2f; // 탭 모드에서 경계 오프셋
    [SerializeField] private float cameraTransitionSpeed = 5f; // 카메라 전환 속도
    private bool isInTabMode = false;                         // 현재 탭 모드 상태
    private bool isTransitioning = false;                     // 전환 중인지 여부
    
     
    
    
    // 원래 경계값 저장 (탭 모드로 전환 시 필요)
    private float originalBoundMinX;
    private float originalBoundMaxX;
    
    private void Start()
    {
        if (target == null) return;
        is2DMode = GameManager.Instance.Is2DMode();
        
        // 원래 경계값 저장
        originalBoundMinX = boundMinX;
        originalBoundMaxX = boundMaxX;
        
        if (is2DMode)
        {
            Set2DPosition();
        }
        else
        {
            Set3DPosition();
        }
    }

    private void Update()
    {
        // 탭 키 입력 처리 (2D 모드에서만)
        if (Input.GetKeyDown(KeyCode.Tab) && !isTransitioning && is2DMode)
        {
            ToggleTabCameraMode();
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if(!GameManager.canPlayerMove) return;
        
        // 전환 중이면 카메라 업데이트 생략
        if (isTransitioning) return;
        
        if (is2DMode)
        {
            Update2DCamera();
        }
        else
        {
            Update3DCamera();
        }
    }

    // 탭 카메라 모드 토글 (2D 모드에서만 동작)
    private void ToggleTabCameraMode()
    {
        if (!is2DMode) return; // 3D 모드에서는 동작하지 않음
        
        isInTabMode = !isInTabMode;
        
        if (isInTabMode)
        {
            // 탭 모드로 전환
            // 경계값 조정
            if (boundCamera)
            {
                // 원래 경계값 저장
                originalBoundMinX = boundMinX;
                originalBoundMaxX = boundMaxX;
                
                // 경계 이동 (X축으로 boundaryOffsetInTabMode만큼)
                boundMinX += boundaryOffsetInTabMode;
                boundMaxX += boundaryOffsetInTabMode;
            }
            
            StartCoroutine(TransitionToTabMode());
        }
        else
        {
            // 원래 모드로 복귀
            // 경계값 복원
            if (boundCamera)
            {
                boundMinX = originalBoundMinX;
                boundMaxX = originalBoundMaxX;
            }
            
            StartCoroutine(TransitionToOriginalMode());
        }
    }

    // 탭 모드로 전환하는 코루틴
    private IEnumerator TransitionToTabMode()
    {
        isTransitioning = true;
        
        // 목표 위치 계산 (타겟의 X 좌표에서 오프셋만큼 이동)
        Vector3 targetPosition = new Vector3(
            target.position.x + tabCameraXOffset, 
            fixed2DYPosition,
            fixed2DZDistance);
        
        // 경계 체크
        if (boundCamera)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, boundMinX, boundMaxX);
        }
            
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        // 목표 회전 계산 (타겟을 향함)
        Quaternion targetRotation = Quaternion.LookRotation(target.position - targetPosition);
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraTransitionSpeed;
            
            // 위치와 회전 보간
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            
            yield return null;
        }
        
        isTransitioning = false;
    }

    // 원래 모드로 전환하는 코루틴
    private IEnumerator TransitionToOriginalMode()
    {
        isTransitioning = true;
        
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        // 원래 위치 계산 (현재 카메라 위치를 2D 모드 기준으로 재계산)
        Vector3 calculatedOriginalPosition;
        Quaternion calculatedOriginalRotation;
        
        // 2D 모드의 원래 위치 계산
        float newCameraX = target.position.x;
        
        // 최대 허용 범위 적용
        if (Mathf.Abs(target.position.x - transform.position.x) > maxHorizontalDistance)
        {
            newCameraX = target.position.x - maxHorizontalDistance;
        }
        
        // 경계 체크
        if (boundCamera)
        {
            newCameraX = Mathf.Clamp(newCameraX, boundMinX, boundMaxX);
        }
        
        calculatedOriginalPosition = new Vector3(newCameraX, fixed2DYPosition, fixed2DZDistance);
        
        if (isSubCamera)
        {
            calculatedOriginalRotation = Quaternion.LookRotation(target.position - calculatedOriginalPosition);
        }
        else
        {
            calculatedOriginalRotation = Quaternion.Euler(0, 0, 0);
        }
        
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraTransitionSpeed;
            
            // 위치와 회전 보간
            transform.position = Vector3.Lerp(startPosition, calculatedOriginalPosition, elapsedTime);
            transform.rotation = Quaternion.Slerp(startRotation, calculatedOriginalRotation, elapsedTime);
            
            yield return null;
        }
        
        isTransitioning = false;
    }

    // 3D 카메라 업데이트 로직
    private void Update3DCamera()
    {
        // 마우스 회전 처리
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        _azimuthAngle += mouseX * rotationSpeed * Time.deltaTime;
        _polarAngle -= mouseY * rotationSpeed * Time.deltaTime;
        _polarAngle = Mathf.Clamp(_polarAngle, -10f, 60f);


        if (Input.GetKey(KeyCode.LeftAlt))
        {
            // 마우스 휠 줌 처리
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0)
            {
                // 위로 올리면(양수) 줌인(거리 감소), 아래로 내리면(음수) 줌아웃(거리 증가)
                distance -= scrollWheel * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
        
        
        
        // 벽감지 처리
        float currentDistance = AdjustCameraDistance();
        
        var cartesianPosition = GetCameraPosition(currentDistance, _polarAngle, _azimuthAngle);
        var cameraPosition = target.position - cartesianPosition;
        
        transform.position = cameraPosition;
        transform.LookAt(target);
    }

    // 2D 카메라 업데이트 로직 (단순 경계)
    private void Update2DCamera()
    {
        // 탭 모드일 때는 일반 카메라 업데이트 생략
        if (isInTabMode)
        {
            // 탭 모드에서도 타겟이 움직이면 카메라가 따라가도록 처리
            Vector3 newTabPosition = new Vector3(
                target.position.x + tabCameraXOffset,
                fixed2DYPosition,
                fixed2DZDistance);
                
            // 경계 체크
            if (boundCamera)
            {
                newTabPosition.x = Mathf.Clamp(newTabPosition.x, boundMinX, boundMaxX);
            }
            
            transform.position = newTabPosition;
            transform.LookAt(target); // 항상 타겟을 바라보게 함
            return;
        }
        
        // 타겟과 카메라 사이의 X축 거리 계산
        float distanceX = target.position.x - transform.position.x;
        
        // 카메라 X 위치 계산
        float newCameraX = transform.position.x;
        
        // 캐릭터가 최대 허용 범위를 벗어났는지 확인
        if (Mathf.Abs(distanceX) > maxHorizontalDistance)
        {
            // 카메라 위치 직접 조정 - 캐릭터가 항상 최대 허용 범위 내에 있도록 함
            newCameraX = target.position.x - Mathf.Sign(distanceX) * maxHorizontalDistance;
        }
        
        // 경계 체크
        if (boundCamera)
        {
            newCameraX = Mathf.Clamp(newCameraX, boundMinX, boundMaxX);
        }
        
        // 카메라 위치 업데이트 (즉시 이동, Y축은 고정)
        Vector3 newPosition = new Vector3(newCameraX, fixed2DYPosition, fixed2DZDistance);
        transform.position = newPosition;
        
        if (isSubCamera)
        {
            transform.LookAt(target);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // 3D 위치 설정
    private void Set3DPosition()
    {
        var cartesianPosition = GetCameraPosition(distance, _polarAngle, _azimuthAngle);
        var cameraPosition = target.position - cartesianPosition;
        
        transform.position = cameraPosition;
        transform.LookAt(target);
    }

    // 2D 위치 설정
    private void Set2DPosition()
    {
        Vector3 targetPosition = new Vector3(target.position.x, fixed2DYPosition, fixed2DZDistance);
        transform.position = targetPosition;
        
        if (isSubCamera)
        {
            transform.LookAt(target);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    Vector3 GetCameraPosition(float r, float polarAngle, float azimuthAngle)
    {
        float b = r * Mathf.Cos(polarAngle * Mathf.Deg2Rad);
        float z = b * Mathf.Cos(azimuthAngle * Mathf.Deg2Rad);
        float y = r * Mathf.Sin(polarAngle * Mathf.Deg2Rad) * -1;
        float x = b * Mathf.Sin(azimuthAngle * Mathf.Deg2Rad);
        
        return new Vector3(x, y, z);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        if (target == null) return;
        
        if (is2DMode)
        {
            Set2DPosition();
        }
        else
        {
            Set3DPosition();
        }
    }
    
    // 카메라와 타겟 사이에 장애물이 있을 때 카메라와 타겟간의 거리를 조절하는 함수
    private float AdjustCameraDistance()
    {
        var currentDistance = distance;
        
        // 타겟에서 카메라 방향으로 레이케이스를 발사
        Vector3 direction = GetCameraPosition(1f, _polarAngle, _azimuthAngle).normalized;
        RaycastHit hit;

        // 타겟에서 카메라 예정 위치까지 레이케이스 발사
        if (Physics.Raycast(target.position, -direction, out hit, distance, obstacleLayerMask))
        {
            float offset = 0.3f;
            currentDistance = hit.distance - offset;
            currentDistance = Mathf.Max(currentDistance, 0.5f);
        }
        return currentDistance;
    }
    
    // 2D/3D 모드 전환 메서드 (외부에서 호출 가능)
    public void Toggle2DMode(bool enable2D)
    {
        // 모드 전환 시 항상 탭 모드 초기화
        if (isInTabMode)
        {
            isInTabMode = false;
            // 경계값 복원
            if (boundCamera)
            {
                boundMinX = originalBoundMinX;
                boundMaxX = originalBoundMaxX;
            }
        }
        
        is2DMode = enable2D;
        
        // 모드 전환 시 즉시 카메라 위치 업데이트
        if (target != null)
        {
            if (is2DMode)
            {
                Set2DPosition();
            }
            else
            {
                Set3DPosition();
            }
        }
    }
    
    // 경계 설정 메서드 (외부에서 호출 가능)
    public void SetBounds(float minX, float maxX)
    {
        boundCamera = true;
        boundMinX = minX;
        boundMaxX = maxX;
        
        // 원래 경계값 저장
        originalBoundMinX = minX;
        originalBoundMaxX = maxX;
    }
    
    // 경계 비활성화 메서드
    public void DisableBounds()
    {
        boundCamera = false;
    }
    
    public bool Is2DMode()
    {
        return is2DMode;
    }
    
    public void AdjustFixed2DYPosition(float deltaY)
    {
        fixed2DYPosition += deltaY;
    }

    // 현재 fixed2DYPosition 값을 가져오는 메서드
    public float GetFixed2DYPosition()
    {
        return fixed2DYPosition;
    }
}