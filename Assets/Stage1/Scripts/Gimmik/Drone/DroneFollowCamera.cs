using UnityEngine;

public class DroneFollowCamera : MonoBehaviour
{
    public static DroneFollowCamera Instance;

    public Transform drone; // ���� ���� ���� ���
    public Vector3 offset = new Vector3(0, 3, -6);
    public float followSpeed = 5f;
    public float rotationSpeed = 5f;

    void Awake()
    {
        Instance = this; // �̱��� �Ҵ�
    }

    void LateUpdate()
    {
        if (drone == null) return;

        Vector3 targetPos = drone.position + drone.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        Quaternion targetRot = Quaternion.LookRotation(drone.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    // �ܺο��� ��� �Ҵ��ϴ� �Լ�
    public void SetTarget(Transform droneTransform)
    {
        drone = droneTransform;
    }
}
