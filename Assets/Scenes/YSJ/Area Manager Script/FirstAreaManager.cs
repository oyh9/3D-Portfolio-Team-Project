using UnityEngine;

public class FirstAreaManager : BaseAreaManager
{
    private bool blueLaserHit = false;

    private void Start()
    {
        ActivatePortals();
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
        Debug.Log("Area1 completed. Barrier disabled.");
    }
}
