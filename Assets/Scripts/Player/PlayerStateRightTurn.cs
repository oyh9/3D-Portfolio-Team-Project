using UnityEngine;

public class PlayerStateRightTurn : IPlayerState
{
    private PlayerController _player;

    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("TurnRight", true);
    }

    public void Update()
    {
        if (!_player.Is2DMode)
        {
            // 걷기 중단 시 Idle 상태로 전환
            if (Input.GetKey(KeyCode.W))
            {
                _player.SetState(PlayerState.Walk);
                return;
            }

            if (!Input.GetKey(KeyCode.D))
            {
                _player.SetState(PlayerState.Idle);
                return;
            }

            // 롤 전환
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _player.SetState(PlayerState.Roll);
                return;
            }

            // 오픈 전환
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _player.SetState(PlayerState.Close);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _player.SetState(PlayerState.Jump);
                return;
            }
            if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            {
                _player.SetState(PlayerState.Idle);
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                _player.SetState(PlayerState.Left);
            }
            
        }
        
        

        // 회전 처리
        _player.HandleRotation();
        
        // 앞으로 이동
        
    }

    public void FixedUpdate()
    {
        _player.MoveForward(_player.MoveSpeed);
    }

    public void Exit()
    {
        _player.Animator.SetBool("TurnRight", false);
        
    }
}
