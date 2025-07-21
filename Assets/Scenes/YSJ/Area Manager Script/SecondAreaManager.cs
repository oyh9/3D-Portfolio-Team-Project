using UnityEngine;

public class SecondAreaManager : BaseAreaManager
{
    private bool redLaserHit = false;

    private void Start()
    {
        ActivatePortals();
    }

    public override void OnColorLaserTriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        if (colorType == Lightbug.LaserMachine.LaserProperties.LaserColorType.Red)
        {
            redLaserHit = true;
            TryComplete();
        }
    }

    public override void OnColorLaserUntriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        if (colorType == Lightbug.LaserMachine.LaserProperties.LaserColorType.Red)
        {
            redLaserHit = false;
        }
    }

    private void TryComplete()
    {
        if (redLaserHit && !isAreaCompleted)
        {
            OnLaserTriggered();
        }
    }

    protected override void OnAreaCompleted()
    {
        Debug.Log("Area2 completed. Barrier disabled.");
    }
}
