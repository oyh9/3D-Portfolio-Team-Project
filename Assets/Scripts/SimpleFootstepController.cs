using System;
using UnityEngine;

public class SimpleFootstepController : MonoBehaviour
{
    [SerializeField] private string footstepSoundName = "Footstep";
    [SerializeField] private string jumpSoundName = "Jump";
    [SerializeField] private string rollSoundName = "Roll";
    [SerializeField]private PlayerController player;
    
    private string _footstepSound= "Walk3D";
    private string _jumpSound = "Jump3D";
    private string _landSound = "Land";


    // 애니메이션 이벤트에서 호출할 함수
    public void PlayFootstep()
    {
        if (player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(footstepSoundName, transform.position);
            
        }
        
    }

    public void PlayFootstepBasic()
    {
        if(!player.Is2DMode&&player.isWalking)
        {
            ImprovedSoundManager.Instance.PlaySound3D(_footstepSound, transform.position);

        }
    }

    public void PlayRotation()
    {
        if(!player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(_footstepSound, transform.position);

        }
        
        
    }

    public void PlayLand()
    {
        if(!player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(_landSound, transform.position);

        }
    }
    
    public void PlayJump()
    {
        if (player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(jumpSoundName, transform.position);
            
        }
        
    }

    public void BasicJump()
    {
        if (!player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(_jumpSound, transform.position);
            
        }

    }
    

    public void PlayRoll()
    {
        if (player.Is2DMode)
        {
            ImprovedSoundManager.Instance.PlaySound3D(rollSoundName, transform.position);
        }
        else
        {
            ImprovedSoundManager.Instance.PlaySound3D(rollSoundName, transform.position);

        }
    }
    
    // 애니메이션 이벤트 함수 (파라미터 있는 버전)
    public void PlayFootstepWithVolume(float volume)
    {
        // 볼륨 조절이 필요한 경우
        var audioSource = ImprovedSoundManager.Instance.PlaySound3DAttached(footstepSoundName, transform);
        if (audioSource != null)
        {
            audioSource.volume *= volume;
        }
    }
}