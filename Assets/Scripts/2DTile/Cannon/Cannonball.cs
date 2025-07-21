using System.Collections;
using UnityEngine;

public class Cannonball : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private ParticleSystem impactEffect;
    
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Collider가 트리거로 설정되어 있는지 확인
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    private void OnEnable()
    {
        // 활성화될 때 자동으로 라이프타임 후에 풀로 반환
        StartCoroutine(ReturnToPoolAfterLifetime());
    }
    
    public void Launch(Vector3 direction)
    {
        if (rb)
        {
            rb.linearVelocity = direction.normalized * speed;
            
            // 물리 충돌은 필요없지만 Rigidbody는 움직임에 필요하므로 kinematic으로 설정
            // rb.isKinematic = true; // 물리효과가 완전히 필요없다면 이 옵션 활성화
        }
    }
    
    private IEnumerator ReturnToPoolAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        if (CannonballPoolManager.Instance != null)
        {
            CannonballPoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // OnCollisionEnter 대신 OnTriggerEnter 사용
    private void OnTriggerEnter(Collider other)
    {
        
        PlayerController player = other.GetComponent<PlayerController>();
        
        ImprovedSoundManager.Instance.PlaySound3D("Boom", transform.position);
        if (player != null)
        {
            player.SetState(PlayerState.Dead);
        }
        
        // 대포알이 무언가와 충돌했을 때 처리
        Debug.Log("Cannonball hit");
        // 데미지를 줄 수 있는 오브젝트라면 데미지 처리
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        
        // 충돌 효과 재생
        if (impactEffect != null)
        {
            ParticleSystem effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            effect.Play();
            
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
        }
        
        // 풀로 반환
        ReturnToPool();
    }
}

// 데미지를 받을 수 있는 오브젝트용 인터페이스
public interface IDamageable
{
    void TakeDamage(float amount);
}