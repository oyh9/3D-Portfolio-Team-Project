using UnityEngine;

public class PlayerStateIdle : IPlayerState
{
    private PlayerController _player;

    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("Walk_Anim", false);
        _player.Animator.SetBool("Roll_Anim", false);
        _player.Animator.SetBool("IsAir", false);
        _player.Animator.SetBool("IsFire", false);
    }

    public void Update()
    {
        if (_player.Is2DMode)
        {
            if (Input.GetKey(KeyCode.D))
            {
                _player.SetState(PlayerState.Walk);
            }
            else if(Input.GetKey(KeyCode.A))
            {
                _player.SetState(PlayerState.Walk);
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _player.SetState(PlayerState.Roll);
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _player.SetState(PlayerState.Close);
            }else if (Input.GetKeyDown(KeyCode.Space)&&_player.IsGrounded)
            {
                _player.SetState(PlayerState.Jump);    
            }
            
            
        }
        else
        {
            
            // 키 입력에 따른 상태 전환
            if (Input.GetKey(KeyCode.W)&&_player.IsGrounded)
            {
                _player.SetState(PlayerState.Walk);
            }
            else if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _player.SetState(PlayerState.Roll);
            }
            else if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _player.SetState(PlayerState.Close);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_player.IsGrounded)
                {
                    _player.SetState(PlayerState.Jump);
                    return;
                    
                }    
            }

            
            if(Input.GetKey(KeyCode.A)&&Input.GetKey(KeyCode.D))
                return;
            if (Input.GetKey(KeyCode.A))
            {
                _player.SetState(PlayerState.Left);
            }

            if (Input.GetKey(KeyCode.D))
            {
                _player.SetState(PlayerState.Right);
            }
            
        }
        
        // 회전 처리
        _player.HandleRotation();
    }

    public void FixedUpdate()
    {
        
    }

    public void Exit()
    {
        
        // 상태 종료 시 필요한 처리
    }
}