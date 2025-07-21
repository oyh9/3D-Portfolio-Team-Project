using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneCamera : MonoBehaviour
{
    public GameObject playerCamera;            // 플레이어용 기본 카메라
    public GameObject droneCamera;             // 드론 추적용 카메라 (FollowDroneCamera 붙어 있어야 함)

    private bool isPlayerView = true;

    void Start()
    {
        // 시작 시 기본 카메라 활성화
        SetCameraState(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isPlayerView = !isPlayerView;
            SetCameraState(isPlayerView);
        }
    }

    void SetCameraState(bool playerView)
    {
        // 플레이어 카메라 활성화/비활성화
        if (playerCamera != null)
        {
            playerCamera.SetActive(playerView);
        }

        // 드론 카메라 활성화/비활성화
        if (droneCamera != null)
        {
            droneCamera.SetActive(!playerView);
        }
    }

    // 드론 소환 후 드론 카메라를 설정하는 메서드
    public void SetDroneCamera(GameObject drone)
    {
        Transform droneCameraTransform = drone.transform.Find("DroneCamera");
        if (droneCameraTransform != null)
        {
            droneCamera = droneCameraTransform.gameObject;
        }
        else
        {
            Debug.LogError("드론의 자식 오브젝트 중 'DroneCamera'를 찾을 수 없습니다.");
        }
    }
}
