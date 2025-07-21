using System.Collections;
using UnityEngine;

public class PlayerStateRoll : IPlayerState
{
    private PlayerController _player;

    private float _animationTimer = 0f;
        
    private float _goToRollTime= 1.4f;
    private float _stopToRollTime = 1.9f;
    private bool _isSpacePressed = false;
    public void Enter(PlayerController player)
    {
        _player = player;
        _player.Animator.SetBool("Roll_Anim", true);
        _animationTimer = 0f;
        _isSpacePressed = false;
        _player.GravityGun.isGrab = false;
    }

    public void Update()
    {
        if (_isSpacePressed)
            return;
        
        _animationTimer += Time.deltaTime;
        
        if (_animationTimer >= _goToRollTime)
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _player.Jump(10f);
            }
            
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _isSpacePressed = true;
                _player.Animator.SetBool("Roll_Anim", false);
                _player.StartCoroutine(ChangeStateAfterDelay(_stopToRollTime));
                return;
            }
            
        }
        // 구르기 완료 후 Idle 상태로 돌아가기
        
        
        _player.HandleRotation();
        
        
        // 구르기 도중 앞으로 빠르게 이동
        
    }

    public void FixedUpdate()
    {
        if (_isSpacePressed)
            return;
        
        if (_animationTimer >= _goToRollTime)
        {
            
            if (_animationTimer>=4f)
            {
                _player.MoveForward(_player.RollSpeed);
                
            }
            else if(_animationTimer is <= 4f and >= 3f)
            {
                _player.MoveForward(_player.RollSpeed*0.8f);
            }
            else
            {
                _player.MoveForward(_player.RollSpeed*0.6f);
            }
            
            
        }
        
    }
    
    
    

    private IEnumerator ChangeStateAfterDelay(float delay)
    {
        float timer = 0f;
        float currentSpeed = _player.GetCurrentSpeed();
        while (timer < delay)
        {
            timer += Time.fixedDeltaTime;
            
            if (timer < delay/2f)
            {
                _player.MoveForward(_player.RollSpeed * 0.6f); 
            }
            else
            {
                _player.MoveForward(0f);
            }
            yield return new WaitForFixedUpdate();
        }
        _player.SetState(PlayerState.Idle);
    }

    public void Exit()
    {
        _player.Animator.SetBool("Roll_Anim", false);
        _player.GravityGun.isGrab = true;
        
        _player = null;
    }
    
    
    
}