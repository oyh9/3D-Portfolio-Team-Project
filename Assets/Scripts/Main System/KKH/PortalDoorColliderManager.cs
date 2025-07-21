using UnityEngine;

public class PortalDoorColliderManager : MonoBehaviour
{
    public GameObject effectPrefab; // 이펙트 프리팹 (파티클 등)
    public GameObject portalDoor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Last Liquid"))
        {
            // 1. 이펙트 생성 및 재생
            if (effectPrefab != null)
            {
                // 현재 위치에서 이펙트 재생
                GameObject effect = Instantiate(effectPrefab, other.transform.position, Quaternion.identity);
                Destroy(effect, 3f); // 3초 후 이펙트 제거
            }

            // 2. 오브젝트 비활성화
            other.gameObject.SetActive(false);
            portalDoor.SetActive(false);

            DialogueManager.Instance.TutorialDialogue(4);
        }
    }
}
