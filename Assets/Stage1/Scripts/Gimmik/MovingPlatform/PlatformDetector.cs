using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
    public float rayDistance = 1.0f; // 플레이어 발 아래 감지 거리
    public LayerMask platformLayer;


    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, platformLayer))
        {
            var platform = hit.collider.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                platform.MoveUp();
            }
        }
    }
}
