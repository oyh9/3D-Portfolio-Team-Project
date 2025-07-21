using UnityEngine;
using System.Collections;

public class SingleSlideDoor : MonoBehaviour
{
    public Transform door;
    public Vector3 openOffset = new Vector3(0, 3f, 0);
    public float openSpeed = 2f;

    private Vector3 closedPos, openPos;
    private bool isOpen = false;

    void Start()
    {
        closedPos = door.position;
        openPos = closedPos + openOffset;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(MoveDoor());
    }

    IEnumerator MoveDoor()
    {
        Vector3 target = isOpen ? openPos : closedPos;
        while (Vector3.Distance(door.position, target) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, target, openSpeed * Time.deltaTime);
            yield return null;
        }
        door.position = target;
    }
}
