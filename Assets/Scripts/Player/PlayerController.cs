using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 플레이어 상태 열거형
public enum PlayerState { None, Idle, Walk, Roll, Open, Close, Jump, Dead, Left,Right, Jumping, Fire}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // 속성 및 설정
    [SerializeField] private float rotationSpeed = 140f;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rollSpeed = 30f;  // 구르기 시 이동 속도
    [SerializeField] private float acceleration = 50f; // 가속도 값 증가
    [SerializeField] private float deceleration = 10f; // 감속도 값 추가
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float groundCheckDistance = 0.1f; // 바닥 체크 거리
    [SerializeField] private float stepOffset = 0.3f;
    [SerializeField] private float maxSlopeAngle = 45f; // 올라갈 수 있는 최대 경사 각도
    [SerializeField] private LayerMask groundLayer; // 바닥으로 인식할 레이어
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("중력 설정")]
    [SerializeField] private float gravity = 9.81f; // 중력 크기
    [SerializeField] private float gravityScale = 1.5f; // 중력 스케일 (조절 가능)
    [SerializeField] private float terminalVelocity = 20f; // 최대 낙하 속도
    [SerializeField] private float jumpForce = 10f; // 점프 힘
    
    [SerializeField] private GravityGun gravityGun;
    public GravityGun GravityGun => gravityGun;
    public float Gravity => gravity;
    public float GravityScale => gravityScale;
    
    [Header("중력 방향 설정")]
    [SerializeField] private Vector3 currentGravityDirection = Vector3.down; // 현재 중력 방향
    [SerializeField] private bool useCharacterRotation = true; // 중력 방향에 따라 캐릭터 회전 사용 여부
    public Vector3 CurrentGravityDirection => currentGravityDirection;
    
    
    // 상태 관리용 변수
    private Vector3 _rotation = Vector3.zero;
    private Dictionary<PlayerState, IPlayerState> _playerStates;
    public PlayerState CurrentState { get; private set; }
    
    public Dissolve dissolve;
    
    public GameObject particleEffectPrefab;
    
    // 컴포넌트 참조
    public Animator Animator { get; private set; }
    private Rigidbody _rigidbody;
    
    // 이동 관련 변수
    private Vector3 _currentVelocity = Vector3.zero;
    private bool _isGrounded = false;
    private Vector3 _gravityVelocity = Vector3.zero; // 중력에 의한 속도
    private bool _isCustomGravityActive = true; // 사용자 정의 중력 활성화 여부
    private Vector3 _groundNormal = Vector3.up; // 현재 바닥의 노말 벡터
    private float _groundSlopeAngle = 0f; // 현재 바닥의 경사 각도
    private bool _isJumping = false;
    
    public bool readyJump = false;
    
    
    public bool isWalking = false;
    
    // 속성
    public float MoveSpeed => moveSpeed;
    public float RollSpeed => rollSpeed;
    public bool IsGrounded => _isGrounded;
    public Vector3 GroundNormal => _groundNormal;
    public float GroundSlopeAngle => _groundSlopeAngle;
    
    
    [SerializeField] private bool is2DMode = true; // 2D 모드 활성화 여부
    public bool Is2DMode { get => is2DMode; set => is2DMode = value; }
    
    // 디버그 용도
    private RaycastHit _groundHit;
    private RaycastHit _forwardHit;
    
    
    private bool _gravityRotationInProgress = false;
    private Vector3 _targetUpDirection;
    private Vector3 _targetForwardDirection;
    
    private bool _userInputDuringRotation = false;
    
    
    public Transform visualTransform;

    public bool nextPlatform=false;

    public float fireSpeed = 0f;
    
    
    
    // 초기화
    private void Awake()
    {
        Animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
        
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        
        transform.eulerAngles = _rotation;

        
    }
    
    private void ConfigureRigidbody()
    {
        // 회전은 직접 제어하므로 물리 회전은 동결
        _rigidbody.freezeRotation = true;
        
        if (Is2DMode)
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ| RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY| RigidbodyConstraints.FreezeRotationZ;
        }
        
        // 중요: Unity 기본 중력 비활성화
        _rigidbody.useGravity = false;
        
        // 적절한 마찰력 및 드래그 설정
        _rigidbody.linearDamping = 1f; // 공기 저항
        _rigidbody.angularDamping = 0.05f;
        _rigidbody.mass = 2f; // 캐릭터 질량 (필요에 따라 조정)
        
        // 연속 충돌 감지 사용 (더 정확한 충돌 처리)
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // 낙하 시 최대 속도 제한
        _rigidbody.maxDepenetrationVelocity = 10f;
        
    }
    
    private void Start()
    {
        Is2DMode = GameManager.Instance.Is2DMode();
        // 상태 초기화
        _playerStates = new Dictionary<PlayerState, IPlayerState>
        {
            { PlayerState.Idle, new PlayerStateIdle() },
            { PlayerState.Walk, new PlayerStateWalk() },
            { PlayerState.Roll, new PlayerStateRoll() },
            { PlayerState.Open, new PlayerStateOpen() },
            { PlayerState.Close, new PlayerStateClose() },
            { PlayerState.Jump, new PlayerStateJump() },
            { PlayerState.Dead, new PlayerStateDead() },
            { PlayerState.Left , new PlayerStateLeftTurn()},
            { PlayerState.Right , new PlayerStateRightTurn()},
            { PlayerState.Jumping , new PlayerStateAir()},
            { PlayerState.Fire , new PlayerStateFire()}
        };
        if (is2DMode)
        {
            ChunkedPoolManager.Instance.SetPlayerTransform(transform);
            UICircleTransition.Instance.SetPlayerTransform(transform);
        }
        
        
        // 초기 상태 설정
        SetState(PlayerState.Open);
        // Rigidbody 설정
        ConfigureRigidbody();

        if (GameManager.Instance.currentSceneName == "Stage3")
        {
            _rotation.y = 90f;
        }
        
        obstacleLayer= LayerMask.GetMask("Obstacle","GrabableObstacle");
        nextPlatform = GameManager.Instance.nextPlatform;
        if (GameManager.Instance.die)
        {
            UICircleTransition.Instance.CircleFadeIn();
            ImprovedSoundManager.Instance.ResumeBGM();
            GameManager.Instance.die=false;
            
        }
        
        
    }

    private void Update()
    {
        // 바닥 체크
        CheckGrounded();
        CheckObstacle();
        
        if(!GameManager.canPlayerMove) return;

        
        
        // 상태 업데이트
        if (CurrentState != PlayerState.None)
        {
            _playerStates[CurrentState].Update();
        }
        
        if (!_gravityRotationInProgress)
        {
            transform.eulerAngles = _rotation;
        }

        if (is2DMode)
        {
            if (nextPlatform)
            {
                if (transform.position.z > 5)
                {
                    if(CurrentState == PlayerState.Dead)return;
                    SetState(PlayerState.Dead);
                }
                else if (transform.position.y < 14)
                {
                    if(CurrentState == PlayerState.Dead)return;
                    SetState(PlayerState.Dead);
                }
                
                
            }
            else
            {
                if (transform.position.z > 5)
                {
                    if(CurrentState == PlayerState.Dead)return;
                    SetState(PlayerState.Dead);
                }
                else if (transform.position.y < -2)
                {
                    if(CurrentState == PlayerState.Dead)return;
                    SetState(PlayerState.Dead);
                }
                
            }
            
            
        }
        
        
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     //Jump(jumpForce);
        //     SetState(PlayerState.Jump);
        // }
        
        //죽었을 때 
        if (Input.GetKeyDown(KeyCode.N))
        {
            SetState(PlayerState.Dead);
        }
        
    }
    
    private void FixedUpdate()
    {
        // 사용자 정의 중력 적용
        if (_isCustomGravityActive)
        {
            ApplyCustomGravity();
        }
        // 상태 업데이트
        if (CurrentState != PlayerState.None)
        {
            _playerStates[CurrentState].FixedUpdate();
        }
        
    }

    #region Gravity

    

    private void ApplyCustomGravity()
    {
        if (!_isCustomGravityActive) return;

        // 현재 중력 방향으로 중력 적용
        Vector3 gravityForce = currentGravityDirection * (gravity * gravityScale * Time.fixedDeltaTime);

        // 중력 적용 (매 프레임마다 누적됨)
        _gravityVelocity += gravityForce;

        // 최대 낙하 속도 제한 (중력 방향을 고려)
        float currentFallSpeed = Vector3.Dot(_gravityVelocity, currentGravityDirection);
        if (currentFallSpeed > terminalVelocity)
        {
            // 중력 방향으로의 속도만 제한 (다른 방향의 속도는 유지)
            Vector3 excessVelocity = currentGravityDirection * (currentFallSpeed - terminalVelocity);
            _gravityVelocity -= excessVelocity;
        }

        // 중력 벡터 적용
        _rigidbody.AddForce(_gravityVelocity, ForceMode.Acceleration);

        // 디버그 정보 표시 (필요시)
        Debug.DrawRay(transform.position, _gravityVelocity, Color.red);
        Debug.DrawRay(transform.position, currentGravityDirection * 2f, Color.yellow);

        // 바닥에 닿았을 때 처리
        if (_isGrounded)
        {
            // 현재 속도 가져오기
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            
            // 중력 방향으로의 속도 성분 계산
            float velocityInGravityDirection = Vector3.Dot(currentVelocity, currentGravityDirection);
            
            if (is2DMode)
            {
                // 중력 방향으로의 속도가 양수이고 점프 중이 아닐 때만 해당 속도 성분 제거
                if (velocityInGravityDirection > 0 && !_isJumping)
                {
                    // 중력 방향 성분만 제거하고 다른 방향의 속도는 유지
                    Vector3 gravityDirVelocity = currentGravityDirection * velocityInGravityDirection;
                    
                    // 중요: 속도가 급격히 바뀌지 않도록 부드럽게 감속
                    Vector3 newVelocity = currentVelocity - (gravityDirVelocity * 0.8f * Time.fixedDeltaTime * 10f);
                    _rigidbody.linearVelocity = newVelocity;
                    
                    // 바닥에 닿았을 때 중력 가속 벡터를 서서히 감소시킴
                    _gravityVelocity = Vector3.Lerp(_gravityVelocity, currentGravityDirection * 0.1f, Time.fixedDeltaTime * 5f);
                }
            }
            else
            {
                // 3D 모드 처리 (경사면 처리 포함)
                if (_groundSlopeAngle > 0)
                {
                    // 현재 중력 기준 "위쪽" 방향 (중력의 반대 방향)
                    Vector3 currentUp = -currentGravityDirection;
                    
                    // 경사면과 현재 "위쪽" 방향 간의 각도를 기준으로 미끄러짐 계산
                    float actualSlopeAngle = Vector3.Angle(_groundNormal, currentUp);
                    
                    // 경사면 아래쪽 방향 계산 (중력 방향을 경사면에 투영)
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(currentGravityDirection, _groundNormal).normalized;
                    
                    // 경사 각도에 비례하여 미끄러짐 힘 계산
                    float slopeForce = gravity * gravityScale * Mathf.Sin(actualSlopeAngle * Mathf.Deg2Rad);
                    
                    // 경사면을 따라 미끄러지는 힘 적용
                    _rigidbody.AddForce(slopeDirection * slopeForce, ForceMode.Acceleration);
                    
                    // 중력 방향으로의 속도가 양수이고 점프 중이 아닐 때
                    if (velocityInGravityDirection > 0 && !_isJumping)
                    {
                        // 중력 방향 성분만 서서히 제거
                        Vector3 gravityDirVelocity = currentGravityDirection * velocityInGravityDirection;
                        Vector3 newVelocity = currentVelocity - (gravityDirVelocity * 0.8f * Time.fixedDeltaTime * 10f);
                        _rigidbody.linearVelocity = newVelocity;
                    }
                }
                else
                {
                    // 평지에서는 중력 방향으로의 속도가 양수이고 점프 중이 아닐 때만 해당 속도 성분 제거
                    if (velocityInGravityDirection > 0 && !_isJumping)
                    {
                        // 중력 방향 성분만 서서히 제거
                        Vector3 gravityDirVelocity = currentGravityDirection * velocityInGravityDirection;
                        Vector3 newVelocity = currentVelocity - (gravityDirVelocity * 0.8f * Time.fixedDeltaTime * 10f);
                        _rigidbody.linearVelocity = newVelocity;
                        
                        // 바닥에 닿았을 때 중력 가속 벡터 서서히 감소
                        _gravityVelocity = Vector3.Lerp(_gravityVelocity, currentGravityDirection * 0.1f, Time.fixedDeltaTime * 5f);
                    }
                }
            }
        }
    }
    
    // 중력 방향 변경 메서드 (중력건 등에서 사용 가능)
    public void SetGravityDirection(Vector3 direction, float strength = 1.0f)
    {
        // 방향은 정규화하여 사용
        Vector3 normalizedDir = direction.normalized;

        // 이전 중력 방향 저장
        Vector3 previousGravityDir = currentGravityDirection;

        // 새 중력 방향 설정
        currentGravityDirection = normalizedDir;
        
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        
        
        // 물리적 제약 조건 설정 (특히 2D 모드를 위한 처리)
        // if (direction == Vector3.forward && is2DMode)
        // {
        //     _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX |
        //                              RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        // }
        // else if (direction == Vector3.down && is2DMode)
        // {
        //     _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX |
        //                              RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        // }

        // 중요: 중력 방향 변경 시 이전 속도의 중력 방향 성분 제거
        Vector3 currentVelocity = _rigidbody.linearVelocity;
    
        // 이전 중력 방향으로의 속도 성분 계산
        float velocityInPrevGravityDir = Vector3.Dot(currentVelocity, previousGravityDir);
    
        // 이전 중력 방향 성분 제거
        if (velocityInPrevGravityDir > 0)
        {
            Vector3 gravityDirVelocity = previousGravityDir * velocityInPrevGravityDir;
            _rigidbody.linearVelocity = currentVelocity - gravityDirVelocity;
        }
    
        // 중력 가속도 초기화 (약간의 초기값만 설정하여 급격한 가속 방지)
        _gravityVelocity = normalizedDir * (gravity * strength * 0.1f);
    
        // 중력 방향이 바뀌었음을 로그로 출력
        Debug.Log($"중력 방향 변경: {previousGravityDir} -> {currentGravityDirection}");
        _gravityRotationInProgress = true;
    
        // 중력 방향 변경 시 캐릭터가 공중에 있다고 간주
        _isGrounded = false;
    }
    
    // 중력 활성화/비활성화 메서드
    public void ToggleCustomGravity(bool active)
    {
        _isCustomGravityActive = active;
        
        // 비활성화 시 중력 벡터 초기화
        if (!active)
        {
            _gravityVelocity = Vector3.zero;
        }
    }
    

    // 중력 회전 완료 설정 메서드
    public void SetGravityRotationComplete()
    {
        _gravityRotationInProgress = false;
    }
    #endregion
    
    
    private void CheckGrounded()
    {
        Ray groundRay = new Ray(transform.position + (-currentGravityDirection * 0.1f), currentGravityDirection);

        // 레이캐스트 실행 및 결과 저장
        bool hit = Physics.Raycast(
            groundRay,
            out _groundHit,
            0.2f,
            groundLayer
        );

        // 바닥 상태 업데이트
        _isGrounded = hit;

        if (is2DMode)
        {
            if (_isGrounded)
            {
                // 바닥 노말 벡터 저장
                _groundNormal = _groundHit.normal;
    
                // 바닥과 수직(중력 반대) 벡터의 각도 계산
                _groundSlopeAngle = Vector3.Angle(_groundNormal, -CurrentGravityDirection);
    
                // 착지 시 속도 보정 (바닥에 닿았을 때 튀어오르는 현상 방지)
                Vector3 currentVel = _rigidbody.linearVelocity;
            
                // 중력 방향으로의 속도 성분 계산
                float velocityInGravityDir = Vector3.Dot(currentVel, currentGravityDirection);
            
                if (velocityInGravityDir > 0 && !_isJumping)
                {
                    // 중력 방향 속도 성분 서서히 감소
                    Vector3 gravityDirVelocity = currentGravityDirection * velocityInGravityDir;
                    Vector3 newVelocity = currentVel - (gravityDirVelocity * 0.8f);
                    _rigidbody.linearVelocity = newVelocity;
                }
            }
            else
            {
                // 공중에 있을 때 기본값 설정
                _groundNormal = -currentGravityDirection;
                _groundSlopeAngle = 0f;
            }
        }
        else
        {
            if (_isGrounded)
            {
                // 바닥 노말 벡터 저장
                _groundNormal = _groundHit.normal;
    
                // 바닥과 수직(위쪽) 벡터의 각도 계산
                _groundSlopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
    
                // 착지 시 속도 보정 (바닥에 닿았을 때 튀어오르는 현상 방지)
                Vector3 currentVel = _rigidbody.linearVelocity;
                if (currentVel.y < 0 && !_isJumping)
                {
                    // 점프 중이 아닐 때만 y축 속도를 0으로 설정
                    _rigidbody.linearVelocity = new Vector3(currentVel.x, 0, currentVel.z);
                }
            }
            else
            {
                // 공중에 있을 때 기본값 설정
                _groundNormal = Vector3.up;
                _groundSlopeAngle = 0f;
            } 
        }
        
        

        // 바닥 체크와 별도로 항상 장애물 체크 수행
        //CheckObstacle();
    }

    // 장애물 체크 및 트리거 함수
    private void CheckObstacle()
    {
        Vector3 raycastStart = transform.position + Vector3.up * 0.5f;

        // 중앙 레이캐스트
        Ray centerRay = new Ray(raycastStart, currentGravityDirection);
        Debug.DrawRay(centerRay.origin, centerRay.direction * 0.7f, Color.red);

        // 앞쪽 레이캐스트 (플레이어의 정면 방향으로 0.3f)
        Vector3 frontPos = raycastStart + transform.forward * 0.5f;
        Ray frontRay = new Ray(frontPos, currentGravityDirection);
        Debug.DrawRay(frontRay.origin, frontRay.direction * 0.7f, Color.green);

        // 뒤쪽 레이캐스트 (플레이어의 반대 방향으로 0.3f)
        Vector3 backPos = raycastStart - transform.forward * 0.5f;
        Ray backRay = new Ray(backPos, currentGravityDirection);
        Debug.DrawRay(backRay.origin, backRay.direction * 0.7f, Color.blue);

        RaycastHit obstacleHit;

        // 세 개의 레이캐스트 모두 확인
        if (Physics.Raycast(centerRay, out obstacleHit, 0.8f, obstacleLayer))
        {
            TriggerObstacleIfPossible(obstacleHit);
        }
        else if (Physics.Raycast(frontRay, out obstacleHit, 0.8f, obstacleLayer))
        {
            TriggerObstacleIfPossible(obstacleHit);
        }
        else if (Physics.Raycast(backRay, out obstacleHit, 0.8f, obstacleLayer))
        {
            TriggerObstacleIfPossible(obstacleHit);
        }
    }
    private void TriggerObstacleIfPossible(RaycastHit hit)
    {
        if (hit.collider.TryGetComponent<ITriggerableObstacle>(out var obstacle))
        {
            obstacle.TriggerObstacle(gameObject);
        }
    }
    
    
    // 앞쪽 경사면 체크 메서드 - 개선된 버전
    private bool CheckForwardSlope(out Vector3 slopeNormal)
    {
        slopeNormal = Vector3.up;
        
        // 여러 높이에서 앞으로 레이캐스트 (계단과 같은 구조물도 감지)
        bool hitFound = false;
        float lowestSlopeAngle = 90f;  // 가장 완만한 경사각 찾기
        
        // 여러 높이에서 레이캐스트 실행
        for (float heightOffset = 0.05f; heightOffset <= 0.4f; heightOffset += 0.15f)
        {
            Ray forwardRay = new Ray(transform.position + Vector3.up * heightOffset, transform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(forwardRay, out hit, 1.0f, groundLayer))
            {
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                
                // 더 완만한 경사면 발견 시 업데이트
                if (slopeAngle < lowestSlopeAngle && slopeAngle <= maxSlopeAngle)
                {
                    lowestSlopeAngle = slopeAngle;
                    slopeNormal = hit.normal;
                    _forwardHit = hit;
                    hitFound = true;
                }
            }
        }
        
        // 추가: 발 앞쪽 아래 방향으로도 레이캐스트 (계단 아래부분 감지)
        Ray downwardRay = new Ray(
            transform.position + transform.forward * 0.3f + Vector3.up * 0.5f, 
            Vector3.down
        );
        
        RaycastHit downHit;
        if (Physics.Raycast(downwardRay, out downHit, 1.0f, groundLayer))
        {
            float slopeAngle = Vector3.Angle(downHit.normal, Vector3.up);
            
            // 더 완만한 경사면 발견 시 업데이트
            if (slopeAngle < lowestSlopeAngle && slopeAngle <= maxSlopeAngle)
            {
                lowestSlopeAngle = slopeAngle;
                slopeNormal = downHit.normal;
                _forwardHit = downHit;
                hitFound = true;
            }
        }
        
        return hitFound;
    }
    
    // 상태 변경 메서드
    public void SetState(PlayerState state)
    {
        if (CurrentState != PlayerState.None)
        {
            _playerStates[CurrentState].Exit();
        }
        
        CurrentState = state;
        _playerStates[CurrentState].Enter(this);
    }
    
    public void SetGravityRotationInProgress(bool inProgress)
    {
        _gravityRotationInProgress = inProgress;
    }

