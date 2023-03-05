namespace DeepDreams.Player.StateMachine.Simple
{
    public interface IState
    {
        PlayerState GetStateType();

        void Tick();
        void OnEnter();
        void OnExit();
    }
}