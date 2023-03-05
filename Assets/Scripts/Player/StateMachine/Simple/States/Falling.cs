namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Falling : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        public Falling(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Falling;
        }

        public void Tick()
        {
            // Debug.Log("Falling Update");
        }

        public void OnEnter()
        {
            // Debug.Log("Falling Enter");
        }

        public void OnExit()
        {
            // Debug.Log("Falling Exit");
        }
    }
}