using UnityEngine;

public class DroneFollowCamera : MonoBehaviour
{
    public static DroneFollowCamera Instance;

    public Transform drone; // 현재 추적 중인 드론
    public Vector3 offset = new Vector3(0, 3, -6);
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    void Awake()
    {
        Instance = this; // 싱글톤 할당
    }

    void LateUpdate()
    {
        if (drone == null) return;

        Vector3 targetPos = drone.position + drone.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        Quaternion targetRot = Quaternion.LookRotation(drone.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    // 외부에서 드론 할당하는 함수
    public void SetTarget(Transform droneTransform)
    {
        drone = droneTransform;
    }
}
