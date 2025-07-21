using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GravityGun : MonoBehaviour
{
    [Header("거리")]
    public float maxGrabDistance = 20f;    
    public float grabForce = 10f;          
    public float throwForce = 20f;         
    public float hoverDistance = 3f;       
    public LayerMask grabLayer;            
    public bool isGrab;

    [Header("무게")]
    public float massInfluenceFactor = 1f;    
    public float maxMassLimit = 10f;           
    public float minMassThreshold = 1f;         
    public float massDistanceFactor = 0.2f;     

    [Header("레이")]
    public int raysPerRing = 8;            
    public int ringCount = 2;              
    public float coneAngle = 15f;          
    public bool visualizeRays = true;      
    public Color rayColor = Color.cyan;    

    [Header("위치")]
    public Transform cameraTransform;      
    public Transform firePoint;            
    public Transform grabPoint;
    public LineRenderer gravityBeam;       
    public Material outlineMaterial;       
    [SerializeField] private PlayerController playerController;

    [Header("힘")]
    public float minThrowForce = 20f;      
    public float maxThrowForce = 60f;      
    public float chargeTime = 2f;          
    public Color chargedBeamColor = Color.red;  
    public bool visualizeChargeLevel = true;    

    
    [Header("거리 조절")]
    public float minHoverDistance = 1f;     // 최소 거리
    public float maxHoverDistance = 5f;    // 최대 거리
    public float scrollSpeed = 2f;          // 휠 스크롤 속도
    private float currentHoverDistance;  
    
    
    
    private GameObject grabbedObject;      
    private Rigidbody grabbedRigidbody;    
    private float originalDrag;            
    private float originalAngularDrag;     
    private GameObject highlightedObject;  
    private Material[] originalMaterials;  
    private float objectMass;              

    private bool originalUseGravity; 
    
    
    
    private float chargeStartTime = 0f;    
    private float currentChargeLevel = 0f; 
    private bool isCharging = false;       
    private Color originalBeamColor;
    private bool isFullyCharged = false;
    private float pulseTimer = 0f;
    private float pulseSpeed = 5f; 
    private float pulseIntensity = 0.3f; 


    
    private List<Vector3> debugRayDirections = new List<Vector3>();
    private List<Vector3> debugRayHitPoints = new List<Vector3>();

    
    
    
    [Header("모드 설정")]
    public bool is2DMode = false;  // 2D 모드 활성화 여부
    public float min2DDistance = 2f;  // 2D 모드에서 최소 거리
    public float max2DDistance = 10f; // 2D 모드에서 최대 거리
    public float mouseMoveSpeed = 5f; // 마우스 이동에 따른 물체 이동 속도
    
    // 2D 모드용 변수들
    private float objectOriginalZ; // 잡은 물체의 원래 Z 위치
    private Plane mousePlane;    
    
    
    [Header("2D 커서 설정")]
    public Image cursorImage;         // 2D 모드에서 사용할 커서 이미지
    public RectTransform cursorRect;  // 커서 이미지의 RectTransform 컴포넌트
    public bool useCursor2D = true;   // 2D 모드에서 커서 사용 여부
    public Vector2 cursorOffset = Vector2.zero;
    
    void Start()
    {
        grabLayer = LayerMask.GetMask("Grabable", "GrabableObstacle");
        if (gravityBeam != null && gravityBeam.material != null)
        {
            originalBeamColor = gravityBeam.material.GetColor("_BeamColor");
        }
        playerController.GetComponent<Transform>();

        
        gravityBeam.enabled = false;
        
        if (gravityBeam == null)
        {
            gravityBeam = gameObject.AddComponent<LineRenderer>();
            gravityBeam.startWidth = 0.01f;
            gravityBeam.endWidth = 0.01f;
            gravityBeam.enabled = false;
        }

        if (firePoint == null)
        {
            firePoint = cameraTransform;
        }

        currentHoverDistance = hoverDistance;
        isGrab = true;
        
        FindAndAssignCursor();
        
        mousePlane = new Plane(Vector3.forward, Vector3.zero);
        is2DMode = GameManager.Instance.Is2DMode();
        
        UpdateCursorVisibility();
        
    }

    void Update()
    {
        // 모드에 따라 다른 방식으로 그랩 가능한 오브젝트 체크
        if (is2DMode)
            Check2DGrabbableObject();
        else
            CheckForGrabbableObject(); // 기존 3D 콘 방식 체크

        // 오브젝트 잡기 (좌클릭)
        if (Input.GetMouseButtonDown(0) && grabbedObject == null && isGrab)
        {
            if (is2DMode)
                Try2DGrabObject();
            else
                TryGrabObject();
        }
        // 오브젝트 놓기/던지기 (좌클릭 해제)
        else if (Input.GetMouseButtonUp(0) && grabbedObject != null)
        {
            DropObject();
        }
        else if (isGrab == false && grabbedObject != null)
        {
            DropObject();
        }
        // 오브젝트 차징 던지기 시작 (우클릭 누르고 있기)
        else if (Input.GetMouseButtonDown(1) && grabbedObject != null)
        {
            StartCharging();
        }
        // 오브젝트 차징 던지기 실행 (우클릭 해제 시)
        else if (Input.GetMouseButtonUp(1) && grabbedObject != null && isCharging)
        {
            ThrowChargedObject();
        }

        // 차징 진행 중 차징 레벨 업데이트
        if (isCharging && grabbedObject != null)
        {
            UpdateChargeLevel();
        }

        // 오브젝트를 잡고 있는 경우 위치 업데이트 및 빔 효과 업데이트
        if (grabbedObject != null)
        {
            HandleScrollInput();
        
            if (is2DMode)
                Update2DGrabbedObjectPosition();
            else
                UpdateGrabbedObjectPosition();
            
            UpdateBeamEffect();
        }
        
        if (is2DMode && useCursor2D && cursorImage != null)
        {
            UpdateCursorPosition();
        }
        
    }

    
    void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        currentChargeLevel = 0f;

    }
    
    void HandleScrollInput()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            return;
        }
        
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0)
        {
            // 휠 입력에 따라 거리 조정 (양수로 변경)
            currentHoverDistance += scrollInput * scrollSpeed;
    
            // 최소/최대 거리로 제한
            currentHoverDistance = Mathf.Clamp(currentHoverDistance, minHoverDistance, maxHoverDistance);
        }
    }

    void UpdateChargeLevel()
    {
        
        float chargeElapsed = Time.time - chargeStartTime;
        currentChargeLevel = Mathf.Clamp01(chargeElapsed / chargeTime);

        
        if (visualizeChargeLevel && gravityBeam != null && gravityBeam.material != null)
        {
            
            if (currentChargeLevel >= 1.0f)
            {
                if (!isFullyCharged)
                {
                    isFullyCharged = true;
                    
                }

                
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulseValue = (Mathf.Sin(pulseTimer) + 1) * 0.5f * pulseIntensity;

                
                Color brightColor = new Color(1.0f, 0.3f, 0.3f);
                Color baseColor = chargedBeamColor; 
                Color pulseColor = Color.Lerp(baseColor, brightColor, pulseValue);

                gravityBeam.material.SetColor("_BeamColor", pulseColor);
            }
            else
            {
                
                float easedCharge = 1 - (1 - currentChargeLevel) * (1 - currentChargeLevel);

                
                Color midColor = new Color(1.0f, 0.6f, 0.0f); 

                Color lerpedColor;
                if (easedCharge < 0.5f)
                {
                    
                    lerpedColor = Color.Lerp(originalBeamColor, midColor, easedCharge * 2f);
                }
                else
                {
                    
                    lerpedColor = Color.Lerp(midColor, chargedBeamColor, (easedCharge - 0.5f) * 2f);
                }

                gravityBeam.material.SetColor("_BeamColor", lerpedColor);
                isFullyCharged = false;
            }
        }
    }

    
    void ThrowChargedObject()
    {
        if (grabbedRigidbody == null) return;
        
        
        // 물리 속성 복원
        grabbedRigidbody.linearDamping = originalDrag;
        grabbedRigidbody.angularDamping = originalAngularDrag;
        grabbedRigidbody.useGravity = originalUseGravity;

        // 차징 레벨에 따른 던지기 힘 계산
        float chargedForce = Mathf.Lerp(minThrowForce, maxThrowForce, currentChargeLevel);

        // 던지는 방향 선택 (2D/3D 모드에 따라)
        Vector3 throwDirection;
        if (is2DMode)
        {
            // 2D 모드: 그랩포인트에서 물체 방향으로 던지기
            throwDirection = (grabbedObject.transform.position - grabPoint.position).normalized;
            throwDirection.z = 0; // z축 방향 무시
            throwDirection = throwDirection.normalized; // 정규화
        }
        else
        {
            // 3D 모드: 플레이어 시선 방향으로 던지기
            throwDirection = cameraTransform.forward;
        }

        // 힘 적용하기
        grabbedRigidbody.AddForce(throwDirection * chargedForce, ForceMode.Impulse);
        // 디버그 로그
        // Debug.Log($"차징 던지기! 레벨: {currentChargeLevel:F2}, 힘: {chargedForce:F1}, 모드: {(is2DMode ? "2D" : "3D")}");

        // 변수 초기화
        grabbedObject = null;
        grabbedRigidbody = null;
        objectMass = 0f;
        isCharging = false;
        currentChargeLevel = 0f;
        
        // 이펙트 초기화
        isFullyCharged = false;
        pulseTimer = 0f;
        
        // 빔 효과 비활성화 및 색상 초기화
        gravityBeam.enabled = false;
        if (gravityBeam.material != null)
        {
            gravityBeam.material.SetColor("_BeamColor", originalBeamColor);
        }
        ImprovedSoundManager.Instance.StopSoundGroup("Beam");
    }

    
    List<Vector3> Generate3DConeDirs(Vector3 forward, float maxAngle, int ringsCount, int raysPerRing)
    {
        List<Vector3> directions = new List<Vector3>();

        
        directions.Add(forward);

        for (int ring = 1; ring <= ringsCount; ring++)
        {
            
            float ringAngle = maxAngle * ((float)ring / ringsCount);

            
            for (int i = 0; i < raysPerRing; i++)
            {
                
                float angularStep = 360f / raysPerRing;
                float currentAngle = angularStep * i;

                
                Quaternion rotation = Quaternion.AngleAxis(currentAngle, forward);

                
                Vector3 rotatedUp = rotation * Vector3.up;

                
                Quaternion tilt = Quaternion.AngleAxis(ringAngle, Vector3.Cross(forward, rotatedUp));
                Vector3 rayDirection = tilt * forward;

                directions.Add(rayDirection.normalized);
            }
        }

        return directions;
    }

    void CheckForGrabbableObject()
    {
        
        if (grabbedObject != null)
        {
            return;
        }

        
        if (highlightedObject != null)
        {
            RemoveHighlight();
        }

        
        debugRayDirections.Clear();
        debugRayHitPoints.Clear();

        
        List<Vector3> rayDirections = Generate3DConeDirs(
            firePoint.forward, coneAngle, ringCount, raysPerRing);

        
        GameObject bestTarget = null;
        float bestScore = float.MinValue; 
        RaycastHit bestHit = new RaycastHit();
        Vector3 rayOrigin = firePoint.position;
        Vector3 mainDirection = firePoint.forward;

        
        foreach (Vector3 direction in rayDirections)
        {
            debugRayDirections.Add(direction);

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, direction, out hit, maxGrabDistance, grabLayer))
            {
                
                float angleScore = 1.0f - (Vector3.Angle(mainDirection, direction) / coneAngle);

                
                float distanceToGrabPoint = Vector3.Distance(hit.point, grabPoint.position);
                float distanceScore = 1.0f - (distanceToGrabPoint / maxGrabDistance);

                
                float angleWeight = 0.5f; 
                float distanceWeight = 0.5f; 
                float score = (angleScore * angleWeight) + (distanceScore * distanceWeight);

                
                if (score > bestScore)
                {
                    bestTarget = hit.collider.gameObject;
                    bestScore = score;
                    bestHit = hit;
                }

                debugRayHitPoints.Add(hit.point);
            }
            else
            {
                debugRayHitPoints.Add(rayOrigin + direction * maxGrabDistance);
            }
        }

        
        if (bestTarget != null)
        {
            highlightedObject = bestTarget;
            ApplyHighlight();
        }
    }
    
    
    
    
    void ApplyHighlight()
    {
        if (highlightedObject != null && outlineMaterial != null)
        {
            Renderer renderer = highlightedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                
                originalMaterials = renderer.materials;

                
                Material[] newMaterials = new Material[originalMaterials.Length + 1];
                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    newMaterials[i] = originalMaterials[i];
                }
                newMaterials[originalMaterials.Length] = outlineMaterial;

                
                renderer.materials = newMaterials;
            }
        }
    }

    
    void RemoveHighlight()
    {
        if (highlightedObject != null)
        {
            Renderer renderer = highlightedObject.GetComponent<Renderer>();
            if (renderer != null && originalMaterials != null)
            {
                
                renderer.materials = originalMaterials;
            }

            highlightedObject = null;
            originalMaterials = null;
        }
    }

    
    void TryGrabObject()
    {
        
        if (highlightedObject != null)
        {
            Rigidbody rb = highlightedObject.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ImprovedSoundManager.Instance.PlaySound3D("Beam",transform.position);
                grabbedObject = highlightedObject;
                grabbedRigidbody = rb;

                currentHoverDistance = hoverDistance;
                
                originalDrag = grabbedRigidbody.linearDamping;
                originalAngularDrag = grabbedRigidbody.angularDamping;

                
                objectMass = grabbedRigidbody.mass;

                
                grabbedRigidbody.linearDamping = 10f;
                grabbedRigidbody.angularDamping = 10f;
                originalUseGravity= grabbedRigidbody.useGravity;
                grabbedRigidbody.useGravity = false;

                
                RemoveHighlight();

                
                gravityBeam.enabled = true;

                
            }
        }
    }

    
    void UpdateGrabbedObjectPosition()
    {
        if (grabbedRigidbody == null) return;

        // 질량에 따른 거리 조정 계산
        float massRatio = Mathf.Clamp(objectMass - minMassThreshold, 0f, maxMassLimit) / maxMassLimit;
        float massFactor = 1f - (massRatio * massInfluenceFactor); 
        float adjustedDistance = currentHoverDistance * (1f + (massRatio * massDistanceFactor));

        // 질량에 따른 높이 조정
        float heightScale = Mathf.InverseLerp(minMassThreshold, maxMassLimit, objectMass); 
        float heightAdjustment = Mathf.Lerp(2f, 0.5f, heightScale); 
    
        // 여기를 cameraTransform.forward로 변경!
        Vector3 targetPosition = grabPoint.position +
                                 cameraTransform.forward * adjustedDistance +  // 변경된 부분
                                 cameraTransform.up * heightAdjustment;        // 변경된 부분

        // 현재 위치에서 목표 위치까지의 방향과 거리 계산
        Vector3 direction = targetPosition - grabbedObject.transform.position;
        float distance = direction.magnitude;

        // 힘의 크기 계산 (질량 고려)
        float forceMagnitude = Mathf.Clamp(distance * grabForce * massFactor, 0, 20f);

        // 힘 적용
        grabbedRigidbody.AddForce(direction.normalized * forceMagnitude, ForceMode.Acceleration);

        // 최대 속도 제한
        float maxVelocity = 10f * massFactor;
        if (grabbedRigidbody.linearVelocity.magnitude > maxVelocity)
        {
            grabbedRigidbody.linearVelocity = grabbedRigidbody.linearVelocity.normalized * maxVelocity;
        }

        // 회전 안정화
        float rotationStabilization = 5f * massFactor;
        grabbedRigidbody.angularVelocity = Vector3.Lerp(
            grabbedRigidbody.angularVelocity,
            Vector3.zero,
            Time.deltaTime * rotationStabilization
        );
    }

    
    void UpdateBeamEffect()
    {
        if (gravityBeam != null && grabbedObject != null)
        {
            
            gravityBeam.SetPosition(0, grabPoint.position);
            gravityBeam.SetPosition(1, grabbedObject.transform.position);
        }
    }

    
    void DropObject()
    {
        if (grabbedRigidbody == null) return;

        
        
        
        grabbedRigidbody.linearDamping = originalDrag;
        grabbedRigidbody.angularDamping = originalAngularDrag;
        grabbedRigidbody.useGravity = originalUseGravity;

        
        float massRatio = Mathf.Clamp01(objectMass / maxMassLimit);
        float velocityDamping = Mathf.Lerp(0.3f, 0.5f, massRatio);

        grabbedRigidbody.linearVelocity *= velocityDamping;
        grabbedRigidbody.angularVelocity *= velocityDamping;

        
        isCharging = false;
        currentChargeLevel = 0f;

        
        grabbedObject = null;
        grabbedRigidbody = null;
        objectMass = 0f;

        
        isFullyCharged = false;
        pulseTimer = 0f;

        
        gravityBeam.enabled = false;
        if (gravityBeam.material != null)
        {
            gravityBeam.material.SetColor("_BeamColor", originalBeamColor);
        }
        
        ImprovedSoundManager.Instance.StopSoundGroup("Beam");

    }

    #region 2DGravityGun

    void Check2DGrabbableObject()
    {
        // 이미 오브젝트를 잡고 있는 경우 검사 필요 없음
        if (grabbedObject != null)
        {
            return;
        }

        // 기존 하이라이트 제거
        if (highlightedObject != null)
        {
            RemoveHighlight();
        }

        // 마우스 위치에서 레이 생성
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGrabDistance, grabLayer))
        {
            // 오브젝트를 찾았으면 하이라이트 적용
            highlightedObject = hit.collider.gameObject;
            ApplyHighlight();
            
            // 디버그용
            Debug.DrawLine(ray.origin, hit.point, Color.green);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * maxGrabDistance, Color.red);
        }
    }

    // 2D 모드에서 오브젝트 잡기 시도
    void Try2DGrabObject()
    {
        if (highlightedObject != null)
        {
            Rigidbody rb = highlightedObject.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                ImprovedSoundManager.Instance.PlaySound3D("Beam",transform.position);
                // 오브젝트 잡기
                grabbedObject = highlightedObject;
                grabbedRigidbody = rb;
                

                
                // 물리 속성 저장 및 변경
                originalDrag = grabbedRigidbody.linearDamping;
                originalAngularDrag = grabbedRigidbody.angularDamping;
                originalUseGravity = grabbedRigidbody.useGravity;
                // 오브젝트의 질량 저장
                objectMass = grabbedRigidbody.mass;
                
                // 2D 모드용 Z 위치 저장
                objectOriginalZ = grabbedObject.transform.position.z;

                // 잡은 오브젝트에 물리 특성 적용
                grabbedRigidbody.linearDamping = 10f;
                grabbedRigidbody.angularDamping = 10f;
                grabbedRigidbody.useGravity = false;

                // 하이라이트 제거 (이미 잡았으므로)
                RemoveHighlight();

                // 빔 효과 활성화
                gravityBeam.enabled = true;

                
            }
        }
    }

    // 2D 모드에서 잡은 오브젝트 위치 업데이트
    void Update2DGrabbedObjectPosition()
    {
        if (grabbedRigidbody == null) return;

        // 마우스 위치를 월드 좌표로 변환
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        Vector3 targetPosition;
        
        // 마우스 평면과 레이의 교차점 계산
        if (mousePlane.Raycast(mouseRay, out rayDistance))
        {
            Vector3 mouseWorldPosition = mouseRay.GetPoint(rayDistance);
            
            // 플레이어(그랩포인트)로부터의 방향 벡터 계산
            Vector3 directionFromGrabPoint = mouseWorldPosition - grabPoint.position;
            directionFromGrabPoint.z = 0; // Z 방향은 무시 (XY 평면으로 제한)
            
            // 거리 제한 적용
            float currentDistance = directionFromGrabPoint.magnitude;
            
            // 최소/최대 거리 강제 적용
            if (currentDistance < min2DDistance)
            {
                directionFromGrabPoint = directionFromGrabPoint.normalized * min2DDistance;
            }
            else if (currentDistance > max2DDistance)
            {
                directionFromGrabPoint = directionFromGrabPoint.normalized * max2DDistance;
            }
            
            // 최종 목표 위치 계산 (그랩포인트 기준)
            targetPosition = grabPoint.position + directionFromGrabPoint;
            targetPosition.z = objectOriginalZ; // Z 위치 고정
            
            // 현재 위치에서 목표 위치까지의 거리 계산
            Vector3 currentPosition = grabbedObject.transform.position;
            float distanceToTarget = Vector3.Distance(
                new Vector3(currentPosition.x, currentPosition.y, 0),
                new Vector3(targetPosition.x, targetPosition.y, 0)
            );
            
            // 마우스가 움직이지 않거나 물체가 목표 위치에 거의 도달한 경우
            if (distanceToTarget < 0.05f)
            {
                // 속도를 크게 감소시키고 직접 위치 보정
                grabbedRigidbody.linearVelocity = Vector3.Lerp(grabbedRigidbody.linearVelocity, Vector3.zero, 0.8f);
                grabbedRigidbody.angularVelocity = Vector3.Lerp(grabbedRigidbody.angularVelocity, Vector3.zero, 0.8f);
                
                // 현재 위치 유지 (약간의 보정만)
                Vector3 newPosition = currentPosition;
                newPosition.x = Mathf.Lerp(currentPosition.x, targetPosition.x, 0.5f * Time.deltaTime);
                newPosition.y = Mathf.Lerp(currentPosition.y, targetPosition.y, 0.5f * Time.deltaTime);
                newPosition.z = objectOriginalZ;
                
                // 위치 직접 설정 (물리 시스템 우회)
                if (Vector3.Distance(currentPosition, newPosition) > 0.001f)
                {
                    grabbedRigidbody.position = newPosition;
                }
            }
            else
            {
                // 움직임이 필요한 경우 - 질량 기반 가중치 적용
                float massRatio = Mathf.Clamp(objectMass - minMassThreshold, 0f, maxMassLimit) / maxMassLimit;
                float massFactor = 1f - (massRatio * massInfluenceFactor);
                
                // 현재 위치에서 목표 위치로의 방향
                Vector3 direction = targetPosition - currentPosition;
                direction.z = 0; // Z 방향으로의 힘 무시
                
                // 거리에 따라 힘 증가 (멀수록 더 강하게)
                float distanceFactor = Mathf.Clamp01(distanceToTarget / 2.0f) + 0.5f;
                float forceMagnitude = grabForce * massFactor * distanceFactor * mouseMoveSpeed;
                
                // 힘 적용
                grabbedRigidbody.AddForce(direction.normalized * forceMagnitude, ForceMode.Acceleration);
                
                // 속도 제한 및 z축 속도 0으로 유지
                Vector3 currentVelocity = grabbedRigidbody.linearVelocity;
                currentVelocity.z = 0;
                
                float maxVelocity = 10f * massFactor;
                if (currentVelocity.magnitude > maxVelocity)
                {
                    currentVelocity = currentVelocity.normalized * maxVelocity;
                }
                
                grabbedRigidbody.linearVelocity = currentVelocity;
            }
            
            // 회전 안정화
            grabbedRigidbody.angularVelocity = Vector3.Lerp(
                grabbedRigidbody.angularVelocity,
                Vector3.zero,
                Time.deltaTime * 8f
            );
            
            // 항상 z 위치 강제 고정
            currentPosition = grabbedObject.transform.position;
            if (Mathf.Abs(currentPosition.z - objectOriginalZ) > 0.001f)
            {
                currentPosition.z = objectOriginalZ;
                grabbedObject.transform.position = currentPosition;
            }
        }
    }
    

    #endregion

    #region 2DCursor

    void UpdateCursorPosition()
    {
        if (cursorRect != null)
        {
            // 마우스 위치를 화면 좌표로 가져오기
            Vector2 mousePosition = Input.mousePosition;
        
            // 오프셋 적용 (필요한 경우)
            mousePosition += cursorOffset;
        
            // UI 커서 위치 업데이트
            cursorRect.position = mousePosition;
        }
    }

