using UnityEngine;

public class StagePanel : MonoBehaviour
{
    private string selectedSceneName;
    
    public void SetSceneToLoad(string sceneName)
    {
        selectedSceneName = sceneName;
    }

    public void LoadSelectedScene()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            LoadingSceneManager.LoadScene(selectedSceneName);
        }
    }
    
    public void OnClick1stStage()
    {
        LoadingSceneManager.LoadScene("Tutorial");
    }

    public void OnClick2ndStage()
    {
        LoadingSceneManager.LoadScene("Temp2ndStage");
    }

    public void OnClick3rdStage()
    {
        LoadingSceneManager.LoadScene("Temp3rdStage");
    }

    public void OnClickStage()
    {
        
    }
}
