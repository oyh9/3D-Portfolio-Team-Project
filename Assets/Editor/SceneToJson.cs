using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
public class SceneToJson : EditorWindow
{
    private string exportFileName = "level_data.json";
    private bool includeInactiveObjects = false;
    private string objectTag = "LevelObject"; // 특정 태그를 가진 오브젝트만 수집
    private float chunkSize = 10f; // 청크 크기
    
    [MenuItem("Tools/Export Scene Objects to JSON")]
    public static void ShowWindow()
    {
        GetWindow<SceneToJson>("씬 오브젝트 JSON 내보내기");
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("씬 오브젝트를 JSON으로 내보내기", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 옵션 설정
        exportFileName = EditorGUILayout.TextField("저장 파일명", exportFileName);
        if (!exportFileName.EndsWith(".json")) exportFileName += ".json";
        
        includeInactiveObjects = EditorGUILayout.Toggle("비활성화 오브젝트 포함", includeInactiveObjects);
        objectTag = EditorGUILayout.TextField("대상 오브젝트 태그", objectTag);
        chunkSize = EditorGUILayout.FloatField("청크 크기", chunkSize);
        
        EditorGUILayout.Space();
        
        // 내보내기 버튼
        if (GUILayout.Button("씬 오브젝트 내보내기", GUILayout.Height(30)))
        {
            ExportSceneObjectsToJson();
        }
    }
    
    private void ExportSceneObjectsToJson()
    {
        // 씬에서 지정된 태그를 가진 모든 오브젝트 수집
        GameObject[] sceneObjects;
        
        if (string.IsNullOrEmpty(objectTag) || objectTag == "Untagged")
        {
            sceneObjects = includeInactiveObjects 
                ? Resources.FindObjectsOfTypeAll<GameObject>() 
                : Object.FindObjectsOfType<GameObject>();
        }
        else
        {
            sceneObjects = GameObject.FindGameObjectsWithTag(objectTag);
        }
        
        // 청크 단위로 오브젝트 구성
        Dictionary<Vector3Int, List<LevelObjectData>> chunks = new Dictionary<Vector3Int, List<LevelObjectData>>();
        
        // 경계값 계산용 변수
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        
        // 각 오브젝트의 정보 수집
        foreach (GameObject obj in sceneObjects)
        {
            // 씬 오브젝트인지 확인 (에셋이나 프리팹 원본 제외)
            if (PrefabUtility.GetPrefabInstanceStatus(obj) != PrefabInstanceStatus.NotAPrefab &&
                PrefabUtility.GetPrefabInstanceStatus(obj) != PrefabInstanceStatus.Connected)
                continue;
            
            // 비활성화된 오브젝트 처리
            if (!obj.activeInHierarchy && !includeInactiveObjects)
                continue;
            
            // 프리팹 원본 정보 가져오기
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (prefabSource == null) continue; // 프리팹이 아닌 경우 스킵
            
            // 경계값 업데이트
            minX = Mathf.Min(minX, obj.transform.position.x);
            maxX = Mathf.Max(maxX, obj.transform.position.x);
            minY = Mathf.Min(minY, obj.transform.position.y);
            maxY = Mathf.Max(maxY, obj.transform.position.y);
            minZ = Mathf.Min(minZ, obj.transform.position.z);
            maxZ = Mathf.Max(maxZ, obj.transform.position.z);
            
            // 청크 좌표 계산
            Vector3Int chunkPos = GetChunkPosition(obj.transform.position);
            
            // 오브젝트 데이터 생성
            LevelObjectData data = new LevelObjectData
            {
                prefabName = prefabSource.name,
                prefabPath = AssetDatabase.GetAssetPath(prefabSource),
                position = obj.transform.position,
                rotation = obj.transform.rotation,
                scale = obj.transform.localScale,
                objectName = obj.name,
                id = System.Guid.NewGuid().ToString().Substring(0, 8),
                isActive = obj.activeInHierarchy,
                chunkPosition = chunkPos
            };
            
            // 청크에 오브젝트 추가
            if (!chunks.ContainsKey(chunkPos))
            {
                chunks[chunkPos] = new List<LevelObjectData>();
            }
            chunks[chunkPos].Add(data);
        }
        
        // 청크 정보 변환
        List<ChunkData> chunkDataList = new List<ChunkData>();
        foreach (var chunk in chunks)
        {
            ChunkData chunkData = new ChunkData
            {
                position = chunk.Key,
                objects = chunk.Value
            };
            chunkDataList.Add(chunkData);
        }
        
        // 레벨 메타 데이터
        LevelMetaData metaData = new LevelMetaData
        {
            minBounds = new Vector3(minX, minY, minZ),
            maxBounds = new Vector3(maxX, maxY, maxZ),
            chunkSize = chunkSize
        };
        
        // 데이터 래핑
        LevelData levelData = new LevelData
        {
            levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            metaData = metaData,
            chunks = chunkDataList
        };
        
        // JSON으로 변환 및 저장
        string json = JsonUtility.ToJson(levelData, true);
        string path = Path.Combine(Application.dataPath, exportFileName);
        
        File.WriteAllText(path, json);
        
        int totalObjects = 0;
        foreach (var chunk in chunks)
        {
            totalObjects += chunk.Value.Count;
        }
        
        Debug.Log($"JSON 파일이 생성되었습니다: {path}");
        Debug.Log($"총 청크 수: {chunks.Count}, 총 오브젝트 수: {totalObjects}");
        Debug.Log($"레벨 경계: X({minX:F1}~{maxX:F1}), Y({minY:F1}~{maxY:F1}), Z({minZ:F1}~{maxZ:F1})");
        
        // 에셋 데이터베이스 갱신
        AssetDatabase.Refresh();
    }
    
    // 청크 좌표 계산 함수
    private Vector3Int GetChunkPosition(Vector3 worldPos)
    {
        int chunkX = Mathf.FloorToInt(worldPos.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPos.y / chunkSize);
        int chunkZ = Mathf.FloorToInt(worldPos.z / chunkSize);
        return new Vector3Int(chunkX, chunkY, chunkZ);
    }
}
#endif