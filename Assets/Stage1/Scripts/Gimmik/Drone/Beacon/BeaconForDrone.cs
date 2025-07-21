using UnityEngine;
using UnityEngine.InputSystem;

public class BeaconForDrone : MonoBehaviour
{
    public LayerMask droneLayer;
    public float rayDistance = 3f;
    public DoubleSlideDoor doubleSlideDoor;
    public SingleSlideDoor singleSlideDoor;
    public float holdTime = 1f; // E키를 눌러야 하는 시간(초)

    private float eKeyTimer = 0f;

    void Update()
    {
        // Raycast 시작 위치를 y축으로 1만큼 올림
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        Ray ray = new Ray(rayOrigin, transform.forward);
        Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, droneLayer))
        {
            if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
            {
                eKeyTimer += Time.deltaTime;
                if (eKeyTimer >= holdTime)
                {
                    if (doubleSlideDoor != null)
                        doubleSlideDoor.ToggleDoor();
                    if (singleSlideDoor != null)
                        singleSlideDoor.ToggleDoor();
                    eKeyTimer = 0f; // 한 번만 작동
                }
            }
            else
            {
                eKeyTimer = 0f;
            }
        }
        else
        {
            eKeyTimer = 0f;
        }
    }
}