using System.Collections;
using UnityEngine;

public class DangerousLiquidCleaner : MonoBehaviour
{
    private LiquidLoader liquidLoader;

    [Header("Button Press Settings")]
    public Transform buttonTransform;                 // 눌리는 버튼 오브젝트
    public Vector3 pressOffset = new Vector3(0, -0.1f, 0); // 얼마나 눌리는지
    public float pressDuration = 0.2f;                // 눌리고 올라오는 속도

    private bool isPressed = false;                   // 중복 충돌 방지용

    void Start()
    {
        liquidLoader = FindAnyObjectByType<LiquidLoader>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isPressed) return;

        if (collision.gameObject.CompareTag("Cube Trigger"))
        {
            isPressed = true;
            CleanDangerousLiquids();

            if (buttonTransform != null)
                StartCoroutine(PressButton(buttonTransform, pressOffset, pressDuration));
        }
    }

    private void CleanDangerousLiquids()
    {
        Liquid[] allLiquids = FindObjectsOfType<Liquid>();

        foreach (Liquid liquid in allLiquids)
        {
            if (liquid == null || liquidLoader == null)
                continue;

            int index = liquid.liquidIndex;

            if (index >= 0 && index < liquidLoader.allLiquids.Count)
            {
                if (liquidLoader.allLiquids[index].Danger >= 6)
                {
                    StartCoroutine(ReturnAfterDelay(liquid.gameObject, 2f));
                }
            }
        }
    }

    private IEnumerator ReturnAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
        LiquidPoolManager.Instance.ReturnLiquid(obj);
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

        // 잠깐 대기
        yield return new WaitForSeconds(0.1f);

        // 버튼 올라감
        elapsed = 0f;
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(targetPos, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = originalPos;

        isPressed = false;
    }
}
