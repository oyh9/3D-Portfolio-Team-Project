using UnityEngine;

public class PlayerStateAir : IPlayerState
{
    private PlayerController _player;
    private Rigidbody rb;
    
    // 레이캐스트 관련 변수
    private float rayDistance = 0.6f; // 레이캐스트 거리를 더 짧게 조정
    private LayerMask groundLayer; // 그라운드 레이어 마스크
    private Vector3 raycastOffset = new Vector3(0, 0.3f, 0); // 캐릭터 발끝에서 약간 위로 오프셋
    
    public void Enter(PlayerController player)
    {
        _player = player;
        rb = _player.GetComponent<Rigidbody>();
        
        // groundLayer 값 설정
        groundLayer = LayerMask.GetMask("Ground"); // 실제 레이어 이름에 맞게 수정
        
        
        _player.Animator.SetBool("IsAir", true);
        
    }

    public void Update()
    {
        // 레이캐스트로 지면 확인
        if (CheckGroundBelowPlayer())
        {
            _player.Animator.SetBool("IsGround", true);
            _player.SetState(PlayerState.Idle);
        }
        
        _player.HandleRotation();
    }

    // 레이캐스트를 사용해 플레이어 아래 지면 체크
    private bool CheckGroundBelowPlayer()
    {
        RaycastHit hit;
        // 플레이어의 로컬 위치에서 오프셋을 더한 위치에서 현재 중력 방향으로 레이를 쏨
        Vector3 rayOrigin = _player.transform.position + _player.transform.TransformDirection(raycastOffset);
        
        // 디버그 레이 표시 (개발 중에만 사용)
        Debug.DrawRay(rayOrigin, _player.CurrentGravityDirection * rayDistance, Color.red);
        
        if (Physics.Raycast(rayOrigin, _player.CurrentGravityDirection, out hit, rayDistance, groundLayer))
        {
            // 레이가 그라운드 레이어에 닿았다면 true 반환
            return true;
        }
        return false;
    }

    public void FixedUpdate()
    {
        if (_player.Is2DMode)
        {
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                _player.MoveForward(15f);
            }
        }
    }

    public void Exit()
    {
        _player.Animator.SetBool("IsAir", false);
    }
}