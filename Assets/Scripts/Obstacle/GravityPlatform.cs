using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 발판을 밟았을 때 트리거되는 인터페이스를 구현
public class GravityPlatform : MonoBehaviour, ITriggerableObstacle
{
    [Tooltip("변경할 중력 방향")]
    [SerializeField] private Vector3 gravityDirection = Vector3.forward;
    
    [Tooltip("중력 강도 배율")]
    [SerializeField] private float gravityStrength = 1.0f;
    
    [Tooltip("회전 전환 속도")]
    [SerializeField] private float rotationSpeed = 8.0f;
    
    // 발판 활성화 상태
    private bool isActive = true;
    
    // 한번 밟으면 비활성화할지 여부
    [SerializeField] private bool deactivateAfterUse = false;
    
    // 다시 활성화되는 시간 (0이면 재활성화 안됨)
    [SerializeField] private float reactivateTime = 0f;
    
    // 새로 추가: 중력 방향별 목표 위치 설정
    [System.Serializable]
    public class GravityPositionMapping
    {
        public Vector3 gravityDir;
        public Vector3 targetPosition;
    }
    
    [Tooltip("중력 방향별 캐릭터 이동 위치 매핑")]
    [SerializeField] private List<GravityPositionMapping> positionMappings = new List<GravityPositionMapping>();
    
    [Tooltip("목표 위치로 이동하는 속도")]
    [SerializeField] private float moveSpeed = 10.0f;
    
    [Tooltip("위치 이동 사용 여부")]
    [SerializeField] private bool usePositionMapping = true;
    
    // 회전 중인 플레이어를 추적하기 위한 Dictionary
    private static Dictionary<GameObject, Coroutine> rotatingPlayers = new Dictionary<GameObject, Coroutine>();
    
