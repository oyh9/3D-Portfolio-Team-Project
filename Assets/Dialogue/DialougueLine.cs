// DialogueLine.cs 수정
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine {
    [TextArea]
    public string text;
    public int characterIconIndex = -1; // -1은 아이콘 없음
    public AudioClip voiceClip;
    public bool waitForInput = true;
    public UnityEvent onLineComplete;
}