// 중력 회전 목표 방향 설정 메서드
    public void SetGravityRotationTarget(Vector3 upDir, Vector3 forwardDir)
    {
        _targetUpDirection = upDir;
        _targetForwardDirection = forwardDir;
    }


// Transform에서 회전값 업데이트 (기존 코드 유지)
    public void UpdateRotationFromTransform()
    {
        _rotation = transform.eulerAngles;
    }
    
    // 회전 처리 메서드 수정
    public void HandleRotation()
    {
        if (is2DMode)
        {
            Vector3 movementDirection = Vector3.zero;
        
            // 플레이어 입력에 따라 이동 방향 결정
            if (Input.GetKey(KeyCode.A))
            {
                // 왼쪽 방향으로 이동 및 시각적 회전
                movementDirection = Vector3.ProjectOnPlane(Vector3.left, -currentGravityDirection).normalized;
                transform.rotation = Quaternion.LookRotation(movementDirection, -currentGravityDirection);
                visualTransform.rotation = transform.rotation; // 모델도 같은 방향으로 회전
            
                // _rotation 값도 업데이트
                UpdateRotationFromTransform();
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // 오른쪽 방향으로 이동 및 시각적 회전
                movementDirection = Vector3.ProjectOnPlane(Vector3.right, -currentGravityDirection).normalized;
                transform.rotation = Quaternion.LookRotation(movementDirection, -currentGravityDirection);
                visualTransform.rotation = transform.rotation; // 모델도 같은 방향으로 회전
            
                // _rotation 값도 업데이트
                UpdateRotationFromTransform();
            }
        }
        else
        {
            // 3D 모드 처리는 기존과 동일
            if (Input.GetKey(KeyCode.A))
            {
                _rotation.y -= rotationSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                _rotation.y += rotationSpeed * Time.deltaTime;
            }
        
            transform.eulerAngles = _rotation;
            visualTransform.rotation = transform.rotation; // 모델도 같은 방향으로 회전
        }
    }

    public void LockRight2D()
    {
        Vector3 rightMovementDirection= Vector3.zero;
        rightMovementDirection = Vector3.ProjectOnPlane(Vector3.right, -currentGravityDirection).normalized;
        transform.rotation = Quaternion.LookRotation(rightMovementDirection, -currentGravityDirection);
        visualTransform.rotation = transform.rotation; // 모델도 같은 방향으로 회전
        // _rotation 값도 업데이트
        UpdateRotationFromTransform();
    }
    
    
    // 사용자 입력 플래그를 리셋하는 메서드 추가
    public void ResetUserInputDuringRotation()
    {
        _userInputDuringRotation = false;
    }

