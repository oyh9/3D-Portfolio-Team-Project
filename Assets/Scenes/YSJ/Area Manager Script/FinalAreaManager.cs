using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;

public class FinalAreaManager : BaseAreaManager
{
    [Header("Final Area Settings")]
    [SerializeField] private RotatableMirror[] mirrors;
    [SerializeField] private GameObject final_laser;
    [Header("Ending Timeline")]
    [SerializeField] private PlayableDirector endingTimeline;
    

    private bool blueLaserHit = false;
    private bool hasShownSuccessUI = false;

    // 이번 프레임에 레이저에 감지된 미러들
    private HashSet<RotatableMirror> mirrorsHitThisFrame = new HashSet<RotatableMirror>();

    private void Start()
    {
        ActivatePortals();
    }

    private void Update()
    {
        if (isAreaCompleted) return;

        // 미러 모두 감지되면 레이저 on
        final_laser.SetActive(AllMirrorsHit());

        if (AllMirrorsHit() && !hasShownSuccessUI)
        {
            ShowSuccessUI();
            hasShownSuccessUI = true;
        }

        // TryComplete();
    }

    private void LateUpdate()
    {
        if (isAreaCompleted) return;

        // 감지 안된 미러는 false 처리
        foreach (var mirror in mirrors)
        {
            if (!mirrorsHitThisFrame.Contains(mirror))
            {
                mirror.SetLaserHit(false);
            }
        }

        mirrorsHitThisFrame.Clear(); // 다음 프레임 준비
    }

    public void RegisterMirrorHit(RotatableMirror mirror)
    {
        if (!mirrorsHitThisFrame.Contains(mirror))
        {
            mirror.SetLaserHit(true);
            mirrorsHitThisFrame.Add(mirror);
        }
    }

    private bool AllMirrorsHit()
    {
        foreach (var mirror in mirrors)
        {
            if (!mirror.IsLaserHit)
                return false;
        }
        return true;
    }

    private void TryComplete()
    {
        if (blueLaserHit && AllMirrorsHit() && !isAreaCompleted)
        {
            isAreaCompleted = true;
            DisableBarrier();
            Invoke(nameof(PlayEndingTimeline), 1f);
            OnAreaCompleted();
            // OnLaserTriggered();
        }
    }

    // 🔵 파란 레이저 감지 시 호출됨
    public override void OnColorLaserTriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        if (colorType == Lightbug.LaserMachine.LaserProperties.LaserColorType.Blue)
        {
            blueLaserHit = true;
            TryComplete();
        }
    }

    public override void OnColorLaserUntriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        if (colorType == Lightbug.LaserMachine.LaserProperties.LaserColorType.Blue)
        {
            blueLaserHit = false;
        }
    }

    private void PlayEndingTimeline()
    {
        if (endingTimeline != null)
        {
            endingTimeline.Play();
        }
    }


    protected override void OnAreaCompleted()
    {
        Debug.Log("Final Area Completed with Blue Laser and All Mirrors!");
    }
}
