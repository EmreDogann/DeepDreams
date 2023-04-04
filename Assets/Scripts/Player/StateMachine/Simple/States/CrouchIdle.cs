namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class CrouchIdle : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        private bool _prevCrouchState;

        public CrouchIdle(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.CrouchIdle;
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
            PlayerState previousState = _stateMachine.GetPreviousStateType();

            if (previousState != PlayerState.CrouchWalk) _blackboard.OnPlayerCrouch.Invoke(true);
            _prevCrouchState = _blackboard.IsCrouching;
        }

        public void OnExit()
        {
            // Debug.Log("Crouching Exit");

            PlayerState currentState = _stateMachine.GetCurrentStateType();
            if (currentState != PlayerState.CrouchWalk) _blackboard.OnPlayerCrouch.Invoke(false);

            if (currentState == PlayerState.Walking || currentState == PlayerState.Running) _blackboard.OnStride.Invoke();
        }
    }
}