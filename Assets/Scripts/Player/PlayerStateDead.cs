using UnityEngine;

public class PlayerStateDead : IPlayerState
{
    private PlayerController _player;
    
    
    public void Enter(PlayerController player)
    {
        GameManager.Instance.die=true;
        ImprovedSoundManager.Instance.PauseBGM();
        _player = player;

        if (player.Is2DMode)
        {
            ChunkedPoolManager.Instance.OffPlayerTransform(_player.transform);
        }
        
        Rigidbody rb = _player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 속도를 0으로 설정
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        
            // 물리 시뮬레이션 비활성화
            rb.isKinematic = true;
        }
        SphereCollider collider = _player.GetComponent<SphereCollider>();
        collider.enabled = false;
        
        // Close 상태로 애니메이션 설정
        _player.dissolve.ApplyDissolveToAllTargets();
        _player.Dead();
        
        UICircleTransition.Instance.CircleFadeOut();
        _player.GravityGun.isGrab = false;
        ImprovedSoundManager.Instance.PlaySound2D("Die");
        
    }

    public void Update()
    {
        
    }

    public void FixedUpdate()
    {
        
    }
    

    public void Exit()
    {
        _player.GravityGun.isGrab = true;
        _player = null;
        
    }
}