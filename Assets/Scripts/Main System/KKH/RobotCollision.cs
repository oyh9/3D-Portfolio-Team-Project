using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RobotCollision : MonoBehaviour
{
    public GameObject sceneLoadText;
    public Image holdGauge;
    public float requiredHoldTime = 1f;

    private bool playerInRange = false;
    private bool hasFirstDialoguePlayed = false;
    private float holdTime = 0f;

    void Start()
    {
        if (sceneLoadText != null)
            sceneLoadText.SetActive(false);

        if (holdGauge != null)
        {
            holdGauge.fillAmount = 0f;
            holdGauge.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKey(KeyCode.E))
        {
            holdTime += Time.deltaTime;

            if (holdGauge != null)
            {
                holdGauge.gameObject.SetActive(true);
                holdGauge.fillAmount = holdTime / requiredHoldTime;
            }

            if (holdTime >= requiredHoldTime)
            {
                if (sceneLoadText != null)
                    sceneLoadText.SetActive(false);

                if (!hasFirstDialoguePlayed)
                {
                    DialogueManager.Instance.TutorialDialogue(1);
                    hasFirstDialoguePlayed = true;
                }
                else
                {
                    DialogueManager.Instance.TutorialDialogue(2);
                }

                ResetGauge();
            }
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            ResetGauge();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (sceneLoadText != null)
                sceneLoadText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (sceneLoadText != null)
                sceneLoadText.SetActive(false);

            ResetGauge();
        }
    }

    private void ResetGauge()
    {
        holdTime = 0f;

        if (holdGauge != null)
        {
            holdGauge.fillAmount = 0f;
            holdGauge.gameObject.SetActive(false);
        }
    }
}
