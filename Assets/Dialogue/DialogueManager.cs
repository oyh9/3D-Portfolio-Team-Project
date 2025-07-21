// DialogueManager.cs 수정
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class DialogueManager : MonoBehaviour {
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public CanvasGroup dialogueCanvas;
    public Image characterIconImage; // 캐릭터 아이콘 이미지
    public float textSpeed = 0.05f;
    
    [Header("Character Icons")]
    public CharacterIconData[] characterIconData; // 캐릭터 아이콘 데이터 배열
    
    private Queue<DialogueLine> dialogueQueue = new();
    private bool isTyping = false;
    private bool skipRequested = false;
    
    [FormerlySerializedAs("tutorialDialogue")] public DialogueSet[] Dialogue;
    public UnityEvent[] onDialogueEnd;
    
    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Tutorial")
        {
            Dialogue[4].lines[3].onLineComplete = onDialogueEnd[0];
            Dialogue[5].lines[7].onLineComplete = onDialogueEnd[1];
            Dialogue[0].lines[7].onLineComplete = onDialogueEnd[2];
        }

        if (sceneName == "Stage3")
        {
            TutorialDialogue(0);
        }
    }

    void Update() {
        if (isTyping) 
        {
            if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                skipRequested = true;
            }
        }
    }

    public void TutorialDialogue(int index) {
        StartDialogue(Dialogue[index]);
    }

    public void StartDialogue(DialogueSet set) {
        StopAllCoroutines();
        dialogueCanvas.alpha = 1;
        dialogueQueue.Clear();
        dialogueText.text = "";

        if (characterIconImage != null)
            characterIconImage.gameObject.SetActive(false);

        foreach (var line in set.lines) dialogueQueue.Enqueue(line);
        StartCoroutine(DisplayAllLines());
    }

    IEnumerator DisplayAllLines() {
        dialogueText.text = "";

        while (dialogueQueue.Count > 0) {
            DialogueLine line = dialogueQueue.Dequeue();

            // 캐릭터 아이콘 표시
            if (characterIconImage != null && line.characterIconIndex >= 0 &&
                line.characterIconIndex < characterIconData.Length &&
                characterIconData[line.characterIconIndex].iconSprite != null) {
                characterIconImage.gameObject.SetActive(true);
                characterIconImage.sprite = characterIconData[line.characterIconIndex].iconSprite;
                characterIconImage.GetComponent<RectTransform>().sizeDelta = characterIconData[line.characterIconIndex].customSize;
            } else if (characterIconImage != null) {
                characterIconImage.gameObject.SetActive(false);
            }

            // 타이핑 효과
            yield return StartCoroutine(TypeBlock(line.text, false, line.voiceClip));

            // 다음 입력 대기
            if (line.waitForInput) {
                yield return StartCoroutine(WaitForInputOrTimeout(2f));
            }

            if (line.onLineComplete != null)
                line.onLineComplete.Invoke();

            dialogueText.text = "";
        }

        if (characterIconImage != null)
            characterIconImage.gameObject.SetActive(false);

        dialogueCanvas.alpha = 0;
    }

    IEnumerator TypeBlock(string text, bool append = false, AudioClip voiceClip = null) {
        isTyping = true;
        skipRequested = false;
        string displayed = append ? dialogueText.text : "";

        if (voiceClip != null)
            AudioSource.PlayClipAtPoint(voiceClip, Camera.main.transform.position, 0.1f);

        for (int i = 0; i < text.Length; i++) {
            if (skipRequested) {
                dialogueText.text = displayed + text.Substring(i);
                break;
            }

            displayed += text[i];
            dialogueText.text = displayed;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
    }

    IEnumerator WaitForInputOrTimeout(float timeoutSeconds) {
        // 클릭 해제될 때까지 대기
        while (Input.GetMouseButton(0))
            yield return null;

        float timer = 0f;

        while (timer < timeoutSeconds) {
            if (Input.GetMouseButtonDown(0))
                yield break;

            timer += Time.deltaTime;
            yield return null;
        }
    }
}