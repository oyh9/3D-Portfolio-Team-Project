using UnityEngine;
using System.Collections;

public class SpearController : MonoBehaviour
{
    public float raycastDistance = 5f; // 레이캐스트 거리
    public float repositionYValue = 1.5f; // 재배치될 Y 위치
    public float attackDuration = 0.5f; // 공격 지속 시간
    
    private Vector3 originalPosition; // 창의 원래 위치
    private bool isAttacking = false;
    
    void Start()
    {
        // 시작할 때 원래 위치 저장
        originalPosition = transform.position;
    }
    
    void Update()
    {
        if (!isAttacking)
        {
            // 하늘 방향(Y축 양의 방향)으로 레이캐스트
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.up, out hit, raycastDistance))
            {
                // 레이캐스트에 맞은 오브젝트가 플레이어인지 확인
                if (hit.collider.CompareTag("Player"))
                {
                    // 공격 코루틴 시작
                    StartCoroutine(AttackAndReturn());
                }
            }
            
            // 디버그용 레이캐스트 시각화
            Debug.DrawRay(transform.position, Vector3.up * raycastDistance, Color.red);
        }
    }
    
    // 공격 후 원래 위치로 돌아가는 코루틴
    private IEnumerator AttackAndReturn()
    {
        isAttacking = true;
        
        // 공격 위치로 이동 (y = 1.5)
        Vector3 attackPosition = transform.position;
        attackPosition.y = repositionYValue;
        transform.position = attackPosition;
        
        Debug.Log("플레이어 감지, 창 공격 시작");
        
        // 공격 지속 시간만큼 대기
        yield return new WaitForSeconds(attackDuration);
        
        // 원래 위치로 돌아가기
        transform.position = originalPosition;
        
        Debug.Log("창이 원래 위치로 돌아옴");
        
        // 약간의 대기 시간을 두어 연속 공격 방지
        yield return new WaitForSeconds(0.2f);
        
        isAttacking = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 트리거에 충돌한 오브젝트가 플레이어인지 확인
        if (other.CompareTag("Player"))
        {
            // 플레이어 컨트롤러 스크립트 참조 가져오기
            PlayerController playerController = other.GetComponent<PlayerController>();
            
            // 플레이어 컨트롤러 스크립트가 존재하는지 확인 후 함수 호출
            if (playerController != null)
            {
                playerController.SetState(PlayerState.Dead);
                //playerController.OnHitBySpear();
                Debug.Log("플레이어가 창에 맞음, 함수 호출됨");
            }
        }
    }
}