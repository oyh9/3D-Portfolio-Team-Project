using System;
using System.Collections;
using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager Instance;

    [SerializeField] private SpawnPoint spawnPoint;
    
    private Vector3 currentCheckpoint;
    private int currentCheckpointOrder;
    private GameObject player;
    [SerializeField] private float respawnDelay = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentCheckpoint = spawnPoint.transform.position;
        currentCheckpointOrder = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnPlayer(player);
        }
    }
    
    public void RegisterPlayer(GameObject playerObj)
    {
        player = playerObj;
    }

    // 세이브포인트 갱신
    public void SetCheckpoint(Vector3 checkpointPosition, int newOrder)
    {
        if (newOrder > currentCheckpointOrder)
        {
            currentCheckpoint = checkpointPosition;
            currentCheckpointOrder = newOrder;
        }
    }

    public void Respawn()
    {
        RespawnPlayer(player);
    }

    // 플레이어가 죽거나 혹은 리스폰 버튼 누를 시 이 함수 호출됨
    public void RespawnPlayer(GameObject player)
    {
        StageManager.Instance?.LoadStage();
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController.Is2DMode)
        {
            ChunkedPoolManager.Instance.ResetAllObjectsToInitialPosition();
        }
        
        
        if (player != null)
        {
            Destroy(player);
        }

        StartCoroutine(WaitAndRespawn());
    }
    
    private IEnumerator WaitAndRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        StageManager.Instance?.SpawnPlayer(currentCheckpoint);
    }
}