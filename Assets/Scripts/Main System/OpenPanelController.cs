using Michsky.UI.Shift;
using UnityEngine;

public class OpenPanelController : MonoBehaviour
{
    [Header("Target UI Components")]
    public ModalWindowManager modalWindow;
    public BlurManager blurManager;

    private bool isPauseOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPauseOpen)
            {
                OpenPausePanel();
                
                Time.timeScale = 0f;
            }
            else
            {
                ClosePausePanel();
                
                Time.timeScale = 1f;
            }
        }
    }

    private void OpenPausePanel()
    {
        GameManager.Instance.UnlockCursor();
        
        if (blurManager != null)
            blurManager.BlurInAnim();
        
        if (modalWindow != null)
            modalWindow.ModalWindowIn();

        isPauseOpen = true;
    }

    private void ClosePausePanel()
    {
        if (GameManager.Instance.Is2DMode())
        {
            GameManager.Instance.UnlockCursor();
        }
        else
        {
            GameManager.Instance.LockCursor();
        }
        
        if (modalWindow != null)
            modalWindow.ModalWindowOut();

        if (blurManager != null)
            blurManager.BlurOutAnim();

        isPauseOpen = false;
    }
}

