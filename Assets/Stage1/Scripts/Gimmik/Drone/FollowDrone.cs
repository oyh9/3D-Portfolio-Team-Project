using UnityEngine;

public class FollowDrone : MonoBehaviour
{
    [Header("드론 설정")]
    public Vector3 offset = new Vector3(-1f, 1f, 0f);  // 드론의 위치 오프셋
    public float followSpeed = 4f;                     // 드론이 따라가는 속도
    public string playerTag = "Player";                // 찾을 태그

    private Transform player;                          // 따라갈 플레이어
    private bool tryFindPlayer = true;                 // 플레이어 찾기 시도 여부
    private SpawnDOTween spawner;                      // 드론을 소환하는 스크립트 참조
    private bool isFollowing = true;                   // 드론이 플레이어를 따라가는 상태

    void Start()
    {
        FindPlayer(); // 시작 시 플레이어 찾기 시도

        // SpawnDOTween 스크립트 찾기
        spawner = FindObjectOfType<SpawnDOTween>();
    }

    void Update()
    {
        // Tab 키를 눌렀을 때 따라가기 상태 전환
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleFollowState();
        }

        if (player == null && tryFindPlayer)
        {
            FindPlayer(); // 플레이어를 찾지 못했을 경우 다시 찾기 시도
        }
    }

    void LateUpdate()
    {
        if (!isFollowing || player == null) return;

        // 목표 위치 = 플레이어 위치 + 오프셋
        Vector3 targetPos = player.position
                          + player.right * offset.x
                          + player.up * offset.y
                          + player.forward * offset.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // 드론 회전 = 플레이어의 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, player.rotation, followSpeed * Time.deltaTime);
    }

    // 플레이어를 수동으로 설정하는 메서드
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        tryFindPlayer = false;
    }

    // 태그로 플레이어 찾기
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
            tryFindPlayer = false;
        }
    }

    // 따라가기 상태 전환 및 드론 재소환
    private void ToggleFollowState()
    {
        isFollowing = !isFollowing;

        if (isFollowing)
        {
            // 따라가기를 다시 시작할 때 드론 재소환
            if (spawner != null)
            {
                spawner.DespawnCurrentDrone();
                //spawner.SpawnDroneWithFade();
            }
        }
        else
        {
            // 따라가기를 중지할 때는 드론을 그대로 둠
            Debug.Log("드론이 플레이어 따라가기를 중지했습니다.");
        }
    }
}
