using UnityEngine;

public class PlayerStateJump : IPlayerState
{
    private PlayerController _player;
    private Rigidbody rb;
    private bool isFalling=false;
    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("IsJump", true);
        _player.JumpStart(10f, 0.7f);
        _player.Animator.SetBool("IsGround", false);
        rb=_player.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        
        
        
        if (rb != null)
        {
            if (_player.CurrentGravityDirection == Vector3.down)
            {
                if (rb.linearVelocity.y < -1)
                {
                
                    _player.Animator.SetBool("IsFall", true);
                    isFalling=true;
                }
                
            }
            else if (_player.CurrentGravityDirection == Vector3.forward)
            {
                if (rb.linearVelocity.z > 1)
                {
                
                    _player.Animator.SetBool("IsFall", true);
                    isFalling=true;
                }
                
            }
            
        }

        if (_player.IsGrounded&&isFalling)
        {
            _player.Animator.SetBool("IsGround", true);
            _player.ChangeStateDelay(PlayerState.Idle, 0.15f);
            //_player.SetState(PlayerState.Idle);
        }

        if (!_player.readyJump)
        {
            _player.HandleRotation();

        }
        
        // 회전 처리
        
       
    }

    public void FixedUpdate()
    {
        if (_player.Is2DMode)
        {
            if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))&&!_player.readyJump)
            {
                _player.MoveForward(_player.MoveSpeed);
            }
            
        }
        else
        {
            if (Input.GetKey(KeyCode.W)&&!_player.readyJump)
            {
                _player.MoveForward(_player.MoveSpeed);
            }
        }
        
    }

    public void Exit()
    {
        _player.Animator.SetBool("IsFall", false);
        _player.Animator.SetBool("IsJump", false);
        isFalling=false;
        
        //_player = null;

    }
    
    
    
    
}