using UnityEngine;
using System.Collections;

public class DoubleSlideDoor : MonoBehaviour
{
    public Transform upDoor;
    public Transform downDoor;

    public Vector3 upOpenOffset = new Vector3(0, 0.9f, 0);
    public Vector3 downOpenOffset = new Vector3(0, -0.9f, 0);
    public float openSpeed = 2f;

    private Vector3 upClosedPos, downClosedPos;
    private Vector3 upOpenPos, downOpenPos;
    private bool isOpen = false;

    void Start()
    {
        if (upDoor == null || downDoor == null)
        {
            Debug.LogError("Doors not assigned.");
            return;
        }

        upClosedPos = upDoor.position;
        downClosedPos = downDoor.position;
        upOpenPos = upClosedPos + upOpenOffset;
        downOpenPos = downClosedPos + downOpenOffset;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(MoveDoor());
    }

    IEnumerator MoveDoor()
    {
        Vector3 targetLeft = isOpen ? upOpenPos : upClosedPos;
        Vector3 targetRight = isOpen ? downOpenPos : downClosedPos;

        while (Vector3.Distance(upDoor.position, targetLeft) > 0.01f || Vector3.Distance(downDoor.position, targetRight) > 0.01f)
        {
            upDoor.position = Vector3.MoveTowards(upDoor.position, targetLeft, openSpeed * Time.deltaTime);
            downDoor.position = Vector3.MoveTowards(downDoor.position, targetRight, openSpeed * Time.deltaTime);
            yield return null;
        }

        upDoor.position = targetLeft;
        downDoor.position = targetRight;
    }

    // 플레이어가 조작할 때 한쪽만 열리게
    public void ToggleDoorOneSide()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(MoveDoorOneSide());
    }

    IEnumerator MoveDoorOneSide()
    {
        Vector3 targetUp = isOpen ? upOpenPos : upClosedPos;
        Vector3 targetDown = downClosedPos; // 한쪽만 열림

        while (Vector3.Distance(upDoor.position, targetUp) > 0.01f || Vector3.Distance(downDoor.position, targetDown) > 0.01f)
        {
            upDoor.position = Vector3.MoveTowards(upDoor.position, targetUp, openSpeed * Time.deltaTime);
            downDoor.position = Vector3.MoveTowards(downDoor.position, targetDown, openSpeed * Time.deltaTime);
            yield return null;
        }
        upDoor.position = targetUp;
        downDoor.position = targetDown;
    }
}
