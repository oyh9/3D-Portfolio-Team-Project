using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// 프리팹을 격자에 맞춰 배치하는 에디터 도구 (Z위치 설정 가능)
public class PrefabPlacementEditorTool : EditorWindow
{
    // 설정
    private GameObject prefabToPlace;
    private float gridSize = 1f;
    private Transform parentTransform;
    private Color gridColor = new Color(0, 0.8f, 0.2f, 0.2f);
    private Color previewColor = new Color(0, 1f, 0.5f, 0.5f);
    
    // Z축 위치 설정
    private float zPosition = 0f;
    private float zStep = 0.5f;
    
    // 배치 모드
    private bool isPlacingMode = false;
    private bool isRotationMode = false;
    private float rotationAngle = 0f;
    
    // 콜라이더 스냅 모드 추가
    private bool isColliderSnapMode = false;
    private Color colliderSnapColor = new Color(1f, 0.5f, 0f, 0.5f);
    
    // 배치된 프리팹 관리
    private List<GameObject> placedPrefabs = new List<GameObject>();
    
    // 프리팹 정보
    private Vector3 prefabSize = Vector3.one;
    private Vector3 prefabCenter = Vector3.zero;
    private Bounds prefabBounds = new Bounds();
    
    // 스크롤 뷰 위치
    private Vector2 scrollPosition;
    
