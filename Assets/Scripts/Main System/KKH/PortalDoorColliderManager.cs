using UnityEngine;

public class PortalDoorColliderManager : MonoBehaviour
{
    public GameObject effectPrefab; // ����Ʈ ������ (��ƼŬ ��)
    public GameObject portalDoor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Last Liquid"))
        {
            // 1. ����Ʈ ���� �� ���
            if (effectPrefab != null)
            {
                // ���� ��ġ���� ����Ʈ ���
                GameObject effect = Instantiate(effectPrefab, other.transform.position, Quaternion.identity);
                Destroy(effect, 3f); // 3�� �� ����Ʈ ����
            }

            // 2. ������Ʈ ��Ȱ��ȭ
            other.gameObject.SetActive(false);
            portalDoor.SetActive(false);

            DialogueManager.Instance.TutorialDialogue(4);
        }
    }
}
