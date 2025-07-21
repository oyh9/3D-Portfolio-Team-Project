using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GimmickType
{
    None,
    DoorController,
    PressurePlate,
    InvisiblePath,
    DisappearBLock,
    // 필요에 따라 추가 가능
}

// 위치, 회전값만 필요한 오브젝트 (On/Off 등의 상태가 필요한 오브젝트의 경우 확장 필요)
[Serializable]
public class SimpleSaveData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class StageSaveData
{
    public List<SimpleSaveData> objects = new();
}

[Serializable]
public class TriggerActionBinding
{
    [Tooltip("트리거 인덱스 번호")]
    public int triggerIndex;

    [Tooltip("실행할 대상 MonoBehaviour (예: StageManager, DoorController 등)")]
    public MonoBehaviour targetComponent;

    [Tooltip("대상 스크립트에서 실행할 메서드 이름 (예: LoadStage, Activate 등)")]
    public string methodName;
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    public Transform PlayerInstance { get; private set; }
    
    [Header("스폰 관련")]
    public GameObject playerPrefab;         // 생성할 플레이어 프리팹
    private GameObject playerInstance;      // 실제 인스턴스된 플레이어
    [SerializeField]private SpawnPoint spawnPoint;
    [SerializeField]private CameraController cameraController;
    
    [Header("트리거 설정")]
    [SerializeField] private List<TriggerActionBinding> triggerBindings = new();

    [Header("오브젝트 설정")]
    [SerializeField] private GameObject[] targetObjects;
    
    private Dictionary<int, Action> triggerActions = new();
    
