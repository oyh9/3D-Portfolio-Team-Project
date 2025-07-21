using System;
using UnityEngine;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    public GameObject sceneLoadText;
    public Image holdGauge;

    public string targetSceneName;

    public string sceneLoadTextName = "Scene Load Check Text";
    public string holdGaugeName = "Scene Load Guage";
    public string canvasName = "Stage Canvas";
    
    private bool playerInRange = false;
    private float holdTime = 0f;
    private float requiredHoldTime = 1f;

    private void Awake()
    {
        GameObject canvasObj = GameObject.Find(canvasName);

        if (sceneLoadText == null || holdGauge == null)
        {
            if (canvasObj != null)
            {
                // Canvas 아래에서 직접 자식 찾기
                if (sceneLoadText == null)
                {
                    Transform textTrans = canvasObj.transform.Find(sceneLoadTextName);
                    if (textTrans != null)
                        sceneLoadText = textTrans.gameObject;
                    else
                        Debug.LogWarning($"'{sceneLoadTextName}' 이름의 GameObject를 {canvasName} 아래에서 찾을 수 없습니다.");
                }

                if (holdGauge == null)
                {
                    Transform gaugeTrans = canvasObj.transform.Find(holdGaugeName);
                    if (gaugeTrans != null)
                        holdGauge = gaugeTrans.GetComponent<Image>();
                    else
                        Debug.LogWarning($"'{holdGaugeName}' 이름의 GameObject를 {canvasName} 아래에서 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError($"'{canvasName}' 이름의 Canvas를 찾을 수 없습니다!");
            }
        }
        
    }

    void Start()
    {
        
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

                if (holdTime >= requiredHoldTime)
                {
                    Debug.Log($"포탈 진입 - '{targetSceneName}' 씬으로 로딩 요청");
                    LoadingSceneManager.LoadScene(targetSceneName);
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
            if (GameManager.Instance.currentSceneName == "Stage3")
            {
                DialogueManager.Instance.TutorialDialogue(2);
            }
            
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
}
