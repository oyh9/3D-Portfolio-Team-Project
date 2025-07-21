using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonballPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private bool expandable = true;
    
    private List<GameObject> pool;
    private Transform poolParent;
    
    public static CannonballPoolManager Instance { get; private set; }
    
    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 풀 초기화
        InitializePool();
    }
    
    private void InitializePool()
    {
        pool = new List<GameObject>();
        
        // 풀의 부모 오브젝트 생성
        poolParent = new GameObject("CannonballPool").transform;
        poolParent.SetParent(transform);
        
        // 설정한 사이즈만큼 대포알 프리팹 생성
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewCannonball();
        }
    }
    
    private GameObject CreateNewCannonball()
    {
        GameObject newCannonball = Instantiate(cannonballPrefab, Vector3.zero, Quaternion.identity, poolParent);
        newCannonball.SetActive(false);
        pool.Add(newCannonball);
        
        // 대포알의 컴포넌트를 미리 캐싱해두면 성능상 이점이 있음
        Rigidbody rb = newCannonball.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        return newCannonball;
    }
    
    public GameObject GetCannonball()
    {
        // 풀에서 비활성화된 대포알 찾기
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }
        
        // 풀이 확장 가능하면 새 대포알 생성
        if (expandable)
        {
            return CreateNewCannonball();
        }
        
        // 모든 대포알이 사용 중이고 확장 불가능하면 null 반환
        return null;
    }
    
    // 대포알 반환 (비활성화)
    public void ReturnToPool(GameObject cannonball)
    {
        // 대포알 리셋
        Rigidbody rb = cannonball.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        cannonball.transform.SetParent(poolParent);
        cannonball.SetActive(false);
    }
}