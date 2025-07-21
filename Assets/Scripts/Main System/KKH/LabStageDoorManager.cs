using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LabStageDoorManager : MonoBehaviour
{
    public GameObject sceneLoadText;
    public Image holdGauge;

    private bool playerInRange = false;
    private float holdTime = 0f;
    private float requiredHoldTime = 1f;

    [SerializeField] private bool isOpenedDoor;

    public Vector3 openTiltRotationDoor1 = new Vector3(0, 90, 0); // 기울일 각도
    public Vector3 openTiltRotationDoor2 = new Vector3(0, 90, 0);
    public Vector3 closeTiltRotationDoor1 = new Vector3(0, -90, 0);
    public Vector3 closeTiltRotationDoor2 = new Vector3(0, -90, 0);
    public float tiltDuration = 1f;

    [SerializeField] private Transform Door1;
    [SerializeField] private Transform Door2;

    private Coroutine doorTiltCoroutine1;
    private Coroutine doorTiltCoroutine2;
    private Coroutine doorTiltCoroutine3;
    private Coroutine doorTiltCoroutine4;

    void Start()
    {
        isOpenedDoor = false;

        if (sceneLoadText != null)
            sceneLoadText.SetActive(false); // 처음엔 숨기기

        if (holdGauge != null)
        {
            holdGauge.fillAmount = 0f;
            holdGauge.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKey(KeyCode.E))
            {
                holdTime += Time.deltaTime;

                if (holdGauge != null)
                {
                    holdGauge.gameObject.SetActive(true);
                    holdGauge.fillAmount = holdTime / requiredHoldTime;
                }

                if (holdTime >= requiredHoldTime && isOpenedDoor == false)
                {
                    Debug.Log($"문 오픈");

                    if (doorTiltCoroutine1 != null && doorTiltCoroutine2 != null)
                    {
                        StopCoroutine(doorTiltCoroutine1);
                        StopCoroutine(doorTiltCoroutine2);
                    }

                    doorTiltCoroutine1 = StartCoroutine(SmoothTilt(Door1, openTiltRotationDoor1, tiltDuration));
                    doorTiltCoroutine2 = StartCoroutine(SmoothTilt(Door2, openTiltRotationDoor2, tiltDuration));

                    holdTime = 0f;

                    isOpenedDoor = true;

                    if (holdGauge != null)
                    {
                        holdGauge.fillAmount = 0f;
                        holdGauge.gameObject.SetActive(false);
                    }
                }
                else if (holdTime >= requiredHoldTime && isOpenedDoor == true)
                {
                    Debug.Log($"문 닫힘");

                    if (doorTiltCoroutine3 != null && doorTiltCoroutine4 != null)
                    {
                        StopCoroutine(doorTiltCoroutine3);
                        StopCoroutine(doorTiltCoroutine4);
                    }

                    doorTiltCoroutine3 = StartCoroutine(SmoothTilt(Door1, closeTiltRotationDoor1, tiltDuration));
                    doorTiltCoroutine4 = StartCoroutine(SmoothTilt(Door2, closeTiltRotationDoor2, tiltDuration));

                    holdTime = 0f;

                    isOpenedDoor = false;

                    if (holdGauge != null)
                    {
                        holdGauge.fillAmount = 0f;
                        holdGauge.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                // 키를 뗐을 때 초기화
                holdTime = 0f;

                if (holdGauge != null)
                {
                    holdGauge.fillAmount = 0f;
                    holdGauge.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // 범위 밖으로 나가면 초기화
            holdTime = 0f;

            if (holdGauge != null)
            {
                holdGauge.fillAmount = 0f;
                holdGauge.gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (sceneLoadText != null)
            {
                sceneLoadText.SetActive(true);
            }
            else
            {
                Debug.LogWarning("sceneLoadText가 null입니다!");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (sceneLoadText != null)
                sceneLoadText.SetActive(false);
        }
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
}