using UnityEngine;

public class PlayerStateWalk : IPlayerState
{
    private PlayerController _player;

    private string _walkSoundName = "Walk3D";
    
    public void Enter(PlayerController player)
    {
        _player = player;
        _player.isWalking = true;
        _player.Animator.SetBool("Walk_Anim", true);
    }

    public void Update()
    {
        if (_player.Is2DMode)
        {
            if (!Input.GetKey(KeyCode.A)&&!Input.GetKey(KeyCode.D))
            {
                _player.SetState(PlayerState.Idle);
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _player.SetState(PlayerState.Roll);
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_player.IsGrounded)
                {
                    _player.SetState(PlayerState.Jump);
                    return;
                    
                }
            }

            // 오픈 전환
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _player.SetState(PlayerState.Close);
                return;
            }
            
        }
        else
        {
            // 걷기 중단 시 Idle 상태로 전환
            if (!Input.GetKey(KeyCode.W))
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
        _player.isWalking = false;
       
        _player.Animator.SetBool("Walk_Anim", false);
        ImprovedSoundManager.Instance.StopSoundGroup(_walkSoundName);

    }
}