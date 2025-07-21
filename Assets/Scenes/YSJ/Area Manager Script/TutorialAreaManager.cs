using UnityEngine;

public class TutorialAreaManager : BaseAreaManager
{
    private bool blueLaserHit = false;

    private void Start()
    {
        ActivatePortals(); // 포탈 시각 효과만 실행
    }

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

    private void TryComplete()
    {
        if (blueLaserHit && !isAreaCompleted)
        {
            OnLaserTriggered();
        }
    }

    protected override void OnAreaCompleted()
    {
        Debug.Log("Tutorial completed. Barrier disabled.");
    }
}
