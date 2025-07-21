using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour, ITriggerableObstacle
{
    public float targetHeight = 3f; // 목표 높이
    public float speed = 0.1f;        // 올라가는 속도 (1초에 몇 미터)

    private bool isMoving = false;
    
    public void TriggerObstacle(GameObject activator)
    {
        StartCoroutine(MoveUp());
    }
    
    
    private IEnumerator MoveUp()
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetHeight, startPosition.z);

        while (transform.position.y < targetHeight)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
    }
    
}
