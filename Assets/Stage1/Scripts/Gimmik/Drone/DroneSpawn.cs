using UnityEngine;

public class DroneSpawn : MonoBehaviour
{
    [Header("드론 설정")]
    public GameObject dronePrefab;
    public Vector3 spawnOffset = new Vector3(-1, 1, 0);

    [Header("플레이어 설정")]
    public string playerTag = "Player";
    public KeyCode summonKey = KeyCode.E;

    private Transform player;
    private GameObject spawnedDrone;

    void Update()
    {
        // 플레이어를 찾지 못한 경우 시도
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }

        // F 키 입력 시 드론 생성
        if (player != null && spawnedDrone == null && Input.GetKeyDown(summonKey))
        {
            Vector3 spawnPos = player.position + spawnOffset;
            spawnedDrone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);

            // FollowDrone 스크립트에 플레이어 할당
            FollowDrone follower = spawnedDrone.GetComponent<FollowDrone>();
            if (follower != null)
            {
                follower.SetPlayer(player);
            }
        }
    }
}
