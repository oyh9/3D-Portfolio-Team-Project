using UnityEngine;

public class CollisionBreakEffect : MonoBehaviour
{
    [Header("충돌 설정")]
    [SerializeField] private float minImpactForce = 5f; // 효과가 발생하는 최소 충돌 속도
    [SerializeField] private GameObject particleEffectPrefab; // 파티클 시스템 프리팹
    [SerializeField] private string prefabName; // 오브젝트 풀에서 사용할 프리팹 이름
    
    [Header("폭발 설정")]
    [SerializeField] private float explosionRadius = 5f; // 폭발 영향 범위
    [SerializeField] private float explosionForce = 500f; // 폭발 힘
    [SerializeField] private float upwardsModifier = 2f; // 위쪽 방향 힘 배율
    [SerializeField] private LayerMask wallLayer; // Wall 레이어 마스크
    
    private Rigidbody rb;
    private bool hasExploded = false; // 폭발이 발생했는지 추적

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    // 오브젝트가 재사용될 때 초기화하는 메서드
    public void ResetObject()
    {
        hasExploded = false;
        
        // 리지드바디 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        // 이미 폭발했으면 무시
        if (hasExploded) return;
        
        // 충돌 속도 계산
        float impactForce = collision.relativeVelocity.magnitude;
        
        // 최소 충돌 속도보다 큰지 확인
        if (impactForce >= minImpactForce)
        {
            // 폭발 효과 실행
            Explode();
            
            // 오브젝트 풀에 반환
            ReturnToPool();
        }
    }
    
    // 즉시 폭발시키는 테스트 함수 (디버깅용)
    public void ForceExplode()
    {
        if (hasExploded) return;
        
        Explode();
        ReturnToPool();
    }
    
    private void Explode()
    {
        hasExploded = true;
        
        // 파티클 효과 생성 및 활성화
        if (particleEffectPrefab != null)
        {
            GameObject effect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                // 파티클 시스템 루프 비활성화 - 1회만 실행되도록 설정
                var main = particles.main;
                main.loop = false;
                
                particles.Play();
                
                // 파티클 시스템이 재생된 후 자동 삭제
                float duration = main.duration + main.startLifetimeMultiplier;
                Destroy(effect, duration);
            }
        }
        ImprovedSoundManager.Instance.PlaySound3D("Boom", transform.position);
        // 디버그 로그
        Debug.Log("폭발 발생! 위치: " + transform.position);
        
        // 폭발 반경 내의 Wall 레이어 오브젝트 찾기
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, wallLayer);
        Debug.Log("폭발 범위 내 오브젝트 수: " + colliders.Length);
        
        // 찾은 각 오브젝트에 폭발 효과 적용
        foreach (Collider hit in colliders)
        {
            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            
            // 리지드바디가 없으면 추가
            if (targetRb == null)
            {
                targetRb = hit.gameObject.AddComponent<Rigidbody>();
                targetRb.mass = 1f; // 기본 질량 설정
                Debug.Log("리지드바디 추가됨: " + hit.gameObject.name);
            }
            
            // 리지드바디가 Kinematic이면 해제
            if (targetRb.isKinematic)
            {
                targetRb.isKinematic = false;
                Debug.Log("isKinematic 해제됨: " + hit.gameObject.name);
            }
            
            // 모든 Constraints 완전히 해제
            targetRb.constraints = RigidbodyConstraints.None;
            Debug.Log("Constraints 해제됨: " + hit.gameObject.name);
            
            // 폭발력 적용
            targetRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            Debug.Log("폭발력 적용됨: " + hit.gameObject.name + ", 힘: " + explosionForce);
            
            // 중력 활성화
            targetRb.useGravity = true;
            
            // 선택적: Wall 오브젝트가 날아간 후 일정 시간 후 파괴
            WallDebris wallDebris = hit.gameObject.AddComponent<WallDebris>();
            if (wallDebris != null)
            {
                wallDebris.InitializeDebris(5f); // 5초 후 파괴
            }
        }
    }
    
    // 오브젝트 풀에 반환하는 메서드
    private void ReturnToPool()
    {
        // 풀 매니저 참조 가져오기
        
        ChunkedPoolManager poolManager = ChunkedPoolManager.Instance;
        
        if (poolManager != null && !string.IsNullOrEmpty(prefabName))
        {
            // 오브젝트를 풀에 반환
            poolManager.ReturnToPool(prefabName, gameObject);
        }
        else
        {
            // 풀 매니저가 없거나 프리팹 이름이 설정되지 않은 경우 그냥 파괴
            Debug.LogWarning("풀 매니저를 찾을 수 없거나 프리팹 이름이 설정되지 않았습니다. 오브젝트를 파괴합니다.");
            Destroy(gameObject);
        }
    }
    
    // 오브젝트가 활성화될 때 호출됨 - 재사용 시 초기화
    private void OnEnable()
    {
        ResetObject();
    }
}