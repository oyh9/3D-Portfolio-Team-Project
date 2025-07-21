using UnityEngine;

public class PlayerStateFire : MonoBehaviour, IPlayerState
{
    private PlayerController _player;
    private float _fireSpeed=0f;
    
    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("IsFire", true);
        _player.LockRight2D();
        _fireSpeed = _player.fireSpeed;
    }

    public void Update()
    {
        if (_player.IsGrounded)
        {
            _player.SetState(PlayerState.Idle);
            return;
        }
        
    }

    public void FixedUpdate()
    {
        _player.MoveForward(_fireSpeed);
    }

    public void Exit()
    {
        
    }
}
