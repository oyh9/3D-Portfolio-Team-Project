using System.Collections;
using UnityEngine;

public class LiquidRefill : MonoBehaviour
{
    [Header("Button Press Settings")]
    public Transform buttonTransform;         // ������ ��ư ������Ʈ
    public Vector3 pressOffset = new Vector3(0, -0.1f, 0); // �󸶳� ��������
    public float pressDuration = 0.2f;        // ������ �ö���� �ӵ�

    private bool isPressed = false;

    public GameObject[] refillPrefabs; // 5�� (�÷��̾��)
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
        if (isPressed) return; // �̹� ������ ���̸� ����

        if (collision.gameObject.CompareTag("Cube Trigger"))
        {
            isPressed = true;

            // ��ư ������ ����ġ ����
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

        // ��ư ������
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(originalPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = targetPos;

        // ������ �� ����
        yield return new WaitForSeconds(0.1f);
        elapsed = 0f;
        while (elapsed < duration)
        {
            button.position = Vector3.Lerp(targetPos, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        button.position = originalPos;

        // �ٽ� �浹 ���� ���·�
        isPressed = false;
    }

    private IEnumerator RefillLiquid()
    {
        for(int i = 0; i < refillPrefabs.Length && i < spawnPoints.Length; i++)
        {
            if (refillPrefabs[i] != null && spawnPoints[i] != null)
            {
                Instantiate(refillPrefabs[i], spawnPoints[i].position, Quaternion.identity);
                yield return new WaitForSeconds(0.1f); // �ణ�� ������ (���ϸ� ���� ����)
            }
        }
    }
}
