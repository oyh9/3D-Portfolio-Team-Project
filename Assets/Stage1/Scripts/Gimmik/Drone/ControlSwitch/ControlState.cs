public abstract class ControlState
{
    public abstract void EnterState(ControlManager context);
    public abstract void UpdateState(ControlManager context);
    public abstract void ExitState(ControlManager context);
}
