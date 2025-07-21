using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSwitch : MonoBehaviour
{
    [Header("오브젝트 참조")]
    public GameObject player;              // 씬에 배치된 플레이어
    public GameObject drone;               // 씬에 배치된 드론

    [Header("카메라 참조")]
    public GameObject playerCamera;        // 플레이어용 카메라 (기본 카메라)

    private Camera droneCamera;
    private bool controllingPlayer = true;

    void Start()
    {
        if (player == null || drone == null || playerCamera == null)
        {
            Debug.LogError("ControlSwitch의 오브젝트 참조가 누락되었습니다.");
            enabled = false;
            return;
        }

        // 드론의 자식에서 Camera 찾기
        droneCamera = drone.GetComponentInChildren<Camera>(true); // 비활성 상태도 탐색
        if (droneCamera == null)
        {
            Debug.LogError("드론 오브젝트에 자식 카메라가 없습니다.");
            enabled = false;
            return;
        }

        SetControlState(true); // 처음에는 플레이어 조작 상태
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            controllingPlayer = !controllingPlayer;
            SetControlState(controllingPlayer);
        }
    }

    void SetControlState(bool isPlayer)
    {
        // 안전하게 컴포넌트 활성/비활성
        var playerController = player.GetComponent<PlayerController>();
        var droneController = drone.GetComponent<DroneController>();
        var followDrone = drone.GetComponent<FollowDrone>();

        if (playerController != null) playerController.enabled = isPlayer;
        if (droneController != null) droneController.enabled = !isPlayer;
        if (followDrone != null) followDrone.enabled = isPlayer;

        // 카메라 전환
        playerCamera.SetActive(isPlayer);
        if (droneCamera != null)
            droneCamera.gameObject.SetActive(!isPlayer);

        // 드론 -> 플레이어로 복귀 시 드론 위치 초기화
        if (isPlayer && drone != null)
        {
            drone.transform.position = Vector3.zero;
        }
    }
}