// 사용자 입력 플래그를 가져오는 메서드 추가
    public bool GetUserInputDuringRotation()
    {
        return _userInputDuringRotation;
    }
    
    public void MoveForward(float targetSpeed)
    {

        if (is2DMode)
        {
            targetSpeed = Mathf.Min(targetSpeed, maxSpeed);

            Vector3 moveDirection;

            // 중력 방향을 고려한 이동 방향 설정
            if (currentGravityDirection == Vector3.down)
            {
                // 기본 중력 (아래 방향)
                moveDirection = transform.forward;
            }
            else if (currentGravityDirection == Vector3.forward)
            {
                // 앞쪽 방향 중력 - 좌우 이동만 가능하도록 처리
                Vector3 right = Vector3.Cross(Vector3.up, currentGravityDirection).normalized;
                if (transform.forward.x > 0)
                    moveDirection = right;
                else
                    moveDirection = -right;
            }
            else
            {
                // 다른 방향의 중력은 중력 평면에 투영된 방향 사용
                Vector3 right = Vector3.Cross(Vector3.up, currentGravityDirection).normalized;
                if (transform.forward.x > 0)
                    moveDirection = right;
                else
                    moveDirection = -right;
            }

            // 목표 속도 벡터 계산 (중력 방향에 직교하는 평면 상의 이동)
            Vector3 gravityPerpendicularPlane =
                Vector3.ProjectOnPlane(moveDirection, currentGravityDirection).normalized;
            Vector3 targetVelocity = gravityPerpendicularPlane * targetSpeed;

            // 현재 속도에서 중력 방향 성분 제외한 수평 속도 계산
            Vector3 currentVel = _rigidbody.linearVelocity;
            Vector3 velocityInGravityDir = Vector3.Project(currentVel, currentGravityDirection);
            Vector3 horizontalVelocity = currentVel - velocityInGravityDir;
            float currentSpeed = horizontalVelocity.magnitude;

            // 현재 속도가 최대 속도를 초과하는 경우 강제로 제한
            if (currentSpeed > maxSpeed)
            {
                Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
                _rigidbody.linearVelocity = limitedVelocity + velocityInGravityDir;

                // 현재 속도 업데이트
                currentVel = _rigidbody.linearVelocity;
                velocityInGravityDir = Vector3.Project(currentVel, currentGravityDirection);
                horizontalVelocity = currentVel - velocityInGravityDir;
                currentSpeed = horizontalVelocity.magnitude;
            }

            // 가속/감속 로직
            if (currentSpeed < targetSpeed)
            {
                // 목표 속도로 서서히 가속
                Vector3 force = (targetVelocity - horizontalVelocity) * acceleration;

                // 중력 방향 성분 제거
                force = Vector3.ProjectOnPlane(force, currentGravityDirection);

                // ForceMode.Force 사용
                _rigidbody.AddForce(force, ForceMode.Force);
            }
            else if (currentSpeed > targetSpeed)
            {
                // 감속 로직
                Vector3 brakingForce;

                if (targetSpeed > 0)
                {
                    // 타겟 속도가 0보다 크면 해당 방향으로 감속
                    brakingForce = (targetVelocity - horizontalVelocity) * deceleration;
                }
                else
                {
                    // 타겟 속도가 0이면 완전히 멈추도록 감속
                    brakingForce = -horizontalVelocity * deceleration;
                }

                // 중력 방향 성분 제거
                brakingForce = Vector3.ProjectOnPlane(brakingForce, currentGravityDirection);

                // ForceMode.Force 사용
                _rigidbody.AddForce(brakingForce, ForceMode.Force);
            }
        }
        else
        {
                // 목표 속도가 최대 속도를 초과하지 않도록 제한
            targetSpeed = Mathf.Min(targetSpeed, maxSpeed);
            
            Vector3 moveDirection;
            Vector3 forwardSlopeNormal;
            bool isClimbableSlope = CheckForwardSlope(out forwardSlopeNormal);
            
            // 이동 방향 결정
            if (_isGrounded)
            {
                // 1. 바닥에 있을 경우 바닥 노말에 맞게 이동 방향 조정
                moveDirection = Vector3.ProjectOnPlane(transform.forward, _groundNormal).normalized;
                
                // 2. 앞에 올라갈 수 있는 경사가 있는 경우, 경사면 방향 고려
                if (isClimbableSlope)
                {
                    // 현재 바닥과 앞쪽 경사면의 노말 벡터를 평균내어 더 자연스러운 방향 계산
                    Vector3 blendedNormal = (_groundNormal + forwardSlopeNormal).normalized;
                    Vector3 slopeDirection = Vector3.ProjectOnPlane(transform.forward, blendedNormal).normalized;
                    
                    // 경사가 가파를수록 위쪽 성분 강화
                    float upwardComponent = Mathf.Clamp01(Vector3.Angle(Vector3.up, blendedNormal) / 45f) * 0.3f;
                    moveDirection = (slopeDirection + Vector3.up * upwardComponent).normalized;
                }
            }
            else
            {
                // 공중에 있을 경우 기본 전방 방향 사용
                moveDirection = transform.forward;
            }
            
            // 목표 속도 벡터 계산 (x, z축만)
            Vector3 horizontalMoveDir = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            Vector3 targetVelocity = horizontalMoveDir * targetSpeed;
            
            // y축 방향성분과 수평성분 분리
            float verticalComponent = moveDirection.y;
            
            // 현재 수평 속도
            Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
            
            // 현재 속도가 최대 속도를 초과하는 경우 강제로 제한
            if (currentSpeed > maxSpeed)
            {
                Vector3 limitedVelocity = horizontalVelocity.normalized * maxSpeed;
                _rigidbody.linearVelocity = new Vector3(
                    limitedVelocity.x,
                    _rigidbody.linearVelocity.y,
                    limitedVelocity.z
                );
                
                // 현재 속도 업데이트
                horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
                currentSpeed = horizontalVelocity.magnitude;
            }
            
            // 가속/감속 로직
            if (currentSpeed < targetSpeed)
            {
                // 목표 속도로 서서히 가속
                Vector3 force = (targetVelocity - horizontalVelocity) * acceleration;
                
                // y축 속도는 유지 (중력 영향 보존)
                force.y = 0;
                
                // ForceMode.Force 사용
                _rigidbody.AddForce(force, ForceMode.Force);
            }
            else if (currentSpeed > targetSpeed) 
            {
                // 감속 로직
                Vector3 brakingForce;
                
                if (targetSpeed > 0)
                {
                    // 타겟 속도가 0보다 크면 해당 방향으로 감속
                    brakingForce = (targetVelocity - horizontalVelocity) * deceleration;
                }
                else
                {
                    // 타겟 속도가 0이면 완전히 멈추도록 감속
                    brakingForce = -horizontalVelocity * deceleration;
                }
                
                brakingForce.y = 0;
                
                
                // ForceMode.Force 사용
                _rigidbody.AddForce(brakingForce, ForceMode.Force);
            }
            
            // 수평 속도 조정 (x, z축 일정하게 유지)
            horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            
            if (horizontalVelocity.magnitude > 0.1f)
            {
                // 방향은 유지하면서 크기를 목표 속도로 조정
                if (targetSpeed > 0)
                {
                    Vector3 normalizedDir = horizontalVelocity.normalized;
                    Vector3 adjustedVelocity = normalizedDir * Mathf.Min(targetSpeed, maxSpeed);
                    
                    // 위아래 방향의 힘과 수평 방향의 힘 결합 (경사면에서 올라가게)
                    if (_isGrounded && verticalComponent > 0)
                    {
                        // 경사면에서 위로 올라가는 힘을 높이의 성분으로 추가
                        Vector3 upwardForce = Vector3.up * (verticalComponent * targetSpeed);
                        adjustedVelocity += upwardForce;
                    }
                    
                    // 현재 y축 속도는 유지하면서 x, z축 속도만 조정
                    _rigidbody.linearVelocity = new Vector3(
                        adjustedVelocity.x,
                        _rigidbody.linearVelocity.y,
                        adjustedVelocity.z
                    );
                }
            }
            
        }
        
        
        
    }


    public void JumpStart(float jump, float timeToJump)
    {
        StartCoroutine(StartJump(jump, timeToJump));
    }
    
    IEnumerator StartJump(float jump, float time)
    {
        readyJump = true;
        yield return new WaitForSecondsRealtime(time);
        Jump(jumpForce);
        
    }
    
    public void Jump(float baseJumpForce)
    {
        if (_isGrounded)
        {
            readyJump = false;
            // 점프 상태 표시를 위한 변수 추가 (클래스 멤버 변수로 선언 필요)
            // private bool _isJumping = false; 를 클래스에 추가
            _isJumping = true;
        
            // 1. 점프 전에 경사면 미끄러짐 속도 성분 제거
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            if (_groundSlopeAngle > 0)
            {
                // 경사면 방향 계산
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, _groundNormal).normalized;
        
                // 경사면 방향으로의 속도 성분 계산
                float slopeVelocityMagnitude = Vector3.Dot(currentVelocity, slopeDirection);
                Vector3 slopeVelocity = slopeDirection * slopeVelocityMagnitude;
        
                // 경사면 속도 성분을 제거한 새 속도 설정
                _rigidbody.linearVelocity = currentVelocity - slopeVelocity;
            }
    
            // 2. 기본 점프력 계산
            float jumpForce = baseJumpForce;
    
            // 3. 경사면에서 추가 상향력 계산 (경사가 클수록 더 높게 점프)
            if (_groundSlopeAngle > 0)
            {
                // 경사면 각도 보정 계수 (최대 30%)
                float slopeBoostFactor = Mathf.Clamp01(_groundSlopeAngle / maxSlopeAngle) * 0.3f;
            
                // 경사면 각도에 비례하여 점프력 증가
                jumpForce *= (1f + slopeBoostFactor);
            }
    
            // 4. 점프 적용 - 현재 y축 속도를 완전히 초기화하고 점프력 적용
            Vector3 velocity = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = new Vector3(velocity.x, 0, velocity.z);
            _rigidbody.AddForce(-currentGravityDirection * jumpForce, ForceMode.Impulse);
        
            // 점프 시 중력 벡터 초기화
            _gravityVelocity = Vector3.zero;
        
            // 잠시 후 점프 상태 해제 (코루틴 사용)
            StartCoroutine(ResetJumpingState());
        }
    }

    private IEnumerator ResetJumpingState()
    {
        // 0.1초 후 점프 상태 해제
        yield return new WaitForSeconds(0.1f);
        _isJumping = false;
        
    }

    public float GetCurrentSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
        float currentSpeed = horizontalVelocity.magnitude;
        return currentSpeed;
    
    }

    

    public void ChangeStateDelay(PlayerState nextState, float delay)
    {
        StartCoroutine(ChangeStateWithDelay(nextState, delay));
    }
    
    private IEnumerator ChangeStateWithDelay(PlayerState nextState, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetState(nextState);
    }
    
    public void Dead()
    {
        // 파티클 효과 생성 및 활성화
        if (particleEffectPrefab != null)
        {
            GameObject effect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                // 파티클 시스템 루프 비활성화 - 1회만 실행되도록 설정
                var main = particles.main;
                //main.loop = false;
                
                particles.Play();
                
                // 파티클 시스템이 재생된 후 자동 삭제
                float duration = main.duration + main.startLifetimeMultiplier;
                Destroy(effect, duration);
            }
        }
        
    }
    
}