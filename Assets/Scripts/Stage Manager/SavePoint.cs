using UnityEngine;

public class SavePoint : MonoBehaviour
{
    // 세이브 포인트 순서
    public int order = 0;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRespawnManager.Instance.SetCheckpoint(transform.position, order);
            
            // 나중에 저장 기능 만들 시( 수동/자동 저장 )
            //StageManager.Instance?.SaveStage();
        }
    }
}
