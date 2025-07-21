using UnityEngine;

public class TriggerCollider : MonoBehaviour
{
    public int triggerIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StageManager.Instance.OnTriggerEntered(triggerIndex);
        }
    }
}
