public interface IPlayerState
{
    void Enter(PlayerController player);
    void Update();
    void FixedUpdate();
    void Exit();
}