using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    
    private bool dialogueActive = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player =other.GetComponent<PlayerController>();
            player.SetState(PlayerState.Dead);
            //PlayerRespawnManager.Instance.RespawnPlayer(other.gameObject);
            if (dialogueActive)
            {
                dialogueActive = false;
                DialogueManager.Instance.TutorialDialogue(4);
            }
        }
    }
}
