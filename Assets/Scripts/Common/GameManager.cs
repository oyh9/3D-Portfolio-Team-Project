using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : Singleton<GameManager>
{
    private bool isCursorLocked = false;
    public static bool canPlayerMove = true;
    
    
    public bool isUIActive = false;
    public bool isChangingScene = false;
    
    public bool is2DMode = false;
    
    public string currentSceneName;
    [HideInInspector]public bool nextPlatform = false;
    [HideInInspector]public bool ChangeSkybox = false;
    [HideInInspector]public bool die = false;
    
    
    void Start()
    {
        
    }

    
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        if (scene.name == "Loading")
        {
            ImprovedSoundManager.Instance.StopBGM();
        }
        
        if (scene.name == "Tutorial")
        {
            canPlayerMove = false;
            LockCursor();
            ImprovedSoundManager.Instance.PlaySound2D("TutorialBGM");
            
        }

        if (scene.name != "Main Menu"&&scene.name != "Stage3")
        {
            LockCursor();
        }
        else if (scene.name == "Stage3")
        {
            is2DMode = true;
            UnlockCursor();
            ImprovedSoundManager.Instance.PlaySound2D("2DModeBGM");
            
        }else if (scene.name == "Main Menu")
        {
            UnlockCursor();
        }
        
        
        isChangingScene = false;
    }

    public bool Is2DMode()
    {
        return is2DMode;
    }
    
    
}
