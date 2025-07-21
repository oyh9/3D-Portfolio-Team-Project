using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("점프패드 설정")]
    public float jumpForce = 15f; // 점프 힘
    public LayerMask playerLayer; // 플레이어 레이어
    public float raycastDistance = 1.5f; // Raycast 거리
    public float cooldown = 0.5f; // 점프 쿨다운 시간

    private float lastJumpTime = -999f; // 마지막 점프 시간

    void Update()
    {
        // 위 방향으로 Raycast를 쏜다
        Ray ray = new Ray(transform.position, Vector3.up);
        Debug.DrawRay(transform.position, Vector3.up * raycastDistance, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, playerLayer))
        {
            // 쿨다운이 지났을 때만 작동
            if (Time.time - lastJumpTime >= cooldown)
            {
                // PlayerController 가져오기 (이름은 실제 사용하는 이름에 맞춰서 수정)
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                if (player != null)
                {
                    //player.ExternalJump(jumpForce);
                    lastJumpTime = Time.time;
                }
            }
        }
    }
}
