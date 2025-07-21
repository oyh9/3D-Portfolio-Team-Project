using UnityEngine;

public class LaserReceiver : MonoBehaviour, ILaserTriggerable
{
    [SerializeField] private BaseAreaManager roomManager;
    [SerializeField] private Lightbug.LaserMachine.LaserProperties.LaserColorType acceptedColor; // 💡 추가
    private bool isCurrentlyHit = false;

    public void SetHit(bool isHit)
    {
        // 기본 SetHit은 의미 없게 만듦
        Debug.LogWarning("SetHit(bool) is deprecated. Use SetHit(bool, LaserColorType) instead.");
    }

    // 오버로드 추가
    public void SetHit(bool isHit, Lightbug.LaserMachine.LaserProperties.LaserColorType incomingColor)
    {
        if (incomingColor != acceptedColor)
        {
            return; // 💡 색이 다르면 무시
        }

        if (isCurrentlyHit == isHit)
            return;

        isCurrentlyHit = isHit;

        if (isHit)
        {
            Debug.Log($"{acceptedColor} Laser ON");
            roomManager?.OnColorLaserTriggered(acceptedColor); // 👈 색깔 기반 트리거
        }
        else
        {
            Debug.Log($"{acceptedColor} Laser OFF");
            roomManager?.OnColorLaserUntriggered(acceptedColor); // 👈 OFF도 처리 필요
        }
    }
}