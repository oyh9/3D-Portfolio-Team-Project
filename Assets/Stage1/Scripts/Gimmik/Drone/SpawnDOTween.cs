using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SpawnDOTween : MonoBehaviour
{
    public GameObject dronePrefab;
    public Transform player;
    public Vector3 spawnOffset = new Vector3(-1, 1, 0);
    public float fadeDuration = 1f;
    public float spawnDistance = 5f;

    [Header("플레이어 설정")]
    public string playerTag = "Player";

    private GameObject currentDrone;

    void Update()
    {
        // 플레이어를 찾는 로직
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (Input.GetKeyDown(KeyCode.Q) && currentDrone == null && player != null)
        {
            SpawnDroneWithFade();
        }
    }

    public void SpawnDroneWithFade()
    {
        Vector3 spawnPos = player.position - player.forward * spawnDistance + Vector3.up;
        currentDrone = Instantiate(dronePrefab, spawnPos, Quaternion.identity);

        // 드론의 모든 Renderer 페이드 처리
        Renderer[] renderers = currentDrone.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // 초기 알파값 0
                Color color = mat.color;
                color.a = 0;
                mat.color = color;

                // 페이드 인
                mat.DOFade(1f, fadeDuration);
            }
        }

        // 드론 이동 및 FollowDrone 활성화
        currentDrone.transform.DOMove(player.position + player.TransformDirection(spawnOffset), fadeDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                FollowDrone follow = currentDrone.GetComponent<FollowDrone>();
                if (follow != null)
                    follow.enabled = true;
            });

        // 드론 카메라 설정
        DroneCamera droneCameraScript = FindObjectOfType<DroneCamera>();
        if (droneCameraScript != null)
        {
            droneCameraScript.SetDroneCamera(currentDrone);
        }
    }

    public void DespawnCurrentDrone()
    {
        StartCoroutine(DestroyDrone());
    }

    IEnumerator DestroyDrone()
    {
        yield return null;
        if (currentDrone != null)
        {
            Destroy(currentDrone); // 현재 드론 제거
            currentDrone = null;
        }
        yield return null;
        
        SpawnDroneWithFade();
    }
}