    // 에디터 창 메뉴 항목
    [MenuItem("Tools/Prefab Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPlacementEditorTool>("프리팹 배치 툴");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Undo.undoRedoPerformed += OnUndoRedo;
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Undo.undoRedoPerformed -= OnUndoRedo;
        isPlacingMode = false;
        isRotationMode = false;
        isColliderSnapMode = false;
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // 기본 설정 섹션
        EditorGUILayout.LabelField("프리팹 배치 설정", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 프리팹 설정
        EditorGUI.BeginChangeCheck();
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("배치할 프리팹", prefabToPlace, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck() && prefabToPlace != null)
        {
            // 프리팹 크기 계산
            CalculatePrefabSize();
        }
        
        // 부모 오브젝트 설정
        parentTransform = (Transform)EditorGUILayout.ObjectField("부모 오브젝트", parentTransform, typeof(Transform), true);
        
        // 격자 크기 설정
        gridSize = EditorGUILayout.FloatField("격자 크기", gridSize);
        if (gridSize <= 0) gridSize = 0.1f;
        
        // Z축 위치 설정
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Z축 위치 설정", EditorStyles.boldLabel);
        
        // Z 위치 슬라이더
        zPosition = EditorGUILayout.FloatField("Z 위치", zPosition);
        
        // Z 위치 조절 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-", GUILayout.Width(30)))
        {
            zPosition -= zStep;
        }
        
        // Z 스텝 조절
        zStep = EditorGUILayout.FloatField("Z 스텝", zStep);
        if (zStep <= 0) zStep = 0.1f;
        
        if (GUILayout.Button("+", GUILayout.Width(30)))
        {
            zPosition += zStep;
        }
        EditorGUILayout.EndHorizontal();
        
        // 현재 Z 레이어 표시
        EditorGUILayout.LabelField($"현재 Z 레이어: {zPosition}", EditorStyles.boldLabel);
        
        // 색상 설정
        EditorGUILayout.Space();
        gridColor = EditorGUILayout.ColorField("격자 색상", gridColor);
        previewColor = EditorGUILayout.ColorField("미리보기 색상", previewColor);
        colliderSnapColor = EditorGUILayout.ColorField("콜라이더 스냅 색상", colliderSnapColor);
        
        EditorGUILayout.Space();
        
        // 회전 설정
        EditorGUILayout.LabelField("회전 설정", EditorStyles.boldLabel);
        
        // 90도 단위 회전 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("0°"))
        {
            rotationAngle = 0f;
        }
        if (GUILayout.Button("90°"))
        {
            rotationAngle = 90f;
        }
        if (GUILayout.Button("180°"))
        {
            rotationAngle = 180f;
        }
        if (GUILayout.Button("270°"))
        {
            rotationAngle = 270f;
        }
        EditorGUILayout.EndHorizontal();
        
        // 회전 각도 슬라이더
        rotationAngle = EditorGUILayout.Slider("회전 각도", rotationAngle, 0f, 359f);
        
        EditorGUILayout.Space();
        
        // 현재 프리팹 정보 표시
        if (prefabToPlace != null)
        {
            EditorGUILayout.LabelField("프리팹 정보", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"크기: {prefabSize.x}x{prefabSize.y}x{prefabSize.z}");
            EditorGUILayout.LabelField($"중심점: {prefabCenter}");
            
            // 콜라이더 정보 표시
            Collider prefabCollider = prefabToPlace.GetComponentInChildren<Collider>();
            if (prefabCollider != null)
            {
                EditorGUILayout.LabelField("콜라이더 정보:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"타입: {prefabCollider.GetType().Name}");
                EditorGUILayout.LabelField($"바운드: {prefabCollider.bounds.size}");
            }
            else
            {
                EditorGUILayout.HelpBox("프리팹에 콜라이더가 없습니다. 콜라이더 스냅 모드를 사용하려면 콜라이더가 필요합니다.", MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space();
        
        // 배치 모드 섹션
        EditorGUILayout.LabelField("배치 모드", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        // 배치 모드 토글 버튼
        GUI.backgroundColor = isPlacingMode ? Color.green : Color.white;
        if (GUILayout.Button("배치 모드", GUILayout.Height(30)))
        {
            isPlacingMode = !isPlacingMode;
            SceneView.RepaintAll();
        }
        
        // 회전 모드 토글 버튼
        GUI.backgroundColor = isRotationMode ? Color.yellow : Color.white;
        if (GUILayout.Button("회전 모드", GUILayout.Height(30)))
        {
            isRotationMode = !isRotationMode;
            SceneView.RepaintAll();
        }
        
        // 콜라이더 스냅 모드 버튼 추가
        GUI.backgroundColor = isColliderSnapMode ? new Color(1f, 0.5f, 0f) : Color.white;
        if (GUILayout.Button("콜라이더 스냅 모드", GUILayout.Height(30)))
        {
            isColliderSnapMode = !isColliderSnapMode;
            SceneView.RepaintAll();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // 배치된 프리팹 목록
        EditorGUILayout.LabelField($"배치된 프리팹: {placedPrefabs.Count}개", EditorStyles.boldLabel);
        
        if (GUILayout.Button("마지막 프리팹 삭제", GUILayout.Height(25)))
        {
            DeleteLastPrefab();
        }
        
        if (GUILayout.Button("모든 프리팹 삭제", GUILayout.Height(25)))
        {
            DeleteAllPrefabs();
        }
        
        // 도움말 표시
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "사용법:\n" +
            "1. 배치할 프리팹과 부모 오브젝트를 선택합니다.\n" +
            "2. 격자 크기와 Z 위치를 설정합니다.\n" +
            "3. 회전 각도를 설정합니다.\n" +
            "4. '배치 모드'를 켜고 씬 뷰에서 클릭하여 프리팹을 배치합니다.\n" +
            "5. '콜라이더 스냅 모드'를 켜면 기존 오브젝트의 콜라이더에 프리팹을 붙여서 배치할 수 있습니다.\n" +
            "6. Z 위치는 +/- 버튼이나 직접 값을 입력하여 조절할 수 있습니다.\n" +
            "7. Page Up/Down 키로 Z 위치를 조절할 수 있습니다.\n" +
            "8. 'Delete' 키로 마지막 배치한 프리팹을 삭제할 수 있습니다.",
            MessageType.Info);
        
        EditorGUILayout.EndScrollView();
        
        if (GUI.changed)
        {
            Repaint();
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacingMode || prefabToPlace == null) return;
        
        Event e = Event.current;
        Camera sceneCamera = sceneView.camera;
        
        // 마우스 위치가 씬 뷰 내에 있는지 확인
        Vector3 mousePosition = e.mousePosition;
        if (IsMouseOutsideSceneView(mousePosition, sceneView)) return;
        
        // 마우스 위치를 월드 좌표로 변환
        Vector3 worldPoint = GetWorldPointFromMouse(mousePosition, sceneCamera, sceneView);
        
        // 배치 위치 계산 (격자 또는 콜라이더 스냅)
        Vector3 placementPosition;
        GameObject snappedObject = null;
        
        if (isColliderSnapMode)
        {
            // 콜라이더 스냅 모드일 때는 레이캐스트로 가장 가까운 콜라이더 찾기
            if (TryGetColliderSnapPosition(worldPoint, sceneCamera, out placementPosition, out snappedObject))
            {
                // 콜라이더에 스냅된 위치 사용
            }
            else
            {
                // 스냅할 콜라이더를 찾지 못하면 격자 위치 사용
                placementPosition = GetSnappedPosition(worldPoint);
            }
        }
        else
        {
            // 일반 모드에서는 격자에 스냅된 위치 사용
            placementPosition = GetSnappedPosition(worldPoint);
        }
        
        // Z 위치 설정 (콜라이더 스냅이 없는 경우에만)
        if (snappedObject == null)
        {
            placementPosition.z = zPosition;
        }
        
        // 프리팹 회전 계산
        Quaternion prefabRotation = Quaternion.Euler(0, 0, rotationAngle);
        
        // 키보드 단축키 처리
        HandleKeyboardShortcuts(e);
        
        // 격자 그리기 (콜라이더 스냅 모드가 아닐 때만)
        if (!isColliderSnapMode)
        {
            DrawGrid(placementPosition, 10);
        }
        
        // 프리팹 미리보기 그리기 (색상은 모드에 따라 다르게)
        Color previewColorToUse = isColliderSnapMode && snappedObject != null ? colliderSnapColor : previewColor;
        DrawPrefabPreview(placementPosition, prefabRotation, previewColorToUse);
        
        // 마우스 클릭 처리
        if (e.type == EventType.MouseDown && e.button == 0) // 왼쪽 마우스 버튼
        {
            if (isPlacingMode)
            {
                PlacePrefab(placementPosition, prefabRotation);
                e.Use(); // 이벤트 소비
            }
        }
        
        // 씬 뷰 갱신
        if (e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        
        SceneView.RepaintAll();
    }
    
    // 콜라이더 스냅 위치 계산 (새로 추가)
    private bool TryGetColliderSnapPosition(Vector3 worldPoint, Camera sceneCamera, out Vector3 snapPosition, out GameObject snappedObject)
    {
        snapPosition = worldPoint;
        snappedObject = null;
        
        // 레이캐스트로 가장 가까운 콜라이더 찾기
        Ray ray = sceneCamera.ScreenPointToRay(HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition));
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        
        if (hits.Length == 0) return false;
        
        // 이미 배치된 프리팹 중 가장 가까운 것 찾기
        float closestDistance = float.MaxValue;
        RaycastHit closestHit = new RaycastHit();
        bool foundValidHit = false;
        
        foreach (RaycastHit hit in hits)
        {
            // 자신(프리팹 미리보기)은 제외
            if (hit.collider.gameObject == prefabToPlace) continue;
            
            // 이미 배치된 프리팹에 속하는지 확인
            bool isPlacedPrefab = false;
            foreach (GameObject placedPrefab in placedPrefabs)
            {
                if (placedPrefab == hit.collider.gameObject || 
                    (placedPrefab != null && hit.collider.transform.IsChildOf(placedPrefab.transform)))
                {
                    isPlacedPrefab = true;
                    break;
                }
            }
            
            if (!isPlacedPrefab) continue;
            
            // 가장 가까운 히트 선택
            float distance = hit.distance;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestHit = hit;
                foundValidHit = true;
            }
        }
        
        if (!foundValidHit) return false;
        
        // 프리팹의 콜라이더 정보 가져오기
        Collider prefabCollider = prefabToPlace.GetComponentInChildren<Collider>();
        if (prefabCollider == null) return false;
        
        // 히트된 오브젝트의 콜라이더 정보
        Collider hitCollider = closestHit.collider;
        
        // 스냅된 오브젝트 설정
        snappedObject = hitCollider.gameObject;
        
        // 콜라이더 유형에 따라 다르게 처리
        if (hitCollider is BoxCollider)
        {
            // 박스 콜라이더에 스냅
            BoxCollider boxCollider = (BoxCollider)hitCollider;
            
            // 히트 포인트의 로컬 좌표 계산
            Vector3 localHitPoint = hitCollider.transform.InverseTransformPoint(closestHit.point);
            
            // 히트 포인트가 박스의 어느 면에 가장 가까운지 계산
            Vector3 boxExtents = boxCollider.size * 0.5f;
            float[] distances = new float[6];
            distances[0] = boxExtents.x - Mathf.Abs(localHitPoint.x - boxExtents.x); // 오른쪽
            distances[1] = boxExtents.x - Mathf.Abs(localHitPoint.x + boxExtents.x); // 왼쪽
            distances[2] = boxExtents.y - Mathf.Abs(localHitPoint.y - boxExtents.y); // 위
            distances[3] = boxExtents.y - Mathf.Abs(localHitPoint.y + boxExtents.y); // 아래
            distances[4] = boxExtents.z - Mathf.Abs(localHitPoint.z - boxExtents.z); // 앞
            distances[5] = boxExtents.z - Mathf.Abs(localHitPoint.z + boxExtents.z); // 뒤
            
            int closestFace = 0;
            float minDistance = distances[0];
            for (int i = 1; i < 6; i++)
            {
                if (distances[i] < minDistance)
                {
                    minDistance = distances[i];
                    closestFace = i;
                }
            }
            
            // 프리팹 위치 계산
            Vector3 direction = Vector3.zero;
            switch (closestFace)
            {
                case 0: direction = new Vector3(1, 0, 0); break; // 오른쪽
                case 1: direction = new Vector3(-1, 0, 0); break; // 왼쪽
                case 2: direction = new Vector3(0, 1, 0); break; // 위
                case 3: direction = new Vector3(0, -1, 0); break; // 아래
                case 4: direction = new Vector3(0, 0, 1); break; // 앞
                case 5: direction = new Vector3(0, 0, -1); break; // 뒤
            }
            
            // 로컬 좌표에서 스냅 위치 계산
            Vector3 localSnapPosition = boxCollider.center + Vector3.Scale(boxExtents, direction);
            
            // 로컬 좌표를 월드 좌표로 변환
            Vector3 worldSnapPosition = hitCollider.transform.TransformPoint(localSnapPosition);
            
            // 프리팹의 콜라이더 크기를 고려하여 오프셋 계산
            if (prefabCollider is BoxCollider)
            {
                BoxCollider prefabBoxCollider = (BoxCollider)prefabCollider;
                Vector3 prefabExtents = Vector3.Scale(prefabBoxCollider.size, prefabToPlace.transform.lossyScale) * 0.5f;
                
                // 회전을 고려한 오프셋 계산
                Quaternion prefabRotation = Quaternion.Euler(0, 0, rotationAngle);
                Vector3 offset = prefabRotation * Vector3.Scale(prefabExtents, direction);
                
                // 최종 스냅 위치 계산
                snapPosition = worldSnapPosition + offset;
            }
            else
            {
                // 다른 유형의 콜라이더인 경우 간단하게 처리
                snapPosition = worldSnapPosition;
            }
            
            return true;
        }
        else if (hitCollider is SphereCollider)
        {
            // 구 콜라이더에 스냅
            SphereCollider sphereCollider = (SphereCollider)hitCollider;
            
            // 구의 중심에서 히트 포인트 방향으로의 벡터 계산
            Vector3 sphereCenter = hitCollider.transform.TransformPoint(sphereCollider.center);
            Vector3 direction = (closestHit.point - sphereCenter).normalized;
            
            // 구의 표면 위치 계산
            Vector3 surfacePoint = sphereCenter + direction * sphereCollider.radius * hitCollider.transform.lossyScale.x;
            
            // 프리팹의 콜라이더 크기를 고려하여 오프셋 계산
            if (prefabCollider is BoxCollider)
            {
                BoxCollider prefabBoxCollider = (BoxCollider)prefabCollider;
                
                // 방향에 따른 프리팹 콜라이더의 크기 계산
                float prefabExtent = 0f;
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
                {
                    prefabExtent = prefabBoxCollider.size.x * 0.5f * prefabToPlace.transform.lossyScale.x;
                }
                else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) && Mathf.Abs(direction.y) > Mathf.Abs(direction.z))
                {
                    prefabExtent = prefabBoxCollider.size.y * 0.5f * prefabToPlace.transform.lossyScale.y;
                }
                else
                {
                    prefabExtent = prefabBoxCollider.size.z * 0.5f * prefabToPlace.transform.lossyScale.z;
                }
                
                snapPosition = surfacePoint + direction * prefabExtent;
            }
            else
            {
                snapPosition = surfacePoint;
            }
            
            return true;
        }
        else if (hitCollider is CapsuleCollider)
        {
            // 캡슐 콜라이더에 스냅 (간단한 처리)
            snapPosition = closestHit.point + closestHit.normal * 0.1f;
            return true;
        }
        else if (hitCollider is MeshCollider)
        {
            // 메시 콜라이더에 스냅
            snapPosition = closestHit.point + closestHit.normal * 0.1f;
            return true;
        }
        
        return false;
    }
    
    // 프리팹 미리보기 그리기 (색상 매개변수 추가)
    private void DrawPrefabPreview(Vector3 position, Quaternion rotation, Color color)
    {
        if (prefabToPlace == null) return;
        
        // 프리팹의 바운딩 박스 표시
        Handles.color = color;
        
        // 회전을 적용한 바운딩 박스 계산
        Vector3 size = prefabSize;
        Vector3 center = position + rotation * prefabCenter;
        
        // 바운딩 박스의 각 꼭지점 좌표
        Vector3[] corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            // 각 꼭지점의 로컬 좌표 계산
            corners[i] = new Vector3(
                ((i & 1) == 0 ? -size.x : size.x) * 0.5f,
                ((i & 2) == 0 ? -size.y : size.y) * 0.5f,
                ((i & 4) == 0 ? -size.z : size.z) * 0.5f
            );
            
            // 회전 적용
            corners[i] = rotation * corners[i];
            
            // 중심점에 상대적인 위치로 변환
            corners[i] += center;
        }
        
        // 각 모서리를 연결하는 선 그리기
        for (int i = 0; i < 4; i++)
        {
            Handles.DrawLine(corners[i], corners[i + 4]); // 앞뒤 연결
            Handles.DrawLine(corners[i], corners[(i + 1) % 4]); // 앞면 연결
            Handles.DrawLine(corners[i + 4], corners[((i + 1) % 4) + 4]); // 뒷면 연결
        }
        
        // 콜라이더 스냅 모드일 때 추가 정보 표시
        if (isColliderSnapMode)
        {
            Handles.Label(center + new Vector3(0, size.y * 0.5f, 0), "콜라이더 스냅 모드", EditorStyles.boldLabel);
        }
    }
    
    // 키보드 단축키 처리
    private void HandleKeyboardShortcuts(Event e)
    {
        if (e.type == EventType.KeyDown)
        {
            if (isRotationMode)
            {
                if (e.keyCode == KeyCode.Q) // 반시계방향 회전
                {
                    rotationAngle = (rotationAngle + 90) % 360;
                    Repaint();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.E) // 시계방향 회전
                {
                    rotationAngle = (rotationAngle - 90 + 360) % 360;
                    Repaint();
                    e.Use();
                }
            }
            
            // Z 위치 조절
            if (e.keyCode == KeyCode.PageUp) // Z 위치 증가
            {
                zPosition += zStep;
                Repaint();
                e.Use();
            }
            else if (e.keyCode == KeyCode.PageDown) // Z 위치 감소
            {
                zPosition -= zStep;
                Repaint();
                e.Use();
            }
            
            // Delete 키로 마지막 프리팹 삭제
            if (e.keyCode == KeyCode.Delete)
            {
                DeleteLastPrefab();
                e.Use();
            }
            
            // C 키로 콜라이더 스냅 모드 토글
            if (e.keyCode == KeyCode.C)
            {
                isColliderSnapMode = !isColliderSnapMode;
                Repaint();
                e.Use();
            }
        }
    }
    
    // 마우스가 씬 뷰 밖에 있는지 확인
    private bool IsMouseOutsideSceneView(Vector3 mousePosition, SceneView sceneView)
    {
        return mousePosition.x < 0 || mousePosition.x > sceneView.position.width ||
               mousePosition.y < 0 || mousePosition.y > sceneView.position.height;
    }
    
    // 마우스 위치를 월드 좌표로 변환
    private Vector3 GetWorldPointFromMouse(Vector3 mousePosition, Camera sceneCamera, SceneView sceneView)
    {
        // 씬 뷰 내 마우스 위치 비율 계산 (0~1)
        mousePosition.y = sceneView.position.height - mousePosition.y; // Y 좌표 반전
        Vector3 viewportPoint = new Vector3(
            mousePosition.x / sceneView.position.width,
            mousePosition.y / sceneView.position.height,
            0);
        
        // 평면에 투영된 위치 계산 (Z=0 평면)
        Ray ray = sceneCamera.ViewportPointToRay(viewportPoint);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }
    
    // 월드 좌표를 격자에 스냅
    private Vector3 GetSnappedPosition(Vector3 worldPoint)
    {
        return new Vector3(
            Mathf.Round(worldPoint.x / gridSize) * gridSize,
            Mathf.Round(worldPoint.y / gridSize) * gridSize,
            zPosition // Z는 사용자가 설정한 값 사용
        );
    }
    
    // 프리팹 크기 계산
    private void CalculatePrefabSize()
    {
        if (prefabToPlace == null) return;
        
        // 렌더러를 사용하여 프리팹 크기 계산
        Renderer[] renderers = prefabToPlace.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            // 모든 렌더러의 바운드를 합친 전체 바운드 계산
            Bounds totalBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                totalBounds.Encapsulate(renderers[i].bounds);
            }
            
            // 로컬 좌표계로 변환
            prefabSize = totalBounds.size;
            prefabCenter = totalBounds.center - prefabToPlace.transform.position;
            prefabBounds = totalBounds;
        }
        else
        {
            // 렌더러가 없는 경우 기본값 사용
            prefabSize = Vector3.one;
            prefabCenter = Vector3.zero;
            prefabBounds = new Bounds(Vector3.zero, Vector3.one);
        }
        
        // 콜라이더 정보 추가 확인
        Collider[] colliders = prefabToPlace.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            // 모든 콜라이더의 바운드를 합친 전체 바운드 계산
            Bounds colliderBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                colliderBounds.Encapsulate(colliders[i].bounds);
            }
            
            // 렌더러와 콜라이더 중 더 큰 바운드 사용
            if (colliderBounds.size.magnitude > prefabBounds.size.magnitude)
            {
                prefabSize = colliderBounds.size;
                prefabCenter = colliderBounds.center - prefabToPlace.transform.position;
                prefabBounds = colliderBounds;
            }
        }
    }
    
    // 격자 그리기
    private void DrawGrid(Vector3 center, int cellCount)
    {
        Handles.color = gridColor;
        
        float halfSize = cellCount * gridSize * 0.5f;
        
        // 수평선 그리기
        for (int i = -cellCount; i <= cellCount; i++)
        {
            float y = center.y + i * gridSize;
            Handles.DrawLine(
                new Vector3(center.x - halfSize, y, center.z),
                new Vector3(center.x + halfSize, y, center.z)
            );
        }
        
        // 수직선 그리기
        for (int i = -cellCount; i <= cellCount; i++)
        {
            float x = center.x + i * gridSize;
            Handles.DrawLine(
                new Vector3(x, center.y - halfSize, center.z),
                new Vector3(x, center.y + halfSize, center.z)
            );
        }
        
        // 현재 셀 강조
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridColor.a * 2);
        Handles.DrawWireCube(center, new Vector3(gridSize, gridSize, 0));
        
        // 현재 Z 위치 표시 (작은 텍스트로)
        Handles.Label(center + new Vector3(halfSize - 80, halfSize - 20, 0), $"Z: {zPosition}", EditorStyles.boldLabel);
    }
    
    // 프리팹 배치
    private void PlacePrefab(Vector3 position, Quaternion rotation)
    {
        if (prefabToPlace == null) return;
        
        // 에디터에서 실행 취소 가능하도록
        Undo.RecordObject(this, "Place Prefab");
        
        // 새 프리팹 생성
        GameObject newPrefab = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
        
        // 위치 및 회전 설정
        newPrefab.transform.position = position;
        newPrefab.transform.rotation = rotation;
        
        // 부모 설정
        if (parentTransform != null)
        {
            newPrefab.transform.SetParent(parentTransform, true);
        }
        
        // 배치 목록에 추가
        placedPrefabs.Add(newPrefab);
        
        // 에디터에서 실행 취소 등록
        Undo.RegisterCreatedObjectUndo(newPrefab, "Place Prefab");
    }
    
    // 마지막 프리팹 삭제
    private void DeleteLastPrefab()
    {
        if (placedPrefabs.Count == 0) return;
        
        int lastIndex = placedPrefabs.Count - 1;
        GameObject lastPrefab = placedPrefabs[lastIndex];
        
        if (lastPrefab != null)
        {
            Undo.RecordObject(this, "Delete Last Prefab");
            Undo.DestroyObjectImmediate(lastPrefab);
            placedPrefabs.RemoveAt(lastIndex);
        }
        else
        {
            // 오브젝트가 이미 삭제된 경우 목록에서만 제거
            placedPrefabs.RemoveAt(lastIndex);
        }
        
        SceneView.RepaintAll();
    }
    
    // 모든 프리팹 삭제
    private void DeleteAllPrefabs()
    {
        if (placedPrefabs.Count == 0) return;
        
        // 확인 대화상자
        if (EditorUtility.DisplayDialog("모든 프리팹 삭제", "정말 모든 프리팹을 삭제하시겠습니까?", "삭제", "취소"))
        {
            Undo.RecordObject(this, "Delete All Prefabs");
            
            foreach (GameObject prefab in placedPrefabs)
            {
                if (prefab != null)
                {
                    Undo.DestroyObjectImmediate(prefab);
                }
            }
            
            placedPrefabs.Clear();
            SceneView.RepaintAll();
        }
    }
    
    // Undo/Redo 이벤트 처리
    private void OnUndoRedo()
    {
        // 목록 정리 (삭제된 오브젝트 제거)
        placedPrefabs.RemoveAll(prefab => prefab == null);
        SceneView.RepaintAll();
    }
}