namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Running : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        public Running(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Running;
        }

        public void Tick() {}

        public void OnEnter()
        {
            // Debug.Log("Running Enter");
            _blackboard.OnPlayerRun.Invoke();
        }

        public void OnExit() {}
    }
}