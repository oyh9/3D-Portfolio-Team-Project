using UnityEngine;

public class BeaconForPlayer : MonoBehaviour
{
    public LayerMask playerLayer;
    public float rayDistance = 10f;
    public DoubleSlideDoor doubleSlideDoor;

    private bool doorOpened = false;

    void Update()
    {
        // 비콘의 정면 방향(transform.forward)으로 Ray를 발사
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, playerLayer))
        {
            if (!doorOpened && doubleSlideDoor != null)
            {
                doubleSlideDoor.ToggleDoor(); // 전체 도어 열기
                doorOpened = true;
            }
        }
        else
        {
            doorOpened = false;
        }
    }
}