    private Dictionary<GimmickType, List<IStageGimmick>> gimmickGroups = new();
    private Dictionary<string, SimpleSaveData> initialTransforms = new();

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                PlayerInstance = playerObj.transform;
        }
        else
            Destroy(gameObject);
        
        foreach (GimmickType type in System.Enum.GetValues(typeof(GimmickType)))
        {
            if (type != GimmickType.None)
                gimmickGroups[type] = new List<IStageGimmick>();
        }

        SaveInitialTransforms();
        AutoRegisterAllGimmicksInScene();
    }

    private void Start()
    {
        // 게임 도중 물체가 커지거나 작아지거나 한다면 Update문에서 실행(FindObjectsOfType을 사용중이기에 비추천, 필요시 다른 방법으로)
        SetMassToAllMassObjects();
        
        SpawnPlayer();
        
        RegisterTriggerActions();

        LoadStage();
    }
    
    
    public void SetActiveTarget(int index)
    {
        targetObjects[index].SetActive(true);
        if (index == 0)
        {
            ImprovedSoundManager.Instance.PlaySound2D("MovePoint");
        }
        
    }

    private void AutoRegisterAllGimmicksInScene()
    {
        // 씬 내 모든 오브젝트에서 IStageGimmick을 구현한 컴포넌트 찾기
        IStageGimmick[] allGimmicks = FindObjectsOfType<MonoBehaviour>(true).OfType<IStageGimmick>().ToArray();

        foreach (var gimmick in allGimmicks)
        {
            RegisterGimmick(gimmick);
        }

        // 디버그
        // Debug.Log($"StageManager 씬에서 찾은 기믹 {allGimmicks.Length}개 등록");
    }

    // 등록 요청을 받으면 타입에 따라 리스트에 추가
    public void RegisterGimmick(IStageGimmick gimmick)
    {
        var type = gimmick.GetGimmickType();
        if (!gimmickGroups.ContainsKey(type))
        {
            gimmickGroups[type] = new List<IStageGimmick>();
        }
        gimmickGroups[type].Add(gimmick);
    }

    // 특정 타입의 기믹들만 불러올 수 있게 하기
    public IStageGimmick GetGimmickByID(GimmickType type, string id)
    {
        return gimmickGroups.TryGetValue(type, out var list)
            ? list.FirstOrDefault(g => g.GetGimmickID() == id)
            : null;
    }
    
    public void SetMassToAllMassObjects()
    {
        MassObject[] massObjects = FindObjectsOfType<MassObject>(true);

        foreach (var massObj in massObjects)
        {
            GameObject obj = massObj.gameObject;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            Collider col = obj.GetComponent<Collider>();

            if (rb == null || col == null) continue;

            Vector3 size = col.bounds.size;
            float volume = size.x * size.y * size.z;
            rb.mass = volume * massObj.multiplier;
        }
    }
    
    public void SpawnPlayer()
    {
        if (spawnPoint == null)
        {
            return;
        }
        
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }
        
        if (playerPrefab == null)
        {
            return;
        }

        if (GameManager.Instance.currentSceneName == "Stage3")
        {
            playerInstance = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            playerInstance.transform.Rotate(0, 90, 0);
        }
        else
        {
            playerInstance = Instantiate(playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            
        }
        
        if (cameraController != null)
        {
            // 자식 중에 "CameraTarget"이라는 이름의 오브젝트를 찾아서 따라가도록
            Transform cameraTarget = playerInstance.transform.Find("CameraTarget");
            cameraController.target = cameraTarget != null ? cameraTarget : playerInstance.transform;
        }
        
        Transform gravityGunTransform = playerInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "GravityGun");

        if (gravityGunTransform != null)
        {
            var gravityGunScript = gravityGunTransform.GetComponent<GravityGun>();
            if (gravityGunScript != null)
            {
                gravityGunScript.cameraTransform = Camera.main.transform;
                gravityGunScript.firePoint = Camera.main.transform;
            }
        }
        
        PlayerRespawnManager.Instance.RegisterPlayer(playerInstance);
    }
    
    public void SpawnPlayer(Vector3 position)
    {
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        if (playerPrefab == null) return;

        playerInstance = Instantiate(playerPrefab, position, Quaternion.identity);

        if (cameraController != null)
        {
            Transform cameraTarget = playerInstance.transform.Find("CameraTarget");
            cameraController.target = cameraTarget != null ? cameraTarget : playerInstance.transform;
        }

        Transform gravityGunTransform = playerInstance.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "GravityGun");

        if (gravityGunTransform != null)
        {
            var gravityGunScript = gravityGunTransform.GetComponent<GravityGun>();
            if (gravityGunScript != null)
            {
                gravityGunScript.cameraTransform = Camera.main.transform;
                gravityGunScript.firePoint = Camera.main.transform;
            }
        }

        PlayerRespawnManager.Instance.RegisterPlayer(playerInstance);
    }
    
    private void RegisterTriggerActions()
    {
        triggerActions.Clear();

        foreach (var binding in triggerBindings)
        {
            int index = binding.triggerIndex;
            var target = binding.targetComponent;
            string method = binding.methodName;

            triggerActions[index] = () =>
            {
                if (target == null || string.IsNullOrEmpty(method))
                {
                    Debug.Log($"Trigger {index}의 바인딩이 비어있음");
                    return;
                }

                var methodInfo = target.GetType().GetMethod(method);
                if (methodInfo == null)
                {
                    Debug.Log($"Trigger {index} - '{target.name}'에서 메서드 '{method}'를 찾을 수 없음");
                    return;
                }

                methodInfo.Invoke(target, null);
            };
        }
    }
    
    
    
    public void OnTriggerEntered(int index)
    {
        if (triggerActions.TryGetValue(index, out var action))
        {
            action.Invoke();
        }
    }

    #region 스테이지 저장

    private void SaveInitialTransforms()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Savable"))
        {
            if (!initialTransforms.ContainsKey(obj.name))
            {
                initialTransforms[obj.name] = new SimpleSaveData
                {
                    name = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation
                };
            }
        }
    }
    
    // 게임 저장시 필요
    public void SaveStage()
    {
        StageSaveData data = new();

        foreach (var obj in GameObject.FindGameObjectsWithTag("Savable"))
        {
            data.objects.Add(new SimpleSaveData
            {
                name = obj.name,
                position = obj.transform.position,
                rotation = obj.transform.rotation
            });
        }

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString($"StageSave_{sceneName}", json);
        PlayerPrefs.Save();

        Debug.Log("스테이지 저장");
    }
    
    public void LoadStage()
    {
        // 물체들의 마지막 위치 저장(세이브 포인트 기준) / SaveStage에서 저장한 것 불러올 때
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string key = $"StageSave_{sceneName}";
        
        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log("저장 데이터 없음");
            return;
        }
        
        string json = PlayerPrefs.GetString(key);
        StageSaveData data = JsonUtility.FromJson<StageSaveData>(json);
        
        foreach (var saved in data.objects)
        {
            GameObject obj = GameObject.Find(saved.name);
            if (obj != null)
            {
                obj.transform.position = saved.position;
                obj.transform.rotation = saved.rotation;

                if (obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
        
        Debug.Log("스테이지 불러오기");
    }

    public void ResetStage()
    {
        foreach (var saved in initialTransforms.Values)
        {
            GameObject obj = GameObject.Find(saved.name);
            if (obj != null)
            {
                obj.transform.position = saved.position;
                obj.transform.rotation = saved.rotation;
                
                if (obj.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }

    public void ResetGameProgress()
    {
        foreach (var scene in new[] { "Stage3", "YSJ" }) // 실제 씬 이름으로
        {
            PlayerPrefs.DeleteKey($"StageSave_{scene}");
        }

        PlayerPrefs.Save();
    }
    
    #endregion
}
