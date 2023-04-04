namespace DeepDreams.Player.StateMachine.Simple.States
{
    public class CrouchWalking : IState
    {
        private readonly PlayerStateMachine _stateMachine;
        private readonly PlayerBlackboard _blackboard;

        private bool _prevCrouchState;

        public CrouchWalking(PlayerStateMachine stateMachine, PlayerBlackboard blackboard)
        {
            _stateMachine = stateMachine;
            _blackboard = blackboard;
        }

        public PlayerState GetStateType()
        {
            return PlayerState.CrouchWalk;
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

            if (previousState != PlayerState.CrouchIdle) _blackboard.OnPlayerCrouch.Invoke(true);
            _prevCrouchState = _blackboard.IsCrouching;
        }

        public void OnExit()
        {
            // Debug.Log("Crouching Exit");
            PlayerState currentState = _stateMachine.GetCurrentStateType();
            if (currentState != PlayerState.CrouchIdle) _blackboard.OnPlayerCrouch.Invoke(false);

            if (currentState == PlayerState.CrouchIdle && _blackboard.StrideDistance > _blackboard.PlayerStride * 0.4f)
                _blackboard.OnStride.Invoke();
        }
    }
}