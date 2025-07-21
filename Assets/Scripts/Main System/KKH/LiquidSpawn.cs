using System.Collections;
using UnityEngine;

public class LiquidSpawn : MonoBehaviour
{
    public Transform[] spawnPoints;
    public LiquidLoader liquidLoader;

    public Transform objectToTilt;
    public Vector3 tiltRotation = new Vector3(30, 0, 0);
    public float tiltDuration = 1f;

    [Header("Button Press Settings")]
    public Transform buttonTransform;         // 눌리는 버튼 오브젝트
    public Vector3 pressOffset = new Vector3(0, -0.1f, 0); // 얼마나 눌리는지
    public float pressDuration = 0.2f;        // 눌리고 올라오는 속도

    private Coroutine tiltCoroutine;
    private bool isPressed = false;           // 중복 충돌 방지용

    void Start()
    {
        liquidLoader = FindAnyObjectByType<LiquidLoader>();
        StartCoroutine(SpawnDelayed());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPressed) return; // 이미 눌리는 중이면 무시

        if (collision.gameObject.CompareTag("Cube Trigger") && objectToTilt != null)
        {
            isPressed = true;

            // 기울이기 시작
            if (tiltCoroutine != null)
                StopCoroutine(tiltCoroutine);

            tiltCoroutine = StartCoroutine(SmoothTilt(objectToTilt, tiltRotation, tiltDuration));

            // 버튼 누르고 원위치 복귀
            if (buttonTransform != null)
                StartCoroutine(PressButton(buttonTransform, pressOffset, pressDuration));
        }
    }

    private void SpawnLiquids()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject liquidObj = LiquidPoolManager.Instance.GetLiquid();
            Liquid liquid = liquidObj.GetComponent<Liquid>();
            liquid.Drop(spawnPoints[i].position);
        }
    }

    private IEnumerator SpawnDelayed()
    {
        yield return null;
        SpawnLiquids();
    }

    private IEnumerator SmoothTilt(Transform target, Vector3 targetEulerAngles, float duration)
    {
        Quaternion startRotation = target.rotation;
        Quaternion endRotation = Quaternion.Euler(targetEulerAngles);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.rotation = endRotation;
    }

    private IEnumerator PressButton(Transform button, Vector3 offset, float duration)
    {
        Vector3 originalPos = button.position;
        Vector3 targetPos = originalPos + offset;
        float elapsed = 0f;

        // 버튼 내려감
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(originalPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = targetPos;

        // 딜레이 후 원복
        yield return new WaitForSeconds(0.1f);
        elapsed = 0f;
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(targetPos, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = originalPos;

        // 다시 충돌 가능 상태로
        isPressed = false;
    }
}
