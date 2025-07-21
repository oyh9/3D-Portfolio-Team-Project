using UnityEngine;
using System.Collections;

public class JumpingCloud : MonoBehaviour, ITriggerableObstacle
{
    private bool canTrigger = true;  // 트리거 가능 여부를 확인하는 변수
    public float jumpForce = 18f;    // 점프 힘
    public float initialBoost = 8f;  // 초기 가속도 부스트
    
    public void TriggerObstacle(GameObject player)
    {
        // 트리거 가능한 상태일 때만 실행
        if (canTrigger)
        {
            ImprovedSoundManager.Instance.PlaySound3D("JumpingCloud", transform.position);
            
            
            // 플레이어 리지드바디 컴포넌트 가져오기 
            Rigidbody rb = player.GetComponent<Rigidbody>();
            PlayerController playerController = player.GetComponent<PlayerController>();
            
            // 바로 점프 효과 적용
            ApplyJump(rb);
            
            playerController.SetState(PlayerState.Jumping);
            // 쿨다운 시작
            StartCoroutine(TriggerCooldown());
        }
    }
    
    // 직접 점프 로직 구현
    private void ApplyJump(Rigidbody rb)
    {
        if (rb != null)
        {
            // 현재 속도 가져오기
            Vector3 currentVelocity = rb.linearVelocity;
            
            // y 속도를 0으로 초기화 (일관된 점프 높이를 위해)
            currentVelocity.y = 0f;
            rb.linearVelocity = currentVelocity;
            
            // 즉각적인 초기 속도 적용
            currentVelocity.y = initialBoost;
            rb.linearVelocity = currentVelocity;
            
            // 추가로 위쪽으로 강한 힘을 가해 탄력있게 점프
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // 점프 효과음이나 파티클 효과 등을 여기에 추가할 수 있습니다
        }
    }
    
    // 쿨다운 코루틴
    private IEnumerator TriggerCooldown()
    {
        canTrigger = false;  // 트리거 비활성화
        yield return new WaitForSeconds(0.1f);  // 0.1초 대기
        canTrigger = true;  // 트리거 다시 활성화
    }
}