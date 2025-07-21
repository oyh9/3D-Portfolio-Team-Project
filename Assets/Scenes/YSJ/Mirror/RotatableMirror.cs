using UnityEngine;
using TMPro;

public class RotatableMirror : MonoBehaviour
{
    [Header("회전 설정")]
    public float rotationSpeed = 15f; // 초당 회전 속도

    [Header("UI")]
    public TMP_Text rotateHintText;

    private bool playerInRange = false;
    private int rotateDirection = 0; // -1: 좌회전, 1: 우회전, 0: 정지

    public bool IsLaserHit { get; private set; } = false;

    public void SetLaserHit(bool isHit)
    {
        IsLaserHit = isHit;
    }

    private void Start()
    {
        if (rotateHintText != null)
            rotateHintText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                rotateDirection = -1;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                rotateDirection = 1;
            }
            else
            {
                rotateDirection = 0;
            }
        }
        else
        {
            rotateDirection = 0;
        }

        // 회전 적용
        if (rotateDirection != 0)
        {
            transform.Rotate(Vector3.up, rotationSpeed * rotateDirection * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (rotateHintText != null)
            {
                rotateHintText.text = "Q: 좌회전 | E: 우회전";
                rotateHintText.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (rotateHintText != null)
                rotateHintText.text = "";
                rotateHintText.gameObject.SetActive(false);
        }
    }
}
