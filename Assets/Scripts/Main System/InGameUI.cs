using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanelPrefab;
    [SerializeField] private GameObject optionPanelPrefab;

    private GameObject spawnedPausePanel;
    private GameObject spawnedOptionPanel;
    private Transform canvasTransform;

    private bool isOpenPausePanel;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        isOpenPausePanel = false;
        if (sceneName == "Tutorial")
        {
            canvasTransform = GameObject.Find("Tutorial Canvas").transform;
        }
        else
        {
            canvasTransform = GameObject.Find("Stage Canvas").transform;
        }

        
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpenPausePanel)
            {
                OnClickPausePanel();
                
                Time.timeScale = 0f;
            }
            else
            {
                OnClickClosePausePanel();
                Time.timeScale = 1f;
            }

            
            GameManager.Instance.isUIActive = isOpenPausePanel;
        }
    }

    public void OnClickPausePanel()
    {
        isOpenPausePanel = !isOpenPausePanel;
        GameManager.Instance.UnlockCursor();
        if (spawnedPausePanel == null)
        {
            spawnedPausePanel = Instantiate(pausePanelPrefab, canvasTransform);
            Debug.Log("canvasTransform " + canvasTransform);

            Transform closeBtnTransform = spawnedPausePanel.transform.Find("Pause X Button");
            if (closeBtnTransform != null)
            {
                Button closePauseButton = closeBtnTransform.GetComponent<Button>();
                if (closePauseButton != null)
                {
                    closePauseButton.onClick.AddListener(OnClickClosePausePanel);
                }
                else
                {
                    Debug.LogWarning("CloseButton 오브젝트에 Button 컴포넌트가 없어요!");
                }
            }
            else
            {
                Debug.LogWarning("CloseButton 오브젝트를 찾지 못했어요!");
            }
        }
    }

    public void OnClickClosePausePanel()
    {
        if (GameManager.Instance.Is2DMode())
        {
            GameManager.Instance.UnlockCursor();
        }
        else
        {
            GameManager.Instance.LockCursor();
        }
        
        Debug.Log("ClosePausePanel 호출됨");
        if (spawnedPausePanel != null)
        {
            Destroy(spawnedPausePanel);
            spawnedPausePanel = null;
            Time.timeScale = 1f;
            isOpenPausePanel = !isOpenPausePanel;
        }
        else
        {
            Debug.LogWarning("spawnedPausePanel이 null이야!");
        }
    }

    // 옵션 패널을 프리팹에서 불러오기. 일단 Stage에서만 사용
    public void OnClickOptionPanel()
    {
        if (spawnedOptionPanel == null)
        {
            spawnedOptionPanel = Instantiate(optionPanelPrefab, canvasTransform);

            Debug.Log("canvasTransform " + canvasTransform);

            Transform closeBtnTransform = spawnedOptionPanel.transform.Find("OptionMenuClose");
            if (closeBtnTransform != null)
            {
                Button closeOptionButton = closeBtnTransform.GetComponent<Button>();
                if (closeOptionButton != null)
                {
                    closeOptionButton.onClick.AddListener(OnClickCloseOptionPanel);
                }
                else
                {
                    Debug.LogWarning("CloseButton 오브젝트에 Button 컴포넌트가 없어요!");
                }
            }
            else
            {
                Debug.LogWarning("CloseButton 오브젝트를 찾지 못했어요!");
            }
        }
    }

    public void OnClickCloseOptionPanel()
    {
        Debug.Log("CloseOptionPanel 호출됨");
        if (spawnedOptionPanel != null)
        {
            Destroy(spawnedOptionPanel);
            spawnedOptionPanel = null;
        }
        else
        {
            Debug.LogWarning("spawnedOptionPanel이 null이야!");
        }
    }

    // 메인으로 돌아가는 버튼
    public void OnClickToMain()
    {
        GameManager.Instance.isChangingScene = true;
        Time.timeScale = 1f;
        LoadingSceneManager.LoadScene("Main Menu");
    }
}
