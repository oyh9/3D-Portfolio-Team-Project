using JetBrains.Annotations;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    // 트리거 감지를 위한 태그
    public string playerTag = "Player";
    
    [SerializeField]private int index = 0;
    [SerializeField]private bool durable = false;

    [SerializeField] private bool onOff=false;
    [SerializeField] [CanBeNull] private GameObject triggerObject;
    
    
    // 이펙트가 이미 트리거 되었는지 확인하는 플래그
    private bool isTriggered = false;
    
    
    private void OnTriggerEnter(Collider other)
    {
        // 아직 트리거되지 않았고 플레이어 태그를 가진 오브젝트와 충돌했는지 확인
        if (!isTriggered && other.CompareTag(playerTag))
        {
            isTriggered = true;
            ImprovedSoundManager.Instance.PlaySound2D("MovePoint");
            DialogueManager.Instance.TutorialDialogue(index);
            if (!durable)
            {
                gameObject.SetActive(false);
            }

            if (onOff)
            {
                SetActive();
            }
        }
    }

    public void SetActive()
    {
        if (triggerObject != null)
        {
            triggerObject.SetActive(true);
        }
    }
    
    
}
