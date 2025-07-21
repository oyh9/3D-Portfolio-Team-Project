using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{    public void OnClickStart()
    {
        // "HasLaunchedBefore"라는 키를 확인해서 최초 실행인지 판단
        if (!PlayerPrefs.HasKey("HasLaunchedBefore"))
        {
            // 최초 실행이므로 튜토리얼 씬으로 이동
            PlayerPrefs.SetInt("HasLaunchedBefore", 1); // 1은 true 의미
            PlayerPrefs.Save();
            LoadingSceneManager.LoadScene("Tutorial"); // Tutorial 씬 로드
        }
        else
        {
            // 이미 튜토리얼을 본 경우 바로 Main 씬으로
            LoadingSceneManager.LoadScene("Main");
        }
    }
}
