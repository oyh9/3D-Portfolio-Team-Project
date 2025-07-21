using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIPanelController : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button exitButton;

    [Header("옵션 패널")]
    [SerializeField] private GameObject optionPanelPrefab;

    private void Awake()
    {
        retryButton.onClick.AddListener(OnRetryClicked);
        loadButton.onClick.AddListener(OnLoadClicked);
        optionButton.onClick.AddListener(OnOptionClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    public void OnRetryClicked()
    {
        StageManager.Instance.ResetStage();
        StageManager.Instance.SpawnPlayer();
    }

    private void OnLoadClicked()
    {
        StageManager.Instance.LoadStage();
        PlayerRespawnManager.Instance.Respawn();
    }

    private void OnOptionClicked()
    {
        if (optionPanelPrefab != null)
            optionPanelPrefab.SetActive(true);
    }

    private void OnExitClicked()
    {
        Time.timeScale = 1;
        
        SceneManager.LoadScene("Main Menu");
    }
}

