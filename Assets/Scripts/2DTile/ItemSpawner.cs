using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("프리팹 설정")]
    [SerializeField] private string prefabName; // 생성할 프리팹 이름 (ChunkedPoolManager에 등록된 이름)
    
    [Header("스폰 위치 설정")]
    [SerializeField] private bool useTransform = true; // Transform을 사용할지, Vector3를 사용할지
    [SerializeField] private Transform spawnTransform; // Transform으로 위치 지정할 경우
    [SerializeField] private Vector3 spawnPosition; // Vector3로 직접 좌표 입력할 경우
    
    [Header("충돌 방향 설정")]
    [SerializeField] private bool checkCollisionDirection = true; // 충돌 방향 체크 여부
    [SerializeField] private Vector3 collisionDirection = Vector3.forward; // 충돌을 감지할 방향
    [Range(0f, 1f)]
    [SerializeField] private float directionThreshold = 0.5f; // 방향 일치 허용 오차 (0: 정확히 일치, 1: 모든 방향 허용)
    
    [Header("기타 옵션")]
    [SerializeField] private bool spawnOnce = true; // 한 번만 생성할지 여부
    [SerializeField] private string playerTag = "Player"; // 플레이어 태그
    
    private bool hasSpawned = false; // 이미 생성됐는지 체크
    private GameObject spawnedObject; // 생성된 오브젝트 참조
    
    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어가 아니면 무시
        if (!collision.gameObject.CompareTag(playerTag))
            return;
            
        // 이미 생성했고 한 번만 생성하는 옵션이 켜져 있으면 무시
        if (hasSpawned && spawnOnce)
            return;
            
        // 충돌 방향 체크가 활성화되어 있다면
        if (checkCollisionDirection)
        {
            // 충돌 지점의 방향 확인
            ContactPoint contact = collision.GetContact(0);
            
            // 지정한 방향과 충돌 법선 벡터의 내적 계산
            float dotProduct = Vector3.Dot(contact.normal, collisionDirection.normalized);
            
            if (dotProduct > -directionThreshold) // 원하는 방향에서 충돌하지 않았으면 무시
                return;
        }
        ImprovedSoundManager.Instance.PlaySound3D("Block", transform.position);
        // 오브젝트 활성화
        SpawnFromPool();
        
        // 생성 여부 체크
        hasSpawned = true;
    }
    
    private void SpawnFromPool()
    {
        // 생성 위치와 회전 결정
        Vector3 finalPosition;
        Quaternion finalRotation;
        
        if (useTransform && spawnTransform != null)
        {
            finalPosition = spawnTransform.position;
            finalRotation = spawnTransform.rotation;
        }
        else
        {
            finalPosition = spawnPosition;
            finalRotation = Quaternion.identity;
        }
        
        // ChunkedPoolManager에서 오브젝트 가져오기
        ChunkedPoolManager poolManager = ChunkedPoolManager.Instance;
        if (poolManager != null)
        {
            // 풀에서 비활성화된 오브젝트 찾기
            Queue<GameObject> objectPool = poolManager.GetObjectPool(prefabName);
            if (objectPool != null && objectPool.Count > 0)
            {
                // 풀에서 오브젝트 가져오기
                spawnedObject = objectPool.Dequeue();
                
                // 오브젝트 설정 및 활성화
                spawnedObject.transform.position = finalPosition;
                spawnedObject.transform.rotation = finalRotation;
                spawnedObject.SetActive(true);
                
                // 물리 오브젝트 초기화
                Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                Debug.LogWarning($"풀에 사용 가능한 '{prefabName}' 오브젝트가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("ChunkedPoolManager를 찾을 수 없습니다.");
        }
    }
    
    // 스포너가 비활성화될 때 호출됨
    private void OnDisable()
    {
        // 스포너가 비활성화될 때 자동으로 리셋
        ResetSpawner();
    }
    
    // 상태 리셋 메서드 (재사용 가능하도록)
    public void ResetSpawner()
    {
        // 생성된 오브젝트가 있다면 비활성화하고 풀에 반환
        if (spawnedObject != null)
        {
            spawnedObject.SetActive(false);
            
            ChunkedPoolManager poolManager = ChunkedPoolManager.Instance;
            if (poolManager != null)
            {
                poolManager.ReturnToPool(prefabName, spawnedObject);
            }
            
            spawnedObject = null;
        }
        
        hasSpawned = false;
    }
    
    // 씬 종료 시 호출됨 (추가 안전장치)
    private void OnDestroy()
    {
        ResetSpawner();
    }
    
    
    // 외부에서 수동으로 활성화할 수 있는 메서드 (필요시)
    public void SpawnItem()
    {
        if (hasSpawned && spawnOnce)
            return;
            
        SpawnFromPool();
        hasSpawned = true;
    }
}