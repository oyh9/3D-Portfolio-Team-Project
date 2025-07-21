using UnityEngine;
using System;

public class DroneBullet : MonoBehaviour
{
    public float lifetime = 1f;
    private Action returnToPool;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction, float speed, Action returnCallback)
    {
        returnToPool = returnCallback;
        rb.linearVelocity = direction * speed;
        CancelInvoke(); // 이전 타이머 제거
        Invoke(nameof(Deactivate), lifetime);
    }

    void Deactivate()
    {
        rb.linearVelocity = Vector3.zero;
        returnToPool?.Invoke();
    }

    void OnDisable()
    {
        CancelInvoke(); // 비활성화시 타이머 제거
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyDrone"))
        {
            collision.gameObject.GetComponent<EnemyDroneState>()?.OnHitByBullet();
        }

        // 풀로 반환
        Deactivate();
    }
}
