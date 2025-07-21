using UnityEngine;

public class PlayerStateClose : IPlayerState
{
    private PlayerController _player;
    private float _animationTimer = 0f;
    private float _closeAnimationDuration = 1.2f; 
    
    public void Enter(PlayerController player)
    {
        _player = player;
        // Close 상태로 애니메이션 설정
        _player.Animator.SetBool("Open_Anim", false);
        _animationTimer = 0f;
        _player.GravityGun.isGrab = false;
    }

    public void Update()
    {
        _animationTimer += Time.deltaTime;
        
        // 애니메이션이 완료된 후에도 Close 상태 유지
        if (_animationTimer >= _closeAnimationDuration)
        {
            // 닫기 상태에서는 입력 확인
            CheckForInput();
        }
        _player?.HandleRotation();
    }

    public void FixedUpdate()
    {
        
    }

    private void CheckForInput()
    {
        // 닫긴 상태에서의 입력 처리
        if (Input.GetKey(KeyCode.W))
        {
            _player.SetState(PlayerState.Open);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            _player.SetState(PlayerState.Open);
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            // 좌측 컨트롤을 다시 누르면 열기 상태로 전환
            _player.SetState(PlayerState.Open);
        }
        
        
    }

    public void Exit()
    {
        _player.GravityGun.isGrab = true;
        _player = null;
        
    }
}