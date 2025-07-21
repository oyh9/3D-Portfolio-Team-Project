using UnityEngine;

public class PlayerStateOpen : IPlayerState
{
    private PlayerController _player;
    private float _animationTimer = 0f;
    private float _openAnimationDuration = 3.5f*2/3f;
    
    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("Open_Anim", true);
        _animationTimer = 0f;
    }

    public void Update()
    {
        
        _animationTimer += Time.deltaTime;
        _player.HandleRotation();
        if (_animationTimer >= _openAnimationDuration)
        {
            _player.SetState(PlayerState.Idle);
            
        }
        
        
    }

    public void FixedUpdate()
    {
        
    }


    public void Exit()
    {
        // Open/Close 상태값은 유지됨 (Open_Anim)
        _player = null;
    }
}