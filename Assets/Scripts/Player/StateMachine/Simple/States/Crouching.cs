namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class Crouching : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        private bool _prevCrouchState;

        public Crouching(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.Crouching;
        }

        public void Tick()
        {
            if (_prevCrouchState != _blackboard.IsCrouching)
            {
                _blackboard.OnPlayerCrouch.Invoke(_blackboard.IsCrouching);
                _prevCrouchState = _blackboard.IsCrouching;
            }
        }

        public void OnEnter()
        {
            // Debug.Log("Crouching Enter");
            _blackboard.OnPlayerCrouch.Invoke(true);
        }

        public void OnExit()
        {
            // Debug.Log("Crouching Exit");
            _blackboard.OnPlayerCrouch.Invoke(false);
        }
    }
}