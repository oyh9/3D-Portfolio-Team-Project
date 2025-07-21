using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float raycastDistance = 50f;
    [SerializeField] private LayerMask targetLayer; // 발사할 대상 레이어(플레이어 등)
    [SerializeField] private LayerMask obstacleLayer; // 발사 방해 레이어(벽 등)
    [SerializeField] private ParticleSystem muzzleFlash;
    //[SerializeField] private AudioClip fireSound;
    [SerializeField] private bool autoFireAtTarget = true; // 타겟 감지 시 자동 발사 여부
    [SerializeField] private float detectionInterval = 0.5f; // 감지 주기
    
    private AudioSource audioSource;
    private float nextFireTime;
    private bool canFire = false;
    private Vector3 fireDirection;
    
    private void Awake()
    {
        // audioSource = GetComponent<AudioSource>();
        // if (audioSource == null && fireSound != null)
        // {
        //     audioSource = gameObject.AddComponent<AudioSource>();
        // }
    }
    
    private void Start()
    {
        // 주기적으로 발사 가능 여부 체크
        if (autoFireAtTarget)
        {
            StartCoroutine(CheckFireConditionRoutine());
        }
    }
    
    // 발사 조건 체크 코루틴
    private IEnumerator CheckFireConditionRoutine()
    {
        while (true)
        {
            CheckFireCondition();
            
            // 발사 가능하면 발사
            if (canFire && Time.time >= nextFireTime)
            {
                Fire();
            }
            
            yield return new WaitForSeconds(detectionInterval);
        }
    }
    
    // 발사 조건 체크 메서드 - 한 번의 레이캐스트로 모든 것을 체크
    private void CheckFireCondition()
    {
        // 전체 레이어 마스크 (모든 레이어를 감지)
        int allLayersMask = Physics.DefaultRaycastLayers;
        
        RaycastHit hit;
        fireDirection = firePoint.forward;
        
        // 모든 레이어에 대해 레이캐스트 수행
        if (Physics.Raycast(firePoint.position, fireDirection, out hit, raycastDistance, allLayersMask))
        {
            // 맞은 오브젝트의 레이어 확인
            int hitLayer = hit.collider.gameObject.layer;
            
            // 타겟 레이어인지 체크
            bool isTargetLayer = ((1 << hitLayer) & targetLayer) != 0;
            
            // 장애물 레이어인지 체크
            bool isObstacleLayer = ((1 << hitLayer) & obstacleLayer) != 0;
            
            // 발사 가능 조건: 타겟 레이어이고, 장애물 레이어가 아님
            canFire = isTargetLayer && !isObstacleLayer;
            
            // 디버그 레이 색상 설정
            Color rayColor = Color.yellow; // 기본 색상
            if (isTargetLayer) rayColor = Color.green; // 타겟 감지
            if (isObstacleLayer) rayColor = Color.red; // 장애물 감지
            
            Debug.DrawRay(firePoint.position, fireDirection * hit.distance, rayColor, detectionInterval);
        }
        else
        {
            // 아무것도 감지되지 않음
            canFire = false;
            Debug.DrawRay(firePoint.position, fireDirection * raycastDistance, Color.white, detectionInterval);
        }
    }
    
    // 대포 발사 메서드
    public void Fire()
    {
        if (Time.time < nextFireTime) return;
        
        nextFireTime = Time.time + 1f / fireRate;
        
        // 오브젝트 풀에서 대포알 가져오기
        GameObject cannonball = CannonballPoolManager.Instance.GetCannonball();
        if (cannonball == null) return;
        
        // 대포알 위치와 회전 설정
        cannonball.transform.position = firePoint.position;
        cannonball.transform.rotation = firePoint.rotation;
        cannonball.SetActive(true);
        
        // 대포알 발사
        Cannonball cannonballComponent = cannonball.GetComponent<Cannonball>();
        if (cannonballComponent != null)
        {
            cannonballComponent.Launch(fireDirection);
        }
        
        // 발사 효과
        if (muzzleFlash != null)
        {
            ParticleSystem effect = Instantiate(muzzleFlash, firePoint.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }
        
        // 발사 소리
        // if (audioSource != null && fireSound != null)
        // {
        //     audioSource.PlayOneShot(fireSound);
        // }
    }
    
    
    
}