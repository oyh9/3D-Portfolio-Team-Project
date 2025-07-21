using UnityEngine;

public class ThirdAreaManager : BaseAreaManager
{
    private bool isRedLaserHit = false;
    private bool isBlueLaserHit = false;

    private void Start()
    {
        ActivatePortals();
    }

    public override void OnColorLaserTriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        Debug.Log($"[ThirdAreaManager] Laser Triggered: {colorType}");
        switch (colorType)
        {
            case Lightbug.LaserMachine.LaserProperties.LaserColorType.Red:
                isRedLaserHit = true;
                break;
            case Lightbug.LaserMachine.LaserProperties.LaserColorType.Blue:
                isBlueLaserHit = true;
                break;
        }

        Debug.Log($"Red hit: {isRedLaserHit}, Blue hit: {isBlueLaserHit}");

        TryComplete();
    }

    public override void OnColorLaserUntriggered(Lightbug.LaserMachine.LaserProperties.LaserColorType colorType)
    {
        switch (colorType)
        {
            case Lightbug.LaserMachine.LaserProperties.LaserColorType.Red:
                isRedLaserHit = false;
                break;
            case Lightbug.LaserMachine.LaserProperties.LaserColorType.Blue:
                isBlueLaserHit = false;
                break;
        }
    }

    private void TryComplete()
    {
        Debug.Log($"TryComplete() check. Red: {isRedLaserHit}, Blue: {isBlueLaserHit}, Completed: {isAreaCompleted}");

    if (isRedLaserHit && isBlueLaserHit && !isAreaCompleted)
    {
        Debug.Log("All conditions met → calling OnLaserTriggered()");
        OnLaserTriggered();
    }
    }

    protected override void OnAreaCompleted()
    {
        Debug.Log("Area3 completed. Barrier disabled.");
    }
}
