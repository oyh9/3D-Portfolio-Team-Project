using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InvisiblePathManager : MonoBehaviour
{
    [Tooltip("정답 경로 순서대로 발판들을 배열에 넣으세요.")]
    public List<InvisibleBlock> blocks;
    private int currentIndex = 0;
    
    public void OnBlockStepped(InvisibleBlock steppedBlock)
    {
        if(steppedBlock == blocks[currentIndex])
        {
            steppedBlock.ActivatePlatform();
            currentIndex++;
            
            // 마지막 발판에 도달했으면 성공 처리 (추가 연출 가능 / 비밀 통로가 해금되거나 문이 열리거나 등등)
            if(currentIndex >= blocks.Count)
            {
                // 추가 처리
            }
        }
        else
        {
            ResetPath();
        }
    }
    
    public void ResetPath()
    {
        foreach(var block in blocks)
        {
            block.DeactivatePlatform();
            block.ResetStepped();
        }
        currentIndex = 0;
    }
}