using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [System.Serializable]
    public class ObstacleType
    {
        public string tag;
        public GameObject prefab;
        public int poolSize = 10;
    }

    [Header("풀링 설정")]
    [SerializeField] private List<ObstacleType> obstacleTypes = new List<ObstacleType>();
    [SerializeField] private Transform objectPoolParent;

    [Header("플레이어 및 거리 설정")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float activationDistance = 15f;
    [SerializeField] private float deactivationDistance = 20f; // 비활성화 거리는 활성화보다 약간 더 길게

    [Header("데이터 설정")]
    [SerializeField] private TextAsset obstacleDataJson;
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private List<ObstacleData> obstaclesData = new List<ObstacleData>();
    private Dictionary<string, GameObject> activeObstacles = new Dictionary<string, GameObject>();

    [System.Serializable]
    public class ObstacleData
    {
        public string id;
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [System.Serializable]
    private class ObstacleDataList
    {
        public List<ObstacleData> obstacles = new List<ObstacleData>();
    }

    private void Awake()
    {
        // 오브젝트 풀 부모가 없으면 생성
        if (objectPoolParent == null)
        {
            GameObject poolParent = new GameObject("ObjectPool");
            objectPoolParent = poolParent.transform;
        }

        // 오브젝트 풀 초기화
        InitializeObjectPool();
        
        // 장애물 데이터 로드
        LoadObstacleData();
    }

    // 오브젝트 풀 초기화
    private void InitializeObjectPool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (ObstacleType obstacleType in obstacleTypes)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            GameObject typeParent = new GameObject(obstacleType.tag + "Pool");
            typeParent.transform.SetParent(objectPoolParent);

            for (int i = 0; i < obstacleType.poolSize; i++)
            {
                GameObject obj = Instantiate(obstacleType.prefab, Vector3.zero, Quaternion.identity, typeParent.transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(obstacleType.tag, objectQueue);
        }
    }

    // JSON에서 장애물 데이터 로드
    private void LoadObstacleData()
    {
        if (obstacleDataJson != null)
        {
            // JSON 데이터에서 장애물 정보 파싱
            ObstacleDataList dataList = JsonUtility.FromJson<ObstacleDataList>(obstacleDataJson.text);
            obstaclesData = dataList.obstacles;
            Debug.Log($"{obstaclesData.Count}개의 장애물 데이터를 로드했습니다.");
        }
        else
        {
            Debug.LogError("장애물 데이터 JSON 파일이 연결되지 않았습니다!");
        }
    }

    // 매 프레임마다 플레이어 위치 기반으로 장애물 활성화/비활성화
    private void Update()
    {
        // 플레이어가 설정되어 있지 않다면 종료
        if (playerTransform == null) return;

        // 모든 장애물 데이터를 확인하며 활성화/비활성화 관리
        foreach (ObstacleData data in obstaclesData)
        {
            // x, y 좌표만 고려하여 거리 계산 (2D 좌표계에서)
            float distance = Vector3.Distance(
                new Vector3(playerTransform.position.x, playerTransform.position.y, 0),
                new Vector3(data.position.x, data.position.y, 0)
            );

            // 활성화 거리 내에 있고 아직 활성화되지 않은 경우
            if (distance <= activationDistance && !activeObstacles.ContainsKey(data.id))
            {
                ActivateObstacle(data);
            }
            // 비활성화 거리를 넘어서고 활성화된 상태인 경우
            else if (distance > deactivationDistance && activeObstacles.ContainsKey(data.id))
            {
                DeactivateObstacle(data.id);
            }
        }
    }

    // 장애물 활성화
    private void ActivateObstacle(ObstacleData data)
    {
        // 해당 타입의 오브젝트 풀이 있는지 확인
        if (poolDictionary.TryGetValue(data.type, out Queue<GameObject> objectPool))
        {
            // 풀에 사용할 수 있는 오브젝트가 있는지 확인
            if (objectPool.Count > 0)
            {
                GameObject obj = objectPool.Dequeue();
                
                // 위치, 회전, 크기 설정
                obj.transform.position = data.position;
                obj.transform.eulerAngles = data.rotation;
                obj.transform.localScale = data.scale;
                
                // 활성화
                obj.SetActive(true);
                
                // 활성화된 오브젝트 목록에 추가
                activeObstacles.Add(data.id, obj);
                
                Debug.Log($"장애물 활성화: {data.id} (타입: {data.type}) 위치: {data.position}");
            }
            else
            {
                Debug.LogWarning($"풀에 사용 가능한 {data.type} 타입의 오브젝트가 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"{data.type} 타입의 오브젝트 풀이 존재하지 않습니다!");
        }
    }

    // 장애물 비활성화
    private void DeactivateObstacle(string id)
    {
        if (activeObstacles.TryGetValue(id, out GameObject obj))
        {
            // 데이터에서 해당 ID를 가진 장애물 찾기
            ObstacleData data = obstaclesData.Find(x => x.id == id);
            
            if (data != null)
            {
                // 비활성화
                obj.SetActive(false);
                
                // 다시 풀에 반환
                poolDictionary[data.type].Enqueue(obj);
                
                // 활성화된 오브젝트 목록에서 제거
                activeObstacles.Remove(id);
                
                Debug.Log($"장애물 비활성화: {id}");
            }
        }
    }

    // 장애물 데이터 저장 예시 (에디터에서 사용할 수 있음)
    public void SaveObstacleData(string fileName)
    {
        ObstacleDataList dataList = new ObstacleDataList();
        dataList.obstacles = obstaclesData;
        
        string json = JsonUtility.ToJson(dataList, true);
        string path = Application.dataPath + "/" + fileName + ".json";
        
        System.IO.File.WriteAllText(path, json);
        Debug.Log($"장애물 데이터가 저장되었습니다: {path}");
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
}