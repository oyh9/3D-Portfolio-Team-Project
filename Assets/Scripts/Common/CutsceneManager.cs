using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector timeline;
    
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject keyGuide;
    
    private string currentSceneName;
    
    void Awake()
    {
        // 타임라인 시작 시 호출될 이벤트 등록
        timeline.played += OnTimelineStarted;
        timeline.stopped += OnTimelineStopped;
        
        // 현재 씬 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;
    }
    
    void OnTimelineStarted(PlayableDirector director)
    {
        GameManager.canPlayerMove = false; // 입력 비활성화
    }
    
    void OnTimelineStopped(PlayableDirector director)
    {
        // 씬이 바뀌었는지 확인
        if (GameManager.Instance.isChangingScene)
        {
            // 씬이 바뀌는 중이면 추가 작업 수행하지 않음
            return;
        }
        
        GameManager.canPlayerMove = true; // 입력 다시 활성화
        crosshair.SetActive(true);
        DialogueManager.Instance.TutorialDialogue(0);
        keyGuide.SetActive(true);
        
    }
    
    // 컴포넌트가 비활성화될 때 이벤트 구독 해제
    void OnDisable()
    {
        if (timeline != null)
        {
            timeline.played -= OnTimelineStarted;
            timeline.stopped -= OnTimelineStopped;
        }
    }
}