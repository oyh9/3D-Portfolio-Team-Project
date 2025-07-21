using System.Collections;
using UnityEngine;

public class DangerousLiquidCleaner : MonoBehaviour
{
    private LiquidLoader liquidLoader;

    [Header("Button Press Settings")]
    public Transform buttonTransform;                 // ������ ��ư ������Ʈ
    public Vector3 pressOffset = new Vector3(0, -0.1f, 0); // �󸶳� ��������
    public float pressDuration = 0.2f;                // ������ �ö���� �ӵ�

    private bool isPressed = false;                   // �ߺ� �浹 ������

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

        // ��ư ������
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(originalPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = targetPos;

        // ��� ���
        yield return new WaitForSeconds(0.1f);

        // ��ư �ö�
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
