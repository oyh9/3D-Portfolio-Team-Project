using System.Collections;
using UnityEngine;

public class LiquidRefill : MonoBehaviour
{
    [Header("Button Press Settings")]
    public Transform buttonTransform;         // 눌리는 버튼 오브젝트
    public Vector3 pressOffset = new Vector3(0, -0.1f, 0); // 얼마나 눌리는지
    public float pressDuration = 0.2f;        // 눌리고 올라오는 속도

    private bool isPressed = false;

    public GameObject[] refillPrefabs; // 5개 (플레이어용)
    public Transform[] spawnPoints;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPressed) return; // 이미 눌리는 중이면 무시

        if (collision.gameObject.CompareTag("Cube Trigger"))
        {
            isPressed = true;

            // 버튼 누르고 원위치 복귀
            if (buttonTransform != null)
                StartCoroutine(PressButton(buttonTransform, pressOffset, pressDuration));
        }

        StartCoroutine(RefillLiquid());
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

    private IEnumerator RefillLiquid()
    {
        for(int i = 0; i < refillPrefabs.Length && i < spawnPoints.Length; i++)
        {
            if (refillPrefabs[i] != null && spawnPoints[i] != null)
            {
                Instantiate(refillPrefabs[i], spawnPoints[i].position, Quaternion.identity);
                yield return new WaitForSeconds(0.1f); // 약간의 딜레이 (원하면 제거 가능)
            }
        }
    }
}
