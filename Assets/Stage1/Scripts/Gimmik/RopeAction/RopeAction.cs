using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeAction : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 10f; // 플레이어가 목표 지점으로 이동하는 속도
    public LayerMask GrapplingObj;
    private LineRenderer line;
    private bool isMoving = false;
    private Vector3 targetPosition;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RopeShoot();
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            EndShoot();
        }

        if (isMoving)
        {
            MovePlayerToTarget();
        }

        DrawLine();
    }

    void RopeShoot()
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, 7f, GrapplingObj);
        if (hitColliders.Length > 0)
        {
            Collider closestCollider = hitColliders[0];
            float closestDistance = Vector3.Distance(player.transform.position, closestCollider.transform.position);

            foreach (var collider in hitColliders)
            {
                float distance = Vector3.Distance(player.transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestCollider = collider;
                    closestDistance = distance;
                }
            }

            targetPosition = closestCollider.transform.position;
            isMoving = true;

            line.positionCount = 2;
            line.SetPosition(0, this.transform.position);
            line.SetPosition(1, targetPosition);
        }
    }

    void EndShoot()
    {
        isMoving = false;
        line.positionCount = 0;
    }

    void MovePlayerToTarget()
    {
        // 플레이어를 목표 지점으로 이동
        player.position = Vector3.MoveTowards(player.position, targetPosition, moveSpeed * Time.deltaTime);

        // 목표 지점에 도달하면 이동 중지
        if (Vector3.Distance(player.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            EndShoot();
        }
    }

    void DrawLine()
    {
        if (isMoving)
        {
            line.SetPosition(0, this.transform.position);
            line.SetPosition(1, targetPosition);
        }
    }
}
