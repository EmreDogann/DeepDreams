namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Walking : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        public Walking(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Walking;
        }

        public void Tick() {}

        public void OnEnter()
        {
            // Debug.Log("Walking Enter");
            _blackboard.OnPlayerWalk.Invoke();
        }

        public void OnExit() {}
    }
}