// OnEnable/OnDisable 이벤트에 추가할 부분 (새로 추가)
    void OnEnable()
    {
        // 2D 모드에서 실제 커서 숨기기 (선택사항)
        if (is2DMode && useCursor2D && cursorImage != null)
        {
            Cursor.visible = false;
        }
    }

    void OnDisable()
    {
        // 실제 커서 다시 표시 (선택사항)
        Cursor.visible = true;
    }

// GameManager의 모드 변경에 대응하는 함수 (선택사항)
    public void OnModeChanged(bool is2DModeNew)
    {
        is2DMode = is2DModeNew;
        UpdateCursorVisibility();
    }

    private void FindAndAssignCursor()
    {
        // 이미 할당된 경우 건너뛰기
        if (cursorImage != null && cursorRect != null)
            return;
        
        // 씬에서 "Cursor"라는 이름의 GameObject 찾기
        GameObject cursorObj = GameObject.Find("Cursor");
    
        // 못 찾았다면 모든 Canvas 내부에서 "Cursor"라는 이름의 오브젝트 검색
        if (cursorObj == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                // Canvas의 자식들 중에서 "Cursor" 검색
                Transform cursorTransform = canvas.transform.Find("Cursor");
                if (cursorTransform != null)
                {
                    cursorObj = cursorTransform.gameObject;
                    break;
                }
            }
        }
    
        // 커서 오브젝트를 찾았으면 컴포넌트 할당
        if (cursorObj != null)
        {
            if (cursorImage == null)
                cursorImage = cursorObj.GetComponent<Image>();
            
            if (cursorRect == null && cursorImage != null)
                cursorRect = cursorImage.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("UI에서 'Cursor' 이름의 이미지를 찾을 수 없습니다. 수동으로 할당해주세요.");
        }
    }

// 커서 가시성 업데이트하는 함수 (코드 중복 방지용)
    private void UpdateCursorVisibility()
    {
        if (is2DMode && useCursor2D && cursorImage != null)
        {
            cursorImage.gameObject.SetActive(true);
            Cursor.visible = false;
        }
        else if (cursorImage != null)
        {
            cursorImage.gameObject.SetActive(false);
            Cursor.visible = true;
        }
    }

// OnModeChanged 함수 수정
    
    
    #endregion
}