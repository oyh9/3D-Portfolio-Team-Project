using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChunkedPoolManager : MonoBehaviour
{
    public static ChunkedPoolManager Instance { get; set; }
    
    [System.Serializable]
    public class ObjectPool
    {
        public string prefabName;
        public GameObject prefab;
        public int initialSize = 10;
        [HideInInspector] public Queue<GameObject> pool;
    }
    
    [SerializeField] private TextAsset levelDataJson;
    [SerializeField] private List<ObjectPool> objectPools = new List<ObjectPool>();
    
    [SerializeField] private int chunkLoadDistance = 1; // 로드할 청크 거리 (현재 청크 주변으로 몇 개의 청크를 로드할지)
    [SerializeField] private bool savePositionChanges = true; // 위치 변경 저장 여부
    
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private LevelData levelData;
    private float chunkSize;
    private Dictionary<Vector3Int, ChunkData> chunkDictionary = new Dictionary<Vector3Int, ChunkData>();
    private Dictionary<string, GameObject> activeObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, LevelObjectData> objectDataDictionary = new Dictionary<string, LevelObjectData>(); // ID로 오브젝트 데이터 참조
    private HashSet<Vector3Int> activeChunks = new HashSet<Vector3Int>();
    private Vector3Int lastPlayerChunk = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue); // 마지막 플레이어 청크 위치
    
    private Transform playerTransform;
    private void Awake()
    {
        Instance = this;
        // 풀 초기화
        InitializePools();
        
        // 레벨 데이터 로드
        if (levelDataJson != null)
        {
            LoadLevelData();
        }
        else
        {
            Debug.LogError("레벨 데이터 JSON이 없습니다!");
        }
    }
    

    private void InitializePools()
    {
        Transform poolParent = new GameObject("ObjectPool").transform;
        poolParent.SetParent(transform);
        
        foreach (ObjectPool pool in objectPools)
        {
            if (pool.prefab == null) continue;
            
            // 프리팹 사전에 추가
            prefabDictionary[pool.prefabName] = pool.prefab;
            
            // 풀 생성
            Queue<GameObject> objectPool = new Queue<GameObject>();
            GameObject poolHolder = new GameObject(pool.prefabName + "Pool");
            poolHolder.transform.SetParent(poolParent);
            
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, Vector3.zero, Quaternion.identity);
                obj.SetActive(false);
                obj.transform.SetParent(poolHolder.transform);
                objectPool.Enqueue(obj);
            }
            
            poolDictionary[pool.prefabName] = objectPool;
        }
    }
    
    private void LoadLevelData()
    {
        levelData = JsonUtility.FromJson<LevelData>(levelDataJson.text);
        chunkSize = levelData.metaData.chunkSize;
        
        // 청크 사전 구성
        chunkDictionary.Clear();
        objectDataDictionary.Clear();
        
        foreach (ChunkData chunk in levelData.chunks)
        {
            chunkDictionary[chunk.position] = chunk;
            
            // 오브젝트 데이터 사전 구성
            foreach (LevelObjectData objData in chunk.objects)
            {
                objectDataDictionary[objData.id] = objData;
            }
        }
        
        // Debug.Log($"레벨 '{levelData.levelName}' 데이터 로드 완료 (청크 수: {levelData.chunks.Count})");
        // Debug.Log($"레벨 경계: X({levelData.metaData.minBounds.x:F1}~{levelData.metaData.maxBounds.x:F1}), Y({levelData.metaData.minBounds.y:F1}~{levelData.metaData.maxBounds.y:F1}), Z({levelData.metaData.minBounds.z:F1}~{levelData.metaData.maxBounds.z:F1})");
    }
    
    private void Update()
    {
        if (playerTransform == null || levelData == null) return;
        
        // 현재 플레이어의 청크 위치 계산
        Vector3Int playerChunk = GetChunkPosition(playerTransform.position);
        
        // 플레이어가 같은 청크에 있으면 업데이트 스킵
        if (playerChunk == lastPlayerChunk) return;
        
        // 청크가 변경됐으므로 청크 업데이트 수행
        lastPlayerChunk = playerChunk;
        UpdateChunks(playerChunk);
        
        // 위치 변경이 저장되는 경우, 위치 변경된 오브젝트가 다른 청크로 이동했는지 확인
        if (savePositionChanges)
        {
            UpdateObjectChunks();
        }
    }
    
    // 오브젝트의 청크 위치 업데이트
    private void UpdateObjectChunks()
    {
        List<string> objectsToUpdate = new List<string>();
        List<string> objectsToRemove = new List<string>(); // 제거할 오브젝트 ID 목록 추가
    
        // 활성화된 모든 오브젝트 순회
        foreach (var kvp in activeObjects)
        {
            string id = kvp.Key;
            GameObject obj = kvp.Value;
        
            // 오브젝트가 파괴되었는지 확인
            if (obj == null)
            {
                objectsToRemove.Add(id); // 파괴된 오브젝트는 제거 목록에 추가
                continue;
            }
        
            if (objectDataDictionary.TryGetValue(id, out LevelObjectData data))
            {
                // 현재 오브젝트의 청크 위치 계산
                Vector3Int currentChunk = GetChunkPosition(obj.transform.position);
            
                // 오브젝트가 다른 청크로 이동했는지 확인
                if (currentChunk != data.chunkPosition)
                {
                    objectsToUpdate.Add(id);
                }
            }
        }
    
        // 파괴된 오브젝트 딕셔너리에서 제거
        foreach (string id in objectsToRemove)
        {
            activeObjects.Remove(id);
        }
    
        // 청크 이동한 오브젝트 처리
        foreach (string id in objectsToUpdate)
        {
            MoveObjectToNewChunk(id);
        }
    }

    
    
    // 오브젝트를 새 청크로 이동
    private void MoveObjectToNewChunk(string id)
    {
        if (!activeObjects.TryGetValue(id, out GameObject obj) ||
            !objectDataDictionary.TryGetValue(id, out LevelObjectData data))
        {
            return;
        }
    
        // 오브젝트가 파괴되었는지 확인
        if (obj == null)
        {
            activeObjects.Remove(id);
            return;
        }
        
        // 이전 청크에서 오브젝트 제거
        if (chunkDictionary.TryGetValue(data.chunkPosition, out ChunkData oldChunk))
        {
            oldChunk.objects.RemoveAll(x => x.id == id);
        }
        
        // 새 청크 위치 계산
        Vector3Int newChunkPos = GetChunkPosition(obj.transform.position);
        
        // 새 청크가 없으면 생성
        if (!chunkDictionary.ContainsKey(newChunkPos))
        {
            ChunkData newChunk = new ChunkData
            {
                position = newChunkPos,
                objects = new List<LevelObjectData>()
            };
            chunkDictionary[newChunkPos] = newChunk;
            levelData.chunks.Add(newChunk);
        }
        
        // 오브젝트 데이터 업데이트
        data.position = obj.transform.position;
        data.rotation = obj.transform.rotation;
        data.scale = obj.transform.localScale;
        data.chunkPosition = newChunkPos;
        
        // 새 청크에 오브젝트 추가
        chunkDictionary[newChunkPos].objects.Add(data);
        
        //Debug.Log($"오브젝트 '{obj.name}'(ID: {id})를 청크 {data.chunkPosition}에서 {newChunkPos}로 이동");
    }
    
    private void UpdateChunks(Vector3Int playerChunk)
    {
        // 새로 활성화할 청크들 계산
        HashSet<Vector3Int> newActiveChunks = new HashSet<Vector3Int>();
        
        // 플레이어 주변의 청크 활성화
        for (int x = -chunkLoadDistance; x <= chunkLoadDistance; x++)
        {
            for (int y = -chunkLoadDistance; y <= chunkLoadDistance; y++)
            {
                for (int z = -chunkLoadDistance; z <= chunkLoadDistance; z++)
                {
                    Vector3Int chunkPos = new Vector3Int(
                        playerChunk.x + x,
                        playerChunk.y + y,
                        playerChunk.z + z
                    );
                    
                    newActiveChunks.Add(chunkPos);
                    
                    // 새로 활성화해야 하는 청크
                    if (!activeChunks.Contains(chunkPos))
                    {
                        ActivateChunk(chunkPos);
                    }
                }
            }
        }
        
        // 비활성화해야 하는 청크 찾기
        List<Vector3Int> chunksToDeactivate = new List<Vector3Int>();
        foreach (Vector3Int chunkPos in activeChunks)
        {
            if (!newActiveChunks.Contains(chunkPos))
            {
                chunksToDeactivate.Add(chunkPos);
            }
        }
        
        // 청크 비활성화
        foreach (Vector3Int chunkPos in chunksToDeactivate)
        {
            DeactivateChunk(chunkPos);
        }
        
        // 활성화된 청크 리스트 업데이트
        activeChunks = newActiveChunks;
    }
    
    private void ActivateChunk(Vector3Int chunkPos)
    {
        // 청크 데이터 찾기
        if (!chunkDictionary.TryGetValue(chunkPos, out ChunkData chunkData))
        {
            // 해당 청크가 존재하지 않으면 무시
            return;
        }
        
        // 청크 내 모든 오브젝트 활성화
        foreach (LevelObjectData objData in chunkData.objects)
        {
            ActivateObject(objData);
        }
        
        // 디버그용
        //Debug.Log($"청크 활성화: {chunkPos}, 오브젝트 수: {chunkData.objects.Count}");
    }
    
    private void DeactivateChunk(Vector3Int chunkPos)
    {
        // 청크 데이터 찾기
        if (!chunkDictionary.TryGetValue(chunkPos, out ChunkData chunkData))
        {
            // 해당 청크가 존재하지 않으면 무시
            return;
        }
        
        // 청크 내 모든 오브젝트 비활성화
        foreach (LevelObjectData objData in chunkData.objects)
        {
            // 위치 저장 옵션이 켜져 있으면 현재 위치/회전/크기 저장
            if (savePositionChanges && activeObjects.TryGetValue(objData.id, out GameObject obj))
            {
                // 오브젝트 데이터 업데이트
                objData.position = obj.transform.position;
                objData.rotation = obj.transform.rotation;
                objData.scale = obj.transform.localScale;
            }
            
            DeactivateObject(objData.id);
        }
        
        // 디버그용
        //Debug.Log($"청크 비활성화: {chunkPos}");
    }
    
    private void ActivateObject(LevelObjectData data)
    {
        // 이미 활성화된 오브젝트는 무시
        if (activeObjects.ContainsKey(data.id)) return;
        
        // 프리팹 찾기
        if (!prefabDictionary.TryGetValue(data.prefabName, out GameObject prefab))
        {
            Debug.LogWarning($"프리팹을 찾을 수 없습니다: {data.prefabName}");
            return;
        }
        
        // 풀에서 오브젝트 가져오기
        if (!poolDictionary.TryGetValue(data.prefabName, out Queue<GameObject> pool))
        {
            Debug.LogWarning($"풀을 찾을 수 없습니다: {data.prefabName}");
            return;
        }
        
        GameObject obj;
        
        if (pool.Count > 0)
        {
            // 풀에서 오브젝트 재사용
            obj = pool.Dequeue();
        }
        else
        {
            // 필요시 새 오브젝트 생성
            obj = Instantiate(prefab);
            Debug.Log($"풀 확장: {data.prefabName}");
        }
        
        // 위치, 회전, 크기 설정
        obj.transform.position = data.position;
        obj.transform.rotation = data.rotation;
        obj.transform.localScale = data.scale;
        obj.name = data.objectName;
        
        // 활성화
        obj.SetActive(true);
        
        // 활성화 오브젝트 등록
        activeObjects[data.id] = obj;
    }
    
    private void DeactivateObject(string id)
    {
        if (activeObjects.TryGetValue(id, out GameObject obj))
        {
            // 비활성화 및 풀에 반환
            if (objectDataDictionary.TryGetValue(id, out LevelObjectData data))
            {
                obj.SetActive(false);
                
                if (poolDictionary.TryGetValue(data.prefabName, out Queue<GameObject> pool))
                {
                    pool.Enqueue(obj);
                }
                
                // 활성화 목록에서 제거
                activeObjects.Remove(id);
            }
        }
    }
    
    // 청크 좌표 계산 함수
    private Vector3Int GetChunkPosition(Vector3 worldPos)
    {
        int chunkX = Mathf.FloorToInt(worldPos.x / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPos.y / chunkSize);
        int chunkZ = Mathf.FloorToInt(worldPos.z / chunkSize);
        return new Vector3Int(chunkX, chunkY, chunkZ);
    }
    
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform=player;
    }

    public void OffPlayerTransform(Transform player)
    {
        if (playerTransform != null)
        {
            playerTransform = null;
            
        }
    }
    
    
    
    // 모든 오브젝트를 초기 위치로 리셋하는 함수
    public void ResetAllObjectsToInitialPosition()
    {
        // 레벨 데이터가 없으면 리턴
        if (levelData == null || levelDataJson == null)
        {
            Debug.LogWarning("레벨 데이터 또는 JSON이 없어 리셋할 수 없습니다.");
            return;
        }
        
        // 활성화된 오브젝트 ID 목록 복사 (반복문 중 컬렉션 변경 방지)
        List<string> activeIds = new List<string>(activeObjects.Keys);
        
        // 초기 JSON 데이터 다시 로드
        LevelData originalLevelData = JsonUtility.FromJson<LevelData>(levelDataJson.text);
        if (originalLevelData == null)
        {
            Debug.LogError("원본 JSON 데이터를 로드할 수 없습니다.");
            return;
        }
        
        // 원본 JSON에서 오브젝트 ID별 초기 위치 정보 추출
        Dictionary<string, LevelObjectData> originalObjectDataDict = new Dictionary<string, LevelObjectData>();
        
        if (originalLevelData.chunks != null)
        {
            foreach (ChunkData chunk in originalLevelData.chunks)
            {
                if (chunk == null || chunk.objects == null) continue;
                
                foreach (LevelObjectData objData in chunk.objects)
                {
                    if (objData == null || string.IsNullOrEmpty(objData.id)) continue;
                    originalObjectDataDict[objData.id] = objData;
                }
            }
        }
        
        int resetCount = 0;
        
        // 모든 활성화된 오브젝트의 위치 리셋
        foreach (string id in activeIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            
            // 활성화된 오브젝트와 원본 데이터 참조 가져오기
            if (activeObjects.TryGetValue(id, out GameObject obj) && obj != null && 
                originalObjectDataDict.TryGetValue(id, out LevelObjectData originalObjData))
            {
                // 물리 효과 초기화
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 물리 효과 일시 중지
                    bool wasKinematic = rb.isKinematic;
                    rb.isKinematic = true;
                    
                    // 속도 및 각속도 초기화
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 오브젝트 위치, 회전, 크기를 초기값으로 설정
                    obj.transform.position = originalObjData.position;
                    obj.transform.rotation = originalObjData.rotation;
                    obj.transform.localScale = originalObjData.scale;
                    
                    // 원래 상태로 복구
                    rb.isKinematic = wasKinematic;
                }
                else
                {
                    // Rigidbody가 없는 경우 그냥 Transform만 설정
                    obj.transform.position = originalObjData.position;
                    obj.transform.rotation = originalObjData.rotation;
                    obj.transform.localScale = originalObjData.scale;
                }
                
                // 물리 효과 외 다른 컴포넌트 초기화 (선택적)
                // 예: 캐릭터 컨트롤러, 애니메이터 등
                CharacterController cc = obj.GetComponent<CharacterController>();
                if (cc != null)
                {
                    // CharacterController 이동 초기화
                    cc.enabled = false;
                    cc.enabled = true; // 위치 재설정을 위해 비활성화 후 활성화
                }
                
                // 현재 데이터 딕셔너리 업데이트
                if (objectDataDictionary.TryGetValue(id, out LevelObjectData currentObjData))
                {
                    // 현재 오브젝트 데이터 업데이트
                    currentObjData.position = originalObjData.position;
                    currentObjData.rotation = originalObjData.rotation;
                    currentObjData.scale = originalObjData.scale;
                    
                    // 청크 위치가 변경되었다면 오브젝트를 새 청크로 이동
                    Vector3Int newChunkPos = GetChunkPosition(originalObjData.position);
                    if (currentObjData.chunkPosition != newChunkPos)
                    {
                        // 이전 청크에서 오브젝트 제거
                        if (chunkDictionary.TryGetValue(currentObjData.chunkPosition, out ChunkData oldChunk) && 
                            oldChunk != null && oldChunk.objects != null)
                        {
                            oldChunk.objects.RemoveAll(x => x != null && x.id == id);
                        }
                        
                        // 청크 위치 업데이트
                        currentObjData.chunkPosition = newChunkPos;
                        
                        // 새 청크에 오브젝트 추가
                        if (chunkDictionary.TryGetValue(newChunkPos, out ChunkData newChunk) && 
                            newChunk != null && newChunk.objects != null)
                        {
                            // 이미 있는지 확인 후 추가
                            if (!newChunk.objects.Exists(x => x != null && x.id == id))
                            {
                                newChunk.objects.Add(currentObjData);
                            }
                        }
                        else
                        {
                            // 새 청크가 없다면 생성
                            ChunkData createdChunk = new ChunkData
                            {
                                position = newChunkPos,
                                objects = new List<LevelObjectData>() { currentObjData }
                            };
                            chunkDictionary[newChunkPos] = createdChunk;
                            levelData.chunks.Add(createdChunk);
                        }
                    }
                }
                
                resetCount++;
            }
        }
        
        //Debug.Log($"총 {resetCount}개 오브젝트의 위치가 초기 상태로 리셋되었습니다.");
    }
    
   // 이름으로 오브젝트 풀 가져오기
    public Queue<GameObject> GetObjectPool(string prefabName)
    {
        if (poolDictionary.TryGetValue(prefabName, out Queue<GameObject> pool))
        {
            return pool;
        }
    
        Debug.LogWarning($"'{prefabName}' 이름의 오브젝트 풀을 찾을 수 없습니다.");
        return null;
    }

    // 오브젝트를 풀에 반환
    public void ReturnToPool(string prefabName, GameObject obj)
    {
        if (obj == null) return;
    
        obj.SetActive(false);
    
        if (poolDictionary.TryGetValue(prefabName, out Queue<GameObject> pool))
        {
            // 부모 오브젝트 찾기 및 설정
            Transform poolParent = transform.Find("ObjectPool")?.Find(prefabName + "Pool");
            if (poolParent != null)
            {
                obj.transform.SetParent(poolParent);
            }
        
            pool.Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"'{prefabName}' 이름의 오브젝트 풀을 찾을 수 없어 오브젝트를 반환할 수 없습니다.");
        }
    }
    
    
    // 외부에서 호출 가능한 레벨 데이터 변경 메서드
    public void LoadNewLevelData(TextAsset newLevelData)
    {
        // 모든 활성화된 오브젝트 비활성화
        List<string> activeIds = new List<string>(activeObjects.Keys);
        foreach (string id in activeIds)
        {
            DeactivateObject(id);
        }
        
        // 활성화된 청크 초기화
        activeChunks.Clear();
        lastPlayerChunk = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
        
        // 새 레벨 데이터 로드
        levelDataJson = newLevelData;
        LoadLevelData();
    }
    
    // 현재 레벨 데이터를 JSON으로 저장
    public void SaveLevelData()
    {
        if (levelData == null) return;
        
        // 활성화된 오브젝트의 현재 변환 정보 업데이트
        foreach (var kvp in activeObjects)
        {
            string id = kvp.Key;
            GameObject obj = kvp.Value;
            
            if (objectDataDictionary.TryGetValue(id, out LevelObjectData data))
            {
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation;
                data.scale = obj.transform.localScale;
            }
        }
        
        // JSON으로 변환
        string json = JsonUtility.ToJson(levelData, true);
        
        // 저장 경로 지정 (원래 파일 경로 이용)
        string path = UnityEngine.Application.dataPath + "/Resources/" + levelDataJson.name + ".json";
        
        // 파일로 저장
        System.IO.File.WriteAllText(path, json);
        
        Debug.Log($"레벨 데이터가 저장되었습니다: {path}");
        
    }
}