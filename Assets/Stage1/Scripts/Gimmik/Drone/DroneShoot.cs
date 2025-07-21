using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneShoot : MonoBehaviour
{
    [Header("총알 설정")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireCooldown = 0.2f;

    [Header("오브젝트 풀 설정")]
    public int poolSize = 10;
    private Queue<GameObject> bulletPool = new Queue<GameObject>();

    private float lastFireTime = -999f;

    void Start()
    {
        // 총알 미리 생성
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time - lastFireTime > fireCooldown)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    void Shoot()
    {
        if (bulletPool.Count == 0 || firePoint == null) return;

        GameObject bullet = bulletPool.Dequeue();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true);

        Vector3 shootDir = firePoint.forward;
        shootDir.y = 0; // Y값 제거해 수평 방향 유지
        shootDir.Normalize();

        DroneBullet bulletScript = bullet.GetComponent<DroneBullet>();
        if (bulletScript != null)
        {
            bulletScript.Fire(shootDir, bulletSpeed, () =>
            {
                bullet.SetActive(false);
                bulletPool.Enqueue(bullet);
            });
        }
    }
}
