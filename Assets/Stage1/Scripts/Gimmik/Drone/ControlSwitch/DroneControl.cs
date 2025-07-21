using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class DroneControl : ControlState
{
    public override void EnterState(ControlManager context)
    {
        context.playerController.enabled = false;
        context.playerCamera.SetActive(false);

        context.droneController.enabled = true;
        context.droneCamera.gameObject.SetActive(true);

        // CinemachineCamera에서 Follow 및 LookAt 설정
        var cineCam = context.droneCamera.GetComponent<CinemachineVirtualCameraBase>();
        if (cineCam != null)
        {
            cineCam.Follow = context.drone.transform;
            cineCam.LookAt = context.drone.transform;
        }

    }

    public override void UpdateState(ControlManager context)
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            context.SwitchState(new PlayerControl());
        }
    }

    public override void ExitState(ControlManager context) { }
}
