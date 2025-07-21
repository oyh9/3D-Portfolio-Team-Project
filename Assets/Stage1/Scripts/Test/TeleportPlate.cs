using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportPlate : MonoBehaviour
{
    [Header("텔레포트 대상 플레이어")]
    public GameObject player;

    [Header("텔레포트 위치")]
    public Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != player) return;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
            player.transform.position = teleportTarget.position;
            cc.enabled = true;
        }
        else
        {
            player.transform.position = teleportTarget.position;
        }
    }
}
