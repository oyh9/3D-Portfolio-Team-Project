using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float moveHeight = 2f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isMovingUp = false;

    private void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * moveHeight;
    }

    private void Update()
    {
        Vector3 goal = isMovingUp ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, goal, moveSpeed * Time.deltaTime);
    }

    public void MoveUp()
    {
        isMovingUp = true;
        CancelInvoke(nameof(ResetPlatform));
        Invoke(nameof(ResetPlatform), 1f); // 1초 뒤 원위치로 복귀
    }

    private void ResetPlatform()
    {
        isMovingUp = false;
    }
}
