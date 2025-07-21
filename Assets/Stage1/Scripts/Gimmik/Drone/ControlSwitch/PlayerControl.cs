using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : ControlState
{
    public override void EnterState(ControlManager context)
    {
        context.playerController.enabled = true;
        context.playerCamera.SetActive(true);

        context.droneController.enabled = false;
        context.droneCamera.gameObject.SetActive(false);

    }

    public override void UpdateState(ControlManager context)
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            context.SwitchState(new DroneControl());
        }
    }

    public override void ExitState(ControlManager context) { }
}