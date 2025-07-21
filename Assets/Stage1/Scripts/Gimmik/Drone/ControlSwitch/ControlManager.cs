using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    [Header("������Ʈ")]
    public GameObject player;
    public GameObject drone;

    [Header("ī�޶�")]
    public GameObject playerCamera;
    public Camera droneCamera;

    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public DroneController droneController;
    [HideInInspector] public FollowDrone followDrone;

    private ControlState currentState;

    
    
    
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        droneController = drone.GetComponent<DroneController>();
        followDrone = drone.GetComponent<FollowDrone>();

        currentState = new PlayerControl();
        currentState.EnterState(this);
    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(ControlState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }
    
    
    
    
    
}
