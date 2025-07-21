using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public abstract class BaseAreaManager : MonoBehaviour
{
    [Header("Area Common Components")]
    [SerializeField] protected GameObject goodUIPanel;
    [SerializeField] protected GameObject barrierObjectInScene;
    public virtual void OnColorLaserTriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType) { }
    public virtual void OnColorLaserUntriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType) { }

    protected Transform player;
    protected bool isAreaCompleted = false;

    [Header("Portal Controllers")]
    [SerializeField] protected GameObject[] portalRounds;
    [SerializeField] protected PortalRound_Controller[] portalEffects; // 각 포탈에 대한 PortalRound_Controller 배열
    

    protected void ActivatePortals()
    {
        // 각 포탈에 대해 개별적으로 활성화
        for (int i = 0; i < portalRounds.Length; i++)
        {
            if (portalEffects != null && portalEffects.Length > i && portalEffects[i] != null)
            {
                portalEffects[i].F_TogglePortalRound(true);
            }
        }
    }

    protected virtual void Awake()
    {
        if (StageManager.Instance != null)
            player = StageManager.Instance.PlayerInstance;
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    public void OnLaserTriggered()
    {
        if (isAreaCompleted) return;

        isAreaCompleted = true;
        ShowSuccessUI();
        DisableBarrier();
        OnAreaCompleted();
    }

    protected virtual void ShowSuccessUI()
    {
        if (goodUIPanel != null)
        {
            CanvasGroup canvasGroup = goodUIPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = goodUIPanel.AddComponent<CanvasGroup>();

            Image bgImage = goodUIPanel.GetComponent<Image>();
            if (bgImage == null)
                bgImage = goodUIPanel.AddComponent<Image>();

            TMPro.TextMeshProUGUI text = goodUIPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text == null)
            {
                Debug.LogWarning("TextMeshProUGUI not found in goodUIPanel.");
                return;
            }

            // 초기 설정
            goodUIPanel.transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
            goodUIPanel.SetActive(true);

            // 컬러 테마 (성채 느낌)
            Color panelStartColor = new Color32(27, 31, 42, 200);      // 어두운 남색 #1B1F2A
            Color panelTargetColor = new Color32(46, 58, 85, 255);     // 조금 더 밝은 푸른빛 #2E3A55
            Color textColor = new Color32(168, 208, 255, 255);         // 연하늘색 #A8D0FF

            bgImage.color = panelStartColor;
            text.color = new Color(textColor.r, textColor.g, textColor.b, 0f); // 텍스트 페이드 인

            // 시퀀스 애니메이션
            Sequence seq = DOTween.Sequence();
            seq.Append(goodUIPanel.transform.DOScale(1.1f, 0.45f).SetEase(Ease.OutBack))
            .Join(canvasGroup.DOFade(1f, 0.4f))
            .Join(bgImage.DOColor(panelTargetColor, 0.4f))
            .Join(text.DOFade(1f, 0.4f))

            .AppendInterval(2f)

            .Append(canvasGroup.DOFade(0f, 0.3f))
            .Join(goodUIPanel.transform.DOScale(0f, 0.3f))
            .Join(bgImage.DOColor(panelStartColor, 0.3f))
            .Join(text.DOFade(0f, 0.3f))

            .OnComplete(() => goodUIPanel.SetActive(false));
        }
    }

    protected virtual void DisableBarrier()
    {
        if (barrierObjectInScene != null)
        {
            // TODO: Dissolve 이펙트 추가 가능
            barrierObjectInScene.SetActive(false);
        }
    }

    public Transform GetPlayerTransform()
    {
        return player;
    }

    protected abstract void OnAreaCompleted();
}