    // 플레이어가 발판을 밟았을 때 호출되는 메서드
    public void TriggerObstacle(GameObject player)
    {
        if (!isActive) return;

        ImprovedSoundManager.Instance.PlaySound3D("MushJump", transform.position);
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // 중력 방향 변경
            playerController.SetGravityDirection(gravityDirection, gravityStrength);
            
            // 이전 회전 코루틴이 실행 중이면 중지
            if (rotatingPlayers.ContainsKey(player) && rotatingPlayers[player] != null)
            {
                StopCoroutine(rotatingPlayers[player]);
            }
            
            // 새로운 방향 설정 시작 - 회전 대신 방향 기반 처리
            Coroutine orientationCoroutine = StartCoroutine(OrientCharacter(player, gravityDirection));
            rotatingPlayers[player] = orientationCoroutine;
            
            // 중력 방향에 따른 목표 위치로 이동
            if (usePositionMapping)
            {
                Vector3 targetPosition = FindTargetPosition(gravityDirection, player);
                StartCoroutine(MoveCharacterToPosition(player, targetPosition));
            }
            
            // 사용 후 비활성화 옵션이 켜져 있으면
            if (deactivateAfterUse)
            {
                isActive = false;
                UpdateVisualState();
                
                if (reactivateTime > 0)
                {
                    StartCoroutine(ReactivateAfterDelay());
                }
            }
        }
    }
    
    
    
  
    
    private Vector3 FindTargetPosition(Vector3 gravityDir, GameObject player)
    {
        Vector3 currentPosition = player.transform.position;
        Vector3 targetPosition = currentPosition;
        
        foreach (var mapping in positionMappings)
        {
            if (mapping.gravityDir.normalized == gravityDir.normalized)
            {
                targetPosition = mapping.targetPosition;
                break;
            }
        }
        
        if (targetPosition == currentPosition)
        {
            if (gravityDir.normalized == Vector3.forward)
            {
                targetPosition = new Vector3(currentPosition.x, 4, 2);
            }
            else if (gravityDir.normalized == Vector3.down)
            {
                targetPosition = new Vector3(currentPosition.x, currentPosition.y-0.2f, 0);
            }
        }
        
        return targetPosition;
    }
    
    
    private IEnumerator MoveCharacterToPosition(GameObject player, Vector3 targetPosition)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        
        if (rb == null || playerController == null) yield break;
        
        // 이동 설정값
        float moveForce = 500f;       // 이동 힘 (리지드바디 mass 고려해서 조정)
        float maxSpeed = 10f;         // 최대 이동 속도
        float slowDistance = 1f;      // 감속 시작 거리
        float arrivalDistance = 0.1f; // 도착 판정 거리
        float maxTime = 3.0f;        // 최대 이동 시간
        float elapsedTime = 0f;
        
        // 초기 위치와 목표 위치의 차이 계산
        Vector3 initialPosition = player.transform.position;
        Vector3 directionToTarget = targetPosition - initialPosition;
        
        // 어느 축으로 이동하는지 판단
        bool isYMovement = Mathf.Abs(directionToTarget.y) > Mathf.Abs(directionToTarget.z);
        
        // 중력 보상값 (위로 올라갈 때 중력에 대항하기 위해)
        float gravityCompensation = 0f;
        if (isYMovement && directionToTarget.y > 0)
        {
            // 위로 올라가는 경우 중력 보상
            gravityCompensation = playerController.Gravity * playerController.GravityScale;
        }
        
        while (elapsedTime < maxTime)
        {
            Vector3 currentPosition = player.transform.position;
            Vector3 toTarget = targetPosition - currentPosition;
            
            // 이동이 필요한 축의 거리와 방향 계산
            float distanceToTarget;
            Vector3 moveDirection = Vector3.zero;
            
            if (isYMovement)
            {
                distanceToTarget = Mathf.Abs(toTarget.y);
                moveDirection = new Vector3(0, Mathf.Sign(toTarget.y), 0);
            }
            else
            {
                distanceToTarget = Mathf.Abs(toTarget.z);
                moveDirection = new Vector3(0, 0, Mathf.Sign(toTarget.z));
            }
            
            // 목표 지점에 도착했는지 확인
            if (distanceToTarget < arrivalDistance)
            {
                // 해당 축의 속도만 0으로
                Vector3 velocity = rb.linearVelocity;
                if (isYMovement)
                {
                    rb.linearVelocity = new Vector3(velocity.x, 0, velocity.z);
                }
                else
                {
                    rb.linearVelocity = new Vector3(velocity.x, velocity.y, 0);
                }
                
                Debug.Log("목표 위치 도달");
                break;
            }
            
            // 거리에 따른 속도 조절
            float targetSpeed = maxSpeed;
            if (distanceToTarget < slowDistance)
            {
                // 가까워질수록 속도 감소
                targetSpeed = maxSpeed * (distanceToTarget / slowDistance);
            }
            
            // 현재 속도 계산
            Vector3 currentVel = rb.linearVelocity;
            float currentSpeed = 0f;
            
            if (isYMovement)
            {
                currentSpeed = Mathf.Abs(currentVel.y);
            }
            else
            {
                currentSpeed = Mathf.Abs(currentVel.z);
            }
            
            // 목표 속도에 도달하지 않았으면 힘을 가함
            if (currentSpeed < targetSpeed)
            {
                Vector3 force = moveDirection * moveForce;
                
                // Y축 이동시 중력 보상 추가
                if (isYMovement && moveDirection.y > 0)
                {
                    force.y += gravityCompensation;
                }
                
                // 힘 적용
                rb.AddForce(force, ForceMode.Force);
            }
            else
            {
                // 속도가 너무 빠르면 감속
                if (isYMovement)
                {
                    rb.linearVelocity = new Vector3(
                        currentVel.x,
                        Mathf.Sign(currentVel.y) * targetSpeed,
                        currentVel.z
                    );
                }
                else
                {
                    rb.linearVelocity = new Vector3(
                        currentVel.x,
                        currentVel.y,
                        Mathf.Sign(currentVel.z) * targetSpeed
                    );
                }
            }
            
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        // 시간 초과시 처리
        if (elapsedTime >= maxTime)
        {
            Debug.Log("시간 초과 - 이동 중단");
        }
        
        // 최종 위치 보정 (작은 오차 수정)
        Vector3 finalPosition = player.transform.position;
        float finalDistance;
        
        if (isYMovement)
        {
            finalDistance = Mathf.Abs(finalPosition.y - targetPosition.y);
            if (finalDistance < 0.5f) // 0.5 유닛 이하면 보정
            {
                finalPosition.y = targetPosition.y;
                player.transform.position = finalPosition;
            }
        }
        else
        {
            finalDistance = Mathf.Abs(finalPosition.z - targetPosition.z);
            if (finalDistance < 0.5f)
            {
                finalPosition.z = targetPosition.z;
                player.transform.position = finalPosition;
            }
        }
        
        // 제약 조건 설정
        if (playerController.CurrentGravityDirection == Vector3.forward)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
        else if (playerController.CurrentGravityDirection == Vector3.down)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private IEnumerator OrientCharacter(GameObject player, Vector3 gravityDir)
    {
        // null 체크를 먼저 수행
        if (player == null) yield break;
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) yield break;
        
        Transform visualTransform = playerController.visualTransform;
        if (visualTransform == null) yield break;
        
        // 현재 방향 정보 저장 (forward 방향 유지를 위해)
        Vector3 currentForward = visualTransform.forward;
        
        // 목표 방향 계산
        Vector3 targetUp = -gravityDir.normalized;
        Vector3 targetForward;
        
        if (playerController.Is2DMode)
        {
            // 현재 플레이어가 바라보는 X방향 확인 (양수 또는 음수)
            float facingDirection = Mathf.Sign(currentForward.x);
            
            // 만약 x 방향이 거의 0에 가까우면 기본값 사용
            if (Mathf.Abs(currentForward.x) < 0.01f)
            {
                // 현재 상태에서 가장 큰 방향 성분 사용
                if (Mathf.Abs(currentForward.z) > Mathf.Abs(currentForward.y))
                {
                    facingDirection = Mathf.Sign(currentForward.z);
                }
                else
                {
                    facingDirection = 1; // 기본값
                }
            }
            
            // 중력 방향과 현재 바라보는 방향에 따라 새로운 forward 계산
            Vector3 worldRight = facingDirection > 0 ? Vector3.right : Vector3.left;
            targetForward = Vector3.ProjectOnPlane(worldRight, targetUp).normalized;
            
            // forward가 너무 작으면 대체 방향 사용
            if (targetForward.magnitude < 0.01f)
            {
                targetForward = Vector3.Cross(targetUp, Vector3.forward).normalized;
                if (facingDirection < 0) targetForward = -targetForward;
            }
            
            // 디버그 정보 출력
            Debug.Log($"원래 방향: {currentForward}, 방향 부호: {facingDirection}, 새 방향: {targetForward}");
        }
        else
        {
            // 3D 모드 처리
            targetForward = Vector3.ProjectOnPlane(currentForward, targetUp).normalized;
            
            if (targetForward.magnitude < 0.01f)
            {
                // 현재 오른쪽 방향 유지
                Vector3 currentRight = visualTransform.right;
                targetForward = Vector3.ProjectOnPlane(currentRight, targetUp).normalized;
            }
        }
        
        // 목표 회전 계산
        Quaternion targetOrientation = Quaternion.LookRotation(targetForward, targetUp);
        
        // 즉시 회전 적용
        visualTransform.rotation = targetOrientation;
        
        // 2D 모드에서는 transform.rotation도 동기화
        if (playerController.Is2DMode)
        {
            player.transform.rotation = targetOrientation;
            playerController.UpdateRotationFromTransform();
        }
        
        // Dictionary에서 제거
        if (rotatingPlayers.ContainsKey(player))
        {
            rotatingPlayers.Remove(player);
        }
        
        // 회전 완료 알림
        playerController.SetGravityRotationComplete();
        
        Debug.Log("캐릭터 회전 완료 - 방향 유지됨");
        
        // 코루틴을 즉시 종료하지만, 이벤트 순서를 보장하기 위해 1프레임 대기
        yield return null;
    }
    
    private IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(reactivateTime);
        isActive = true;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = isActive ? Color.cyan : Color.gray;
        }
    }
    
    private void Start()
    {
        UpdateVisualState();
    }
    
    private void OnDestroy()
    {
        foreach (var kvp in rotatingPlayers)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        rotatingPlayers.Clear();
    }
    
}