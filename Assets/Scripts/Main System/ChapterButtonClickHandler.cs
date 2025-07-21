using Michsky.UI.Shift;
using UnityEngine;

public class ChapterButtonClickHandler : MonoBehaviour
{
    [SerializeField] private ChapterButton chapterButton;
    [SerializeField] private StagePanel stagePanel;
    [SerializeField] private ModalWindowManager modalWindow;

    public void OnClick()
    {
        if (chapterButton != null && stagePanel != null)
        {
            stagePanel.SetSceneToLoad(chapterButton.buttonTitle);
        }
        else
        {
            Debug.LogWarning("ChapterButton 또는 StagePanel 연결이 빠짐");
        }
        
        modalWindow.windowTitle.text = "Play  " + chapterButton.buttonTitle;
    }
}
