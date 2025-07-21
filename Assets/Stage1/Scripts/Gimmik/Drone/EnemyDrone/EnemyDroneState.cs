using UnityEngine;

public class EnemyDroneState : MonoBehaviour
{
    private enum DroneState { Patrol, Alert, Stunned }
    private DroneState currentState = DroneState.Patrol;

    [Header("Rotation & Detection")]
    public float patrolAngle = 45f;
    public float rotationSpeed = 30f;
    [SerializeField] float detectionRange = 15f;
    [SerializeField] LayerMask playerLayer; // "Player"�� ����
    [SerializeField] Transform rayOrigin;
    public float raycastInterval = 0.2f;
    public float loseTargetDelay = 2f;

    [Header("Stun Settings")]
    public float stunDuration = 2f;

    [Header("Laser")]
    public Transform laserOrigin;
    public LineRenderer laserLine;

    private int direction = 1;
    private float currentAngle = 0f;
    private float raycastTimer = 0f;
    private float lastSeenTime = 0f;
    private float stunTimer = 0f;

    private Transform playerTarget;

    void Update()
    {
        raycastTimer -= Time.deltaTime;

        switch (currentState)
        {
            case DroneState.Patrol:
                PatrolRotation();
                if (raycastTimer <= 0f)
                    DetectPlayer();
                break;

            case DroneState.Alert:
                TrackPlayer();
                if (raycastTimer <= 0f)
                    CheckStillSeeingPlayer();
                FireLaser();

                if (Time.time - lastSeenTime > loseTargetDelay)
                    ResetToPatrol();
                break;

            case DroneState.Stunned:
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0f)
                    ResetToPatrol();
                break;
        }
    }

    // ����: ����
    void PatrolRotation()
    {
        float rotateStep = rotationSpeed * Time.deltaTime * direction;
        currentAngle += rotateStep;

        if (Mathf.Abs(currentAngle) > patrolAngle)
            direction *= -1;

        Quaternion baseRot = Quaternion.Euler(0f, -90f, 0f); // �ʱ� ���� ����
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        transform.rotation = baseRot * rotation;
    }

    // ����: ����
    void TrackPlayer()
    {
        if (playerTarget == null) return;

        Vector3 dir = playerTarget.position - transform.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 3f);
        }
    }

    // Ž��: �÷��̾� ����
    void DetectPlayer()
    {
        raycastTimer = raycastInterval;

        // Ray ���� ��ġ (��� �߽� �Ǵ� ����ο� ��ġ�� �� ������Ʈ)
        Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position + Vector3.up * 1.0f;

        // ����� ���� ����: X�� �������� forward ����
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, detectionRange, playerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                playerTarget = hit.collider.transform;
                lastSeenTime = Time.time;
                currentState = DroneState.Alert;
            }
        }
    }

    // ���� �� ��� ���̴��� Ȯ��
    void CheckStillSeeingPlayer()
    {
        raycastTimer = raycastInterval;

        if (playerTarget == null) return;

        Vector3 dirToPlayer = playerTarget.position - transform.position;
        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
                lastSeenTime = Time.time;
        }
    }

    // ������ �߻�
    void FireLaser()
    {
        if (!laserLine.enabled)
            laserLine.enabled = true;

        laserLine.SetPosition(0, laserOrigin.position);

        if (Physics.Raycast(laserOrigin.position, laserOrigin.forward, out RaycastHit hit, detectionRange))
        {
            laserLine.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Player"))
            {
                // �÷��̾�� ������ �� ó�� ����
            }
        }
        else
        {
            laserLine.SetPosition(1, laserOrigin.position + laserOrigin.right * detectionRange);
        }
    }

    // ����: �ǰ� ���� (Stun)
    public void OnHitByBullet()
    {
        currentState = DroneState.Stunned;
        stunTimer = stunDuration;
        laserLine.enabled = false;
    }

    // ���� �ʱ�ȭ
    public void ResetToPatrol()
    {
        playerTarget = null;
        laserLine.enabled = false;
        currentAngle = 0f;
        direction = 1;
        currentState = DroneState.Patrol;
        raycastTimer = 0f;
        lastSeenTime = 0f;
    }
}
