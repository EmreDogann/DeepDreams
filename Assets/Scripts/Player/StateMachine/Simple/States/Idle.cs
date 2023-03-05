namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Idle : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        public Idle(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Idle;
        }

        public void Tick()
        {
            // Debug.Log("Idle Update");
        }

        public void OnEnter()
        {
            // Debug.Log("Idle Enter");
            _blackboard.MoveSpeed = 0.0f;
        }

        public void OnExit()
        {
            // Debug.Log("Idle Exit");
        }
    }
}