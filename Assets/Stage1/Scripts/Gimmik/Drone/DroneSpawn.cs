using UnityEngine;

public class DroneSpawn : MonoBehaviour
{
    [Header("��� ����")]
    public GameObject dronePrefab;
    public Vector3 spawnOffset = new Vector3(-1, 1, 0);

    [Header("�÷��̾� ����")]
    public string playerTag = "Player";
    public KeyCode summonKey = KeyCode.E;

    private Transform player;
    private GameObject spawnedDrone;

    void Update()
    {
        // �÷��̾ ã�� ���� ��� �õ�
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }

        // F Ű �Է� �� ��� ����
        if (player != null && spawnedDrone == null && Input.GetKeyDown(summonKey))
        {
            Vector3 spawnPos = player.position + spawnOffset;
            spawnedDrone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);

            // FollowDrone ��ũ��Ʈ�� �÷��̾� �Ҵ�
            FollowDrone follower = spawnedDrone.GetComponent<FollowDrone>();
            if (follower != null)
            {
                follower.SetPlayer(player);
            }
        }
    }
}
